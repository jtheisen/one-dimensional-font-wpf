using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.ColorSpaces.Conversion;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Numerics;
using System.Reactive.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OneDimFont
{
    [ContentProperty("Content")]
    public class OneDim2 : ContentControl
    {
        static OneDim2()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(OneDim2), new FrameworkPropertyMetadata(typeof(OneDim2)));
        }

        public OneDim2()
        {
            //LayoutUpdated += (o, e) => Update();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            Observable.Interval(TimeSpan.FromSeconds(1)).ObserveOnDispatcher().Subscribe(i => Update());
        }

        void Update()
        {
            var reference = (FrameworkElement)Template.FindName("Content", this);
            var image = (System.Windows.Controls.Image)Template.FindName("Image", this);

            var w = Math.Max((int)reference.ActualWidth, 1);
            var h = Math.Max((int)reference.ActualHeight, 1);

            var rtb = new RenderTargetBitmap(w, h, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(reference);

            image.Source = TransformBitmap(rtb);
        }

        BitmapSource TransformBitmap(RenderTargetBitmap source)
        {
                //.Mutate(x => x.get)
                //.Clone(x => x.Rotate(RotateMode.Rotate90));

            //sourceImage.Clone(x => x.Rotate(RotateMode.Rotate90));

            var buffer = new UInt32[source.PixelWidth * source.PixelHeight];

            source.CopyPixels(buffer, source.PixelWidth * 4, 0);

            var target = new UInt32[source.PixelWidth];

            for (var i = 0; i < source.PixelWidth; ++i)
            {
                var x = new Vector3();

                var hueFactor = 1.0f / source.PixelHeight * 360;

                for (var j = 0; j < source.PixelHeight; ++j)
                {
                    //if (j != 50 && j != 40) continue;

                    //var r = (buffer[j * source.PixelWidth + i] & 255) / 255.0;

                    uint packed = buffer[j * source.PixelWidth + i];
                    var pp = new Rgba32(packed);

                    if (pp.A != 255) continue;

                    Rgb p = pp;

                    //if (packed != 0u) System.Diagnostics.Debugger.Break();
                    

                    var converter = new ColorSpaceConverter();

                    var hsl = new Hsl(j * hueFactor, 1, .5f);

                    //if (j == source.PixelHeight >> 1) System.Diagnostics.Debugger.Break();

                    var rgb = converter.ToRgb(in hsl);

                    x = Vector3.Max(x, Vector3.Multiply(1 - p.R, rgb.ToVector3()));

                }

                target[i] = new Rgba32(x).PackedValue;
            }

            //return RenderTargetBitmap.Create(source.PixelWidth, source.PixelHeight, 96, 96, source.Format, source.Palette, buffer, source.PixelWidth * 4);
            return RenderTargetBitmap.Create(source.PixelWidth, 1, 96, 96, source.Format, source.Palette, target, source.PixelWidth * 4);
        }

        // Doesn't work
        Image<Rgba32> GetImageForRenderTargetBitmap(RenderTargetBitmap source)
        {
            var buffer = new Byte[source.PixelWidth * source.PixelHeight * 4];

            source.CopyPixels(buffer, source.PixelWidth * 4, 0);

            return Image<Rgba32>.Load(buffer);
        }

        //BitmapSource CreateBitmapSource(Image<Rgba32> source)
        //{
        //    source.Save()

        //    return RenderTargetBitmap.Create(source.Width, source.Height, 96, 96, System.Windows.Media.PixelFormats.Bgra32, null, target, source.PixelWidth * 4);
        //}
    }
}

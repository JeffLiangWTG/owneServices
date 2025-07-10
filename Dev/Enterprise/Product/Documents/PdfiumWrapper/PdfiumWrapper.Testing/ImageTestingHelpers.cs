using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.PdfiumWrapper.Testing
{
	public static class ImageTestingHelpers
	{
		public static TempFile CreateImageFile(ImageFormat format, params Image[] pages)
		{
			if (pages.Length > 1 && format != ImageFormat.Tiff)
			{
				throw new ArgumentException("Only tiff can be multipage");
			}
			else if (pages.Length == 0)
			{
				throw new ArgumentException("Need to specify at least one page");
			}

			var images = new Queue<Image>(pages);
			var firstImage = images.Dequeue();
			var tempFile = TempFile.New();
			if (images.Count == 0)
			{
				firstImage.Save(tempFile.Filename, format);
				return tempFile;
			}

			var encoder = Encoder.SaveFlag;
			var encoderInfo = ImageCodecInfo.GetImageEncoders().First(i => i.MimeType == "image/tiff");
			using (var encoderParameters = new EncoderParameters(1))
			{
				encoderParameters.Param[0] = new EncoderParameter(encoder, (long)EncoderValue.MultiFrame);
				firstImage.Save(tempFile.Filename, encoderInfo, encoderParameters);

				encoderParameters.Param[0] = new EncoderParameter(encoder, (long)EncoderValue.FrameDimensionPage);
				while (images.Count > 0)
				{
					firstImage.SaveAdd(images.Dequeue(), encoderParameters);
				}

				encoderParameters.Param[0] = new EncoderParameter(encoder, (long)EncoderValue.Flush);
				firstImage.SaveAdd(encoderParameters);
			}

			return tempFile;
		}

		public static Color[] GetPageColors(Image img)
		{
			var nbPages = img.GetFrameCount(FrameDimension.Page);
			var colors = new Color[nbPages];

			for (var i = 0; i < nbPages; i++)
			{
				img.SelectActiveFrame(FrameDimension.Page, i);
				colors[i] = AsNamedColor(GetCenterPixelColor(img));
			}

			return colors;
		}

		public static TempFile CreateImageFile(ImageFormat format, params Color[] colorsOfPages)
		{
			return CreateImageFile(format, colorsOfPages.Select(c => ImageOfColor(c, 1, 1)).ToArray());
		}

		public static Bitmap ImageOfColor(Color c, int w, int h)
		{
			Bitmap bmp = new Bitmap(w, h);

			try
			{
				using (var g = Graphics.FromImage(bmp))
				using (var brush = new SolidBrush(c))
				{
					g.FillRectangle(brush, 0, 0, w, h);
				}
			}
			catch
			{
				bmp.Dispose();
				throw;
			}

			return bmp;
		}

		public static Color GetCenterPixelColor(Image img)
		{
			return ((Bitmap)img).GetPixel(img.Width / 2, img.Height / 2);
		}

		public static Color AsNamedColor(Color unnamed)
		{
			if (unnamed.IsNamedColor)
			{
				return unnamed;
			}

			return typeof(Color)
				.GetProperties(BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.Public)
				.Select(prop => prop.GetValue(null))
				.OfType<Color>()
				.FirstOrDefault(c => c.ToArgb() == unnamed.ToArgb());
		}
	}
}

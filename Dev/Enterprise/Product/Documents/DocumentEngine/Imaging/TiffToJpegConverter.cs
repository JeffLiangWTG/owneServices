using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.DocumentEngine.Imaging
{
	public static class TiffToJpegConverter
	{
		static EncoderParameters Parameters
		{
			get
			{
				var parameters = new EncoderParameters(1);
				parameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)100);
				return parameters;
			}
		}

		[ThreadSafe]
		static IEnumerable<PixelFormat> _formatsNotToConvert;
		static IEnumerable<PixelFormat> FormatsNotToConvert { get { return _formatsNotToConvert ?? (_formatsNotToConvert = new[] { PixelFormat.Format1bppIndexed, PixelFormat.Format4bppIndexed, PixelFormat.Format8bppIndexed }); } }

		static bool IsMultiPage(Image image)
		{
			return image.FrameDimensionsList.Contains(FrameDimension.Page.Guid) && image.GetFrameCount(FrameDimension.Page) > 1;
		}

		public static byte[] TryShrinkImageByConvertingToJpeg(Image image, EncoderParameters parameters = null)
		{
			return !IsMultiPage(image) && !FormatsNotToConvert.Contains(image.PixelFormat) ? ConvertTiffToJpeg(image, parameters ?? Parameters) : null;
		}

		static byte[] ConvertTiffToJpeg(Image image, EncoderParameters encoderParameters)
		{
			try
			{
				using (var ms = new MemoryStream())
				{
					image.Save(ms, JpegCodec.Codec, encoderParameters);
					return ms.GetBuffer();
				}
			}
			catch (ExternalException)
			{
				//Source stream is closed or some serialisation problem is stopping us from converting the file. That is ok
				return null;
			}
			catch (ArgumentException ex)
			{
				if (image.RawFormat.Guid == ImageFormat.Tiff.Guid && !(ex is ArgumentNullException))
				{
					Globals.Message.Show(Res.GetString("890e0f95-74fc-4a31-83d5-a386daa5a568", "This TIFF compression type is not supported by .NET. Supported types are: CCITT3, CCITT4, LZW, RLE or None."));
					return null;
				}
				else
				{
					var sb = new StringBuilder();
					sb.AppendLine("PixelFormat: " + image.PixelFormat);
					sb.AppendLine("RawFormat: " + image.RawFormat);
					sb.AppendLine((NoResString)"Width: " + image.Width);
					sb.AppendLine((NoResString)"Height: " + image.Height);
					sb.AppendLine("HorizontalResolution: " + image.HorizontalResolution);
					sb.AppendLine("VerticalResolution: " + image.VerticalResolution);

					throw new ArgumentException(sb.ToString(), ex);
				}
			}
		}

		public static class JpegCodec
		{
			[ThreadStatic]
			static ImageCodecInfo codec;
			public static ImageCodecInfo Codec => codec ?? (codec = ImageCodecInfo.GetImageEncoders().First(e => e.MimeType == "image/jpeg"));
		}
	}
}

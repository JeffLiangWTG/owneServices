using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using CargoWise.Common;
using CargoWise.Types;
using FlexCel.Core;
using FlexCel.Pdf;

namespace Enterprise.DocumentEngine.FlexCelInterface
{
	public class ImageToPDFConverter
	{
		public ImageToPDFConverter()
		{
			writer = new PdfWriter();
			writer.PageSize = new TPaperDimensions(TPaperSize.A4);
			writer.Compress = true;
			writer.YAxisGrowsDown = true; // Makes it write from the top of the page down, instead of the bottom of the page up when an image is too big to fit on the page.
		}
		readonly PdfWriter writer;

		public byte[] ConvertToPDF(byte[] imageData)
		{
			using (MemoryStream imageStream = new MemoryStream(imageData))
			{
				try
				{
					using (Image imageInput = Image.FromStream(imageStream))
					using (MemoryStream pdfOutputStream = new MemoryStream())
					{
						int pages = GetPagesAsPDF(imageInput, pdfOutputStream);
						return pages > 0 ? pdfOutputStream.CopyToByteArray() : null;
					}
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					return null;
				}
			}
		}

		internal int GetPagesAsPDF(Image imageInput, Stream pdfOutput)
		{
			int pageCount = imageInput.GetFrameCount(FrameDimension.Page);
			if (pageCount > 0)
			{
				for (int index = 0; index < pageCount; index++)
				{
					if (index == 0)
					{
						writer.BeginDoc(pdfOutput);
					}
					else
					{
						writer.NewPage();
					}

					imageInput.SelectActiveFrame(FrameDimension.Page, index);
					RectangleF bounds = GetOutputSize(imageInput.Size);
					writer.DrawImage(imageInput, bounds, null);
				}

				writer.EndDoc();
			}

			return pageCount;
		}

		RectangleF GetOutputSize(Size sourceSize)
		{
			float maxXScale = (float)writer.PageSize.Width / sourceSize.Width;
			float maxYScale = (float)writer.PageSize.Height / sourceSize.Height;

			float scale = Math.Min(maxXScale, maxYScale) * 72 / 100; // Page sizes are in 1/100th's of an inch, images are drawn in points or 1/72s of an inch.

			return new RectangleF(0, 0, (scale * sourceSize.Width), (scale * sourceSize.Height));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard coded value")]
		public static bool IsPDF(byte[] contents)
		{
			var result = false;

			ZString stringContents = Encoding.ASCII.GetString(contents);

			var streamHeader = stringContents.SubstringSafe(0, 4);
			if (streamHeader.Equals("%PDF"))
			{
				ZString streamEnd = stringContents.TrimEnd();
				result = streamEnd.SubstringSafe(streamEnd.Length - 4, 4).Equals("%EOF");
			}

			return result;
		}

		public static bool IsPDFA(byte[] contents)
		{
			var result = IsPDF(contents);

			var stringContents = Encoding.ASCII.GetString(contents);
			var conformanceStringIndex = stringContents.IndexOf(PDFAConformanceString, StringComparison.OrdinalIgnoreCase);

			result &= conformanceStringIndex > -1;

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "It's an XML field...")]
		const string PDFAConformanceString = "<pdfaid:conformance>A</pdfaid:conformance>";

		public static bool IsTiff(byte[] content)
		{
			return ImageUtils.GetImageType(content) == TXlsImgType.Tiff;
		}

		public static List<string> GetAllowedFileExtensions()
		{
			return new List<string>(new string[]
			{
				".PDF",
				".BMP",
				".GIF",
				".EXIF",
				".EXF",
				".JPG",
				".JPEG",
				".PNG",
				".TIF",
				".TIFF",
			});
		}
	}
}

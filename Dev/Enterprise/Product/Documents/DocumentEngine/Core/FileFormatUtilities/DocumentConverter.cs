using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.Common;
using CargoWise.PdfiumWrapper;
using CargoWise.Types;
using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using FlexCel.Core;
using FlexCel.Draw;
using FlexCel.Pdf;
using FlexCel.XlsAdapter;
using HtmlAgilityPack;
using static Enterprise.DocumentEngineCore.Registry.DocumentSigningRegistryConstants;
using Encoder = System.Drawing.Imaging.Encoder;
using Watermark = Enterprise.RemotePrinting.Engine.Watermark;

namespace Enterprise.DocumentEngine.FileFormatUtilities
{
	/// <summary>
	/// Converts documents between different formats.
	/// </summary>
	public static class DocumentConverter
	{
		public static bool ShouldConvertFromExcel(OutputFormatType formatType, DeliveryInfo info)
		{
			return info.ShouldConvertFromExcel(formatType);
		}
		public static byte[] ConvertFromExcel(byte[] xlsData, OutputFormatType reFormatType, ColourDepth colourDepth)
		{
			return ConvertFromExcel(xlsData, reFormatType, colourDepth, false);
		}

		public static byte[] ConvertFromExcel(byte[] xlsData, OutputFormatType reFormatType, ColourDepth colourDepth, bool localCulture)
		{
			return ConvertFromExcelCore(string.Empty, xlsData, reFormatType, null, GetDesiredPixelFormat(colourDepth), localCulture, 1);
		}

		public static void ConvertFromExcel(byte[] xlsData, string outputFileFullPath, OutputFormatType reFormatType, Watermark watermark, ColourDepth colourDepth)
		{
			ConvertFromExcel(xlsData, outputFileFullPath, reFormatType, watermark, colourDepth, false, 1);
		}

		public static byte[] ConvertFromExcel(byte[] xlsData, string outputFileFullPath, OutputFormatType reFormatType, Watermark watermark, bool localCulture, decimal lineSpacing)
		{
			return ConvertFromExcelCore(outputFileFullPath, xlsData, reFormatType, watermark, GetDesiredPixelFormat(ColourDepth.TrueColour), localCulture, lineSpacing);
		}

		public static byte[] ConvertFromExcel(byte[] xlsData, string outputFileFullPath, OutputFormatType reFormatType, Watermark watermark, ColourDepth colourDepth, bool localCulture, decimal lineSpacing, bool signDocument = false)
		{
			return ConvertFromExcelCore(outputFileFullPath, xlsData, reFormatType, watermark, GetDesiredPixelFormat(colourDepth), localCulture, lineSpacing, signDocument);
		}

		public static void WriteOutputFromExcel(Stream xlsDataStream, Stream outputStream, OutputFormatType reFormatType, Watermark watermark = null, ColourDepth colourDepth = ColourDepth.TrueColour, bool localCulture = false, decimal lineSpacing = 1, bool signDocument = false)
		{
			WriteOutputFromExcelCore(xlsDataStream, outputStream, reFormatType, watermark, GetDesiredPixelFormat(colourDepth), localCulture, lineSpacing, signDocument);
		}

		public static ExcelInterface.DocDataMultiplex ConvertFromExcel_IncludingHTML(byte[] xlsData, OutputFormatType reFormatType, ColourDepth colourDepth, int docNumber)
		{
			return ConvertFromExcel_IncludingHTML(xlsData, reFormatType, colourDepth, false, docNumber);
		}

		public static ExcelInterface.DocDataMultiplex ConvertFromExcel_IncludingHTML(byte[] xlsData, OutputFormatType reFormatType, ColourDepth colourDepth, bool localCulture, int docNumber)
		{
			return ConvertFromExcel_IncludingHTML_Core(string.Empty, xlsData, reFormatType, null, GetDesiredPixelFormat(colourDepth), localCulture, 1, docNumber);
		}

		public static ExcelInterface.DocDataMultiplex ConvertFromExcel_IncludingHTML(byte[] xlsData, string outputFileFullPath, OutputFormatType reFormatType, Watermark watermark, bool localCulture, decimal lineSpacing, int docNumber)
		{
			return ConvertFromExcel_IncludingHTML_Core(outputFileFullPath, xlsData, reFormatType, watermark, GetDesiredPixelFormat(ColourDepth.TrueColour), localCulture, lineSpacing, docNumber);
		}

		public static ExcelInterface.DocDataMultiplex ConvertFromExcel_IncludingHTML(byte[] xlsData, string outputFileFullPath, OutputFormatType reFormatType, Watermark watermark, ColourDepth colourDepth, bool localCulture, decimal lineSpacing, int docNumber, bool signDocument = false, string signingOption = DocumentsSignBy.PFX, string signerName = "", ZGuid? branch = null, bool shouldResetExcelModifyPWD = false)
		{
			return ConvertFromExcel_IncludingHTML_Core(outputFileFullPath, xlsData, reFormatType, watermark, GetDesiredPixelFormat(colourDepth), localCulture, lineSpacing, docNumber, signDocument, signingOption, signerName, branch, shouldResetExcelModifyPWD);
		}

		[SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
		static ExcelInterface.DocDataMultiplex ConvertFromExcel_IncludingHTML_Core(string outputFileFullPath, byte[] xlsData, OutputFormatType reFormatType, Watermark watermark,
			PixelFormat desiredPixelFormat, bool localCulture, decimal lineSpacing, int docNumber, bool signDocument = false, string signingOption = DocumentsSignBy.PFX, string signerName = "", ZGuid? branch = null, bool shouldResetExcelModifyPWD = false)
		{
			ExcelInterface.DocDataMultiplex result = null;

			if (reFormatType == OutputFormatType.HTMF && docNumber != 0)
			{
				reFormatType = OutputFormatType.PDF;
			}

			if (!string.IsNullOrEmpty(outputFileFullPath))
			{
				FileSaveHelper.DeleteFile(outputFileFullPath);
			}

			using (var xLInterface = new ExcelInterface())
			{
				xLInterface.Xls.Linespacing = Convert.ToDouble(lineSpacing);

				using (var ms = new MemoryStream(xlsData))
				{
					xLInterface.LoadExcelFile(ms);
				}

				if (shouldResetExcelModifyPWD && xLInterface.Xls.Protection.HasSheetPassword)
				{
					xLInterface.ProtectSheets(Env.Registry.ExcelPasswordForModifying);
				}

				if (localCulture)
				{
					Culture.Set(Culture.CurrentCompanyCountryCulture);
				}

				try
				{
					using (var outputStream = (string.IsNullOrEmpty(outputFileFullPath) || reFormatType == OutputFormatType.HTML || reFormatType == OutputFormatType.HTMF) ? (Stream)new MemoryStream() : new FileStream(outputFileFullPath, FileMode.Create))
					{
						switch (reFormatType)
						{
							case OutputFormatType.PDF:
							case OutputFormatType.PDFC:
								xLInterface.ExportToPdfAndScale(outputStream, 100, watermark, FlexCelPdfExportSafe.DefaultPdfVersion, TPdfType.Standard, signDocument, signingOption: signingOption, signerName: signerName, branch: branch);
								result = new ExcelInterface.DocDataMultiplex(outputStream.CopyToByteArray(), false);
								break;
							case OutputFormatType.PDFAcrobat5:
								xLInterface.ExportToPdfAndScale(outputStream, 100, watermark, TPdfVersion.v14, TPdfType.Standard, signDocument, signingOption: signingOption, signerName: signerName, branch: branch);
								result = new ExcelInterface.DocDataMultiplex(outputStream.CopyToByteArray(), false);
								break;

							case OutputFormatType.PDFA:
								xLInterface.ExportToPdfAndScale(outputStream, 100, watermark, TPdfType.PDFA2, signDocument, signingOption: signingOption, signerName: signerName, branch: branch);
								result = new ExcelInterface.DocDataMultiplex(outputStream.CopyToByteArray(), false);
								break;

							case OutputFormatType.TIF:
								xLInterface.ExportToMultiPageTiffAndScale(outputStream, false, 100, false, Env.Registry.PDFTIFResolution, desiredPixelFormat, watermark);
								result = new ExcelInterface.DocDataMultiplex(outputStream.CopyToByteArray(), false);
								break;

							case OutputFormatType.FAX:
								xLInterface.ExportToMultiPageTiffAndScale(outputStream, false, 100, true, 196, PixelFormat.Format1bppIndexed, watermark);
								result = new ExcelInterface.DocDataMultiplex(outputStream.CopyToByteArray(), false);
								break;

							case OutputFormatType.HTML:
								result = xLInterface.ExportToHTMLAndScale(outputFileFullPath, 100);
								break;

							case OutputFormatType.HTMF:
								result = xLInterface.ExportFirstSheetToHTMLAndRestToPDF(outputFileFullPath, 100, watermark);
								break;

							case OutputFormatType.XLS:
								xLInterface.SaveToStream(outputStream, "XLS");
								result = new ExcelInterface.DocDataMultiplex(outputStream.CopyToByteArray(), false, xLInterface.Xls.Protection.HasSheetPassword);
								break;

							case OutputFormatType.XLSX:
								xLInterface.SaveToStream(outputStream, "XLSX");
								result = new ExcelInterface.DocDataMultiplex(outputStream.CopyToByteArray(), false, xLInterface.Xls.Protection.HasSheetPassword);
								break;

							default:
								throw new DocumentConvertException("File format not supported: " + reFormatType);
						}
					}
				}
				finally
				{
					if (localCulture)
					{
						Culture.Set(Culture.Default);
					}

					if ((reFormatType == OutputFormatType.PDF || reFormatType == OutputFormatType.PDFC) && signDocument)
					{
						ReplaceSignatureFormatForETSICompliance(outputFileFullPath, branch);
					}
				}

				return result;
			}
		}

		public const string HTMLFilesDirectory = ExcelInterface.HTMLFilesDirectory;

		public static string GetSafeImagesDirectoryName(string fileName)
		{
			var imagesDirectoryName = Path.GetFileNameWithoutExtension(fileName);
			var sh1Bytes = SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(imagesDirectoryName));
			return WebUtility.UrlEncode(Convert.ToBase64String(sh1Bytes)).Replace("%", "_") + HTMLFilesDirectory;
		}

		public static byte[] PostProcessHTMLForEmail(byte[] htmlData)
		{
			string htmlString = Encoding.UTF8.GetString(htmlData);

			return Encoding.Unicode.GetBytes(PostProcessHTMLForEmail(htmlString));
		}

		public static string PostProcessHTMLForEmail(string htmlString)
		{
			//Yes, I know that you're not meant to use regexes to parse HTML. But HTML output by FlexCel is always going to have a format like this, so I can get away with it.
			//htmlString = new Regex("<img src='([^'/])+/").Replace(htmlString, "<img src='");
			//We are doing two things here: Correctly ending img and col tags (by replacing > with />) and removing the directory part of the img src path, since the file name will be directly available as an attachment.
			htmlString = new Regex("<img src=['\"]([^'\"/]+)/([^'\"]+)['\"]([^/>]*)>").Replace(htmlString, "<img src='cid:$2'$3/>");
			htmlString = new Regex("<v:imagedata\\s+src=['\"]([^'\"/]+)/([^'\"]+)['\"]").Replace(htmlString, "<v:imagedata src=\"cid:$2\"");
			htmlString = new Regex("<br>").Replace(htmlString, "<br />");
			htmlString = new Regex("<col([^/>]*)>").Replace(htmlString, "<col$1/>");
			htmlString = new Regex("\"style").Replace(htmlString, "\" style");

			return htmlString;
		}

		const string identifierForEDILinkPrefix = "<a href=\"file://edient:";
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "The string literal is safe to use in this context and does not need to be externalized.")]
		const string flexCelHelperContainer = "Helper container";

		static void HideHtmlNode(HtmlNode node)
		{
			var styleAttribute = node.Attributes["style"];

			if (styleAttribute != null)
			{
				node.Attributes["style"].Value = styleAttribute.Value +
												 (styleAttribute.Value.EndsWith(";")
													 ? (NoResString)"height: 0 !important;"
													 : (NoResString)";height: 0 !important;");
			}
			else
			{
				node.SetAttributeValue((NoResString)"style", (NoResString)"height: 0 !important;");
			}
		}

		/*
		 * If we merge cells in Excel and insert images into them, FlexCell will generate an empty table. The empty table and the image tag will be located in the same <td> element.
		 * The image tag has the Position: absolute attribute, which means the image will float above the empty table. However, web email clients do not support this positioning attribute. This will cause the empty table and the image to become separated.
		 * Therefore, we need to hide these empty tables to fit the display requirements of web email clients.
		 * You can refer to this link (https://www.campaignmonitor.com/css/positioning-display/position/) to see the difference in CSS support between email desktop clients and web mail clients.
		 */
		public static string HideEmptyTableForHtml(string htmlString)
		{
			if (IsHTMLFlexCelConversion(htmlString))
			{
				var doc = new HtmlDocument();
				doc.LoadHtml(htmlString);
				var tables = doc.DocumentNode
					?.SelectNodes($"//table[@summary='{flexCelHelperContainer}']")
					?.Where(t => string.IsNullOrWhiteSpace(t.InnerText));

				if (tables != null)
				{
					foreach (var table in tables)
					{
						table.DescendantsAndSelf().Where(n => n.NodeType == HtmlNodeType.Element)
							.ForEach(HideHtmlNode);
					}
				}

				return doc.DocumentNode?.OuterHtml?.Replace((NoResString)"<style type='text/css'>", (NoResString)"<style type = 'text/css'>");
			}

			return htmlString;
		}

		public static void FormatHtmlContent(string file)
		{
			if (File.Exists(file))
			{
				var htmlString = File.ReadAllText(file);

				if (htmlString.Contains(identifierForEDILinkPrefix))
				{
					htmlString = RemoveUnnecessaryEDILinkPrefixForHTML(htmlString);
				}

				htmlString = HideEmptyTableForHtml(htmlString);

				File.WriteAllText(file, htmlString);
			}
		}

		public static string RemoveUnnecessaryEDILinkPrefixForHTML(string htmlString)
		{
			return htmlString.Replace(identifierForEDILinkPrefix, "<a href=\"edient:");
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "programmatic constant")]
		internal const string startOfCss = @"<style type = 'text/css'>
<!--
.flxmain_table {table-layout:fixed; border-collapse:collapse;border-spacing: 0}
table.flxmain_table td {overflow:hidden;padding: 0 1.5pt}
.flxmain_bordered_table {table-layout:fixed; border-collapse:collapse;border-spacing: 0;border:1px solid silver}
.flxmain_bordered_table td {overflow:hidden;padding: 0 1.5pt;border:1px solid silver}
 .imagediv {position:absolute;border:none}
 table td.imagecell {vertical-align:top;text-align:left;padding:0}
table td.flxHeading {background-color:#E7E7E7;text-align:center;border: 1px solid black;font-family:helvetica,arial,sans-serif;font-size:10pt}
";

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "programmatic constant")]
		internal const string endOfCss = @"-->
</style>
</head>
<body>
";

		internal const string startOfHead = @"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.01//EN"" ""http://www.w3.org/TR/html4/strict.dtd"">
<!--[if gte vml 1]><html xmlns:v=""urn:schemas-microsoft-com:vml""><![endif]-->
<html>
<head>
<meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8""/>
<meta http-equiv=""Content-Style-Type"" content=""text/css""/>
<meta name=""Generator"" content=""FlexCel""/>
<title>

</title>
";

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "programmatic constant")]
		internal const string endOfBody = @"</body>
</html>";

		//Try to accept only documents that won't throw exception in CombineHTMLDocumentsForEmail.
		public static bool IsHTMLFlexCelConversion(string htmlString)
		{
			return htmlString.Contains(startOfCss) && htmlString.Contains(endOfCss)
				&& htmlString.Contains((NoResString)"<body>") && htmlString.Contains((NoResString)"</body>");
		}

		static byte[] ConvertFromExcelCore(string outputFileFullPath, byte[] xlsData, OutputFormatType reFormatType, Watermark watermark, PixelFormat desiredPixelFormat, bool localCulture, decimal lineSpacing, bool signDocument = false)
		{
			if (!string.IsNullOrEmpty(outputFileFullPath))
			{
				FileSaveHelper.DeleteFile(outputFileFullPath);
			}

			using (var outputStream = string.IsNullOrEmpty(outputFileFullPath) ? (Stream)new MemoryStream() : new FileStream(outputFileFullPath, FileMode.Create))
			{
				WriteOutputFromExcelCore(new MemoryStream(xlsData), outputStream, reFormatType, watermark, desiredPixelFormat, localCulture, lineSpacing, signDocument);
				return outputStream is MemoryStream ms ? ms.ToArray() : null;
			}
		}

		static void WriteOutputFromExcelCore(Stream xlsDataStream, Stream outputStream, OutputFormatType reFormatType, Watermark watermark, PixelFormat desiredPixelFormat, bool localCulture, decimal lineSpacing, bool signDocument = false)
		{
			using (ExcelInterface xLInterface = new ExcelInterface())
			{
				xLInterface.Xls.Linespacing = Convert.ToDouble(lineSpacing);
				xLInterface.LoadExcelFile(xlsDataStream);

				if (localCulture)
				{
					Culture.Set(Culture.CurrentCompanyCountryCulture);
				}

				try
				{
					switch (reFormatType)
					{
						case OutputFormatType.PDF:
							xLInterface.ExportToPdfAndScale(outputStream, 100, watermark, TPdfType.Standard, signDocument);
							break;

						case OutputFormatType.PDFAcrobat5:
							xLInterface.ExportToPdfAndScale(outputStream, 100, watermark, TPdfVersion.v14, TPdfType.Standard, signDocument);
							break;

						case OutputFormatType.PDFA:
							xLInterface.ExportToPdfAndScale(outputStream, 100, watermark, TPdfType.PDFA2, signDocument);
							break;

						case OutputFormatType.TIF:
							xLInterface.ExportToMultiPageTiffAndScale(outputStream, false, 100, false, Env.Registry.PDFTIFResolution, desiredPixelFormat, watermark);
							break;

						case OutputFormatType.FAX:
							xLInterface.ExportToMultiPageTiffAndScale(outputStream, false, 100, true, 196, PixelFormat.Format1bppIndexed, watermark);
							break;

						case OutputFormatType.XLS:
							xLInterface.SaveToStream(outputStream, "XLS");
							break;

						case OutputFormatType.XLSX:
							xLInterface.SaveToStream(outputStream, "XLSX");
							break;

						case OutputFormatType.HTML:
						case OutputFormatType.HTMF:
							throw new DocumentConvertException("HTML file format requires additional data to be returned. Please call function 'ConvertFromExcel_IncludingHTML' instead.");

						default:
							throw new DocumentConvertException("File format not supported: " + reFormatType);
					}
				}
				finally
				{
					if (localCulture)
					{
						Culture.Set(Culture.Default);
					}
				}
			}
		}

		static PixelFormat GetDesiredPixelFormat(ColourDepth colourDepth)
		{
			switch (colourDepth)
			{
				case ColourDepth.Colour256:
					return PixelFormat.Format8bppIndexed;

				case ColourDepth.BlackAndWhite:
					return PixelFormat.Format1bppIndexed;

				default:
					return PixelFormat.Format24bppRgb;
			}
		}

		public static void ConvertTIFToPDF(string sourceImageFullPath, string outputFileFullPath)
		{
			using (TempFileWithDelayedDelete convertedTifFile = TempFileWithDelayedDelete.New())
			{
				using (Bitmap sourceImage = (Bitmap)Bitmap.FromFile(sourceImageFullPath))
				{
					ConvertToTiff(sourceImage, convertedTifFile.Filename, Env.Registry.PDFTIFResolution, IsRegistryBlackAndWhite, CompressionType.LZW);
				}

				PdfWriter pdf = new PdfWriter();
				pdf.YAxisGrowsDown = true; //this is to keep it as GDI+, on pdf coords y axis goes up.
				using (FileStream pdfFile = new FileStream(outputFileFullPath, FileMode.Create))
				{
					using (FileStream imgData = new FileStream(convertedTifFile.Filename, FileMode.Open))
					{
						using (ZImage imageToConvert = ZImage.FromStream(imgData))
						{
							int pageCount = imageToConvert.PageCount;
							TPaperDimensions a4 = new TPaperDimensions(TPaperSize.A4);
							TPaperDimensions a4Rotated = new TPaperDimensions(TPaperSize.A4Rotated);

							if (pageCount == 0)
							{
								pdf.BeginDoc(pdfFile); //This one is to create an empty doc if there are no sheets.  
							}

							RectangleF portraitPageDimensions = new RectangleF(0, 0, (float)pdf.PageSize.Width / 100f * 72f, (float)pdf.PageSize.Height / 100f * 72f);
							RectangleF landscapePageDimensions = new RectangleF(0, 0, (float)pdf.PageSize.Height / 100f * 72f, (float)pdf.PageSize.Width / 100f * 72f);
							RectangleF pageDimensions;

							for (int i = 0; i < pageCount; i++)
							{
								imageToConvert.CurrentPageIndex = i;
								pageDimensions = imageToConvert.IsA4LandscapePage ? landscapePageDimensions : portraitPageDimensions;
								pdf.PageSize = imageToConvert.IsA4LandscapePage ? a4Rotated : a4;

								if (!imageToConvert.IsA4Page) // scale it so it belongs on a4
								{
									pageDimensions = new RectangleF(new PointF(0, 0), GetAdjustedSizeForA4Page(imageToConvert, portraitPageDimensions));
								}

								if (i == 0)
								{
									pdf.BeginDoc(pdfFile); //after we set the papersize.
								}
								else
								{
									pdf.NewPage();
								}

								var imageAsTUIImage = (TUIImage)(Image)imageToConvert;
								pdf.DrawImage(imageAsTUIImage, pageDimensions, imgData);
							}
						}
					}
					pdf.EndDoc();
				}
			}
		}

		public static byte[] ConvertTIFToPDF(byte[] sourceTIFAsBytes)
		{
			byte[] returnBytes;

			using (TempFileWithDelayedDelete outputPDFFile = TempFileWithDelayedDelete.NewWithExtension("PDF"))
			using (TempFileWithDelayedDelete inputTIFFile = TempFileWithDelayedDelete.NewWithExtension("TIF"))
			{
				using (FileStream writer = new FileStream(inputTIFFile.Filename, FileMode.Open))
				{
					writer.Write(sourceTIFAsBytes, 0, sourceTIFAsBytes.Length);
				}

				ConvertTIFToPDF(inputTIFFile.Filename, outputPDFFile.Filename);
				returnBytes = File.ReadAllBytes(outputPDFFile.Filename);
			}

			return returnBytes;
		}

		public static void ConvertTIFToPDF(byte[] sourceTifBytes, string outputFileFullPath)
		{
			using (var inputTifFile = TempFileWithDelayedDelete.NewWithExtension("TIF"))
			{
				using (var writer = new FileStream(inputTifFile.Filename, FileMode.Open))
				{
					writer.Write(sourceTifBytes, 0, sourceTifBytes.Length);
				}

				ConvertTIFToPDF(inputTifFile.Filename, outputFileFullPath);
			}
		}

		internal static bool IsRegistryBlackAndWhite
		{
			get
			{
				return Env.Registry.PDFTIFColourDepth.ToString() == nameof(ColourDepth.BlackAndWhite);
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "These pixels are not for rendering on screen")]
		static SizeF GetAdjustedSizeForA4Page(Image imageToConvert, RectangleF portraitPageDimensions)
		{
			SizeF newSize = new SizeF(0, 0);

			if (imageToConvert.Width > imageToConvert.Height)
			{
				float ratio = portraitPageDimensions.Width / imageToConvert.Width;
				newSize.Width = portraitPageDimensions.Width;
				newSize.Height = imageToConvert.Height * ratio;
			}
			else
			{
				float ratio = portraitPageDimensions.Height / imageToConvert.Height;
				newSize.Height = portraitPageDimensions.Height;
				newSize.Width = imageToConvert.Width * ratio;
			}

			return newSize;
		}

		public static string GetFileExtensionFromAttachmentFormat(string attachmentFormat, int attachmentNumber = 0)
		{
			switch (attachmentFormat)
			{
				case AttachmentTypeList.Codes.Htmf:
					return attachmentNumber == 0 ? AttachmentTypeList.Codes.Html : AttachmentTypeList.Codes.Pdf;
				case AttachmentTypeList.Codes.Pdfa:
				case AttachmentTypeList.Codes.Pdfc:
					return AttachmentTypeList.Codes.Pdf;
			}

			return attachmentFormat;
		}

		#region Convert To Tiff

		public static byte[] CompressTIFImage(byte[] sourceData)
		{
			byte[] compressedTIFBytes;
			using (TempFile tempFile = TempFile.New())
			{
				ConvertToTiff(sourceData, tempFile.Filename, Env.Registry.PDFTIFResolution, IsRegistryBlackAndWhite, CompressionType.LZW);
				compressedTIFBytes = File.ReadAllBytes(tempFile.Filename);
			}
			return compressedTIFBytes;
		}

		public static void ConvertToTiff(Bitmap source, string outputFileFullPath, int resolution, bool blackAndWhite, CompressionType compression)
		{
			ConvertToTiff(source, outputFileFullPath, resolution, blackAndWhite, compression, false);
		}

		public static void ConvertToTiff(byte[] sourceData, string outputFileFullPath, int resolution, bool blackAndWhite, CompressionType compression)
		{
			ConvertToTiff(sourceData, outputFileFullPath, resolution, blackAndWhite, compression, false);
		}

		static void ConvertToTiff(byte[] sourceData, string outputFileFullPath, int resolution, bool blackAndWhite, CompressionType compression, bool toFax)
		{
			using (MemoryStream sourceStream = new MemoryStream(sourceData))
			{
				using (Bitmap source = TryToGetBitmapFromStream(sourceStream))
				{
					if (source != null)
					{
						ConvertToTiff(source, outputFileFullPath, resolution, blackAndWhite, compression, toFax);
					}
				}
			}
		}

		static Bitmap TryToGetBitmapFromStream(MemoryStream sourceStream)
		{
			Bitmap result = null;

			// .NET's Bitmap fails to be created from some formats of tiff files.
			// Thus, we try to convert it and if failed due to ArgumentException - return null
			try
			{
				result = (Bitmap)Image.FromStream(sourceStream);
			}
			catch (ArgumentException)
			{ }

			return result;
		}

		static void ConvertToTiff(Bitmap source, string outputFileFullPath, int resolution, bool blackAndWhite, CompressionType compression, bool toFax)
		{
			ImageCodecInfo info = FileSaveHelper.GetTiffEncoder();
			int paramCount = 2;
			EncoderParameters ep = new EncoderParameters(paramCount);
			ep.Param[0] = new EncoderParameter(Encoder.SaveFlag, (long)EncoderValue.MultiFrame);
			ep.Param[1] = new EncoderParameter(Encoder.Compression, GetCompressionType(compression));

			source.SelectActiveFrame(FrameDimension.Page, 0);
			float workingResolution = CalcWorkingResolution(source, resolution, toFax);

			var width = Math.Max(1, (int)(source.Width * workingResolution / source.HorizontalResolution));
			var height = Math.Max(1, (int)(source.Height * workingResolution / source.VerticalResolution));

			using (Bitmap outImg = new Bitmap(width, height, GetPixelFormat(blackAndWhite, source.PixelFormat)))
			{
				outImg.SetResolution(workingResolution, workingResolution);
				//First image is handled differently.
				ProcessImage(source, outImg, blackAndWhite);
				outImg.Save(outputFileFullPath, info, ep);

				ep.Param[0] = new EncoderParameter(Encoder.SaveFlag, (long)EncoderValue.FrameDimensionPage);

				//Now the rest of images.

				Bitmap page = null;

				for (int i = 1; i < source.GetFrameCount(FrameDimension.Page); i++)
				{
					source.SelectActiveFrame(FrameDimension.Page, i);
					workingResolution = CalcWorkingResolution(source, resolution, toFax);
					int newWidth = (int)(source.Width * workingResolution / source.HorizontalResolution);
					int newHeight = (int)(source.Height * workingResolution / source.VerticalResolution);

					using (page = new Bitmap(newWidth, newHeight, GetPixelFormat(blackAndWhite, source.PixelFormat)))
					{
						page.SetResolution(workingResolution, workingResolution);
						ProcessImage(source, page, blackAndWhite);
						outImg.SaveAdd(page, ep);
					}
					page = null;
				}

				ep.Param[0] = new EncoderParameter(Encoder.SaveFlag, (long)EncoderValue.Flush);
				outImg.SaveAdd(ep);
			}
		}

		static float CalcWorkingResolution(Bitmap source, int resolution, bool toFax)
		{
			//If the resolution is already lower than what we need, keep it that way.
			//This does not apply to fax.
			if (!toFax && source.HorizontalResolution == source.VerticalResolution && source.HorizontalResolution < resolution)
			{
				return (int)source.HorizontalResolution;
			}
			return resolution;
		}

		static long GetCompressionType(CompressionType compression)
		{
			switch (compression)
			{
				case CompressionType.CCITT3:
					return (long)EncoderValue.CompressionCCITT3;
				case CompressionType.LZW:
				default:
					return (long)EncoderValue.CompressionLZW;
			}
		}

		static void ProcessImage(Bitmap source1, Bitmap outImg, bool blackAndWhite)
		{
			BitmapResult source = ResizeSourceToOutImgDimensions(source1, outImg);
			if (blackAndWhite)
			{
				MakeBlackAndWhiteIfRequired(source.Image, outImg);
			}
			else
			{
				if (outImg.PixelFormat == PixelFormat.Format8bppIndexed)
				{
					using (Bitmap srcImg = ConvertToARGB32(source.Image))
					{
						OctreeQuantizer.ConvertTo256Colors(srcImg, outImg);
					}
				}
				else //PixelFormat is 1 bpp.
				{
					MakeBlackAndWhiteIfRequired(source.Image, outImg);
				}
			}
			if (source.Converted)
			{
				source.Image.Dispose();
				source.Image = null;
			}
		}

		static void MakeBlackAndWhiteIfRequired(Bitmap source, Bitmap outImg)
		{
			if (source.PixelFormat == outImg.PixelFormat)
			{
				BitmapClone.Copy(source, outImg);
			}
			else
			{
				using (Bitmap srcImg = ConvertToARGB32(source))
				{
					FloydSteinbergDither.ConvertToBlackAndWhite(srcImg, outImg);
				}
			}
		}

		static Bitmap ConvertToARGB32(Bitmap source)
		{
			if (source.PixelFormat == PixelFormat.Format32bppPArgb)
			{
				return (Bitmap)source.Clone();
			}
			Bitmap result = null;
			result = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppPArgb);
			try
			{
				result.SetResolution(source.HorizontalResolution, source.VerticalResolution);
				using (Graphics gr = Graphics.FromImage(result))
				{
					gr.InterpolationMode = InterpolationMode.NearestNeighbor;
					gr.DrawImageUnscaled(source, 0, 0);
				}
			}
			catch
			{
				result.Dispose();
				throw;
			}
			return result;
		}

		internal static PixelFormat GetPixelFormat(bool blackAndWhite, PixelFormat sourceFormat)
		{
			if (blackAndWhite)
			{
				return PixelFormat.Format1bppIndexed;
			}
			return sourceFormat == PixelFormat.Format1bppIndexed ? sourceFormat : PixelFormat.Format8bppIndexed;
		}

		class BitmapResult
		{
			public BitmapResult(Bitmap img, bool converted)
			{
				Image = img;
				Converted = converted;
			}

			public Bitmap Image { get; set; }
			public bool Converted { get; private set; }
		}

		static BitmapResult ResizeSourceToOutImgDimensions(Bitmap source, Bitmap outImg)
		{
			BitmapResult result = null;
			if (source.Size == outImg.Size)
			{
				result = new BitmapResult(source, false);
			}
			else
			{
				result = new BitmapResult(new Bitmap(source, outImg.Size), true);
			}
			return result;
		}

		#endregion

		#region Convert to Fax

		public static void ConvertToFax(Bitmap source, string outputFileFullPath)
		{
			ConvertToTiff(source, outputFileFullPath, 196, true, CompressionType.CCITT3, true);
		}

		public static void ConvertToFax(byte[] sourceData, string outputFileFullPath)
		{
			ConvertToTiff(sourceData, outputFileFullPath, 196, true, CompressionType.CCITT3, true);
		}

		#endregion

		public static class BitmapClone
		{
			/// <summary>
			/// Copies the source image into result. Both images have to be the same size and same pixelformat.
			/// </summary>
			[SuppressMessage("CargoWiseOne", "CW1017", Justification = "These values do not represent pixels")]
			public static void Copy(Bitmap source, Bitmap result)
			{
				if (result.PixelFormat != source.PixelFormat)
				{
					throw new ArgumentException("Both Source and Result have to have the same PixelFormat");
				}

				if (result.Size != source.Size)
				{
					throw new ArgumentException("Both Source and Result have to have the same Size");
				}

				BitmapData destBits = result.LockBits(new Rectangle(0, 0, result.Width, result.Height), ImageLockMode.ReadWrite, result.PixelFormat);
				try
				{
					int sourceWidth = source.Width;
					int sourceHeight = source.Height;
					//lock the bits of the original bitmap
					BitmapData sourceBits = source.LockBits(new Rectangle(0, 0, sourceWidth, sourceHeight), ImageLockMode.ReadOnly, source.PixelFormat);
					try
					{
						int sourceBitsStride = sourceBits.Stride;
						if (destBits.Stride != sourceBitsStride)
						{
							throw new ArgumentException("Both Source and Result have to have the same Size");
						}

						IntPtr sourceBitsScan0 = sourceBits.Scan0;
						IntPtr destBitsScan0 = destBits.Scan0;

						byte[] buff = new byte[sourceBitsStride * sourceHeight];
						Marshal.Copy(sourceBitsScan0, buff, 0, sourceBitsStride * sourceHeight);
						Marshal.Copy(buff, 0, destBitsScan0, sourceBitsStride * sourceHeight);
					}
					finally
					{
						source.UnlockBits(sourceBits);
					}
				}
				finally
				{
					result.UnlockBits(destBits);
				}

				result.Palette = source.Palette;
			}
		}

		public static void MergePDFs(IEnumerable<string> sourceFilesFullPath, string outputFileFullPath, ICollection<string> invalidPDF)
		{
			using (var outPdf = new PdfDocument())
			{
				foreach (var source in sourceFilesFullPath)
				{
					try
					{
						using (var sourcePdf = PdfDocument.LoadFile(source))
						{
							outPdf.InsertPages(sourcePdf, outPdf.PageCount);
						}
					}
					catch (PdfiumException)
					{
						invalidPDF.Add(source);
					}
				}

				if (outPdf.PageCount > 0)
				{
					outPdf.Save(outputFileFullPath);
				}
			}
		}

		public static ZBlob MergeXLSs(IEnumerable<ZBlob> xlsDatas)
		{
			XlsFile outputFile = null;

			foreach (var data in xlsDatas)
			{
				var inputFile = new XlsFile();
				using (var inputMemoryStream = new MemoryStream(data))
				{
					inputFile.Open(inputMemoryStream);
				}

				if (outputFile == null)
				{
					outputFile = inputFile;
				}
				else
				{
					MergeXls(outputFile, inputFile);
				}
			}

			using (var outputMemoryStream = new MemoryStream())
			{
				outputFile.Save(outputMemoryStream);

				return outputMemoryStream.CopyToByteArray();
			}
		}

		static void MergeXls(XlsFile outputFile, XlsFile inputFile)
		{
			for (var sheetNumber = 1; sheetNumber <= inputFile.SheetCount; sheetNumber++)
			{
				inputFile.ActiveSheet = sheetNumber;
				if (inputFile.SheetVisible == TXlsSheetVisible.Visible)
				{
					outputFile.InsertAndCopySheets(sheetNumber, outputFile.SheetCount + 1, 1, inputFile);
				}
			}
		}

		static void ReplaceSignatureFormatForETSICompliance(string outputFileFullPath, ZGuid? branchPk)
		{
			if (branchPk == null)
			{
				return;
			}

			var signingOptionProvider = DocumentsDataRegistry.Instance.DocumentSigningServiceCredentials.GetFallBackValueAtAllLevels(Guid.Empty, branchPk.Value.ToGuid(), Guid.Empty);
			if (signingOptionProvider == null)
			{
				return;
			}

			if (signingOptionProvider.ProviderCode == PdfSigningOptionCodes.DigitalSign)
			{
				var hexToFind = "616462652E706B6373372E6465746163686564"; // adbe.pkcs7.detached
				var hexToReplace = "455453492E43416445532E6465746163686564"; // ETSI.CAdES.detached

				var bytesToFind = HexStringToBytes(hexToFind);
				var bytesToReplace = HexStringToBytes(hexToReplace);

				if (bytesToFind.Length != bytesToReplace.Length)
				{
					throw new DocumentConvertException("Bytes lenghts mismatch. Can't create the PDF otherwise it will be corrupted.");
				}

				var match = true;
				var pdfBytes = File.ReadAllBytes(outputFileFullPath);
				for (var i = 0; i <= pdfBytes.Length - bytesToFind.Length; i++)
				{
					match = true;
					for (var j = 0; j < bytesToFind.Length; j++)
					{
						if (pdfBytes[i + j] != bytesToFind[j])
						{
							match = false;
							break;
						}
					}

					if (match)
					{
						for (var j = 0; j < bytesToFind.Length; j++)
						{
							pdfBytes[i + j] = bytesToReplace[j];
						}
						break;
					}
				}

				if (!match)
				{
					throw new DocumentConvertException("No adbe.pkcs7.detached format found.");
				}

				var successfulSave = false;
				var savedFileAttempts = 0;
				while (savedFileAttempts < 3 && !successfulSave)
				{
					try
					{
						File.WriteAllBytes(outputFileFullPath, pdfBytes);
						successfulSave = true;
					}
					catch (Exception e) when (!e.IsCriticalException())
					{
						savedFileAttempts++;
						Thread.Sleep(200);
					}
				}

				byte[] HexStringToBytes(string hex)
				{
					var length = hex.Length;
					var bytes = new byte[length / 2];

					for (var i = 0; i < length; i += 2)
					{
						bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
					}

					return bytes;
				}
			}
		}
	}

	public enum CompressionType
	{
		CCITT3,
		LZW
	}
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.BrandManager;
using CargoWise.Types;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.FlexCelInterface.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentEngineCore.Registry.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using FlexCel.XlsAdapter;
using NUnit.Framework;
using static Enterprise.DocumentEngineCore.Registry.DocumentSigningRegistryConstants;

namespace Enterprise.DocumentEngine.FileFormatUtilities.Testing
{
	sealed class DocumentConverterTest : TransactionedTestCase
	{
		public void TestRemoveUnnecessaryEDILinkPrefixForHTML()
		{
			var html = "<html>Hello <a href=\"file://edient:Command=ShowEditForm&amp;LicenceCode=WTLDCNJNC&amp;ControllerID=JobShipment&amp;BusinessEntityPK=208dd56f-16d8-43d6-a7c2-d2974bc3f923&amp;Hash=%2bnwydvFXRLOpdzyqY4EikfbeKxGiaOPDw\"  title=\"Click Here\"style='text-decoration: none;'>";
			var formattedHtml = DocumentConverter.RemoveUnnecessaryEDILinkPrefixForHTML(html);
			AssertEquals("<html>Hello <a href=\"edient:Command=ShowEditForm&amp;LicenceCode=WTLDCNJNC&amp;ControllerID=JobShipment&amp;BusinessEntityPK=208dd56f-16d8-43d6-a7c2-d2974bc3f923&amp;Hash=%2bnwydvFXRLOpdzyqY4EikfbeKxGiaOPDw\"  title=\"Click Here\"style='text-decoration: none;'>", formattedHtml);
		}

		public void TestFormatEmptyTableForHtml()
		{
			var html = $@"<html>
				{DocumentConverter.startOfCss}
				{DocumentConverter.endOfCss}
                <table>
                <tr>
                <td class='imagecell' colspan='47' rowspan='8' style='width:527.42pt;height:97.12pt;'>
                <img src='cid:d3273bae1d3748d9865a47417074dd78.png' width='701' height='126' alt='Picture 2' class='imagediv' style='margin-top: 0.71pt; margin-left: 1.59pt; z-index:1;width:525.75pt;height:94.5pt;' v:shapes='picture3c64933415854449b0f467d78678c6e41'>
                <table border='0' cellpadding='0' cellspacing='0' summary='Helper container' style='border-collapse: collapse; border-spacing: 0;'><tr>
                <td class='flx2' style='width:527.42pt;height:94.5pt;border:0;'>
                </td></tr></table>
                </td>
                </tr>
                </table>
                </body>
                </html>";
			var formattedHtml = DocumentConverter.HideEmptyTableForHtml(html);
			AssertContains(@"<table>
                <tr>
                <td class='imagecell' colspan='47' rowspan='8' style='width:527.42pt;height:97.12pt;'>
                <img src='cid:d3273bae1d3748d9865a47417074dd78.png' width='701' height='126' alt='Picture 2' class='imagediv' style='margin-top: 0.71pt; margin-left: 1.59pt; z-index:1;width:525.75pt;height:94.5pt;' v:shapes='picture3c64933415854449b0f467d78678c6e41'>
                <table border='0' cellpadding='0' cellspacing='0' summary='Helper container' style='border-collapse: collapse; border-spacing: 0;height: 0 !important;'><tr style=""height: 0 !important;"">
                <td class='flx2' style='width:527.42pt;height:94.5pt;border:0;height: 0 !important;'>
                </td></tr></table>", formattedHtml);

			var htmlHasInnerText = $@"<html>
				{DocumentConverter.startOfCss}
				{DocumentConverter.endOfCss}
                <table>
                <tr>
                <td class='imagecell' colspan='47' rowspan='8' style='width:527.42pt;height:97.12pt;'>
                <img src='cid:d3273bae1d3748d9865a47417074dd78.png' width='701' height='126' alt='Picture 2' class='imagediv' style='margin-top: 0.71pt; margin-left: 1.59pt; z-index:1;width:525.75pt;height:94.5pt;' v:shapes='picture3c64933415854449b0f467d78678c6e41'>
                <table border='0' cellpadding='0' cellspacing='0' summary='Helper container' style='border-collapse: collapse; border-spacing: 0;'><tr>
                <td class='flx2' style='width:527.42pt;height:94.5pt;border:0;'>
				<div>Test</div>
                </td></tr></table>
                </td>
                </tr>
                </table>
                </body>
                </html>";
			var formattedHtmlHasInnerText = DocumentConverter.HideEmptyTableForHtml(htmlHasInnerText);
			AssertContains(@"<table>
                <tr>
                <td class='imagecell' colspan='47' rowspan='8' style='width:527.42pt;height:97.12pt;'>
                <img src='cid:d3273bae1d3748d9865a47417074dd78.png' width='701' height='126' alt='Picture 2' class='imagediv' style='margin-top: 0.71pt; margin-left: 1.59pt; z-index:1;width:525.75pt;height:94.5pt;' v:shapes='picture3c64933415854449b0f467d78678c6e41'>
                <table border='0' cellpadding='0' cellspacing='0' summary='Helper container' style='border-collapse: collapse; border-spacing: 0;'><tr>
                <td class='flx2' style='width:527.42pt;height:94.5pt;border:0;'>
				<div>Test</div>
                </td></tr></table>
                </td>
                </tr>
                </table>", formattedHtmlHasInnerText);
		}

		public void TestIsBlackAndWhite()
		{
			var registryItem = Env.Registry.RawRegistry.FindByName("PDFTIFColourDepth");
			AssertNotNull(registryItem);

			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.ColourDepth.BlackAndWhite);
			AssertEquals(nameof(ColourDepth.BlackAndWhite), Env.Registry.PDFTIFColourDepth.ToString());
			Assert(DocumentConverter.IsRegistryBlackAndWhite);

			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.ColourDepth.Colour256);
			AssertEquals(nameof(ColourDepth.Colour256), Env.Registry.PDFTIFColourDepth.ToString());
			Assert(!DocumentConverter.IsRegistryBlackAndWhite);

			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.ColourDepth.TrueColour);
			AssertEquals(nameof(ColourDepth.TrueColour), Env.Registry.PDFTIFColourDepth.ToString());
			Assert(!DocumentConverter.IsRegistryBlackAndWhite);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInDirectReformatFromXlsToMulitpageTIFToBinaryData()
		{
			var data = GetXlsFile();
			using (var tiff = Image.FromStream(new MemoryStream(DocumentConverter.ConvertFromExcel(data, OutputFormatType.TIF, ColourDepth.Colour256))))
			{
				AssertEquals(tiff.RawFormat, ImageFormat.Tiff);
				AssertEquals((float)Env.Registry.PDFTIFResolution, tiff.HorizontalResolution);
				AssertEquals((float)Env.Registry.PDFTIFResolution, tiff.VerticalResolution);
				AssertEquals(PixelFormat.Format8bppIndexed, tiff.PixelFormat);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInDirectReformatFromXlsToMulitpageTIFToStream()
		{
			using (var data = GetXlsStream())
			using (var output = new MemoryStream())
			{
				DocumentConverter.WriteOutputFromExcel(data, output, OutputFormatType.TIF, null, ColourDepth.Colour256);
				output.Seek(0, SeekOrigin.Begin);
				using (Image tiff = Image.FromStream(output))
				{
					AssertEquals(tiff.RawFormat, ImageFormat.Tiff);
					AssertEquals((float)Env.Registry.PDFTIFResolution, tiff.HorizontalResolution);
					AssertEquals((float)Env.Registry.PDFTIFResolution, tiff.VerticalResolution);
					AssertEquals(PixelFormat.Format8bppIndexed, tiff.PixelFormat);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDirectReformatFromXlsToFax()
		{
			var data = GetXlsFile();

			var tempFileName = Env.GetTempFileName();
			try
			{
				DocumentConverter.ConvertFromExcel(data, tempFileName, OutputFormatType.FAX, null, ColourDepth.BlackAndWhite);
				using (var tiff = Image.FromFile(tempFileName))
				{
					AssertEquals(tiff.RawFormat, ImageFormat.Tiff);
					AssertEquals(tiff.HorizontalResolution, 196f);
					AssertEquals(tiff.VerticalResolution, 196f);
					AssertEquals(tiff.PixelFormat, PixelFormat.Format1bppIndexed);
				}
			}
			finally
			{
				File.Delete(tempFileName);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDirectReformatFromXlsToFaxStream()
		{
			using (var data = GetXlsStream())
			using (var output = new MemoryStream())
			{
				DocumentConverter.WriteOutputFromExcel(data, output, OutputFormatType.FAX, null, ColourDepth.BlackAndWhite);
				output.Seek(0, SeekOrigin.Begin);
				using (Image tiff = Image.FromStream(output))
				{
					AssertEquals(tiff.RawFormat, ImageFormat.Tiff);
					AssertEquals(tiff.HorizontalResolution, 196f);
					AssertEquals(tiff.VerticalResolution, 196f);
					AssertEquals(tiff.PixelFormat, PixelFormat.Format1bppIndexed);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestConvertFromExcel_DoesntAcceptHTML()
		{
			var data = GetXlsFile();

			var tempFileName = Env.GetTempFileName();

			try
			{
				AssertExceptionThrown(typeof(DocumentConvertException), () => { DocumentConverter.ConvertFromExcel(data, tempFileName, OutputFormatType.HTML, null, ColourDepth.TrueColour); });
				AssertExceptionThrown(typeof(DocumentConvertException), () => { DocumentConverter.ConvertFromExcel(data, tempFileName, OutputFormatType.HTMF, null, ColourDepth.TrueColour); });
			}
			finally
			{
				TempFile.TryDeleteHandleAllExceptions(tempFileName);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestWriteOutputFromExcel_DoesntAcceptHTML()
		{
			using (var data = GetXlsStream())
			using (var output = new MemoryStream())
			{
				AssertExceptionThrown<DocumentConvertException>(
					"HTML file format requires additional data to be returned. Please call function 'ConvertFromExcel_IncludingHTML' instead.",
					() => { DocumentConverter.WriteOutputFromExcel(data, output, OutputFormatType.HTML, null, ColourDepth.TrueColour); });
				AssertExceptionThrown<DocumentConvertException>(
					"HTML file format requires additional data to be returned. Please call function 'ConvertFromExcel_IncludingHTML' instead.",
					() => { DocumentConverter.WriteOutputFromExcel(data, output, OutputFormatType.HTMF, null, ColourDepth.TrueColour); });
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDirectReformatFromXlsToHTML()
		{
			var data = GetXlsFile();

			var result = DocumentConverter.ConvertFromExcel_IncludingHTML(data, OutputFormatType.HTML, ColourDepth.TrueColour, 0);

			Assert(result.isHTML);
			Assert(result.Data.Length > 0);
			var htmlAsString = Encoding.UTF8.GetString(result.Data);
			AssertContains("<meta name=\"Generator\" content=\"FlexCel", htmlAsString);
			AssertContains("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">", htmlAsString);
			AssertContains("TestTemplate Header1", htmlAsString);
			Assert(DocumentConverter.IsHTMLFlexCelConversion(htmlAsString));
			AssertEquals(29, result.AdditionalAttachments.Count);
			foreach (var attachment in result.AdditionalAttachments)
			{
				Assert(attachment.Data.Length > 0);
				AssertContains("<img src='" + DocumentConverter.GetSafeImagesDirectoryName(result.Path) + "/" + attachment.Filename + "' ", htmlAsString);
			}

			htmlAsString = DocumentConverter.PostProcessHTMLForEmail(htmlAsString);
			foreach (var attachment in result.AdditionalAttachments)
			{
				Assert(attachment.Data.Length > 0);
				//attachment.Filename = "0e5371120d9342d9a2085ef48268e872.png"
				Assert(new Regex("^[a-f0-9]{32}\\.png$").IsMatch(attachment.Filename));
				AssertContains("v:imagedata src=\"cid:" + attachment.Filename + "\" ", htmlAsString);
				AssertContains("<img src='cid:" + attachment.Filename + "' ", htmlAsString);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDirectReformatFromXlsToXlsx()
		{
			var data = GetXlsFile();

			var tempFileName = Env.GetTempFileName();
			var tempFileName2 = Env.GetTempFileName();
			try
			{
				DocumentConverter.ConvertFromExcel(data, tempFileName, OutputFormatType.XLSX, null, ColourDepth.TrueColour);
				using (var xLInterface = new ExcelInterface())
				{
					xLInterface.LoadExcelFile(tempFileName);
					AssertEquals("XLSX", xLInterface.GetExtensionForExcelFromFile().ToUpperInvariant());
				}

				var xlsxData = File.ReadAllBytes(tempFileName);

				DocumentConverter.ConvertFromExcel(xlsxData, tempFileName2, OutputFormatType.XLS, null, ColourDepth.TrueColour);
				using (var xLInterface = new ExcelInterface())
				{
					xLInterface.LoadExcelFile(tempFileName2);
					AssertEquals("XLS", xLInterface.GetExtensionForExcelFromFile().ToUpperInvariant());
				}
			}
			finally
			{
				File.Delete(tempFileName);
				File.Delete(tempFileName2);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDirectReformatFromXlsToXlsxStream()
		{
			using (var data = GetXlsStream())
			using (var output1 = new MemoryStream())
			{
				DocumentConverter.WriteOutputFromExcel(data, output1, OutputFormatType.XLSX, null, ColourDepth.TrueColour);
				using (ExcelInterface xLInterface = new ExcelInterface())
				{
					output1.Seek(0, SeekOrigin.Begin);
					xLInterface.LoadExcelFile(output1);
					AssertEquals("XLSX", xLInterface.GetExtensionForExcelFromFile().ToUpperInvariant());
				}

				using (var output2 = new MemoryStream())
				{
					DocumentConverter.WriteOutputFromExcel(output1, output2, OutputFormatType.XLS, null, ColourDepth.TrueColour);
					using (ExcelInterface xLInterface = new ExcelInterface())
					{
						output2.Seek(0, SeekOrigin.Begin);
						xLInterface.LoadExcelFile(output2);
						AssertEquals("XLS", xLInterface.GetExtensionForExcelFromFile().ToUpperInvariant());
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDirectReformatFromXlsToPDF()
		{
			AssertDirectReformatFromXlsToPDF(OutputFormatType.PDF);
			AssertDirectReformatFromXlsToPDF(OutputFormatType.PDFA);
			AssertDirectReformatFromXlsToPDFStream(OutputFormatType.PDF);
			AssertDirectReformatFromXlsToPDFStream(OutputFormatType.PDFA);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestRequiresAdministrativePrivileges("read the private key of X509 Certificate")]
		public void TestDirectReformatFromXlsToSignedPDF()
		{
			AssertDirectReformatFromXlsToSignedPDF(DigitalSignatureTestHelper.GetDigitalSignatureRegistry());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestRequiresAdministrativePrivileges("read the private key of X509 Certificate")]
		public void TestDirectReformatFromXlsToSignedPDFStream()
		{
			AssertDirectReformatFromXlsToSignedPDFStream(DigitalSignatureTestHelper.GetDigitalSignatureRegistry());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestRequiresAdministrativePrivileges("read the private key of X509 Certificate")]
		public void TestDirectReformatFromXlsToSignedPDF_WithPasswordProtectedCertificate()
		{
			AssertDirectReformatFromXlsToSignedPDF(DigitalSignatureTestHelper.GetDigitalSignatureRegistry_WithPassword());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestRequiresAdministrativePrivileges("read the private key of X509 Certificate")]
		public void TestDirectReformatFromXlsToSignedPDFStream_WithPasswordProtectedCertificate()
		{
			AssertDirectReformatFromXlsToSignedPDFStream(DigitalSignatureTestHelper.GetDigitalSignatureRegistry_WithPassword());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDirectReformatFromXlsToSignedPDF_WithNoCertificate()
		{
			byte[] data = null;

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(Path.Combine(UnitTestingConstants.TestDocumentDigitalSignatureFilePath, "TestSignXlsDocument.xls"));
				using (var ms = new MemoryStream())
				{
					excelInterface.SaveToStream(ms);
					data = ms.ToArray();
				}
			}

			string tempFileName = Env.GetTempFileName();
			try
			{
				DocumentConverter.ConvertFromExcel(data, tempFileName, OutputFormatType.PDF, null, ColourDepth.BlackAndWhite, true, 1, true);

				var expectedFileName = "ExpectedUnsignedPDFFromXls.pdf";
				using (var expectedPdfData = File.OpenRead(Path.Combine(UnitTestingConstants.TestDocumentDigitalSignatureFilePath, expectedFileName)))
				using (var actualPdfData = File.OpenRead(tempFileName))
				{
					// Uncomment the following line if this test is failing and you want to see what the actual output is.
					//actualPdfData.CopyToFile(@"c:\tmp\" + expectedFileName);

					AssertMultilineASCIIEquals("Output PDF should be the same.", ImageToPDFConverterTest.GetStringForPDFComparison(expectedPdfData), ImageToPDFConverterTest.GetStringForPDFComparison(actualPdfData));
				}
			}
			finally
			{
				File.Delete(tempFileName);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDirectReformatFromXlsToSignedPDFStream_WithNoCertificate()
		{
			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(Path.Combine(UnitTestingConstants.TestDocumentDigitalSignatureFilePath, "TestSignXlsDocument.xls"));
				using (var ms = new MemoryStream())
				{
					excelInterface.SaveToStream(ms);
					using (var actualPdfData = new MemoryStream())
					{
						DocumentConverter.WriteOutputFromExcel(ms, actualPdfData, OutputFormatType.PDF, null, ColourDepth.BlackAndWhite, true, 1, true);

						var expectedFileName = "ExpectedUnsignedPDFFromXls.pdf";
						var expectedPdfData = File.ReadAllBytes(Path.Combine(UnitTestingConstants.TestDocumentDigitalSignatureFilePath, expectedFileName));

						actualPdfData.Seek(0, SeekOrigin.Begin);
						AssertMultilineASCIIEquals("Output PDF should be the same.", ImageToPDFConverterTest.GetStringForPDFComparison(expectedPdfData), ImageToPDFConverterTest.GetStringForPDFComparison(actualPdfData));
					}
				}
			}
		}

		public void TestGetFileExtensionFromAttachmentFormat()
		{
			foreach (CodeDescriptionPair entry in new AttachmentTypeList())
			{
				var expectedExtension = entry.Code == AttachmentTypeList.Codes.Pdfa || entry.Code == AttachmentTypeList.Codes.Pdfc ? AttachmentTypeList.Codes.Pdf : entry.Code == AttachmentTypeList.Codes.Htmf ? AttachmentTypeList.Codes.Html : entry.Code;
				AssertEquals(expectedExtension, DocumentConverter.GetFileExtensionFromAttachmentFormat(entry.Code));
			}
			AssertEquals("HTML", DocumentConverter.GetFileExtensionFromAttachmentFormat(AttachmentTypeList.Codes.Htmf, 0));
			AssertEquals("PDF", DocumentConverter.GetFileExtensionFromAttachmentFormat(AttachmentTypeList.Codes.Htmf, 1));
			AssertEquals("BLAH", DocumentConverter.GetFileExtensionFromAttachmentFormat("BLAH"));
		}

		/// <summary>
		/// Fax should be rescanned at 196 dpi.
		/// </summary>
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTiffToFax()
		{
			ReformatImageToFax(PrintProcessingConstants.TestMultipageTifFullPath);
		}

		/// <summary>
		/// Fax should be rescanned at 196 dpi.
		/// </summary>
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTiffToFax2()
		{
			ReformatImageToFax(PrintProcessingConstants.TestMixedbppTifFullPath);
		}

		/// <summary>
		/// Should keep the 1bpp format, not convert it to 8bpp even when this is what is asked.
		/// </summary>
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReformatFromTiff1bppToTiff8bpp()
		{
			ReformatImageToTiff((PrintProcessingConstants.TestFaxTifFullPath), 98, false, CompressionType.LZW, PixelFormat.Format1bppIndexed);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReformatFromTiffTrueColorToTiff8bpp()
		{
			ReformatImageToTiff((PrintProcessingConstants.Test32bppTifFullPath), 40, false, CompressionType.LZW, PixelFormat.Format8bppIndexed);
		}

		/// <summary>
		/// The image will be upgraded to 8bpp.
		/// </summary>
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReformatFromTiff4bppToTiff8bpp()
		{
			ReformatImageToTiff(PrintProcessingConstants.Test4bppTifFullPath, 72, false, CompressionType.LZW, PixelFormat.Format4bppIndexed);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReformatFromTiff8bppToTiff8bpp()
		{
			ReformatImageToTiff(PrintProcessingConstants.TestMultipageTifFullPath, 60, false, CompressionType.LZW, PixelFormat.Format8bppIndexed);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReformatFromMixedTiffToTiff8bpp()
		{
			ReformatImageToTiff(PrintProcessingConstants.TestMixedbppTifFullPath, 96, false, CompressionType.LZW, PixelFormat.Format8bppIndexed);
		}

		/// <summary>
		/// A document at a lower resolution should not be updated to higher.
		/// </summary>
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLowerResolution()
		{
			ReformatImageToTiff(PrintProcessingConstants.TestMixedbppTifFullPath, 500, false, CompressionType.LZW, PixelFormat.Format8bppIndexed);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestConvertTIFToPDF()
		{
			Env.Registry.RawRegistry.FindByName("PDFTIFColourDepth").SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.ColourDepth.TrueColour);

			using (TempFile outputFile = TempFile.NewWithExtension("pdf"))
			{
				DocumentConverter.ConvertTIFToPDF(PrintProcessingConstants.TestMixedbppTifFullPath, outputFile.Filename);

				using (var expectedPdfData = File.OpenRead(ExpectedPdfFilePath))
				using (var actualPdfData = File.OpenRead(outputFile.Filename))
				{
					// Uncomment the following line if this test is failing and you want to see what the actual output is.
					//actualPdfData.CopyToFile(@"c:\tmp\" + ExpectedPdfFileName);

					AssertMultilineASCIIEquals("Output PDF should be the same.", ImageToPDFConverterTest.GetStringForPDFComparison(expectedPdfData), ImageToPDFConverterTest.GetStringForPDFComparison(actualPdfData));
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestConvertTIFToPDFWithBytes()
		{
			Env.Registry.RawRegistry.FindByName("PDFTIFColourDepth").SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.ColourDepth.TrueColour);

			string sourceTIFFilePath = PrintProcessingConstants.TestMixedbppTifFullPath;
			byte[] actualPdfData = DocumentConverter.ConvertTIFToPDF(File.ReadAllBytes(sourceTIFFilePath));

			using (var expectedPdfData = File.OpenRead(ExpectedPdfFilePath))
			{
				// Uncomment the following line if this test is failing and you want to see what the actual output is.
				//actualPdfData.CopyToFile(@"c:\tmp\" + ExpectedPdfFileName);

				AssertMultilineASCIIEquals("Output PDF should be the same.", ImageToPDFConverterTest.GetStringForPDFComparison(expectedPdfData), ImageToPDFConverterTest.GetStringForPDFComparison(actualPdfData));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestConvertTIFToPDFWithBytesAndOutputPath()
		{
			Env.Registry.RawRegistry.FindByName("PDFTIFColourDepth").SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.ColourDepth.TrueColour);

			string sourceTIFFilePath = PrintProcessingConstants.TestMixedbppTifFullPath;

			using (var outputPdf = TempFile.NewWithExtension("PDF"))
			{
				DocumentConverter.ConvertTIFToPDF(File.ReadAllBytes(sourceTIFFilePath), outputPdf.Filename);

				using (var expectedPdfData = File.OpenRead(ExpectedPdfFilePath))
				using (var actualPdfData = File.OpenRead(outputPdf.Filename))
				{
					// Uncomment the following line if this test is failing and you want to see what the actual output is.
					//actualPdfData.CopyToFile(@"c:\tmp\" + ExpectedPdfFileName);

					AssertMultilineASCIIEquals("Output PDF should be the same.",
						ImageToPDFConverterTest.GetStringForPDFComparison(expectedPdfData),
						ImageToPDFConverterTest.GetStringForPDFComparison(actualPdfData));
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCompressTIFImage()
		{
			string fullColourFilename = PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.Test32bppTifFileName;
			byte[] trueColourBytes = File.ReadAllBytes(fullColourFilename);
			byte[] compressedBytes = DocumentConverter.CompressTIFImage(trueColourBytes);

			Assert("Number of Compressed bytes should be less than the true colour bytes", compressedBytes.Length < trueColourBytes.Length);

			using (TempFile file = TempFile.New())
			{
				using (FileStream stream = new FileStream(file.Filename, FileMode.OpenOrCreate))
				{
					stream.Write(compressedBytes, 0, compressedBytes.Length);
				}

				using (Image compressedImage = Image.FromFile(file.Filename))
				{
					Assert("Image should not be true colour compression", compressedImage.PixelFormat != PixelFormat.Format24bppRgb);
					AssertEquals("Image should be the same pixel format as the registry specifies", GetRegistrySpecifiedPixelFormat(), compressedImage.PixelFormat);
				}
			}
		}

		[ExpectNoExceptions]
		public void TestConvertToTiffDoesNotThrowExceptionOnEmptyStream()
		{
			DocumentConverter.ConvertToTiff(Array.Empty<byte>(), "doesnotmatter", 0, true, CompressionType.LZW);
			DocumentConverter.ConvertToTiff(new byte[] { 0x10 }, "doesnotmatter", 0, true, CompressionType.LZW);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestConvertToTiffDoesNotThrowExceptionOnSmallBitmap()
		{
			using (var sourceImage = (Bitmap)Image.FromFile(PrintProcessingConstants.TestSmallTifFileFullPath))
			{
				DocumentConverter.ConvertToTiff(sourceImage, "doesNotMatter", 96, true, CompressionType.LZW);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestMergePDFs()
		{
			var sources = new List<string>
			{
				PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestPDFFileName,
				PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestPDFMultiPageFile
			};

			using (var outputPdf = TempFile.NewWithExtension("PDF"))
			{
				DocumentConverter.MergePDFs(sources, outputPdf.Filename, new List<string>());

				using (var expectedPdfData = File.OpenRead(PrintProcessingConstants.TestMergedPDFFileFullPath))
				using (var actualPdfData = File.OpenRead(outputPdf.Filename))
				{
					// Uncomment the following line if this test is failing and you want to see what the actual output is.
					// actualPdfData.CopyToFile(@"D:\" + ExpectedPdfFileName);

					AssertMultilineASCIIEquals("Output PDF should be the same.",
						ImageToPDFConverterTest.GetStringForPDFComparison(expectedPdfData),
						ImageToPDFConverterTest.GetStringForPDFComparison(actualPdfData));
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMerge_InvalidPDF()
		{
			var sources = new List<string>
			{
				PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestCorruptedPDF,
				PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestPDFMultiPageFile,
			};

			using (var outputPdf = TempFile.NewWithExtension("PDF"))
			{
				var listInvalidPDF = new List<string>();
				DocumentConverter.MergePDFs(sources, outputPdf.Filename, listInvalidPDF);

				CombineAssertions(() =>
				{
					AssertEquals("PDF Should have been flagged as invalid", 1, listInvalidPDF.Count);
					Assert(PrintProcessingConstants.TestCorruptedPDF + " should be marked as invalid", listInvalidPDF.Contains(PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestCorruptedPDF));
				});
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMerge_TwoInvalidPDFs()
		{
			var sources = new List<string>
			{
				PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestCorruptedPDF,
				PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestCorruptedPDF
			};

			using (var outputPdf = TempFile.NewWithExtension("PDF"))
			{
				var listInvalidPDF = new List<string>();
				DocumentConverter.MergePDFs(sources, outputPdf.Filename, listInvalidPDF);

				CombineAssertions(() =>
				{
					Assert("PDF is corrupt and should be added to invalid list", listInvalidPDF.Contains(PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestCorruptedPDF));
					AssertEquals("PDFs Should have been flagged as invalid", 2, listInvalidPDF.Count);
				});
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMergePDFs_WhenNamesContainUnicodeChars()
		{
			var sources = new List<string>
			{
				PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestPDFFileNameWithUnicodeChar,
				PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestPDFMultiPageFileWithUnicodeChar
			};

			using (var outputPdf = TempFile.NewWithExtension("PDF"))
			{
				AssertNoExceptionThrown(() => DocumentConverter.MergePDFs(sources, outputPdf.Filename, new List<string>()));

				using (var expectedPdfData = File.OpenRead(PrintProcessingConstants.TestMergedPDFFileFullPath))
				using (var actualPdfData = File.OpenRead(outputPdf.Filename))
				{
					AssertMultilineASCIIEquals("Output PDF should be the same.",
						ImageToPDFConverterTest.GetStringForPDFComparison(expectedPdfData),
						ImageToPDFConverterTest.GetStringForPDFComparison(actualPdfData));
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMergeXLSs()
		{
			var file = File.ReadAllBytes(PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestGeneratedReport);
			AssertExcelSheetCount(file, 1);

			var file2 = File.ReadAllBytes(PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestGeneratedReportWith2Sheets);
			AssertExcelSheetCount(file2, 2);

			var mergedFile = DocumentConverter.MergeXLSs(new ZBlob[] { file, file2 });

			AssertExcelSheetCount(mergedFile, 3);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestETSI_PDFComplianceForDigitalSignProvider()
		{
			var data = GetXlsFile();
			var tempFileName = Env.GetTempFileName();

			var signingServiceConfig = new DocumentSigningServiceCredentialsWithProviderConfiguration() { ProviderCode = PdfSigningOptionCodes.DigitalSign, AccessKey = "authId", ClientID = "authName", KeyID = "authSecretKey" };
			DocumentsDataRegistry.Instance.DocumentSigningServiceCredentials.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, signingServiceConfig);

			try
			{
				var result = DocumentConverter.ConvertFromExcel_IncludingHTML(data, tempFileName, OutputFormatType.PDF, null, ColourDepth.BlackAndWhite, true, 1, 1, true, signingOption: PdfSigningOptionCodes.Placeholder, branch: GlbBranch.CurrentBranch.PK);

				Assert(result.Data.Length > 0);

				var hexToFind = "455453492E43416445532E6465746163686564"; // ETSI.CAdES.detached
				var bytesToFind = HexStringToBytes(hexToFind);
				var pdfBytes = File.ReadAllBytes(tempFileName);
				var found = FindSequence(pdfBytes, bytesToFind);

				Assert("Document sign format is ETSI compliance", found);
			}
			finally
			{
				File.Delete(tempFileName);
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

			bool FindSequence(byte[] bytesArray, byte[] bytesToFind)
			{
				for (var i = 0; i <= bytesArray.Length - bytesToFind.Length; i++)
				{
					var match = true;
					for (var j = 0; j < bytesToFind.Length; j++)
					{
						if (bytesArray[i + j] != bytesToFind[j])
						{
							match = false;
							break;
						}
					}

					if (match)
					{
						return true;
					}
				}

				return false;
			}
		}

		public void TestGetSafeImagesDirectoryName()
		{
			var directoryName = DocumentConverter.GetSafeImagesDirectoryName("testTempFile.png");
			AssertNotContains("%", directoryName);
			AssertEndsWith("Should end with '_files'", "_files", directoryName);
		}

		byte[] GetXlsFile()
		{
			byte[] data = null;

			using (ExcelInterface xLInterface = new ExcelInterface())
			{
				xLInterface.LoadExcelFile(PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestGeneratedReport);
				using (MemoryStream ms = new MemoryStream())
				{
					xLInterface.SaveToStream(ms);
					data = ms.ToArray();
				}
			}
			return data;
		}

		Stream GetXlsStream()
		{
			var ms = new MemoryStream();
			using (var xLInterface = new ExcelInterface())
			{
				xLInterface.LoadExcelFile(PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestGeneratedReport);
				xLInterface.SaveToStream(ms);
			}
			return ms;
		}

		void AssertDirectReformatFromXlsToPDF(OutputFormatType outputFormatType)
		{
			var data = GetXlsFile();

			var tempFileName = Env.GetTempFileName();
			try
			{
				DocumentConverter.ConvertFromExcel(data, tempFileName, outputFormatType, null, ColourDepth.BlackAndWhite);
				using (var pdf = new StreamReader(tempFileName))
				{
					var signature = pdf.ReadLine();
					Assert("Generated file is not a valid PDF file: It should start with '%PDF' and starts with '" + signature + "'", signature.StartsWith("%PDF"));
				}
			}
			finally
			{
				File.Delete(tempFileName);
			}
		}

		void AssertDirectReformatFromXlsToPDFStream(OutputFormatType outputFormatType)
		{
			using (var data = GetXlsStream())
			using (var output = new MemoryStream())
			{
				DocumentConverter.WriteOutputFromExcel(data, output, outputFormatType, null, ColourDepth.BlackAndWhite);
				output.Seek(0, SeekOrigin.Begin);
				var signature = ImageToPDFConverterTest.GetStringForPDFComparison(output);
				Assert("Generated file is not a valid PDF file: It should start with '%PDF' and starts with '" + signature + "'", signature.StartsWith("%PDF"));
			}
		}

		void AssertDirectReformatFromXlsToSignedPDF(DigitalSignatureRegistry signatureRegistry)
		{
			byte[] data = null;
			DocumentsDataRegistry.Instance.DigitalSignature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, signatureRegistry);

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(Path.Combine(UnitTestingConstants.TestDocumentDigitalSignatureFilePath, "TestSignXlsDocument.xls"));
				using (var ms = new MemoryStream())
				{
					excelInterface.SaveToStream(ms);
					data = ms.ToArray();
				}
			}

			string tempFileName = Env.GetTempFileName();
			try
			{
				DocumentConverter.ConvertFromExcel(data, tempFileName, OutputFormatType.PDF, null, ColourDepth.BlackAndWhite, true, 1, true);

				var expectedFileName = "ExpectedSignedPDFFromXls.pdf";
				var expectedPdfData = File.ReadAllBytes(Path.Combine(UnitTestingConstants.TestDocumentDigitalSignatureFilePath, expectedFileName));
				var actualPdfData = File.ReadAllBytes(tempFileName);

				// Uncomment the following line if this test is failing and you want to see what the actual output is.
				//File.WriteAllBytes(@"c:\tmp\" + expectedFileName, actualPdfData);

				AssertMultilineASCIIEquals("Output PDF should be the same.", ImageToPDFConverterTest.GetStringForPDFComparison(expectedPdfData), ImageToPDFConverterTest.GetStringForPDFComparison(actualPdfData));

				var expectedSignature = new List<(string key, string value)>()
				{
					(("Location", "Sango")),
					(("Reason", $@"{BrandingFactory.Instance.CompanyBrandingName} \(WTG\) has provided the feature to generate and sign this PDF document using the data verified by the application users and available in {Core.Constants.ProductName} at the time of signing. WTG or any of its employees shall not be held responsible for any discrepancies observed in the data contained in this document.")),
					(("ContactInfo", "Sango@Sango.com"))
				};

				var actualSignature = ImageToPDFConverterTest.GetSignatureFromPDF(actualPdfData);

				AssertContainsExactElementsInAnyOrder(expectedSignature, actualSignature);
			}
			finally
			{
				File.Delete(tempFileName);
			}
		}

		void AssertDirectReformatFromXlsToSignedPDFStream(DigitalSignatureRegistry signatureRegistry)
		{
			DocumentsDataRegistry.Instance.DigitalSignature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, signatureRegistry);

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(Path.Combine(UnitTestingConstants.TestDocumentDigitalSignatureFilePath, "TestSignXlsDocument.xls"));
				using (var ms = new MemoryStream())
				{
					excelInterface.SaveToStream(ms);
					using (var actualPdfData = new MemoryStream())
					{
						DocumentConverter.WriteOutputFromExcel(ms, actualPdfData, OutputFormatType.PDF, null, ColourDepth.BlackAndWhite, true, 1, true);

						var expectedFileName = "ExpectedSignedPDFFromXls.pdf";
						var expectedPdfData = File.ReadAllBytes(Path.Combine(UnitTestingConstants.TestDocumentDigitalSignatureFilePath, expectedFileName));

						actualPdfData.Seek(0, SeekOrigin.Begin);
						AssertMultilineASCIIEquals("Output PDF should be the same.", ImageToPDFConverterTest.GetStringForPDFComparison(expectedPdfData), ImageToPDFConverterTest.GetStringForPDFComparison(actualPdfData));

						var expectedSignature = new List<(string key, string value)>()
						{
							(("Location", "Sango")),
							(("Reason", $@"{BrandingFactory.Instance.CompanyBrandingName} \(WTG\) has provided the feature to generate and sign this PDF document using the data verified by the application users and available in {Core.Constants.ProductName} at the time of signing. WTG or any of its employees shall not be held responsible for any discrepancies observed in the data contained in this document.")),
							(("ContactInfo", "Sango@Sango.com"))
						};

						var actualSignature = ImageToPDFConverterTest.GetSignatureFromPDF(actualPdfData.ToArray());

						AssertContainsExactElementsInAnyOrder(expectedSignature, actualSignature);
					}
				}
			}
		}

		void VerifyGeneratedFile(string fileName, int expectedFrameCount, float[] expectedResolution, PixelFormat[] expectedPixelFormat)
		{
			using (Image tiff = Image.FromFile(fileName))
			{
				AssertEquals(tiff.GetFrameCount(FrameDimension.Page), expectedFrameCount);
				for (int i = 0; i < tiff.GetFrameCount(FrameDimension.Page); i++)
				{
					tiff.SelectActiveFrame(FrameDimension.Page, i);
					AssertEquals(tiff.RawFormat, ImageFormat.Tiff);
					AssertEquals(tiff.HorizontalResolution, expectedResolution[i]);
					AssertEquals(tiff.VerticalResolution, expectedResolution[i]);
					AssertEquals(tiff.PixelFormat, expectedPixelFormat[i]);
				}
			}
		}

		void ReformatImageToTiff(string tiffFile, int resolution, bool blackAndWhite, CompressionType compression, PixelFormat pixFormat)
		{
			int expectedFrameCount = 0;
			float[] expectedResolution;
			PixelFormat[] expectedPixelFormat;

			using (Image img = Image.FromFile(tiffFile))
			{
				expectedFrameCount = img.GetFrameCount(FrameDimension.Page);
				expectedResolution = new float[expectedFrameCount];
				expectedPixelFormat = new PixelFormat[expectedFrameCount];

				for (int i = 0; i < expectedFrameCount; i++)
				{
					img.SelectActiveFrame(FrameDimension.Page, i);
					if (img.HorizontalResolution != img.VerticalResolution || img.HorizontalResolution > resolution)
					{
						expectedResolution[i] = resolution;
					}
					else
					{
						expectedResolution[i] = img.HorizontalResolution;
					}

					expectedPixelFormat[i] = DocumentConverter.GetPixelFormat(blackAndWhite, img.PixelFormat);
				}
			}

			byte[] tiffData = File.ReadAllBytes(tiffFile);
			using (TempFile tempFileName = TempFile.New())
			{
				DocumentConverter.ConvertToTiff(tiffData, tempFileName.Filename, resolution, blackAndWhite, compression);
				VerifyGeneratedFile(tempFileName.Filename, expectedFrameCount, expectedResolution, expectedPixelFormat);
			}
		}

		void ReformatImageToFax(string tiffFile)
		{
			int expectedFrameCount = 0;
			using (Image img = Image.FromFile(tiffFile))
			{
				expectedFrameCount = img.GetFrameCount(FrameDimension.Page);
			}

			byte[] tiffData = File.ReadAllBytes(tiffFile);
			using (TempFile tempFileName = TempFile.New())
			{
				DocumentConverter.ConvertToFax(tiffData, tempFileName.Filename);
				float[] expectedResolution = new float[expectedFrameCount];
				PixelFormat[] expectedPixelFormat = new PixelFormat[expectedFrameCount];
				for (int i = 0; i < expectedFrameCount; i++)
				{
					expectedResolution[i] = 196;
					expectedPixelFormat[i] = PixelFormat.Format1bppIndexed;
				}
				VerifyGeneratedFile(tempFileName.Filename, expectedFrameCount, expectedResolution, expectedPixelFormat);
			}
		}

		void AssertExcelSheetCount(byte[] data, int sheetCount)
		{
			var file = new XlsFile();
			var memoryStream = new MemoryStream(data);
			file.Open(memoryStream);

			AssertEquals("Sheet count", sheetCount, file.SheetCount);
		}

		PixelFormat GetRegistrySpecifiedPixelFormat() => DocumentConverter.IsRegistryBlackAndWhite ? PixelFormat.Format1bppIndexed : PixelFormat.Format8bppIndexed;

		string ExpectedPdfFilePath => Path.Combine(BaseSourcePath, "Enterprise", "Product", "Documents", "PrintProcessing", "PrintProcessing.Test", "Imaging", "Testing", ExpectedPdfFileName);

		const string ExpectedPdfFileName = "ExpectedPDFConversionOutput_x64.pdf";
	}
}

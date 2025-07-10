using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using AppDomainWrappers.Net;
using CargoWise.BuildTools;
using CargoWise.Common;
using CargoWise.IO;
using CargoWise.PdfiumWrapper;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore.Registry;
using FlexCel.AspNet;
using FlexCel.Core;
using FlexCel.Render;
using FlexCel.XlsAdapter;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.FlexCelInterface.Testing
{
	sealed class FlexCelTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestDataValidationFirstFormulaIsTooLong()
		{
			var stream = resourceRetriever.Value.GetResourceStream("DocumentWithDataValidationFirstFormulaIsTooLong.xlsx");
			var xls = new XlsFile();
			xls.Open(stream);
		}

		[DeveloperOnlyTest]
		public void TestArabicWhenExportingPdf()
		{
			PdfExportingAssertion("ArabicTest.xlsx", "ArabicTest.pdf", (originalStream, expectedStream) =>
			{
				var pdfData = originalStream.CopyToByteArray();
				AssertMultilineASCIIEquals("Output PDF should be the same.", ImageToPDFConverterTest.GetStringForPDFComparison(expectedStream), ImageToPDFConverterTest.GetStringForPDFComparison(pdfData));
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAssemblyInBinIsSameAsPrebuilt()
		{
			var dllName = "FlexCel.dll";
			AssertFileSameAsBytes(Path.Combine(AssemblyLoader.GetBinPath(), dllName), File.ReadAllBytes(Path.Combine(BaseSourcePath, "Common", "Testing", "Enterprise.Dat.Implementation", "Enterprise.Dat.SpecialFileHandling", "prebuilt", dllName)));
		}

		public void TestKhmerCharactersWhenExportingPdf()
		{
			PdfExportingAssertion("KhmerCharacters.xlsx", string.Empty, (originalStream, _) =>
			{
				using var pdfDocument = new PdfDocument(originalStream);
				var allText = pdfDocument.GetAllText();
				AssertContains("#ផ្ល ូវ ភូមិតាំង្រសឹង ឃុំសែង្កសាទប", allText);
			});
		}

		public void TestArabicNumberWhenExportingPdf()
		{
			PdfExportingAssertion("ArabicNumberTest.xlsx", string.Empty, (originalStream, _) =>
			{
				using var pdfDocument = new PdfDocument(originalStream);
				var allText = pdfDocument.GetAllText();
				var lines = allText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
				AssertEquals(3, lines.Length);
				AssertContains("۱۳/٤٤٤/۱۰۸۳/۱", lines[0]);
				AssertContains("13/444/1083/1", lines[1]);
				AssertContains("١٣/٤٤٤/١٠٨٣/١", lines[2]);
			});
		}

		void PdfExportingAssertion(string original, string expected, Action<Stream, Stream> assertAction)
		{
			var originalStream = resourceRetriever.Value.GetResourceStream(original);

			var excelFile = new XlsFile();
			excelFile.Open(originalStream);

			using var stream = new MemoryStream();
			using var pdfExport = new FlexCelPdfExportSafe(excelFile);
			pdfExport.BeginExport(stream);
			pdfExport.ExportSheet();
			pdfExport.EndExport();

			//var pdfData = stream.CopyToByteArray();
			// Uncomment the following line if this test is failing, and you want to see what the actual output is.
			//File.WriteAllBytes(@"c:\tmp\xxx.pdf", pdfData);

			Stream expectedStream = null;
			if (!string.IsNullOrEmpty(expected))
			{
				expectedStream = resourceRetriever.Value.GetResourceStream(expected);
			}

			using (expectedStream)
			{
				assertAction.Invoke(stream, expectedStream);
			}
		}

		[ExpectNoExceptions]
		public void TestFlexCelDoesNotThrowWhenOpening()
		{
			using var outputStream = new MemoryStream();
			var stream = resourceRetriever.Value.GetResourceStream("TestCrash-FlexCelv7-16-0-0.xlsx");
			var excelFile = new XlsFile();
			excelFile.Open(stream);
			excelFile.Save(outputStream, TFileFormats.Xls);
			outputStream.Position = 0;
			excelFile.Open(outputStream);
		}

		[ExpectNoExceptions]
		public void TestNoNREWhenMovingColumns()
		{
			var stream = resourceRetriever.Value.GetResourceStream("NREWhenMovingColumn.xlsx");
			var xls = new XlsFile();
			xls.Open(stream);
			var range = new TXlsCellRange(1, 3, FlxConsts.Max_Rows2007 - 1, 3);
			xls.MoveRange(range, 1, 2, TFlxInsertMode.ShiftColRight);
		}

		[ExpectNoExceptions]
		public void TestNoStuckWhenExportPDF()
		{
			using var pdfExport = new FlexCelPdfExportSafe(new XlsFile(resourceRetriever.Value.GetResourceStream("FlexcelStuckTest.xlsx"), true), true);
			using var stream = new MemoryStream();
			pdfExport.BeginExport(stream);
			pdfExport.ExportSheet();
			pdfExport.EndExport();
		}

		[ExpectNoExceptions]
		public void TestKhmerDocumentExportPDF()
		{
			using var pdfExport = new FlexCelPdfExportSafe(new XlsFile(resourceRetriever.Value.GetResourceStream("KhmerDocument.xls"), true), true);
			using var stream = new MemoryStream();
			pdfExport.BeginExport(stream);
			pdfExport.ExportSheet();
			pdfExport.EndExport();
		}

		[ExpectNoExceptions]
		public void TestExportingPDFWithCustomPropertiesThatHavePadding()
		{
			using var excelInterface = new ExcelInterface();
			using var stream = new MemoryStream();
			excelInterface.LoadExcelFile(resourceRetriever.Value.GetResourceStream("CustomPropertiesThatHavePadding.xls"));
			excelInterface.ExportToPdfAndScale(stream, 100, null);
		}

		[ExpectNoExceptions]
		public void TestNoIndexOutOfRangeExceptionThrownWhenDeletingColumn()
		{
			var xls = new XlsFile(resourceRetriever.Value.GetResourceStream("IndexOutOfRangeTest.xls"), true);
			var cellRange = new TXlsCellRange(1, 4, 1, 4);
			xls.DeleteRange(cellRange, TFlxInsertMode.ShiftColRight);
		}

		[ExpectNoExceptions]
		public void TestExportToPdfAndScaleWithPngImageInDocument()
		{
			using var excelInterface = new ExcelInterface();
			using var stream = new MemoryStream();
			excelInterface.LoadExcelFile(resourceRetriever.Value.GetResourceStream("ExportToPdfAndScaleWithPngImageInDocument.xlsx"));
			excelInterface.ExportToPdfAndScale(stream, 100, null);
		}

		[ExpectNoExceptions]
		public void TestGetObjectPropertiesWithBorderShape()
		{
			var excel = new XlsFile(resourceRetriever.Value.GetResourceStream("DocCrashingWhenCallingGetObjectProperties_Flexcelv7-1-0-0.xls"), true);
			var range1 = new TXlsCellRange(2, 1, 2, FlxConsts.Max_Columns2007);
			excel.InsertAndCopyRange(range1, 1, 1, 1, TFlxInsertMode.ShiftRowDown, TRangeCopyMode.All);

			var range2 = new TXlsCellRange(3, 1, 3, FlxConsts.Max_Columns2007);
			excel.DeleteRange(range2, TFlxInsertMode.ShiftRowDown);

			AssertEquals(1, excel.ObjectCount);
			var objectProperties = excel.GetObjectProperties(excel.ObjectCount, false);

			AssertNotNull(objectProperties);
		}

		public void TestGetListOfAllPublicIDisposableClasses()
		{
			var flexCelAssemblies = new List<Assembly>()
			{
				typeof(XlsFile).Assembly,
				typeof(FlexCelAspViewer).Assembly,
				Assembly.Load("FlexCelWinforms"),
			};

			var typeNames = GetDisposableTypeNames(flexCelAssemblies);

			typeNames.Sort();

			#region expectedTypeNames
			var expectedTypeNames = @"

FlexCel.AspNet.FlexCelAspExport
FlexCel.AspNet.FlexCelAspViewer
FlexCel.Core.TUIBrush
FlexCel.Core.TUIFont
FlexCel.Core.TUIGraphics
FlexCel.Core.TUIHatchBrush
FlexCel.Core.TUIImage
FlexCel.Core.TUIImageAttributes
FlexCel.Core.TUILinearGradientBrush
FlexCel.Core.TUIMultiPageSaver
FlexCel.Core.TUIPathGradientBrush
FlexCel.Core.TUIPen
FlexCel.Core.TUISolidBrush
FlexCel.Core.TUITextureBrush
FlexCel.Core.TZippyReader
FlexCel.Core.TZippyWriter
FlexCel.Draw.TGdipUIFont
FlexCel.Draw.TGdipUIGraphics
FlexCel.Draw.TGdipUIHatchBrush
FlexCel.Draw.TGdipUIImage
FlexCel.Draw.TGdipUIImageAttributes
FlexCel.Draw.TGdipUILinearGradientBrush
FlexCel.Draw.TGdipUIMultiPageParameters
FlexCel.Draw.TGdipUIPathGradientBrush
FlexCel.Draw.TGdipUIPen
FlexCel.Draw.TGdipUISolidBrush
FlexCel.Draw.TGdipUITextureBrush
FlexCel.Pdf.TPdfSigner
FlexCel.Render.FlexCelHtmlExport
FlexCel.Render.FlexCelImgExport
FlexCel.Render.FlexCelPdfExport
FlexCel.Render.FlexCelPrintDocument
FlexCel.Render.FlexCelSVGExport
FlexCel.Report.FlexCelReport
FlexCel.Report.TLinqDataTable`1
FlexCel.Report.TLinqDataTableState`1
FlexCel.Report.TOneCellValue
FlexCel.Report.VirtualDataTable
FlexCel.Report.VirtualDataTableState
FlexCel.Winforms.FlexCelPreview

".Trim();
			#endregion

			AssertMultilineASCIIEquals(@"typeNames in FlexCel Implementing IDisposable - Things that we have to dispose if and when we use them.

If this test starts failing after we update FlexCel, it means that either new classes have been added that are Disposable, or existing classes have been MADE disposable.

If an existing class is made disposable then we need to check our usage of that class to make sure the objects are being disposed properly.
", expectedTypeNames, string.Join("\r\n", typeNames.ToArray()));
		}

		public void TestSettingColumnsHiddenWithMultipleTabsWorks()
		{
			var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.MultipleTemplatesWithOptionalColumns.xls", "MultipleTemplatesWithOptionalColumns.xls");
			var sourceTemplate = new ExcelTemplateForUnitTesting("MultipleTemplatesWithOptionalColumns.xls", Path.GetFullPath(tempFileName));
			var xlsFile = new XlsFile();
			using var templateStream = sourceTemplate.GetAsTemplateStream();
			xlsFile.Open(templateStream);
			xlsFile.ActiveSheet = 1;
			xlsFile.SetColHidden(1, true);
			xlsFile.SetColWidth(1, 0);

			xlsFile.ActiveSheet = 2;
			xlsFile.SetColHidden(2, true);
			xlsFile.SetColWidth(2, 0);

			xlsFile.ActiveSheet = 3;
			xlsFile.SetColHidden(3, true);
			xlsFile.SetColWidth(3, 0);

			using var savedStream = new MemoryStream();
			xlsFile.Save(savedStream);

			savedStream.Seek(0, SeekOrigin.Begin);
			var xlsFileReloaded = new XlsFile();
			xlsFileReloaded.Open(savedStream);
			xlsFileReloaded.ActiveSheet = 3;
			Assert("sheet 3 column 3 is hidden", xlsFileReloaded.GetColHidden(3));

			xlsFileReloaded.ActiveSheet = 1;
			Assert("sheet 1 column 1 is hidden", xlsFileReloaded.GetColHidden(1));

			xlsFileReloaded.ActiveSheet = 2;
			Assert("sheet 2 column 2 is hidden", xlsFileReloaded.GetColHidden(2));
		}

		public void TestURLsConvertedToPDFKeepURLEncodedHyperLinks()
		{
			using var xlInterface = new ExcelInterface();
			xlInterface.NewExcelFile(1);
			var workSheet = xlInterface.WorkSheets[0];
			var hyperLinkURL = "http://localhost/WebTracker/AutoLoginRequestHandler.axd?AutoLogin=dc1xZ9KkXOd%2bToiGoLFgBtzyOfLkdlLyGGgLQVQM3PPLafyQq6QJ6xuAjs2Yohn3%2bPrG%2fSts8q6whKVF%2f7f39npxgzpzTZ%2f%2bwm1AvbC3nBCa74bvnu%2bKEX6ieNfCX%2fVeSqFP8qjQp2vgYEgP%2bnFCEnG946lr5q6iznVKki%2fx%25";
			var hyperLink = new ExcelHyperlink(THyperLinkType.URL, "Test Hyperlink", hyperLinkURL, "", "", "");
			workSheet[0, 0] = hyperLink;
			using var stream = new MemoryStream();
			using FlexCelPdfExport pdfExport = new FlexCelPdfExportSafe(xlInterface.Xls, true);
			pdfExport.FontEmbed = FlexCel.Pdf.TFontEmbed.None;
			pdfExport.Compress = false;
			pdfExport.BeginExport(stream);
			pdfExport.ExportSheet();
			pdfExport.EndExport();

			stream.Seek(0, SeekOrigin.Begin);
			using var reader = new StreamReader(stream);
			var pdfContent = reader.ReadToEnd();
			AssertEquals("pdfContent.Contains(hyperLinkURL)", true, pdfContent.Contains(hyperLinkURL));
		}

		[ExpectNoExceptions]
		public void TestFlexCelDoesNotExplodeConvertingA24TabSpreadsheetToAPdf()
		{
			using var xlInterface = new ExcelInterface();
			xlInterface.LoadExcelFile(resourceRetriever.Value.GetResourceStream("PDFConversionCrasher.xls"));
			using var pdfOutputStream = new MemoryStream();
			xlInterface.ExportToPdfAndScale(pdfOutputStream, 100, null);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestEmbeddedFontsDoNotEndUpCreatingA20MbPdfFile()
		{
			using var xlInterface = new ExcelInterface();
			xlInterface.LoadExcelFile(Path.Combine(BuildConstants.LocalEnterprisePath, @"Enterprise\Product\Documents\ExcelTemplates\Documents\Customs\NZ\NZCustomsDeliveryOrder.xls"));
			AssertEquals("Precondition: DocumentsDataRegistry.Instance.EmbedFontsInPDF.Value", true, DocumentsDataRegistry.Instance.EmbedFontsInPDF.Value);
			using var pdfOutputStream = new MemoryStream();
			xlInterface.ExportToPdfAndScale(pdfOutputStream, 100, null);
			AssertEquals("pdfOutputStream.Length < 200000 - Ended up being " + pdfOutputStream.Length.ToString(), true, (pdfOutputStream.Length < 200000));
		}

		/// <summary>
		/// Validates that a file saved with Excel can be recalculated with FlexCel.
		/// </summary>
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAllFormulasInTemplatesCanBeCalculated()
		{
			AssertFormulas(new DirectoryInfo(Path.Combine(BaseSourcePath, "Enterprise\\Product\\Documents\\ExcelTemplates")));
		}

		public void TestCellReplaceForRichString()
		{
			var workSheet = DocumentEngineTestHelper.CreateExcelWorkSheetFromString("{A}-[placeholder]");
			var excelFile = workSheet.ParentExcelInterface.Xls;
			var cellValue = new TRichString();
			cellValue.SetFromHtml("<b>&lt;Dexter&gt;</b> Morgan", excelFile.GetDefaultFormat, excelFile);
			AssertEquals(0, cellValue.RTFRun(0).FirstChar);
			cellValue = cellValue.Replace("<Dexter>", "");
			AssertEquals(0, cellValue.RTFRun(0).FirstChar);
		}

		[ExpectNoExceptions]
		public void TestCopyingRangeWithFunctionsIntroducedInExcel2007ToAnotherSheetDoesNotThrow()
		{
			var source = DocumentEngineTestHelper.CreateExcelWorkSheetFromString(@"{A}-F[=IFERROR(FIND(""X:"", B1), ""Y"")]").ParentExcelInterface.Xls;
			var target = new XlsFile(1, true);
			using var outputStream = new MemoryStream();
			target.ActiveSheet = 1;
			target.InsertAndCopyRange(new TXlsCellRange(1, 1, 1, 1), 2, 1, 1, TFlxInsertMode.ShiftRowDown, TRangeCopyMode.All, source, 1);
			target.Save(outputStream);
		}

		public void TestPdfExportDoesNotCreateBlankPagesWhenRowsAreHidden()
		{
			using var excelInterface = new ExcelInterface();
			//No hidden row, exports to PDF as 1 page
			excelInterface.LoadExcelFile(resourceRetriever.Value.GetResourceStream("OnePageForPDFExportBlankPageIssue.xls"));
			using (var pdfExport = new FlexCelPdfExportSafe(excelInterface.Xls))
			using (var stream = new MemoryStream())
			{
				pdfExport.Export(stream);
				AssertEquals(1, pdfExport.CurrentPage - 1); // CurrentPage returns the next page that we are going to print. So total rendered pages is CurrentPage - 1
			}

			//Let's hide a row. Should still export as 1 page
			excelInterface.WorkSheets[0].SetRowHeight(1, 0);
			using (var pdfExport = new FlexCelPdfExportSafe(excelInterface.Xls))
			using (var stream = new MemoryStream())
			{
				pdfExport.Export(stream);
				AssertEquals(1, pdfExport.CurrentPage - 1); // CurrentPage returns the next page that we are going to print. So total rendered pages is CurrentPage - 1
			}
		}

		[ExpectNoExceptions]
		public void TestPdfExportDoesNotCrashWithHiddenSheets()
		{
			using var excelInterface = new ExcelInterface();
			using var stream = new MemoryStream();
			excelInterface.LoadExcelFile(resourceRetriever.Value.GetResourceStream("PDFConversionCrasherWithHiddenSheet.xls"));
			excelInterface.ExportToPdfAndScale(stream, 100);
		}

		[ExpectNoExceptions]
		public void TestFlexCelDoesNotThrowWhenSavingAndOpeningFileWithAutoFilterOnDeletedRange()
		{
			using var outputStream = new MemoryStream();
			var excelFile = new XlsFile();
			excelFile.Open(resourceRetriever.Value.GetResourceStream("AutoFilter.xlsx"));

			var range = new TXlsCellRange(1, 1, 3, 16383);
			excelFile.DeleteRange(range, TFlxInsertMode.ShiftRowDown);
			excelFile.Save(outputStream, TFileFormats.Xlsx);

			outputStream.Position = 0;
			excelFile.Open(outputStream);
		}

		[ExpectNoExceptions]
		public void TestFlexCelDoesNotThrowWhenExportingLargeTextBoxToXls()
		{
			using var outputStream = new MemoryStream();
			var excelFile = new XlsFile();
			excelFile.Open(resourceRetriever.Value.GetResourceStream("DocCrashingWhenExportingLargeTextbox_FlexCelv6-17-0-0.xls"));
			excelFile.Save(outputStream, TFileFormats.Xls);

			outputStream.Position = 0;
			excelFile.Open(outputStream);
		}

		[ExpectNoExceptions]
		public void TestFlexCelDoesNotThrowWhenSettingFormats()
		{
			var excelFile = new XlsFile(resourceRetriever.Value.GetResourceStream("DocCrashingWhenModifyingFormats_FlexCelv6-8-9-0.xls"), true);
			var formatCount = excelFile.FormatCount;

			for (var index = 0; index < formatCount; index++)
			{
				var format = excelFile.GetFormat(index);
				format.Borders.Bottom.Color = TExcelColor.Automatic;
				excelFile.SetFormat(index, format);
			}
		}

		[ExpectNoExceptions]
		public void TestFlexCelDoesNotThrowWhenDeletingRanges()
		{
			var excelFile = new XlsFile(resourceRetriever.Value.GetResourceStream("DocCrashingWhenDeletingRows_FlexCelv6-10-0-0.xls"), true);
			var range = new TXlsCellRange(9, 1, 9, FlxConsts.Max_Columns);
			excelFile.DeleteRange(range, TFlxInsertMode.ShiftRowDown);
		}

		#region TestFlexCelExceptionsCanBeSerializedAndDeserialized
		public void TestFlexCelExceptionsCanBeSerializedAndDeserialized()
		{
			var appDomainWrapper = new AppDomainWrapper("DomainForFexCelExceptionTest");
			var config = new ProcessConfig
			{
				NamespacePath = "Enterprise.DocumentEngine.FlexCelInterface.Testing",
				ClassName = nameof(FlexCelTest),
				MethodName = nameof(TestFlexCelExceptionsCanBeSerializedAndDeserializedStatic),
			};
			config.AssemblyFile = Path.Combine(config.BinFolder, "Enterprise.DocumentEngine.Test.dll");
			var result = appDomainWrapper.RunMethodInProcess48(config);
			AssertEquals(string.Empty, result);
		}

		[ExpectNoExceptions]
		static void TestFlexCelExceptionsCanBeSerializedAndDeserializedStatic()
		{
			var adw = new AppDomainWrapper();
			var flexCelAssembly = Assembly.Load("FlexCel");
			foreach (var type in flexCelAssembly.GetTypes())
			{
				if (type.IsSubclassOf(typeof(Exception)))
				{
					var instance = adw.CreateInstanceAndUnwrap(flexCelAssembly.FullName, type.FullName);
					AssertEquals(type, instance.GetType());
				}
			}
		}
		#endregion

		[ExpectNoExceptions]
		public void TestFlexCelDoesNotCrashWithAsteriskInFormulas()
		{
			var xlsFile = new XlsFile(resourceRetriever.Value.GetResourceStream("FormulaEvaluationCrasher.xls"), true);
			xlsFile.Recalc(true);
		}

		[ExpectNoExceptions]
		public void TestFlexCelDoesNotThrowWhenExportingToPDF_WhenFileContainsUnicodeNonCharacterUFFFF()
		{
			var excelFile = new XlsFile(1, true);
			excelFile.SetCellValue(1, 1, '\uFFFF');
			using var stream = new MemoryStream();
			using var pdfExport = new FlexCelPdfExportSafe(excelFile);
			pdfExport.BeginExport(stream);
			pdfExport.ExportSheet();
			pdfExport.EndExport();
		}

		[ExpectNoExceptions]
		public void TestFlexCelDoesNotThrowWhenExportingToPDF_WhenFileHasBothRowsAndColumnsPrintTitles()
		{
			var xlsFile = new XlsFile(resourceRetriever.Value.GetResourceStream("PDFConversionCrasherWithRowsAndColumnsPrintTitles.xls"), true);

			using var stream = new MemoryStream();
			using var pdfExport = new FlexCelPdfExportSafe(xlsFile, true);
			pdfExport.BeginExport(stream);
			for (var i = 1; i <= xlsFile.SheetCount; i++)
			{
				xlsFile.ActiveSheet = i;
				if (xlsFile.SheetVisible == TXlsSheetVisible.Visible)
				{
					pdfExport.ExportSheet();
				}
			}
			pdfExport.EndExport();
		}

		public void TestImageTransparencyIsPreservedWhenExportingToXlsx()
		{
			using var excelInterface = new ExcelInterface();
			excelInterface.LoadExcelFile(resourceRetriever.Value.GetResourceStream("TransparentImage.xls"));
			AssertImageTransparency(excelInterface);

			using var outputStream = new MemoryStream();
			excelInterface.SaveToStream(outputStream, ExcelFileFormatOptionList.Codes.XLSX);

			outputStream.Position = 0;
			excelInterface.LoadExcelFile(outputStream);
			AssertImageTransparency(excelInterface);

			void AssertImageTransparency(ExcelInterface excel)
			{
				var worksheet = excel.WorkSheets[0];
				AssertEquals(1, worksheet.GetImageCount());
				var imageProperties = worksheet.GetImageProperties(0);
				AssertEquals(0xffffff, imageProperties.TransparentColor);
			}
		}

		[DeveloperOnlyTest]
		[RequiresSoftware(RequiredSoftware.Fonts)]
		public void TestKhmerPdfExport()
		{
			var xlsFile = new XlsFile(resourceRetriever.Value.GetResourceStream("Khmer.xlsx"), true);

			using var stream = new MemoryStream();
			using var pdfExport = new FlexCelPdfExportSafe(xlsFile, true);
			pdfExport.BeginExport(stream);
			for (var i = 1; i <= xlsFile.SheetCount; i++)
			{
				xlsFile.ActiveSheet = i;
				if (xlsFile.SheetVisible == TXlsSheetVisible.Visible)
				{
					pdfExport.ExportSheet();
				}
			}
			pdfExport.EndExport();

			var pdfData = stream.CopyToByteArray();
			// Uncomment the following line if this test is failing, and you want to see what the actual output is.
			//File.WriteAllBytes(@"C:\tmp\" + expectedFileName, pdfData);

			using var expected = resourceRetriever.Value.GetResourceStream("Khmer.pdf");
			AssertMultilineASCIIEquals("Output PDF should be the same.", ImageToPDFConverterTest.GetStringForPDFComparison(expected), ImageToPDFConverterTest.GetStringForPDFComparison(pdfData));
		}

		[ExpectNoExceptions]
		public void TestClearSheetDoesNotCrash()
		{
			var xlsFile = new XlsFile(resourceRetriever.Value.GetResourceStream("DocCrashingWhenClearingSheet_FlexCelv6-19-5-0.xls"), true);
			xlsFile.ClearSheet();
		}

		[ExpectNoExceptions]
		public void TestFourTrafficLights_ExportToTiffDoesNotCrash()
		{
			var xlsFile = new XlsFile(resourceRetriever.Value.GetResourceStream("4TrafficLights.xlsx"), true);
			using var stream = new MemoryStream();
			using var imageExport = new FlexCelImgExport(xlsFile, true);
			imageExport.SaveAsImage(stream, ImageExportType.Tiff, ImageColorDepth.TrueColor);
		}

		[ExpectNoExceptions]
		public void TestFlexCelDoesNotThrowInvalidCastExceptionWhenRecalculating()
		{
			var xlsFile = new XlsFile(resourceRetriever.Value.GetResourceStream("DocCrashingWhenRecalculating_1_FlexCelv6-21-6-0.xls"), true);
			xlsFile.Recalc();

			xlsFile = new XlsFile(resourceRetriever.Value.GetResourceStream("DocCrashingWhenRecalculating_2_FlexCelv6-21-6-0.xlsx"), true);
			xlsFile.Recalc();
		}

		[ExpectNoExceptions]
		public void TestFlexCelMergeFilesIntoOneXLSFillShadeColors()
		{
			var outFile = new XlsFile();
			outFile.NewFile(1);
			using var fileStream = resourceRetriever.Value.GetResourceStream("TestFlexCelMergeFilesIntoOneXLSFillShadeColors.xlsx");
			var bytes = new byte[fileStream.Length];
			fileStream.Read(bytes, 0, bytes.Length);
			using var inputStream = new MemoryStream(bytes);
			var inputFile = new XlsFile();
			inputStream.Position = 0;
			inputFile.Open(inputStream);
			outFile.InsertAndCopySheets(1, 1, 1, inputFile);
		}

		[ExpectNoExceptions]
		public void TestCellWith32KCharacters()
		{
			var xlsFile = new XlsFile(resourceRetriever.Value.GetResourceStream("DocCrashingWhenExportingCellWithOver32kCharacters_FlexCelv6-23-0-0.xls"), true);
			using var stream = new MemoryStream();
			using var imageExport = new FlexCelImgExport(xlsFile, true);
			imageExport.SaveAsImage(stream, ImageExportType.Tiff, ImageColorDepth.TrueColor);
		}

		public void TestRightToLeftMarkDoesNotShowUpInPdf()
		{
			var rightToLeftMark = (char)0x200F;
			var excelFile = new XlsFile(1, true);
			excelFile.SetCellValue(1, 1, "ENG: " + rightToLeftMark + "سجل");
			excelFile.SetCellValue(2, 1, "ENG: سجل");
			excelFile.AutofitCol(1, false, 1);

			using var stream = new MemoryStream();
			using var pdfExport = new FlexCelPdfExportSafe(excelFile);
			pdfExport.UseExcelProperties = false;
			pdfExport.FontEmbed = FlexCel.Pdf.TFontEmbed.None;
			pdfExport.BeginExport(stream);
			pdfExport.ExportSheet();
			pdfExport.EndExport();

			using var pdfDocument = new PdfDocument(stream);
			Assert("PDF should not contain the right-to-left mark.", !pdfDocument.GetAllText().Contains(rightToLeftMark));
		}

		[ExpectNoExceptions]
		public void TestFlexCelShouldNotThrowFormatExceptionWhenCalculatingInvalidFormulas()
		{
			var xls = new XlsFile();
			xls.NewFile(1, TExcelFileFormat.v2016);
			xls.ActiveSheet = 1;
			var content = "={1, \"A\",3 }";
			xls.RecalcRelativeFormula(1, 1, 1, content);
		}

		public void TestFlexCelShouldNotExportTabSymbolsToPdf()
		{
			var excelFile = new XlsFile(1, true);
			excelFile.SetCellValue(1, 1, "A\tBC");
			excelFile.SetCellValue(2, 1, new TFormula("=CONCATENATE(\"AB\", CHAR(9), \"C\")"));

			using var stream = new MemoryStream();
			using var pdfExport = new FlexCelPdfExportSafe(excelFile);
			pdfExport.BeginExport(stream);
			pdfExport.ExportSheet();
			pdfExport.EndExport();

			//var pdfData = stream.CopyToByteArray();
			// Uncomment the following line if this test is failing, and you want to see what the actual output is.
			//File.WriteAllBytes(@"c:\tmp\TestFlexCelShouldNotExportTabSymbolsToPDF.pdf", pdfData);

			using var pdfDocument = new PdfDocument(stream);
			var results = pdfDocument.Search("ABC").ToList();
			AssertEquals(2, results.Count);
			Assert("PDF should not contain tabs", !pdfDocument.GetAllText().Contains("\t"));
			Assert("PDF should not contain NonCharacters", !pdfDocument.GetAllText().Contains("\ufffe"));
		}

		public void TestIndexedColorsAreCopiedCorrectly()
		{
			var darkGreyColorIndex = 56;
			var sourceXlsFile = new XlsFile(1, true);

			var sourceFormat = TFlxFormat.CreateStandard2007();
			sourceFormat.FillPattern.Pattern = TFlxPatternStyle.Solid;
			sourceFormat.FillPattern.FgColor = TExcelColor.FromIndex(darkGreyColorIndex);

			var sourceFormatIndex = sourceXlsFile.AddFormat(sourceFormat);
			sourceXlsFile.SetCellFormat(1, 1, sourceFormatIndex);

			var sourceColor = sourceXlsFile.GetColorPalette(darkGreyColorIndex).ToArgb();
			AssertEquals("Pre-condition", 0xFF_33_33_33.ToString("X4"), sourceColor.ToString("X4"));

			var sourceRange = new TXlsCellRange(1, 1, 1, sourceXlsFile.ColCount);

			using var stream = typeof(StmTemplateBase).Assembly.GetManifestResourceStream("Enterprise.DocumentEngine.DocBuilder.SectionEditing.BaseDocBuilderTemplate.xls");
			var targetXlsFile = new XlsFile(1, true);
			targetXlsFile.Open(stream);
			targetXlsFile.InsertAndCopyRange(sourceRange, 1, 1, 1, TFlxInsertMode.ShiftRowDown, TRangeCopyMode.All, sourceXlsFile, 1);

			var targetFormatIndex = targetXlsFile.GetCellFormat(1, 1);
			var targetFormat = targetXlsFile.GetFormat(targetFormatIndex);

			var targetColor = targetXlsFile.GetColorPalette(darkGreyColorIndex).ToArgb();
			AssertEquals(0xFF_33_33_33.ToString("X4"), targetColor.ToString("X4"));
			AssertEquals(darkGreyColorIndex, targetFormat.FillPattern.FgColor.Index);
		}

		[ExpectNoExceptions]
		public void TestFlexCelDoesNotCrashWhenExportingImage()
		{
			var excelFile = new XlsFile();
			excelFile.Open(resourceRetriever.Value.GetResourceStream("DocCrashingWhenExportingToPDF_FlexCelv7-0-0-0.xlsx"));
			using var pdfExport = new FlexCelPdfExportSafe(excelFile);
			using var stream = new MemoryStream();
			pdfExport.Export(stream);
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		List<string> GetDisposableTypeNames(List<Assembly> assemblies)
		{
			var typeNames = new List<string>();
			foreach (var assembly in assemblies)
			{
				foreach (var type in assembly.GetTypes())
				{
					if (typeof(IDisposable).IsAssignableFrom(type) && type.IsPublic)
					{
						typeNames.Add(type.FullName);
					}
				}
			}
			return typeNames;
		}

		static string GetError(string fileName, Verify verify)
		{
			string result = null;
			try
			{
				result = verify.Invoke(fileName);
			}
			catch (Exception ex)
			{
				Fail("An exception occurred while verifying '" + fileName + "':\r\n\r\n" + ex);
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:Do not use BaseSourcePath")]
		static void AssertFormulas(DirectoryInfo di)
		{
			var children = di.GetDirectories();

			foreach (var dic in children)
			{
				AssertFormulas(dic);
			}

			var fi = di.GetFiles("*.xls");
			foreach (var fiTemp in fi)
			{
				if (fiTemp.FullName == Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\ExcelTemplates\Reports\EDI\Pricelist\PriceListClientTemplate.xlsx"))
				{
					continue;// FlexCel's Trim Function issue would cause a failure to this template. Temporarily exclude this template. Should be undone after FlexCel upgrates in WI00585954.
				}

				var err = GetError(fiTemp.FullName, VerifySupportedFormulas);
				AssertNull(fiTemp.FullName + " has errors.", err);

				err = GetError(fiTemp.FullName, VerifyInternalRecalculate);
				if (err != null && err.IndexOf("TODAY", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					err = null;  // Today() returns different values each day, so it is OK that it returns a different value than Excel.
				}
				AssertNull(fiTemp.FullName + " has errors.", err);
			}
		}

		static string VerifyInternalRecalculate(string fileName)
		{
			var xls1 = new XlsFile();
			xls1.Open(fileName);
			var xls2 = new XlsFile();
			xls2.Open(fileName);
			var diffCount = 0;
			xls1.Recalc();
			var result = new StringBuilder();

			for (var sheet = 1; sheet <= xls1.SheetCount; sheet++)
			{
				xls1.ActiveSheet = sheet;
				xls2.ActiveSheet = sheet;
				var aColCount = xls1.ColCount;
				for (var r = 1; r <= xls1.RowCount; r++)
				{
					for (var c = 1; c <= aColCount; c++)
					{
						if (xls1.GetCellValue(r, c) is TFormula f)
						{
							var ad = new TCellAddress(r, c);
							var f2 = (TFormula)xls2.GetCellValue(r, c);
							f.Result ??= "";

							f2.Result ??= "";

							double eps = 0;
							if (f.Result is double d && f2.Result is double d2)
							{
								if (d2 == 0)
								{
									if (Math.Abs(d) < double.Epsilon)
									{
										eps = 0;
									}
									else
									{
										eps = double.NaN;
									}
								}
								else
								{
									eps = d / d2;
								}
								if (Math.Abs(eps - 1) < 0.001)
								{
									f.Result = d2;
								}
							}

							if (!f.Result.Equals(f2.Result))
							{
								result.Append("\nFile: " + xls1.ActiveFileName + "   --- Sheet:" +
									xls1.SheetName + " --- Cell:" + ad.CellRef + " --- Calculated: " + f.Result.ToString() + "    Excel: " + f2.Result.ToString() + "  dif: " + eps.ToString() + "   formula: " + f.Text
									);
								diffCount++;
							}
						}
					}
				}
			}
			if (diffCount > 0)
			{
				result.Insert(0, "Found " + diffCount + " formulas different from Excel:");
				return result.ToString();
			}
			return null;
		}

		static string VerifySupportedFormulas(string fileName)
		{
			var xls = new XlsFile();
			xls.Open(fileName);
			var usl = xls.RecalcAndVerify();
			if (usl.Count == 0)
			{
				return null;
			}

			var sb = new StringBuilder(usl.Count + " Issues Found:");
			for (var i = 0; i < usl.Count; i++)
			{
				sb.Append("\n File: " + xls.ActiveFileName + "   Cell: " + usl[i].Cell.CellRef + ": " + usl[i].ErrorType);
				if (usl[i].FunctionName != null)
				{
					sb.Append(" Function:" + usl[i].FunctionName);
				}
			}
			return sb.ToString();
		}

		delegate string Verify(string fileName);
	}

	static class EmbeddedResourceRetrieverExtensions
	{
		public static Stream GetResourceStream(this EmbeddedResourceRetriever retriever, string resourceName)
		{
			return retriever.GetStream("Enterprise.DocumentEngine.Test.FlexCelInterface.Testing." + resourceName);
		}
	}
}

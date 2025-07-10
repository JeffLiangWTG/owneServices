using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.PdfiumWrapper;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentScanning.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using FlexCel.XlsAdapter;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.DocumentDelivery.Tests
{
#if !WINZOR
	using Enterprise.RemoteDesktopServices;
	using Enterprise.ZArchitecture.GUI;
#endif

	sealed class DocumentDeliveryPresenterTest : TestCaseWithFactory
	{
		public void TestSaveAsFunctionalityWhenDeliveryInstructionsOfficialRecipientIsNull()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test Save Document",
				@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[#DocumentHeader]
{B}-[<Z0_Description>]
{A}-[#EndOfReport]", ".DummyBODocSupportable");

			var dummy = Factory.New<DummyBODocSupportable>();
			dummy.Z0_Description = "Test Save Documents";
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;
			var pivot = documentCommand.Documents.AddNew();
			pivot.SI_SU = documentCommand.PK;
			pivot.SI_SO = template.PK;

			using (var tempFile = TempFile.NewWithExtension("xls"))
			using (var documentPack = new DocumentPack(documentCommand, dummy, null, null))
			using (var printTask = new PrintTask(documentCommand))
			{
				printTask.Add(documentPack);
				var deliveryInstructions = new DeliveryInstructionsWithEmptyRecipient(documentPack);

				var view = new Mock<IDocumentDeliveryView>();
				var presenter = new DocumentDeliveryPresenter(view.Object, printTask, deliveryInstructions);

				view.Setup(m => m.GetSaveAsFileName()).Returns(new SaveAsFileInfo(File.Open(tempFile.Filename, FileMode.Create, FileAccess.ReadWrite), tempFile.Filename, SaveAsFileType.Xls));
				view.Raise(m => m.SaveAsButtonClicked += null, presenter, EventArgs.Empty);

				using (var expectedResult = new ExcelInterface())
				{
					expectedResult.LoadExcelFile(tempFile.Filename);
					expectedResult.ActiveWorksheet = 0;
					var cell = expectedResult.WorkSheets[0].GetCell(0, 1);
					AssertEquals("Test Save Documents", cell.Value.ToString());
				}
				view.VerifyAll();
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSaveAsFunctionalityShouldUseDeliveryInstructionsLanguage()
		{
			var template = Factory.New<StmTemplateBase>();
			template.SO_Name = "Test";
			template.SO_Template = new ExcelTemplateForUnitTesting("NumberFilterBeUsedInTemplate.xlsx", TestFilesSubFolder.ReportTestFiles).GetAsByteArray();
			template.SO_DataContext = nameof(Core.Constants.DataContext.UnitTest);

			var reportCommand = Factory.New<ReportCommand>();
			reportCommand.SU_MenuName = "Dummy Report";
			var document = reportCommand.Documents.AddNew();
			document.SI_SU = reportCommand.PK;
			document.SI_SO = template.PK;

			using (var tempFile = TempFile.NewWithExtension("PDF"))
			using (var printSet = new ReportPrintSet(reportCommand))
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.English))
			{
				AssertEquals("Current language should be English", Core.SharedConstants.Languages.English, Res.CurrentLanguage);

				var fileName = tempFile.Filename;

				var view = new Mock<IDocumentDeliveryView>();
				var deliveryInstructions = new DeliveryInstructions(printSet[0]);
				deliveryInstructions.Language = Core.SharedConstants.Languages.ChineseSimplified;
				var presenter = new DocumentDeliveryPresenterForDeliveryLanguage(view.Object, printSet, deliveryInstructions);

				view.Setup(m => m.GetSaveAsFileName()).Returns(new SaveAsFileInfo(File.OpenWrite(fileName), fileName, SaveAsFileType.Pdf));
				view.Raise(m => m.SaveAsButtonClicked += null, presenter, EventArgs.Empty);
				AssertEquals("TemporaryLanguage should be ChineseSimplified", Core.SharedConstants.Languages.ChineseSimplified, presenter.TemporaryLanguage);
				view.VerifyAll();
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSaveAsFunctionalityShouldUseWatermarkWhenExportingtoPDF()
		{
			AssertSaveAsFunctionalityShouldUseWatermarkWhenExporting("PDF", SaveAsFileType.Pdf);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSaveAsFunctionalityShouldUseWatermarkWhenExportingtoTif()
		{
			AssertSaveAsFunctionalityShouldUseWatermarkWhenExporting("Tif", SaveAsFileType.Tif);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSaveAsFunctionalityShouldUseInstructionsReport()
		{
			var template = Factory.New<StmTemplateBase>();
			template.SO_Name = "Test";
			template.SO_Template = new ExcelTemplateForUnitTesting("NumberFilterBeUsedInTemplate.xlsx", TestFilesSubFolder.ReportTestFiles).GetAsByteArray();
			template.SO_DataContext = nameof(Core.Constants.DataContext.UnitTest);

			var reportCommand = Factory.New<ReportCommand>();
			reportCommand.SU_MenuName = "Dummy Report";
			var document = reportCommand.Documents.AddNew();
			document.SI_SU = reportCommand.PK;
			document.SI_SO = template.PK;

			using (var tempFile = TempFile.NewWithExtension("PDF"))
			using (var printSet = new ReportPrintSet(reportCommand))
			{
				var report = printSet[0][0] as Report;
				report.PrepareForRender();
				AssertEquals(5m, report.FilterCollection.OfType<NumberField>().First(n => n.DisplayName == "Some Number").Value);

				var fileName = tempFile.Filename;
				SaveReportAsFile(printSet, SaveAsFileType.Pdf, fileName);
				AssertEquals(5m, report.FilterCollection.OfType<NumberField>().First(n => n.DisplayName == "Some Number").Value);
			}
		}

		public void TestSaveReportAsCsvWithColumnHeadings()
		{
			CreateDummyBusinessObject(1, "One");
			CreateDummyBusinessObject(2, "Two");
			CreateDummyBusinessObject(3, "Three");
			Factory.Save();

			var expectedFileContent = @"""Number"",""Text""
""1"",""One""
""2"",""Two""
""3"",""Three""";
			var expectedMessage = "{0} has been successfully saved.";
			AssertSaveReportAsCsv(TemplateStringExportableToCsv, SaveAsFileType.CsvWithColumnHeadings, expectedFileContent, expectedMessage);
		}

		public void TestSaveReportAsCsv()
		{
			CreateDummyBusinessObject(1, "One");
			CreateDummyBusinessObject(2, "Two");
			CreateDummyBusinessObject(3, "Three");
			Factory.Save();

			var expectedFileContent = @"""1"",""One""
""2"",""Two""
""3"",""Three""";
			var expectedMessage = "{0} has been successfully saved.";
			AssertSaveReportAsCsv(TemplateStringExportableToCsv, SaveAsFileType.Csv, expectedFileContent, expectedMessage);
		}

		public void TestSaveReportAsCsv_ReportHasNoData()
		{
			var expectedErrorMessage = "Report 'Dummy Report' does not contain any data. The generated file should be discarded.";
			AssertSaveReportAsCsv(TemplateStringExportableToCsv, SaveAsFileType.Csv, null, expectedErrorMessage);
		}

		public void TestSaveReportAsCsv_ReportHasData_CannotExportToCSV()
		{
			CreateDummyBusinessObject(1, "One");
			CreateDummyBusinessObject(2, "Two");
			CreateDummyBusinessObject(3, "Three");
			Factory.Save();

			var expectedErrorMessage = @"Report 'Dummy Report' does not contain any data that can be exported to 'CSV' format. This report contains data that would be suitable for exporting to XLS, PDF or TIFF.
This is because 'CSV' requires a consistent format throughout and not all report templates have a suitable structure.";
			AssertSaveReportAsCsv(TemplateStringNonExportableToCsv, SaveAsFileType.Csv, null, expectedErrorMessage);
		}

		public void TestSaveReportAsXlsOrXlsxWhenItIsRemote()
		{
			AssertEquals(nameof(SaveAsFileType.Xls), SaveReportAndReturnFileFormatWhenOpened(SaveAsFileType.Xls, true));
			AssertEquals(nameof(SaveAsFileType.Xlsx), SaveReportAndReturnFileFormatWhenOpened(SaveAsFileType.Xlsx, true));
		}

		public void TestSaveReport_HasDataCannotExportToCSV_SaveAsCsvFirstAndThenAsXls()
		{
			CreateDummyBusinessObject(1, "One");
			CreateDummyBusinessObject(2, "Two");
			CreateDummyBusinessObject(3, "Three");
			Factory.Save();

			var expectedErrorMessage = @"Report 'Dummy Report' does not contain any data that can be exported to 'CSV' format. This report contains data that would be suitable for exporting to XLS, PDF or TIFF.
This is because 'CSV' requires a consistent format throughout and not all report templates have a suitable structure.";
			var expectedMessage = "{0} has been successfully saved.";
			var expectedFileContent = @"{B}-[Page 1 of 1]
{B}-[1]   {C}-[One]
{B}-[2]   {C}-[Two]
{B}-[3]   {C}-[Three]";

			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test", TemplateStringNonExportableToCsv);

			var reportCommand = Factory.New<ReportCommand>();
			reportCommand.SU_MenuName = "Dummy Report";
			var document = reportCommand.Documents.AddNew();
			document.SI_SU = reportCommand.PK;
			document.SI_SO = template.PK;

			using (Report.TemporarilyUseMainConnection())
			using (var printSet = new ReportPrintSet(reportCommand))
			{
				using (var csvTempFile = TempFile.NewWithExtension("CSV"))
				{
					SaveReportAsFile(printSet, SaveAsFileType.Csv, csvTempFile.Filename);
					using (var csvStream = File.OpenRead(csvTempFile.Filename))
					{
						var csvBytes = csvStream.CopyToByteArray();
						AssertNull(csvBytes);
						AssertEquals(expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
				using (var xlsTempFile = TempFile.NewWithExtension("xls"))
				{
					SaveReportAsFile(printSet, SaveAsFileType.Xls, xlsTempFile.Filename);
					using (var xlsStream = File.OpenRead(xlsTempFile.Filename))
					{
						var xlsBytes = xlsStream.CopyToByteArray();
						AssertNotNull(xlsBytes);

						using (var excelInterface = new ExcelInterface())
						{
							excelInterface.LoadExcelFile(xlsBytes);
							AssertMultilineASCIIEquals("Report should be saved in a xls file selected by the user.", expectedFileContent, excelInterface.WorkSheets[0].ToString());
						}
						AssertEquals(string.Format(expectedMessage, xlsTempFile.Filename), UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
		}

		public void TestSaveReport_HasDataExportToCSV_SaveAsCsvFirstAndThenAsXls()
		{
			CreateDummyBusinessObject(1, "One");
			CreateDummyBusinessObject(2, "Two");
			CreateDummyBusinessObject(3, "Three");
			Factory.Save();

			var expectedMessage = "{0} has been successfully saved.";
			var expectedExcelFileContent = @"{B}-[Page 1 of 1]
{B}-[1]   {C}-[One]
{B}-[2]   {C}-[Two]
{B}-[3]   {C}-[Three]";
			var expectedCsvContent = @"""1"",""One""
""2"",""Two""
""3"",""Three""";

			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test", TemplateStringExportableToCsv);

			var reportCommand = Factory.New<ReportCommand>();
			reportCommand.SU_MenuName = "Dummy Report";
			var document = reportCommand.Documents.AddNew();
			document.SI_SU = reportCommand.PK;
			document.SI_SO = template.PK;

			using (Report.TemporarilyUseMainConnection())
			using (var printSet = new ReportPrintSet(reportCommand))
			{
				var view = new Mock<IDocumentDeliveryView>();
				var presenter = new DocumentDeliveryPresenter(view.Object, printSet, new DeliveryInstructions(printSet[0]).Clone() as DeliveryInstructions);

				using (var csvTempFile = TempFile.NewWithExtension("CSV"))
				{
					view.Setup(m => m.GetSaveAsFileName()).Returns(new SaveAsFileInfo(File.OpenWrite(csvTempFile.Filename), csvTempFile.Filename, SaveAsFileType.Csv));
					view.Raise(m => m.SaveAsButtonClicked += null, presenter, EventArgs.Empty);

					using (var csvStream = File.OpenRead(csvTempFile.Filename))
					{
						var csvBytes = csvStream.CopyToByteArray();
						AssertMultilineASCIIEquals("Report should be saved in a CSV file selected by the user.", expectedCsvContent, Encoding.UTF8.GetString(csvBytes));
						AssertEquals(string.Format(expectedMessage, csvTempFile.Filename), UnitTestUserNotification.Instance.LastMessage.Text);
					}
					view.VerifyAll();
				}

				using (var xlsTempFile = TempFile.NewWithExtension("xls"))
				{
					view.Setup(m => m.GetSaveAsFileName()).Returns(new SaveAsFileInfo(File.OpenWrite(xlsTempFile.Filename), xlsTempFile.Filename, SaveAsFileType.Xls));
					view.Raise(m => m.SaveAsButtonClicked += null, presenter, EventArgs.Empty);

					using (var xlsStream = File.OpenRead(xlsTempFile.Filename))
					{
						var xlsBytes = xlsStream.CopyToByteArray();
						AssertNotNull(xlsBytes);

						using (var excelInterface = new ExcelInterface())
						{
							excelInterface.LoadExcelFile(xlsBytes);
							AssertMultilineASCIIEquals("Report should be saved in a xls file selected by the user.", expectedExcelFileContent, excelInterface.WorkSheets[0].ToString());
						}
						AssertEquals(string.Format(expectedMessage, xlsTempFile.Filename), UnitTestUserNotification.Instance.LastMessage.Text);
					}
					view.VerifyAll();
				}
			}
		}

		public void TestSaveAsButtonFormulaTooLongExceptionHandled()
		{
			using (DocumentsDataRegistry.Instance.ExcelDefaultRenderingFormat.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ExcelFileFormatOptionList.Codes.XLSX))
			{
				var businessObject = Factory.New<DummyBusinessObject>();
				businessObject.Z0_Number = 1;
				for (int i = 0; i < 11; i++)
				{
					DummyChildBusinessObject child = businessObject.Collection.AddNew();
					child.Z0_Number = i;
				}
				Factory.Save();

				var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
	@"{A}-[#Config]
{A}-[Data:ReportData=select Number, GroupID % 666 as GroupNumber from (Select DummyBizo1.Z0_Number as Number, ROW_NUMBER() OVER (ORDER BY DummyBizo1.Z0_Number DESC) AS GroupID from dbo.DummyBizo as DummyBizo1 cross join dbo.DummyBizo as DummyBizo2 cross join dbo.DummyBizo as DummyBizo3) as dummytable]
{A}-[#SectionBody:Data=ReportData]
{B}-[Blah1]   {C}-[<ReportData.Number>]
{A}-[#GroupBy:ReportData.GroupNumber:GroupTitle]
{B}-[Group Title <ReportData.GroupNumber>]
{A}-[#GroupBy:ReportData.GroupNumber]
{B}-[Sub Total]   {C}-[<Total ReportData.Number>]
{A}-[#SectionFooter]
{B}-[GRAND TOTAL]   {C}-[<Total ReportData.Number>]
{A}-[#EndOfReport]");

				var reportCommand = Factory.New<ReportCommand>();
				reportCommand.SU_MenuName = "Dummy Report";
				var document = reportCommand.Documents.AddNew();
				document.SI_SU = reportCommand.PK;
				document.SI_SO = template.PK;

				using (var tempFile = TempFile.NewWithExtension("Xls"))
				using (var printSet = new ReportPrintSet(reportCommand))
				{
					var fileName = tempFile.Filename;
					AssertNoExceptionThrown(() => SaveReportAsFile(printSet, SaveAsFileType.Xls, fileName));
				}
			}
		}

		public void TestSaveAsDocument_ApplyLanguage()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test Save Document",
				@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[#DocumentHeader]
{B}-[<DateTimeAsString('01/01/2022', 'dd-MMM-yy')>]
{A}-[#EndOfReport]", ".DummyBODocSupportable");
			template.IsDocBuilderStyleForTest = true;

			var dummy = Factory.New<DummyBODocSupportable>();

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;
			var pivot = documentCommand.Documents.AddNew();
			pivot.SI_SU = documentCommand.PK;
			pivot.SI_SO = template.PK;

			using (var tempFile = TempFile.NewWithExtension("xls"))
			using (var documentPack = new DocumentPack(documentCommand, dummy, null, null))
			using (var printTask = new PrintTask(documentCommand))
			{
				printTask.Add(documentPack);
				var deliveryInstructions = new DeliveryInstructions(documentPack);
				deliveryInstructions.Language = Core.SharedConstants.Languages.French;

				var view = new Mock<IDocumentDeliveryView>();
				var presenter = new DocumentDeliveryPresenter(view.Object, printTask, deliveryInstructions);

				view.Setup(m => m.GetSaveAsFileName()).Returns(new SaveAsFileInfo(File.Open(tempFile.Filename, FileMode.Create, FileAccess.ReadWrite), tempFile.Filename, SaveAsFileType.Xls));
				view.Raise(m => m.SaveAsButtonClicked += null, presenter, EventArgs.Empty);

				using (var expectedResult = new ExcelInterface())
				{
					expectedResult.LoadExcelFile(tempFile.Filename);
					expectedResult.ActiveWorksheet = 0;
					var cell = expectedResult.WorkSheets[0].GetCell(0, 1);
					AssertEquals("01-janv.-22", cell.Value.ToString());
				}
				view.VerifyAll();
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSaveAsForSingleDocument()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test Save Document",
				@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[#DocumentHeader]
{B}-[<Z0_Description>]
{A}-[#EndOfReport]", ".DummyBODocSupportable");

			var dummy = Factory.New<DummyBODocSupportable>();
			dummy.Z0_Description = "Test Save Documents";
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;
			var pivot = documentCommand.Documents.AddNew();
			pivot.SI_SU = documentCommand.PK;
			pivot.SI_SO = template.PK;

			using (var tempFile = TempFile.NewWithExtension("xls"))
			using (var documentPack = new DocumentPack(documentCommand, dummy, null, null))
			using (var printTask = new PrintTask(documentCommand))
			{
				printTask.Add(documentPack);
				var deliveryInstructions = new DeliveryInstructions(documentPack);

				var view = new Mock<IDocumentDeliveryView>();
				var presenter = new DocumentDeliveryPresenter(view.Object, printTask, deliveryInstructions);

				view.Setup(m => m.GetSaveAsFileName()).Returns(new SaveAsFileInfo(File.Open(tempFile.Filename, FileMode.Create, FileAccess.ReadWrite), tempFile.Filename, SaveAsFileType.Xls));
				view.Raise(m => m.SaveAsButtonClicked += null, presenter, EventArgs.Empty);

				using (var expectedResult = new ExcelInterface())
				{
					expectedResult.LoadExcelFile(tempFile.Filename);
					expectedResult.ActiveWorksheet = 0;
					var cell = expectedResult.WorkSheets[0].GetCell(0, 1);
					AssertEquals("Test Save Documents", cell.Value.ToString());
				}
				view.VerifyAll();
			}

			using (var tempFile = TempFile.NewWithExtension("xlsx"))
			using (var documentPack = new DocumentPack(documentCommand, dummy, null, null))
			using (var printTask = new PrintTask(documentCommand))
			{
				printTask.Add(documentPack);
				var deliveryInstructions = new DeliveryInstructions(documentPack);

				var view = new Mock<IDocumentDeliveryView>();
				var presenter = new DocumentDeliveryPresenter(view.Object, printTask, deliveryInstructions);

				view.Setup(m => m.GetSaveAsFileName()).Returns(new SaveAsFileInfo(File.Open(tempFile.Filename, FileMode.Create, FileAccess.ReadWrite), tempFile.Filename, SaveAsFileType.Xlsx));
				view.Raise(m => m.SaveAsButtonClicked += null, presenter, EventArgs.Empty);

				using (var expectedResult = new ExcelInterface())
				{
					expectedResult.LoadExcelFile(tempFile.Filename);
					expectedResult.ActiveWorksheet = 0;
					var cell = expectedResult.WorkSheets[0].GetCell(0, 1);
					AssertEquals("Test Save Documents", cell.Value.ToString());
				}
				view.VerifyAll();
			}

			using (var tempFile = TempFile.NewWithExtension("pdf"))
			using (var documentPack = new DocumentPack(documentCommand, dummy, null, null))
			using (var printTask = new PrintTask(documentCommand))
			{
				printTask.Add(documentPack);
				var deliveryInstructions = new DeliveryInstructions(documentPack);

				var view = new Mock<IDocumentDeliveryView>();
				var presenter = new DocumentDeliveryPresenter(view.Object, printTask, deliveryInstructions);

				view.Setup(m => m.GetSaveAsFileName()).Returns(new SaveAsFileInfo(File.Open(tempFile.Filename, FileMode.Create, FileAccess.ReadWrite), tempFile.Filename, SaveAsFileType.Pdf));
				view.Raise(m => m.SaveAsButtonClicked += null, presenter, EventArgs.Empty);

				using (var pdfDocument = PdfDocument.LoadFile(tempFile.Filename))
				{
					AssertEquals(1, pdfDocument.PageCount);
					var allText = pdfDocument.GetAllText();
					AssertEquals("Test Save Documents\r\nTraining / Test \r\n Non Commercial Use only\0", allText);
				}
				view.VerifyAll();
			}

			using (var tempFile = TempFile.NewWithExtension("tif"))
			using (var documentPack = new DocumentPack(documentCommand, dummy, null, null))
			using (var printTask = new PrintTask(documentCommand))
			{
				printTask.Add(documentPack);
				var deliveryInstructions = new DeliveryInstructions(documentPack);

				var view = new Mock<IDocumentDeliveryView>();
				var presenter = new DocumentDeliveryPresenter(view.Object, printTask, deliveryInstructions);

				view.Setup(m => m.GetSaveAsFileName()).Returns(new SaveAsFileInfo(File.Open(tempFile.Filename, FileMode.Create, FileAccess.ReadWrite), tempFile.Filename, SaveAsFileType.Tif));
				view.Raise(m => m.SaveAsButtonClicked += null, presenter, EventArgs.Empty);

				var documentUtilities = ObjectFactory.Get<IDocumentUtilities>();
				var expectedContent = documentUtilities.GetFileAsBytes(TestCaseWithFactory.BaseSourcePath + @"Enterprise\Product\Documents\DocumentEngine.GUI\DocumentEngine.GUI\Testing\DocumentSaveAsTif.tif");
				var actualContent = documentUtilities.GetFileAsBytes(tempFile.Filename);
				AssertEquals("Output file should be the same.", expectedContent, actualContent);
				view.VerifyAll();
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSaveAsForMultiDocuments()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test Save Document",
				@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[#DocumentHeader]
{B}-[<Z0_Description>]
{A}-[#EndOfReport]", ".DummyBODocSupportable");

			var dummy = Factory.New<DummyBODocSupportable>();
			dummy.Z0_Description = "Test Save Documents";
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;
			var pivot1 = documentCommand.Documents.AddNew();
			pivot1.SI_SU = documentCommand.PK;
			pivot1.SI_SO = template.PK;

			var pivot2 = documentCommand.Documents.AddNew();
			pivot2.SI_SU = documentCommand.PK;
			pivot2.SI_SO = template.PK;

			using (var tempFile = TempFile.NewWithExtension("xls"))
			using (var documentPack = new DocumentPack(documentCommand, dummy, null, null))
			using (var printTask = new PrintTask(documentCommand))
			{
				printTask.Add(documentPack);
				var deliveryInstructions = new DeliveryInstructions(documentPack);

				var view = new Mock<IDocumentDeliveryView>();
				var presenter = new DocumentDeliveryPresenter(view.Object, printTask, deliveryInstructions);

				view.Setup(m => m.GetSaveAsFileName()).Returns(new SaveAsFileInfo(File.Open(tempFile.Filename, FileMode.Create, FileAccess.ReadWrite), tempFile.Filename, SaveAsFileType.Xls));
				view.Raise(m => m.SaveAsButtonClicked += null, presenter, EventArgs.Empty);

				using (var expectedResult = new ExcelInterface())
				{
					expectedResult.LoadExcelFile(tempFile.Filename);
					expectedResult.ActiveWorksheet = 0;
					var cell1 = expectedResult.WorkSheets[0].GetCell(0, 1);
					AssertEquals("Test Save Documents", cell1.Value);
					var cell2 = expectedResult.WorkSheets[1].GetCell(0, 1);
					AssertEquals("Test Save Documents", cell2.Value);
				}
				view.VerifyAll();
			}

			using (var tempFile = TempFile.NewWithExtension("xlsx"))
			using (var documentPack = new DocumentPack(documentCommand, dummy, null, null))
			using (var printTask = new PrintTask(documentCommand))
			{
				printTask.Add(documentPack);
				var deliveryInstructions = new DeliveryInstructions(documentPack);

				var view = new Mock<IDocumentDeliveryView>();
				var presenter = new DocumentDeliveryPresenter(view.Object, printTask, deliveryInstructions);

				view.Setup(m => m.GetSaveAsFileName()).Returns(new SaveAsFileInfo(File.Open(tempFile.Filename, FileMode.Create, FileAccess.ReadWrite), tempFile.Filename, SaveAsFileType.Xlsx));
				view.Raise(m => m.SaveAsButtonClicked += null, presenter, EventArgs.Empty);

				using (var expectedResult = new ExcelInterface())
				{
					expectedResult.LoadExcelFile(tempFile.Filename);
					expectedResult.ActiveWorksheet = 0;
					var cell1 = expectedResult.WorkSheets[0].GetCell(0, 1);
					AssertEquals("Test Save Documents", cell1.Value);
					var cell2 = expectedResult.WorkSheets[1].GetCell(0, 1);
					AssertEquals("Test Save Documents", cell2.Value);
				}
				view.VerifyAll();
			}

			using (var tempFile = TempFile.NewWithExtension("pdf"))
			using (var documentPack = new DocumentPack(documentCommand, dummy, null, null))
			using (var printTask = new PrintTask(documentCommand))
			{
				printTask.Add(documentPack);
				var deliveryInstructions = new DeliveryInstructions(documentPack);

				var view = new Mock<IDocumentDeliveryView>();
				var presenter = new DocumentDeliveryPresenter(view.Object, printTask, deliveryInstructions);

				view.Setup(m => m.GetSaveAsFileName()).Returns(new SaveAsFileInfo(File.Open(tempFile.Filename, FileMode.Create, FileAccess.ReadWrite), tempFile.Filename, SaveAsFileType.Pdf));
				view.Raise(m => m.SaveAsButtonClicked += null, presenter, EventArgs.Empty);

				using (var pdfDocument = PdfDocument.LoadFile(tempFile.Filename))
				{
					AssertEquals(2, pdfDocument.PageCount);
					var allText = pdfDocument.GetAllText();
					AssertEquals("Test Save Documents\r\nTraining / Test \r\n Non Commercial Use only\0Test Save Documents\r\nTraining / Test \r\n Non Commercial Use only\0", allText);
				}
				view.VerifyAll();
			}

			using (var tempFile = TempFile.NewWithExtension("tif"))
			using (var documentPack = new DocumentPack(documentCommand, dummy, null, null))
			using (var printTask = new PrintTask(documentCommand))
			{
				printTask.Add(documentPack);
				var deliveryInstructions = new DeliveryInstructions(documentPack);

				var view = new Mock<IDocumentDeliveryView>();
				var presenter = new DocumentDeliveryPresenter(view.Object, printTask, deliveryInstructions);

				view.Setup(m => m.GetSaveAsFileName()).Returns(new SaveAsFileInfo(File.Open(tempFile.Filename, FileMode.Create, FileAccess.ReadWrite), tempFile.Filename, SaveAsFileType.Tif));
				view.Raise(m => m.SaveAsButtonClicked += null, presenter, EventArgs.Empty);

				var documentUtilities = ObjectFactory.Get<IDocumentUtilities>();
				var expectedContent = documentUtilities.GetFileAsBytes(TestCaseWithFactory.BaseSourcePath + @"Enterprise\Product\Documents\DocumentEngine.GUI\DocumentEngine.GUI\Testing\DocumentsSaveAsTif.tif");
				var actualContent = documentUtilities.GetFileAsBytes(tempFile.Filename);
				AssertEquals("Output file should be the same.", expectedContent, actualContent);
				view.VerifyAll();
			}
		}

		public void TestDoNotHideLanguageDropDownIfPrintSetContainsDocBuilderDocument()
		{
			var dummy = Factory.New<DummyBODocSupportable>();
			var template = StmTemplateBase.GetDocBuilderTemplate(Factory, DocBuilderTemplateType.System);
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;
			var document = documentCommand.Documents.AddNew();
			document.DocConfigs.AddNew();
			document.SI_SO = template.PK;
			document.SI_SU = documentCommand.PK;

			using (var printSet = new DocumentPrintSet(documentCommand, null))
			{
				var view = new Mock<IDocumentDeliveryView>();

				view.Setup(m => m.HideLanguageSelectionDropDown());
				var presenter = new DocumentDeliveryPresenter(view.Object, printSet, new DeliveryInstructions());

				Assert("The selection drop down box is not hidden.", true);
				view.Verify(m => m.HideLanguageSelectionDropDown(), Times.Never());
			}
		}

		public void TestHideLanguageDropDownIfPrintSetDoesNotContainDocBuilderDocument()
		{
			var dummy = Factory.New<DummyBODocSupportable>();
			var template = Factory.New<StmTemplateBase>();
			template.SO_DataContext = ".DummyBODocSupportable";
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;
			var document = documentCommand.Documents.AddNew();
			document.SI_SO = template.PK;
			document.SI_SU = documentCommand.PK;

			using (var printSet = new DocumentPrintSet(documentCommand, null))
			{
				var view = new Mock<IDocumentDeliveryView>();

				view.Setup(m => m.HideLanguageSelectionDropDown());
				var presenter = new DocumentDeliveryPresenter(view.Object, printSet, new DeliveryInstructions());

				Assert("The selection drop down box is hidden.", true);
				view.Verify(m => m.HideLanguageSelectionDropDown(), Times.Exactly(1));
			}
		}

		public void TestHideLanguageDropDown_WithStreamMode_WithoutSupportsLanguageSelectionOverride()
		{
			var command = Factory.New<DocumentCommand>();

			using (var printSet = new DocumentPrintSetWithStreaming(command, 1, Enumerable.Empty<DocumentPack>()))
			{
				var view = new Mock<IDocumentDeliveryView>();

				view.Setup(m => m.HideLanguageSelectionDropDown());
				var presenter = new DocumentDeliveryPresenter(view.Object, printSet, new DeliveryInstructions());

				Assert("The selection drop down box is hidden.", true);
				view.Verify(m => m.HideLanguageSelectionDropDown(), Times.Exactly(1));
			}
		}

		public void TestHideLanguageDropDown_WithStreamMode_WithSupportsLanguageSelectionOverride()
		{
			var command = Factory.New<DocumentCommand>();

			using (var printSet = new DocumentPrintSetWithStreaming(command, 1, Enumerable.Empty<DocumentPack>(), true))
			{
				var view = new Mock<IDocumentDeliveryView>();

				view.Setup(m => m.HideLanguageSelectionDropDown());
				var presenter = new DocumentDeliveryPresenter(view.Object, printSet, new DeliveryInstructions());

				Assert("The selection drop down box is not hidden.", true);
				view.Verify(m => m.HideLanguageSelectionDropDown(), Times.Never());
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[SnailTest]
		public void TestSaveExportOfNonReport()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
	@"{A}-[#Config]
{A}-[Data:ReportData=Select DummyBizo1.Z0_Number as Number from dbo.DummyBizo as DummyBizo1 cross join dbo.DummyBizo as DummyBizo2 cross join dbo.DummyBizo as DummyBizo3]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.Number>]
{A}-[#EndOfReport]");

			var excelTemplate = new ExcelTemplateForUnitTesting("InvalidFormatTemplate.xls", TestFilesSubFolder.ReportTestFiles);

			var tooManyRowsForExcel = 110;
			var businessObject = Factory.New<DummyBusinessObject>();
			businessObject.Z0_Number = 1;

			for (int i = 0; i < tooManyRowsForExcel; i++)
			{
				DummyChildBusinessObject child = businessObject.Collection.AddNew();
				child.Z0_Number = 1;
			}
			Factory.Save();

			var reportCommand = Factory.New<ReportCommand>();
			var pivot = reportCommand.Documents.AddNew();
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;

			using (var tempFile = TempFile.NewWithExtension("CSV"))
			using (var pack = new DocumentPack(reportCommand))
			using (var report = new Report(pack, excelTemplate, Guid.Empty, Core.Constants.DataContext.None, "TestReport"))
			using (var printTask = new PrintTask())
			using (Report.TemporarilyStopErrorsThrowingAnException())
			{
				var fileName = tempFile.Filename;

				printTask.Add(pack);

				var view = new Mock<IDocumentDeliveryView>();
				var presenter = new DocumentDeliveryPresenter(view.Object, printTask, new DeliveryInstructions());

				view.Setup(m => m.GetSaveAsFileName()).Returns(new SaveAsFileInfo(File.OpenWrite(fileName), fileName, SaveAsFileType.Csv));
				view.Raise(m => m.SaveAsButtonClicked += null, presenter, EventArgs.Empty);

				using (var reader = File.OpenRead(fileName))
				{
					AssertNull(reader.CopyToByteArray());
				}
				AssertNotEquals("No message notifying of successful save should be shown", "TestReport has been successfully saved", UnitTestUserNotification.Instance.LastMessage.Text);
				view.VerifyAll();
				ErrorReporter.Clear();
			}
		}

		[ExpectNoExceptions]
		public void TestCancelDeliveryScreen_ShouldResetCachedReports()
		{
			var printTask = new Mock<PrintTask>();
			var view = new DocumentDeliveryViewWhichCanFireEvent();
			printTask.Setup(m => m.ResetCachedReports());
			var presenter = new DocumentDeliveryPresenter(view, printTask.Object, new DeliveryInstructions());
			view.FireViewClosed();
			printTask.Verify(m => m.ResetCachedReports(), Times.Exactly(1));
		}

		[SnailTest]
		[ExpectNoExceptions]
		public void TestDoSaveAsButtonCoreWithExceedRowsLimit()
		{
			var businessObject = Factory.New<DummyBusinessObject>();
			businessObject.Z0_Number = 1;
			for (int i = 0; i < 3; i++)
			{
				DummyChildBusinessObject child = businessObject.Collection.AddNew();
				child.Z0_Number = 1;
			}
			Factory.Save();

			AssertEquals("The excel file extension should change from 'Xls' to 'Xlsx'.", SaveReportAndReturnFileFormatWhenOpened(SaveAsFileType.Xls, false, true), "Xlsx");
		}

		void AssertSaveAsFunctionalityShouldUseWatermarkWhenExporting(string extension, SaveAsFileType saveAsFileType)
		{
			if (saveAsFileType == SaveAsFileType.Pdf)
			{
				DocumentsDataRegistry.Instance.EmbedFontsInPDF.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			}

			var template = Factory.New<StmTemplateBase>();
			template.SO_Name = "Test";
			template.SO_Template = new ExcelTemplateForUnitTesting("NumberFilterBeUsedInTemplate.xlsx", TestFilesSubFolder.ReportTestFiles).GetAsByteArray();
			template.SO_DataContext = nameof(Core.Constants.DataContext.UnitTest);

			var reportCommand = Factory.New<ReportCommand>();
			reportCommand.SU_MenuName = "Dummy Report";

			var document = reportCommand.Documents.AddNew();
			document.SI_SU = reportCommand.PK;
			document.SI_SO = template.PK;

			using (var tempFile = TempFile.NewWithExtension(extension))
			using (var printSet = new ReportPrintSet(reportCommand))
			{
				var report = printSet[0][0] as Report;
				report.PrepareForRender();

				var deliveryinfo = report.GetDeliveryInfo(true);
				AssertEquals(WatermarkHelper.NonCommercialUseWatermarkText, deliveryinfo.Watermark.AsText);

				var fileName = tempFile.Filename;
				SaveReportAsFile(printSet, saveAsFileType, fileName);

				if (saveAsFileType == SaveAsFileType.Pdf)
				{
					using (var pdfDocument = PdfDocument.LoadFile(fileName))
					{
						Assert("PDF should contain the watermark text", pdfDocument.GetAllText().Contains(WatermarkHelper.NonCommercialUseWatermarkText));
					}
				}
				else if (saveAsFileType == SaveAsFileType.Tif)
				{
					string expectedPath = TestCaseWithFactory.BaseSourcePath + @"Enterprise\Product\Documents\DocumentEngine.GUI\DocumentEngine.GUI\Testing\Watermark.Tif";
					var documentUtilities = ObjectFactory.Get<IDocumentUtilities>();
					AssertEquals("Output file should be the same.", documentUtilities.GetFileAsBytes(expectedPath), documentUtilities.GetFileAsBytes(fileName));
				}
			}
		}

		void CreateDummyBusinessObject(int number, string text)
		{
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Number = number;
			dummy.Z0_VarCharMax = text;
		}

		void AssertSaveReportAsCsv(string templateString, SaveAsFileType saveAsType, string expectedFileContent, string expectedMessage)
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test", templateString);

			var reportCommand = Factory.New<ReportCommand>();
			reportCommand.SU_MenuName = "Dummy Report";
			var document = reportCommand.Documents.AddNew();
			document.SI_SU = reportCommand.PK;
			document.SI_SO = template.PK;

			using (Report.TemporarilyUseMainConnection())
			using (var tempFile = TempFile.NewWithExtension("CSV"))
			using (var printSet = new ReportPrintSet(reportCommand))
			{
				var fileName = tempFile.Filename;
				SaveReportAsFile(printSet, saveAsType, fileName);
				using (var stream = File.OpenRead(fileName))
				{
					var bytes = stream.CopyToByteArray();

					if (expectedFileContent != null)
					{
						AssertMultilineASCIIEquals("Report should be saved in a CSV file selected by the user.", expectedFileContent, Encoding.UTF8.GetString(bytes));
						AssertEquals(string.Format(expectedMessage, fileName), UnitTestUserNotification.Instance.LastMessage.Text);
					}
					else
					{
						AssertNull(bytes);
						AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
		}

		void SaveReportAsFile(PrintTask printTask, SaveAsFileType saveAsType, string fileName)
		{
			var view = new Mock<IDocumentDeliveryView>();
			var presenter = new DocumentDeliveryPresenter(view.Object, printTask, new DeliveryInstructions(printTask[0]).Clone() as DeliveryInstructions);

			view.Setup(m => m.GetSaveAsFileName()).Returns(new SaveAsFileInfo(File.OpenWrite(fileName), fileName, saveAsType));

			view.Object.SaveAsButtonClicked += null;
			view.Raise(m => m.SaveAsButtonClicked += null, presenter, EventArgs.Empty);
		}

		string SaveReportAndReturnFileFormatWhenOpened(SaveAsFileType saveAsType, bool isRemote = false, bool isTestSaveExcelWithExceedRowsLimit = false)
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
	@"{A}-[#Config]
{A}-[Data:ReportData=Select DummyBizo1.Z0_Number as Number from dbo.DummyBizo as DummyBizo1 cross join dbo.DummyBizo as DummyBizo2 cross join dbo.DummyBizo as DummyBizo3]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.Number>]
{A}-[#EndOfReport]");

			var reportCommand = Factory.New<ReportCommand>();
			reportCommand.SU_MenuName = "Dummy Report";
			var document = reportCommand.Documents.AddNew();
			document.SI_SU = reportCommand.PK;
			document.SI_SO = template.PK;

			using (Report.TemporarilyUseMainConnection())
			using (var tempFile = TempFile.NewWithExtension(saveAsType.ToString()))
			using (var printSet = new ReportPrintSet(reportCommand))
			{
				var fileName = tempFile.Filename;
				var view = new Mock<IDocumentDeliveryView>();
				var presenter = isTestSaveExcelWithExceedRowsLimit ?
					new DocumentDeliveryPresenterThatSaveStreamWithOverlimitRows(view.Object, printSet, new DeliveryInstructions(printSet[0]).Clone() as DeliveryInstructions) :
					new DocumentDeliveryPresenter(view.Object, printSet, new DeliveryInstructions(printSet[0]).Clone() as DeliveryInstructions);
				Stream fileStream;
#if !WINZOR
				if (isRemote)
				{
					var mappedClientPath = new Mock<MappedClientPath>();
					mappedClientPath.Setup(m => m.GetMappedPath(fileName)).Returns(fileName);
					fileStream = new RemoteFileSaveStream(fileName, mappedClientPath.Object);
				}
				else
#endif
				{
					fileStream = File.Open(fileName, FileMode.Create, FileAccess.ReadWrite);
				}

				var fileInfo = new SaveAsFileInfo(fileStream, fileName, saveAsType);

				view.Setup(m => m.GetSaveAsFileName()).Returns(fileInfo);
				view.Raise(m => m.SaveAsButtonClicked += null, presenter, EventArgs.Empty);

				var xls = new XlsFile(fileInfo.DisplayFileName);
				var result = xls.FileFormatWhenOpened.ToString();

				if (File.Exists(fileInfo.DisplayFileName))
				{
					File.Delete(fileInfo.DisplayFileName);
				}

				return result;
			}
		}

		const string TemplateStringExportableToCsv = @"{A}-[#Config]
{A}-[Name=Dummy Report]
{A}-[Data:ReportData=SELECT * FROM dbo.DummyBizo]
{A}-[ColumnHeadings:]    {B}-[DisplayLabel=""Number"", HeadingText=""Number""]    {C}-[DisplayLabel=""Text"", HeadingText=""Text""]
{A}-[#DocumentHeader]
{B}-[Page <CurrentPage> of <TotalPages>]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.Z0_Number>]    {C}-[<ReportData.Z0_VarCharMax>]
{A}-[#EndOfReport]";

		const string TemplateStringNonExportableToCsv = @"{A}-[#Config]
{A}-[Name=Dummy Report]
{A}-[Data:ReportData=SELECT * FROM dbo.DummyBizo]
{A}-[ColumnHeadings:]    {B}-[DisplayLabel=""Number"", HeadingText=""Number""]    {C}-[DisplayLabel=""Text"", HeadingText=""Text""]
{A}-[#DocumentHeader]
{B}-[Page <CurrentPage> of <TotalPages>]
{A}-[#SectionBody:Data=ReportData]
{A}-[#GroupBy:ReportData.Z0_Number]
{B}-[<ReportData.Z0_Number>]    {C}-[<ReportData.Z0_VarCharMax>]
{A}-[#EndOfReport]";

		sealed class DocumentDeliveryPresenterThatSaveStreamWithOverlimitRows : DocumentDeliveryPresenter
		{
			public DocumentDeliveryPresenterThatSaveStreamWithOverlimitRows(IDocumentDeliveryView view, PrintTask printTask,
				DeliveryInstructions deliveryInstructions) : base(view, printTask, deliveryInstructions)
			{
			}

			protected override void SaveStream(ExcelInterface excelInterface, Stream fileContents, SaveAsFileType type, Stream stream, DeliveryInfo deliveryInfo)
			{
				excelInterface.LoadExcelFile(fileContents);
				ExcelWorkSheet tempSheet = excelInterface.WorkSheets[excelInterface.ActiveWorksheet];
				tempSheet.DuplicateRows(1, 1, 2, 65555);
				excelInterface.SaveToStream(fileContents);
				base.SaveStream(excelInterface, fileContents, type, stream, deliveryInfo);
			}
		}

		sealed class DocumentDeliveryViewWhichCanFireEvent : IDocumentDeliveryView
		{
			internal void FireViewClosed()
			{
				ViewClosed(this, new FormClosedEventArgs(CloseReason.UserClosing));
			}

			public void HideLanguageSelectionDropDown()
			{
			}

			public event EventHandler SaveAsButtonClicked { add { } remove { } }
			public event FormClosedEventHandler ViewClosed;

			public SaveAsFileInfo GetSaveAsFileName()
			{
				return null;
			}

			public void HidePageRangesPanel()
			{
			}

			public void HideBackgroundDeliveryCheckBox()
			{
			}
		}

		[Serializable]
		sealed class DeliveryInstructionsWithEmptyRecipient : DeliveryInstructions
		{
			public DeliveryInstructionsWithEmptyRecipient(DocumentPack docPack) : base(docPack) { }

			protected override DocDeliveryContactCollection GetRecipientsCore(IStmMenuItem menuItem)
			{
				return new DocDeliveryContactCollection(Factory);
			}
		}

		sealed class DocumentDeliveryPresenterForDeliveryLanguage : DocumentDeliveryPresenter
		{
			public DocumentDeliveryPresenterForDeliveryLanguage(IDocumentDeliveryView view, PrintTask printTask,
				DeliveryInstructions deliveryInstructions) : base(view, printTask, deliveryInstructions)
			{
			}

			protected override void SaveStream(ExcelInterface excelInterface, Stream fileContents, SaveAsFileType type, Stream stream, DeliveryInfo deliveryInfo)
			{
				TemporaryLanguage = Res.CurrentLanguage;
				base.SaveStream(excelInterface, fileContents, type, stream, deliveryInfo);
			}

			public string TemporaryLanguage;
		}
	}
}

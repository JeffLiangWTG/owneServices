using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.PdfiumWrapper;
using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.DocumentEngine.DocumentDelivery;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.GUI.DocumentDelivery;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.Testing
{
	sealed class XLSPreviewFormTest : TestCaseWithFactory
	{
		public void TestSaveFileFormatResetToXlsxWhenDocumentEngineTooManyRowsForXls()
		{
			AssertSaveFileWithTooManyLimitationForXls(resourceRetrieverDocumentEngineGUI.Value.GetStream("Enterprise.DocumentEngine.GUI.Testing.TooManyRowsForXls.xlsx"));
		}

		public void TestSaveFileFormatResetToXlsxWhenDocumentEngineTooLongFormulaForXls()
		{
			AssertSaveFileWithTooManyLimitationForXls(resourceRetrieverDocumentEngineGUI.Value.GetStream("Enterprise.DocumentEngine.GUI.Testing.TooLongFormulaForXls.xlsx"));
		}

		void AssertSaveFileWithTooManyLimitationForXls(Stream fileStream)
		{
			using (var tempFile = TempFile.NewWithExtension("xls"))
			{
				var deliveryInfo = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Report);
				deliveryInfo.SetFileContents(fileStream, "xlsx");
				var documentDeliveryInfos = new DeliveryInfo[1] { deliveryInfo };

				SaveFile(fileStream, documentDeliveryInfos, DialogResult.No);
				Assert(!File.Exists(tempFile.Filename));

				var savedFileFullName = Path.ChangeExtension(tempFile.Filename, "xlsx");
				SaveFile(fileStream, documentDeliveryInfos, DialogResult.Yes);
				Assert(File.Exists(savedFileFullName));
				File.Delete(savedFileFullName);

				void SaveFile(Stream stream, DeliveryInfo[] deliveryInfos, DialogResult result)
				{
					var info = new SaveAsFileInfo(File.Open(tempFile.Filename, FileMode.Create, FileAccess.ReadWrite), tempFile.Filename, SaveAsFileType.Xls);
					using (var previewForm = new XLSPreviewFormForPreviewInExcelTesting(stream, deliveryInfos, null, info))
					{
						previewForm.Show();
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddAnswer(result);
						previewForm.SaveAsForTesting();
						Assert(!File.Exists(tempFile.Filename));
					}
				}
			}
		}

		public void TestSaveAsButtonVisibilityInDifferentSecuritieSettings()
		{
			using (var stream = new ControllableReadStream(1, new byte[] { 1, 2, 3, 4 }))
			{
				stream.Position = stream.Length;

				var documentDeliveryInfos = new DeliveryInfo[1] { new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document) };

				Env.Security.SaveAsDocumentButton.IsAllowed = true;
				using (var previewForm = new XLSPreviewFormForPreviewInExcelTesting(stream, documentDeliveryInfos, null))
				{
					previewForm.Show();
					Assert(previewForm.SaveAsButtonIsVisible);
				}

				Env.Security.SaveAsDocumentButton.IsAllowed = false;
				using (var previewForm = new XLSPreviewFormForPreviewInExcelTesting(stream, documentDeliveryInfos, null))
				{
					previewForm.Show();
					Assert(!previewForm.SaveAsButtonIsVisible);
				}

				var reportDeliveryInfos = new DeliveryInfo[1] { new DeliveryInfo(DeliveryInfo.DeliveryFormats.Report) };

				Env.Security.SaveAsReportButton.IsAllowed = true;
				using (var previewForm = new XLSPreviewFormForPreviewInExcelTesting(stream, reportDeliveryInfos, null))
				{
					previewForm.Show();
					Assert(previewForm.SaveAsButtonIsVisible);
				}

				Env.Security.SaveAsReportButton.IsAllowed = false;
				using (var previewForm = new XLSPreviewFormForPreviewInExcelTesting(stream, reportDeliveryInfos, null))
				{
					previewForm.Show();
					Assert(!previewForm.SaveAsButtonIsVisible);
				}
			}
		}

		public void TestSaveAsVisibleWhenDeliveryInfosAreMoreThanOne()
		{
			using (var stream = new ControllableReadStream(1, new byte[] { 1, 2, 3, 4 }))
			{
				stream.Position = stream.Length;

				using (var previewForm = new XLSPreviewFormForPreviewInExcelTesting(stream, null, null))
				{
					previewForm.Show();
					Assert(!previewForm.SaveAsButtonIsVisible);
				}

				var deliveryInfos = new DeliveryInfo[1] { new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document) };
				using (var previewForm = new XLSPreviewFormForPreviewInExcelTesting(stream, deliveryInfos, null))
				{
					previewForm.Show();
					Assert(previewForm.SaveAsButtonIsVisible);
				}
			}
		}

		[GuiTest]
		public void TestSaveAsFunctionality()
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

			var assertXls = new Action<string>(fileName =>
			{
				using (var excelInterface = new ExcelInterface())
				{
					excelInterface.LoadExcelFile(fileName);
					excelInterface.ActiveWorksheet = 0;
					var cell1 = excelInterface.WorkSheets[0].GetCell(0, 1);
					AssertEquals("Test Save Documents", cell1.Value);
					var cell2 = excelInterface.WorkSheets[1].GetCell(0, 1);
					AssertEquals("Test Save Documents", cell2.Value);
				}
			});
			AssertSaveAsType(documentCommand, dummy, "xls", SaveAsFileType.Xls, assertXls);

			var assertXlsx = new Action<string>(fileName =>
			{
				using (var expectedResult = new ExcelInterface())
				{
					expectedResult.LoadExcelFile(fileName);
					expectedResult.ActiveWorksheet = 0;
					var cell1 = expectedResult.WorkSheets[0].GetCell(0, 1);
					AssertEquals("Test Save Documents", cell1.Value);
					var cell2 = expectedResult.WorkSheets[1].GetCell(0, 1);
					AssertEquals("Test Save Documents", cell2.Value);
				}
			});
			AssertSaveAsType(documentCommand, dummy, "xlsx", SaveAsFileType.Xlsx, assertXlsx);

			var assertPdf = new Action<string>(fileName =>
			{
				using (var pdfDocument = PdfDocument.LoadFile(fileName))
				{
					AssertEquals(2, pdfDocument.PageCount);
					var allText = pdfDocument.GetAllText();
					AssertEquals("Test Save Documents\r\nTraining / Test \r\n Non Commercial Use only\0Test Save Documents\r\nTraining / Test \r\n Non Commercial Use only\0", allText);
				}
			});
			AssertSaveAsType(documentCommand, dummy, "pdf", SaveAsFileType.Pdf, assertPdf);

			var assertPdfa = new Action<string>(fileName =>
			{
				using (var pdfDocument = PdfDocument.LoadFile(fileName))
				{
					AssertEquals(2, pdfDocument.PageCount);
					var allText = pdfDocument.GetAllText();
					AssertEquals("Test Save Documents\r\nTraining / Test \r\n Non Commercial Use only\0Test Save Documents\r\nTraining / Test \r\n Non Commercial Use only\0", allText);
				}
			});
			AssertSaveAsType(documentCommand, dummy, "pdf", SaveAsFileType.Pdfa, assertPdfa);

			var assertTif = new Action<string>(fileName =>
			{
				var documentUtilities = ObjectFactory.Get<IDocumentUtilities>();
				var expectedContent = resourceRetrieverDocumentEngineGUI.Value.GetBytes("Enterprise.DocumentEngine.GUI.Testing.DocumentsSaveAsTif.tif");
				var actualContent = documentUtilities.GetFileAsBytes(fileName);
				AssertEquals("Output file should be the same.", expectedContent, actualContent);
			});
			AssertSaveAsType(documentCommand, dummy, "tif", SaveAsFileType.Tif, assertTif);
		}

		void AssertSaveAsType(DocumentCommand documentCommand, IDocumentSupportable documentSupportable, string extension, SaveAsFileType saveAsFileType, Action<string> assertMethod)
		{
			using (var tempFile = TempFile.NewWithExtension(extension))
			using (var documentPack = new DocumentPack(documentCommand, documentSupportable, null, null))
			{
				var deliveryInstructions = new DeliveryInstructions(documentPack);
				var deliverables = deliveryInstructions.DocumentsToBeDelivered.OfType<IDeliverable>().Where(iDeliverable => iDeliverable.IncludedInPrint);

				var strategy = new DefaultCreateDeliveryInfoStrategy();
				var docDeliveryContact = deliveryInstructions.OfficialRecipient;
				docDeliveryContact.AttachmentType = extension.ToUpper();

				var deliveryInfos = deliverables.Select(deliverable => strategy.CreateDeliveryInfo(deliverable, docDeliveryContact, deliveryInstructions));

				using (var xlsPreviewForm = new XLSPreviewFormForSaveAsFunctionalityTest(deliveryInfos.First().FileContents, deliveryInfos.ToArray(), null, new SaveAsFileInfo(File.Open(tempFile.Filename, FileMode.Create, FileAccess.ReadWrite), tempFile.Filename, saveAsFileType)))
				{
					xlsPreviewForm.Show();
					xlsPreviewForm.SaveAsForTesting();
					assertMethod(tempFile.Filename);
				}
			}
		}

		[ExpectNoExceptions]
		public void TestEndOfFileConditionHandledForStream()
		{
			using (Stream stream = new ControllableReadStream(1, new byte[] { 1, 2, 3, 4 }))
			{
				stream.Position = stream.Length;

				using (XLSPreviewFormForPreviewInExcelTesting previewForm = new XLSPreviewFormForPreviewInExcelTesting(stream, null, null))
				{
					previewForm.Show();
				}
			}
		}

		public void TestOpenInExcelShowsTheErrorWhenBadThingsHappen()
		{
			using (Stream stream = new ControllableReadStream(1, new byte[] { 1 }))
			using (XLSPreviewFormForPreviewInExcelTesting previewForm = new XLSPreviewFormForPreviewInExcelTesting(stream, null, null))
			{
				AssertEquals("Precondition: LastMessage.Text", null, UnitTestUserNotification.Instance.LastMessage.Text);
				previewForm.Show();
				previewForm.OpenFileInExcelForTesting();
				AssertEquals("LastMessage.Text", "Error Opening Excel File: The selected file could not be loaded. It may be damaged or an unsupported format. Please check the file and try again.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[ExpectNoExceptions]
		public void TestChangePageToLargeNumber()
		{
			using (Stream stream = new ControllableReadStream(1, new byte[] { 1, 2, 3, 4 }))
			using (XLSPreviewFormForPreviewInExcelTesting previewForm = new XLSPreviewFormForPreviewInExcelTesting(stream, null, null))
			{
				previewForm.Show();

				var textBox = previewForm.GetCurrentPageTextBox();
				textBox.Focus();
				textBox.Text = "2147483649";
				textBox.Parent.Focus();
			}
		}

		[ExpectNoExceptions]
		public void TestIndexOutOfRangeNoExceptionThrown()
		{
			AssertPreview("IndexOutOfRangeExceptionWhenPreview.xlsx",
				resourceRetrieverDocumentEngine.Value.GetStream("Enterprise.DocumentEngine.Test.FlexCelInterface.Testing.IndexOutOfRangeExceptionWhenPreview.xlsx"));
		}

		[ExpectNoExceptions]
		public void TestUnexpectedCharOnFormulaNoExceptionThrown()
		{
			AssertPreview("TestUnexpectedCharOnFormula.xlsx",
				resourceRetrieverDocumentEngine.Value.GetStream("Enterprise.DocumentEngine.Test.FlexCelInterface.Testing.TestUnexpectedCharOnFormula.xlsx"));
		}

		[RequiresSTA]
		public void TestOnePageSlowPreviewWithCenterAcrossCellsRemovedFromFirstRow()
		{
			AssertPreview("OnePageSlowPreviewWithCenterAcrossCellsRemovedFromFirstRow.xls",
				resourceRetrieverDocumentEngine.Value.GetStream("Enterprise.DocumentEngine.Test.FlexCelInterface.Testing.OnePageSlowPreviewWithCenterAcrossCellsRemovedFromFirstRow.xls"));
		}

		[RequiresSTA]
		public void TestOnePageSlowPreview()
		{
			AssertPreview("OnePageSlowPreview.xls",
				resourceRetrieverDocumentEngine.Value.GetStream("Enterprise.DocumentEngine.Test.FlexCelInterface.Testing.OnePageSlowPreview.xls"));
		}

		[RequiresSTA]
		[ExpectNoExceptions]
		public void TestInvalidFileNoExceptionThrown()
		{
			AssertPreview("InvalidFile.xls",
				resourceRetrieverDocumentEngine.Value.GetStream("Enterprise.DocumentEngine.Test.FlexCelInterface.Testing.InvalidFile.xls"));
			AssertEquals("Excel invalid file message", "Error reading Excel records. File invalid", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestOnePageSlowPreviewAsXlsx()
		{
			AssertEquals("Precondition: LastMessage.Text", null, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertPreview("OnePageSlowPreview.xlsx",
				resourceRetrieverDocumentEngine.Value.GetStream("Enterprise.DocumentEngine.Test.FlexCelInterface.Testing.OnePageSlowPreview.xlsx"));
			AssertEquals("Precondition: LastMessage.Text", null, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestOpenInExcelButtonWidth()
		{
			using (var stream = new ControllableReadStream(1, new byte[] { 1, 2, 3, 4 }))
			{
				stream.Position = stream.Length;

				using (var previewForm = new XLSPreviewFormForPreviewInExcelTesting(stream, null, null))
				{
					previewForm.Show();
					Assert(!previewForm.OpenInExcelButton.Text.EndsWith("..."));
				}
			}
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetrieverDocumentEngine.IsValueCreated)
			{
				resourceRetrieverDocumentEngine.Value.Dispose();
			}
			if (resourceRetrieverDocumentEngineGUI.IsValueCreated)
			{
				resourceRetrieverDocumentEngineGUI.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetrieverDocumentEngine = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever(typeof(PrintTaskTest).Assembly));

		readonly Lazy<EmbeddedResourceRetriever> resourceRetrieverDocumentEngineGUI = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		static void AssertPreview(string fileName, Stream fileStream)
		{
			var start = DateTime.Now;
			var deliveryInfos = new DeliveryInfo[] { new DeliveryInfo(DeliveryInfo.DeliveryFormats.Report) };

			using (var previewForm = new XLSPreviewForm(fileStream, deliveryInfos, null))
			{
				previewForm.Show();
				Application.DoEvents();
			}

			var elapsed = DateTime.Now - start;

			Assert("Time taken to show a one page preview should take less than 10 seconds - took " + elapsed.ToString() + " seconds.\r\n" + fileName, elapsed < TimeSpan.FromSeconds(10));
		}

		class XLSPreviewFormForPreviewInExcelTesting : XLSPreviewForm
		{
			public XLSPreviewFormForPreviewInExcelTesting(Stream xlsStream, DeliveryInfo[] deliveryInfos, IDeliverCapableForm parentForm)
				: base(xlsStream, deliveryInfos, parentForm)
			{
			}

			public XLSPreviewFormForPreviewInExcelTesting(Stream xlsStream, DeliveryInfo[] deliveryInfos, IDeliverCapableForm parentForm, SaveAsFileInfo saveAsFileInfo)
				: base(xlsStream, deliveryInfos, parentForm)
			{
				this.saveAsFileInfo = saveAsFileInfo;
			}

			public void OpenFileInExcelForTesting()
			{
				OpenFileInExcel();
			}

			public void SaveAsForTesting()
			{
				SaveAsButton.PerformClick();
			}

			readonly SaveAsFileInfo saveAsFileInfo;

			internal override SaveAsFileInfo GetSaveAsInfo()
			{
				if (saveAsFileInfo != null)
				{
					return saveAsFileInfo;
				}
				return base.GetSaveAsInfo();
			}

			public bool SaveAsButtonIsVisible => SaveAsButton.Visible;
		}

		class XLSPreviewFormForSaveAsFunctionalityTest : XLSPreviewForm
		{
			public XLSPreviewFormForSaveAsFunctionalityTest(Stream xlsStream, DeliveryInfo[] deliveryInfos, IDeliverCapableForm parentForm, SaveAsFileInfo saveAsFileInfo)
				: base(xlsStream, deliveryInfos, parentForm)
			{
				SaveAsFileInfo = saveAsFileInfo;
			}

			readonly SaveAsFileInfo SaveAsFileInfo;

			public void SaveAsForTesting() => SaveAsButton.PerformClick();

			internal override SaveAsFileInfo GetSaveAsInfo() => SaveAsFileInfo;

			protected override void SheetsListBox_SelectedIndexChanged(object sender, EventArgs e)
			{
				// Suspend the preview because the rendering engine may be different on VS and DAT and CW1 when running the unit test, so just suspend the document preview, just compare the saved files.
			}
		}
	}
}

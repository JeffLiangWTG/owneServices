using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data.Testing;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DataProviders;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.GUI.DocumentDelivery;
using Enterprise.DocumentEngine.GUI.DocumentMenu;
using Enterprise.DocumentEngine.GUI.Scheduler;
using Enterprise.DocumentEngine.GUI.Visualisation;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Scheduler.Business.Testing;
using Enterprise.DocumentEngine.Shared;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.ExcelTemplates;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Scheduler.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.Testing
{
	[TestedType(typeof(DocDeliveryForm))]
	sealed class DocDeliveryFormTest : ZFormBasherTest
	{
		public void TestSaveAsButtonVisibilityForReport()
		{
			var command = Factory.NewWithValidTestData<ReportCommand>();
			var printTask = new PrintTask(command);
			var pack = new DocumentPack(command);
			printTask.Add(pack);
			var instructions = new DeliveryInstructions(pack);

			Env.Security.SaveAsReportButton.IsAllowed = true;
			using (MockDocDeliveryForm form = new MockDocDeliveryForm(printTask, instructions))
			{
				form.Show();
				Assert("Preview Button is visible", form.saveAsButton.Visible);
			}

			Env.Security.SaveAsReportButton.IsAllowed = false;
			using (MockDocDeliveryForm form = new MockDocDeliveryForm(printTask, instructions))
			{
				form.Show();
				Assert("Preview Button is not visible", !form.saveAsButton.Visible);
			}
		}

		public void TestDocDeliveryFormControlsVisibilityWithOverridePrintDetailsInstruction()
		{
			var command = Factory.NewWithValidTestData<ReportCommand>();
			var printTask = new PrintTask(command);
			var pack = new DocumentPack(command);
			printTask.Add(pack);
			var instructions = new DeliveryInstructions(pack);
			instructions.DeliveryOptions = AllowedDeliveryOptions.OverridePrintDetails;

			using (MockDocDeliveryForm form = new MockDocDeliveryForm(printTask, instructions))
			{
				form.Show();
				Assert("Documents To Send Tab should be visible", form.DocumentsTabPage.TabVisible);
				Assert("Destination Tab should be invisible", !form.MainPage.TabVisible);
				Assert("Included EDocs Tab should be invisible", !form.IncludedEDocsTabPage.TabVisible);
				Assert("Cover Note Tab should be invisible", !form.CoverNotePage.TabVisible);
				Assert("Language DropEdit should be invisible", !form.LanguageZDropEdit.Visible);
				Assert("Draft Checkbox should be invisible", !form.PrintAsDraftCheckbox.Visible);
				Assert("Modify Button should be invisible", !form.VisualiseButton.Visible);
				Assert("Preview Button should be invisible", !form.PreviewButton.Visible);
				Assert("Save As Button should be invisible", !form.saveAsButton.Visible);
				Assert("Cancel Button should be visible", form.CloseButton.Visible);
				Assert("Deliver Button should be visible", form.DeliverButton.Visible);
				Assert("Show Only Printers User Can Print To CheckBox should be visible", form.ShowOnlyPrintersUserCanPrintToCheckBox.Visible);
			}
		}

		public void TestSaveAsButtonVisibilityForDocument()
		{
			var command = Factory.NewWithValidTestData<DocumentCommand>();
			var printTask = new PrintTask(command);
			var pack = new DocumentPack(command);
			printTask.Add(pack);
			var instructions = new DeliveryInstructions(pack);

			Env.Security.SaveAsDocumentButton.IsAllowed = true;
			using (MockDocDeliveryForm form = new MockDocDeliveryForm(printTask, instructions))
			{
				form.Show();
				Assert("Preview Button is visible", form.saveAsButton.Visible);
			}

			Env.Security.SaveAsDocumentButton.IsAllowed = false;
			using (MockDocDeliveryForm form = new MockDocDeliveryForm(printTask, instructions))
			{
				form.Show();
				Assert("Preview Button is not visible because security is not allowed.", !form.saveAsButton.Visible);
			}

			var pack2 = new DocumentPack(command);
			printTask.Add(pack2);
			instructions.DocumentPackCount = 2;
			Env.Security.SaveAsDocumentButton.IsAllowed = true;
			using (MockDocDeliveryForm form = new MockDocDeliveryForm(printTask, instructions))
			{
				form.Show();
				Assert("Preview Button is not visible because multi doc packs.", !form.saveAsButton.Visible);
			}
		}

		public void TestSaveAsButtonVisibilityForForms()
		{
			var command = Factory.NewWithValidTestData<DocumentCommand>();
			var printTask = new PrintTask(command);
			var pack = new DocumentPack(command);
			var instructions = new MockDocumentDeliveryInstructions(pack);

			Env.Security.SaveAsDocumentButton.IsAllowed = true;
			using (MockDocDeliveryForm form = new MockDocDeliveryForm(printTask, instructions))
			{
				form.Show();
				Assert("Preview Button is visible", form.saveAsButton.Visible);
			}

			Env.Security.SaveAsDocumentButton.IsAllowed = false;
			using (MockDocDeliveryForm form = new MockDocDeliveryForm(printTask, instructions))
			{
				form.Show();
				Assert("Preview Button is not visible because security is not allowed.", !form.saveAsButton.Visible);
			}
		}

		public void TestShowOnlyPrintersUserCanPrintToCheckBox()
		{
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.SU_BusinessContext = nameof(BusinessContext.Test);
			var reportTempate1 = Factory.New<StmTemplateBase>();
			reportTempate1.SO_Name = "template 1";
			reportTempate1.SO_DataContext = "GenericFreightJob";
			var pivot1 = documentCommand.Documents.AddNew();
			pivot1.SI_DocumentTitle = "Test Document1";
			pivot1.SI_SU = documentCommand.PK;
			pivot1.SI_SO = reportTempate1.PK;
			pivot1.DocConfigs.AddNew();

			var reportTempate2 = Factory.New<StmTemplateBase>();
			reportTempate2.SO_Name = "template 2";
			reportTempate2.SO_DataContext = "GenericFreightJob";
			var pivot2 = documentCommand.Documents.AddNew();
			pivot2.SI_DocumentTitle = "Test Document2";
			pivot2.SI_SU = documentCommand.PK;
			pivot2.SI_SO = reportTempate2.PK;
			pivot2.DocConfigs.AddNew();

			var stmPrintQueue1 = Factory.NewWithValidTestData<StmPrintQueue>();
			var stmPrintQueue2 = Factory.NewWithValidTestData<StmPrintQueue>();
			Env.Security.GetPrintQueueCheckPoint(stmPrintQueue1.PK.ToGuid(), null).IsAllowed = false;
			Env.Security.GetPrintQueueCheckPoint(stmPrintQueue2.PK.ToGuid(), null).IsAllowed = true;

			Factory.Save();

			var docSupportedBO = Factory.New<DummyBODocSupportable>();
			documentCommand.Parent = docSupportedBO;
			using (var printTask = new DocumentPrintSet(documentCommand))
			using (var documentPack = new DocumentPack(documentCommand, docSupportedBO, null, null))
			{
				printTask.Add(documentPack);
				var instructions = new DeliveryInstructions(printTask[0]);
				using (var form = new MockDocDeliveryForm(printTask, instructions))
				{
					form.Show();

					Assert(form.ShowOnlyPrintersUserCanPrintToCheckBox.Checked);
					AssertEquals(1, instructions.PrinterDelivery.Printers.Count);
					foreach (var deliverable in instructions.DocumentsToBeDelivered.OfType<IDeliverable>())
					{
						AssertEquals(1, deliverable.PrinterDetails.Printers.Count);
					}

					form.ShowOnlyPrintersUserCanPrintToCheckBox.Checked = false;
					AssertEquals(2, instructions.PrinterDelivery.Printers.Count);
					foreach (var deliverable in instructions.DocumentsToBeDelivered.OfType<IDeliverable>())
					{
						AssertEquals(2, deliverable.PrinterDetails.Printers.Count);
					}
				}
			}
		}

		public void TestSpecificPageRangesHidden_MultiDocumentPacks()
		{
			using (DocumentsDataRegistry.Instance.EnableSpecificPageRangesPrintingOption.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var documentCommand1 = Factory.New<DocumentCommand>();
				documentCommand1.SU_MenuName = "menu 1";
				documentCommand1.SU_BusinessContext = nameof(BusinessContext.Test);
				var reportTempate1 = Factory.NewWithValidTestData<StmTemplateBase>();
				reportTempate1.SO_Name = "template 1";
				reportTempate1.SO_DataContext = "GenericFreightJob";
				var pivot1 = documentCommand1.Documents.AddNew();
				pivot1.SI_DocumentTitle = "Test Document1";
				pivot1.SI_SU = documentCommand1.PK;
				pivot1.SI_SO = reportTempate1.PK;
				pivot1.DocConfigs.AddNew();

				var documentCommand2 = Factory.New<DocumentCommand>();
				documentCommand2.SU_MenuName = "menu 2";
				documentCommand2.SU_BusinessContext = nameof(BusinessContext.Test);
				var reportTempate2 = Factory.NewWithValidTestData<StmTemplateBase>();
				reportTempate2.SO_Name = "template 2";
				reportTempate2.SO_DataContext = "GenericFreightJob";
				var pivot2 = documentCommand2.Documents.AddNew();
				pivot2.SI_DocumentTitle = "Test Document2";
				pivot2.SI_SU = documentCommand2.PK;
				pivot2.SI_SO = reportTempate2.PK;
				pivot2.DocConfigs.AddNew();

				var menuMenuPivot = Factory.New<StmMenuMenuPivot>();
				menuMenuPivot.SF_SU_Inward = documentCommand1.PK;
				menuMenuPivot.SF_SU_Outward = documentCommand2.PK;

				Factory.Save();

				var docSupportedBO = Factory.New<DummyBODocSupportable>();
				documentCommand1.Parent = docSupportedBO;
				documentCommand2.Parent = docSupportedBO;
				using (var documentPack1 = new DocumentPack(documentCommand1, docSupportedBO, null, null))
				using (var documentPack2 = new DocumentPack(documentCommand2, docSupportedBO, null, null))
				using (var printTask = new DocumentPrintSet(documentCommand1))
				{
					printTask.Add(documentPack1);
					printTask.Add(documentPack2);

					var instructions = new DeliveryInstructions(printTask[0]);
					using (var form = new MockDocDeliveryForm(printTask, instructions))
					{
						AssertEquals(2, printTask.GetDocumentPacks().Count());
						form.Show();
						AssertEquals("SpecifiedPageRangesCheckBox should not be visible", false, form.PageRangesSpecifiedCheckBox.Visible);
					}
				}
			}
		}

		public void TestSpecificPageRangesHidden_MultiTemplates()
		{
			using (DocumentsDataRegistry.Instance.EnableSpecificPageRangesPrintingOption.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var documentCommand = Factory.New<DocumentCommand>();
				documentCommand.SU_BusinessContext = nameof(BusinessContext.Test);
				var reportTempate1 = Factory.New<StmTemplateBase>();
				reportTempate1.SO_Name = "template 1";
				reportTempate1.SO_DataContext = "GenericFreightJob";
				var pivot1 = documentCommand.Documents.AddNew();
				pivot1.SI_DocumentTitle = "Test Document1";
				pivot1.SI_SU = documentCommand.PK;
				pivot1.SI_SO = reportTempate1.PK;
				pivot1.DocConfigs.AddNew();

				var reportTempate2 = Factory.New<StmTemplateBase>();
				reportTempate2.SO_Name = "template 2";
				reportTempate2.SO_DataContext = "GenericFreightJob";
				var pivot2 = documentCommand.Documents.AddNew();
				pivot2.SI_DocumentTitle = "Test Document2";
				pivot2.SI_SU = documentCommand.PK;
				pivot2.SI_SO = reportTempate2.PK;
				pivot2.DocConfigs.AddNew();

				Factory.Save();

				var docSupportedBO = Factory.New<DummyBODocSupportable>();
				documentCommand.Parent = docSupportedBO;
				using (var printTask = new DocumentPrintSet(documentCommand))
				using (var documentPack = new DocumentPack(documentCommand, docSupportedBO, null, null))
				{
					printTask.Add(documentPack);
					var instructions = new DeliveryInstructions(printTask[0]);
					using (var form = new MockDocDeliveryForm(printTask, instructions))
					{
						AssertEquals(1, printTask.GetDocumentPacks().Count());
						AssertEquals(2, printTask.GetFirstDocumentPack().OfType<Report>().Count());
						form.Show();
						Assert("SpecifiedPageRangesCheckBox should not be visible", !form.PageRangesSpecifiedCheckBox.Visible);
					}
				}
			}
		}

		[RequiresSTA]
		public void TestSpecificPageRangesHidden_SingleTemplateAndEDocs()
		{
			using (DocumentsDataRegistry.Instance.EnableSpecificPageRangesPrintingOption.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var documentCommand = Factory.New<DocumentCommand>();
				documentCommand.SU_BusinessContext = "Shipment";
				var reportTempate = Factory.New<StmTemplateBase>();
				var template1 = DocumentEngineTestHelper.CreateTemplateFromString(
					@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.ZStringProperty>]
{B}-[<Collection.CollectionOwnProperty>]
{A}-[#EndOfReport]");

				reportTempate.SO_Template = template1;
				reportTempate.SO_DataContext = "GenericFreightJob";
				var pivot1 = Factory.New<StmMenuTemplatePivotBase>();
				pivot1.SI_DocumentTitle = "Test Document1";
				pivot1.SI_SU = documentCommand.PK;
				pivot1.SI_SO = reportTempate.PK;

				Factory.Save();

				var docSupportedBO = Factory.New<DummyBODocSupportable>();
				docSupportedBO.Collection.AddNew();
				docSupportedBO.Collection.AddNew();
				docSupportedBO.DocManagerInfo.AddFileOrDocument(new byte[] { 1, 2, 3 }, "testEDoc1.txt", "COR");
				documentCommand.Parent = docSupportedBO;
				using (var printTask = new DocumentPrintSet(documentCommand))
				using (var documentPack = new DocumentPack(documentCommand, docSupportedBO, null, null))
				{
					printTask.Add(documentPack);
					var instructions = new DeliveryInstructions(documentPack);
					using (var form = new MockDocDeliveryForm(printTask, instructions))
					{
						AssertEquals(1, printTask.GetDocumentPacks().Count());
						var docPack = instructions.DocPack;
						AssertEquals("the doc pack has 2 ideliverables.", 2, docPack.Count);
						AssertEquals("the doc pack only has 1 report.", 1, docPack.OfType<Report>().Count());
						var report = printTask.GetFirstDocumentPack().OfType<Report>().First();
						AssertEquals(true, printTask.GetFirstDocumentPack().OfType<Report>().First().CanSpecifyPageRanges);
						form.Show();
						Assert("SpecifiedPageRangesCheckBox should be visible", form.PageRangesSpecifiedCheckBox.Visible);
						AssertEquals(2, instructions.DataSourceRowCountIfPageRangesSpecifiable);
					}
				}
			}
		}

		public void TestSpecificPageRangesHidden_MultiSheets()
		{
			using (DocumentsDataRegistry.Instance.EnableSpecificPageRangesPrintingOption.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var documentCommand = Factory.New<DocumentCommand>();
				documentCommand.SU_BusinessContext = nameof(BusinessContext.Test);
				var reportTempate = Factory.New<StmTemplateBase>();
				var unicodeTempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.DocumentTestFiles.UnicodeTest.xls", "UnicodeTest.xls");
				var excelTemplate = new ExcelTemplateForUnitTesting("UnicodeTest.xls", Path.GetFullPath(unicodeTempFileName));
				reportTempate.SO_Template = excelTemplate.GetAsByteArray();
				reportTempate.SO_DataContext = "GenericFreightJob";
				var pivot1 = documentCommand.Documents.AddNew();
				pivot1.SI_DocumentTitle = "Test Document1";
				pivot1.SI_SU = documentCommand.PK;
				pivot1.SI_SO = reportTempate.PK;
				pivot1.DocConfigs.AddNew();

				Factory.Save();

				var docSupportedBO = Factory.New<DummyBODocSupportable>();
				documentCommand.Parent = docSupportedBO;
				using (var printTask = new DocumentPrintSet(documentCommand))
				using (var documentPack = new DocumentPack(documentCommand, docSupportedBO, null, null))
				{
					printTask.Add(documentPack);
					var instructions = new DeliveryInstructions(documentPack);
					using (var form = new MockDocDeliveryForm(printTask, instructions))
					{
						AssertEquals(1, printTask.GetDocumentPacks().Count());
						AssertEquals(1, printTask.GetFirstDocumentPack().OfType<Report>().Count());
						var report = printTask.GetFirstDocumentPack().OfType<Report>().First();
						AssertEquals(3, report.XlInterface.SheetCount);
						AssertEquals(false, report.CanSpecifyPageRanges);
						form.Show();
						Assert("SpecifiedPageRangesCheckBox should not be visible", !form.PageRangesSpecifiedCheckBox.Visible);
					}
				}

				var template = Factory.New<StmTemplateBase>();
				template.SO_Name = "Test";
				var groupByPageTempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.GroupByPage.xls", "GroupByPage.xls");
				template.SO_Template = new ExcelTemplateForUnitTesting("GroupByPage.xls", Path.GetFullPath(groupByPageTempFileName)).GetAsByteArray();
				template.SO_DataContext = nameof(Core.Constants.DataContext.UnitTest);

				var reportCommand = Factory.New<ReportCommand>();
				reportCommand.SU_MenuName = "Dummy Report";
				var document = reportCommand.Documents.AddNew();
				document.SI_SU = reportCommand.PK;
				document.SI_SO = template.PK;

				using (var printTask = new PrintTask(reportCommand))
				using (var documentPack = new DocumentPack(reportCommand))
				{
					printTask.Add(documentPack);
					var instructions = new DeliveryInstructions(documentPack);
					using (var form = new MockDocDeliveryForm(printTask, instructions))
					{
						AssertEquals(1, printTask.GetDocumentPacks().Count());
						AssertEquals(1, printTask.GetFirstDocumentPack().OfType<Report>().Count());
						var report = printTask.GetFirstDocumentPack().OfType<Report>().First();
						AssertEquals(2, report.XlInterface.SheetCount);
						report.SetOriginalDataRowSourceIfPageRangesSpecifiableForTest(new DummyDataRowSource());
						Assert(report.CanSpecifyPageRanges);
						form.Show();
						Assert("SpecifiedPageRangesCheckBox should be visible as only 1 of 2 sheets is template sheet.", !form.PageRangesSpecifiedCheckBox.Visible);
					}
				}
			}
		}

		class DummyDataRowSource : IDataRowSource
		{
			public int RowCount => 3;

			public IDataRowSource Filter(string expressions) => null;

			public IDataRowSource GetFirstNRows(int n) => null;

			public IDataRowSource GetRowsFromIndexes(int[] indexes) => null;

			public IDataRowSource[] GroupBy(string[] columnNames) => null;

			public int GroupCount(string[] columnNames) => 0;

			public IDataRowSource Split(int rowsToKeep) => null;
		}

		public void TestSpecificPageRangesHidden_MultiSectionBodyAreas()
		{
			using (DocumentsDataRegistry.Instance.EnableSpecificPageRangesPrintingOption.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var documentCommand = Factory.New<DocumentCommand>();
				documentCommand.SU_BusinessContext = nameof(BusinessContext.Test);
				var reportTempate = Factory.New<StmTemplateBase>();
				var template1 = DocumentEngineTestHelper.CreateTemplateFromString(
					@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.ZStringProperty>]
{B}-[<Collection.CollectionOwnProperty>]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.ZStringProperty>]
{B}-[<Collection.CollectionOwnProperty>]
{A}-[#EndOfReport]");

				reportTempate.SO_Template = template1;
				reportTempate.SO_DataContext = "GenericFreightJob";
				var pivot1 = documentCommand.Documents.AddNew();
				pivot1.SI_DocumentTitle = "Test Document1";
				pivot1.SI_SU = documentCommand.PK;
				pivot1.SI_SO = reportTempate.PK;
				pivot1.DocConfigs.AddNew();

				Factory.Save();

				var docSupportedBO = Factory.New<DummyBODocSupportable>();
				documentCommand.Parent = docSupportedBO;
				using (var printTask = new DocumentPrintSet(documentCommand))
				using (var documentPack = new DocumentPack(documentCommand, docSupportedBO, null, null))
				{
					printTask.Add(documentPack);
					var instructions = new DeliveryInstructions(documentPack);
					using (var form = new MockDocDeliveryForm(printTask, instructions))
					{
						AssertEquals(1, printTask.GetDocumentPacks().Count());
						AssertEquals(1, printTask.GetFirstDocumentPack().OfType<Report>().Count());
						var report = printTask.GetFirstDocumentPack().OfType<Report>().First();
						AssertEquals(1, report.XlInterface.SheetCount);
						AssertEquals(false, report.CanSpecifyPageRanges);
						form.Show();
						Assert("SpecifiedPageRangesCheckBox should not be visible", !form.PageRangesSpecifiedCheckBox.Visible);
					}
				}
			}
		}

		public void TestSpecificPageRanges()
		{
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.SU_BusinessContext = "Shipment";
			var reportTempate = Factory.New<StmTemplateBase>();
			var template1 = DocumentEngineTestHelper.CreateTemplateFromString(
				@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.ZStringProperty>]
{B}-[<Collection.CollectionOwnProperty>]
{A}-[#EndOfReport]");

			reportTempate.SO_Template = template1;
			reportTempate.SO_DataContext = "GenericFreightJob";
			var pivot1 = Factory.New<StmMenuTemplatePivotBase>();
			pivot1.SI_DocumentTitle = "Test Document1";
			pivot1.SI_SU = documentCommand.PK;
			pivot1.SI_SO = reportTempate.PK;

			Factory.Save();

			var docSupportedBO = Factory.New<DummyBODocSupportable>();
			docSupportedBO.Collection.AddNew();
			docSupportedBO.Collection.AddNew();
			using (var printTask = new DocumentPrintSet(documentCommand))
			using (var documentPack = new DocumentPack(documentCommand, docSupportedBO, null, null))
			{
				printTask.Add(documentPack);
				var instructions = new DeliveryInstructions(documentPack);

				Assert("registry default value should be false.", !DocumentsDataRegistry.Instance.EnableSpecificPageRangesPrintingOption.Value);
				using (var form = new MockDocDeliveryForm(printTask, instructions))
				{
					form.Show();
					Assert("SpecifiedPageRangesCheckBox should not be visible as the registry is false", !form.PageRangesSpecifiedCheckBox.Visible);
				}

				using (DocumentsDataRegistry.Instance.EnableSpecificPageRangesPrintingOption.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (var form = new MockDocDeliveryForm(printTask, instructions))
				{
					AssertEquals(1, printTask.GetDocumentPacks().Count());
					AssertEquals(1, printTask.GetFirstDocumentPack().OfType<Report>().Count());
					var report = printTask.GetFirstDocumentPack().OfType<Report>().First();
					AssertEquals(1, report.XlInterface.SheetCount);
					AssertEquals(true, printTask.GetFirstDocumentPack().OfType<Report>().First().CanSpecifyPageRanges);
					form.Show();
					Assert("SpecifiedPageRangesCheckBox should be visible", form.PageRangesSpecifiedCheckBox.Visible);
					AssertEquals(2, instructions.DataSourceRowCountIfPageRangesSpecifiable);

					Assert(!form.PageRangesTextBox.Enabled);
					Assert(!instructions.PageRangesSpecified);

					form.PageRangesSpecifiedCheckBox.Checked = true;
					Assert(form.PageRangesTextBox.Enabled);
					Assert(instructions.PageRangesSpecified);

					form.PageRangesTextBox.Focus();
					form.PageRangesTextBox.Text = "1,2";
					form.PageRangesSpecifiedCheckBox.Focus();
					System.Windows.Forms.Application.DoEvents();
					AssertEquals("1,2", instructions.SpecifiedPageRangesText);
					Assert("specific page ranges text is valid.", !instructions.SpecifiedPageRangesTextInfo.HasErrors());

					form.PageRangesTextBox.Focus();
					form.PageRangesTextBox.Text = "1,2,3";
					form.PageRangesSpecifiedCheckBox.Focus();
					System.Windows.Forms.Application.DoEvents();
					AssertEquals("1,2,3", instructions.SpecifiedPageRangesText);
					Assert("specific page ranges text is invalid.", instructions.SpecifiedPageRangesTextInfo.HasErrors());
				}
			}
		}

		[GuiTest]
		public void TestDeliveryDocumentInPreviewForm_DeliveryMethodIsChangedButNotTriggered()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Translate Legacy Document",
				@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[TranslateLegacyDocument]
{A}-[#EndOfReport]", ".DummyBODocSupportable");

			var dummy = Factory.New<DummyBODocSupportable>();
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;
			var pivot = documentCommand.Documents.AddNew();
			pivot.SI_SU = documentCommand.PK;
			pivot.SI_SO = template.PK;

			var printer = Factory.NewWithValidTestData<StmPrintQueue>();
			printer.SQ_DisplayName = "Test Printer";
			printer.SQ_QueueName = "Test Printer";
			printer.SQ_ServerName = "LocalHost";

			Factory.Save();

			using (var documentPack = new DocumentPack(documentCommand, dummy, null, null))
			{
				var deliveryInstructions = new DeliveryInstructions(documentPack);
				deliveryInstructions.AllowAutoDelivery = false;
				deliveryInstructions.PrinterDelivery.PrintQueuePK = printer.PK;
				var printTask = new PrintTask();
				printTask.Add(documentPack);
				using (var form = new MockDocDeliveryForm(printTask, deliveryInstructions))
				{
					form.Show();
					form.PreviewButton.PerformClick();
					System.Windows.Forms.Application.DoEvents();

					var previewForms = ZApplication.GetOpenForms().OfType<XLSPreviewForm>();
					AssertEquals(1, previewForms.Count());
					var previewForm = previewForms.First();

					var columnIndex = form.RecipientsGrid.Columns.IndexOf(col => col.ColumnName == "DeliveryMethodDescription");
					var column = form.RecipientsGrid.Columns[columnIndex];
					form.RecipientsGrid.CurrentCell = new DataGridCell(0, columnIndex);
					form.RecipientsGrid.BeginEdit(column.ColumnStyle, 0);
					(form.RecipientsGrid.LastFocusedColumn.EditControl as ZGridDropEdit).Text = "E-Mail";
					System.Windows.Forms.Application.DoEvents();

					previewForm.DeliverButton.PerformClick();

					AssertEquals("there should be error.", "There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(DialogResult.None, form.DialogResult);
				}
			}
		}

		public void TestDeliverFormBeMarkedForActivate_WhenCloseXLSPreviewForm()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Translate Legacy Document",
				@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[TranslateLegacyDocument]
{A}-[#EndOfReport]", ".DummyBODocSupportable");
			var dummy = Factory.New<DummyBODocSupportable>();
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;
			var pivot = documentCommand.Documents.AddNew();
			pivot.SI_SU = documentCommand.PK;
			pivot.SI_SO = template.PK;

			var printer = Factory.NewWithValidTestData<StmPrintQueue>();
			printer.SQ_DisplayName = "Test Printer";
			printer.SQ_QueueName = "Test Printer";
			printer.SQ_ServerName = "LocalHost";

			Factory.Save();

			using (var documentPack = new DocumentPack(documentCommand, dummy, null, null))
			{
				var deliveryInstructions = new DeliveryInstructions(documentPack);
				deliveryInstructions.AllowAutoDelivery = false;
				deliveryInstructions.PrinterDelivery.PrintQueuePK = printer.PK;
				var printTask = new PrintTask();
				printTask.Add(documentPack);
				using (var form = new MockDocDeliveryForm(printTask, deliveryInstructions))
				{
					form.Show();
					ZFormModaliser.ShowDialogsInTest = true;
					form.PreviewButton.PerformClick();
					var previewForms = ZApplication.GetOpenForms().OfType<XLSPreviewForm>();
					var previewForm = previewForms.First();

					AssertEquals("DocDeliveryForm", (previewForm.DeliverForm as Form).Name);

					previewForm.Close();
				}
			}
		}

		public void TestXLSPreviewFormNotBeClosed_WhenCloseDocDeliveryForm()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Translate Legacy Document",
				@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[TranslateLegacyDocument]
{A}-[#EndOfReport]", ".DummyBODocSupportable");
			var dummy = Factory.New<DummyBODocSupportable>();
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;
			var pivot = documentCommand.Documents.AddNew();
			pivot.SI_SU = documentCommand.PK;
			pivot.SI_SO = template.PK;

			var printer = Factory.NewWithValidTestData<StmPrintQueue>();
			printer.SQ_DisplayName = "Test Printer";
			printer.SQ_QueueName = "Test Printer";
			printer.SQ_ServerName = "LocalHost";

			Factory.Save();

			using (var documentPack = new DocumentPack(documentCommand, dummy, null, null))
			{
				var deliveryInstructions = new DeliveryInstructions(documentPack);
				deliveryInstructions.AllowAutoDelivery = false;
				deliveryInstructions.PrinterDelivery.PrintQueuePK = printer.PK;
				var printTask = new PrintTask();
				printTask.Add(documentPack);
				using (var form = new MockDocDeliveryForm(printTask, deliveryInstructions))
				{
					form.Show();
					ZFormModaliser.ShowDialogsInTest = true;
					form.PreviewButton.PerformClick();
					form.Close();
					var previewForms = ZApplication.GetOpenForms().OfType<XLSPreviewForm>();

					AssertEquals(1, previewForms.Count());

					previewForms.First().Close();
				}
			}
		}

		public void TestIncludedEDocsTabPageShouldNotVisible_DeliveryDocumentWithMultipleDocumentPacks()
		{
			var instructions = new MockDocumentDeliveryInstructions();
			instructions.DocumentPackCount = 4;

			using (var form = new MockDocDeliveryForm(instructions))
			{
				form.Show();

				Assert("Multi doc pack groupbox shown", form.MultiDocPackGroupbox.Visible);
				Assert("Included EDocs Tab page hidden", !form.IncludedEDocsTabPage.TabVisible);
			}

			instructions.DocumentPackCount = 1;

			using (var form = new MockDocDeliveryForm(instructions))
			{
				form.Show();

				Assert("Multi doc pack groupbox hidden", !form.MultiDocPackGroupbox.Visible);
				Assert("Included EDocs Tab page shown", form.IncludedEDocsTabPage.TabVisible);
			}
		}

		public void TestIncludedEDocsTabPageShouldVisible_DeliveryDocument()
		{
			var documentCommand = Factory.NewWithValidTestData<DocumentCommand>();
			using (var documentPack = new DocumentPack(documentCommand))
			using (var printTask = new PrintTask(documentCommand))
			{
				var deliveryInstructions = new MockDocumentDeliveryInstructions(documentPack);
				using (var form = new MockDocDeliveryForm(printTask, deliveryInstructions))
				{
					Assert("IncludedEDocsTabPage should be visible", form.IncludedEDocsTabPage.TabVisible);
				}
			}
		}

		public void TestIncludedEDocsTabPageShouldNotVisible_DeliveryReport()
		{
			var reportCommand = Factory.New<ReportCommand>();
			var reportTempate = Factory.New<StmTemplateBase>();
			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_DocumentTitle = "Test Document";
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = reportTempate.PK;

			Factory.Save();

			using (var reportPrintTask = new ReportPrintSet(reportCommand))
			using (var reportPack = new DocumentPack(reportCommand))
			{
				var reportDeliveryInstructions = new MockDocumentDeliveryInstructions(reportPack);
				using (var form = new MockDocDeliveryForm(reportPrintTask, reportDeliveryInstructions))
				{
					Assert("IncludedEDocsTabPage should not be visible", !form.IncludedEDocsTabPage.TabVisible);
				}
			}
		}

		public void TestDeliveryEmailSubjectMacroRelatedObjectsContainsMostTopObjectFromPrintTask()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Z0_Code = "ABC";
			var command = Factory.NewWithValidTestData<ReportCommand>();
			var printTask = new PrintTask(command);
			printTask.MostTopLevelBusinessObject = dummy;
			var pack = new DocumentPack(command);
			printTask.Add(pack);
			var instructions = new DeliveryInstructions(pack);

			using (var form = new MockDocDeliveryForm(instructions))
			{
				form.changeCell = true;
				form.Show();
				System.Windows.Forms.Application.DoEvents();

				var macroColumn = form.RecipientsGrid.ColumnStyles.OfType<ZMacrosFindBoxColumnStyleInfo>().First();
				AssertNotNull(macroColumn);
				AssertNotNull(macroColumn.Roots);
				Assert(macroColumn.RootTypes.Contains(typeof(DummyBusinessObject)));
			}
		}

		public void TestRecipientsGridContainsMacrosColumn()
		{
			var instructions = new DeliveryInstructions();
			instructions.Recipients.RemoveAndDeleteAll();

			using (var form = new MockDocDeliveryForm(instructions))
			{
				form.changeCell = true;
				form.Show();
				System.Windows.Forms.Application.DoEvents();

				var macroColumn = form.RecipientsGrid.ColumnStyles.OfType<ZMacrosFindBoxColumnStyleInfo>().First();
				AssertNotNull(macroColumn);
				AssertNotNull(macroColumn.Roots);
			}
		}

		[ExpectNoExceptions]
		public void TestListMangerPositionNotEqualsToRowNumber()
		{
			using (Env.CurrentUser.SetUserEmailAddressInTESTINGOnly("test@test.com"))
			{
				DeliveryInstructions instructions = new DeliveryInstructions();
				instructions.Recipients.RemoveAndDeleteAll();
				DocDeliveryContact contact = instructions.Recipients.AddNew();
				contact.DeliveryMethod = "EML";

				using (MockDocDeliveryForm form = new MockDocDeliveryForm(instructions))
				{
					form.changeCell = true;
					form.Show();
					DataGridTextBoxColumn columnStyle = (DataGridTextBoxColumn)form.RecipientsGrid.Columns["DeliveryAddress"].ColumnStyle;
					Message msg = new Message();
					form.RecipientsGrid.CurrentCell = new DataGridCell(0, form.RecipientsGrid.TableStyles[0].GridColumnStyles.IndexOf(columnStyle));
					form.ProcessCmdKey(ref msg, Keys.Control | Keys.E);
				}
			}
		}

		public void TestGetErrorMessageForNonVisualisableMenuItem()
		{
			ReportCommand reportCommand = Factory.NewWithValidTestData<ReportCommand>();
			Factory.Save();
			DeliveryInstructions instructions = new DeliveryInstructions(new DocumentPack(reportCommand));
			using (DocDeliveryForm deliveryForm = new DocDeliveryForm(instructions, Env.Security.None))
			{
				AssertEquals("Only documents can have overriding data.", deliveryForm.GetErrorMessageForNonVisualisableMenuItem());
			}

			DocumentsDataRegistry.Instance.AllowDocumentsToBeModified.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			DocumentCommand documentCommand = Factory.NewWithValidTestData<DocumentCommand>();
			Factory.Save();
			instructions = new DeliveryInstructions(new DocumentPack(documentCommand));
			using (DocDeliveryForm deliveryForm = new DocDeliveryForm(instructions, Env.Security.None))
			{
				AssertEquals($"This feature allows users to modify the values on this document. You do not currently have access to use this feature because this feature is disabled in your system. To enable it, please ask your system administrator to turn on the Registry item 'Documents -> Allow Documents To Be Modified'. You will need to log out and log back into {BrandingFactory.Instance.ProductName} once this registry value has been changed.", deliveryForm.GetErrorMessageForNonVisualisableMenuItem());
			}

			DocumentsDataRegistry.Instance.AllowDocumentsToBeModified.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			documentCommand = Factory.NewWithValidTestData<DocumentCommand>();
			documentCommand.SU_SupportsVisualisation = true;
			documentCommand.SU_IsModifiable = false;
			Factory.Save();
			instructions = new DeliveryInstructions(new DocumentPack(documentCommand));
			using (DocDeliveryForm deliveryForm = new DocDeliveryForm(instructions, Env.Security.None))
			{
				AssertEquals("This feature allows users to modify the values on the select document. However, you cannot modify this particular document, because your system admin has restricted modifying this document.", deliveryForm.GetErrorMessageForNonVisualisableMenuItem());
			}

			DocumentsDataRegistry.Instance.AllowDocumentsToBeModified.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			documentCommand = Factory.NewWithValidTestData<DocumentCommand>();
			documentCommand.SU_SupportsVisualisation = true;
			documentCommand.SU_IsModifiable = true;
			documentCommand.SU_MenuPath = "TestPath";
			documentCommand.SU_MenuName = "Test Name";
			Factory.Save();
			instructions = new DeliveryInstructions(new DocumentPack(documentCommand));
			Enterprise.ZArchitecture.Modules.ZController controller = ZControllerFactory.Create(ControllerIDs.AccBankAccount);
			SecurityCheckpoint checkpoint = Env.Security.FindOrCreateDocumentOverrideCheckpoint(documentCommand.PK.ToGuid(), documentCommand.SU_MenuNameMultilingual, ModuleIDs.Customs.JobDeclaration, Env.Security.CustomsDeclarationEnquiry);
			checkpoint.IsAllowed = false;
			using (DocDeliveryForm deliveryForm = new DocDeliveryForm(instructions, checkpoint))
			{
				AssertEquals(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Operate -> Customs -> Customs Declarations -> TestPath -- Test Name", deliveryForm.GetErrorMessageForNonVisualisableMenuItem());
			}

			DocumentsDataRegistry.Instance.AllowDocumentsToBeModified.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			documentCommand = Factory.NewWithValidTestData<DocumentCommand>();
			documentCommand.SU_SupportsVisualisation = true;
			documentCommand.SU_IsModifiable = true;
			Factory.Save();
			instructions = new DeliveryInstructions(new DocumentPack(documentCommand));
			controller = ZControllerFactory.Create(ControllerIDs.AccBankAccount);
			checkpoint = Env.Security.FindOrCreateDocumentOverrideCheckpoint(documentCommand.PK.ToGuid(), documentCommand.SU_MenuNameMultilingual, ModuleIDs.Customs.JobDeclaration, Env.Security.CustomsDeclarationEnquiry);
			checkpoint.IsAllowed = true;
			using (DocDeliveryForm deliveryForm = new DocDeliveryForm(instructions, checkpoint))
			{
				AssertEquals("", deliveryForm.GetErrorMessageForNonVisualisableMenuItem());
			}
		}

		public void TestLanguageSelection()
		{
			// Create only Reports => LanguageZDropEdit.Visible = false
			var instructions = DocDeliveryFormTestHelper.CreateInstructions(Factory);
			using (var form = new MockDocDeliveryForm(instructions))
			{
				form.Show();
				Assert("Language selection should not be shown for reports.", !form.LanguageZDropEdit.Visible);
			}

			// Create a non DocBuilderStyle Doc => LanguageZDropEdit.Visible = false
			var docSupportedBO = Factory.New<DummyBODocSupportable>();
			var command = Factory.NewWithValidTestData<DocumentCommand>();
			var templateRecord = Factory.New<StmTemplateBase>();
			templateRecord.SO_DataContext = ".DummyBODocSupportable";
			var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.DocumentTestFiles.CustomisableSectionTest.xls", "CustomisableSectionTest.xls");
			var excelTemplate = new ExcelTemplateForUnitTesting("CustomisableSectionTest.xls", Path.GetFullPath(tempFileName));
			templateRecord.SO_Template = excelTemplate.GetAsByteArray();
			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SO = templateRecord.PK;
			pivot.SI_SU = command.PK;
			var docPack = new DocumentPack(command, docSupportedBO, null, null);
			instructions = new DeliveryInstructions(docPack);
			using (var form = new MockDocDeliveryForm(instructions))
			{
				form.Show();
				Assert("Language selection should not be visible for non docBuilder style documents.", !form.LanguageZDropEdit.Visible);
			}

			// Create DocBuilder System specific => LanguageZDropEdit.Visible = true
			command = Factory.NewWithValidTestData<DocumentCommand>();
			var systemSectionsTemplate = StmTemplateBase.GetDocBuilderTemplate(Factory, DocBuilderTemplateType.System);
			systemSectionsTemplate.SO_DataContext = ".DummyBODocSupportable";
			systemSectionsTemplate.SO_Template = excelTemplate.GetAsByteArray();
			pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SO = systemSectionsTemplate.PK;
			pivot.SI_SU = command.PK;
			docPack = new DocumentPack(command, docSupportedBO, null, null);
			instructions = new DeliveryInstructions(docPack);
			using (var form = new MockDocDeliveryForm(instructions))
			{
				form.Show();
				Assert("Language selection should be visible for docBuilder style documents.", form.LanguageZDropEdit.Visible);
			}
		}

		public void TestIsDocument_DocumentCommand()
		{
			var documentCommand = Factory.NewWithValidTestData<DocumentCommand>();
			var documentPack = new DocumentPack(documentCommand);
			var printTask = new PrintTask(documentCommand);
			var deliveryInstructions = new DeliveryInstructions(documentPack);

			using (var form = new MockDocDeliveryForm(printTask, deliveryInstructions))
			{
				Assert("IsDocument is true when delivering via a document command and not delivering a form ", form.IsDocument);
				Assert("IsDocument and IsForm are mutually exclusive", !form.IsForm);
			}
		}

		public void TestIsForm_DocumentCommand()
		{
			var documentCommand = Factory.NewWithValidTestData<DocumentCommand>();
			var documentPack = new DocumentPack(documentCommand);
			var printTask = new PrintTask(documentCommand);
			var formDeliveryInstructions = new MockDocumentDeliveryInstructions(documentPack);

			using (var form = new MockDocDeliveryForm(printTask, formDeliveryInstructions))
			{
				Assert("IsForm is true when delivery instructions is delivering form via a document command", form.IsForm);
				Assert("IsForm and IsDocument are mutually exclusive", !form.IsDocument);
			}
		}

		public void TestIsForm_StmMenuItem()
		{
			var genericCommand = Factory.NewWithValidTestData<StmMenuItem>();
			var documentPack = new DocumentPack(genericCommand);
			var printTask = new PrintTask(genericCommand);
			var formDeliveryInstructions = new MockDocumentDeliveryInstructions(documentPack);

			using (var form = new MockDocDeliveryForm(printTask, formDeliveryInstructions))
			{
				Assert("IsForm is true when delivery instructions is delivering form via a generic menuitem", form.IsForm);
				Assert("IsForm and IsDocument are mutually exclusive", !form.IsDocument);
			}
		}

		#region Preview

		public void TestPreview_S3ExceptionIsCaught()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			Env.Security.PreviewReportButton.IsAllowed = true;
			var command = Factory.NewWithValidTestData<ReportCommand>();
			var printTask = new PrintTask(command);

			using (var templateStream = new MemoryStream())
			using (var creationExcelInterface = new ExcelInterface())
			using (var docPack = new DocumentPack(command))
			{
				creationExcelInterface.NewExcelFile(1);
				var workSheet = creationExcelInterface.WorkSheets[0];
				workSheet[0, 0] = "#config";
				workSheet[1, 0] = "Name=TemplateFromStream";
				workSheet[2, 0] = "PageStyle=Portrait";
				workSheet[3, 0] = "#SectionBody";
				workSheet[4, 1] = "Blah";
				workSheet[5, 0] = "#EndOfReport";
				creationExcelInterface.SaveToStream(templateStream);

				var excelTemplate = new ExcelTemplateWrappingStream("TemplateFromStream", templateStream);

				using (var report = new Report(docPack, excelTemplate, new MyDummyDocWrapper(), "TestPreviewButtonIsVisibleWhenParentStmMenuCommandIsNotSet", null, DocumentDirection.ANY, false))
				{
					var instructions = new DeliveryInstructions(docPack) { DocumentPackCount = 1, AllowPreview = true };
					var provider = ObjectFactory.Get<IDocumentFactoryProvider>();
					var documentFactory = provider.GetFactory(Factory) as BusinessObjectFactory;
					var deliverable = documentFactory.New<IStorageFile>() as BusinessObject;
					deliverable[StorageDocsSchema.SC_SM.Name] = ZGuid.NewZGuid();
					documentFactory.Save();

					docPack.Add(deliverable);
					printTask.Add(docPack);
					instructions.DeliverablesToBePrinted.Add(report);
					report.IncludedInPrint = true;

					using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.S3))
					using (SystemDataRegistry.Instance.DocManagerStorageBucketName.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "test"))
					using (SystemDataRegistry.Instance.EDocsStorageServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "s3.wtg.zone"))
					using (var form = new MockDocDeliveryForm(printTask, instructions))
					{
						form.PreviewRequested += Form_PreviewRequested;
						form.Show();
						AssertNoExceptionThrown(form.PreviewButton.PerformClick);
						AssertEquals("Warning displayed regarding S3 exception", true, UnitTestUserNotification.Instance.LastMessage.Text.StartsWith($"Unable to access {Core.Constants.EDocsStorageProviders.Code.S3} storage, please contact your system administrator to check the configuration of the eDocs storage"));
					}
				}
			}
		}

		public void TestPreviewMultiDocPack()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			var command = Factory.NewWithValidTestData<ReportCommand>();
			var printTask = new PrintTask(command);
			var pack = new DocumentPack(command);
			var instructions = new DeliveryInstructions(pack);
			instructions.DeliveryOptions = AllowedDeliveryOptions.All;
			Env.Security.PreviewReportButton.IsAllowed = true;

			instructions.DocumentPackCount = 3;
			PrintPreviewRequestedEventFired = false;

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			using (MockDocDeliveryForm form = new MockDocDeliveryForm(printTask, instructions))
			{
				form.PreviewRequested += new PrintTask.PreviewRequestedEventHandler(Form_PreviewRequested);
				form.Show();
				Assert("Precondition: Print Preview event not yet fired", !PrintPreviewRequestedEventFired);
				AssertEquals("Precondition: Cancel button text is Cancel", "&Cancel", form.CloseButton.Text);
				form.PreviewButton.PerformClick();
				AssertEquals("Warning displayed regarding multi doc pack preview", "There is more than one document to preview. Would you like to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Print Preview event fired", PrintPreviewRequestedEventFired);
				PrintPreviewRequestedEventFired = false;
				Assert("Dialog Still Open", form.Visible);
				AssertEquals("Cancel button changed to Close", "Close", form.CloseButton.Text);
			}

			instructions = new DeliveryInstructions(pack);
			instructions.DocumentPackCount = 3;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);

			using (MockDocDeliveryForm form = new MockDocDeliveryForm(printTask, instructions))
			{
				form.Show();
				AssertEquals("Precondition: Cancel button text is Cancel", "&Cancel", form.CloseButton.Text);
				form.PreviewButton.PerformClick();
				AssertEquals("Warning displayed regarding multi doc pack preview", "There is more than one document to preview. Would you like to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Dialog not OK'd", form.DialogResult != DialogResult.OK);
				Assert("Destination not set to Preview", instructions.Destination != DeliveryInstructionDestination.Preview);
				AssertEquals("Cancel button text is STILL Cancel", "&Cancel", form.CloseButton.Text);
			}
		}

		bool PrintPreviewRequestedEventFired;

		void Form_PreviewRequested(object sender, DeliveryInstructions instructions)
		{
			PrintPreviewRequestedEventFired = true;
		}

		public void TestPreviewNotAllowed()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			var command = Factory.NewWithValidTestData<ReportCommand>();
			var printTask = new PrintTask(command);
			var pack = new DocumentPack(command);
			var instructions = new DeliveryInstructions(pack);
			Env.Security.PreviewReportButton.IsAllowed = true;

			instructions.DocumentPackCount = 1;
			instructions.AllowPreview = false;

			using (MockDocDeliveryForm form = new MockDocDeliveryForm(printTask, instructions))
			{
				form.Show();
				form.PreviewButton.PerformClick();
				AssertEquals("Error displayed regarding no preview allowed", "Cannot preview this many documents. Please print to view them.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Dialog not OK'd", form.DialogResult != DialogResult.OK);
				Assert("Destination not set to Preview", instructions.Destination != DeliveryInstructionDestination.Preview);
				AssertEquals("Cancel button text is STILL Cancel", "&Cancel", form.CloseButton.Text);
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			instructions.AllowPreview = true;
			using (MockDocDeliveryForm form = new MockDocDeliveryForm(printTask, instructions))
			{
				form.PreviewRequested += new PrintTask.PreviewRequestedEventHandler(Form_PreviewRequested);
				form.Show();
				form.PreviewButton.PerformClick();
				AssertEquals("Message displayed regarding no documents to preview.", "There are no documents to preview.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (Report testReport = new Report(new DocumentPack(), null))
			{
				instructions.DeliverablesToBePrinted.Add(testReport);
				testReport.IncludedInPrint = true;
				using (MockDocDeliveryForm form = new MockDocDeliveryForm(printTask, instructions))
				{
					form.PreviewRequested += new PrintTask.PreviewRequestedEventHandler(Form_PreviewRequested);
					form.Show();
					form.PreviewButton.PerformClick();
					AssertNull("No Error displayed regarding no preview allowed", UnitTestUserNotification.Instance.LastMessage.Text);
					Assert("Print Preview event fired", PrintPreviewRequestedEventFired);
					AssertEquals("Cancel button text is Close", "Close", form.CloseButton.Text);
					PrintPreviewRequestedEventFired = false;
				}
			}
		}

		public void TestPreviewButtonVisibility()
		{
			var command = Factory.NewWithValidTestData<ReportCommand>();
			var printTask = new PrintTask(command);
			var pack = new DocumentPack(command);
			var instructions = new DeliveryInstructions(pack);
			Env.Security.PreviewReportButton.IsAllowed = true;

			instructions.DeliveryOptions = AllowedDeliveryOptions.AllExceptPreview;
			using (MockDocDeliveryForm form = new MockDocDeliveryForm(printTask, instructions))
			{
				form.Show();
				Assert("Preview Button not visible", !form.PreviewButton.Visible);
				Assert("Send Button is visible", form.DeliverButton.Visible);
			}

			instructions.DeliveryOptions = AllowedDeliveryOptions.All;
			using (MockDocDeliveryForm form = new MockDocDeliveryForm(printTask, instructions))
			{
				form.Show();
				Assert("Preview Button is visible", form.PreviewButton.Visible);
				Assert("Send Button is visible", form.DeliverButton.Visible);
			}
		}

		public void TestPreviewButtonVisibilityRespectsSecuritySetting()
		{
			var command = Factory.NewWithValidTestData<ReportCommand>();
			var printTask = new PrintTask(command);
			var pack = new DocumentPack(command);
			var instructions = new DeliveryInstructions(pack);
			instructions.DeliveryOptions = AllowedDeliveryOptions.All;

			Env.Security.PreviewReportButton.IsAllowed = true;
			using (var form = new MockDocDeliveryForm(printTask, instructions))
			{
				form.Show();
				Assert("Preview Button is visible", form.PreviewButton.Visible);
			}

			Env.Security.PreviewReportButton.IsAllowed = false;
			using (var form = new MockDocDeliveryForm(printTask, instructions))
			{
				form.Show();
				Assert("Preview Button is not visible", !form.PreviewButton.Visible);
			}

			var command2 = Factory.NewWithValidTestData<DocumentCommand>();
			printTask = new PrintTask(command2);
			pack = new DocumentPack(command2);
			instructions = new DeliveryInstructions(pack);
			instructions.DeliveryOptions = AllowedDeliveryOptions.All;

			Env.Security.PreviewDocumentButton.IsAllowed = true;
			using (var form = new MockDocDeliveryForm(printTask, instructions))
			{
				form.Show();
				Assert("Preview Button is visible", form.PreviewButton.Visible);
			}

			Env.Security.PreviewDocumentButton.IsAllowed = false;
			using (var form = new MockDocDeliveryForm(printTask, instructions))
			{
				form.Show();
				Assert("Preview Button is not visible", !form.PreviewButton.Visible);
			}
		}

		public void TestPreviewButtonIsVisibleWhenDeliveringForms()
		{
			var command = Factory.NewWithValidTestData<StmMenuItem>();
			command.SU_MenuType = "FRM";
			var printTask = new PrintTask(command);
			var instructions = new DeliveryInstructionForForms();
			instructions.DeliveryOptions = AllowedDeliveryOptions.All;

			Env.Security.PreviewReportButton.IsAllowed = false;
			Env.Security.PreviewDocumentButton.IsAllowed = false;
			using (var form = new MockDocDeliveryForm(printTask, instructions))
			{
				form.Show();
				Assert("Preview Button is visible", form.PreviewButton.Visible);
			}
		}

		[Serializable]
		class DeliveryInstructionForForms : DeliveryInstructions
		{
			public DeliveryInstructionForForms()
			{ }

			public override ZBool IsDeliveringFormDocument => true;
		}

		public void TestPreviewButtonIsVisibleWhenDeliveringDocumentPack()
		{
			var printTask = new PrintTask();
			var instructions = new DeliveryInstructions();
			instructions.DeliveryOptions = AllowedDeliveryOptions.All;
			instructions.DocumentPackCount = 2;

			Env.Security.PreviewDocumentButton.IsAllowed = true;
			using (var form = new MockDocDeliveryForm(printTask, instructions))
			{
				form.Show();
				Assert("Preview Button is visible", form.PreviewButton.Visible);
			}
		}

		public void TestPreviewButtonIsVisibleWhenParentStmMenuCommandIsNotSet()
		{
			Env.Security.PreviewDocumentButton.IsAllowed = true;

			using (var templateStream = new MemoryStream())
			using (var creationExcelInterface = new ExcelInterface())
			{
				creationExcelInterface.NewExcelFile(1);
				var workSheet = creationExcelInterface.WorkSheets[0];
				workSheet[0, 0] = "#config";
				workSheet[1, 0] = "Name=TemplateFromStream";
				workSheet[2, 0] = "PageStyle=Portrait";
				workSheet[3, 0] = "#SectionBody";
				workSheet[4, 1] = "Blah";
				workSheet[5, 0] = "#EndOfReport";
				creationExcelInterface.SaveToStream(templateStream);

				var excelTemplate = new ExcelTemplateWrappingStream("TemplateFromStream", templateStream);
				var docPack = new DocumentPack();
				var report = new Report(docPack, excelTemplate, new MyDummyDocWrapper(), "TestPreviewButtonIsVisibleWhenParentStmMenuCommandIsNotSet", null, DocumentDirection.ANY, false);
				docPack.Add(report);

				var instructions = new DeliveryInstructions(docPack);
				instructions.DeliveryOptions = AllowedDeliveryOptions.All;

				using (var form = new MockDocDeliveryForm(instructions))
				{
					form.Show();
					Assert("Preview Button is visible", form.PreviewButton.Visible);
				}
			}
		}

		public void TestBackgroundDeliveryCheckBoxVisibility()
		{
			var command = Factory.NewWithValidTestData<ReportCommand>();
			var printTask = new PrintTask(command);
			var pack = new DocumentPack(command);
			var instructions = new DeliveryInstructions(pack);
			instructions.DeliveryOptions = AllowedDeliveryOptions.All;
			using (var form = new MockDocDeliveryForm(printTask, instructions))
			{
				form.Show();
				Assert("BackgroundDelivery CheckBox is visible for reports", form.BackgroundDeliveryCheckBox.Visible);
				AssertEquals("BackgroundDeliveryCheckBox Caption should be Schedule", "Schedule", form.BackgroundDeliveryCheckBox.Text);
			}

			var command2 = Factory.NewWithValidTestData<DocumentCommand>();
			printTask = new PrintTask(command2);
			pack = new DocumentPack(command2);
			instructions = new DeliveryInstructions(pack);
			instructions.DeliveryOptions = AllowedDeliveryOptions.All;
			using (var form = new MockDocDeliveryForm(printTask, instructions))
			{
				form.Show();
				Assert("BackgroundDelivery CheckBox is not visible for documents when it is not from menu", !form.BackgroundDeliveryCheckBox.Visible);
			}

			using (command2.FromMenu())
			using (var form = new MockDocDeliveryForm(printTask, instructions))
			{
				form.Show();
				Assert("BackgroundDelivery CheckBox is visible for documents when it is from menu", form.BackgroundDeliveryCheckBox.Visible);
			}

			printTask = new PrintTask();
			instructions = new DeliveryInstructions();
			instructions.DeliveryOptions = AllowedDeliveryOptions.All;
			instructions.DocumentPackCount = 2;
			using (var form = new MockDocDeliveryForm(printTask, instructions))
			{
				form.Show();
				Assert("BackgroundDelivery CheckBox is not visible for document pack", !form.BackgroundDeliveryCheckBox.Visible);
			}

			var command3 = Factory.NewWithValidTestData<StmMenuItem>();
			command3.SU_MenuType = "FRM";
			printTask = new PrintTask(command3);
			instructions = new DeliveryInstructionForForms();
			instructions.DeliveryOptions = AllowedDeliveryOptions.All;
			using (var form = new MockDocDeliveryForm(printTask, instructions))
			{
				form.Show();
				Assert("BackgroundDelivery CheckBox is not visible for forms", !form.BackgroundDeliveryCheckBox.Visible);
			}
		}

		public void TestBackgroundDeliveryCheckBoxVisibilityIfIDelivarableNotUnique()
		{
			using (Env.SetTemporaryUserContext(User.SupportUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				Assert("Shoud be Supprot User ", Env.Instance.CurrentUser.IsSupportUser);

				var command2 = Factory.NewWithValidTestData<DocumentCommand>();
				var printTask = new PrintTask(command2);
				var pack = new DocumentPack(command2);
				var instructions = new DeliveryInstructions(pack);
				instructions.DeliverablesToBePrinted.Add(new Report(pack, null));
				instructions.DeliverablesToBePrinted.Add(new Report(pack, null));

				instructions.DeliveryOptions = AllowedDeliveryOptions.All;
				using (var form = new MockDocDeliveryForm(printTask, instructions))
				{
					form.Show();
					Assert("BackgroundDelivery CheckBox is not visible", !form.BackgroundDeliveryCheckBox.Visible);
				}
			}
		}
		public void TestBackgroundDeliveryCheckBoxVisibilityIfDocumentReferenceGuideOrReportReferenceGuide()
		{
			var menuItemName = "Document Engine Reference Guide";
			var filterForMenuItem = new DocumentZQuery(StmMenuItemSchema.SU_MenuName, menuItemName);
			var command = Factory.LoadTop1<DocumentCommand>(filterForMenuItem);
			var printTask = new PrintTask(command);

			var dummyDocumentMenuCustomisation = DocumentMenuCustomisation.New(null, null, Factory);
			var documentPack = new DocumentPack(command, dummyDocumentMenuCustomisation, null, null);
			var instructions = new DeliveryInstructions(documentPack);
			instructions.DeliveryOptions = AllowedDeliveryOptions.All;

			var deliverablesThatHaveInvalidSourceBO = instructions?.DocumentsToBeDelivered?.OfType<Report>().Where(r => !r.IsIdentifiablePKValid)?.ToArray();
			AssertGreaterThan("Deliverables have InvalidSourceBO", deliverablesThatHaveInvalidSourceBO.Length, 0);

			using (var form = new MockDocDeliveryForm(printTask, instructions))
			{
				form.Show();
				Assert("BackgroundDelivery CheckBox is not visible for Document Engine Reference Guide", !form.BackgroundDeliveryCheckBox.Visible);
			}

			menuItemName = "Report Reference Guide";
			filterForMenuItem = new DocumentZQuery(StmMenuItemSchema.SU_MenuName, menuItemName);
			command = Factory.LoadTop1<DocumentCommand>(filterForMenuItem);
			printTask = new PrintTask(command);

			var dummyReportMenuCustomisation = new ReportMenuCustomisation(Factory, "RepRefFilesReports");
			documentPack = new DocumentPack(command, dummyReportMenuCustomisation, null, null);
			instructions = new DeliveryInstructions(documentPack);
			instructions.DeliveryOptions = AllowedDeliveryOptions.All;

			deliverablesThatHaveInvalidSourceBO = instructions?.DocumentsToBeDelivered?.OfType<Report>().Where(r => !r.IsIdentifiablePKValid)?.ToArray();
			AssertGreaterThan("Deliverables have InvalidSourceBO", deliverablesThatHaveInvalidSourceBO.Length, 0);

			using (var form = new MockDocDeliveryForm(printTask, instructions))
			{
				form.Show();
				Assert("BackgroundDelivery CheckBox is not visible for Report Reference Guide", !form.BackgroundDeliveryCheckBox.Visible);
			}
		}

		#endregion

		#region CoverNotePage

		public void TestCoverNotePageVisibility()
		{
			var formCommand = Factory.NewWithValidTestData<StmMenuItem>();
			formCommand.SU_MenuType = "FRM";
			var formPrintTask = new PrintTask(formCommand);
			var formInstructions = new DeliveryInstructionForForms();

			using (var form = new MockDocDeliveryForm(formPrintTask, formInstructions))
			{
				form.Show();
				Assert("CoverNote Page is not visible when delivering forms", !form.CoverNotePage.TabVisible);
			}

			var documentCommand = Factory.NewWithValidTestData<DocumentCommand>();
			var documentPrintTask = new PrintTask(documentCommand);
			var documentPack = new DocumentPack(documentCommand);
			var documentInstructions = new DeliveryInstructions(documentPack);

			using (var form = new MockDocDeliveryForm(documentPrintTask, documentInstructions))
			{
				form.Show();
				Assert("CoverNote Page is visible when delivering documents", form.CoverNotePage.TabVisible);
			}
		}

		#endregion

		#region Modify

		[RequiresSTA]
		public void TestVisualiseButton()
		{
			var instructions = new DeliveryInstructions();

			instructions.AllowModify = true;
			using (var form = new MockDocDeliveryForm(instructions))
			{
				form.Show();
				Assert("Visualise Button is enabled", form.VisualiseButton.Enabled);
			}

			instructions.AllowModify = false;
			using (var form = new MockDocDeliveryForm(instructions))
			{
				form.Show();
				Assert("Visualise Button is not enabled", !form.VisualiseButton.Enabled);
			}
		}

		public void TestClickVisualiseButton_SecurityUnGranted()
		{
			DocumentsDataRegistry.Instance.AllowDocumentsToBeModified.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var documentCommand = Factory.NewWithValidTestData<DocumentCommand>();
			documentCommand.SU_SupportsVisualisation = true;
			documentCommand.SU_IsModifiable = true;
			documentCommand.SU_MenuPath = "AWB";
			documentCommand.SU_MenuName = "TestTest";
			Factory.Save();
			var instructions = new DeliveryInstructions(new DocumentPack(documentCommand));
			var checkpoint = Env.Security.FindOrCreateDocumentOverrideCheckpoint(documentCommand.PK.ToGuid(), documentCommand.SU_MenuNameMultilingual, ModuleIDs.Customs.JobDeclaration, Env.Security.CustomsDeclarationEnquiry);
			checkpoint.IsAllowed = false;
			using (DocDeliveryForm deliveryForm = new DocDeliveryForm(instructions, checkpoint))
			{
				deliveryForm.Show();
				deliveryForm.VisualiseButton.PerformClick();
				AssertEquals("Caption", "Access Denied: Modify", UnitTestUserNotification.Instance.LastMessage.Caption);
				var expectMessage = @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Operate -> Customs -> Customs Declarations -> AWB -- TestTest";
				AssertEquals("Message", expectMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestDocPackRebuildIfLanguageChanged()
		{
			var childBizO = new MockDocSupportBizO();

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = childBizO;
			using (var templateStream = new MemoryStream())
			using (var documentPack = new DocumentPack(documentCommand, childBizO, null, null))
			{
				using (var creationExcelInterface = new ExcelInterface())
				{
					creationExcelInterface.NewExcelFile(1);
					ExcelWorkSheet workSheet = creationExcelInterface.WorkSheets[0];
					workSheet[0, 0] = "#config";
					workSheet[1, 0] = "Name=TemplateFromStream";
					workSheet[2, 0] = "PageStyle=Portrait";
					workSheet[3, 0] = "#SectionBody";
					workSheet[4, 1] = "Blah";
					workSheet[5, 0] = "#EndOfReport";
					creationExcelInterface.SaveToStream(templateStream, AttachmentTypeList.Codes.Pdf);
				}

				var excelTemplate = new ExcelTemplateWrappingStream("TemplateFromStream", templateStream);

				var wrapper = new MyDummyDocWrapper();
				var report = new Report(documentPack, excelTemplate, wrapper, "Test_" + excelTemplate.TemplateName, null, DocumentDirection.ANY, false);
				documentPack.Add(report);

				var deliveryInstructions = new DeliveryInstructions(documentPack);

				using (var form = new MockDocDeliveryForm(deliveryInstructions))
				{
					form.Show();
					ZFormModaliser.ShowDialogsInTest = true;
					var dummyObj = Factory.NewWithValidTestData<DummyBusinessObject>();

					form.VisualiseDocument();
					AssertEquals("DocPack is not rebuilt since language is not changed", Core.SharedConstants.Languages.EnglishAmerican, deliveryInstructions.DocPack.Language);

					deliveryInstructions.Language = Core.SharedConstants.Languages.ChineseSimplified;
					form.VisualiseDocument();
					AssertEquals("DocPack is rebuilt since language is changed", Core.SharedConstants.Languages.ChineseSimplified, deliveryInstructions.DocPack.Language);
				}
			}
		}

		public void TestClickVisualiseButton_VisualiserFormMarkOwnerForm()
		{
			(DocumentPack documentPack, DocumentCommand documentCommand) GetPackAndCommand(string templateAsString)
			{
				var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "TestShowVisualiserFormWhenModifyDocuments", templateAsString);
				template.SO_DataContext = "UnitTest";

				var dummy = Factory.New<DummyBODocSupportable>();
				dummy.Collection.AddNew();

				var documentCommand = Factory.New<DocumentCommand>();
				documentCommand.Parent = dummy;

				var pivot = documentCommand.Documents.AddNew();
				pivot.SI_SU = documentCommand.PK;
				pivot.SI_SO = template.PK;
				pivot.SI_IsSystemDefined = true;

				return (new DocumentPack(documentCommand, dummy, null, null), documentCommand);
			}

			var rightTemplate = @"{A}-[#config]
			{A}-[Name=TestBeginLoopNotNestedInSectionBody]
			{A}-[#DocumentHeader]
			{B}-[THIS IS DOCUMENTHEADER]
			{A}-[#EndOfReport]";

			var mockIPrintTaskUIProvider = new Mock<IPrintTaskUIProvider>();
			var mockIVisualizedReportView = new Mock<IVisualizedReportView>();
			using (new PrintTaskUIProviderFactory.OverriderForTesting(mockIPrintTaskUIProvider.Object))
			{
				var (documentPack1, documentCommand1) = GetPackAndCommand(rightTemplate);
				using (var printTask = new DocumentPrintSet(documentCommand1))
				{
					printTask.Add(documentPack1);
					var instructions = new DeliveryInstructions(documentPack1);
					using (var form = new MockDocDeliveryForm(printTask, instructions))
					{
						var docPack = instructions.DocPack;
						var report = printTask.GetFirstDocumentPack().OfType<Report>().First();
						mockIPrintTaskUIProvider.Setup(m => m.ShowErrors(report)).Returns(true);
						form.Show();
						ZFormModaliser.ShowDialogsInTest = true;
						form.VisualiseButton.PerformClick();
						AssertEquals("MockDocDeliveryForm", (ZFormModaliser.LastFormShownDialogForTest as VisualiserForm).OwnerName_ForTest);
					}
				}
			}
		}

		#endregion

		#region Validation

		public void TestIgnoreValidationSuspendedWhenDeliverDocuments()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			using (new DisposableAction(Factory.SuspendValidation, Factory.ResumeValidation))
			{
				var deliveryInstructions = new DeliveryInstructions();
				using (deliveryInstructions.GetValidationSuspender())
				{
					deliveryInstructions.Recipients.RemoveAndDeleteAll();

					AssertNoErrors("DeliveryInstructions has no errors", deliveryInstructions);
					Assert("DeliveryInstructions suspend validation", deliveryInstructions.IsValidationSuspended);

					var testReport = new Report(new DocumentPack(), null);
					testReport.PrintCopyType = PrintCopyType.ALL;
					testReport.IncludedInPrint = true;

					deliveryInstructions.DeliverablesToBePrinted.Add(testReport);
					deliveryInstructions.DocumentPackCount = 1;

					var contact = new DocDeliveryContact(Factory);
					contact.Name = "Jerry";
					contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
					contact.DeliveryAddress = "";

					deliveryInstructions.Recipients.Add(contact);

					using (MockDocDeliveryForm form = new MockDocDeliveryForm(deliveryInstructions))
					{
						form.Show();
						form.DeliverButton.PerformClick();

						AssertHasError("Recipients has no email address", contact.DeliveryAddressInfo, "Please enter an Email Address.");
						Assert("DeliveryInstructions ignore validation no longer suspended", !deliveryInstructions.IgnoreValidationSuspended);
						AssertEquals("DeliveryInstructions has errors", "There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
		}

		public void TestManyCopiesToPrint()
		{
			StmPrintQueue printer = Factory.NewWithValidTestData<StmPrintQueue>();
			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			DeliveryInstructions instructions = new DeliveryInstructions();
			Report testReport = new Report(new DocumentPack(), null);
			testReport.PrintCopyType = PrintCopyType.ALL;
			testReport.IncludedInPrint = true;
			instructions.DeliverablesToBePrinted.Add(testReport);
			instructions.DocumentPackCount = 5;
			instructions.PrinterDelivery.NumberOfCopies = 5;
			AssertEquals("Precondition: Zero contacts", 0, instructions.Recipients.Count);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
			using (MockDocDeliveryForm form = new MockDocDeliveryForm(instructions))
			{
				form.Show();
				form.DeliverButton.PerformClick();
				AssertNull("Error NOT displayed as only 5 copies", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			instructions = new DeliveryInstructions();
			testReport = new Report(new DocumentPack(), null);
			testReport.PrintCopyType = PrintCopyType.ALL;
			testReport.IncludedInPrint = true;
			instructions.DeliverablesToBePrinted.Add(testReport);
			instructions.DocumentPackCount = 1;
			instructions.PrinterDelivery.NumberOfCopies = 15;
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
			TestContact.DeliveryMethod = "PRN";
			instructions.Recipients.Add(TestContact);
			instructions.PrinterDelivery.Printers.Add(printer);
			instructions.PrinterDelivery.PrintQueuePK = printer.PK;
			using (MockDocDeliveryForm form = new MockDocDeliveryForm(instructions))
			{
				form.Show();
				form.DeliverButton.PerformClick();
				AssertEquals("Error displayed as 15 copies", "You are going to print 15 copies of each document. Are you sure this is correct?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestNoRecipientsError()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			DeliveryInstructions instructions = new DeliveryInstructions();
			Report testReport = new Report(new DocumentPack(), null);
			testReport.PrintCopyType = PrintCopyType.ALL;
			testReport.IncludedInPrint = true;
			instructions.DeliverablesToBePrinted.Add(testReport);
			instructions.DocumentPackCount = 5;
			AssertEquals("Precondition: Zero contacts", 0, instructions.Recipients.Count);

			using (MockDocDeliveryForm form = new MockDocDeliveryForm(instructions))
			{
				form.Show();
				form.DeliverButton.PerformClick();
				AssertNull("Error NOT displayed as multi doc pack", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			instructions.DocumentPackCount = 1;
			instructions.PrinterDelivery.NumberOfCopies = 50;

			using (MockDocDeliveryForm form = new MockDocDeliveryForm(instructions))
			{
				form.Show();
				instructions.Recipients.RemoveAndDeleteAll();
				form.DeliverButton.PerformClick();
				AssertEquals("Error displayed. Not showing for number of copies as there're other errors", "You have not specified any recipients.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			instructions.Recipients.Add(TestContact);

			using (MockDocDeliveryForm form = new MockDocDeliveryForm(instructions))
			{
				form.Show();
				form.DeliverButton.PerformClick();
				AssertNull("Error NOT displayed", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Dialog OKd", DialogResult.OK, form.DialogResult);
			}
		}

		public void TestNoDocumentsError()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			DeliveryInstructions instructions = new DeliveryInstructions();
			instructions.DocumentPackCount = 1;
			instructions.Recipients.Add(TestContact);

			AssertEquals("Precondition: There should not be any documents.", 0, instructions.DeliverablesToBePrinted.Count);

			using (MockDocDeliveryForm form = new MockDocDeliveryForm(instructions))
			{
				form.Show();
				form.DeliverButton.PerformClick();
				AssertEquals("Error Message", "There are no documents to deliver.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("DialogResult", DialogResult.None, form.DialogResult);
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			using (DocumentPack pack = new DocumentPack())
			using (Report testReport = new Report(pack, null))
			{
				testReport.PrintCopyType = PrintCopyType.ALL;
				instructions.DeliverablesToBePrinted.Add(testReport);
				AssertEquals("There should be a document.", 1, instructions.DeliverablesToBePrinted.Count);

				using (MockDocDeliveryForm form = new MockDocDeliveryForm(instructions))
				{
					form.Show();
					form.DeliverButton.PerformClick();
					AssertNull("There should not be any messages.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("DialogResult", DialogResult.OK, form.DialogResult);
				}
			}

			using (DocumentPack pack = new DocumentPack())
			{
				IDocumentFactoryProvider provider = ObjectFactory.Get<IDocumentFactoryProvider>();
				BusinessObjectFactory documentFactory = (BusinessObjectFactory)provider.GetFactory(Factory);
				IDeliverable deliverable = (IDeliverable)documentFactory.New<IStorageFile>();
				((BusinessObject)deliverable)[StorageDocsSchema.SC_SM.Name] = ZGuid.NewZGuid();

				pack.Add(deliverable);
				instructions = new DeliveryInstructions(pack);

				using (MockDocDeliveryForm form = new MockDocDeliveryForm(instructions))
				{
					form.Show();

					instructions.Recipients[0].DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;
					instructions.Recipients[0].DeliveryAddress = "+61280012200";
					deliverable.IncludedInPrint = true;
					instructions.DeliverablesToBePrinted.Rebuild();  // Every change on the form, rebuilds this collection.
					AssertEquals("Can't deliver a .tif to the Fax", false, deliverable.IncludedInPrint);
					form.DeliverButton.PerformClick();
					AssertEquals("Error Message", "There are no documents to deliver.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("DialogResult", DialogResult.None, form.DialogResult);

					instructions.Recipients[0].DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
					instructions.Recipients[0].DeliveryAddress = "123@123.com";
					deliverable.IncludedInPrint = false;
					form.DeliverButton.PerformClick();
					AssertEquals("Error Message", "There are no documents to deliver.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("DialogResult", DialogResult.None, form.DialogResult);

					deliverable.IncludedInPrint = true;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					form.DeliverButton.PerformClick();
					AssertNull("There should not be any messages.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("DialogResult", DialogResult.OK, form.DialogResult);
				}
			}
		}

		public void TestContactsValidationError()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			DeliveryInstructions instructions = new DeliveryInstructions();
			instructions.DocumentPackCount = 1;
			instructions.Recipients.Add(TestContact);
			Report testReport = new Report(new DocumentPack(), null);
			testReport.PrintCopyType = PrintCopyType.ALL;
			instructions.DeliverablesToBePrinted.Add(testReport);
			instructions.Recipients[0].DeliveryMethod = "ABC";

			TestContact.Name = "";

			using (MockDocDeliveryForm form = new MockDocDeliveryForm(instructions))
			{
				form.Show();
				form.DeliverButton.PerformClick();
				AssertEquals("Error displayed", "There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			instructions.Recipients[0].DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;

			using (MockDocDeliveryForm form = new MockDocDeliveryForm(instructions))
			{
				form.Show();
				form.DeliverButton.PerformClick();
				AssertNull("Error NOT displayed", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Dialog OKd", DialogResult.OK, form.DialogResult);
			}
		}

		#endregion

		#region KeyPress

		public void TestProcessCmdKey_WhenRecipientsGridDoesNotHaveTableStyles()
		{
			using (MockDocDeliveryForm docDeliveryForm = new MockDocDeliveryForm(new DeliveryInstructions()))
			{
				docDeliveryForm.RecipientsGrid.SuspendRefreshTableStyles();
				docDeliveryForm.Show();
				ErrorReporter.Clear();
				Message msg = new Message();
				AssertNoExceptionThrown(() => docDeliveryForm.ProcessCmdKey(ref msg, Keys.Control | Keys.E));
			}
		}

		#endregion

		public void TestSaveFileFilters()
		{
			var queue = Factory.New<StmPrintQueue>();
			queue.SQ_ServerName = System.Environment.MachineName;
			queue.SQ_QueueName = "Awesome Printer";
			queue.SQ_DisplayName = "Awesome Printer";
			var command = Factory.NewWithValidTestData<ReportCommand>();
			Factory.Save();

			var instructions = GetDeliveryInstructionForTest(command);
			instructions.BackgroundDelivery = false;

			ZGuid defaultInstructionPK = ZGuid.NewZGuid();
			instructions.PrinterDelivery.PrintQueuePK = instructions.PrinterDelivery.Printers[0].PK;
			instructions.PrinterDelivery.NumberOfCopies = 10;
			Env.Registry.DeliverReportsInBackground = true;

			var saveHelper = new DocumentSaveHelper();
			var printTask = new PrintTask(command);
			printTask.DeliveryInstructionsDefaultPK = defaultInstructionPK;
			using (var form = new MockDocDeliveryForm(printTask, instructions))
			using (var dialog = new ZSaveFileDialog())
			{
				var disabledFilterIndexes = saveHelper.SetDialogFiltersWithDisabled(dialog, printTask);

				AssertEquals("FileDialog should have 2 filters(CSV<with column heading>, XML) disabled", string.Format("{0},{1}", DialogFilterIndex.CSVWithHeader, DialogFilterIndex.XML), string.Join(",", disabledFilterIndexes.ToArray()));
				AssertEquals("Should display 6 formats by default",
					"Portable Document Format (*.pdf)|*.pdf|Portable Document Format Archive (*.pdf)|*.pdf|Microsoft Excel 97-2003 Spreadsheet (*.xls)|*.xls|Microsoft Excel 2007 Spreadsheet (*.xlsx)|*.xlsx|Tagged Image File (*.tif)|*.tif|Comma Separated Values (*.csv)|*.csv", dialog.Filter);
			}

			command = Factory.New<ReportCommand>();
			var templateWithHeader = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[Data:ReportData=Select DummyBizo1.Z0_Number as Number from dbo.DummyBizo as DummyBizo1 cross join dbo.DummyBizo as DummyBizo2 cross join dbo.DummyBizo as DummyBizo3]
{A}-[ColumnHeadings:]    {B}-[DisplayLabel=""Number"", HeadingText=""Number""]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.Number>]
{A}-[#EndOfReport]");
			var pivot = command.Documents.AddNew();
			pivot.SI_SU = command.PK;
			pivot.SI_SO = templateWithHeader.PK;

			printTask = new PrintTask(command);
			printTask.DeliveryInstructionsDefaultPK = defaultInstructionPK;
			using (var form = new MockDocDeliveryForm(printTask, instructions))
			using (var dialog = new ZSaveFileDialog())
			{
				var disabledFilterIndexes = saveHelper.SetDialogFiltersWithDisabled(dialog, printTask);

				AssertEquals("FileDialog should have none filter disabled", "", string.Join(",", disabledFilterIndexes.ToArray()));
				AssertEquals("Should display 8 formats when report has column headers",
					"Portable Document Format (*.pdf)|*.pdf|Portable Document Format Archive (*.pdf)|*.pdf|Microsoft Excel 97-2003 Spreadsheet (*.xls)|*.xls|Microsoft Excel 2007 Spreadsheet (*.xlsx)|*.xlsx|Tagged Image File (*.tif)|*.tif|Comma Separated Values (*.csv)|*.csv|Comma Separated Values (With Column Headings) (*.csv)|*.csv|XML Files (*.xml)|*.xml", dialog.Filter);
			}

			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[DisableXLSXExport]
{A}-[Data:ReportData=Select DummyBizo1.Z0_Number as Number from dbo.DummyBizo as DummyBizo1 cross join dbo.DummyBizo as DummyBizo2 cross join dbo.DummyBizo as DummyBizo3]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.Number>]
{A}-[#EndOfReport]");

			command = Factory.New<ReportCommand>();
			pivot = command.Documents.AddNew();
			pivot.SI_SU = command.PK;
			pivot.SI_SO = template.PK;

			printTask = new PrintTask(command);
			printTask.DeliveryInstructionsDefaultPK = defaultInstructionPK;
			using (var form = new MockDocDeliveryForm(printTask, instructions))
			using (var dialog = new ZSaveFileDialog())
			{
				var disabledFilterIndexes = saveHelper.SetDialogFiltersWithDisabled(dialog, printTask);

				AssertEquals("FileDialog should have 3 filters(XLSX, CSV<with column heading>, XML) disabled",
					string.Format("{0},{1},{2}", DialogFilterIndex.XLSX, DialogFilterIndex.CSVWithHeader, DialogFilterIndex.XML), string.Join(",", disabledFilterIndexes.ToArray()));
				AssertEquals("Should not display XLSX formats when report marked as DisableXLSXExport",
					"Portable Document Format (*.pdf)|*.pdf|Portable Document Format Archive (*.pdf)|*.pdf|Microsoft Excel 97-2003 Spreadsheet (*.xls)|*.xls|Tagged Image File (*.tif)|*.tif|Comma Separated Values (*.csv)|*.csv", dialog.Filter);
			}

			template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[DisableCSVExport]
{A}-[Data:ReportData=Select DummyBizo1.Z0_Number as Number from dbo.DummyBizo as DummyBizo1 cross join dbo.DummyBizo as DummyBizo2 cross join dbo.DummyBizo as DummyBizo3]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.Number>]
{A}-[#EndOfReport]");

			command = Factory.New<ReportCommand>();
			pivot = command.Documents.AddNew();
			pivot.SI_SU = command.PK;
			pivot.SI_SO = template.PK;

			printTask = new PrintTask(command);
			printTask.DeliveryInstructionsDefaultPK = defaultInstructionPK;
			using (var form = new MockDocDeliveryForm(printTask, instructions))
			using (var dialog = new ZSaveFileDialog())
			{
				var disabledFilterIndexes = saveHelper.SetDialogFiltersWithDisabled(dialog, printTask);
				AssertEquals("FileDialog should have 3 filters(CSV, CSV<with column heading>, XML) disabled",
					string.Format("{0},{1},{2}", DialogFilterIndex.CSV, DialogFilterIndex.CSVWithHeader, DialogFilterIndex.XML), string.Join(",", disabledFilterIndexes.ToArray()));
				AssertEquals("Should not display CSV and XML formats when report marked as DisableCSVExport",
					"Portable Document Format (*.pdf)|*.pdf|Portable Document Format Archive (*.pdf)|*.pdf|Microsoft Excel 97-2003 Spreadsheet (*.xls)|*.xls|Microsoft Excel 2007 Spreadsheet (*.xlsx)|*.xlsx|Tagged Image File (*.tif)|*.tif", dialog.Filter);
			}
		}

		public void TestGetDialogFilterIndex()
		{
			var command = Factory.NewWithValidTestData<ReportCommand>();
			var instructions = GetDeliveryInstructionForTest(command);
			var printTask = new PrintTask(command);

			using (var form = new MockDocDeliveryForm(printTask, instructions))
			using (var dialog = new ZSaveFileDialog())
			{
				var disabledFileExtensions = new List<int>() { 4, 6, 7 };
				dialog.Filter = "Portable Document Format (*.pdf)|*.pdf|Portable Document Format Archive (*.pdf)|*.pdf|Microsoft Excel 97-2003 Spreadsheet (*.xls)|*.xls|Tagged Image File (*.tif)|*.tif|XML Files (*.xml)|*.xml";
				var expectedFileExtension = new string[] { "PDF", "PDF", "XLS", "TIF", "XML" };

				var expectedFilter = GetExpectedFilterIndexExtension();
				var saveHelper = new DocumentSaveHelper();
				for (int i = 1; i <= expectedFileExtension.Length; i++)
				{
					dialog.FilterIndex = i;
					AssertEquals("Saved file should have the same extension as expected", expectedFileExtension[i - 1], expectedFilter[saveHelper.GetFileDialogFilterIndexPlusDisabled(dialog, disabledFileExtensions)]);
				}
			}
		}

		Dictionary<int, string> GetExpectedFilterIndexExtension()
		{
			var dic = new Dictionary<int, string>();

			dic.Add(1, "PDF");
			dic.Add(2, "PDF");
			dic.Add(3, "XLS");
			dic.Add(4, "XLSX");
			dic.Add(5, "TIF");
			dic.Add(6, "CSV");
			dic.Add(7, "CSV");
			dic.Add(8, "XML");

			return dic;
		}

		public void TestShowInTaskbarIsTrue()
		{
			DeliveryInstructions instructions = new DeliveryInstructions();
			using (DocDeliveryForm frm = new DocDeliveryForm(instructions, Env.Security.None))
			{
				AssertEquals(true, frm.ShowInTaskbar);
			}
		}

#if !WINZOR

		public void TestSystemDefaultContactSalutation()
		{
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_ContactType = ContactType.Sales.Code;
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var pack = new DocumentPack(menuItem);
			pack.DocumentSupporter = new AutoDeliveryBizO(org);
			var task = new PrintTask();
			task.Add(pack);

			var instructions = new DeliveryInstructions(pack);
			var businessObject = Factory.New<DummyBusinessObject>();
			var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.InvalidFormatTemplate.xls", "InvalidFormatTemplate.xls");
			var excelTemplate = new ExcelTemplateForUnitTesting("InvalidFormatTemplate.xls", Path.GetFullPath(tempFileName));
			var deliverable = new Report(pack, excelTemplate, BODocDataProvider.Get(businessObject), "Test_" + excelTemplate.TemplateName, null, DocumentDirection.ANY, false);
			instructions.DeliverablesToBePrinted.Add(deliverable);

			using (var parentForm = new DocDeliveryForm(instructions, Env.Security.None))
			{
				parentForm.Show();

				const string gridLayout =
	@"<DocumentElement>
	<OGridColumnSettings>
		<MappingName>Name</MappingName>
		<Width>80</Width>
		<IsVisible>true</IsVisible>
	</OGridColumnSettings>
	<OGridSortSettings>
		<SortPropertyName>Name</SortPropertyName>
		<SortDirection>0</SortDirection>
	</OGridSortSettings>
</DocumentElement>";
				var layoutData = Encoding.Default.GetBytes(gridLayout);
				new TestDataGridLayoutManager().LoadLayout(parentForm.RecipientsGrid, new MemoryStream(layoutData));
				new DataGridLayoutManager().SaveDefaultLayout(parentForm.RecipientsGrid);
			}

			using (var parentForm = new DocDeliveryForm(instructions, Env.Security.None))
			{
				parentForm.Show();
				instructions.Recipients[0].DeliveryMethod = "";
				instructions.Language = Core.Constants.Languages.ChineseSimplified;
				AssertEquals(ContactType.Sales.DefaultName.ToString(Core.Constants.Languages.ChineseSimplified), instructions.Recipients[0].Name);
			}
		}
#endif

		public void TestDestinationInstructionsSet()
		{
			var command = Factory.NewWithValidTestData<ReportCommand>();
			var printTask = new PrintTask(command);
			var pack = new DocumentPack(command);
			var instructions = new DeliveryInstructions(pack);
			instructions.DeliveryOptions = AllowedDeliveryOptions.All;
			Env.Security.PreviewReportButton.IsAllowed = true;

			instructions.DocumentPackCount = 1;
			using (MockDocDeliveryForm form = new MockDocDeliveryForm(printTask, instructions))
			{
				form.Show();
				form.DeliverButton.PerformClick();
				AssertEquals("Delivery instructions set to TakenFromContact", DeliveryInstructionDestination.TakenFromContact, instructions.Destination);
			}

			instructions.DocumentPackCount = 3;
			using (MockDocDeliveryForm form = new MockDocDeliveryForm(printTask, instructions))
			{
				form.Show();
				form.PrintMultipleDocPackRadioButton.PerformClick();
				form.DeliverButton.PerformClick();
				AssertEquals("Delivery instructions set to Print", DeliveryInstructionDestination.Print, instructions.Destination);
			}

			using (MockDocDeliveryForm form = new MockDocDeliveryForm(printTask, instructions))
			{
				form.Show();
				form.AutoDeliverMultipleDocPackRadioButton.PerformClick();
				form.DeliverButton.PerformClick();
				AssertEquals("Delivery instructions set to AutoDeliver", DeliveryInstructionDestination.Auto, instructions.Destination);
			}

			using (MockDocDeliveryForm form = new MockDocDeliveryForm(printTask, instructions))
			{
				form.PreviewRequested += new PrintTask.PreviewRequestedEventHandler(Form_PreviewRequested);
				form.Show();
				PrintPreviewRequestedEventFired = false;
				form.PreviewButton.PerformClick();
				Assert("Print Preview Event Fired", PrintPreviewRequestedEventFired);
				AssertEquals("Cancel button text is Close", "Close", form.CloseButton.Text);
			}
		}

		public void TestMultiDocPackRecipientsNotShown()
		{
			DeliveryInstructions instructions = new DeliveryInstructions();
			instructions.DocumentPackCount = 4;

			using (MockDocDeliveryForm form = new MockDocDeliveryForm(instructions))
			{
				form.Show();
				Assert("Multi doc pack groupbox shown", form.MultiDocPackGroupbox.Visible);
				Assert("Documents to Send tabpage not shown", !form.MainTabControl.TabPages.Contains(form.DocumentsTabPage));
				AssertEquals("There are 4 document packs to deliver.", form.MultiDocPacksLabel.Text);

				instructions.DocumentPackCount = 997;
				AssertEquals("There are 997 document packs to deliver.", form.MultiDocPacksLabel.Text);
			}

			instructions.DocumentPackCount = 1;

			using (MockDocDeliveryForm form = new MockDocDeliveryForm(instructions))
			{
				form.Show();
				Assert("Multi doc pack groupbox hidden", !form.MultiDocPackGroupbox.Visible);
				Assert("Documents to Send tabpage is shown", form.MainTabControl.TabPages.Contains(form.DocumentsTabPage));
			}
		}

		public void TestCloseButtonCancelsDialog()
		{
			using (MockDocDeliveryForm form = new MockDocDeliveryForm(new DeliveryInstructions()))
			{
				form.Show();
				form.CloseButton.PerformClick();

				AssertEquals("Dialog Result set to Cancel", DialogResult.Cancel, form.DialogResult);
			}
		}

		public void TestTextTemplateOnReport()
		{
			ReportCommand command = Factory.Load<ReportCommand>(ReportScheduleTaskTest.TestReportPK);
			DocumentPack pack = new DocumentPack(command);
			DeliveryInstructions instructions = new DeliveryInstructions(pack);
			instructions.IncludeCoverNote = true;

			using (MockDocDeliveryForm form = new MockDocDeliveryForm(instructions))
			{
				form.Show();
				form.CoverNotePage.Show();
				var template = TextTemplatesTestHelper.NewTemplate(form.CoverNoteTextBox);
				template.S8_Description = "Test";
				template.S8_TemplateText = "Coversheet for <DocumentPackTitle>";
				template.Factory.Save();
				TextTemplatesTestHelper.ApplyTemplate(form.CoverNoteTextBox, template.S8_Description);
				AssertEquals("Coversheet for Bookings - Consignor Booking Status Report", form.CoverNoteTextBox.Text);
			}
		}

		public void TestTextTemplateOnDocument()
		{
			DocumentCommand command = Factory.New<DocumentCommand>();
			DocBusinessObject.Z0_Code = "XYZ";
			DocumentPack pack = new DocumentPack(command, DocBusinessObject, null, null, false, null);
			DeliveryInstructions instructions = new DeliveryInstructions(pack);
			instructions.DocumentPackTitle = "Test Document";
			instructions.IncludeCoverNote = true;

			using (MockDocDeliveryForm form = new MockDocDeliveryForm(instructions))
			{
				form.Show();
				form.CoverNotePage.Show();
				var template = TextTemplatesTestHelper.NewTemplate(form.CoverNoteTextBox);
				template.S8_Description = "Test";
				template.S8_TemplateText = "Coversheet for <Z0_Code> - <DocumentPackTitle>";
				template.Factory.Save();
				TextTemplatesTestHelper.ApplyTemplate(form.CoverNoteTextBox, template.S8_Description);
				AssertEquals("Coversheet for XYZ - Test Document", form.CoverNoteTextBox.Text);
			}
		}

		public void TestFormClosed_ShouldFireEvent()
		{
			DocBusinessObject.Z0_Code = "XYZ";
			var pack = new DocumentPack(Factory.New<DocumentCommand>(), DocBusinessObject, null, null, false, null);
			var instructions = new DeliveryInstructions(pack)
			{
				DocumentPackTitle = "Test Document",
				IncludeCoverNote = true
			};

			using (var form = new MockDocDeliveryForm(new PrintTask(), instructions))
			{
				form.Show();
				var eventFired = false;

				var view = (IDocumentDeliveryView)form;
				view.ViewClosed += (s, e) => eventFired = true;
				form.Close();

				Assert(eventFired);
			}
		}

		public void TestSaveDefaultPrinterWhenUsingBackgroundWorker()
		{
			var queue = Factory.New<StmPrintQueue>();
			queue.SQ_ServerName = System.Environment.MachineName;
			queue.SQ_QueueName = "Awesome Printer";
			queue.SQ_DisplayName = "Awesome Printer";
			Factory.Save();

			var command = Factory.NewWithValidTestData<ReportCommand>();
			Factory.Save();

			var instructions = GetDeliveryInstructionForTest(command);
			instructions.BackgroundDelivery = false;

			instructions.PrinterDelivery.PrintQueuePK = instructions.PrinterDelivery.Printers[0].PK;
			instructions.PrinterDelivery.NumberOfCopies = 10;

			var printTask = new PrintTask();
			printTask.DeliveryInstructionsDefaultPK = command.PK;

			Env.Registry.DeliverReportsInBackground = true;
			using (var form = new MockDocDeliveryForm(printTask, instructions))
			{
				form.Show();
				form.DeliverButton.PerformClick();
			}

			AssertDeliverySettingIsSaved(queue, printTask);

			instructions = GetDeliveryInstructionForTest(command);
			instructions.BackgroundDelivery = true;

			printTask.DeliveryInstructionsDefaultPK = command.PK;
			instructions.PrinterDelivery.PrintQueuePK = instructions.PrinterDelivery.Printers[0].PK;
			instructions.PrinterDelivery.NumberOfCopies = 10;

			Env.Registry.DeliverReportsInBackground = true;
			using (var form = new MockDocDeliveryForm(printTask, instructions))
			{
				form.Show();
				form.DeliverButton.PerformClick();
			}

			AssertDeliverySettingIsSaved(queue, printTask);
		}

		void AssertDeliverySettingIsSaved(StmPrintQueue queue, PrintTask printTask)
		{
			var defaultPrinter = StmDefaultPrinter.LoadOrCreateDefaultPrinter(Factory, GlbStaff.CurrentUser, Factory.Load<IStmMenuItem>(printTask.DeliveryInstructionsDefaultPK));
			AssertNotNull("default printer setting is saved.", defaultPrinter);
			AssertEquals("default printer setting is saved.", queue.PK, defaultPrinter.SDP_SQ_Printer);
			AssertEquals("default printer setting is saved.", (byte)10, defaultPrinter.SDP_NumberOfCopies);
		}

		DeliveryInstructions GetDeliveryInstructionForTest(ReportCommand command)
		{
			var pack = new DocumentPack(command);
			var instructions = new DeliveryInstructions(pack);
			var testReport = new Report(pack, null);
			testReport.PrintCopyType = PrintCopyType.ALL;
			instructions.DeliverablesToBePrinted.Add(testReport);

			instructions.Recipients.RemoveAndDeleteAll();
			var contact = instructions.Recipients.AddNew();
			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			contact.AttachmentType = OrgConstants.AttachmentType.PDF;
			contact.DeliveryAddress = "test@edi.com.au";
			return instructions;
		}

		public void TestRunPreSaveValidation_ConcurrentCollectionModification()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
					@"{A}-[#Config]
{A}-[#EndOfReport]");

			var reportCommand = Factory.New<ReportCommand>();
			var pivot = reportCommand.Documents.AddNew();
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Company1";
			org.MainAddress.OA_Address1 = "Main Address";

			Factory.Save();

			using (var pack = new DocumentPack(reportCommand))
			{
				var instructions = new DeliveryInstructions(pack);
				instructions.AllowAutoDelivery = false;
				var contact1 = instructions.Recipients.AddNew();
				contact1.OrgHeaderPK = org.PK;
				contact1.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
				contact1.DeliveryAddress = "test@test.com";

				var contact2 = instructions.Recipients.AddNew();
				contact2.DeliveryMethod = "EML";
				contact2.AttachmentType = "PDF";
				contact2.DeliveryAddress = "";

				AssertNoExceptionThrown(() =>
				{
					using (var form = new MockDocDeliveryForm(instructions))
					{
						form.Show();
						contact1.NotificationsChanged += (o, e) => { instructions.Recipients.Remove(contact2); };
						contact2.NotificationsChanged += (o, e) => { instructions.Recipients.Remove(contact1); };
						form.DeliverButton.PerformClick();
					}
				});
			}
		}

		#region Delivery

		public void TestDeliver_Document()
		{
			DocumentCommand command = Factory.New<DocumentCommand>();
			DocumentPack pack = new DocumentPack(command);
			DeliveryInstructions instructions = new DeliveryInstructions(pack);
			Report testReport = new Report(pack, null);
			testReport.PrintCopyType = PrintCopyType.ALL;
			instructions.DeliverablesToBePrinted.Add(testReport);

			instructions.Recipients.RemoveAndDeleteAll();
			DocDeliveryContact contact = instructions.Recipients.AddNew();
			contact.DeliveryMethod = "EML";
			contact.AttachmentType = "PDF";
			contact.DeliveryAddress = "test@edi.com.au";

			using (MockDocDeliveryForm form = new MockDocDeliveryForm(instructions))
			{
				form.Show();
				form.DeliverButton.PerformClick();
				AssertEquals("No errors", false, instructions.HasErrors);
				AssertNull("No errors", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("DialogResult", DialogResult.OK, form.DialogResult);
				AssertNull("No schedule created", form.scheduleTask);
			}
		}

		public void TestDeliver_Report_ForegroundDelivery()
		{
			ReportCommand command = Factory.New<ReportCommand>();
			DocumentPack pack = new DocumentPack(command);
			DeliveryInstructions instructions = new DeliveryInstructions(pack);
			Report testReport = new Report(pack, null);
			testReport.PrintCopyType = PrintCopyType.ALL;
			instructions.DeliverablesToBePrinted.Add(testReport);

			instructions.Recipients.RemoveAndDeleteAll();
			DocDeliveryContact contact = instructions.Recipients.AddNew();
			contact.DeliveryMethod = "EML";
			contact.AttachmentType = "PDF";
			contact.DeliveryAddress = "test@edi.com.au";

			bool originalRegistry = Env.Registry.DeliverReportsInBackground;
			Env.Registry.DeliverReportsInBackground = false;
			try
			{
				using (MockDocDeliveryForm form = new MockDocDeliveryForm(instructions))
				{
					form.Show();
					form.DeliverButton.PerformClick();
					AssertEquals("No errors", false, instructions.HasErrors);
					AssertNull("No errors", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("DialogResult", DialogResult.OK, form.DialogResult);
					AssertNull("No schedule created", form.scheduleTask);
				}
			}
			finally
			{
				Env.Registry.DeliverReportsInBackground = originalRegistry;
			}
		}

		public void TestDeliver_Report_ScheduleDelivery()
		{
			ReportCommand command = Factory.New<ReportCommand>();
			DocumentPack pack = new DocumentPack(command);
			DeliveryInstructions instructions = new DeliveryInstructions(pack);
			Report testReport = new Report(pack, null);
			testReport.PrintCopyType = PrintCopyType.ALL;
			instructions.DeliverablesToBePrinted.Add(testReport);

			instructions.BackgroundDelivery = true;
			instructions.Recipients.RemoveAndDeleteAll();
			DocDeliveryContact contact = instructions.Recipients.AddNew();
			contact.DeliveryMethod = "EML";
			contact.AttachmentType = "PDF";
			contact.DeliveryAddress = "test@edi.com.au";

			bool originalRegistry = Env.Registry.DeliverReportsInBackground;
			Env.Registry.DeliverReportsInBackground = true;
			try
			{
				using (MockDocDeliveryForm form = new MockDocDeliveryForm(instructions))
				{
					form.Show();

					Env.Security.ScheduledTaskNew.IsAllowed = false;
					form.DeliverButton.PerformClick();
					AssertEquals("There should be an error message.", Env.Security.ScheduledTaskNew.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNull("No form should be shown.", ZFormModaliser.LastFormShownDialogForTest);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					Env.Security.ScheduledTaskNew.IsAllowed = true;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;

					form.DeliverButton.PerformClick();
					AssertEquals("No errors", false, instructions.HasErrors);
					AssertNull("No errors", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("DialogResult", DialogResult.None, form.DialogResult);
					AssertNotNull("Schedule created", form.scheduleTask);
					AssertEquals("Schedule private - ie not a recurring schedule", true, form.scheduleTask.S5_IsPrivate);
					AssertEquals(false, testReport.ShouldUpdateSchedulableFilters);

					using (ScheduleDatePickerForm scheduleForm = (ScheduleDatePickerForm)ZFormModaliser.LastFormShownDialogForTest)
					{
						AssertEquals("scheduleForm.BusinessEntity", form.scheduleTask, scheduleForm.LastDataSourceForTest);
					}

					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					form.DeliverButton.PerformClick();
					AssertEquals("No errors", false, instructions.HasErrors);
					AssertNull("No errors", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("DialogResult", DialogResult.Cancel, form.DialogResult);
					AssertNotNull("Schedule created", form.scheduleTask);
					AssertEquals("Schedule private - ie not a recurring schedule", true, form.scheduleTask.S5_IsPrivate);

					using (ScheduleDatePickerForm scheduleForm = (ScheduleDatePickerForm)ZFormModaliser.LastFormShownDialogForTest)
					{
						AssertEquals("scheduleForm.BusinessEntity", form.scheduleTask, scheduleForm.LastDataSourceForTest);
					}
				}
			}
			finally
			{
				Env.Registry.DeliverReportsInBackground = originalRegistry;
			}
		}

		public void TestScheduleTaskIsPopulated()
		{
			DeliveryInstructions instructions = DocDeliveryFormTestHelper.CreateInstructionsWithValidData(Factory);
			instructions.Recipients[0].DeliveryMethod = "xxx";

			try
			{
				using (MockDocDeliveryForm form = new MockDocDeliveryForm(instructions))
				{
					form.Show();
					instructions.BackgroundDelivery = true;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;

					form.DeliverButton.PerformClick();
					AssertNull("scheduleTask not created", form.scheduleTask);

					instructions.Recipients[0].DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
					instructions.Recipients[0].DeliveryAddress = "bob@bob.com";
					form.DeliverButton.PerformClick();
					AssertEquals("scheduleTask.Recipients.Count", 1, form.scheduleTask.Recipients.Count);
					AssertEquals("scheduleTask.Recipients[0].S6_DeliveryMethod", Core.Constants.ContactNotifyModes.Email, form.scheduleTask.Recipients[0].S6_DeliveryMethod);
					AssertEquals("scheduleTask.S5_ScheduleState.IsEmpty", false, form.scheduleTask.S5_ScheduleState.IsEmpty);
					ZBlob scheduleState = form.scheduleTask.S5_ScheduleState;
					StmScheduleTaskRecipient recipient = form.scheduleTask.Recipients[0];

					instructions.IncludeCoverNote = true;
					instructions.Recipients[0].DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;
					instructions.Recipients[0].DeliveryAddress = "+61280012200";
					form.DeliverButton.PerformClick();
					AssertEquals("scheduleTask.Recipients.Count", 1, form.scheduleTask.Recipients.Count);
					AssertEquals("scheduleTask.Recipients[0].S6_DeliveryMethod", Core.Constants.ContactNotifyModes.Fax, form.scheduleTask.Recipients[0].S6_DeliveryMethod);
					AssertEquals("scheduleTask.S5_ScheduleState.IsEmpty", false, form.scheduleTask.S5_ScheduleState.IsEmpty);
					Assert("scheduleTask.S5_ScheduleState should have changed.", form.scheduleTask.S5_ScheduleState != scheduleState);
					AssertEquals("Old recipient should be deleted.", true, recipient.IsDeleted);
				}
			}
			finally
			{
				instructions?.DocPack?.Dispose();
			}
		}

		public void TestCannotScheduleIfHasNoRecipients()
		{
			DeliveryInstructions instructions = DocDeliveryFormTestHelper.CreateInstructions(Factory);

			using (MockDocDeliveryForm form = new MockDocDeliveryForm(instructions))
			{
				form.Show();
				instructions.Recipients.RemoveAndDeleteAll();
				AssertNull("Precondition: No error message should be shown yet.", UnitTestUserNotification.Instance.LastMessage.Text);

				form.DeliverButton.PerformClick();
				AssertEquals("An error message should be shown.", "You have not specified any recipients.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("DialogResult", DialogResult.None, form.DialogResult);
			}
		}

		public void TestShowConfirmationPopupIfAnyRecipientHasNDR()
		{
			GlbEmailAddress.LoadOrNew(Factory, "aa@aa.com.au").GI_DeliveryStatus = EmailDeliveryReportStatus.Codes.NonDeliveryReport;
			GlbEmailAddress.LoadOrNew(Factory, "bb@bb.com.au").GI_DeliveryStatus = EmailDeliveryReportStatus.Codes.NonDeliveryReport;
			Factory.Save();

			var instructions = DocDeliveryFormTestHelper.CreateInstructions(Factory);
			bool originalRegistry = Env.Registry.DeliverReportsInBackground;
			Env.Registry.DeliverReportsInBackground = false;
			try
			{
				IEnumerable<ZString> actuals = Array.Empty<ZString>();
				var mockPreSendChecker = new Mock<IEmailPreSendChecker>();
				mockPreSendChecker
					.Setup(x => x.PromptUserIfSendingToNdrRecipients(It.IsAny<IEnumerable<ZString>>()))
					.Callback<IEnumerable<ZString>>((args) => { actuals = args; })
					.Returns(true);

				using (ObjectFactory.Substitute(mockPreSendChecker.Object))
				using (var form = new MockDocDeliveryForm(instructions))
				{
					form.Show();
					instructions.Recipients.RemoveAndDeleteAll();
					var contactA = instructions.Recipients.AddNew();
					contactA.DeliveryMethod = "EML";
					contactA.AttachmentType = "PDF";
					contactA.DeliveryAddress = "aa@aa.com.au";
					contactA.EmailCarbonCopyRecipients.AddNew().EmailAddress = "aaCC@aa.com.au";
					contactA.EmailBlindCarbonCopyRecipients.AddNew().EmailAddress = "aaBCC@aa.com.au";
					var contactB = instructions.Recipients.AddNew();
					contactB.DeliveryMethod = "EML";
					contactB.AttachmentType = "PDF";
					contactB.DeliveryAddress = "bb@bb.com.au";
					contactB.EmailCarbonCopyRecipients.AddNew().EmailAddress = "bbCC@bb.com.au";
					contactB.EmailBlindCarbonCopyRecipients.AddNew().EmailAddress = "bbBCC@bb.com.au";

					form.DeliverButton.PerformClick();

					var expectedEmailsToCheck = new ZString[]
					{
						"aa@aa.com.au",
						"aaCC@aa.com.au",
						"aaBCC@aa.com.au",
						"bb@bb.com.au",
						"bbCC@bb.com.au",
						"bbBCC@bb.com.au"
					};

					AssertContainsExactElementsInAnyOrder(expectedEmailsToCheck, actuals);
				}
			}
			finally
			{
				Env.Registry.DeliverReportsInBackground = originalRegistry;
				instructions?.DocPack?.Dispose();
			}
		}

		public void TestDeliverNotShowPrinterHelperDialog()
		{
			var printer = Factory.NewWithValidTestData<StmPrintQueue>();
			Factory.Save();

			var pack = new DocumentPack();
			var printTask = new PrintTask();
			printTask.Add(pack);

			var testReport = new Report(pack, null)
			{
				PrintCopyType = PrintCopyType.ALL,
				IncludedInPrint = true
			};

			var contact = new DocDeliveryContact(Factory)
			{
				DeliveryMethod = Core.Constants.ContactNotifyModes.Print,
				DeliveryAddress = "123@123.com",
			};

			var instructions = printTask.TaskSettings.DocPacksDeliveryInstructions[0];
			instructions.Recipients.RemoveAndDeleteAll();
			instructions.Recipients.Add(contact);
			instructions.DeliverablesToBePrinted.Add(testReport);
			instructions.PrinterDelivery.Printers.Add(printer);
			instructions.PrinterDelivery.PrintQueuePK = printer.PK;

			using (var form = new MockDocDeliveryForm(printTask, instructions))
			{
				form.Show();
				form.DeliverButton.PerformClick();
				AssertNull("Should not show PrinterHelpDialog", ZFormModaliser.LastFormShownDialogForTest?.Text);
			}
		}

		public void TestScrollBarShowInCoverNote()
		{
			var instructions = new DeliveryInstructions();
			instructions.IncludeCoverNote = true;
			using (var form = new MockDocDeliveryForm(instructions))
			{
				form.Show();
				form.CoverNotePage.Show();
				AssertEquals("Scroll bar should be available in cover note text box.", ScrollBars.Vertical, form.CoverNoteTextBox.ScrollBars);
			}
		}

		public void TestShouldDisableImportDataMenuItemForDocumentsAndEDocs()
		{
			var instructions = new DeliveryInstructions();
			using (var form = new MockDocDeliveryForm(instructions))
			{
				AssertEquals("Should disable documents grid import data menuitem", true, form.DocumentsGrid.DisableImportDataMenuItem);
				AssertEquals("Should disable included eDocs grid import data menuitem", true, form.IncludedEDocsGrid.DisableImportDataMenuItem);
			}
		}

		#endregion

		public void TestShowVisualiserFormWhenModifyDocuments()
		{
			(DocumentPack documentPack, DocumentCommand documentCommand) GetPackAndCommand(string templateAsString)
			{
				var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "TestShowVisualiserFormWhenModifyDocuments", templateAsString);
				template.SO_DataContext = "UnitTest";

				var dummy = Factory.New<DummyBODocSupportable>();
				dummy.Collection.AddNew();

				var documentCommand = Factory.New<DocumentCommand>();
				documentCommand.Parent = dummy;

				var pivot = documentCommand.Documents.AddNew();
				pivot.SI_SU = documentCommand.PK;
				pivot.SI_SO = template.PK;
				pivot.SI_IsSystemDefined = true;

				return (new DocumentPack(documentCommand, dummy, null, null), documentCommand);
			}

			var rightTemplate = @"{A}-[#config]
			{A}-[Name=TestBeginLoopNotNestedInSectionBody]
			{A}-[#DocumentHeader]
			{B}-[THIS IS DOCUMENTHEADER]
			{A}-[#EndOfReport]";
			var warningTemplate = @"{A}-[#config]
			{A}-[Name=TestBeginLoopNestedInSectionBody]
			{A}-[DataContext=.DummyDocumentSupportable]
			{A}-[#DocumentHeader]
			{B}-[THIS IS DOCUMENTHEADER]
			{A}-[#SectionBody:Data=Collection]
			{B}-[<Collection.Z0_VarCharMax>]
			{A}-[#BeginLoop:Data=CollectionLevel1]
			{B}-[<CollectionLevel1.Z0_VarCharMax>]
			{A}-[#EndLoop]
			{A}-[#SectionFooter]
			{A}-[#EndOfReport]";
			var errorTemplate = @"{A}-[#config]
			{A}-[Name=TestBeginLoopNotNestedInSectionBody]
			{A}-[#DocumentHeader2]
			{B}-[THIS IS DOCUMENTHEADER]
			{A}-[#EndOfReport]";

			var mockIPrintTaskUIProvider = new Mock<IPrintTaskUIProvider>();
			var mockIVisualizedReportView = new Mock<IVisualizedReportView>();
			using (new PrintTaskUIProviderFactory.OverriderForTesting(mockIPrintTaskUIProvider.Object))
			{
				var (documentPack1, documentCommand1) = GetPackAndCommand(rightTemplate);
				using (var printTask = new DocumentPrintSet(documentCommand1))
				{
					printTask.Add(documentPack1);
					var instructions = new DeliveryInstructions(documentPack1);
					using (var form = new MockDocDeliveryForm(printTask, instructions))
					{
						var docPack = instructions.DocPack;
						var report = printTask.GetFirstDocumentPack().OfType<Report>().First();
						mockIPrintTaskUIProvider.Setup(m => m.ShowErrors(report)).Returns(true);
						form.Show();
						ZFormModaliser.ShowDialogsInTest = true;
						AssertNull("No dialog shows yet", ZFormModaliser.LastFormShownDialogForTest);
						form.VisualiseButton.PerformClick();
						Assert("VisualiserForm dialog should show", ZFormModaliser.LastFormShownDialogForTest is VisualiserForm);
						ZFormModaliser.LastFormShownDialogForTest.Dispose();
						ZFormModaliser.LastFormShownDialogForTest = null;
					}
				}

				var (documentPack2, documentCommand2) = GetPackAndCommand(warningTemplate);
				using (var printTask = new DocumentPrintSet(documentCommand2))
				{
					printTask.Add(documentPack2);
					var instructions = new DeliveryInstructions(documentPack2);
					using (var form = new MockDocDeliveryForm(printTask, instructions))
					{
						var docPack = instructions.DocPack;
						var report = printTask.GetFirstDocumentPack().OfType<Report>().First();
						mockIPrintTaskUIProvider.Setup(m => m.ShowErrors(report)).Returns(true);
						form.Show();
						ZFormModaliser.ShowDialogsInTest = true;
						AssertNull("No dialog shows yet", ZFormModaliser.LastFormShownDialogForTest);
						form.VisualiseButton.PerformClick();
						Assert("VisualiserForm dialog should show", ZFormModaliser.LastFormShownDialogForTest is VisualiserForm);
						ZFormModaliser.LastFormShownDialogForTest.Dispose();
						ZFormModaliser.LastFormShownDialogForTest = null;
					}
				}

				var (documentPack3, documentCommand3) = GetPackAndCommand(errorTemplate);
				using (var printTask = new DocumentPrintSet(documentCommand3))
				{
					printTask.Add(documentPack3);
					var instructions = new DeliveryInstructions(documentPack3);
					using (var form = new MockDocDeliveryForm(printTask, instructions))
					{
						var docPack = instructions.DocPack;
						var report = printTask.GetFirstDocumentPack().OfType<Report>().First();
						mockIPrintTaskUIProvider.Setup(m => m.ShowErrors(report)).Returns(false);
						form.Show();
						ZFormModaliser.ShowDialogsInTest = true;
						AssertNull("No dialog shows yet", ZFormModaliser.LastFormShownDialogForTest);
						form.VisualiseButton.PerformClick();
						AssertNull("VisualiserForm dialog should not show", ZFormModaliser.LastFormShownDialogForTest);
					}
				}
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new DocDeliveryForm(new DeliveryInstructions(), Env.Security.None);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestContact = new DocDeliveryContact(Factory);
			TestContact.Name = "Zubin";
			TestContact.DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;
			TestContact.DeliveryAddress = "+61290251199";
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever(typeof(PrintTaskTest).Assembly));

		DocDeliveryContact TestContact;

		DocDummyBusinessObject DocBusinessObject
		{
			get
			{
				if (fDocBusinessObject == null)
				{
					BusinessObjectFactory newFactory = new BusinessObjectFactory();
					fDocBusinessObject = newFactory.New<DocDummyBusinessObject>();
					fDocBusinessObject.Z0_Code = "Tst";
					fDocBusinessObject.Z0_Description = "Desc";
					newFactory.Save();
				}
				return fDocBusinessObject;
			}
		}

		DocDummyBusinessObject fDocBusinessObject;

		ZDocumentMenuItem DocumentMenuItem
		{
			get
			{
				if (fDocumentMenuItem == null)
				{
					fDocumentMenuItem = new ZDocumentMenuItem();
				}
				return fDocumentMenuItem;
			}
		}

		ZDocumentMenuItem fDocumentMenuItem;

		#region Test Objects

		class DocDummyBusinessObjectDocumentSupporter : DocumentSupporter
		{
			public DocDummyBusinessObjectDocumentSupporter(DocDummyBusinessObject docDummyBusinessObject)
				: base(docDummyBusinessObject)
			{
			}

			public override DocumentSupporterDataState GetDataStateBeforeRun(IStmMenuItem commandAboutToBeRun)
			{
				if (commandAboutToBeRun.SU_MenuName == "Menu Item That Causes Error")
				{
					return new DocumentSupporterDataState(false, "Error occurred");
				}
				else
				{
					return null;
				}
			}

			public override BusinessContext BusinessContext
			{
				get { return BusinessContext.Shipment; }
			}

			public override string GetFilterValue(DocumentFilters filterName)
			{
				string value = null;

				switch (filterName)
				{
					case DocumentFilters.BUY:
						value = GlbStaff.CurrentUser.PK.ToString();
						break;

					default:
						value = base.GetFilterValue(filterName);
						break;
				}

				return value;
			}

			protected override DocumentWrapper[] GetDocumentWrappersInternal(Enterprise.Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
			{
				return new DocumentWrapper[] { new Enterprise.DocumentEngine.Testing.DummyDocumentWrapper(BusinessObject, Factory) };
			}

			protected override Core.Constants.DataContext[] GetSupportedDataContexts()
			{
				return new Core.Constants.DataContext[] { Core.Constants.DataContext.Shipment };
			}

			public override ISecurityCheckpoint CustomisationSecurityCheckpoint { get { return Env.Security.None; } }

			public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contact, DocumentDirection direction)
			{
				if (contact == ContactType.Consignee)
				{
					return new OrgHeaderContact(Factory.Load<OrgHeader>(new Guid("47F51331-B556-4BD2-977C-F99132751BA9")), null);
				}
				else
				{
					return null;
				}
			}
		}

		class DocDummyBusinessObject : DummyBusinessObject, IDocumentSupportable, IDocumentEventsForMenu
		{
			public DocDummyBusinessObject(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public DocumentSupporter DocumentSupporter
			{
				get { return new DocDummyBusinessObjectDocumentSupporter(this); }
			}

			#region IDocumentEvents Members

			public event DocumentPrintedEventHandler DocumentPrinted;
			public void NotifyDocumentPrinted(DocumentPrintedEventArgs e)
			{
				if (DocumentPrinted != null)
				{
				}
			}

			public event DocumentPrintedEventHandler DocumentPrePreviewed;
			public void NotifyDocumentPrePreviewed(DocumentPrintedEventArgs e)
			{
				if (DocumentPrePreviewed != null)
				{
				}
			}

			public event DocumentPrintedEventHandler DocumentPrePrinted;
			public void NotifyDocumentPrePrinted(DocumentPrintedEventArgs e)
			{
				if (DocumentPrePrinted != null)
				{
				}
			}

			public event DocumentCancelEventHandler DocumentPrintRequested;

			public void NotifyDocumentPrintRequested(IStmMenuItem menuItem)
			{
				if (DocumentPrintRequested != null)
				{
				}
			}

			public bool CancelPrintRequest
			{
				get { return false; }
			}

			#endregion

		}

		class MyDummyDocWrapper : DocumentWrapper
		{
			public MyDummyDocWrapper()
				: base(new DocumentWrapperForTesting(""), new BusinessObjectFactory())
			{
			}

			public override string ToString()
			{
				return "My ID";
			}
		}

		#endregion

		#endregion
	}

	[UseSnapshotProtection]
	public class DocDeliveryFormTest_BackgroundDelivery : TestCase
	{
		public void TestDeliver_Report_BackgroundDelivery()
		{
			var factory = new BusinessObjectFactory();
			var command = factory.NewWithValidTestData<ReportCommand>();
			factory.Save();

			var pack = new DocumentPack(command);
			var instructions = new DeliveryInstructions(pack);
			var testReport = new Report(pack, null);
			testReport.PrintCopyType = PrintCopyType.ALL;
			instructions.DeliverablesToBePrinted.Add(testReport);
			instructions.Recipients.RemoveAndDeleteAll();
			DocDeliveryContact contact = instructions.Recipients.AddNew();
			contact.DeliveryMethod = "EML";
			contact.AttachmentType = "PDF";
			contact.DeliveryAddress = "test@edi.com.au";

			Env.Registry.DeliverReportsInBackground = true;
			using (var form = new MockDocDeliveryForm(instructions))
			{
				form.Show();
				form.DeliverButton.PerformClick();
				AssertEquals("No errors", false, instructions.HasErrors);
				AssertNull("No errors", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("DialogResult", DialogResult.Cancel, form.DialogResult);
				AssertNotNull("Schedule created", form.scheduleTask);
				AssertEquals(true, form.scheduleTask.IsInDatabase);
				AssertEquals(false, form.scheduleTask.IsDeleted);
				AssertEquals(true, form.scheduleTask.S5_IsActive);
				AssertEquals(true, form.scheduleTask.S5_IsPrivate);
				AssertEquals(false, testReport.ShouldUpdateSchedulableFilters);
				Assert(form.scheduleTask.S5_NextScheduledPrintRunTimeUtc <= ZDateTime.UtcNow);
				var notifications = new NotificationBuffer();
				new ScheduleTaskRunner().Process(StmMenuItemSchema.Constants.Prefix, true, notifications, new CancellationToken());
				Assert(notifications.AsString, !notifications.HasErrors);
				var scheduleTask = new BusinessObjectFactory().Load(form.scheduleTask.GetType(), form.scheduleTask.PK);
				AssertNull(scheduleTask);
			}
		}

		[TestTimeZoneUNLOCO("NZAKL")]
		public void TestScheduleTaskRunsInBackgroundInNZ()
		{
			AssertScheduleTaskRunsImmediately();
		}

		[TestTimeZoneUNLOCO("AUPER")]
		public void TestScheduleTaskRunsInBackgroundInPerth()
		{
			AssertScheduleTaskRunsImmediately();
		}

		[TestTimeZoneUNLOCO("USLAX")]
		public void TestScheduleTaskRunsInBackgroundInLosAngeles()
		{
			AssertScheduleTaskRunsImmediately();
		}

		void AssertScheduleTaskRunsImmediately()
		{
			DeliveryInstructions instructions = DocDeliveryFormTestHelper.CreateInstructionsWithValidData(new BusinessObjectFactory());
			instructions.Recipients[0].DeliveryMethod = "xxx";

			try
			{
				using (var form = new MockDocDeliveryForm(instructions))
				{
					form.Show();
					instructions.BackgroundDelivery = false;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;

					form.DeliverButton.PerformClick();
					AssertNull("scheduleTask not created", form.scheduleTask);

					instructions.Recipients[0].DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
					instructions.Recipients[0].DeliveryAddress = "bob@bob.com";
					form.DeliverButton.PerformClick();
					AssertNotNull("scheduleTask created", form.scheduleTask);
					Assert("scheduleTask saved", form.scheduleTask.IsInDatabase);
					Assert(!form.scheduleTask.IsDeleted);
					new ScheduleTaskRunner().Process(StmMenuItemSchema.Constants.Prefix, true, new NotificationBuffer(), new CancellationToken());
					var scheduleTask = new BusinessObjectFactory().Load(form.scheduleTask.GetType(), form.scheduleTask.PK);
					AssertNull("scheduleTask.IsDeleted - Will not be deleted unless the task was run.", scheduleTask);
				}
			}
			finally
			{
				instructions?.DocPack?.Dispose();
			}
		}
	}
}

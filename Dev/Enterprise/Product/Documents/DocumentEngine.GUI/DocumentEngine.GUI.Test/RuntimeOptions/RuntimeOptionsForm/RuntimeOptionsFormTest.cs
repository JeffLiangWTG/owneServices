using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Bi.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.ExcelTemplates;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions.Testing
{
	sealed class RuntimeOptionsFormTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestSaveReportStatistic()
		{
			using (var documentPack = new DocumentPack(GetReportCommandWithTemplate("Empty But Valid Report", EmptyAndValidTemplate)))
			{
				var report = (Report)documentPack[0];
				var deliveryInstructions = new DeliveryInstructions(documentPack);
				using (var printTask = new PrintTask())
				{
					printTask.Add(documentPack);
					StmReportRun reportStatistic = null;
					var saveCount = 0;
					using (var overrider = new PrintTaskUIProviderFactory.OverriderForTesting(new PrintTaskFormsProviderThatAutomaticallyDeliversForTest()))
					using (var runtimeOptionsForm = new RuntimeOptionsFormForTest(printTask, report, AllowedDeliveryOptions.All, deliveryInstructions, Env.Security.None))
					{
						runtimeOptionsForm.Show();
						BusinessObjectFactory.SetOnFactorySaveInTransactionForTest(factory =>
						{
							var stmReportRuns = (factory as IBusinessObjectFactoryInternals).AllBusinessObjects.OfType<StmReportRun>()
								.Where(stmReportRun => stmReportRun.RRI_ReportDescription == StmReportRun.PreviewTaskDescription);
							if (reportStatistic == null && stmReportRuns.Any())
							{
								saveCount++;
								reportStatistic = stmReportRuns.First();
							}
						});
						runtimeOptionsForm.PreviewButton_Click(runtimeOptionsForm, EventArgs.Empty);

						AssertEquals("Save StmReportRun only once", 1, saveCount);
						AssertEquals("StmReportRun is saved only when the status is FIN", Core.Constants.StmReportRunState.Finished, reportStatistic.RRI_Status);
						var previewForm = ZApplication.GetOpenForms().OfType<XLSPreviewForm>().FirstOrDefault();
						previewForm.Dispose();
					}
				}
			}
		}

		public void TestPreviewWhenHideSheetIfConditionallyHidden()
		{
			var content = new Dictionary<string, string>();
			content.Add("Sheet1",
	@"{A}-[#Config]
{A}-[Name=Test]
{A}-[Data:ReportData=SELECT 'Jerry Test Sheet1' as TextContent]
{A}-[HideSheetIf=""<Filter Text>""==""ShowSheet2""]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.TextContent>]
{A}-[#EndOfReport]");

			content.Add("Sheet2",
	@"{A}-[#Config]
{A}-[Name=Test]
{A}-[Data:ReportData=SELECT 'Jerry Test Sheet2' as TextContent]
{A}-[HideSheetIf=""<Filter Text>""==""ShowSheet1""]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.TextContent>]
{A}-[#EndOfReport]");

			content.Add("Filter",
				@"{A}-[Filter Text] {B}-[Type] {C}-[Text]
{A}-[#End]");

			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty, content);
			var reportCommand = DocumentEngineTestHelper.CreateReportCommandWithExcelTemplate("Test HideSheetIf", excelTemplate, Factory);
			Factory.Save();

			using (var documentPack = new DocumentPack(reportCommand))
			{
				var report = (Report)documentPack[0];
				var deliveryInstructions = new DeliveryInstructions(documentPack);

				using (var printTask = new PrintTask())
				{
					printTask.Add(documentPack);

					using (var runtimeOptionsForm = new RuntimeOptionsForm(printTask, report, AllowedDeliveryOptions.All, deliveryInstructions, Env.Security.None))
					{
						runtimeOptionsForm.Show();

						var textField = (TextField)report.FilterCollection["Filter Text"];
						textField.Value = "ShowSheet2";

						runtimeOptionsForm.PreviewButton.PerformClick();

						var previewForm = ZApplication.GetOpenForms().OfType<XLSPreviewForm>().FirstOrDefault();

						try
						{
							AssertEquals("Should have 1 SheetNames", 1, previewForm.SheetNames.Count);
							AssertEquals("Should have Sheet2", "Sheet2", previewForm.SheetNames[0].StrictName);
						}
						finally
						{
							previewForm.Dispose();
						}
					}
				}
			}
		}

		public void TestTimeoutExceptionShouldBeShownToUser()
		{
			var template = ConfigurableTemplateTestHelper.SetupSystemTemplateFromString(Factory,
@"{A}-[#Config]
{A}-[DataContext=None]
{A}-[Name=Test Report]
{A}-[Data:ReportDataTest=Select * from dbo.StmALog]
{A}-[#SectionBody:Data=ReportDataTest]
{B}-[<ReportDataTest.SL_Table>]
{A}-[#EndOfReport]");

			var reportCommand = Factory.New<ReportCommand>();
			reportCommand.SU_MenuName = "Test Report";

			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_DocumentTitle = "Test Document";
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;

			Factory.Save();

			using (DocumentsDataRegistry.Instance.ReportPreviewTimeout.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 30))
			using (var documentPack = new DocumentPack(reportCommand))
			{
				var report = (Report)documentPack[0];
				var deliveryInstructions = new DeliveryInstructions(documentPack);

				using (var printTask = new PrintTask())
				{
					Db.Connection.ExecuteNonQuery("update dbo.stmdata set SD_BinaryValue = convert(varbinary(8000), N'0') where SD_Name like 'ReportPreviewTimeout'");
					printTask.Add(documentPack);

					using (DocumentsDataRegistry.Instance.EnableSpecificPageRangesPrintingOption.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
					using (var runtimeOptionsForm = new RuntimeOptionsForm(printTask, report, AllowedDeliveryOptions.All, deliveryInstructions, Env.Security.None))
					{
						DocumentsDataRegistry.Instance.RemoveItemFromCacheIfOlderThan("ReportPreviewTimeout", new TimeSpan(1));

						runtimeOptionsForm.Show();
						AssertNoExceptionThrown(runtimeOptionsForm.ActionButton.PerformClick);
					}

					AssertExceptionThrown<DocumentEngineException>(() => printTask.Run(deliveryInstructions));
					AssertContains(@"Severity: [Fatal Error (without error report)] Message: [During the running of this report, the query timed out.
This could be because the server is very busy or the report is complex and runs on a large data set and needs a longer query timeout.", report.ErrorManager.ToString());
				}
			}
		}

		public void TestMaximumSizeAndMinimumSize()
		{
			var content = new Dictionary<string, string>();
			content.Add("Sheet1",
	@"{A}-[#Config]
{A}-[Name=Test]
{A}-[Data:ReportData=SELECT 'Jerry Test Sheet1' as TextContent]
{A}-[HideSheetIf=""<Filter Text>""==""ShowSheet2""]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.TextContent>]
{A}-[#EndOfReport]");

			content.Add("Sheet2",
	@"{A}-[#Config]
{A}-[Name=Test]
{A}-[Data:ReportData=SELECT 'Jerry Test Sheet2' as TextContent]
{A}-[HideSheetIf=""<Filter Text>""==""ShowSheet1""]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.TextContent>]
{A}-[#EndOfReport]");

			content.Add("Filter",
				@"{A}-[Filter Text] {B}-[Type] {C}-[Text]
{A}-[#End]");

			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty, content);
			var reportCommand = DocumentEngineTestHelper.CreateReportCommandWithExcelTemplate("Test HideSheetIf", excelTemplate, Factory);

			Factory.Save();

			using (var documentPack = new DocumentPack(reportCommand))
			{
				var report = (Report)documentPack[0];
				var deliveryInstructions = new DeliveryInstructions(documentPack);

				using (var printTask = new PrintTask())
				{
					printTask.Add(documentPack);

					using (var runtimeOptionsForm = new RuntimeOptionsForm(printTask, report, AllowedDeliveryOptions.All, deliveryInstructions, Env.Security.None))
					{
						runtimeOptionsForm.Show();

						AssertEquals(ControlDpiScalingHelper.NewScaledSize(595, 200, false), runtimeOptionsForm.MinimumSize);
						Assert(runtimeOptionsForm.MaximumSize.Width.ToString(), runtimeOptionsForm.MaximumSize.Width >= 595);
						Assert(runtimeOptionsForm.MaximumSize.Height.ToString(), runtimeOptionsForm.MaximumSize.Height >= 200);
					}
				}
			}
		}

		public void TestMaximumSizeIsNotSetWhenDisplayingError()
		{
			using (var report = new Report(new DocumentPack(), EmptyAndValidTemplate))
			using (var form = new RuntimeOptionsForm(report))
			{
				form.Show();
				AssertEquals(default(Size), form.MaximumSize);
			}
		}

		public void TestShouldHandleExceptionForCustomizeReportWhenShowDeliveryForm()
		{
			var template = ConfigurableTemplateTestHelper.SetupSystemTemplateFromString(Factory,
@"{A}-[#Config]
{A}-[DataContext=None]
{A}-[Name=Test Report]
{A}-[Data:ReportDataTest=select [ORGCODE] from AddressProfile('330860ba-e97d-4be4-9830-9cc80c2fbde5', <Fallback Address>, <Address Type>, <Organization Type> ) WHERE (Active = Active) AND (OrgReported = Payables)]
{A}-[#SectionBody:Data=ReportDataTest]
{B}-[<ReportDataTest.ORGCODE>]
{A}-[#EndOfReport]");

			var reportCommand = Factory.New<ReportCommand>();
			reportCommand.SU_MenuName = "Test Report";

			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_DocumentTitle = "Test Document";
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;

			Factory.Save();

			using (Report.TemporarilyUseMainConnection())
			using (var documentPack = new DocumentPack(reportCommand))
			{
				var report = (Report)documentPack[0];
				var deliveryInstructions = new DeliveryInstructions(documentPack);

				using (var printTask = new PrintTask())
				{
					printTask.Add(documentPack);

					using (DocumentsDataRegistry.Instance.EnableSpecificPageRangesPrintingOption.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
					using (var runtimeOptionsForm = new RuntimeOptionsForm(printTask, report, AllowedDeliveryOptions.All, deliveryInstructions, Env.Security.None))
					{
						runtimeOptionsForm.Show();

						AssertNoExceptionThrown(runtimeOptionsForm.ActionButton.PerformClick);
					}

					AssertExceptionThrown<DocumentEngineException>(() => printTask.Run(deliveryInstructions));
					var error = string.Format(@"Severity: [Fatal Error (without error report)] Message: [Error loading table [ReportDataTest]. Error: [Incorrect syntax near '<'.] occurred running SQL: [
--Udf Parameters: 

--Server: {0}

--WhereClause Parameters: 

--Report Name: Test Document

--Staff Name: CargoWise Support

--Time Out: 300

select [ORGCODE] from AddressProfile('330860ba-e97d-4be4-9830-9cc80c2fbde5', <Fallback Address>, <Address Type>, <Organization Type> ) WHERE (Active = Active) AND (OrgReported = Payables) option (recompile)

]. SqlException: Msg 102, Level 15, State 1, Line 8, Incorrect syntax near '<'.] Cell: [N/A] Sheetname: [(unknown)]", report.RunningConnection.ServerName);
					AssertContains(error, report.ErrorManager.ToString());
				}
			}
		}

		[RequiresSTA]
		public void TestConfigurationManagementShouldShowWhenOrientationIsShown()
		{
			var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.DocumentTestFiles.RowsToRepeatAtTop.xls", "RowsToRepeatAtTop.xls");
			var excelTemplate = new ExcelTemplateForUnitTesting("RowsToRepeatAtTop.xls", Path.GetFullPath(tempFileName));

			var template = Factory.NewWithValidTestData<StmTemplateBase>();
			template.SO_Name = "NewLine Template 1";
			template.SO_Template = excelTemplate.GetAsByteArray();

			var command = Factory.NewWithValidTestData<ReportCommand>();
			command.SU_MenuName = "NewLine Report 1";

			var document = command.Documents.AddNew();
			document.SI_SU = command.PK;
			document.SI_SO = template.PK;

			Factory.Save();

			using (var printSet = new ReportPrintSet(command))
			using (var form = new RuntimeOptionsForm(printSet, printSet.GetDocumentPacks().First().GetFirstReport(), AllowedDeliveryOptions.All, new DeliveryInstructions(), Env.Security.None))
			{
				Assert(form.ColumnArrangementGroupBox_Exposed != null);
			}
		}

		public void TestShouldRememberPositionAndSizeIsFalse()
		{
			using (var form = new RuntimeOptionsForm())
			{
				Assert(!(bool)form.GetType().GetProperty("ShouldRememberPositionAndSize", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form));
			}
		}

		public void TestOpenedFormCacheUsage()
		{
			var currentAdditionalFormsCountCount = OpenedFormCache.GetInstance().AdditionalFormsCount;

			using (var report = new Report(new DocumentPack(), EmptyAndValidTemplate))
			{
				using (var form = new RuntimeOptionsForm(new PrintTask(), report, AllowedDeliveryOptions.All, new DeliveryInstructions(), Env.Security.None))
				{
					form.Show();
					AssertEquals(currentAdditionalFormsCountCount + 1, OpenedFormCache.GetInstance().AdditionalFormsCount);
				}
				AssertEquals(currentAdditionalFormsCountCount, OpenedFormCache.GetInstance().AdditionalFormsCount);

				using (var form = new RuntimeOptionsForm(report))
				{
					form.Show();
					AssertEquals(currentAdditionalFormsCountCount + 1, OpenedFormCache.GetInstance().AdditionalFormsCount);
				}
				AssertEquals(currentAdditionalFormsCountCount, OpenedFormCache.GetInstance().AdditionalFormsCount);

				using (var form = new RuntimeOptionsForm(report, Factory.NewWithValidTestData<ReportScheduleTask>()))
				{
					form.Show();
					AssertEquals(currentAdditionalFormsCountCount + 1, OpenedFormCache.GetInstance().AdditionalFormsCount);
				}
				AssertEquals(currentAdditionalFormsCountCount, OpenedFormCache.GetInstance().AdditionalFormsCount);

				using (var form = new RuntimeOptionsForm(report, AllowedDeliveryOptions.All, new DeliveryInstructions(), Env.Security.None, false))
				{
					form.Show();
					AssertEquals(currentAdditionalFormsCountCount + 1, OpenedFormCache.GetInstance().AdditionalFormsCount);
				}
				AssertEquals(currentAdditionalFormsCountCount, OpenedFormCache.GetInstance().AdditionalFormsCount);
			}
		}

		[RequiresSTA]
		public void TestErrorLabelWhenCustomizedDocumentElementsExist()
		{
			var systemTemplate = ConfigurableTemplateTestHelper.SetupSystemTemplateFromString(Factory,
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#ConfigurableSection:GEN, My Section]
{B}-[Hello World]
{A}-[#EndOfReport]");

			var helper = new TemplateTestHelper();
			helper.AddWorkSheet("Document",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#ConfigurableSection:GEN, My Other Section]
{B}-[<Z0_DoesNotExist>]
{A}-[#EndOfReport]");
			helper.CreateTemplate(Factory, SectionRepositoryTemplateNames.User);

			var dummy = Factory.New<DummyBODocSupportable>();
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var document = documentCommand.Documents.AddNew();
			document.SI_SU = documentCommand.PK;
			document.SI_SO = systemTemplate.PK;

			var config = document.DocConfigs.AddNew();
			config.S3_SI = document.PK;

			var configItem1 = config.ConfigItems.AddNew();
			configItem1.S4_S3 = document.PK;
			configItem1.S4_SectionItemName = "My Section";
			configItem1.S4_SectionType = "BDY";

			var configItem2 = config.ConfigItems.AddNew();
			configItem2.S4_S3 = document.PK;
			configItem2.S4_SectionItemName = "My Other Section";
			configItem2.S4_SectionType = "BDY";

			using (var documentPack = new DocumentPack(documentCommand, dummy, null, documentCommand))
			{
				using (var report = documentPack.GetFirstReport())
				{
					using (var stream = new MemoryStream())
					{
						AssertExceptionThrown("Exception should be thrown.", typeof(DocumentEngineException), () => report.Save(stream));
					}

					var deliveryInstructions = new DeliveryInstructions();

					using (var form = new RuntimeOptionsForm(report, AllowedDeliveryOptions.All, deliveryInstructions, Env.Security.None))
					{
						AssertMultilineASCIIEquals("form.ErrorLabel",
@"ERRORS WERE FOUND during the generation of this template, but the document is still able to be produced. Some fields may be missing data, but the general layout of the document will probably be OK. 

If you want to produce the document 'As is', click 'Continue' or click 'Cancel' to exit.

WARNING: Customized Document Elements used, please check any custom strips used in this document.",
							form.ErrorLabel);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[RequiresSTA]
		public void TestAdjustLabelsToFitContentDoesNotExceedMaxSizeOfColumn()
		{
			using (Res.TemporarilySwitchLanguage("FRN"))
			using (var report = new Report(new DocumentPack(), new ExcelTemplateForUnitTesting(BaseSourcePath + @"Enterprise\Product\Documents\ExcelTemplates\Reports\Job Profit - Shipping.xls", TestFilesSubFolder.ReportTestFiles)))
			{
				report.PrepareForRender();
				using (var runtimeOptionsForm = new RuntimeOptionsForm(report, AllowedDeliveryOptions.All, null, null))
				{
					runtimeOptionsForm.Show();
					var controlWithZLabel = new string[] { "DateRangeFieldUserControl", "ColumnConfigurationFieldUserControl", "DateFieldUserControl", "OptionGroupUserControl", "MultipleSelectionLookupUserControl" };
					foreach (var key in controlWithZLabel)
					{
						foreach (var ctrl in runtimeOptionsForm.Controls.Find(key, true))
						{
							AssertGreaterThanOrEqualTo(ControlDpiScalingHelper.ScaleToCurrentDpiX(500), ctrl.Width);
						}
					}
				}
			}
		}

#if !WINZOR

		[RequiresSTA]
		public void TestLanguageOfDateRangeFilter()
		{
			var helper = new TemplateTestHelper();
			helper.AddWorkSheet(@"Document",
@"{A}-[#Config]
{A}-[Name=Dummy Report]
{A}-[#SectionBody]
{B}-[Hello World]
{A}-[#EndOfReport]");
			helper.AddWorkSheet(@"Filters",
@"{A}-[My Filter]    {B}-[Type]    {C}-[Date Range]
{B}-[Language]    {C}-[ZH-CN]
{A}-[#End]");

			var template = helper.CreateTemplate(Factory, "Dummy Report");
			var excelTemplate = template.GetExcelTemplate();

			using (var mockChs = Res.GetLanguageInstance(Core.SharedConstants.Languages.ChineseSimplified).UseMockData())
			{
				var resourceStringsKey = DocBuilderResourceStrings.GetKey("", DocBuilderResourceStrings.ReportLabelKeyPrefix, "My Filter");
				mockChs.Put(resourceStringsKey, new ResourceStringData(resourceStringsKey, "我的过滤器"));
				mockChs.Put("DateRangeFieldUserControl|e576fb7f-c061-463d-8d69-8440d39aee4c", new ResourceStringData("DateRangeFieldUserControl|e576fb7f-c061-463d-8d69-8440d39aee4c", "从"));
				mockChs.Put("DateRangeFieldUserControl|855bb905-c365-4759-ba65-74408fa4ef5d", new ResourceStringData("DateRangeFieldUserControl|855bb905-c365-4759-ba65-74408fa4ef5d", "到"));

				using (var documentPack = new DocumentPack())
				using (var report = new Report(new DocumentPack(), excelTemplate))
				{
					var stmTemplate = Factory.New<StmTemplate>();
					stmTemplate.SO_IsSystemDefined = true;
					stmTemplate.SO_Name = "LegacyDocument";
					stmTemplate.SO_ExcelTemplatePath = report.Template.TemplateSourceLocation;
					report.StTemplate = stmTemplate;
					report.PrepareForRender();

					using (var runtimeOptionsForm = new RuntimeOptionsForm(report, AllowedDeliveryOptions.All, null, null))
					{
						runtimeOptionsForm.Show();
						var labels = runtimeOptionsForm.Controls.Find("DateRangeFieldUserControl", true)[0].Find(cc => cc is Label).ToArray();
						AssertEquals("我的过滤器", labels[0].Text);
						AssertEquals("到", labels[1].Text);
						AssertEquals("从", labels[2].Text);
					}
				}
			}
		}

#endif

		[RequiresSTA]
		public void TestErrorLabel()
		{
			var systemTemplate = ConfigurableTemplateTestHelper.SetupSystemTemplateFromString(Factory,
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#ConfigurableSection:GEN, My Section]
{B}-[<Z0_DoesNotExist>]
{A}-[#EndOfReport]");

			var dummy = Factory.New<DummyBODocSupportable>();
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var document = documentCommand.Documents.AddNew();
			document.SI_SU = documentCommand.PK;
			document.SI_SO = systemTemplate.PK;

			var config = document.DocConfigs.AddNew();
			config.S3_SI = document.PK;

			var configItem1 = config.ConfigItems.AddNew();
			configItem1.S4_S3 = document.PK;
			configItem1.S4_SectionItemName = "My Section";
			configItem1.S4_SectionType = "BDY";

			using (var documentPack = new DocumentPack(documentCommand, dummy, null, documentCommand))
			{
				using (var report = documentPack.GetFirstReport())
				{
					using (var stream = new MemoryStream())
					{
						AssertExceptionThrown("Exception should be thrown.", typeof(DocumentEngineException), () => report.Save(stream));
					}

					var deliveryInstructions = new DeliveryInstructions();

					using (var form = new RuntimeOptionsForm(report, AllowedDeliveryOptions.All, deliveryInstructions, Env.Security.None))
					{
						AssertMultilineASCIIEquals("form.ErrorLabel",
@"ERRORS WERE FOUND during the generation of this template, but the document is still able to be produced. Some fields may be missing data, but the general layout of the document will probably be OK. 

If you want to produce the document 'As is', click 'Continue' or click 'Cancel' to exit.",
							form.ErrorLabel);
					}
				}
			}
		}

		public void TestCopyToClipboardButtonOnlyShownWhenReportHasErrors()
		{
			using (var report = new Report(new DocumentPack(), EmptyAndValidTemplate))
			using (var form = new RuntimeOptionsForm(new PrintTask(), report, AllowedDeliveryOptions.All, new DeliveryInstructions(), Env.Security.None))
			{
				form.Show();
				Assert(!form.CopyButton.Visible);
			}

			using (var report = new Report(new DocumentPack(), BadExcelFormatTest))
			using (var form = new RuntimeOptionsForm(new PrintTask(), report, AllowedDeliveryOptions.All, new DeliveryInstructions(), Env.Security.None))
			{
				form.Show();
				Assert(form.CopyButton.Visible);
			}
		}

		public void TestPreviewButtonDisabledWhenReportHasErrors()
		{
			using (var report = new Report(new DocumentPack(), EmptyAndValidTemplate))
			{
				using (var form = new RuntimeOptionsForm(new PrintTask(), report, AllowedDeliveryOptions.All, new DeliveryInstructions(), Env.Security.None))
				{
					form.Show();
					Assert(form.PreviewButton.Visible);
				}
			}

			using (var report = new Report(new DocumentPack(), BadExcelFormatTest))
			{
				using (var form = new RuntimeOptionsForm(new PrintTask(), report, AllowedDeliveryOptions.All, new DeliveryInstructions(), Env.Security.None))
				{
					form.Show();
					Assert(!form.PreviewButton.Visible);
				}
			}
		}

		public void TestDeliverThenPreviewThenClosePreviewDoesntDisposeAsDeliveryMayStillOccur()
		{
			Env.Registry.DeliverReportsInBackground = false;

			AssertEquals("Pre-condition: UnitTestUserNotification.Instance.LastMessage.Text", null, UnitTestUserNotification.Instance.LastMessage.Text);

			using (var documentPack = new DocumentPack(GetReportCommandWithTemplate("Empty But Valid Report", EmptyAndValidTemplate)))
			{
				var report = (Report)documentPack[0];
				var deliveryInstructions = GetDeliveryInstructions(documentPack);

				using (var printTask = new PrintTask())
				{
					printTask.Add(documentPack);

					var uiProvider = new PrintTaskFormsProviderThatAutomaticallyPreviewsFromDocDeliveryFormForTest();
					using (var overrider = new PrintTaskUIProviderFactory.OverriderForTesting(uiProvider))
					using (var runtimeOptionsForm = new RuntimeOptionsFormForTest(printTask, report, AllowedDeliveryOptions.All, deliveryInstructions, Env.Security.None))
					{
						runtimeOptionsForm.Show();

						AssertEquals("Pre-condition: Application.OpenForms.FindAll<XLSPreviewForm>().Count", 0, Application.OpenForms.FindAll<XLSPreviewForm>().Count);
						AssertEquals("Pre-condition: Application.OpenForms.FindAll<DocDeliveryForm>().Count", 0, Application.OpenForms.FindAll<DocDeliveryForm>().Count);
						runtimeOptionsForm.ActionButton.PerformClick();

						AssertEquals("clonedInstructions.DocPack.IsDisposed", true, uiProvider.LastDeliveryInstructions.DocPack.IsDisposed);
					}

					AssertEquals("UnitTestUserNotification.Instance.LastMessage.Text", null, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		[RequiresSTA]
		public void TestChangeReportFiltersDoesNotThrowExceptionIfNonMatchingFilter()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "ABCDEFG";
			Factory.RefreshEnabled = false;
			Factory.Save();

			var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.MultipleSelectionLookup.xls", "MultipleSelectionLookup.xls");
			var template = new ExcelTemplateForUnitTesting("MultipleSelectionLookup.xls", Path.GetFullPath(tempFileName));

			using (var documentPack = new DocumentPack(GetReportCommandWithTemplate("A Menu Name", template)))
			using (var report = (Report)documentPack[0])
			{
				report.PrepareForRender();
				var orgFilter = (MultipleSelectionLookup)report.FilterCollection["Orgs"];
				orgFilter.ValueAsStringForSerialisation = "ABCDEFG";
				var newFactory = new BusinessObjectFactory();
				newFactory.RefreshEnabled = false;

				var reloadedOrg = newFactory.Load<OrgHeader>(org1.PK);
				reloadedOrg.OH_Code = "CODECHANGED";
				newFactory.Save();

				using (var form = new RuntimeOptionsForm(report, Factory.NewWithValidTestData<ReportScheduleTask>()))
				{
					AssertNoExceptionThrown(form.Show);
				}
			}
		}

		public void TestPreviewThenDeliver()
		{
			Env.Registry.DeliverReportsInBackground = false;

			AssertEquals("Pre-condition: UnitTestUserNotification.Instance.LastMessage.Text", null, UnitTestUserNotification.Instance.LastMessage.Text);

			using (var documentPack = new DocumentPack(GetReportCommandWithTemplate("Empty But Valid Report", EmptyAndValidTemplate)))
			{
				var report = (Report)documentPack[0];
				var deliveryInstructions = new DeliveryInstructions(documentPack);

				using (var printTask = new PrintTask())
				{
					printTask.Add(documentPack);

					using (var overrider = new PrintTaskUIProviderFactory.OverriderForTesting(new PrintTaskFormsProviderThatAutomaticallyDeliversForTest()))
					using (var runtimeOptionsForm = new RuntimeOptionsForm(printTask, report, AllowedDeliveryOptions.All, deliveryInstructions, Env.Security.None))
					{
						runtimeOptionsForm.Show();

						AssertEquals("GetOpenForms<XLSPreviewForm>().Count", 0, Application.OpenForms.FindAll<XLSPreviewForm>().Count);
						runtimeOptionsForm.PreviewButton.PerformClick();

						AssertEquals("GetOpenForms<XLSPreviewForm>().Count", 1, Application.OpenForms.FindAll<XLSPreviewForm>().Count);
						var previewForm = Application.OpenForms.FindAll<XLSPreviewForm>()[0];

						previewForm.DeliverButton.PerformClick();
					}

					AssertEquals("UnitTestUserNotification.Instance.LastMessage.Text", null, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestPreviewThenGenerateReportStatistics()
		{
			using (var documentPack = new DocumentPack(GetReportCommandWithTemplate("Empty But Valid Report", EmptyAndValidTemplate)))
			{
				var report = (Report)documentPack[0];
				var deliveryInstructions = new DeliveryInstructions(documentPack);
				using (var printTask = new PrintTask())
				{
					printTask.Add(documentPack);
					using (var overrider = new PrintTaskUIProviderFactory.OverriderForTesting(new PrintTaskFormsProviderThatAutomaticallyDeliversForTest()))
					using (var runtimeOptionsForm = new RuntimeOptionsFormForTest(printTask, report, AllowedDeliveryOptions.All, deliveryInstructions, Env.Security.None))
					{
						runtimeOptionsForm.Show();
						runtimeOptionsForm.PreviewButton_Click(runtimeOptionsForm, EventArgs.Empty);

						var zQuery = new ZQuery(StmReportRunSchema.RRI_ReportDescription, StmReportRun.PreviewTaskDescription);
						var stmReportRun = Factory.LoadTop1<StmReportRun>(zQuery);

						AssertNotNull(stmReportRun);
						Assert(stmReportRun.IsPreview);
						AssertEquals(stmReportRun.RRI_GS_NKPrintUser, "E");
						AssertEquals(stmReportRun.RRI_IsSystemDefined, false);
						AssertNotEquals(stmReportRun.RRI_SystemCreateTimeUtc, null);
						AssertGreaterThan(stmReportRun.RRI_SystemCreateUser.Length, 0);

						var previewForm = ZApplication.GetOpenForms().OfType<XLSPreviewForm>().FirstOrDefault();
						previewForm.Dispose();
					}
				}
			}
		}

		public void TestClonedInstructionsAreDisposedWhenPreviewClosedWithoutDelivery()
		{
			AssertEquals("Pre-condition: UnitTestUserNotification.Instance.LastMessage.Text", null, UnitTestUserNotification.Instance.LastMessage.Text);

			using (var documentPack = new DocumentPack(GetReportCommandWithTemplate("Empty But Valid Report", EmptyAndValidTemplate)))
			{
				var report = (Report)documentPack[0];
				var deliveryInstructions = new DeliveryInstructions(documentPack);

				using (var printTask = new PrintTask())
				{
					printTask.Add(documentPack);

					using (var overrider = new PrintTaskUIProviderFactory.OverriderForTesting(new PrintTaskFormsProviderThatAutomaticallyDeliversForTest()))
					using (var runtimeOptionsForm = new RuntimeOptionsFormForTest(printTask, report, AllowedDeliveryOptions.All, deliveryInstructions, Env.Security.None))
					{
						runtimeOptionsForm.Show();

						AssertEquals("GetOpenForms<XLSPreviewForm>().Count", 0, Application.OpenForms.FindAll<XLSPreviewForm>().Count);
						runtimeOptionsForm.PreviewButton.PerformClick();

						var clonedInstructions = runtimeOptionsForm.LastPreviewedDeliveryInstructions;
						AssertNotNull("form.LastPreviewedDeliveryInstructions", clonedInstructions);
						AssertNotNull("clonedInstructions.DocPack", clonedInstructions.DocPack);
						AssertEquals("clonedInstructions.DocPack.IsDisposed", false, clonedInstructions.DocPack.IsDisposed);

						AssertEquals("GetOpenForms<XLSPreviewForm>().Count", 1, Application.OpenForms.FindAll<XLSPreviewForm>().Count);
						var previewForm = Application.OpenForms.FindAll<XLSPreviewForm>()[0];
						previewForm.Close();

						AssertEquals("clonedInstructions.DocPack.IsDisposed", true, clonedInstructions.DocPack.IsDisposed);
					}
				}
			}
		}

		[RequiresSTA]
		public void TestHandleExceptionForCustomizeReportWithPreviewButton()
		{
			var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.InvalidSectionAreaTemplate.xls", "InvalidSectionAreaTemplate.xls");
			var excelTemplate = new ExcelTemplateForUnitTesting("InvalidSectionAreaTemplate.xls", Path.GetFullPath(tempFileName));

			using (var documentPack = new DocumentPack(GetReportCommandWithTemplate("Invalid Template", excelTemplate)))
			{
				var report = (Report)documentPack[0];
				var deliveryInstructions = new DeliveryInstructions(documentPack);

				using (var printTask = new PrintTask())
				{
					printTask.Add(documentPack);

					using (var overrider = new PrintTaskUIProviderFactory.OverriderForTesting(new PrintTaskFormsProviderThatAutomaticallyDeliversForTest()))
					using (var runtimeOptionsForm = new RuntimeOptionsFormForTest(printTask, report, AllowedDeliveryOptions.All, deliveryInstructions, Env.Security.None))
					{
						runtimeOptionsForm.Show();

						runtimeOptionsForm.PreviewButton_Click(runtimeOptionsForm, EventArgs.Empty);
						AssertMultilineASCIIEquals("Report should have errors.", @"There are errors in the report that cannot be previewed.
Area type #SortBy:ReportData.Product is not defined! Error in report TestTemplate", UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
		}

		[RequiresSTA]
		public void TestOptionalTemplatesAreDisplayedInOrder()
		{
			using (var documentPack = new DocumentPack(GetReportCommandWithTemplate("Report with Optional Template", FilterTestTemplate)))
			{
				var report = (Report)documentPack[0];
				var deliveryInstructions = new DeliveryInstructions(documentPack);

				using (var printTask = new PrintTask())
				{
					printTask.Add(documentPack);

					using (var overrider = new PrintTaskUIProviderFactory.OverriderForTesting(new PrintTaskFormsProviderThatAutomaticallyDeliversForTest()))
					using (var runtimeOptionsForm = new RuntimeOptionsFormForTest(printTask, report, AllowedDeliveryOptions.All, deliveryInstructions, Env.Security.None))
					{
						runtimeOptionsForm.Show();
						var optionalTemplateA = runtimeOptionsForm.FindSingle<ZCheckBox>(c => c.Name == "template a");
						var optionalTemplateB = runtimeOptionsForm.FindSingle<ZCheckBox>(c => c.Name == "template b");
						var optionalTemplateC = runtimeOptionsForm.FindSingle<ZCheckBox>(c => c.Name == "template c");
						var optionalTemplateD = runtimeOptionsForm.FindSingle<ZCheckBox>(c => c.Name == "template d");

						CombineAssertions(() =>
						{
							AssertEquals("Template C should be first", 0, optionalTemplateC.TabIndex);
							AssertEquals("Template B should be second", 1, optionalTemplateB.TabIndex);
							AssertEquals("Template D should be third", 2, optionalTemplateD.TabIndex);
							AssertEquals("Template A should be fourth", 3, optionalTemplateA.TabIndex);
						});
					}
				}
			}
		}

		[RequiresSTA]
		public void TestAutoWidth()
		{
			var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.WidthTestTemplate.xls", "WidthTestTemplate.xls");
			var excelTemplate = new ExcelTemplateForUnitTesting("WidthTestTemplate.xls", Path.GetFullPath(tempFileName));
			using (var documentPack = new DocumentPack(GetReportCommandWithTemplate("Report with Optional Template", excelTemplate)))
			{
				var report = (Report)documentPack[0];
				var deliveryInstructions = new DeliveryInstructions(documentPack);

				using (var printTask = new PrintTask())
				{
					printTask.Add(documentPack);

					using (var overrider = new PrintTaskUIProviderFactory.OverriderForTesting(new PrintTaskFormsProviderThatAutomaticallyDeliversForTest()))
					using (var runtimeOptionsForm = new RuntimeOptionsFormForTest(printTask, report, AllowedDeliveryOptions.All, deliveryInstructions, Env.Security.None))
					{
						runtimeOptionsForm.Show();
						var controls = runtimeOptionsForm.Find(c => c.Name == "template a");

						var maxWidth = (int)Math.Ceiling((double)TextRenderer.MeasureText("template a", runtimeOptionsForm.Controls.Find("OptionalTemplatesGroupBox", true).First().Font).Width);
						maxWidth += ControlDpiScalingHelper.ScaleToCurrentDpiX(24);

						foreach (var control in controls)
						{
							AssertEquals("Font when measureText should be parent's font", maxWidth, control.Width);
						}
					}
				}
			}
		}

		[RequiresSTA]
		public void TestOptionalTemplatesAreDisplayedInOrder_ProperlySelectsTemplatesForScheduleReport()
		{
			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			using (var report = new Report(new DocumentPack(Factory.New<StmMenuItem>()), FilterTestTemplate))
			{
				report.PrepareForRender();
				report.OptionalTemplateSheetCollection["template b"].Selected = true;
				report.OptionalTemplateSheetCollection["template d"].Selected = true;
				scheduleTask.S5_ScheduleState = scheduleTask.Serialize(report);
			}

			using (var report = new Report(new DocumentPack(Factory.New<StmMenuItem>()), FilterTestTemplate))
			using (var runtimeOptionsForm = new RuntimeOptionsForm(report, scheduleTask))
			{
				runtimeOptionsForm.Show();

				var optionalTemplateA = runtimeOptionsForm.FindSingle<ZCheckBox>(c => c.Name == "template a");
				var optionalTemplateB = runtimeOptionsForm.FindSingle<ZCheckBox>(c => c.Name == "template b");
				var optionalTemplateC = runtimeOptionsForm.FindSingle<ZCheckBox>(c => c.Name == "template c");
				var optionalTemplateD = runtimeOptionsForm.FindSingle<ZCheckBox>(c => c.Name == "template d");

				CombineAssertions(() =>
				{
					AssertEquals("Template C should be first", 0, optionalTemplateC.TabIndex);
					AssertEquals("Template B should be second", 1, optionalTemplateB.TabIndex);
					AssertEquals("Template D should be third", 2, optionalTemplateD.TabIndex);
					AssertEquals("Template A should be fourth", 3, optionalTemplateA.TabIndex);

					Assert("Template C should not be selected", !optionalTemplateC.Checked);
					Assert("Template B should be selected", optionalTemplateB.Checked);
					Assert("Template D should be selected", optionalTemplateD.Checked);
					Assert("Template A should not be selected", !optionalTemplateA.Checked);
				});
			}
		}

		public void TestClonedInstructionsAreNotDisposedWhenDeliveringFromPreview()
		{
			Env.Registry.DeliverReportsInBackground = false;

			AssertEquals("Pre-condition: UnitTestUserNotification.Instance.LastMessage.Text", null, UnitTestUserNotification.Instance.LastMessage.Text);

			using (var documentPack = new DocumentPack(GetReportCommandWithTemplate("Empty But Valid Report", EmptyAndValidTemplate)))
			{
				var report = (Report)documentPack[0];
				var deliveryInstructions = new DeliveryInstructions(documentPack);

				using (var printTask = new PrintTask())
				{
					printTask.Add(documentPack);

					using (var overrider = new PrintTaskUIProviderFactory.OverriderForTesting(new PrintTaskFormsProviderThatAutomaticallyDeliversForTest()))
					using (var runtimeOptionsForm = new RuntimeOptionsFormForTest(printTask, report, AllowedDeliveryOptions.All, deliveryInstructions, Env.Security.None))
					{
						runtimeOptionsForm.Show();

						AssertEquals("GetOpenForms<XLSPreviewForm>().Count", 0, Application.OpenForms.FindAll<XLSPreviewForm>().Count);
						runtimeOptionsForm.PreviewButton.PerformClick();

						var clonedInstructions = runtimeOptionsForm.LastPreviewedDeliveryInstructions;
						AssertNotNull("form.LastPreviewedDeliveryInstructions", clonedInstructions);
						AssertNotNull("clonedInstructions.DocPack", clonedInstructions.DocPack);
						AssertEquals("clonedInstructions.DocPack.IsDisposed", false, clonedInstructions.DocPack.IsDisposed);

						AssertEquals("GetOpenForms<XLSPreviewForm>().Count", 1, Application.OpenForms.FindAll<XLSPreviewForm>().Count);
						var previewForm = Application.OpenForms.FindAll<XLSPreviewForm>()[0];
						previewForm.DeliverButton.PerformClick();

						AssertEquals("clonedInstructions.DocPack.IsDisposed", true, clonedInstructions.DocPack.IsDisposed);
					}
				}
			}
		}

		[RequiresSTA]
		public void TestActionAndCloseButtonText()
		{
			using (var form = new RuntimeOptionsForm())
			{
				AssertEquals("form.ActionButton.Text", "OK", form.ActionButton.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("form.CloseButton.Text", "&Cancel", form.CloseButton.GetExtension<ILabelCaptionRenderer>().Caption);
			}

			using (var report = new Report(new DocumentPack(), NewLineXls))
			using (var form = new RuntimeOptionsForm(new PrintTask(), report, AllowedDeliveryOptions.All, new DeliveryInstructions(), Env.Security.None))
			{
				AssertEquals("form.ActionButton.Text", "&Deliver", form.ActionButton.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("form.CloseButton.Text", "&Close", form.CloseButton.GetExtension<ILabelCaptionRenderer>().Caption);
			}
		}

		public void TestRuntimeOptionsFormDoesntShowForDocumentPrintsWithNoErrors()
		{
			var command = Factory.New<ReportCommand>();
			command.SU_MenuName = "Test Report";

			var template = Factory.New<StmTemplateBase>();
			template.SO_Name = "Test Template";
			template.SO_Template = NewLineXls.GetAsByteArray();

			var menuTemplatePivot = Factory.New<StmMenuTemplatePivotBase>();
			menuTemplatePivot.SI_DocumentTitle = "Test Document";
			menuTemplatePivot.SI_SU = command.PK;
			menuTemplatePivot.SI_SO = template.PK;
			Factory.Save();

			using (var printSet = new ReportPrintSet(command))
			{
				var deliveryInstruction = new DeliveryInstructions(printSet[0]);
				var contact = deliveryInstruction.Recipients[0];
				contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
				contact.Email = "test@test.com";
				deliveryInstruction.Destination = DeliveryInstructionDestination.TakenFromContact;
				using (var printTask = new PrintTask())
				{
					printTask.Add(printSet[0]);
					printTask.IsReportPrintSet = true;

					SimulateRequestDelivery(printTask, deliveryInstruction.DeliveryOptions, deliveryInstruction, Env.Security.None);

					AssertNotNull("LastFormShownDialogForTest should not be null", ZFormModaliser.LastFormShownDialogForTest);
					AssertEquals("LastFormShownDialogForTest should be a DocDeliveryForm", typeof(DocDeliveryForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				}
			}
		}

		[GuiTest]
		[RequiresSTA]
		public void TestLanguageRestoredAfterLoadingConfiguration()
		{
			using (var documentPack = new DocumentPack(GetReportCommandWithTemplate("Empty But Valid Report", EmptyAndValidTemplate)))
			{
				var report = (Report)documentPack[0];
				var deliveryInstructions = new DeliveryInstructions(documentPack);

				using (var printTask = new PrintTask())
				{
					printTask.Add(documentPack);

					using (var runtimeOptionsForm = new RuntimeOptionsForm(printTask, report, AllowedDeliveryOptions.All, deliveryInstructions, Env.Security.None))
					{
						runtimeOptionsForm.Show();
						deliveryInstructions.Language = Core.SharedConstants.Languages.ChineseSimplified;
						AssertEquals(Core.SharedConstants.Languages.ChineseSimplified, deliveryInstructions.Language);
						report.ColumnHeadingManager.DefaultTemplateConfigurationManager.Load(report.FilterCollection, report.GroupByCollection, report.SortOrderCollection, report.OrientationManager, report.Parent);
						AssertEquals(Core.SharedConstants.Languages.EnglishAmerican, deliveryInstructions.Language);
					}
				}
			}
		}

		public void TestRuntimeOptionsFormShouldNotShowTimeOverrideControl_WhenIsErrorForm()
		{
			using (var runtimeOptionsForm = new RuntimeOptionsForm())
			{
				runtimeOptionsForm.Show();
				Assert("TimeOutOverride control should be visible", runtimeOptionsForm.TimeOutEdit.Visible);
			}

			using (var report = new Report(new DocumentPack(), EmptyAndValidTemplate))
			using (var runtimeOptionsForm = new RuntimeOptionsForm(report))
			{
				runtimeOptionsForm.Show();
				Assert("TimeOutOverride control should not be visible when Runtime options form is an error form", !runtimeOptionsForm.TimeOutEdit.Visible);
			}
		}

		public void TestEdwDataSourceCheckBoxVisible()
		{
			using (var runtimeOptionsForm = new RuntimeOptionsForm())
			{
				runtimeOptionsForm.Show();

				Assert("Should not be visible", !runtimeOptionsForm.EdwDataSourceCheckBox.Visible);
			}

			var stmMenuItem = Factory.LoadTop1<StmMenuItem>(new ZQuery());
			using (var report = new Report(new DocumentPack(stmMenuItem), EmptyAndValidTemplate))
			using (var runtimeOptionsForm = new RuntimeOptionsForm(report))
			{
				runtimeOptionsForm.Show();
				Assert("Should not be visible", !runtimeOptionsForm.EdwDataSourceCheckBox.Visible);
			}

			using (var report = new Report(new DocumentPack(stmMenuItem), EmptyAndValidTemplate))
			{
				var collection = new SupportEdwDataSourceReportCollection();
				var supportEdwDataSourceReport = collection.AddNew();
				supportEdwDataSourceReport.ReportName = stmMenuItem.SU_MenuName;
				supportEdwDataSourceReport.BusinessContext = stmMenuItem.SU_BusinessContext;
				SystemDataRegistry.Instance.ListOfSupportedReportsUsingEdwAsDataSource.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

				BiServers.ClearBiServersCache();
				var requirementMessage = BiServiceTaskHelpers.IsEdwEnabled();
				AssertEquals("IsEdwEnabled() should return empty string.", string.Empty, requirementMessage);

				using (var runtimeOptionsForm = new RuntimeOptionsForm(report))
				{
					runtimeOptionsForm.Show();
					Assert("Should be visible when the report StmMenuItem is in the ListOfSupportedReportsUsingEdwAsDataSource registry", runtimeOptionsForm.EdwDataSourceCheckBox.Visible);
				}

				using (var mainDbConnection = Db.NewAdminConnection())
				using (SnapshotCreator.CreateSnapshot(mainDbConnection, Db.Connection.CloseConnection, Db.EdwDatabaseName, Db.EdwDatabaseName + "-SS"))
				{
					DataUtils.SaveDbExtendedProperty(mainDbConnection, BiConstants.MainDbSchemaVersionExtPtyName, "0.0", Db.EdwDatabaseName);
					requirementMessage = BiServiceTaskHelpers.IsEdwEnabled();
					Assert(!string.IsNullOrEmpty(requirementMessage));

					using (var runtimeOptionsForm = new RuntimeOptionsForm(report))
					{
						runtimeOptionsForm.Show();
						Assert("Should not be visible when the IsEdwEnabled is not empty", !runtimeOptionsForm.EdwDataSourceCheckBox.Visible);
					}
				}
			}
		}

		public void TestEdwDataSourceCheckBoxNotVisibleWhenHasErrors()
		{
			var systemTemplate = ConfigurableTemplateTestHelper.SetupSystemTemplateFromString(Factory,
				@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#ConfigurableSection:GEN, My Section]
{B}-[<Z0_DoesNotExist>]
{A}-[#EndOfReport]");

			var stmMenuItem = Factory.LoadTop1<StmMenuItem>(new ZQuery());
			var dummy = Factory.New<DummyBODocSupportable>();
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;
			documentCommand.SU_MenuName = stmMenuItem.SU_MenuName;
			documentCommand.SU_BusinessContext = stmMenuItem.SU_BusinessContext;

			var document = documentCommand.Documents.AddNew();
			document.SI_SU = documentCommand.PK;
			document.SI_SO = systemTemplate.PK;

			var config = document.DocConfigs.AddNew();
			config.S3_SI = document.PK;

			var configItem1 = config.ConfigItems.AddNew();
			configItem1.S4_S3 = document.PK;
			configItem1.S4_SectionItemName = "My Section";
			configItem1.S4_SectionType = "BDY";

			using (var documentPack = new DocumentPack(documentCommand, dummy, null, documentCommand))
			{
				using (var report = documentPack.GetFirstReport())
				{
					var collection = new SupportEdwDataSourceReportCollection();
					var supportEdwDataSourceReport = collection.AddNew();
					supportEdwDataSourceReport.ReportName = documentCommand.SU_MenuName;
					supportEdwDataSourceReport.BusinessContext = documentCommand.SU_BusinessContext;
					SystemDataRegistry.Instance.ListOfSupportedReportsUsingEdwAsDataSource.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

					using (var stream = new MemoryStream())
					{
						AssertExceptionThrown("Exception should be thrown.", typeof(DocumentEngineException), () => report.Save(stream));
					}

					using (var form = new RuntimeOptionsForm(report, AllowedDeliveryOptions.All, null, Env.Security.None))
					{
						form.Show();
						Assert("form.ErrorLabel", form.ErrorLabel.StartsWith("ERRORS"));
						Assert("Should not be visible when the report ", !form.EdwDataSourceCheckBox.Visible);
					}
				}
			}
		}

		public void TestNoExceptionWhenReportDoesntHaveMenuItem()
		{
			var stmMenuItem = Factory.LoadTop1<StmMenuItem>(new ZQuery());

			var collection = new SupportEdwDataSourceReportCollection();
			var supportEdwDataSourceReport = collection.AddNew();
			supportEdwDataSourceReport.ReportName = stmMenuItem.SU_MenuName;
			supportEdwDataSourceReport.BusinessContext = stmMenuItem.SU_BusinessContext;
			SystemDataRegistry.Instance.ListOfSupportedReportsUsingEdwAsDataSource.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			using (var report = new Report(new DocumentPack(), EmptyAndValidTemplate))
			using (var form = new RuntimeOptionsForm(report))
			{
				AssertNoExceptionThrown(form.Show);
				Assert("Should not be visible", !form.EdwDataSourceCheckBox.Visible);
			}
		}

		[TestedType(typeof(RuntimeOptionsForm.ReportProcessingError))]
		public class ReportProcessingErrorTest : NonPersistentBusinessObjectTestCase
		{
			protected override BusinessObject GetNewBusinessObject()
			{
				return new RuntimeOptionsForm.ReportProcessingError();
			}
		}

		[TestedType(typeof(RuntimeOptionsForm.ReportProcessingErrorToBind))]
		public class ReportProcessingErrorToBindTest : NonPersistentBusinessObjectTestCase
		{
			protected override BusinessObject GetNewBusinessObject()
			{
				var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.EmptyAndValidTemplate.xls", "EmptyAndValidTemplate.xls");
				var template = new ExcelTemplateForUnitTesting("EmptyAndValidTemplate.xls", Path.GetFullPath(tempFileName));
				var report = new Report(new DocumentPack(Factory.LoadTop1<StmMenuItem>(new ZQuery())), template);
				return new RuntimeOptionsForm.ReportProcessingErrorToBind(new RuntimeOptionsForm.ReportProcessingErrorList(report.ErrorManager));
			}

			readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever(typeof(DocumentEngine.Testing.PrintTaskTest).Assembly));
			protected override void TearDown()
			{
				if (resourceRetriever.IsValueCreated)
				{
					resourceRetriever.Value.Dispose();
				}
				base.TearDown();
			}
		}

		[TestedType(typeof(RuntimeOptionsForm.ReportProcessingErrorList))]
		public class ReportProcessingErrorListTest : NonPersistentBusinessObjectCollectionTestCase<RuntimeOptionsForm.ReportProcessingErrorList>
		{
			protected override RuntimeOptionsForm.ReportProcessingErrorList GetCollectionToTest()
			{
				var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.EmptyAndValidTemplate.xls", "EmptyAndValidTemplate.xls");
				var template = new ExcelTemplateForUnitTesting("EmptyAndValidTemplate.xls", Path.GetFullPath(tempFileName));
				var report = new Report(new DocumentPack(Factory.LoadTop1<StmMenuItem>(new ZQuery())), template);
				return new RuntimeOptionsForm.ReportProcessingErrorList(report.ErrorManager);
			}

			protected override BusinessObject GetNewElementToAddToTheCollection()
			{
				return new RuntimeOptionsForm.ReportProcessingError();
			}

			readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever(typeof(DocumentEngine.Testing.PrintTaskTest).Assembly));

			protected override void TearDown()
			{
				if (resourceRetriever.IsValueCreated)
				{
					resourceRetriever.Value.Dispose();
				}
				base.TearDown();
			}
		}

		public void TestMaxDop_DefaultValueShouldShow_And_Operability()
		{
			using (var report = new Report(new DocumentPack(), EmptyAndValidTemplate))
			using (var form = new RuntimeOptionsForm(new PrintTask(), report, AllowedDeliveryOptions.All, new DeliveryInstructions(), Env.Security.None))
			{
				form.Show();
				AssertEquals(true, form.MaxDopEdit.Visible);
				AssertEquals(false, form.MaxDopEdit.ReadOnly);
				AssertEquals(true, form.MaxDopEdit.Enabled);
				AssertEquals(form.MaxDopEdit.Text, "0");
			}
		}

		#region Implementation

		protected override void TearDown()
		{
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
			base.TearDown();
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever(typeof(PrintTaskTest).Assembly));

		ExcelTemplate emptyAndValidTemplate;
		ExcelTemplate EmptyAndValidTemplate
		{
			get
			{
				if (emptyAndValidTemplate == null)
				{
					var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.EmptyAndValidTemplate.xls", "EmptyAndValidTemplate.xls");
					emptyAndValidTemplate = new ExcelTemplateForUnitTesting("EmptyAndValidTemplate.xls", Path.GetFullPath(tempFileName));
				}
				return emptyAndValidTemplate;
			}
		}

		ExcelTemplate badExcelFormatTest;
		ExcelTemplate BadExcelFormatTest
		{
			get
			{
				if (badExcelFormatTest == null)
				{
					var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.BadExcelFormatTest.xls", "BadExcelFormatTest.xls");
					badExcelFormatTest = new ExcelTemplateForUnitTesting("BadExcelFormatTest.xls", Path.GetFullPath(tempFileName));
				}
				return badExcelFormatTest;
			}
		}

		ExcelTemplate filterTestTemplate;
		ExcelTemplate FilterTestTemplate
		{
			get
			{
				if (filterTestTemplate == null)
				{
					var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.FilterTestTemplate.xls", "FilterTestTemplate.xls");
					filterTestTemplate = new ExcelTemplateForUnitTesting("FilterTestTemplate.xls", Path.GetFullPath(tempFileName));
				}
				return filterTestTemplate;
			}
		}

		ExcelTemplate newLineXls;
		ExcelTemplate NewLineXls
		{
			get
			{
				if (newLineXls == null)
				{
					var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.DocumentTestFiles.NewLine.xls", "NewLine.xls");
					newLineXls = new ExcelTemplateForUnitTesting("NewLine.xls", Path.GetFullPath(tempFileName));
				}
				return newLineXls;
			}
		}

		ReportCommand GetReportCommandWithTemplate(string menuName, ExcelTemplate excelTemplate)
		{
			var menuItem = Factory.New<ReportCommand>();
			menuItem.SU_MenuName = menuName;
			menuItem.SU_IsSystemDefined = false;
			var template = Factory.New<StmTemplate>();
			template.SO_Template = excelTemplate.GetAsByteArray();
			var menuTemplateLink = Factory.New<StmMenuTemplatePivot>();
			menuTemplateLink.SI_SU = menuItem.PK;
			menuTemplateLink.SI_SO = template.PK;
			return menuItem;
		}

		DeliveryInstructions GetDeliveryInstructions(DocumentPack documentPack)
		{
			var deliveryInstructions = new DeliveryInstructions(documentPack);
			var recipient = deliveryInstructions.Recipients.AddNew();
			recipient.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			recipient.EmailToRecipients.Value = "unit.test@cargowise.com";

			return deliveryInstructions;
		}

		void SimulateRequestDelivery(PrintTask printTask, AllowedDeliveryOptions deliveryOptions, DeliveryInstructions deliveryInstructions, ISecurityCheckpoint modifyDocumentCheckPoint)
		{
			printTask.Form_DeliveryRequested(null, deliveryOptions, deliveryInstructions, modifyDocumentCheckPoint);
		}

		#endregion
	}
}

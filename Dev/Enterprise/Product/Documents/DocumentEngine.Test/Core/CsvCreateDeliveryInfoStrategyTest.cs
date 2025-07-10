using System;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.ExcelTemplates;
using Enterprise.MasterFiles.Business;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ContactNotifyModes = Enterprise.Core.Constants.ContactNotifyModes;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class CsvCreateDeliveryInfoStrategyTest : TestCaseWithFactory
	{
		public void TestRunScheduledReportAsCsvFileWithSheetNameOverride()
		{
			RunScheduledReportAsCsvFile("Client - Order Status Summary Report");
		}

		public void TestRunScheduledReportAsCsvFileWithSheetNameOverrideAndLanguage()
		{
			RunScheduledReportAsCsvFile("Client - Order Status Summary Report", "ZH-CN");
		}

		public void TestRunScheduledReportAsCsvFileWithSheetNameOverrideAndLanguageAndReportTitle()
		{
			RunScheduledReportAsCsvFile("Client - Order Status Summary Report", "ZH-CN", "Order Status Summary Report");
		}

		public void TestRunScheduledReportAsCsvFile()
		{
			RunScheduledReportAsCsvFile();
		}

		void RunScheduledReportAsCsvFile(string sheetNameOverride = "", string language = "", string reportTitle = "")
		{
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_VarCharMax = "Hello World";

			var reportCommand = Factory.New<ReportCommand>();
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test Report",
$@"{{A}}-[#Config]
{{A}}-[Name=Dummy Report]
{(string.IsNullOrEmpty(sheetNameOverride) ? "" : $"{{A}}-[SheetNameOverride={sheetNameOverride}]")}
{(string.IsNullOrEmpty(reportTitle) ? "" : $"{{A}}-[ReportTitle={reportTitle}]")}
{{A}}-[Data:ReportData=SELECT * FROM dbo.DummyBizo]
{{A}}-[ColumnHeadings:]    {{B}}-[DisplayLabel=""VarCharMax"", HeadingText=""VarCharMax""]
{{A}}-[#SectionBody:Data=ReportData]
{{B}}-[<ReportData.Z0_VarCharMax>]
{{A}}-[#EndOfReport]");

			var pivot = reportCommand.Documents.AddNew();
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;

			using (Report.TemporarilyUseMainConnection())
			using (string.IsNullOrEmpty(language) ? null : Res.TemporarilySwitchLanguage(language))
			using (var documentPack = new DocumentPack(reportCommand))
			{
				var deliveryInstructions = new DeliveryInstructions(documentPack);
				deliveryInstructions.Recipients.RemoveAndDeleteAll();

				var recipient = deliveryInstructions.Recipients.AddNew();
				recipient.DeliveryMethod = ContactNotifyModes.Email;
				recipient.AttachmentType = AttachmentTypeList.Codes.CsvWithHeadings;
				recipient.Email = @"unit.test@cargowise.com";

				var scheduledReport = Factory.NewWithValidTestData<ReportScheduleTask>();
				scheduledReport.PopulateDefaultsFromDeliveryInstructions(deliveryInstructions);

				AssertEquals("Pre-condition: There should be one recipient.", 1, scheduledReport.Recipients.Count);

				var scheduledReportRecipient = scheduledReport.Recipients[0];
				scheduledReport.Run();

				var query = new ZDBOnlyQuery(typeof(StmPrintJob));
				query.AddSubQuery(GetEmailToSubQuery("unit.test@cargowise.com"), JoinCondition.And);

				var printJobs = Factory.Load<StmPrintJob>(query);

				AssertEquals("There should be one print job created.", 1, printJobs.Length);
				AssertMultilineASCIIEquals("CSV should be outputted to the print job.",
@"""VarCharMax""
""Hello World""",
					printJobs[0].SP_CustomProperties.ToUTF8());
			}
		}

		public void TestCreateDeliveryInfoWithColumnHeadingsWithOneHidden()
		{
			CreateDummyBusinessObject(1, "One");
			CreateDummyBusinessObject(2, "Two");
			CreateDummyBusinessObject(3, "Three");

			Factory.Save();

			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty,
@"{A}-[#Config]
{A}-[Name=Dummy Report]
{A}-[Data:ReportData=SELECT * FROM dbo.DummyBizo]
{A}-[ColumnHeadings:]    {B}-[DisplayLabel=""Number"", HeadingText=""Number"", Hidden]    {C}-[DisplayLabel=""VarCharMax"", HeadingText=""VarCharMax""]
{A}-[#DocumentHeader]
{B}-[Page <CurrentPage> of <TotalPages>]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.Z0_Number>]    {C}-[<ReportData.Z0_VarCharMax>]
{A}-[#EndOfReport]");

			using (var report = new Report(new DocumentPack(), excelTemplate))
			{
				var strategy = new CsvCreateDeliveryInfoStrategy();
				strategy.IncludeColumnHeadings = true;

				var result = strategy.CreateDeliveryInfo(report, null, new DeliveryInstructions());

				AssertEquals("FileFormat should be CSV.", "CSV", result.FileFormat);

				using (var stream = result.FileContents)
				{
					AssertMultilineASCIIEquals("CSV should be outputted to the file contents.",
@"""VarCharMax""
""One""
""Two""
""Three""",
						Encoding.UTF8.GetString(stream.CopyToByteArray()));
				}
			}
		}

		public void TestCreateDeliveryInfoWithColumnHeadings()
		{
			CreateDummyBusinessObject(1, "One");
			CreateDummyBusinessObject(2, "Two");
			CreateDummyBusinessObject(3, "Three");

			Factory.Save();

			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty,
@"{A}-[#Config]
{A}-[Name=Dummy Report]
{A}-[Data:ReportData=SELECT * FROM dbo.DummyBizo]
{A}-[ColumnHeadings:]    {B}-[DisplayLabel=""Number"", HeadingText=""Number""]    {C}-[DisplayLabel=""VarCharMax"", HeadingText=""VarCharMax""]
{A}-[#DocumentHeader]
{B}-[Page <CurrentPage> of <TotalPages>]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.Z0_Number>]    {C}-[<ReportData.Z0_VarCharMax>]
{A}-[#EndOfReport]");

			using (var report = new Report(new DocumentPack(), excelTemplate))
			{
				var strategy = new CsvCreateDeliveryInfoStrategy();
				strategy.IncludeColumnHeadings = true;

				var result = strategy.CreateDeliveryInfo(report, null, new DeliveryInstructions());

				AssertEquals("FileFormat should be CSV.", "CSV", result.FileFormat);

				using (var stream = result.FileContents)
				{
					AssertMultilineASCIIEquals("CSV should be outputted to the file contents.",
@"""Number"",""VarCharMax""
""1"",""One""
""2"",""Two""
""3"",""Three""",
						Encoding.UTF8.GetString(stream.CopyToByteArray()));
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateDeliveryInfoWithConditionalHideColumnHeadings()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = organisation.SalesOpportunities.AddNew();
			opportunity.P8_EstimatedValue = 12;
			opportunity.P8_OpportunityType = "DUD";
			Factory.Save();

			var excelTemplate = new ExcelTemplateForUnitTesting("HideColumnIfWithNonExistantOptionalColumns2.xls", TestFilesSubFolder.ReportTestFiles);
			excelTemplate.ContainsCustomisedSections = true;
			var pack = new DocumentPack();
			using (var report = new Report(pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				var strategy = new CsvCreateDeliveryInfoStrategy();
				strategy.IncludeColumnHeadings = true;

				var result = strategy.CreateDeliveryInfo(report, null, new DeliveryInstructions());
				using (var stream = result.FileContents)
				{
					AssertMultilineASCIIEquals("CSV should be outputted to the file contents without columns should be hidden.",
						@"""Stage Desc."",""Last Activity Date"",""Estimated Close Date"",""Objective"",""Objective Desc."",""Estimated Value""
"""","""","""",""DUD"","""",""12.00""",
						Encoding.UTF8.GetString(stream.CopyToByteArray()));
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReadColumnHeadingsWithHideIfDescriptionEmptyOption()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("HideIfDescriptionEmptyColumnShouldNotBeExportedIfEmpty.xls", TestFilesSubFolder.ReportTestFiles);
			excelTemplate.ContainsCustomisedSections = true;
			var pack = new DocumentPack();
			using (var report = new Report(pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("EmptyColumnHeading", ""));
				report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("NonEmptyColumnHeading", "Objective Desc"));

				report.PrepareForRender();

				int startingColumn;
				var columns = ReportDataExportStrategy.ReadColumnHeadings(report, out startingColumn);
				AssertEquals("Stage Desc.,Last Activity Date,Estimated Close Date,Objective,Estimated Value 1,Estimated Value 2", string.Join(",", columns));
			}
		}

		public void TestAddingColumnHeadingToComplexReportProduceRightCSV()
		{
			CreateDummyBusinessObject(1, "Sango");
			Factory.Save();

			var columnHeadingsTemplate = new StringBuilder();

			for (var i = 1; i <= 3; i++)
			{
				columnHeadingsTemplate.Append($@"{{{ExcelWorkSheet.GetColumnTitle(i)}}}-[DisplayLabel=""Test {i}"", HeadingText=""Test {i}"", Hidden]    ");
			}

			for (var i = 4; i <= 10; i++)
			{
				columnHeadingsTemplate.Append($@"{{{ExcelWorkSheet.GetColumnTitle(i)}}}-[DisplayLabel=""Test {i}"", HeadingText=""Test {i}""]    ");
			}

			for (var i = 11; i <= 14; i++)
			{
				columnHeadingsTemplate.Append($@"{{{ExcelWorkSheet.GetColumnTitle(i)}}}-[DisplayLabel=""{i}"", Description="""", HeadingText="" Test {i}"", HideIfDescriptionEmpty]    ");
			}

			for (var i = 15; i <= 23; i++)
			{
				columnHeadingsTemplate.Append($@"{{{ExcelWorkSheet.GetColumnTitle(i)}}}-[DisplayLabel=""{i}"", HeadingText=""Test {i}""]    ");
			}

			var template = $@"{{A}}-[#Config]
{{A}}-[Name=Dummy Report]
{{A}}-[Data:ReportData=SELECT * FROM dbo.DummyBizo]
{{A}}-[ColumnHeadings:]    {columnHeadingsTemplate.ToString()}
{{A}}-[#SectionBody:Data=ReportData]
{{B}}-[<ReportData.Z0_Code>]   {{C}}-[<ReportData.Z0_VarCharMax>]  {{E}}-[<ReportData.Z0_Number>] 
{{A}}-[#EndOfReport]";

			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty, template);

			using (var report = new Report(new DocumentPack(), excelTemplate))
			{
				var strategy = new CsvCreateDeliveryInfoStrategy();
				strategy.IncludeColumnHeadings = true;

				report.PrepareForRender();
				var worksheet = report.ColumnHeadingManager.CurrentConfiguration.Worksheets[0];

				worksheet.ColumnHeadings[1].Hidden = false;
				worksheet.ColumnHeadings[1].CurrentPosition = ReportDataExportStrategy.ReadColumnHeadings(report, out var menballec).ToArray().Length;

				var result = strategy.CreateDeliveryInfo(report, null, new DeliveryInstructions());

				AssertEquals("FileFormat should be CSV.", "CSV", result.FileFormat);

				using (var stream = result.FileContents)
				{
					AssertMultilineASCIIEquals("CSV should be outputted to the file contents.",
@"""Test 4"",""Test 5"",""Test 6"",""Test 7"",""Test 8"",""Test 9"",""Test 10"",""Test 15"",""Test 16"",""Test 17"",""Test 18"",""Test 19"",""Test 20"",""Test 2"",""Test 21"",""Test 22"",""Test 23""
""1"","""","""","""","""","""","""","""","""","""","""","""","""","""","""","""",""Sango""",
						Encoding.UTF8.GetString(stream.CopyToByteArray()));
				}
			}
		}

		public void TestCreateDeliveryInfo()
		{
			CreateDummyBusinessObject(1, "One");
			CreateDummyBusinessObject(2, "Two");
			CreateDummyBusinessObject(3, "Three");
			CreateDummyBusinessObject(4, "F,o,u,r");
			CreateDummyBusinessObject(5, "F\"i\"v\"e");

			Factory.Save();

			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty,
@"{A}-[#Config]
{A}-[Name=Dummy Report]
{A}-[Data:ReportData=SELECT * FROM dbo.DummyBizo]
{A}-[#DocumentHeader]
{B}-[Page <CurrentPage> of <TotalPages>]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.Z0_Number>]    {C}-[<ReportData.Z0_VarCharMax>]
{A}-[#EndOfReport]");

			using (var report = new Report(new DocumentPack(), excelTemplate))
			{
				var strategy = new CsvCreateDeliveryInfoStrategy();
				strategy.IncludeColumnHeadings = false;

				var result = strategy.CreateDeliveryInfo(report, null, new DeliveryInstructions());

				AssertEquals("FileFormat should be CSV.", "CSV", result.FileFormat);

				using (var stream = result.FileContents)
				{
					AssertMultilineASCIIEquals("CSV should be outputted to the file contents.",
@"""1"",""One""
""2"",""Two""
""3"",""Three""
""4"",""F,o,u,r""
""5"",""F""""i""""v""""e""",
						Encoding.UTF8.GetString(stream.CopyToByteArray()));
				}
			}
		}

		public void TestCreateDeliveryInfoIncludeColumnHeadingsButDoNotHaveAnyColumnInformation()
		{
			CreateDummyBusinessObject(1, "One");
			CreateDummyBusinessObject(2, "Two");
			CreateDummyBusinessObject(3, "Three");

			Factory.Save();

			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty,
@"{A}-[#Config]
{A}-[Name=Dummy Report]
{A}-[Data:ReportData=SELECT * FROM dbo.DummyBizo]
{A}-[#DocumentHeader]
{B}-[Page <CurrentPage> of <TotalPages>]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.Z0_Number>]    {C}-[<ReportData.Z0_VarCharMax>]
{A}-[#EndOfReport]");

			using (Report.TemporarilyUseMainConnection())
			using (var report = new Report(new DocumentPack(), excelTemplate))
			{
				var strategy = new CsvCreateDeliveryInfoStrategy();
				strategy.IncludeColumnHeadings = true;

				var result = strategy.CreateDeliveryInfo(report, null, new DeliveryInstructions());

				AssertEquals("FileFormat should be CSV.", "CSV", result.FileFormat);

				using (var stream = result.FileContents)
				{
					AssertMultilineASCIIEquals("CSV should be outputted to the file contents.",
@"""1"",""One""
""2"",""Two""
""3"",""Three""",
						Encoding.UTF8.GetString(stream.CopyToByteArray()));
				}
			}
		}

		public void TestEmailReportUsingAttachmentTypeCsv()
		{
			using (Report.TemporarilyUseMainConnection())
			{
				CreateDummyBusinessObject(1, "One");
				CreateDummyBusinessObject(2, "Two");
				CreateDummyBusinessObject(3, "Three");
				Factory.Save();

				var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("Dummy Report", string.Empty,
	@"{A}-[#Config]
{A}-[Name=Dummy Report]
{A}-[Data:ReportData=SELECT Z0_Number, Z0_VarCharMax FROM dbo.DummyBizo]
{A}-[#DocumentHeader]
{B}-[Page <CurrentPage> of <TotalPages>]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.Z0_Number>]    {C}-[<ReportData.Z0_VarCharMax>]
{A}-[#EndOfReport]");
				var template = Factory.New<StmTemplateBase>();
				template.SO_Template = excelTemplate.GetAsByteArray();

				var reportCommand = Factory.New<ReportCommand>();
				var pivot = reportCommand.Documents.AddNew();
				pivot.SI_SU = reportCommand.PK;
				pivot.SI_SO = template.PK;

				var deliveryInstructions = new DeliveryInstructions
				{
					Destination = DeliveryInstructionDestination.TakenFromContact
				};

				deliveryInstructions.Recipients.RemoveAndDeleteAll();

				var deliveryContact = deliveryInstructions.Recipients.AddNew();
				deliveryContact.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Email;
				deliveryContact.Email = "unit.test@cargowise.com";
				deliveryContact.AttachmentType = AttachmentTypeList.Codes.Csv;

				var printJobs = DeliveryTestHelper.DeliverReport(reportCommand, deliveryInstructions);
				var printJob = printJobs.First();

				AssertEquals(AttachmentTypeList.Codes.Csv, printJob.SP_EmailAttachmentFormat);
				using (var stream = new MemoryStream(printJob.SP_CustomProperties))
				{
					AssertEquals(
	@"""1"",""One""
""2"",""Two""
""3"",""Three""
",
						Encoding.UTF8.GetString(stream.ToArray()));
				}

				var printJobManager = ObjectFactory.Get<IPrintJobManager>();
				printJobManager.ProcessPrintJobs(printJobs);

				AssertEquals("One email should have been created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				var email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertContains("Dummy Report", email.Subject);
			}
		}

		#region GenerateCSVFileWhenNoDataCanBeExported

		public void TestGenerateCSVFileWhenNoDataCanBeExported_ReportHasData_ShouldReportWarning()
		{
			var expectedMessage = Globals.CanShowDialogs ? @"Report 'Dummy Report' does not contain any data that can be exported to 'CSV' format. This report contains data that would be suitable for exporting to XLS, PDF or TIFF.
This is because 'CSV' requires a consistent format throughout and not all report templates have a suitable structure." : null;

			AssertGenerateCSVFileWhenNoDataCanBeExported(templateStringWithData, expectedMessage);
		}

		public void TestGenerateCSVFileWhenNoDataCanBeExported_ReportHasNoData_ShouldNotReportWarning()
		{
			AssertGenerateCSVFileWhenNoDataCanBeExported(templateStringWithNoData, null);
		}

		void AssertGenerateCSVFileWhenNoDataCanBeExported(string templateString, string expectedMessage)
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("Dummy Report", string.Empty, templateString);
			var template = Factory.New<StmTemplateBase>();
			template.SO_Template = excelTemplate.GetAsByteArray();

			var reportCommand = Factory.New<ReportCommand>();
			reportCommand.SU_MenuName = "Dummy Report";
			var pivot = reportCommand.Documents.AddNew();
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;

			var deliveryInstructions = new DeliveryInstructions
			{
				Destination = DeliveryInstructionDestination.TakenFromContact
			};

			deliveryInstructions.Recipients.RemoveAndDeleteAll();

			var deliveryContactPDF = deliveryInstructions.Recipients.AddNew();
			deliveryContactPDF.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Email;
			deliveryContactPDF.Email = "unit.test@cargowise.com";
			deliveryContactPDF.AttachmentType = AttachmentTypeList.Codes.Pdf;

			var deliveryContactCSV = deliveryInstructions.Recipients.AddNew();
			deliveryContactCSV.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Email;
			deliveryContactCSV.Email = "unit.test@cargowise.com";
			deliveryContactCSV.AttachmentType = AttachmentTypeList.Codes.Csv;

			var printJobs = DeliveryTestHelper.DeliverReport(reportCommand, deliveryInstructions).OrderBy(job => job.SP_EmailAttachmentFormat).ToArray();
			AssertEquals(2, printJobs.Length);
			AssertEquals(AttachmentTypeList.Codes.Csv, printJobs[0].SP_EmailAttachmentFormat);
			AssertEquals(AttachmentTypeList.Codes.Pdf, printJobs[1].SP_EmailAttachmentFormat);

			var printJobManager = ObjectFactory.Get<IPrintJobManager>();
			printJobManager.ProcessPrintJobs(printJobs);

			AssertEquals("One email should have been created", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertContains("Dummy Report", email.Subject);

			var attachements = email.Attachments.Cast<AttachmentDef>().OrderBy(attachement => attachement.DisplayName);
			AssertEquals(2, attachements.Count());
			Assert("One CSV file should be attached", attachements.ElementAt(0).DisplayName.EndsWith(AttachmentTypeList.Codes.Csv, StringComparison.InvariantCultureIgnoreCase));
			Assert("One PDF file should be attached", attachements.ElementAt(1).DisplayName.EndsWith(AttachmentTypeList.Codes.Pdf, StringComparison.InvariantCultureIgnoreCase));

			AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

			ErrorReporter.Clear();
		}

		#region Scheduled report

		public void TestGenerateCSVFileWhenNoDataCanBeExported_ScheduledReport_ReportHasData_SendReportContingency()
		{
			GenerateScheduledReport(true, EmptyReportContingencyList.Codes.SendReport);

			var query = new ZDBOnlyQuery(typeof(StmPrintJob));
			query.AddSubQuery(GetEmailToSubQuery("unit.test@cargowise.com"), JoinCondition.And);

			var printJobs = Factory.Load<StmPrintJob>(query);
			AssertEquals("There should be one print job created.", 1, printJobs.Length);
			AssertMultilineASCIIEquals("CSV should be outputted to the print job.", string.Empty, printJobs[0].SP_CustomProperties.ToUTF8());

			var emails = Env.OutgoingMailManager.EmailsCreated;
			AssertEquals("No email should have been created", 0, emails.Count);
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

			var expectedMessage = @"Report 'Another Dummy Report' does not contain any data that can be exported to 'CSV' format. This report contains data that would be suitable for exporting to XLS, PDF or TIFF.
This is because 'CSV' requires a consistent format throughout and not all report templates have a suitable structure.";
			AssertEquals("There should be one single notification", 1, scheduledReportNotifications.Events.Length);
			AssertEquals(expectedMessage, scheduledReportNotifications.Events[0].Message);
		}

		public void TestGenerateCSVFileWhenNoDataCanBeExported_ScheduledReport_ReportHasNoData_SendReportContingency()
		{
			GenerateScheduledReport(false, EmptyReportContingencyList.Codes.SendReport);

			var query = new ZDBOnlyQuery(typeof(StmPrintJob));
			query.AddSubQuery(GetEmailToSubQuery("unit.test@cargowise.com"), JoinCondition.And);

			var printJobs = Factory.Load<StmPrintJob>(query);
			AssertEquals("There should be one print job created.", 1, printJobs.Length);
			AssertMultilineASCIIEquals("CSV should be outputted to the print job.", string.Empty, printJobs[0].SP_CustomProperties.ToUTF8());

			var emails = Env.OutgoingMailManager.EmailsCreated;
			AssertEquals("No email should have been created", 0, emails.Count);
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("There should be no notifications", 0, scheduledReportNotifications.Events.Length);
		}

		public void TestGenerateCSVFileWhenNoDataCanBeExported_ScheduledReport_ReportHasData_SendEmailNotifContingency()
		{
			GenerateScheduledReport(true, EmptyReportContingencyList.Codes.SendEmailNotification);

			var query = new ZDBOnlyQuery(typeof(StmPrintJob));
			query.AddSubQuery(GetEmailToSubQuery("unit.test@cargowise.com"), JoinCondition.And);

			var printJobs = Factory.Load<StmPrintJob>(query);
			AssertEquals("There should be one print job created.", 1, printJobs.Length);
			AssertMultilineASCIIEquals("CSV should be outputted to the print job.", string.Empty, printJobs[0].SP_CustomProperties.ToUTF8());

			var emails = Env.OutgoingMailManager.EmailsCreated;
			AssertEquals("No email should have been created", 0, emails.Count);
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

			var expectedMessage = @"Report 'Another Dummy Report' does not contain any data that can be exported to 'CSV' format. This report contains data that would be suitable for exporting to XLS, PDF or TIFF.
This is because 'CSV' requires a consistent format throughout and not all report templates have a suitable structure.";
			AssertEquals("There should be one single notification", 1, scheduledReportNotifications.Events.Length);
			AssertEquals(expectedMessage, scheduledReportNotifications.Events[0].Message);
		}

		public void TestGenerateCSVFileWhenNoDataCanBeExported_ScheduledReport_ReportHasNoData_SendEmailNotifContingency()
		{
			GenerateScheduledReport(false, EmptyReportContingencyList.Codes.SendEmailNotification);

			var query = new ZDBOnlyQuery(typeof(StmPrintJob));
			query.AddSubQuery(GetEmailToSubQuery("unit.test@cargowise.com"), JoinCondition.And);

			var printJobs = Factory.Load<StmPrintJob>(query);
			AssertEquals("No print job should be created.", 0, printJobs.Length);

			var emails = Env.OutgoingMailManager.EmailsCreated;
			AssertEquals("One email should have been created", 1, emails.Count);
			AssertContains("The resulting document was empty and therefore has not been delivered.", emails[0].Body);
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("There should be no notifications", 0, scheduledReportNotifications.Events.Length);
		}

		void GenerateScheduledReport(bool runOnReportWithData, string emptyReportContingency)
		{
			Globals.IsUserInteractive = false;

			var reportCommand = Factory.New<ReportCommand>();
			reportCommand.SU_MenuName = "Another Dummy Report";
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test Report", runOnReportWithData ? templateStringWithData : templateStringWithNoData);

			var pivot = reportCommand.Documents.AddNew();
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;

			using (var documentPack = new DocumentPack(reportCommand))
			{
				var deliveryInstructions = new DeliveryInstructions(documentPack);
				deliveryInstructions.Recipients.RemoveAndDeleteAll();

				var recipient = deliveryInstructions.Recipients.AddNew();
				recipient.DeliveryMethod = ContactNotifyModes.Email;
				recipient.AttachmentType = AttachmentTypeList.Codes.CsvWithHeadings;
				recipient.Email = @"unit.test@cargowise.com";

				var scheduledReport = Factory.NewWithValidTestData<ReportScheduleTask>();
				scheduledReport.PopulateDefaultsFromDeliveryInstructions(deliveryInstructions);

				AssertEquals("Pre-condition: There should be one recipient.", 1, scheduledReport.Recipients.Count);

				var scheduledReportRecipient = scheduledReport.Recipients[0];
				scheduledReportRecipient.S6_EmptyReportDeliveryOptions = emptyReportContingency;

				scheduledReportNotifications.Clear();
				scheduledReport.Run(scheduledReportNotifications);
			}
			ErrorReporter.Clear();
		}
		readonly NotificationBuffer scheduledReportNotifications = new NotificationBuffer();

		const string templateStringWithData = @"{A}-[#Config]
{A}-[Name=Dummy Report]
{A}-[Data:ReportData=Select KLL = 1 Union All Select KLL = 9]
{A}-[#SectionBody:Data=ReportData]
{A}-[#GroupBy:ReportData.KLL]
{B}-[<ReportData.KLL>]
{A}-[#EndOfReport]";

		const string templateStringWithNoData = @"{A}-[#Config]
{A}-[Name=Dummy Report]
{A}-[Data:ReportData=SELECT Z0_Number, Z0_VarCharMax FROM dbo.DummyBizo]
{A}-[#DocumentHeader]
{B}-[Page <CurrentPage> of <TotalPages>]
{A}-[#SectionBody:Data=ReportData]
{A}-[#EndOfReport]";

		#endregion

		#endregion

		public void TestDateFormats()
		{
			using (MemoryStream templateStream = new MemoryStream())
			{
				using (ExcelInterface creationExcelInterface = new ExcelInterface())
				{
					creationExcelInterface.NewExcelFile(1);
					ExcelWorkSheet workSheet = creationExcelInterface.WorkSheets[0];

					workSheet[0, 0] = "#config";
					workSheet[1, 0] = "Name=TemplateFromStream";
					workSheet[2, 0] = "PageStyle=Portrait";
					workSheet[3, 0] = "DataContext=None";
					workSheet[4, 0] = "Data:Collection=select 40437.67877 as Z0_DateTime";
					workSheet[5, 0] = "#SectionBody:Data=Collection";
					workSheet[6, 1] = "<collection.Z0_DateTime>";
					workSheet.SetCellFormat(6, 1, new DocumentEngineIntegration.CellFormat() { FormatPattern = "d/mm/yyyy h:mm AM/PM" });
					workSheet[7, 0] = "#EndOfReport";
					creationExcelInterface.SaveToStream(templateStream);
				}

				ExcelTemplateWrappingStream excelTemplate = new ExcelTemplateWrappingStream("TemplateFromStream", templateStream);
				using (var report = new Report(new DocumentPack(), excelTemplate))
				{
					var strategy = new CsvCreateDeliveryInfoStrategy();
					strategy.IncludeColumnHeadings = true;

					var result = strategy.CreateDeliveryInfo(report, null, new DeliveryInstructions());

					AssertEquals("FileFormat should be CSV.", "CSV", result.FileFormat);

					using (var stream = result.FileContents)
					{
						AssertMultilineASCIIEquals("CSV should be outputted to the file contents.",
	@"""16/09/2010 4:17 PM""",
							Encoding.UTF8.GetString(stream.CopyToByteArray()));
					}
				}
			}
		}

		ZDBOnlySubQuery GetEmailToSubQuery(string emailAddress)
		{
			return (ZDBOnlySubQuery)new ZDBOnlySubQuery(typeof(StmPrintJobCopyRecipient), StmPrintJobCopyRecipientSchema.SPR_SP)
				.AddToFilter(StmPrintJobCopyRecipientSchema.SPR_RecipientType, SQLComparisonOperator.Equal, "TO")
				.AddToFilter(StmPrintJobCopyRecipientSchema.SPR_EmailAddress, SQLComparisonOperator.Equal, emailAddress);
		}

		void CreateDummyBusinessObject(int number, string text)
		{
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Number = number;
			dummy.Z0_VarCharMax = text;
			dummy.Z0_Code = "Hey";
		}
	}
}

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Common;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using FlexCel.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Scheduler.Business.Testing
{
	[TestedType(typeof(ReportScheduleTask))]
	public sealed class ReportScheduleTaskTest : EnterpriseBusinessObjectTestCase
	{
		public void TestStmReportRunWithMultipleEDocRecipients()
		{
			TestCaseHelper.ClearTable(StmPrintJob.Schema.TableName);

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "JTOTST1";
			org1.OH_FullName = "Jerry Test Organisation 1";

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "JTOTST2";
			org2.OH_FullName = "Jerry Test Organisation 2";

			Factory.Save();

			var content = new Dictionary<string, string>();
			content.Add("Sheet1",
	@"{A}-[#Config]
{A}-[Name=Jerry Test]
{A}-[EmailSubject=Subject Test <Organisation PK>]
{A}-[Data:ReportData=SELECT OH_FullName FROM dbo.OrgHeader WHERE OH_PK = <Organisation PK>]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.OH_FullName>]
{A}-[#EndOfReport]");

			content.Add("Filter",
	@"{A}-[Organisation PK] {B}-[Type] {C}-[Organisation Lookup]
{B}-[LinkToScheduledReportRecipientForOrganisation]
{A}-[#End]");

			var template = DocumentEngineTestHelper.CreateExcelTemplateFromString("Jerry Test", string.Empty, content);
			var reportCommand = DocumentEngineTestHelper.CreateReportCommandWithExcelTemplate("Jerry Simple Test", template, Factory);

			var reportScheduleTask = Factory.New<ReportScheduleTaskForEmptyReportContingencyTesting>();
			reportScheduleTask.S5_ParentID = reportCommand.PK;
			reportScheduleTask.S5_EndDate = reportScheduleTask.S5_StartDate.AddDays(10);
			reportScheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddMinutes(-5);

			reportScheduleTask.StmReportRun = Factory.New<StmReportRun>();
			reportScheduleTask.StmReportRun.RRI_S5_Schedule = reportScheduleTask.PK;
			reportScheduleTask.StmReportRun.RRI_StartTimeUtc = ZDateTime.UtcNow;

			var recipient1 = reportScheduleTask.Recipients.AddNew();
			recipient1.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.EDoc;
			recipient1.S6_AttachmentType = AttachmentTypeList.Codes.Xls;
			recipient1.S6_OH = org1.PK;

			var recipient2 = reportScheduleTask.Recipients.AddNew();
			recipient2.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.EDoc;
			recipient2.S6_AttachmentType = AttachmentTypeList.Codes.Xls;
			recipient2.S6_OH = org2.PK;

			var recipient3 = reportScheduleTask.Recipients.AddNew();
			recipient3.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.EDoc;
			recipient3.S6_AttachmentType = AttachmentTypeList.Codes.Xls;

			var docPack = new DocumentPack(reportCommand);
			using (var report = (Report)docPack[0])
			{
				report.SetScheduleTask(reportScheduleTask);
				report.PrepareForRender();
				reportScheduleTask.S5_ScheduleState = reportScheduleTask.SerializeForTesting(docPack.DeliveryInstructions, report);
			}
			reportScheduleTask.RunForTesting();

			var printJobs = Factory.Load<StmPrintJob>(new ZQuery());
			AssertEquals("Should generate 3 StmPrintJob records", 3, printJobs.Length);

			AssertNotNull("Should generate a StmPrintJob for recipient1 with organisation is org1", printJobs.Where(j => j.SP_EmailSubjectLine.EndsWith($"Subject Test {org1.PK} {org1.OH_Code} (XLS)")).FirstOrDefault());
			AssertNotNull("Should generate a StmPrintJob for recipient2 with organisation is org2", printJobs.Where(j => j.SP_EmailSubjectLine.EndsWith($"Subject Test {org2.PK} {org2.OH_Code} (XLS)")).FirstOrDefault());
			AssertNotNull("Should generate a StmPrintJob for recipient3 without organisation", printJobs.Where(j => j.SP_EmailSubjectLine.EndsWith($"Subject Test (XLS)")).FirstOrDefault());
		}

		[UseSnapshotProtection(true)]
		public void TestRunReportScheduleTaskWithLinkToScheduledReportRecipientForOrganisationLookupField()
		{
			TestCaseHelper.ClearTable(StmPrintJob.Schema.TableName);

			var content = new Dictionary<string, string>();
			content.Add("Sheet1",
	@"{A}-[#Config]
{A}-[Name=Jerry Test]
{A}-[EmailSubject=<Organisation PK>]
{A}-[Data:ReportData=SELECT OH_FullName FROM dbo.OrgHeader WHERE OH_PK = <Organisation PK>]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.OH_FullName>]
{A}-[#EndOfReport]");

			content.Add("Filter",
	@"{A}-[Organisation PK] {B}-[Type] {C}-[Organisation Lookup]
{B}-[LinkToScheduledReportRecipientForOrganisation]
{A}-[#End]");

			var template = DocumentEngineTestHelper.CreateExcelTemplateFromString("Jerry Test", string.Empty, content);

			var reportCommand = DocumentEngineTestHelper.CreateReportCommandWithExcelTemplate("Jerry Simple Test", template, Factory);
			using (RawDataRegistry.Instance.ReportStatisticsLogExecutionPlan.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var documentPack = new DocumentPack(reportCommand))
			{
				var report = documentPack[0] as Report;
				report.PrepareForRender();

				var deliveryInstructions = new DeliveryInstructions(documentPack);
				deliveryInstructions.Recipients.RemoveAndDeleteAll();

				var org1 = Factory.NewWithValidTestData<OrgHeader>();
				org1.OH_Code = "JTOTST1";
				org1.OH_FullName = "Jerry Test Organisation 1";
				var recipient1 = deliveryInstructions.Recipients.AddNew();
				recipient1.OrgHeaderPK = org1.PK;
				recipient1.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
				recipient1.AttachmentType = AttachmentTypeList.Codes.Xls;
				recipient1.Name = "Test1";
				recipient1.Email = "test1@test.com";

				var org2 = Factory.NewWithValidTestData<OrgHeader>();
				org2.OH_Code = "JTOTST2";
				org2.OH_FullName = "Jerry Test Organisation 2";
				var recipient2 = deliveryInstructions.Recipients.AddNew();
				recipient2.OrgHeaderPK = org2.PK;
				recipient2.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
				recipient2.AttachmentType = AttachmentTypeList.Codes.Xls;
				recipient2.Name = "Test2";
				recipient2.Email = "test2@test.com";

				var org3 = Factory.NewWithValidTestData<OrgHeader>();
				org3.OH_Code = "JTOTST3";
				org3.OH_FullName = "Jerry Test Organisation 3";

				var recipient3 = deliveryInstructions.Recipients.AddNew();
				recipient3.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
				recipient3.AttachmentType = AttachmentTypeList.Codes.Xls;
				recipient3.Name = "Test3";
				recipient3.Email = "test3@test.com";

				var reportScheduleTask = Factory.NewWithValidTestData<ReportScheduleTaskForTest>();
				reportScheduleTask.PopulateDefaultsFromDeliveryInstructions(deliveryInstructions);
				reportScheduleTask.S5_ParentID = reportCommand.PK;
				reportScheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddDays(-5);
				report.FilterCollection.OfType<LookupField>().First().Value = org3.PK.ToGuid();
				reportScheduleTask.S5_ScheduleState = reportScheduleTask.SerializeForTesting(deliveryInstructions, report);
				Factory.Save();

				using (Env.SetTemporaryUserContext(User.ServiceUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
				{
					var notification = new NotificationBuffer();
					new ScheduleTaskRunner().Process(StmMenuItemSchema.Constants.Prefix, true, notification, new CancellationToken());

					var newFactory = new BusinessObjectFactory();
					var jobs = newFactory.Load<StmPrintJob>(new ZQuery());
					AssertEquals("Should generate 3 StmPrintJob records", 3, jobs.Length);
					AssertEquals("Should generate 1 StmReportRun record", 1, reportScheduleTask.StmReportRuns.Count);

					var stmReportRun = reportScheduleTask.StmReportRuns[0];
					AssertContains("Should have the first sql execution plan", org1.PK.ToString(), stmReportRun.RRI_ExecutionPlanText, true);
					AssertContains("Should have the second sql execution plan", org2.PK.ToString(), stmReportRun.RRI_ExecutionPlanText, true);
					AssertContains("Should have the third sql execution plan", org3.PK.ToString(), stmReportRun.RRI_ExecutionPlanText, true);

					var job1 = jobs.Where(j => j.SP_EmailSubjectLine.Contains(org1.PK.ToString())).FirstOrDefault();
					AssertDocumentContent(job1, "Jerry Test Organisation 1");

					var job2 = jobs.Where(j => j.SP_EmailSubjectLine.Contains(org2.PK.ToString())).FirstOrDefault();
					AssertDocumentContent(job2, "Jerry Test Organisation 2");

					var job3 = jobs.Where(j => j.SP_EmailSubjectLine.Contains(org3.PK.ToString())).FirstOrDefault();
					AssertDocumentContent(job3, "Jerry Test Organisation 3");
				}
			}

			void AssertDocumentContent(StmPrintJob job, string expectedContent)
			{
				using (var excelInterface = new ExcelInterface())
				{
					using (var stream = new MemoryStream(job.SP_CustomProperties))
					{
						excelInterface.LoadExcelFile(stream);
						var contents = excelInterface.WorkSheets[0][0, 1].ToString();

						AssertEquals(expectedContent, contents);
					}
				}
			}
		}

		public void TestStmReportRunWithEDocRecipient_SystemDefinedReportHasNoDocType() => AssertStmReportRunWithEDocRecipient(true, string.Empty, "SREP");

		public void TestStmReportRunWithEDocRecipient_CustomizedReportHasDocType() => AssertStmReportRunWithEDocRecipient(false, "AAA", "AAA");

		public void TestStmReportRunWithEDocRecipient_CustomizedReportHasNoDocType() => AssertStmReportRunWithEDocRecipient(false, string.Empty, string.Empty);

		void AssertStmReportRunWithEDocRecipient(bool isSystemDefined, string newDocType, string expectedDocType)
		{
			TestCaseHelper.ClearTable(StmPrintJob.Schema.TableName);

			var command = DocumentEngineTestHelper.CreateReportCommandWithExcelTemplate("Test Empty Report", newDocType, string.Empty, SimpleTestTemplate, Factory);
			command.SU_IsSystemDefined = isSystemDefined;
			var reportScheduleTask = Factory.New<ReportScheduleTaskForEmptyReportContingencyTesting>();
			reportScheduleTask.S5_ParentID = command.PK;
			reportScheduleTask.S5_EndDate = reportScheduleTask.S5_StartDate.AddDays(10);
			reportScheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddMinutes(-5);

			reportScheduleTask.StmReportRun = Factory.New<StmReportRun>();
			reportScheduleTask.StmReportRun.RRI_S5_Schedule = reportScheduleTask.PK;
			reportScheduleTask.StmReportRun.RRI_StartTimeUtc = ZDateTime.UtcNow;

			var recipient1 = reportScheduleTask.Recipients.AddNew();
			recipient1.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.EDoc;
			recipient1.S6_AttachmentType = AttachmentTypeList.Codes.Csv;

			var docPack = new DocumentPack(command);
			using (var report = (Report)docPack[0])
			{
				report.SetScheduleTask(reportScheduleTask);
				report.PrepareForRender();
				reportScheduleTask.S5_ScheduleState = reportScheduleTask.SerializeForTesting(docPack.DeliveryInstructions, report);
			}
			reportScheduleTask.RunForTesting();

			var printJobs = Factory.Load<StmPrintJob>(new ZQuery());

			AssertEquals("Should have 1 print job", 1, printJobs.Length);
			AssertEquals("QUE", printJobs[0].SP_Status);
			AssertEquals("DDS", printJobs[0].SP_JobType);
			AssertEquals("CSV", printJobs[0].SP_EmailAttachmentFormat);
			AssertEquals(expectedDocType, printJobs[0].SP_DocumentType);
			AssertEquals("Test Empty Report.CSV", printJobs[0].SP_EmailAttachments);
			AssertContains("Test Empty Report (CSV)", printJobs[0].SP_EmailSubjectLine);
			AssertEquals("RTS", printJobs[0].SP_RelatedBusinessContext);
			AssertEquals(reportScheduleTask.StmReportRun.PK, printJobs[0].SP_ParentGuid);
			AssertEquals("StmReportRun", printJobs[0].SP_ParentTableName);
		}

		public void TestRunScheduledReportWithHideSheetIfConditionallyHidden()
		{
			TestCaseHelper.ClearTable(StmPrintJob.Schema.TableName);

			var reportScheduleTask = Factory.NewWithValidTestData<ReportScheduleTaskForEmptyReportContingencyTesting>();

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
			reportScheduleTask.S5_ParentID = reportCommand.PK;

			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var recipient = reportScheduleTask.Recipients.AddNew();
			recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			recipient.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.Staff;
			recipient.S6_AttachmentType = AttachmentTypeList.Codes.Xls;
			recipient.S6_EmailToRecipientsAsString = "Jerry@test.com";
			recipient.S6_GS_NKRecipient = staff.GS_Code;

			Factory.Save();

			using (var pack = new DocumentPack())
			using (var report = new Report(pack, excelTemplate))
			{
				report.PrepareForRender();

				var textField = (TextField)report.FilterCollection["Filter Text"];
				textField.Value = "ShowSheet2";

				reportScheduleTask.S5_ScheduleState = reportScheduleTask.SerializeForTesting(null, report);
			}

			using (Env.SetTemporaryUserContext(User.ServiceUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				reportScheduleTask.RunForTesting();

				var printJobs = Factory.Load<StmPrintJob>(new ZQuery());
				AssertEquals("Should have 1 print job", 1, printJobs.Length);

				using (var excelInterface = new ExcelInterface(printJobs[0].SP_CustomProperties))
				{
					AssertEquals(string.Empty, excelInterface.WorkSheets[0].ToString());
					AssertEquals("{B}-[Jerry Test Sheet2]", excelInterface.WorkSheets[1].ToString());
				}
			}
		}

		public void TestRunPrintTasksForReportWithInvalidDeliveryGroupException()
		{
			var reportCommand = DocumentEngineTestHelper.CreateReportCommandWithExcelTemplate("SimpleTest", SimpleTestTemplate, Factory);
			using (var documentPack = new DocumentPack(reportCommand))
			{
				var report = documentPack[0] as Report;
				report.PrepareForRender();

				var deliveryInstructions = new DeliveryInstructions(documentPack);
				deliveryInstructions.Recipients.RemoveAndDeleteAll();

				var recipient = deliveryInstructions.Recipients.AddNew();
				recipient.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
				recipient.AttachmentType = "PDF";
				recipient.Name = "Unit Test";
				recipient.Email = "unit.test@cargowise.com";

				var reportScheduleTask = Factory.NewWithValidTestData<ReportScheduleTaskForTest>();
				reportScheduleTask.PopulateDefaultsFromDeliveryInstructions(deliveryInstructions);
				reportScheduleTask.S5_ParentID = reportCommand.PK;
				reportScheduleTask.S5_ScheduleState = reportScheduleTask.SerializeForTesting(deliveryInstructions, report);
				Factory.Save();

				using (Env.SetTemporaryUserContext(User.ServiceUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
				{
					var notification = new NotificationBuffer();
					AssertNoExceptionThrown(() => reportScheduleTask.Run(notification));
					AssertContains("'SimpleTest' cannot be delivered, there is something wrong with the delivery group, the error reason is: 'Delivery group id does not exist in db'.", notification.AsString);
				}
			}
		}

		public void TestTemplateCopyShouldNotCopyPrintUserIfNotAllowScheduleOtherStaffAsPrintUser()
		{
			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_Code = "TS1";
			user.GS_FullName = "Test user 1";
			RateSecurityTestHelper.CreateSecurityRight(Factory, user, Env.Security.ScheduleOtherStaffAsPrintUser.Code, false);
			Factory.Save();

			using (Env.SetTemporaryUserContext(user.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				var otherUser = Factory.NewWithValidTestData<GlbStaff>();
				otherUser.GS_Code = "TS2";
				otherUser.GS_FullName = "Test user 2";
				Factory.Save();

				var reportScheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
				reportScheduleTask.S5_GS_NKPrintUser = otherUser.GS_Code;
				Factory.Save();

				var copyTask = (ReportScheduleTask)reportScheduleTask.TemplateCopy();

				AssertEquals("TS2", reportScheduleTask.S5_GS_NKPrintUser);
				AssertEquals("TS1", copyTask.S5_GS_NKPrintUser);
			}
		}

		[TestUtcOffset(-5, 0, 0)]
		[TestDate(2010, 9, 17, 12, 0, 0)]
		public void TestDateScheduleDescriptionShouldUseTodaysDateAsBaseDateInsteadOfTheNextRunDate()
		{
			var reportScheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			reportScheduleTask.S5_IsActive = ZBool.True;
			reportScheduleTask.S5_ScheduleDescription = "BLI DOM - WEEKLY";
			reportScheduleTask.S5_DateScheduleFirstRun = new ZDateTime(2010, 9, 10);
			reportScheduleTask.CalcNextRunTimeLocal = new ZDateTime(2010, 9, 24, 8, 0, 0);
			reportScheduleTask.Recurrence.WeeklyRange = ZBool.True;
			reportScheduleTask.Recurrence.TaskPeriodCount = 1;
			reportScheduleTask.Recurrence.Friday = ZBool.True;
			reportScheduleTask.Recurrence.StartDateLocal = new ZDateTime(2010, 9, 9);

			var dateSchedule = new DateSchedule(reportScheduleTask);

			dateSchedule.ByWeek = ZBool.True;
			dateSchedule.DayName = WeekDayList.Codes.Friday;
			dateSchedule.PeriodScope = PeriodScopeList.Codes.Previous;
			dateSchedule.PeriodCount = 1;

			AssertEquals("dateSchedule.Description", "The Friday of the week prior to when the report is run. If the Report ran today (17-Sep-10), the date used would be 10-Sep-10.", dateSchedule.Description);
		}

		public void TestRunScheduledReportCheckEmailSignature()
		{
			var emailFormat = new EmailFormat();
			emailFormat.EmailSignatureFields.RemoveAndDeleteAll();
			emailFormat.EmailSignatureFields.Add(new EmailSignatureField("1", Core.Constants.EmailFormat.EmailFieldCodes.UserName));

			DocumentsDataRegistry.Instance.EmailFormat.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, emailFormat);

			var reportCommand = DocumentEngineTestHelper.CreateReportCommandWithExcelTemplate("SimpleTest", SimpleTestTemplate, Factory);
			reportCommand.SU_EmailSenderOverride = "TestEmailSignature@Cargowise.com";
			reportCommand.SU_IsSystemDefined = ZBool.True;
			Factory.Save();

			using (var documentPack = new DocumentPack(reportCommand))
			{
				var report = documentPack[0] as Report;
				report.PrepareForRender();

				var deliveryInstructions = new DeliveryInstructions(documentPack);
				deliveryInstructions.Recipients.RemoveAndDeleteAll();

				var recipient = deliveryInstructions.Recipients.AddNew();
				recipient.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
				recipient.AttachmentType = "PDF";
				recipient.Name = "Unit Test";
				recipient.Email = "unit.test@cargowise.com";

				var reportScheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
				reportScheduleTask.PopulateDefaultsFromDeliveryInstructions(deliveryInstructions);
				reportScheduleTask.S5_ParentID = reportCommand.PK;
				reportScheduleTask.S5_ScheduleState = reportScheduleTask.SerializeForTesting(deliveryInstructions, report);
				Factory.Save();

				using (Env.SetTemporaryUserContext(User.ServiceUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
				{
					reportScheduleTask.Run();
				}

				var printJobs = Factory.Load<StmPrintJob>(new ZQuery());
				AssertEquals("1 print job should be created", 1, printJobs.Length);
				AssertEquals("SimpleTest" + StmPrintJob.LanguageDelimiter + DataRegistry.Instance.EnglishSpelling, printJobs[0].SP_DocumentName);
				AssertEquals("the email signature should equal current user's full name.", GlbStaff.CurrentUser.GS_FullName, printJobs[0].SP_EmailSignature);
			}
		}

		public void TestScheduleReportAutoHealFilterSerializedByCode()
		{
			var reportScheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();

			var org = DocumentEngineTestHelper.GetNewOrganization("testOrg", Factory);

			var templateContent = new Dictionary<string, string>();
			templateContent.Add("Template",
@"{A}-[#Config]
{A}-[#EndOfReport]");

			templateContent.Add("Filters",
@"{A}-[filter] {B}-[Type] {C}-[Organisation MultipleSelectionLookup]
{B}-[Field] {C}-[OH_PK]
{A}-[#End]");

			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", "", templateContent);
			var reportCommand = DocumentEngineTestHelper.CreateReportCommandWithExcelTemplate("Test Org Code Serialize", excelTemplate, Factory);
			reportScheduleTask.S5_ParentID = reportCommand.PK;

			using (var pack = new DocumentPack())
			using (var report = new Report(pack, excelTemplate))
			{
				report.PrepareForRender();

				var filter = report.FilterCollection["filter"];
				var orgFilter = (MultipleSelectionLookup)filter;
				orgFilter.ValueAsStringForSerialisation = "testOrg";
				Assert(!orgFilter.SerialisedByPK);
				reportScheduleTask.S5_ScheduleState = reportScheduleTask.SerializeForTesting(null, report);
			}

			using (Env.SetTemporaryUserContext(User.ServiceUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				reportScheduleTask.Run();

				org.OH_Code = "testOrg1";
				reportScheduleTask.Run();

				var deserializedValue = reportScheduleTask.CreateReportFromTask();
				using (var deserializedReport = deserializedValue.Report)
				{
					AssertEquals("Report.FilterCollection.Count", 1, deserializedReport.FilterCollection.Count);
					Assert(deserializedReport.FilterCollection[0] is MultipleSelectionLookup);

					var orgFilter = (MultipleSelectionLookup)deserializedReport.FilterCollection[0];
					Assert("org filter should not have validation error", !orgFilter.BindToList.HasErrors());
					Assert("org filter should be serialized by pk after running", orgFilter.SerialisedByPK);
					AssertEquals(org.PK.ToString(), orgFilter.ValueAsStringForSerialisation);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[GuiTest, TestDate(2008, 8, 8)]
		public void TestRunScheduledReportWithValidationErrors()
		{
			SetUpPostMasterGroup();

			var excelTemplate = new ExcelTemplateForUnitTesting("RequiredTextFilter.xls", TestFilesSubFolder.ReportTestFiles);
			var reportCommand = DocumentEngineTestHelper.CreateReportCommandWithExcelTemplate("RequiredTextFilter", excelTemplate, Factory);
			reportCommand.SU_BusinessContext = "RepJobCostingReport";
			reportCommand.SU_IsSystemDefined = ZBool.True;
			Factory.Save();

			using (Report.TemporarilyStopErrorsThrowingAnException())
			using (var documentPack = new DocumentPack(reportCommand))
			{
				var report = documentPack[0] as Report;
				report.PrepareForRender();

				var deliveryInstructions = new DeliveryInstructions(documentPack);
				deliveryInstructions.Recipients.RemoveAndDeleteAll();

				var recipient = deliveryInstructions.Recipients.AddNew();
				recipient.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
				recipient.AttachmentType = "PDF";
				recipient.Name = "Unit Test";
				recipient.Email = "unit.test@cargowise.com";

				var branch = Factory.NewWithValidTestData<GlbBranch>();
				branch.GB_Code = "XYZ";

				var reportScheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
				reportScheduleTask.PopulateDefaultsFromDeliveryInstructions(deliveryInstructions);
				reportScheduleTask.S5_ParentID = reportCommand.PK;
				reportScheduleTask.S5_ScheduleState = reportScheduleTask.SerializeForTesting(deliveryInstructions, report);
				reportScheduleTask.S5_EndDate = reportScheduleTask.S5_StartDate.AddDays(10);
				reportScheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddMinutes(-5);
				reportScheduleTask.S5_GB = branch.PK;

				Factory.Save();

				AssertEquals("reportScheduleTask.S5_IsActive", true, reportScheduleTask.S5_IsActive);

				reportScheduleTask.Run();

				AssertEquals("Emails to Client", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				var email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("email.Recipients", "staff@group.com", email.Recipients.RecipientsAsDelimitedString());
				AssertEquals("Error Running Scheduled Report [RequiredTextFilter]", email.Subject);
				AssertMultilineASCIIEquals("email.Body", string.Format(
@"Scheduled Report Information:

   Description = [RequiredTextFilter]
   Start Date = [08-Aug-08 00:00:00]
   End Date = [18-Aug-08 00:00:00]
   Report = [RequiredTextFilter : RepJobCostingReport :  : NCT]
   Branch = [XYZ]

This scheduled report has been marked inactive until the following problems have been resolved:-


Errors Found
---------------
Severity: [Error (without error report)] Message: [Error - Value: 'Some description' should have data.] Cell: [N/A] Sheetname: [(unknown)] Cell Content: [] Occurences: [1]"),
					email.Body);

				Env.OutgoingMailManager.EmailsCreated.Clear();

				AssertEquals("reportScheduleTask.S5_IsActive", false, reportScheduleTask.S5_IsActive);
			}
		}

		public void TestRunScheduleReportWithDisableXLSXExport()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[DisableXLSXExport]
{A}-[Data:ReportData=Select DummyBizo1.Z0_Number as Number from dbo.DummyBizo as DummyBizo1 cross join dbo.DummyBizo as DummyBizo2 cross join dbo.DummyBizo as DummyBizo3]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.Number>]
{A}-[#EndOfReport]");

			var reportCommand = Factory.New<ReportCommand>();
			var pivot = reportCommand.Documents.AddNew();
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;

			using (var pack = new DocumentPack(reportCommand))
			using (var report = Report.NewForTesting(pack))
			{
				ReportScheduleTask scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
				report.SetScheduleTask(scheduleTask);
				scheduleTask.S5_ParentID = reportCommand.PK;
				Assert(scheduleTask.DisableXLSXExport);
			}

			template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[Data:ReportData=Select DummyBizo1.Z0_Number as Number from dbo.DummyBizo as DummyBizo1 cross join dbo.DummyBizo as DummyBizo2 cross join dbo.DummyBizo as DummyBizo3]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.Number>]
{A}-[#EndOfReport]");

			pivot.SI_SO = template.PK;

			using (var pack = new DocumentPack(reportCommand))
			using (var report = Report.NewForTesting(pack))
			{
				ReportScheduleTask scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
				report.SetScheduleTask(scheduleTask);
				scheduleTask.S5_ParentID = reportCommand.PK;
				Assert(!scheduleTask.DisableXLSXExport);
			}
		}

		public void TestDisableCSVExport()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[DisableCSVExport]
{A}-[Data:ReportData=Select DummyBizo1.Z0_Number as Number from dbo.DummyBizo as DummyBizo1 cross join dbo.DummyBizo as DummyBizo2 cross join dbo.DummyBizo as DummyBizo3]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.Number>]
{A}-[#EndOfReport]");

			var reportCommand = Factory.New<ReportCommand>();
			var pivot = reportCommand.Documents.AddNew();
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;

			using (var pack = new DocumentPack(reportCommand))
			using (var report = Report.NewForTesting(pack))
			{
				ReportScheduleTask scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
				report.SetScheduleTask(scheduleTask);
				scheduleTask.S5_ParentID = reportCommand.PK;
				Assert(scheduleTask.DisableCSVExport);
			}

			template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[Data:ReportData=Select DummyBizo1.Z0_Number as Number from dbo.DummyBizo as DummyBizo1 cross join dbo.DummyBizo as DummyBizo2 cross join dbo.DummyBizo as DummyBizo3]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.Number>]
{A}-[#EndOfReport]");

			pivot.SI_SO = template.PK;

			using (var pack = new DocumentPack(reportCommand))
			using (var report = Report.NewForTesting(pack))
			{
				ReportScheduleTask scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
				report.SetScheduleTask(scheduleTask);
				scheduleTask.S5_ParentID = reportCommand.PK;
				Assert(!scheduleTask.DisableCSVExport);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRunScheduleReportDetectsIncompatibleTemplateChangeAndEmailUserAndMarkInActive()
		{
			SetUpPostMasterGroup();
			var reportCommand = Factory.NewWithValidTestData<ReportCommand>();
			reportCommand.SU_BusinessContext = "Test";
			reportCommand.SU_MenuPath = "Testing/";
			reportCommand.SU_MenuName = "Test Stuff";
			reportCommand.SU_FilterList = "";

			var excelTemplate = new ExcelTemplateForUnitTesting("FilterSortGroupby.xls", TestFilesSubFolder.ReportTestFiles);
			var template = Factory.New<StmTemplate>();
			template.SO_Name = "Filter Sort Group by";
			template.SO_Template = excelTemplate.GetAsByteArray();

			var pivot = Factory.New<StmMenuTemplatePivot>();
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;
			Factory.Save();

			using (var pack = new DocumentPack(reportCommand))
			{
				var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
				var report = (Report)pack[0];
				report.SetScheduleTask(scheduleTask);
				report.PrepareForRender();
				var instructions = new DeliveryInstructions(pack);
				instructions.Recipients.RemoveAll();

				var contact = instructions.Recipients.AddNew();
				contact.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Email;
				contact.AttachmentType = OrgConstants.AttachmentType.PDF;
				contact.OrgHeaderPK = Organization.PK;
				contact.Name = "charlie";
				contact.Email = "test@test.com";

				scheduleTask.PopulateDefaultsFromDeliveryInstructions(instructions);
				scheduleTask.S5_ScheduleDescription = "Test Report 1";
				Factory.Save();

				var taskGuid = scheduleTask.PK;
				// modify template
				var stmTemplate = Factory.Load<StmTemplate>(template.PK);
				stmTemplate.SO_Template = new ExcelTemplateForUnitTesting("TextFilter.xls", TestFilesSubFolder.ReportTestFiles).GetAsByteArray();
				Factory.Save();

				using (var task = Factory.Load<ReportScheduleTaskForTesting>(taskGuid))
				{
					AssertEquals("task.S5_IsActive", true, task.S5_IsActive);
					task.Run();
					AssertEquals("task.S5_IsActive", false, task.S5_IsActive);
				}
				AssertEquals("Emails to Client", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				var email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("email.Recipients", "staff@group.com", email.Recipients.RecipientsAsDelimitedString());
				AssertEquals("Error encountered while running schedule report", email.Subject);
				AssertMultilineASCIIEquals("email.Body", string.Format(
@"Report Name: [Test Report 1]

Report Information:

MenuItem:-
   BusinessContext = [Test]
   Name with Path = [Testing/Test Stuff]
   Filter = []
   PK = [{0}]
   IsSystemDefined = [N]
   IsClientSpecific = [N]

Template:-
   Name = [Filter Sort Group by]
   DataContext = []
   ExcelFilePath = []
   PK = [{1}]
   IsSystemDefined = [N]
   IsClientSpecific = [N]

Scheduled Task:-
   Description = [Test Report 1]

Errors Found
---------------
Report template has been modified since report was scheduled, current schedule is canceled since continue running it might cause unexpected behavior.
Please verify the report will work with the updated template then remark the schedule task as Active.

Validation failure details: 

>> Failed to find a compatible Filter Field, please check the updated report template and make sure the following Filter Field exists, if the filter does exist, please make sure it's compatible with the saved filter field value:

Filter display name: [Age]
Filter type: [NumberField]
Filter field list: [xx_foo]
Saved filter value: [42]

Filter display name: [Description Display]
Filter type: [MultipleChoice]
Filter field list: []
Saved filter value: [HDR]

Filter display name: [Text Stuff]
Filter type: [TextField]
Filter field list: [XX_Field]
Saved filter value: [Hello]

>> Failed to find a compatible Group By, please check the updated report template and make sure the following Group By exists:

Group By display name: [Period]
Group By field list: [GL.Period,GL.GLAccount]

>> Failed to find a compatible Sort Order, please check the updated report template and make sure the following Sort Order exists:

Sort Order display name: [Country]
Sort Order field list: [Country, Objective]", reportCommand.PK, template.PK), email.Body.Trim());
				Env.OutgoingMailManager.EmailsCreated.Clear();
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRunScheduleReportDetectsIncompatibleTemplateChangeAndEmailUserAndMarkInActive_SendNotificationEmailToGroupRole()
		{
			SetUpNotificationGroup();
			AssertSendNotificationEmailToStaffRole("groupStaff@user.com");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRunScheduleReportDetectsIncompatibleTemplateChangeAndEmailUserAndMarkInActive_SendNotificationEmailToStaffRole()
		{
			SetUpNotificationStaffRole("JTR");
			AssertSendNotificationEmailToStaffRole("roleStaff@user.com");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRunScheduleReportDetectsIncompatibleTemplateChangeAndEmailUserAndMarkInActive_SendNotificationEmailToGroupRoleIfNoStaffRole()
		{
			SetUpNotificationGroup();
			SetUpNotificationStaffRole("JTT");
			AssertSendNotificationEmailToStaffRole("groupStaff@user.com");
		}

		void AssertSendNotificationEmailToStaffRole(string expectedEmailAddress)
		{
			var reportCommand = Factory.NewWithValidTestData<ReportCommand>();
			reportCommand.SU_BusinessContext = "Test";
			reportCommand.SU_MenuPath = "Testing/";
			reportCommand.SU_MenuName = "Test Stuff";
			reportCommand.SU_FilterList = "";

			var excelTemplate = new ExcelTemplateForUnitTesting("FilterSortGroupby.xls", TestFilesSubFolder.ReportTestFiles);
			var template = Factory.New<StmTemplate>();
			template.SO_Name = "Filter Sort Group by";
			template.SO_Template = excelTemplate.GetAsByteArray();

			var pivot = Factory.New<StmMenuTemplatePivot>();
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;
			Factory.Save();

			using (var pack = new DocumentPack(reportCommand))
			{
				var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
				var report = (Report)pack[0];
				report.SetScheduleTask(scheduleTask);
				report.PrepareForRender();
				var instructions = new DeliveryInstructions(pack);
				instructions.Recipients.RemoveAll();

				var newStaff = Factory.NewWithValidTestData<GlbStaff>();
				newStaff.GS_Code = "RSF";
				newStaff.GS_EmailAddress = "roleStaff@user.com";

				var staffAssignments = Organization.StaffAssignments.AddNew();
				staffAssignments.O8_Role = "JTR";
				staffAssignments.O8_GS_NKPersonResponsible = newStaff.GS_Code;

				var contact = instructions.Recipients.AddNew();
				contact.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Email;
				contact.AttachmentType = OrgConstants.AttachmentType.PDF;
				contact.OrgHeaderPK = Organization.PK;
				contact.Name = "charlie";
				contact.Email = "test@test.com";

				scheduleTask.PopulateDefaultsFromDeliveryInstructions(instructions);
				scheduleTask.S5_ScheduleDescription = "Test Report 1";
				Factory.Save();

				var taskGuid = scheduleTask.PK;
				var stmTemplate = Factory.Load<StmTemplate>(template.PK);
				stmTemplate.SO_Template = new ExcelTemplateForUnitTesting("TextFilter.xls", TestFilesSubFolder.ReportTestFiles).GetAsByteArray();
				Factory.Save();

				var notifications = new NotificationBuffer();
				using (var task = Factory.Load<ReportScheduleTaskForTesting>(taskGuid))
				{
					AssertEquals("task.S5_IsActive", true, task.S5_IsActive);
					task.Run(notifications);
					AssertEquals("task.S5_IsActive", false, task.S5_IsActive);
				}
				AssertEquals("Emails to Client", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				var email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("email.Recipients", expectedEmailAddress, email.Recipients.RecipientsAsDelimitedString());
				AssertEquals("Error encountered while running schedule report", email.Subject);
				AssertContains(@"Report template has been modified since report was scheduled, current schedule is canceled since continue running it might cause unexpected behavior.
Please verify the report will work with the updated template then remark the schedule task as Active.", email.Body.Trim());
				AssertContains(@"Report template has been modified since report was scheduled, current schedule is canceled since continue running it might cause unexpected behavior.
Please verify the report will work with the updated template then remark the schedule task as Active.", notifications.AsString);
				AssertContains(string.Format(@"Error notification email was sent to the following email address.
{0}", expectedEmailAddress), notifications.AsString);
				Env.OutgoingMailManager.EmailsCreated.Clear();
			}
		}

		void SetUpNotificationGroup()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var staff = group.Staff.AddNew();
			staff.GS_EmailAddress = "groupStaff@user.com";
			staff.GS_LoginName = "JerryTest";
			staff.GS_Code = "JYT";

			SystemDataRegistry.Instance.SRRErrorNotificationOptions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Enterprise.Core.Constants.ErrorNotificationOptions.Code.GRP);
			SystemDataRegistry.Instance.SRRErrorNotificationGroups.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
		}

		void SetUpNotificationStaffRole(string code)
		{
			var staffRoles = new CodeDescriptionBoolDisallowNewCollection
			{
				new CodeDescriptionBoolDisallowNew { Bool = true, Code = code },
			};

			SystemDataRegistry.Instance.SRRErrorNotificationStaffRoles.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, staffRoles);
			SystemDataRegistry.Instance.SRRErrorNotificationOptions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Enterprise.Core.Constants.ErrorNotificationOptions.Code.ROL);
		}

		public void TestRunScheduleReport_WhenInfrastructureDbErrorOccurs_DontDeactive()
		{
			using (var report = GetReportForScheduleReport())
			{
				var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTaskForTestingInfrastructureErrors>();
				scheduleTask.PopulateDefaultsFromDeliveryInstructions(GetDeliveryInstructionsForScheduleReport(report.Parent));
				scheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow;
				scheduleTask.S5_IsPrivate = true;
				Factory.Save();

				var initialS5_NextScheduledPrintRunTime = scheduleTask.S5_NextScheduledPrintRunTimeUtc;
				scheduleTask.DbServerName = "Test Report Server initial";

				var notifications = new NotificationBuffer();
				AssertExceptionThrown<SqlException>(() => scheduleTask.RunSafe(notifications));

				var anotherFactory = new BusinessObjectFactory();
				var reloadedScheduleTask = anotherFactory.Load<ReportScheduleTask>(scheduleTask.PK);
				Assert("Scheduled report should not be deactivated when an infrastructure DB error occurs.", reloadedScheduleTask.S5_IsActive);
				Assert(notifications.HasWarnings);
				AssertContains("Scheduled Task 'Client - Order Status Summary' could not run due to an infrastructure DB error on the DB server 'Test Report Server New', exception type: AlwaysOnAccessError, SQL error number: 983 (see next event for details). It will be run again later.", notifications.AsString);
				AssertEquals("S5_NextScheduledPrintRunTimeUtc should not have been updated", initialS5_NextScheduledPrintRunTime, scheduleTask.S5_NextScheduledPrintRunTimeUtc);

				notifications.Clear();
				scheduleTask = Factory.NewWithValidTestData<ReportScheduleTaskForTestingInfrastructureErrors>();
				scheduleTask.PopulateDefaultsFromDeliveryInstructions(GetDeliveryInstructionsForScheduleReport(report.Parent));
				scheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow;
				scheduleTask.S5_IsPrivate = false;
				Factory.Save();
				AssertExceptionThrown<SqlException>(() => scheduleTask.RunSafe(notifications));
				AssertContains($"Scheduled Task 'Client - Order Status Summary' could not run due to an infrastructure DB error on the DB server '{Db.ServerName}', exception type: AlwaysOnAccessError, SQL error number: 983 (see next event for details). It will be run again later.", notifications.AsString);
			}
		}

		public void TestRunScheduleReport_WhenLoginNameChanged_DontDeactive()
		{
			var reportCommand = DocumentEngineTestHelper.CreateReportCommandWithExcelTemplate("SimpleTest", SimpleTestTemplate, Factory);

			using (var documentPack = new DocumentPack(reportCommand))
			{
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_LoginName = "staff";
				staff.GS_Code = "STA";

				var report = documentPack[0] as Report;
				report.PrepareForRender();

				var deliveryInstructions = new DeliveryInstructions(documentPack);

				var reportScheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
				reportScheduleTask.PopulateDefaultsFromDeliveryInstructions(deliveryInstructions);
				reportScheduleTask.UserFK = staff.PK;
				reportScheduleTask.S5_ParentID = reportCommand.PK;
				reportScheduleTask.S5_ScheduleState = reportScheduleTask.SerializeForTesting(deliveryInstructions, report);

				staff.GS_LoginName = "staff2";

				Factory.Save();

				var buffer = new NotificationBuffer();
				reportScheduleTask.Run(buffer);

				Assert(reportScheduleTask.S5_IsActive);
				Assert(!buffer.HasErrors);
			}
		}

		public void TestRunScheduleReport_MaxReportConnectionsExceeded()
		{
			var mutex = new ReportMutex();
			var report1 = Report.NewForTesting(new DocumentPack());

			try
			{
				SystemDataRegistry.Instance.ReportMaxConnections.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);
				mutex.Lock(report1);

				using (var report2 = GetReportForScheduleReport())
				{
					var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
					scheduleTask.PopulateDefaultsFromDeliveryInstructions(GetDeliveryInstructionsForScheduleReport(report2.Parent));
					scheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow;
					Factory.Save();

					var notifications = new NotificationBuffer();
					AssertExceptionThrown<MaxConcurrentReportConnectionsExceeded>(() => scheduleTask.RunSafe(notifications));

					var anotherFactory = new BusinessObjectFactory();
					var reloadedScheduleTask = anotherFactory.Load<ReportScheduleTask>(scheduleTask.PK);
					Assert("Scheduled report should be deactivated when an MaxReportConnectionsExceeded error occurs and retry invalid.", !reloadedScheduleTask.S5_IsActive);
					Assert("S5_NextScheduledPrintRunTimeUtc should be empty", scheduleTask.S5_NextScheduledPrintRunTimeUtc.IsEmpty);
					Assert(notifications.HasWarnings);
					AssertContains("Scheduled Report 'Client - Order Status Summary' could not be delivered at this time due to maximum concurrent report limit reached, and will be processed again on next run cycle.\r\nThe following reports are currently running:\r\n\r\n", notifications.AsString);
				}
			}
			finally
			{
				mutex.Unlock(report1);
				ErrorReporter.Clear();
			}
		}

		public void TestRunScheduleReport_10RetriesFail_ShouldNotifyUser()
		{
			var mutex = new ReportMutex();
			var report1 = Report.NewForTesting(new DocumentPack());

			var newStaff = Factory.NewWithValidTestData<GlbStaff>();
			newStaff.GS_Code = "GS";
			newStaff.GS_EmailAddress = "staff@email.com";
			Factory.Save();

			try
			{
				SystemDataRegistry.Instance.ReportMaxConnections.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);
				mutex.Lock(report1);

				using (var report2 = GetReportForScheduleReport())
				using (Env.SetTemporaryUserContext(newStaff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
				{
					var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
					scheduleTask.PopulateDefaultsFromDeliveryInstructions(GetDeliveryInstructionsForScheduleReport(report2.Parent));
					scheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow;
					scheduleTask.S5_SystemLastEditUser = newStaff.GS_Code;
					Factory.Save();

					var notifications = new NotificationBuffer();
					AssertExceptionThrown<MaxConcurrentReportConnectionsExceeded>(() => scheduleTask.RunSafe(notifications));

					AssertEquals("One error email should be created.", 1, Env.OutgoingMailManager.EmailsCreated.Count);
					var emailCreated = Env.OutgoingMailManager.EmailsCreated[0];
					AssertEquals("Email.Recipients.Count", 1, emailCreated.Recipients.Count);
					AssertEquals("Email.Recipients[0]", newStaff.GS_EmailAddress, emailCreated.Recipients[0]);
					AssertEquals("Email subject", true, emailCreated.Subject.Contains("has been deactivated"));
					AssertEquals("Email Body", true, emailCreated.Body.Contains("could not be delivered at this time due to maximum concurrent report limit reached"));
				}
			}
			finally
			{
				mutex.Unlock(report1);
				ErrorReporter.Clear();
			}
		}

		[ExpectNoExceptions]
		public void TestRunScheduleReportWithReportDB()
		{
			using (var report = GetReportForScheduleReport())
			{
				var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTaskForTestingWithReportDb>();
				scheduleTask.PopulateDefaultsFromDeliveryInstructions(GetDeliveryInstructionsForScheduleReport(report.Parent));
				scheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow;
				Factory.Save();

				var initialS5_NextScheduledPrintRunTime = scheduleTask.S5_NextScheduledPrintRunTimeUtc;

				var notifications = new NotificationBuffer();
				scheduleTask.RunSafe(notifications);

				Assert(!notifications.HasErrors);
				Assert(!notifications.HasWarnings);
				Assert("S5_NextScheduledPrintRunTimeUtc should have been updated", initialS5_NextScheduledPrintRunTime < scheduleTask.S5_NextScheduledPrintRunTimeUtc);
			}
		}

		public void TestIsErrorThatShouldRetry()
		{
			var task = Factory.NewWithValidTestData<ReportScheduleTask>();
			Assert("should retry on maxconcurrent", task.IsErrorThatShouldRetryForTesting(new MaxConcurrentReportConnectionsExceeded("", "")));
			Assert("should retry on metadatachanged", task.IsErrorThatShouldRetryForTesting(new MetadataHasChangedException()));
			Assert("Should retry on deleted ghost records", task.IsErrorThatShouldRetryForTesting(new GhostRecordsBeingDeletedException()));
		}

		public void TestRetryWaitPeriod()
		{
			var task = Factory.NewWithValidTestData<ReportScheduleTask>();
			AssertEquals(TimeSpan.FromSeconds(1), task.RetryWaitPeriodForTesting);
		}

		public void TestSetDefaultValues()
		{
			AssertEquals("S5_ScheduleType", ReportScheduleTask.ScheduleType, ScheduleTask.S5_ScheduleType);
			AssertEquals("S5_ParentTableCode", StmMenuItemSchema.Constants.Prefix, ScheduleTask.S5_ParentTableCode);
		}

		public void TestLookups()
		{
			AssertEquals(typeof(ReportScheduleTaskLookups), ScheduleTask.Lookups.GetType());
		}

		public void TestValidation()
		{
			AssertEquals(typeof(ReportScheduleTaskValidation), ScheduleTask.Validation.GetType());
		}

		[TestDate(2006, 1, 1)]
		public void TestIsActive()
		{
			ScheduleTask.S5_StartDate = ZDateTime.Today;
			ScheduleTask.S5_TaskPeriod = "D";
			ScheduleTask.S5_TaskPeriodCount = 1;
			ScheduleTask.S5_IsActive = false;
			Factory.Save();

			AssertEquals(false, ScheduleTask.S5_IsActive);
			AssertEquals(ZDateTime.Empty, ScheduleTask.S5_NextScheduledPrintRunTimeUtc);

			ScheduleTask.S5_IsActive = true;
			Factory.Save();

			AssertEquals(true, ScheduleTask.S5_IsActive);
			AssertNotEquals(ZDateTime.Empty, ScheduleTask.S5_NextScheduledPrintRunTimeUtc);
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate]
		public void TestCalculateNewDailyScheduleDate()
		{
			TestDateAttribute.UseUNLOCO = true;
			TestDateAttribute.Date = new DateTime(2018, 2, 10, 22, 0, 0);

			var reportScheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			var printerPK = ZGuid.NewZGuid();
			Instructions.PrinterDelivery.PrintQueuePK = printerPK;
			reportScheduleTask.PopulateDefaultsFromDeliveryInstructions(Instructions);
			reportScheduleTask.Branch.GB_RL_NKHomePort = "AUSYD";
			reportScheduleTask.S5_WeekDaysOnly = false;
			reportScheduleTask.S5_StartDate = Env.Time.GetUtcFromLocalTime(new ZDateTime(2018, 2, 4).ToDateTime());
			reportScheduleTask.Recurrence.TaskPeriod = ScheduleRecurrenceType.Daily;
			reportScheduleTask.S5_NextScheduledPrintRunTimeUtc = Env.Time.GetUtcFromLocalTime(ZDateTime.Today.AddDays(-5).ToDateTime());
			reportScheduleTask.S5_DailyStartTime = new ZDateTime(1991, 1, 1, 10, 50, 0);

			Factory.Save();

			AssertEquals("S5_StartDate should not be changed", new ZDateTime(2018, 2, 3, 13, 0, 0), reportScheduleTask.S5_StartDate);
			AssertEquals("Next Run Time Local date will be set to today", ZDateTime.Today.AddHours(10).AddMinutes(50), reportScheduleTask.CalcNextRunTimeLocal);

			reportScheduleTask.S5_DailyStartTime = new ZDateTime(1991, 1, 1, 6, 50, 0);
			Factory.Save();

			AssertEquals("S5_StartDate should not be changed", new ZDateTime(2018, 2, 3, 13, 0, 0), reportScheduleTask.S5_StartDate);
			AssertEquals("Next Run Time Local date will be set to tomorrow", ZDateTime.Today.AddDays(1).AddHours(6).AddMinutes(50), reportScheduleTask.CalcNextRunTimeLocal);

			reportScheduleTask.S5_WeekDaysOnly = true;
			Factory.Save();

			AssertEquals("S5_StartDate should be changed to 04/02/2018 13:00:00", Env.Time.GetUtcFromLocalTime(ZDateTime.Today.AddDays(-6).ToDateTime()), reportScheduleTask.S5_StartDate);
			AssertEquals("Next Run Time Local date will be set to the nearest Monday", ZDateTime.Today.AddDays(1).AddHours(6).AddMinutes(50), reportScheduleTask.CalcNextRunTimeLocal);

			reportScheduleTask.S5_StartDate = Env.Time.GetUtcFromLocalTime(new ZDateTime(2018, 2, 17).ToDateTime());
			Factory.Save();

			AssertEquals("S5_StartDate will be changed to 18/02/2018 13:00:00", new ZDateTime(2018, 2, 18, 13, 0, 0), reportScheduleTask.S5_StartDate);
			AssertEquals("Next Run Time Local date will be set to 19/02/2018 06:50:00", new ZDateTime(2018, 2, 19, 6, 50, 0), reportScheduleTask.CalcNextRunTimeLocal);

			reportScheduleTask.S5_WeekDaysOnly = false;
			reportScheduleTask.S5_StartDate = Env.Time.GetUtcFromLocalTime(new ZDateTime(2018, 2, 16).ToDateTime());
			Factory.Save();

			AssertEquals("S5_StartDate will not be changed", new ZDateTime(2018, 2, 15, 13, 0, 0), reportScheduleTask.S5_StartDate);
			AssertEquals("Next Run Time Local date will be set to 16/02/2018 06:50:00", new ZDateTime(2018, 2, 16, 6, 50, 0), reportScheduleTask.CalcNextRunTimeLocal);
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate]
		public void TestCalculateNewWeeklyScheduleDate()
		{
			TestDateAttribute.UseUNLOCO = true;
			TestDateAttribute.Date = new DateTime(2018, 2, 10, 22, 0, 0);

			var reportScheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			var printerPK = ZGuid.NewZGuid();
			Instructions.PrinterDelivery.PrintQueuePK = printerPK;
			reportScheduleTask.PopulateDefaultsFromDeliveryInstructions(Instructions);
			reportScheduleTask.Branch.GB_RL_NKHomePort = "AUSYD";
			reportScheduleTask.S5_StartDate = Env.Time.GetUtcFromLocalTime(new ZDateTime(2018, 2, 4).ToDateTime());
			reportScheduleTask.Recurrence.TaskPeriod = ScheduleRecurrenceType.Weekly;
			reportScheduleTask.Recurrence.TaskPeriodCount = 2;
			reportScheduleTask.Recurrence.DayList = "YNYYNNN";
			reportScheduleTask.S5_NextScheduledPrintRunTimeUtc = Env.Time.GetUtcFromLocalTime(ZDateTime.Today.AddDays(-5).ToDateTime());
			reportScheduleTask.S5_DailyStartTime = new ZDateTime(1991, 1, 1, 10, 50, 0);

			Factory.Save();

			AssertEquals("S5_StartDate should not be changed (03/02/2018 13:00:00)", new ZDateTime(2018, 2, 3, 13, 0, 0), reportScheduleTask.S5_StartDate);
			AssertEquals("Next Run Time Local date will be set to Today (11/02/2018 10:50:00)", new ZDateTime(2018, 2, 11, 10, 50, 0), reportScheduleTask.CalcNextRunTimeLocal);

			reportScheduleTask.S5_DailyStartTime = new ZDateTime(1991, 1, 1, 6, 50, 0);
			Factory.Save();

			AssertEquals("S5_StartDate should not be changed (03/02/2018 13:00:00)", new ZDateTime(2018, 2, 3, 13, 0, 0), reportScheduleTask.S5_StartDate);
			AssertEquals("Next Run Time Local date will be set to Tuesday (13/02/2018 06:50:00)", new ZDateTime(2018, 2, 13, 6, 50, 0), reportScheduleTask.CalcNextRunTimeLocal);

			reportScheduleTask.Recurrence.DayList = "NYYYNNN";
			Factory.Save();

			AssertEquals("S5_StartDate should be changed to 04/02/2018 13:00:00", new ZDateTime(2018, 2, 4, 13, 0, 0), reportScheduleTask.S5_StartDate);
			AssertEquals("Next Run Time Local date will be set to the Monday (12/02/2018 06:50:00)", new ZDateTime(2018, 2, 12, 6, 50, 0), reportScheduleTask.CalcNextRunTimeLocal);

			reportScheduleTask.S5_StartDate = Env.Time.GetUtcFromLocalTime(new ZDateTime(2018, 2, 17).ToDateTime());
			Factory.Save();

			AssertEquals("S5_StartDate will be changed to 18/02/2018 13:00:00", new ZDateTime(2018, 2, 18, 13, 0, 0), reportScheduleTask.S5_StartDate);
			AssertEquals("Next Run Time Local date will be set to 19/02/2018 06:50:00", new ZDateTime(2018, 2, 19, 6, 50, 0), reportScheduleTask.CalcNextRunTimeLocal);
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate]
		public void TestCalculateNewMonthlyScheduleDate()
		{
			TestDateAttribute.UseUNLOCO = true;
			TestDateAttribute.Date = new DateTime(2018, 2, 10, 22, 0, 0);

			var reportScheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			var printerPK = ZGuid.NewZGuid();
			Instructions.PrinterDelivery.PrintQueuePK = printerPK;
			reportScheduleTask.PopulateDefaultsFromDeliveryInstructions(Instructions);
			reportScheduleTask.Branch.GB_RL_NKHomePort = "AUSYD";
			reportScheduleTask.S5_StartDate = Env.Time.GetUtcFromLocalTime(new ZDateTime(2018, 2, 4).ToDateTime());
			reportScheduleTask.Recurrence.TaskPeriod = ScheduleRecurrenceType.Monthly;
			reportScheduleTask.Recurrence.TaskPeriodCount = 2;
			reportScheduleTask.Recurrence.WeekDayOccurrenceNumber = 0;
			reportScheduleTask.Recurrence.DayOfMonth = 11;
			reportScheduleTask.S5_NextScheduledPrintRunTimeUtc = Env.Time.GetUtcFromLocalTime(ZDateTime.Today.AddDays(-5).ToDateTime());
			reportScheduleTask.S5_DailyStartTime = new ZDateTime(1991, 1, 1, 10, 50, 0);

			Factory.Save();

			AssertEquals("S5_StartDate will be changed to 10/02/2018 13:00:00", new ZDateTime(2018, 2, 10, 13, 0, 0), reportScheduleTask.S5_StartDate);
			AssertEquals("Next Run Time Local date will be set to 11/02/2018 10:50:00", new ZDateTime(2018, 2, 11, 10, 50, 0), reportScheduleTask.CalcNextRunTimeLocal);

			reportScheduleTask.S5_DailyStartTime = new ZDateTime(1991, 1, 1, 6, 50, 0);
			Factory.Save();

			AssertEquals("S5_StartDate should not be changed (10/02/2018 13:00:00)", new ZDateTime(2018, 2, 10, 13, 0, 0), reportScheduleTask.S5_StartDate);
			AssertEquals("Next Run Time Local date will be set to 11/02/2018 06:50:00", new ZDateTime(2018, 2, 11, 6, 50, 0), reportScheduleTask.CalcNextRunTimeLocal);

			reportScheduleTask.Recurrence.DayNumber = 99;
			Factory.Save();

			AssertEquals("S5_StartDate will be changed to 27/02/2018 13:00:00", new ZDateTime(2018, 2, 27, 13, 0, 0), reportScheduleTask.S5_StartDate);
			AssertEquals("Next Run Time Local date will be set to 28/02/2018 6:50:00", new ZDateTime(2018, 2, 28, 6, 50, 0), reportScheduleTask.CalcNextRunTimeLocal);

			TestDateAttribute.Date = new DateTime(2018, 2, 27, 22, 0, 0);
			reportScheduleTask.Recurrence.DayNumber = 99;
			Factory.Save();

			AssertEquals("S5_StartDate will be changed to 27/02/2018 13:00:00", new ZDateTime(2018, 2, 27, 13, 0, 0), reportScheduleTask.S5_StartDate);
			AssertEquals("Next Run Time Local date will be set to 31/03/2018 6:50:00", new ZDateTime(2018, 2, 28, 6, 50, 0), reportScheduleTask.CalcNextRunTimeLocal);

			TestDateAttribute.Date = new DateTime(2018, 2, 10, 22, 0, 0);
			reportScheduleTask.Recurrence.WeekDayOccurrenceNumber = 1;
			reportScheduleTask.Recurrence.DayName = WeekDayList.Codes.Monday;
			Factory.Save();

			AssertEquals("S5_StartDate will be changed to 04/03/2018 13:00:00", new ZDateTime(2018, 3, 4, 13, 0, 0), reportScheduleTask.S5_StartDate);
			AssertEquals("Next Run Time Local date will be set to 05/03/2018 6:50:00", new ZDateTime(2018, 3, 5, 6, 50, 0), reportScheduleTask.CalcNextRunTimeLocal);

			reportScheduleTask.S5_StartDate = Env.Time.GetUtcFromLocalTime(new ZDateTime(2018, 2, 4).ToDateTime());
			Factory.Save();

			AssertEquals("S5_StartDate will be changed to 04/02/2018 13:00:00", new ZDateTime(2018, 2, 4, 13, 0, 0), reportScheduleTask.S5_StartDate);
			AssertEquals("Next Run Time Local date will be set to 05/03/2018 6:50:00", new ZDateTime(2018, 3, 5, 6, 50, 0), reportScheduleTask.CalcNextRunTimeLocal);

			reportScheduleTask.Recurrence.WeekDayOccurrenceNumber = 2;
			reportScheduleTask.Recurrence.DayName = WeekDayList.Codes.Sunday;
			Factory.Save();

			AssertEquals("S5_StartDate will be changed to 10/02/2018 13:00:00", new ZDateTime(2018, 2, 10, 13, 0, 0), reportScheduleTask.S5_StartDate);
			AssertEquals("Next Run Time Local date will be set to 11/03/2018 06:50:00", new ZDateTime(2018, 3, 11, 6, 50, 0), reportScheduleTask.CalcNextRunTimeLocal);

			reportScheduleTask.S5_DailyStartTime = new ZDateTime(1991, 1, 1, 10, 50, 0);
			Factory.Save();

			AssertEquals("S5_StartDate should not be changed (10/02/2018 13:00:00)", new ZDateTime(2018, 2, 10, 13, 0, 0), reportScheduleTask.S5_StartDate);
			AssertEquals("Next Run Time Local date will be set to 11/02/2018 10:50:00", new ZDateTime(2018, 2, 11, 10, 50, 0), reportScheduleTask.CalcNextRunTimeLocal);

			reportScheduleTask.Recurrence.WeekDayOccurrenceNumber = 2;
			reportScheduleTask.Recurrence.DayName = WeekDayList.Codes.Monday;
			Factory.Save();

			AssertEquals("S5_StartDate will be changed to 11/02/2018 13:00:00", new ZDateTime(2018, 2, 11, 13, 0, 0), reportScheduleTask.S5_StartDate);
			AssertEquals("Next Run Time Local date will be set to 12/02/2018 10:50:00", new ZDateTime(2018, 2, 12, 10, 50, 0), reportScheduleTask.CalcNextRunTimeLocal);
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate]
		public void TestCalculateNewYearlyScheduleDate()
		{
			TestDateAttribute.UseUNLOCO = true;
			TestDateAttribute.Date = new DateTime(2018, 2, 10, 22, 0, 0);

			var reportScheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			var printerPK = ZGuid.NewZGuid();
			Instructions.PrinterDelivery.PrintQueuePK = printerPK;
			reportScheduleTask.PopulateDefaultsFromDeliveryInstructions(Instructions);
			reportScheduleTask.Branch.GB_RL_NKHomePort = "AUSYD";
			reportScheduleTask.S5_StartDate = Env.Time.GetUtcFromLocalTime(new ZDateTime(2018, 2, 4).ToDateTime());
			reportScheduleTask.Recurrence.TaskPeriod = ScheduleRecurrenceType.Yearly;
			reportScheduleTask.Recurrence.WeekDayOccurrenceNumber = 0;
			reportScheduleTask.Recurrence.MonthNumber = 1;
			reportScheduleTask.Recurrence.DayOfMonth = 11;
			reportScheduleTask.S5_NextScheduledPrintRunTimeUtc = Env.Time.GetUtcFromLocalTime(ZDateTime.Today.AddDays(-5).ToDateTime());
			reportScheduleTask.S5_DailyStartTime = new ZDateTime(1991, 1, 1, 10, 50, 0);

			Factory.Save();

			AssertEquals("S5_StartDate will be changed to 10/01/2019 13:00:00", new ZDateTime(2019, 1, 10, 13, 0, 0), reportScheduleTask.S5_StartDate);
			AssertEquals("Next Run Time Local date will be set to 11/01/2019 10:50:00", new ZDateTime(2019, 1, 11, 10, 50, 0), reportScheduleTask.CalcNextRunTimeLocal);

			reportScheduleTask.S5_StartDate = Env.Time.GetUtcFromLocalTime(new ZDateTime(2018, 2, 4).ToDateTime());
			reportScheduleTask.Recurrence.MonthNumber = 2;
			reportScheduleTask.Recurrence.DayOfMonth = 6;
			Factory.Save();

			AssertEquals("S5_StartDate will be changed to 05/02/2018 13:00:00", new ZDateTime(2018, 2, 5, 13, 0, 0), reportScheduleTask.S5_StartDate);
			AssertEquals("Next Run Time Local date will be set to 06/02/2019 10:50:00", new ZDateTime(2019, 2, 6, 10, 50, 0), reportScheduleTask.CalcNextRunTimeLocal);

			reportScheduleTask.Recurrence.DayOfMonth = 11;
			Factory.Save();

			AssertEquals("S5_StartDate will be changed to 10/02/2018 13:00:00", new ZDateTime(2018, 2, 10, 13, 0, 0), reportScheduleTask.S5_StartDate);
			AssertEquals("Next Run Time Local date will be set to 11/02/2018 10:50:00", new ZDateTime(2018, 2, 11, 10, 50, 0), reportScheduleTask.CalcNextRunTimeLocal);

			reportScheduleTask.S5_DailyStartTime = new ZDateTime(1991, 1, 1, 6, 50, 0);
			Factory.Save();

			AssertEquals("S5_StartDate should not be changed (10/02/2018 13:00:00)", new ZDateTime(2018, 2, 10, 13, 0, 0), reportScheduleTask.S5_StartDate);
			AssertEquals("Next Run Time Local date will be set to 11/02/2019 06:50:00", new ZDateTime(2019, 2, 11, 6, 50, 0), reportScheduleTask.CalcNextRunTimeLocal);

			reportScheduleTask.Recurrence.DayOfMonth = 14;
			Factory.Save();

			AssertEquals("S5_StartDate will be changed to 13/02/2018 13:00:00", new ZDateTime(2018, 2, 13, 13, 0, 0), reportScheduleTask.S5_StartDate);
			AssertEquals("Next Run Time Local date will be set to 14/02/2018 6:50:00", new ZDateTime(2018, 2, 14, 6, 50, 0), reportScheduleTask.CalcNextRunTimeLocal);

			reportScheduleTask.S5_StartDate = Env.Time.GetUtcFromLocalTime(new ZDateTime(2018, 2, 4).ToDateTime());
			reportScheduleTask.Recurrence.WeekDayOccurrenceNumber = 1;
			reportScheduleTask.Recurrence.DayName = WeekDayList.Codes.Monday;
			reportScheduleTask.Recurrence.MonthNumber = 1;
			Factory.Save();

			AssertEquals("S5_StartDate will be changed to 06/01/2019 13:00:00", new ZDateTime(2019, 1, 6, 13, 0, 0), reportScheduleTask.S5_StartDate);
			AssertEquals("Next Run Time Local date will be set to 07/01/2019 6:50:00)", new ZDateTime(2019, 1, 7, 6, 50, 0), reportScheduleTask.CalcNextRunTimeLocal);

			reportScheduleTask.S5_StartDate = Env.Time.GetUtcFromLocalTime(new ZDateTime(2018, 2, 4).ToDateTime());
			reportScheduleTask.Recurrence.WeekDayOccurrenceNumber = 2;
			reportScheduleTask.Recurrence.DayName = WeekDayList.Codes.Sunday;
			reportScheduleTask.Recurrence.MonthNumber = 2;
			Factory.Save();

			AssertEquals("S5_StartDate will be changed to 10/02/2018 13:00:00", new ZDateTime(2018, 2, 10, 13, 0, 0), reportScheduleTask.S5_StartDate);
			AssertEquals("Next Run Time Local date will be set to 10/02/2019 6:50:00", new ZDateTime(2019, 2, 10, 6, 50, 0), reportScheduleTask.CalcNextRunTimeLocal);

			reportScheduleTask.S5_DailyStartTime = new ZDateTime(1991, 1, 1, 10, 50, 0);
			Factory.Save();

			AssertEquals("S5_StartDate should not be changed (10/02/2018 13:00:00)", new ZDateTime(2018, 2, 10, 13, 0, 0), reportScheduleTask.S5_StartDate);
			AssertEquals("Next Run Time Local date will be set to 11/02/2018 10:50:00", new ZDateTime(2018, 2, 11, 10, 50, 0), reportScheduleTask.CalcNextRunTimeLocal);

			reportScheduleTask.Recurrence.WeekDayOccurrenceNumber = 2;
			reportScheduleTask.Recurrence.DayName = WeekDayList.Codes.Monday;
			Factory.Save();

			AssertEquals("S5_StartDate will be changed to 11/02/2018 13:00:00", new ZDateTime(2018, 2, 11, 13, 0, 0), reportScheduleTask.S5_StartDate);
			AssertEquals("Next Run Time Local date will be set to 12/02/2018 10:50:00", new ZDateTime(2018, 2, 12, 10, 50, 0), reportScheduleTask.CalcNextRunTimeLocal);
		}

		[TestDate]
		public void TestCalculateNewAccountingPeriodScheduleDate()
		{
			TestDateAttribute.UseUNLOCO = true;
			TestDateAttribute.Date = new DateTime(2018, 2, 10, 22, 0, 0);

			CreateAccPeriodTestData();

			var reportScheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			var printerPK = ZGuid.NewZGuid();
			Instructions.PrinterDelivery.PrintQueuePK = printerPK;
			reportScheduleTask.PopulateDefaultsFromDeliveryInstructions(Instructions);
			reportScheduleTask.Branch.GB_RL_NKHomePort = "AUSYD";
			reportScheduleTask.S5_StartDate = Env.Time.GetUtcFromLocalTime(new ZDateTime(2018, 2, 4).ToDateTime());
			reportScheduleTask.Recurrence.TaskPeriod = ScheduleRecurrenceType.AccountingPeriod;
			reportScheduleTask.Recurrence.WeekDayOccurrenceNumber = 0;
			reportScheduleTask.Recurrence.DayOfAccountingPeriod = 37;
			reportScheduleTask.S5_NextScheduledPrintRunTimeUtc = Env.Time.GetUtcFromLocalTime(ZDateTime.Today.AddDays(-5).ToDateTime());
			reportScheduleTask.S5_DailyStartTime = new ZDateTime(1991, 1, 1, 10, 50, 0);

			Factory.Save();

			AssertEquals("S5_StartDate will be changed to 05/02/2018 13:00:00", new ZDateTime(2018, 2, 5, 13, 0, 0), reportScheduleTask.S5_StartDate);
			AssertEquals("Next Run Time Local date will be set to 07/05/2018 10:50:00", new ZDateTime(2018, 5, 7, 10, 50, 0), reportScheduleTask.CalcNextRunTimeLocal);

			reportScheduleTask.Recurrence.DayOfAccountingPeriod = 42;
			Factory.Save();

			AssertEquals("S5_StartDate will be changed to 10/02/2018 13:00:00", new ZDateTime(2018, 2, 10, 13, 0, 0), reportScheduleTask.S5_StartDate);
			AssertEquals("Next Run Time Local date will be set to 11/02/2018 10:50:00", new ZDateTime(2018, 2, 11, 10, 50, 0), reportScheduleTask.CalcNextRunTimeLocal);

			reportScheduleTask.S5_DailyStartTime = new ZDateTime(1991, 1, 1, 6, 50, 0);
			Factory.Save();

			AssertEquals("S5_StartDate should not be changed (10/02/2018 13:00:00)", new ZDateTime(2018, 2, 10, 13, 0, 0), reportScheduleTask.S5_StartDate);
			AssertEquals("Next Run Time Local date will be set to 12/05/2018 06:50:00", new ZDateTime(2018, 5, 12, 06, 50, 0), reportScheduleTask.CalcNextRunTimeLocal);

			reportScheduleTask.Recurrence.AccountingLastDay = true;
			Factory.Save();

			AssertEquals("S5_StartDate will be changed to 30/03/2018 13:00:00", new ZDateTime(2018, 3, 30, 13, 0, 0), reportScheduleTask.S5_StartDate);
			AssertEquals("Next Run Time Local date will be set to 31/03/2018 06:50:00", new ZDateTime(2018, 3, 31, 06, 50, 0), reportScheduleTask.CalcNextRunTimeLocal);

			TestDateAttribute.Date = new DateTime(2018, 3, 30, 22, 0, 0);
			reportScheduleTask.S5_DailyStartTime = new ZDateTime(1991, 1, 1, 10, 50, 0);
			Factory.Save();

			AssertEquals("S5_StartDate should not be changed (30/03/2018 13:00:00)", new ZDateTime(2018, 3, 30, 13, 0, 0), reportScheduleTask.S5_StartDate);
			AssertEquals("Next Run Time Local date will be set to 31/03/2018 10:50:00", new ZDateTime(2018, 3, 31, 10, 50, 0), reportScheduleTask.CalcNextRunTimeLocal);

			reportScheduleTask.S5_DailyStartTime = new ZDateTime(1991, 1, 1, 6, 50, 0);
			Factory.Save();

			AssertEquals("S5_StartDate should not be changed (30/03/2018 13:00:00)", new ZDateTime(2018, 3, 30, 13, 0, 0), reportScheduleTask.S5_StartDate);
			AssertEquals("Next Run Time Local date will be set to 30/06/2018 06:50:00", new ZDateTime(2018, 6, 30, 6, 50, 0), reportScheduleTask.CalcNextRunTimeLocal);

			TestDateAttribute.Date = new DateTime(2018, 1, 13, 22, 0, 0);
			reportScheduleTask.Recurrence.AccountingLastDay = false;
			reportScheduleTask.S5_StartDate = Env.Time.GetUtcFromLocalTime(new ZDateTime(2018, 1, 8).ToDateTime());
			reportScheduleTask.Recurrence.WeekDayOccurrenceNumber = 1;
			reportScheduleTask.Recurrence.DayName = WeekDayList.Codes.Sunday;
			Factory.Save();

			AssertEquals("S5_StartDate will be changed to 31/03/2018 13:00:00", new ZDateTime(2018, 3, 31, 13, 0, 0), reportScheduleTask.S5_StartDate);
			AssertEquals("Next Run Time Local date will be set to 01/04/2018 06:50:00", new ZDateTime(2018, 4, 1, 6, 50, 0), reportScheduleTask.CalcNextRunTimeLocal);

			reportScheduleTask.S5_StartDate = Env.Time.GetUtcFromLocalTime(new ZDateTime(2018, 1, 8).ToDateTime());
			reportScheduleTask.Recurrence.WeekDayOccurrenceNumber = 2;
			reportScheduleTask.Recurrence.DayName = WeekDayList.Codes.Sunday;
			reportScheduleTask.S5_DailyStartTime = new ZDateTime(1991, 1, 1, 10, 50, 0);
			Factory.Save();

			AssertEquals("S5_StartDate will be changed to 13/01/2018 13:00:00", new ZDateTime(2018, 1, 13, 13, 0, 0), reportScheduleTask.S5_StartDate);
			AssertEquals("Next Run Time Local date will be set to 14/01/2018 10:50:00", new ZDateTime(2018, 1, 14, 10, 50, 0), reportScheduleTask.CalcNextRunTimeLocal);

			reportScheduleTask.S5_StartDate = Env.Time.GetUtcFromLocalTime(new ZDateTime(2018, 1, 8).ToDateTime());
			reportScheduleTask.Recurrence.WeekDayOccurrenceNumber = 2;
			reportScheduleTask.Recurrence.DayName = WeekDayList.Codes.Sunday;
			reportScheduleTask.S5_DailyStartTime = new ZDateTime(1991, 1, 1, 06, 50, 0);
			Factory.Save();

			AssertEquals("S5_StartDate will be changed to 13/01/2018 13:00:00", new ZDateTime(2018, 1, 13, 13, 0, 0), reportScheduleTask.S5_StartDate);
			AssertEquals("Next Run Time Local date will be set to 08/04/2018 06:50:00", new ZDateTime(2018, 4, 8, 6, 50, 0), reportScheduleTask.CalcNextRunTimeLocal);
		}

		public void TestUserFK()
		{
			ReportScheduleTask scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			AssertEquals(Env.CurrentUser.PK, scheduleTask.UserFK);

			GlbStaff staff = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_LoginName, SQLComparisonOperator.NotEqual, Env.CurrentUser.LoginName));
			AssertNotNull("Precondition: There must be a staff", staff);

			scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			var report = new ReportSerializationInfo(staff.GS_LoginName.ToString(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			var serializedReport = JsonConverterHelper.Serialize(report);
			scheduleTask.S5_ScheduleState = Encoding.UTF8.GetBytes(serializedReport);
			scheduleTask.S5_GS_NKPrintUser = staff.GS_Code;

			AssertEquals(staff.PK, scheduleTask.UserFK);

			ZGuid anyGuid = ZGuid.NewZGuid();
			scheduleTask.UserFK = anyGuid;
			AssertEquals(anyGuid, scheduleTask.UserFK);
			AssertNotEquals(staff.GS_Code, scheduleTask.S5_GS_NKPrintUser);
			AssertEquals(GlbStaff.CurrentUser.GS_Code, scheduleTask.S5_GS_NKPrintUser);

			scheduleTask.UserFK = staff.PK;
			AssertEquals(staff.GS_Code, scheduleTask.S5_GS_NKPrintUser);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestUserFKIsStoredInReportInfo()
		{
			var staff = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_LoginName, SQLComparisonOperator.NotEqual, Env.CurrentUser.LoginName));
			AssertNotNull("Precondition: There must be a staff", staff);

			ScheduleTask.UserFK = staff.PK;

			var menuItem = Factory.NewWithValidTestData<ReportCommand>();
			menuItem.SU_BusinessContext = "RepShipment";
			menuItem.SU_ContactType = "SAL";
			Factory.Save();

			var excelTemplate = new ExcelTemplateForUnitTesting("TextFilter.xls", TestFilesSubFolder.ReportTestFiles);
			using (var pack = new DocumentPack(menuItem))
			using (var report = new Report(pack, excelTemplate))
			{
				report.PrepareForRender();
				var textField = (TextField)report.FilterCollection[1];
				textField.ValueAsStringForSerialisation = "My Test Field Value";
				AssertEquals("Should read filters", 2, report.FilterCollection.Count);

				var instructions = new DeliveryInstructions();
				instructions.CoverNote = "Barf";
				instructions.IncludeCoverNote = true;

				ScheduleTask.S5_ScheduleState = ScheduleTask.SerializeForTesting(instructions, report);
				AssertEquals(staff.GS_LoginName, ScheduleTask.CreateReportFromTask().User);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestJsonConverter()
		{
			var staff = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_LoginName, SQLComparisonOperator.NotEqual, Env.CurrentUser.LoginName));
			AssertNotNull("Precondition: There must be a staff", staff);

			ScheduleTask.UserFK = staff.PK;

			var menuItem = Factory.NewWithValidTestData<ReportCommand>();
			menuItem.SU_BusinessContext = "RepShipment";
			menuItem.SU_ContactType = "SAL";
			Factory.Save();

			var excelTemplate = new ExcelTemplateForUnitTesting("TextFilter.xls", TestFilesSubFolder.ReportTestFiles);
			using (var pack = new DocumentPack(menuItem))
			using (var report = new Report(pack, excelTemplate))
			{
				report.PrepareForRender();
				var textField = (TextField)report.FilterCollection[1];
				textField.ValueAsStringForSerialisation = "My Test Field Value";
				AssertEquals("Should read filters", 2, report.FilterCollection.Count);

				var instructions = new DeliveryInstructions();
				instructions.CoverNote = "Barf";
				instructions.IncludeCoverNote = true;

				ScheduleTask.S5_ScheduleState = ScheduleTask.SerializeForTesting(instructions, report);
				AssertEquals(staff.GS_LoginName, ScheduleTask.CreateReportFromTask().User);
			}
		}

		public void TestSetPrintUserUpdatesRecipients()
		{
			var orgSender = Factory.NewWithValidTestData<GlbStaff>();
			orgSender.GS_FullName = "John Cena";
			orgSender.GS_Code = "JCE";
			orgSender.GS_EmailAddress = "youcant@cme.com";
			var newSender = Factory.NewWithValidTestData<GlbStaff>();
			newSender.GS_FullName = "Fred Durst";
			newSender.GS_Code = "FDU";
			newSender.GS_EmailAddress = "break@stuff.com";
			var nullEmailSender = Factory.NewWithValidTestData<GlbStaff>();
			nullEmailSender.GS_FullName = "Bilbo Baggins";
			nullEmailSender.GS_Code = "BBA";
			nullEmailSender.GS_EmailAddress = null;

			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var someContact = CreateContact(organisation, "someContact", "someContact@contact.com", "someFax");
			var someGroup = Factory.NewWithValidTestData<GlbGroup>();
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			Factory.NewWithValidTestData<GlbStaff>();

			var recip1 = CreateRecipient(ScheduledReportDeliveryRecipientConstants.RecipientType.Staff, Core.Constants.ContactNotifyModes.Email, staff1.GS_Code);
			var recip2 = CreateRecipient(ScheduledReportDeliveryRecipientConstants.RecipientType.Contact, Core.Constants.ContactNotifyModes.Email, someContact.PK);
			var recip3 = CreateRecipient(ScheduledReportDeliveryRecipientConstants.RecipientType.Group, Core.Constants.ContactNotifyModes.Email, someGroup.PK);
			var recip4 = CreateRecipient(ScheduledReportDeliveryRecipientConstants.RecipientType.Group, Core.Constants.ContactNotifyModes.Print, someGroup.PK);
			recip4.S6_EmailFromAddress = "icant@change.com";

			ScheduleTask.Recipients.Add(recip1);
			ScheduleTask.Recipients.Add(recip2);
			ScheduleTask.Recipients.Add(recip3);
			ScheduleTask.Recipients.Add(recip4);

			ScheduleTask.UserFK = orgSender.PK;
			AssertEquals("Print User email hasn't been set", orgSender.GS_EmailAddress, ScheduleTask.PrintUser.GS_EmailAddress);
			AssertEquals("From Email not the same as Print User's email", ScheduleTask.PrintUser.GS_EmailAddress, recip1.S6_EmailFromAddress);
			AssertEquals("From Email not the same as Print User's email", ScheduleTask.PrintUser.GS_EmailAddress, recip2.S6_EmailFromAddress);
			AssertEquals("From Email not the same as Print User's email", ScheduleTask.PrintUser.GS_EmailAddress, recip3.S6_EmailFromAddress);
			AssertEquals("From Email for non-email recipient shouldn't change", "icant@change.com", recip4.S6_EmailFromAddress);

			ScheduleTask.UserFK = newSender.PK;
			AssertEquals("Print User email hasn't updated", newSender.GS_EmailAddress, ScheduleTask.PrintUser.GS_EmailAddress);
			AssertEquals("From Email not the same as Print User's email", ScheduleTask.PrintUser.GS_EmailAddress, recip1.S6_EmailFromAddress);
			AssertEquals("From Email not the same as Print User's email", ScheduleTask.PrintUser.GS_EmailAddress, recip2.S6_EmailFromAddress);
			AssertEquals("From Email not the same as Print User's email", ScheduleTask.PrintUser.GS_EmailAddress, recip3.S6_EmailFromAddress);
			AssertEquals("From Email for non-email recipient shouldn't change", "icant@change.com", recip4.S6_EmailFromAddress);

			ScheduleTask.UserFK = nullEmailSender.PK;
			AssertNullOrEmpty("Print User email hasn't updated", ScheduleTask.PrintUser.GS_EmailAddress);
			AssertNullOrEmpty("From Email not the same as Print User's email", recip1.S6_EmailFromAddress);
			AssertNullOrEmpty("From Email not the same as Print User's email", recip2.S6_EmailFromAddress);
			AssertNullOrEmpty("From Email not the same as Print User's email", recip3.S6_EmailFromAddress);
			AssertEquals("From Email for non-email recipient shouldn't change", "icant@change.com", recip4.S6_EmailFromAddress);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAddColumnPreserveCurrencyFormatInformation()
		{
			SetUpPostMasterGroup();

			var reportCommand = Factory.NewWithValidTestData<ReportCommand>();
			reportCommand.SU_BusinessContext = "Test";
			reportCommand.SU_ContactType = ContactType.All.Code;
			reportCommand.Factory.Save();

			var excelTemplate = new ExcelTemplateForUnitTesting("GL Transaction Report.xls", TestFilesSubFolder.ReportTestFiles);
			var template = Factory.New<StmTemplate>();
			template.SO_Name = "GL Transaction Report";
			template.SO_Template = excelTemplate.GetAsByteArray();

			var pivot = Factory.New<StmMenuTemplatePivot>();
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;
			Factory.Save();

			ZGuid taskGuid;
			using (var pack = new DocumentPack(reportCommand))
			{
				var task = Factory.NewWithValidTestData<ReportScheduleTask>();
				var report = (Report)pack[0];
				report.SetScheduleTask(task);
				report.PrepareForRender();

				var testPeriodRangeField = new AccountingPeriodsRangeField(Factory);
				TestCaseHelper.ClearTable(AccPeriodManagement.Schema.TableName);
				var periodTestHelper = new AccountingPeriodTestHelper();
				periodTestHelper.PostPeriodsForEntireYear(2003);
				periodTestHelper.PostPeriodsForEntireYear(2002);

				var periodRange = report.FilterCollection["Period Range"] as AccountingPeriodsRangeField;
				periodRange.PeriodFrom = 200201;
				periodRange.PeriodTo = 200212;
				var startGlAccount = report.FilterCollection["Start GL Account"] as LookupField;
				startGlAccount.Value = Guid.NewGuid();
				var endGlAccount = report.FilterCollection["End GL Account"] as LookupField;
				endGlAccount.Value = Guid.NewGuid();

				var instructions = new DeliveryInstructions(pack);
				instructions.Recipients.RemoveAll();
				var contact = instructions.Recipients.AddNew();
				contact.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Email;
				contact.AttachmentType = OrgConstants.AttachmentType.PDF;
				contact.OrgHeaderPK = Organization.PK;
				contact.Name = "charlie";
				contact.Email = "test@test.com";

				task.PopulateDefaultsFromDeliveryInstructions(instructions);
				Factory.Save();

				taskGuid = task.PK;
			}

			try
			{
				PrintTaskForTesting.DocumentsRun = new List<List<BusinessObject>>();
				Report.RenderedWorkSheetsForTesting = new List<ExcelWorkSheet>();
				using (var task = Factory.Load<ReportScheduleTaskForTesting>(taskGuid))
				{
					task.Run();
					AssertEquals("task.CreatedPrintTasksForRun.Count", 1, task.CreatedPrintTasksForRun.Count);
					Assert("Serialized report is deserialized before report analysing", task.IsSerializedReportDesrializedBeforeRunning);

					var report = (Report)PrintTaskForTesting.DocumentsRun[0][0];
					report.PrepareForRender();

					var headings = report.ColumnHeadingManager.CurrentConfiguration.Worksheets[report.WorkSheetCurrentlyBeingProcessed.SheetName].ColumnHeadings;
					headings["Transaction Line Charge Code"].CurrentPosition = 0;
					headings["Charge Code Description"].CurrentPosition = 1;
					headings["Charge Code Description"].Hidden = false;
					headings["Post Period"].CurrentPosition = 2;
					headings["Debit Amount"].CurrentPosition = 3;
					headings["Credit Amount"].CurrentPosition = 4;

					report.XlInterface.Xls.ActiveSheet = 1;
					using (var reportOutputStream = new MemoryStream())
					{
						report.Save(reportOutputStream);

						using (var excelInterface = new ExcelInterface())
						{
							excelInterface.LoadExcelFile(reportOutputStream);
							var workSheet = excelInterface.WorkSheets[0];

							var excelFile = workSheet.ParentExcelInterface.Xls;
							excelFile.ActiveSheet = 1;

							CombineAssertions(delegate
							{
								AssertColumnContents(excelFile, 4, "200801", "");
								AssertColumnContents(excelFile, 5, "16.00", "#,##0.00");
								AssertColumnContents(excelFile, 6, "17.00", "#,##0.00");
							});
						}
					}
				}
			}
			finally
			{
				PrintTaskForTesting.DocumentsRun = null;
				Report.RenderedWorkSheetsForTesting = null;
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestStmReportRunPart2()
		{
			SetUpPostMasterGroup();

			var reportCommand = Factory.NewWithValidTestData<ReportCommand>();
			reportCommand.SU_BusinessContext = "Test";
			reportCommand.SU_ContactType = ContactType.All.Code;
			reportCommand.Factory.Save();

			var excelTemplate = new ExcelTemplateForUnitTesting("GL Transaction Report.xls", TestFilesSubFolder.ReportTestFiles);
			var template = Factory.New<StmTemplate>();
			template.SO_Name = "GL Transaction Report";
			template.SO_Template = excelTemplate.GetAsByteArray();

			var pivot = Factory.New<StmMenuTemplatePivot>();
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;
			Factory.Save();

			ZGuid taskGuid;
			using (var pack = new DocumentPack(reportCommand))
			{
				var task = Factory.NewWithValidTestData<ReportScheduleTask>();
				var report = (Report)pack[0];
				report.SetScheduleTask(task);
				report.PrepareForRender();

				var testPeriodRangeField = new AccountingPeriodsRangeField(Factory);
				TestCaseHelper.ClearTable(AccPeriodManagement.Schema.TableName);
				var periodTestHelper = new AccountingPeriodTestHelper();
				periodTestHelper.PostPeriodsForEntireYear(2003);
				periodTestHelper.PostPeriodsForEntireYear(2002);

				var periodRange = report.FilterCollection["Period Range"] as AccountingPeriodsRangeField;
				periodRange.PeriodFrom = 200201;
				periodRange.PeriodTo = 200212;
				var startGlAccount = report.FilterCollection["Start GL Account"] as LookupField;
				startGlAccount.Value = Guid.NewGuid();
				var endGlAccount = report.FilterCollection["End GL Account"] as LookupField;
				endGlAccount.Value = Guid.NewGuid();

				var instructions = new DeliveryInstructions(pack);
				instructions.Recipients.RemoveAll();

				var contact = instructions.Recipients.AddNew();
				contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
				contact.AttachmentType = OrgConstants.AttachmentType.PDF;
				contact.OrgHeaderPK = Organization.PK;
				contact.Name = "charlie";
				contact.Email = "test@test.com";

				contact = instructions.Recipients.AddNew();
				contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
				contact.AttachmentType = OrgConstants.AttachmentType.PDF;
				contact.OrgHeaderPK = Organization.PK;
				contact.Name = "Jerry";
				contact.Email = "jerry@test.com";

				task.PopulateDefaultsFromDeliveryInstructions(instructions);
				Factory.Save();

				taskGuid = task.PK;
			}

			try
			{
				PrintTaskForTesting.DocumentsRun = new List<List<BusinessObject>>();
				Report.RenderedWorkSheetsForTesting = new List<ExcelWorkSheet>();
				using (var task = Factory.Load<ReportScheduleTaskForTesting>(taskGuid))
				{
					ReportScheduleTask.ShouldChangeReportDbForTesting.Value = true;
					task.StmReportRun = Factory.New<StmReportRun>();
					task.StmReportRun.RRI_S5_Schedule = task.PK;
					task.StmReportRun.RRI_StartTimeUtc = ZDateTime.UtcNow;
					DataRegistry.Instance.ReportStatisticsLogExecutionPlan = true;
					task.Run();
					Factory.Save();

					var stmReportRunReloaded = new BusinessObjectFactory().Load<StmReportRun>(task.StmReportRun.PK);

					AssertEquals(Core.Constants.StmReportRunState.Finished, stmReportRunReloaded.RRI_Status);
					Assert(stmReportRunReloaded.RRI_ReportSizeBytes > 1000);
					AssertEquals(reportCommand.SU_MenuName, stmReportRunReloaded.RRI_ReportName);
					Assert(stmReportRunReloaded.RRI_QueryText.Contains("SELECT 1 as InvoiceDate, 2 as PostDate, 3 as Branch, 4 as Department, 5 as Ledger, 6 as TransactionType, 7 as TransactionNum, 8 as SecondRef, 9 as TransactionDesc, 10 as Account, 11 as Job, 12 as ChargeCode, 13 as ChargeCodeDescription, 14 as LocalLanguageChargeCodeDescription, 200801 as Period, 16 as Debit, 17 as Credit, 18 as Amount, 19 as GLAccount, 20 as ClosingPeriodDate, 21 as GLAccountDesc, 22 as OpeningBalance, 23 as OpeningPeriodDate option (recompile)"));
					AssertEquals("@p32", SqlParameterNameGenerator.Next());
					AssertNotEquals("", stmReportRunReloaded.RRI_SQLServer);
					Assert(stmReportRunReloaded.RRI_RowsReturned > 0);
					Assert(stmReportRunReloaded.RRI_ClientCPUDurationMilliseconds > 0);
					Assert(stmReportRunReloaded.RRI_ClientRunDurationMilliseconds > 0);
					Assert(stmReportRunReloaded.RRI_SQLCPUDurationMilliseconds >= 0); //can't check > 0 because sometimes the report takes 0ms on the server due to its simplicity.
					AssertEquals("SQLCPUTime is only allowed to be called twice", 2, task.CallCountForSQLCPUTime);
					Assert(stmReportRunReloaded.RRI_SQLRunDurationMilliseconds >= 0);
					Assert(stmReportRunReloaded.RRI_TotalRunDurationMilliseconds >= (stmReportRunReloaded.RRI_ClientRunDurationMilliseconds + stmReportRunReloaded.RRI_SQLRunDurationMilliseconds));
					AssertContains("<SHOWPLANXML ", stmReportRunReloaded.RRI_ExecutionPlanText.ToUpperInvariant());
					AssertContains("</SHOWPLANXML>", stmReportRunReloaded.RRI_ExecutionPlanText.ToUpperInvariant());
				}
			}
			finally
			{
				PrintTaskForTesting.DocumentsRun = null;
				Report.RenderedWorkSheetsForTesting = null;
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestStmReportRunPart2_NoScheduleState()
		{
			SetUpPostMasterGroup();

			var reportCommand = Factory.NewWithValidTestData<ReportCommand>();
			reportCommand.SU_BusinessContext = "Test";
			reportCommand.SU_ContactType = ContactType.All.Code;
			reportCommand.Factory.Save();

			var excelTemplate = new ExcelTemplateForUnitTesting("GL Transaction Report.xls", TestFilesSubFolder.ReportTestFiles);
			var template = Factory.New<StmTemplate>();
			template.SO_Name = "GL Transaction Report";
			template.SO_Template = excelTemplate.GetAsByteArray();

			var pivot = Factory.New<StmMenuTemplatePivot>();
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;
			Factory.Save();

			ZGuid taskGuid;
			using (var pack = new DocumentPack(reportCommand))
			{
				var task = Factory.NewWithValidTestData<ReportScheduleTask>();
				var report = (Report)pack[0];
				report.SetScheduleTask(task);

				var instructions = new DeliveryInstructions(pack);
				instructions.Recipients.RemoveAll();
				var contact = instructions.Recipients.AddNew();
				contact.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Email;
				contact.AttachmentType = OrgConstants.AttachmentType.PDF;
				contact.OrgHeaderPK = Organization.PK;
				contact.Name = "charlie";
				contact.Email = "test@test.com";

				task.PopulateDefaultsFromDeliveryInstructions(instructions);
				Factory.Save();

				taskGuid = task.PK;
			}

			try
			{
				PrintTaskForTesting.DocumentsRun = new List<List<BusinessObject>>();
				Report.RenderedWorkSheetsForTesting = new List<ExcelWorkSheet>();
				using (var task = Factory.Load<ReportScheduleTaskForTesting>(taskGuid))
				{
					task.StmReportRun = Factory.New<StmReportRun>();
					task.StmReportRun.RRI_S5_Schedule = task.PK;
					task.StmReportRun.RRI_StartTimeUtc = ZDateTime.UtcNow;
					task.S5_ScheduleState = ZBlob.Empty;
					task.Run();
					Factory.Save();

					var stmReportRunReloaded = new BusinessObjectFactory().Load<StmReportRun>(task.StmReportRun.PK);

					AssertNotEquals(Enterprise.Core.Constants.StmReportRunState.Finished, stmReportRunReloaded.RRI_Status);
					AssertEquals(reportCommand.SU_MenuName, stmReportRunReloaded.RRI_ReportName);
					AssertEquals(0L, stmReportRunReloaded.RRI_ReportSizeBytes);
					AssertEquals(ZString.Empty, stmReportRunReloaded.RRI_QueryText);
					AssertEquals(0, stmReportRunReloaded.RRI_RowsReturned);
					AssertEquals(ZString.Empty, stmReportRunReloaded.RRI_ExecutionPlanText);
					AssertEquals(0, stmReportRunReloaded.RRI_TotalRunDurationMilliseconds);
					AssertEquals(0, stmReportRunReloaded.RRI_ClientCPUDurationMilliseconds);
					AssertEquals(0, stmReportRunReloaded.RRI_ClientRunDurationMilliseconds);
					AssertEquals(0, stmReportRunReloaded.RRI_SQLCPUDurationMilliseconds);
					AssertEquals(0, stmReportRunReloaded.RRI_SQLRunDurationMilliseconds);
				}
			}
			finally
			{
				PrintTaskForTesting.DocumentsRun = null;
				Report.RenderedWorkSheetsForTesting = null;
			}
		}

		public void TestScheduledReportShouldHaveQueryText()
		{
			using (DataRegistry.Instance.RawRegistry.ReportStatisticsLogExecutionPlan.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var report = GetReportForScheduleReport())
			{
				var scheduleTask1 = Factory.NewWithValidTestData<ReportScheduleTask>();
				scheduleTask1.PopulateDefaultsFromDeliveryInstructions(GetDeliveryInstructionsForScheduleReport(report.Parent));
				scheduleTask1.S5_IsPrivate = true;
				scheduleTask1.StmReportRun = Factory.New<StmReportRun>();
				scheduleTask1.StmReportRun.RRI_S5_Schedule = scheduleTask1.PK;
				scheduleTask1.StmReportRun.RRI_StartTimeUtc = ZDateTime.UtcNow;

				var scheduleTask2 = Factory.NewWithValidTestData<ReportScheduleTask>();
				scheduleTask2.PopulateDefaultsFromDeliveryInstructions(Instructions);
				scheduleTask2.S5_IsPrivate = true;
				scheduleTask2.StmReportRun = Factory.New<StmReportRun>();
				scheduleTask2.StmReportRun.RRI_S5_Schedule = scheduleTask2.PK;
				scheduleTask2.StmReportRun.RRI_StartTimeUtc = ZDateTime.UtcNow;

				Factory.Save();

				scheduleTask1.RunSafe(new NotificationBuffer());
				var stmReportRun1 = Factory.Load<StmReportRun>(scheduleTask1.StmReportRun.PK);
				AssertNotNull(stmReportRun1);
				AssertContains("--UDF PARAMETERS", stmReportRun1.RRI_QueryText.ToUpper());
				AssertContains("<SHOWPLANXML ", stmReportRun1.RRI_ExecutionPlanText.ToUpper());
				AssertContains(Core.Constants.StmReportRunState.Finished, stmReportRun1.RRI_Status);

				var reportCommand = Factory.Load<ReportCommand>(scheduleTask2.S5_ParentID);
				using var docPack = new DocumentPack(reportCommand);
				AssertEquals("Pre-Condition: report command has no documents to deliver.", 0, docPack.Count);

				scheduleTask2.RunSafe(new NotificationBuffer());
				var stmReportRun2 = Factory.Load<StmReportRun>(scheduleTask2.StmReportRun.PK);
				AssertContains(Core.Constants.StmReportRunState.Finished, stmReportRun2.RRI_Status);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestScheduledReportWithOptionalTemplates()
		{
			ZGuid taskGuid;

			var reportCommand = Factory.NewWithValidTestData<ReportCommand>();
			reportCommand.SU_BusinessContext = "Test";
			reportCommand.SU_ContactType = ContactType.All.Code;
			reportCommand.Factory.Save();

			var excelTemplate = new ExcelTemplateForUnitTesting("MultipleTemplatesWithOptionalTemplates.xls", TestFilesSubFolder.ReportTestFiles);
			var template = Factory.New<StmTemplate>();
			template.SO_Name = "MultipleTemplatesWithOptionalTemplates";
			template.SO_Template = excelTemplate.GetAsByteArray();

			var pivot = Factory.New<StmMenuTemplatePivot>();
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;

			Factory.Save();

			using (var pack = new DocumentPack(reportCommand))
			{
				var task = Factory.NewWithValidTestData<ReportScheduleTask>();
				var report = (Report)pack[0];
				report.SetScheduleTask(task);
				report.PrepareForRender();
				report.OptionalTemplateSheetCollection["Sheet1"].Selected = false;

				var selectOptionalTemplateSheet = report.OptionalTemplateSheetCollection["Sheet3"];
				selectOptionalTemplateSheet.Selected = true;
				selectOptionalTemplateSheet.DisplayName = "New Display Name";
				selectOptionalTemplateSheet.FieldName = "New Field Name";
				Factory.Save();

				AssertEquals("Count", 2, report.OptionalTemplateSheetCollection.Count);
				AssertEquals("Collection Should contain sheet1", true, report.OptionalTemplateSheetCollection.Contains("Sheet1"));
				Assert("report.TemplateSheets.Contains(\"Sheet2\")", report.TemplateSheets.Contains("Sheet2"));
				AssertEquals("Collection Should contain sheet3", true, report.OptionalTemplateSheetCollection.Contains("Sheet3"));
				AssertEquals("Selected Sheet", true, report.OptionalTemplateSheetCollection["Sheet3"].Selected);
				AssertEquals("Sheet 3 Display Name", "New Display Name", report.OptionalTemplateSheetCollection["Sheet3"].DisplayName);
				AssertEquals("Sheet 3 Field Name", "New Field Name", report.OptionalTemplateSheetCollection["Sheet3"].FieldName);

				var instructions = new DeliveryInstructions(pack);
				instructions.Recipients.RemoveAll();

				var contact = instructions.Recipients.AddNew();
				contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
				contact.AttachmentType = "PDF";
				contact.OrgHeaderPK = Organization.PK;
				contact.Name = "Blah Blah";
				contact.Email = "blah@blah.com";

				task.PopulateDefaultsFromDeliveryInstructions(instructions);
				Factory.Save();
				taskGuid = task.PK;
			}

			try
			{
				PrintTaskForTesting.DocumentsRun = new List<List<BusinessObject>>();
				Report.RenderedWorkSheetsForTesting = new List<ExcelWorkSheet>();
				using (var task = Factory.Load<ReportScheduleTaskForTesting>(taskGuid))
				{
					task.Run();
					Assert("Serialized report is deserialized before report analysing", task.IsSerializedReportDesrializedBeforeRunning);
					AssertEquals("ReportScheduleTask.DeliveryInstructionsRunForTesting.Count", 1, task.CreatedPrintTasksForRun.Count);

					var report = (Report)PrintTaskForTesting.DocumentsRun[0][0];

					AssertEquals("Count", 2, report.OptionalTemplateSheetCollection.Count);
					AssertEquals("Collection Should contain sheet1", true, report.OptionalTemplateSheetCollection.Contains("Sheet1"));
					Assert("report.TemplateSheets.Contains(\"Sheet2\")", report.TemplateSheets.Contains("Sheet2"));
					AssertEquals("Collection Should contain sheet3", true, report.OptionalTemplateSheetCollection.Contains("Sheet3"));
					AssertEquals("Selected Sheet", true, report.OptionalTemplateSheetCollection["Sheet3"].Selected);
					AssertEquals("Sheet 3 Display Name", "New Display Name", report.OptionalTemplateSheetCollection["Sheet3"].DisplayName);
					AssertEquals("Sheet 3 Field Name", "New Field Name", report.OptionalTemplateSheetCollection["Sheet3"].FieldName);

					AssertEquals("Report.RenderedWorkSheets.Count", 2, Report.RenderedWorkSheetsForTesting.Count);
					AssertEquals("Report.RenderedWorkSheets[0].SheetName", "Sheet2", Report.RenderedWorkSheetsForTesting[0].SheetName);
					AssertEquals("Report.RenderedWorkSheets[1].SheetName", "Sheet3", Report.RenderedWorkSheetsForTesting[1].SheetName);
				}
			}
			finally
			{
				PrintTaskForTesting.DocumentsRun = null;
				Report.RenderedWorkSheetsForTesting = null;
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSerialization()
		{
			var menuItem = Factory.NewWithValidTestData<ReportCommand>();
			menuItem.SU_BusinessContext = "RepShipment";
			menuItem.SU_ContactType = "SAL";
			Factory.Save();

			var excelTemplate = new ExcelTemplateForUnitTesting("TextFilter.xls", TestFilesSubFolder.ReportTestFiles);
			using (var pack = new DocumentPack(menuItem))
			using (var report = new Report(pack, excelTemplate))
			{
				report.PrepareForRender();
				var textField = (TextField)report.FilterCollection[1];
				textField.ValueAsStringForSerialisation = "My Test Field Value";
				AssertEquals("Should read filters", 2, report.FilterCollection.Count);

				pack.Language = Core.SharedConstants.Languages.ChineseSimplified;
				AssertEquals("Report language", Core.SharedConstants.Languages.ChineseSimplified, report.Language);

				var instructions = new DeliveryInstructions();
				instructions.CoverNote = "Barf";
				instructions.IncludeCoverNote = true;

				ScheduleTask.S5_ScheduleState = ScheduleTask.SerializeForTesting(instructions, report);
				var deserializedValue = ScheduleTask.CreateReportFromTask();

				AssertEquals("Instructions.CoverNote", "Barf", deserializedValue.Instructions.CoverNote);
				AssertEquals("Instructions.IncludeCoverNote", true, deserializedValue.Instructions.IncludeCoverNote);
				AssertEquals("Language", Core.SharedConstants.Languages.ChineseSimplified, deserializedValue.Language);

				using (var deserializedReport = deserializedValue.Report)
				{
					AssertEquals("Report.FilterCollection.Count", 1, deserializedReport.FilterCollection.Count);
					var deserializedTextFilter = (TextField)deserializedReport.FilterCollection[0];
					AssertEquals("Report.FilterCollection[0].ValueAsString", "My Test Field Value", deserializedTextFilter.ValueAsStringForSerialisation);
					AssertEquals("Report.FilterCollection[0].FieldName", "XX_Field", deserializedTextFilter.FieldName);
				}
			}
		}

		public void TestCreateReportFromTaskSetsScheduleTask()
		{
			ReportScheduleTask scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.PopulateDefaultsFromDeliveryInstructions(Instructions);
			ReportSerializationInfo reportInfo = scheduleTask.CreateReportFromTask();
			AssertEquals("CreateReportFromTask().ScheduleTask", scheduleTask, reportInfo.Report.ScheduleTask);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRunTaskReportWithNonMatchingFilter()
		{
			SetUpPostMasterGroup();
			var org1 = Factory.New<OrgHeader>();
			Factory.RefreshEnabled = false;
			org1.OH_Code = "ABCDEFG";
			Factory.Save();

			var notifications = new NotificationBuffer();
			var menuItem = GetReportCommandWithTemplate("Test Sango", "MultipleSelectionLookup.xls");

			using (var documentPack = new DocumentPack(menuItem))
			using (var report = (Report)documentPack[0])
			{
				report.PrepareForRender();

				var orgFilter = (MultipleSelectionLookup)report.FilterCollection["Orgs"];
				orgFilter.ValueAsStringForSerialisation = "ABCDEFG";

				var deliveryInstructions = new DeliveryInstructions(documentPack);
				deliveryInstructions.Recipients.RemoveAll();

				var recipient = deliveryInstructions.Recipients.AddNew();
				recipient.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
				recipient.OrgHeaderPK = Organization.PK;
				recipient.AttachmentType = "PDF";
				recipient.Name = "Unit Test";
				recipient.Email = "unit.test@cargowise.com";

				var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
				scheduleTask.PopulateDefaultsFromDeliveryInstructions(deliveryInstructions);
				scheduleTask.S5_ParentID = menuItem.PK;
				scheduleTask.S5_ScheduleState = scheduleTask.SerializeForTesting(deliveryInstructions, report);
				Factory.Save();

				report.SetScheduleTask(scheduleTask);

				var newFactory = new BusinessObjectFactory();
				newFactory.RefreshEnabled = false;

				var reloadedOrg = newFactory.Load<OrgHeader>(org1.PK);
				reloadedOrg.OH_Code = "CODECHANGED";
				newFactory.Save();

				AssertEquals("Recipients number in schedule task must be the same as in delivery instructions.", deliveryInstructions.Recipients.Count, scheduleTask.Recipients.Count);

				using (var task = Factory.Load<ReportScheduleTaskForTesting>(scheduleTask.PK))
				{
					AssertEquals("task.S5_IsActive", true, task.S5_IsActive);
					AssertExceptionThrown<DocumentEngineException>(() => { task.Run(notifications); });
					AssertEquals("task.S5_IsActive", false, task.S5_IsActive);
				}
			}
		}

		[ExpectNoExceptions]
		public void TestPopulateDefaultsFromDeliverInstructionsWithScheduledReportWithOptionalColumns()
		{
			SetUpPostMasterGroup();

			StmMenuItem menuItem = Factory.LoadTop1<StmMenuItem>(new DocumentZQuery("RepOrdersReport", "Client - Order Status Summary"));
			ReportCommand command = Factory.Load<ReportCommand>(menuItem.PK);
			DocumentPack pack = new DocumentPack(command);
			using (Report report = (Report)pack[0])
			{
				report.PrepareForRender();
				LookupField configField = (LookupField)report.FilterCollection["Client"];
				OrgHeader org = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "A"));
				configField.Value = org.PK.ToGuid();
				report.ColumnHeadingManager.DefaultTemplateConfigurationManager.Load();

				DeliveryInstructions instructions = new DeliveryInstructions(pack);

				instructions.Recipients.RemoveAll();

				DocDeliveryContact contact1 = instructions.Recipients.AddNew();
				contact1.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
				contact1.AttachmentType = "PDF";
				contact1.OrgHeaderPK = Organization.PK;
				contact1.Name = "BOB1234";
				contact1.Email = "blah@blah.com";

				var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
				scheduleTask.PopulateDefaultsFromDeliveryInstructions(instructions);
				scheduleTask.Run();
			}
		}

		public void TestPopulateDefaultsFromDeliveryInstructions()
		{
			ReportCommand menuItem = Factory.NewWithValidTestData<ReportCommand>();
			menuItem.SU_BusinessContext = "RepShipment";
			menuItem.SU_ContactType = "SAL";
			Factory.Save();

			DocumentPack pack = new DocumentPack(menuItem);
			DeliveryInstructions instructions = new DeliveryInstructions(pack);
			instructions.Recipients.RemoveAll();

			DocDeliveryContact contact1 = instructions.Recipients.AddNew();
			contact1.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			contact1.AttachmentType = "PDF";
			contact1.OrgHeaderPK = Organization.PK;
			contact1.Name = "BOB1234";

			DocDeliveryContact contact2 = instructions.Recipients.AddNew();
			contact2.DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;
			contact2.AttachmentType = "XLS";
			contact2.OrgHeaderPK = Organization.PK;

			ReportScheduleTask scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			StmScheduleTaskRecipient initialRecipient1 = scheduleTask.Recipients.AddNew();
			StmScheduleTaskRecipient initialRecipient2 = scheduleTask.Recipients.AddNew();

			scheduleTask.PopulateDefaultsFromDeliveryInstructions(instructions);

			AssertEquals("Schedule type must be 'REP'.", ReportScheduleTask.ScheduleType, scheduleTask.S5_ScheduleType);
			AssertEquals("Parent ID must be the same as MenuItem ID.", menuItem.PK, scheduleTask.S5_ParentID);
			AssertEquals("Parent table code must be StmMenuItem prefix.", StmMenuItemSchema.Constants.Prefix, scheduleTask.S5_ParentTableCode);
			AssertEquals("Schedule state must be the serialized document pack.", scheduleTask.Serialize(), scheduleTask.S5_ScheduleState);
			AssertEquals("Recipients number in schedule task must be the same as in delivery instructions.", instructions.Recipients.Count, scheduleTask.Recipients.Count);

			AssertEquals("initialRecipient1 should be removed.", false, scheduleTask.Recipients.Contains(initialRecipient1));
			AssertEquals("initialRecipient2 should be removed.", false, scheduleTask.Recipients.Contains(initialRecipient2));
			AssertEquals("initialRecipient1.IsDeleted", true, initialRecipient1.IsDeleted);
			AssertEquals("initialRecipient1.IsDeleted", true, initialRecipient1.IsDeleted);
		}

		[ExpectExceptionMessage(typeof(ArgumentException), "Recipients must not contain items with different values of S6_SQ.\r\nParameter name: recipients")]
		public void TestCannotGroupRecipientsWithDifferentPrintersTogether()
		{
			ReportScheduleTaskRecipient recipient1 = CreateRecipient(ScheduledReportDeliveryRecipientConstants.RecipientType.Staff, Core.Constants.ContactNotifyModes.Print, GlbStaff.CurrentUser.GS_Code);
			ReportScheduleTaskRecipient recipient2 = CreateRecipient(ScheduledReportDeliveryRecipientConstants.RecipientType.Staff, Core.Constants.ContactNotifyModes.Print, GlbStaff.CurrentUser.GS_Code);
			recipient1.S6_SQ = ZGuid.NewZGuid();
			recipient2.S6_SQ = ZGuid.NewZGuid();
			ScheduleTask.AddScheduleRecipientsToInstructionsForTesting(new DeliveryInstructions(Pack), new ReportScheduleTaskRecipient[] { recipient1, recipient2 }, Notifications);
		}

		[ExpectNoExceptions]
		public void TestAddScheduleRecipientsToInstructionsWhenContactIsNull()
		{
			// Previously would throw a NullReferenceException on the contact in AddScheduleRecipientsToInstructions.
			SetUpPostMasterGroup();
			Instructions.Recipients[0].Name = "BOB123";
			ScheduleTask.PopulateDefaultsFromDeliveryInstructions(Instructions);
			ScheduleTask.Run();
		}

		public void TestPopulateDefaultsFromMenuItem()
		{
			StmMenuItem menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = "Goober";
			ReportScheduleTask scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.PopulateDefaultsFromMenuItem(menuItem);
			AssertEquals("S5_ScheduleType", ReportScheduleTask.ScheduleType, scheduleTask.S5_ScheduleType);
			AssertEquals("S5_ParentID", menuItem.PK, scheduleTask.S5_ParentID);
			AssertEquals("S5_ScheduleDescription", "Goober", scheduleTask.S5_ScheduleDescription);
			AssertEquals("S5_ParentTableCode", StmMenuItemSchema.Constants.Prefix, scheduleTask.S5_ParentTableCode);
			AssertNoNotifications(scheduleTask);
		}

		public void TestSetDeliveryInstructions()
		{
			ReportCommand menuItem = Factory.NewWithValidTestData<ReportCommand>();
			menuItem.SU_BusinessContext = "RepShipment";
			Factory.Save();

			DocumentPack pack = new DocumentPack(menuItem);
			DeliveryInstructions instructions = new DeliveryInstructions(pack);
			instructions.Recipients.RemoveAll();

			ReportScheduleTask scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();

			DocDeliveryContact contact1 = instructions.Recipients.AddNew();
			contact1.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			contact1.AttachmentType = "PDF";
			contact1.OrgHeaderPK = Organization.PK;
			contact1.Name = "BOB1234";

			DocDeliveryContact contact2 = instructions.Recipients.AddNew();
			contact2.DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;
			contact2.AttachmentType = "XLS";
			contact2.OrgHeaderPK = Organization.PK;

			scheduleTask.PopulateDefaultsFromDeliveryInstructions(instructions);
			AssertEquals("Schedule type must be first 3 letters from MenuItem Business Contects", ReportScheduleTask.ScheduleType, scheduleTask.S5_ScheduleType.ToUpper());
			AssertEquals("Parent Id must be PK of MenuItem", menuItem.PK, scheduleTask.S5_ParentID);
			AssertEquals("Parent Table Code must be prefix of MenuItem table", StmMenuItemSchema.Constants.Prefix, scheduleTask.S5_ParentTableCode);

			AssertEquals("Number of recipients in Schedule Task must be the same as in Instructions", 2, scheduleTask.Recipients.Count);

			AssertEquals("Delivery type of first recipient must be equal to delivery type of first instruction contact ", contact1.DeliveryMethod, scheduleTask.Recipients[0].S6_DeliveryMethod);
			AssertEquals("Attachment Type of first recipient must be equal to attachment type of first instruction contact ", contact1.AttachmentType, scheduleTask.Recipients[0].S6_AttachmentType);
			AssertEquals("Delivery to type of first recipient must be contact as first instruction contact has contact name", "CON", scheduleTask.Recipients[0].S6_DeliveryToType);
			AssertEquals("Organisation of first recipient must be equal to first instruction contact organisation", contact1.OrgHeaderPK, scheduleTask.Recipients[0].S6_OH);
			AssertEquals("Contact of first recipient must be equal to first instruction contact contact", contact1.Contact.PK, scheduleTask.Recipients[0].S6_OC);

			AssertEquals("Delivery type of second recipient must be equal to delivery type of second instruction contact ", contact2.DeliveryMethod, scheduleTask.Recipients[1].S6_DeliveryMethod);
			AssertEquals("Attachment Type of second recipient must be equal to attachment type of second instruction contact ", contact2.AttachmentType, scheduleTask.Recipients[1].S6_AttachmentType);
			AssertEquals("Organisation of second recipient must be equal to second instruction organisation", contact2.OrgHeaderPK, scheduleTask.Recipients[1].S6_OH);
		}

		[TestDate(2006, 1, 1)]
		[ExpectNoExceptions()]
		public void TestRunWithNoS5_ScheduleState()
		{
			ReportScheduleTask scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			Instructions.PrinterDelivery.NumberOfCopies = 2;
			ZGuid printerPK = ZGuid.NewZGuid();
			Instructions.PrinterDelivery.PrintQueuePK = printerPK;
			scheduleTask.PopulateDefaultsFromDeliveryInstructions(Instructions);
			scheduleTask.S5_StartDate = ZDateTime.Now.AddDays(-1);
			scheduleTask.S5_ScheduleState = ZBlob.Empty;
			Factory.Save();

			scheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddMinutes(-5);
			Factory.Save();

			scheduleTask.RunSafe(new NotificationBuffer());
		}

		[TestDate(2006, 1, 1)]
		public void TestRunCheckingInstructionsGetSetup()
		{
			using (ReportScheduleTaskForTesting scheduleTask = Factory.NewWithValidTestData<ReportScheduleTaskForTesting>())
			{
				Instructions.PrinterDelivery.NumberOfCopies = 2;
				ZGuid printerPK = ZGuid.NewZGuid();
				Instructions.PrinterDelivery.PrintQueuePK = printerPK;
				scheduleTask.PopulateDefaultsFromDeliveryInstructions(Instructions);
				scheduleTask.S5_StartDate = ZDateTime.Now.AddDays(-1);
				Factory.Save();

				scheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddMinutes(-5);
				Factory.Save();

				try
				{
					PrintTaskForTesting.DeliveryInstructionsRun = new List<DeliveryInstructions>();

					scheduleTask.RunSafe(new NotificationBuffer());
					AssertEquals("Should have run one instruction", 1, PrintTaskForTesting.DeliveryInstructionsRun.Count);
					AssertEquals("Should have two copies in PrinterDelivery", 2, PrintTaskForTesting.DeliveryInstructionsRun[0].PrinterDelivery.NumberOfCopies);
				}
				finally
				{
					PrintTaskForTesting.DeliveryInstructionsRun = null;
				}
			}
		}

		[TestDate(2006, 1, 1)]
		public void TestRun()
		{
			ReportScheduleTask scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.PopulateDefaultsFromDeliveryInstructions(Instructions);
			scheduleTask.S5_StartDate = ZDateTime.Now.AddDays(-1);
			Factory.Save();
			scheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddMinutes(-5);
			Factory.Save();

			using (Env.SetTemporaryUserContext(User.WebUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				scheduleTask.RunSafe(new NotificationBuffer());
				AssertEquals("Current user should be restored at the end of the report", "ZZ", Env.CurrentUser.Initials);
			}
		}

		[TestDate(2006, 1, 1)]
		public void TestRunWithWrongUser()
		{
			SetUpPostMasterGroup();

			var tempStaff = Factory.NewWithValidTestData<GlbStaff>();
			tempStaff.GS_LoginName = "Victim staff";
			tempStaff.GS_Code = "_X_";

			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.UserFK = tempStaff.PK;
			scheduleTask.PopulateDefaultsFromDeliveryInstructions(Instructions);
			scheduleTask.S5_StartDate = ZDateTime.Now.AddDays(-1);
			scheduleTask.S5_ScheduleDescription = "Test report";
			Factory.Save();

			scheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddMinutes(-5);
			tempStaff.Delete();
			Factory.Save();

			var notifications = new NotificationBuffer();
			scheduleTask.RunSafe(notifications);

			Assert(notifications.HasErrors);
			AssertContains("Scheduled Report 'Test report' was deactivated because it is configured to run under user '_X_ or Victim staff' which cannot be found.", notifications.AsString);

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("One or more errors were encountered for Scheduled Task - Test report run on 01-Jan-06 00:00:00", email.Subject);
			AssertContains("Scheduled Report 'Test report' was deactivated because it is configured to run under user '_X_ or Victim staff' which cannot be found.", email.Body);
		}

		[TestDate(2006, 1, 1)]
		public void TestRunWithInactiveUser()
		{
			SetUpPostMasterGroup();

			GlbStaff inactiveStaff = Factory.NewWithValidTestData<GlbStaff>();
			inactiveStaff.GS_LoginName = "Inactive staff";
			inactiveStaff.GS_Code = "INA";

			ReportScheduleTask scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.UserFK = inactiveStaff.PK;
			scheduleTask.PopulateDefaultsFromDeliveryInstructions(Instructions);
			scheduleTask.S5_StartDate = ZDateTime.Now.AddDays(-1);
			scheduleTask.S5_ScheduleDescription = "Test report";
			Factory.Save();

			scheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddMinutes(-5);
			inactiveStaff.GS_IsActive = false;
			Factory.Save();

			var notifications = new NotificationBuffer();
			scheduleTask.RunSafe(notifications);

			Assert(notifications.HasErrors);
			AssertContains("Scheduled Report 'Test report' was deactivated because it is configured to run under user 'Inactive staff' which is inactive.", notifications.AsString);

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("One or more errors were encountered for Scheduled Task - Test report run on 01-Jan-06 00:00:00", email.Subject);
			AssertContains("Scheduled Report 'Test report' was deactivated because it is configured to run under user 'Inactive staff' which is inactive.", email.Body);
		}

		public void TestGlbStaffCannotBeDeactivatedWhenAssignedToScheduledReport()
		{
			var scheduleDescription = "Test report";
			var heroStaff = Factory.New<GlbStaff>();
			heroStaff.GS_LoginName = "Hero";
			heroStaff.GS_Code = "EGI";

			ReportScheduleTask scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.UserFK = heroStaff.PK;
			scheduleTask.PopulateDefaultsFromDeliveryInstructions(Instructions);
			scheduleTask.S5_StartDate = ZDateTime.Now.AddDays(-1);
			scheduleTask.S5_ScheduleDescription = scheduleDescription;
			Factory.Save();

			heroStaff.GS_IsActive = false;
			AssertHasError(heroStaff.GS_IsActiveInfo, string.Format("Staff cannot be deactivated because it has been set as a print user or recipient on at least one scheduled report:\r\n{0}.", scheduleDescription));

			scheduleTask.UserFK = GlbStaff.CurrentUser.PK;
			scheduleTask.PopulateDefaultsFromDeliveryInstructions(Instructions);
			Factory.Save();
			heroStaff.GS_IsActive = false;
			heroStaff.RunPreSaveValidation();
			AssertNoErrors(heroStaff.GS_IsActiveInfo);

			scheduleTask.UserFK = heroStaff.PK;
			scheduleTask.PopulateDefaultsFromDeliveryInstructions(Instructions);
			Factory.Save();

			heroStaff.GS_IsActive = true;
			heroStaff.RunPreSaveValidation();
			AssertNoErrors(heroStaff.GS_IsActiveInfo);

			var recipient = scheduleTask.Recipients.AddNew();
			recipient.S6_DeliveryToType = "STF";
			recipient.S6_GS_NKRecipient = heroStaff.GS_Code;
			recipient.S6_AttachmentType = "PDF";
			recipient.S6_DeliveryMethod = "EML";
			scheduleTask.S5_ScheduleDescription = scheduleDescription;
			Factory.Save();

			heroStaff.GS_IsActive = false;
			heroStaff.RunPreSaveValidation();
			AssertHasError(heroStaff.GS_IsActiveInfo, string.Format("Staff cannot be deactivated because it has been set as a print user or recipient on at least one scheduled report:\r\n{0}.", scheduleDescription));

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Guid.Empty, Env.CurrentDepartmentPK))
			{
				heroStaff.RunPreSaveValidation();
				AssertNoErrors(heroStaff.GS_IsActiveInfo);
			}

			recipient.S6_GS_NKRecipient = "E";
			Factory.Save();

			heroStaff.GS_IsActive = true;
			heroStaff.GS_IsActive = false;
			heroStaff.RunPreSaveValidation();
			AssertNoErrors(heroStaff.GS_IsActiveInfo);

			recipient.S6_GS_NKRecipient = heroStaff.GS_Code;
			heroStaff.GS_IsActive = false;
			Factory.Save();

			heroStaff.RunPreSaveValidation();
			AssertNoErrors(heroStaff.GS_IsActiveInfo);
		}

		[TestDate(2006, 1, 1)]
		public void TestRunInNonUserInteractiveModeDoesNotShowAnyForms()
		{
			using (new PrintTaskUIProviderForTestingSuppressor()) // Stop the suppressing of notifications.
			{
				bool originalIsUserInteractive = Globals.IsUserInteractive;
				Globals.IsUserInteractive = false;
				long originalFormConstructedCount = TestingState.FormConstructedCount;

				ReportScheduleTaskForTesting scheduleTask = Factory.NewWithValidTestData<ReportScheduleTaskForTesting>();
				scheduleTask.PopulateDefaultsFromDeliveryInstructions(Instructions);
				scheduleTask.S5_StartDate = ZDateTime.Now.AddDays(-1);
				Factory.Save();
				scheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddMinutes(-5);
				Factory.Save();

				using (Env.SetTemporaryUserContext(User.WebUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
				{
					try
					{
						scheduleTask.RunSafe(new NotificationBuffer());
						AssertEquals("Current user should be restored at the end of the report", "ZZ", Env.CurrentUser.Initials);
						AssertEquals("No new forms should be shown.", originalFormConstructedCount, TestingState.FormConstructedCount);
					}
					finally
					{
						Globals.IsUserInteractive = originalIsUserInteractive;
					}
				}
			}
		}

		[ExpectNoExceptions]
		public void TestRunDoesNotCrashIfStaffIsNull()
		{
			var instructions = CreateTestInstructions();
			ScheduleTask.PopulateDefaultsFromDeliveryInstructions(instructions);
			var recipient = ScheduleTask.Recipients.AddNew();
			recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			recipient.S6_AttachmentType = OrgConstants.AttachmentType.TIF;
			recipient.S6_EmailToRecipientsAsString = "bubba@prison.com";
			ScheduleTask.Run();
		}

		[ExpectNoExceptions]
		public void TestRun_InvalidParentID()
		{
			ReportScheduleTask scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.PopulateDefaultsFromDeliveryInstructions(Instructions);
			scheduleTask.S5_ParentID = ZGuid.NewZGuid();
			scheduleTask.S5_StartDate = ZDateTime.Now.AddDays(-1);
			Factory.Save();
			scheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddMinutes(-5);
			Factory.Save();

			scheduleTask.Run();
		}

		[ExpectNoExceptions]
		public void TestDeliverAndContinueDoesNotCrash()
		{
			var instructions = CreateTestInstructions();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Bob";

			ScheduleTask.PopulateDefaultsFromDeliveryInstructions(instructions);
			ScheduleTask.S5_ScheduleDescription = "Test Schedule";
			ScheduleTask.Recipients.Add(CreateRecipient(ScheduledReportDeliveryRecipientConstants.RecipientType.Staff, Core.Constants.ContactNotifyModes.Email, staff.GS_Code));
			ScheduleTask.S5_IsActive = false;
			ScheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now;
			ScheduleTask.Run();
		}

		public void TestDeliverScheduleTaskHasPrintUserVaule()
		{
			var instructions = CreateTestInstructions();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Bob";

			ScheduleTask.PopulateDefaultsFromDeliveryInstructions(instructions);
			AssertEquals(GlbStaff.CurrentUser.GS_Code, ScheduleTask.S5_GS_NKPrintUser);

			ScheduleTask.UserFK = staff.PK;
			ScheduleTask.PopulateDefaultsFromDeliveryInstructions(instructions);
			AssertEquals(staff.GS_Code, ScheduleTask.S5_GS_NKPrintUser);
		}

		public void TestActiveRecipient()
		{
			var emptyReportInfo = new ReportSerializationInfo(null, null);
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var activeContact = CreateContact(organisation, "c1", "c1@o2.com", "c1fax");

			var activeContactWithOverridenAddress = CreateContact(organisation, "c3", "c3@o2.com", "");

			var activeRecipientWithNoDeliveryType = CreateContact(organisation, "c4", "c4@o2.com", "");

			var inactiveContact = CreateContact(organisation, "c2", "c2@o2.com", "");
			inactiveContact.OC_IsActive = false;

			var activeGroup = Factory.New<GlbGroup>();
			activeGroup.GG_Code = "g1";

			var activeStaff1 = CreateStaff(activeGroup, "XXX", "s3", "s3@g2.com", "s3fax");
			var activeStaff2 = CreateStaff(activeGroup, "ZZZ", "s4", "s4@g2.com", "s4fax");

			var inactiveGroup = Factory.New<GlbGroup>();
			inactiveGroup.GG_Code = "g2";
			inactiveGroup.GG_IsActive = false;

			var inactiveStaff1 = CreateStaff(inactiveGroup, "VVV", "s1", "s1@g1.com", "s1fax");
			inactiveStaff1.GS_IsActive = false;

			var inactiveStaff2 = CreateStaff(inactiveGroup, "WWW", "s2", "s2@g1.com", "s2fax");
			inactiveStaff2.GS_IsActive = false;

			var activeRecipient1 = CreateRecipient(ScheduledReportDeliveryRecipientConstants.RecipientType.Contact, Core.Constants.ContactNotifyModes.Email, activeContact.PK);
			var activeRecipient2 = CreateRecipient(ZString.Empty, Core.Constants.ContactNotifyModes.Email, activeContactWithOverridenAddress.OC_ContactName);
			activeRecipient2.S6_EmailToRecipientsAsString = "override@g2.com";
			var activeRecipient3 = CreateRecipient(ScheduledReportDeliveryRecipientConstants.RecipientType.Group, Core.Constants.ContactNotifyModes.Print, activeGroup.PK);
			var activeRecipient4 = CreateRecipient(ScheduledReportDeliveryRecipientConstants.RecipientType.Staff, Core.Constants.ContactNotifyModes.Fax, activeStaff1.GS_Code);
			var activeRecipient5 = CreateRecipient(ScheduledReportDeliveryRecipientConstants.RecipientType.Staff, Core.Constants.ContactNotifyModes.Fax, activeStaff2.GS_Code);
			var activeRecipient6 = CreateRecipient(ZString.Empty, Core.Constants.ContactNotifyModes.Email, activeRecipientWithNoDeliveryType.PK);

			var inactiveRecipient1 = CreateRecipient(ScheduledReportDeliveryRecipientConstants.RecipientType.Contact, Core.Constants.ContactNotifyModes.Email, inactiveContact.PK);
			var inactiveRecipient2 = CreateRecipient(ScheduledReportDeliveryRecipientConstants.RecipientType.Group, Core.Constants.ContactNotifyModes.Print, inactiveGroup.PK);
			var inactiveRecipient3 = CreateRecipient(ScheduledReportDeliveryRecipientConstants.RecipientType.Staff, Core.Constants.ContactNotifyModes.Fax, inactiveStaff1.GS_Code);
			var inactiveRecipient4 = CreateRecipient(ScheduledReportDeliveryRecipientConstants.RecipientType.Staff, Core.Constants.ContactNotifyModes.Fax, inactiveStaff2.GS_Code);

			DeliveryInstructions[] instructions = ScheduleTask.GetDeliveryInstructionsForTesting(emptyReportInfo, Pack, Notifications);

			AssertEquals("Instruction Length", 1, instructions.Length);
			AssertEquals("ActiveRecipients.Count", 6, scheduleTask.ActiveRecipientsCollection.Count);
		}

		public void TestGetDeliveryInstructions()
		{
			ReportSerializationInfo emptyReportInfo = new ReportSerializationInfo(null, null);

			OrgHeader organisation1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader organisation2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader organisation3 = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			OrgContact contact1 = CreateContact(organisation1, "c1", "c1@o1.com", "c1fax");
			OrgContact contact2 = CreateContact(organisation2, "c2", "c2@o2.com", "c2fax");
			OrgContact contact3 = CreateContact(organisation2, "c3", "c3@o2.com", "c3fax");
			OrgContact contact4 = CreateContact(organisation3, "", "c4@o2.com", "c4fax");
			OrgContact contact5 = CreateContact(organisation3, "", "c5@o2.com", "c5fax");
			OrgContact contact6 = CreateContact(organisation3, "c6", "c5@o2.com", "");
			contact6.OC_IsActive = false;

			GlbGroup group1 = Factory.New<GlbGroup>();
			GlbGroup group2 = Factory.New<GlbGroup>();
			GlbGroup group3 = Factory.New<GlbGroup>();
			GlbGroup group4 = Factory.New<GlbGroup>();
			GlbGroup group5 = Factory.New<GlbGroup>();
			GlbGroup group6 = Factory.New<GlbGroup>();

			group5.GG_Code = "g5";
			group6.GG_Code = "g6";

			GlbStaff staff1 = CreateStaff(group1, "VVV", "s1", "s1@g1.com", "s1fax");
			GlbStaff staff2 = CreateStaff(group1, "WWW", "s2", "s2@g1.com", "s2fax");
			GlbStaff staff3 = CreateStaff(group2, "XXX", "s3", "s3@g2.com", "s3fax");
			GlbStaff staff4 = CreateStaff(group3, "YYY", "s4", "s4@g3.com", "s4fax");
			GlbStaff staff5 = CreateStaff(group4, "ZZZ", "s5", null, null);

			group4.Staff.Add(staff1);
			group5.Staff.Add(staff5);

			ZGuid sq1 = ZGuid.NewZGuid();
			ZGuid sq2 = ZGuid.NewZGuid();

			StmScheduleTaskRecipient recipient1 = CreateRecipient(ScheduledReportDeliveryRecipientConstants.RecipientType.Contact, Core.Constants.ContactNotifyModes.Email, contact1.OC_ContactName);
			StmScheduleTaskRecipient recipient2 = CreateRecipient(ScheduledReportDeliveryRecipientConstants.RecipientType.Staff, Core.Constants.ContactNotifyModes.Fax, staff1.GS_Code);
			StmScheduleTaskRecipient recipient3 = CreateRecipient(ScheduledReportDeliveryRecipientConstants.RecipientType.Group, Core.Constants.ContactNotifyModes.Print, group1.PK);
			StmScheduleTaskRecipient recipient4 = CreateRecipient(ScheduledReportDeliveryRecipientConstants.RecipientType.Staff, Core.Constants.ContactNotifyModes.Email, staff3.GS_Code);
			StmScheduleTaskRecipient recipient5 = CreateRecipient(ScheduledReportDeliveryRecipientConstants.RecipientType.Contact, Core.Constants.ContactNotifyModes.Fax, contact2.OC_ContactName);
			StmScheduleTaskRecipient recipient6 = CreateRecipient(ScheduledReportDeliveryRecipientConstants.RecipientType.Staff, Core.Constants.ContactNotifyModes.Print, staff4.GS_Code);
			StmScheduleTaskRecipient recipient7 = CreateRecipient(ScheduledReportDeliveryRecipientConstants.RecipientType.Group, Core.Constants.ContactNotifyModes.Email, group2.PK);
			StmScheduleTaskRecipient recipient8 = CreateRecipient(ScheduledReportDeliveryRecipientConstants.RecipientType.Contact, Core.Constants.ContactNotifyModes.Print, contact3.OC_ContactName);
			StmScheduleTaskRecipient recipient9 = CreateRecipient(ScheduledReportDeliveryRecipientConstants.RecipientType.Group, Core.Constants.ContactNotifyModes.Fax, group3.PK);
			StmScheduleTaskRecipient recipient10 = CreateRecipient(ScheduledReportDeliveryRecipientConstants.RecipientType.Staff, Core.Constants.ContactNotifyModes.Print, staff5.GS_Code);
			StmScheduleTaskRecipient recipient11 = CreateRecipient(ScheduledReportDeliveryRecipientConstants.RecipientType.Group, Core.Constants.ContactNotifyModes.Email, group4.PK);
			StmScheduleTaskRecipient recipient12 = CreateRecipient(ScheduledReportDeliveryRecipientConstants.RecipientType.Group, Core.Constants.ContactNotifyModes.Fax, group4.PK);
			StmScheduleTaskRecipient recipient13 = CreateRecipient(ScheduledReportDeliveryRecipientConstants.RecipientType.Staff, Core.Constants.ContactNotifyModes.Email, staff1.GS_Code);

			StmScheduleTaskRecipient recipient14 = CreateRecipient(ScheduledReportDeliveryRecipientConstants.RecipientType.Group, Core.Constants.ContactNotifyModes.Email, group5.PK);
			StmScheduleTaskRecipient recipient15 = CreateRecipient(ScheduledReportDeliveryRecipientConstants.RecipientType.Group, Core.Constants.ContactNotifyModes.Fax, group5.PK);
			StmScheduleTaskRecipient recipient16 = CreateRecipient(ScheduledReportDeliveryRecipientConstants.RecipientType.Group, Core.Constants.ContactNotifyModes.Print, group6.PK);
			StmScheduleTaskRecipient recipient17 = CreateRecipient(ScheduledReportDeliveryRecipientConstants.RecipientType.Staff, Core.Constants.ContactNotifyModes.Email, staff5.GS_Code);
			StmScheduleTaskRecipient recipient18 = CreateRecipient(ScheduledReportDeliveryRecipientConstants.RecipientType.Staff, Core.Constants.ContactNotifyModes.Fax, staff5.GS_Code);

			StmScheduleTaskRecipient recipient19 = CreateRecipient(ScheduledReportDeliveryRecipientConstants.RecipientType.Contact, Core.Constants.ContactNotifyModes.Email, contact4.OC_ContactName);
			StmScheduleTaskRecipient recipient20 = CreateRecipient(ScheduledReportDeliveryRecipientConstants.RecipientType.Contact, Core.Constants.ContactNotifyModes.Email, contact5.OC_ContactName);
			StmScheduleTaskRecipient recipient21 = CreateRecipient(ScheduledReportDeliveryRecipientConstants.RecipientType.Contact, Core.Constants.ContactNotifyModes.Email, contact6.OC_ContactName);

			recipient1.S6_AttachmentType = OrgConstants.AttachmentType.PDF;
			recipient4.S6_AttachmentType = OrgConstants.AttachmentType.TIF;
			recipient7.S6_AttachmentType = OrgConstants.AttachmentType.TIF;
			recipient11.S6_AttachmentType = OrgConstants.AttachmentType.XLS;
			recipient13.S6_AttachmentType = OrgConstants.AttachmentType.PDF;

			recipient1.S6_OH = organisation1.PK;
			recipient5.S6_OH = organisation2.PK;
			recipient8.S6_OH = organisation2.PK;
			recipient19.S6_OH = organisation3.PK;
			recipient20.S6_OH = organisation3.PK;

			recipient3.S6_SQ = sq1;
			recipient6.S6_SQ = sq2;
			recipient8.S6_SQ = sq1;
			recipient10.S6_SQ = sq2;

			recipient1.EmailToRecipients.Value = "efo1@efo.com";
			recipient4.S6_EmailToRecipientsAsString = "efo4@efo.com";
			recipient4.S6_CarbonCopyRecipientsAsString = "cc1@efo.com, cc2@efo.com";
			recipient4.S6_BlindCarbonCopyRecipientsAsString = "bcc1@efo.com, bcc2@efo.com";
			recipient11.EmailToRecipients.Value = "efo11@efo.com";
			recipient19.S6_AttachmentType = OrgConstants.AttachmentType.PDF;
			recipient20.S6_AttachmentType = OrgConstants.AttachmentType.PDF;
			recipient21.S6_AttachmentType = OrgConstants.AttachmentType.PDF;

			recipient19.S6_EmailToRecipientsAsString = "efo19@efo.com";
			recipient20.S6_EmailToRecipientsAsString = "efo20@efo.com";

			DeliveryInstructions[] instructions = ScheduleTask.GetDeliveryInstructionsForTesting(emptyReportInfo, Pack, Notifications);
			AssertEquals("Length", 2, instructions.Length);
			AssertEquals("[0].ParentGuid", ScheduleTask.PK, instructions[0].ParentGuid);
			AssertEquals("[1].ParentGuid", ScheduleTask.PK, instructions[1].ParentGuid);
			AssertEquals("[0].DocPack", Pack, instructions[0].DocPack);
			AssertEquals("[1].DocPack", Pack, instructions[1].DocPack);
			AssertEquals("[0].PrinterDelivery.PrintQueuePK", sq1, instructions[0].PrinterDelivery.PrintQueuePK);
			AssertEquals("[0].PrinterDelivery.PrintQueuePK", sq2, instructions[1].PrinterDelivery.PrintQueuePK);
			AssertEquals("[0].Recipients.Count", 12, instructions[0].Recipients.Count);
			AssertEquals("[0].Recipients.Count", 2, instructions[1].Recipients.Count);

			// Will delete comments
			// Contacts info shouldn't show if there is an override.
			AssertDocDeliveryContact("[0].Recipients[0]", instructions[0].Recipients[0], "c1", Core.Constants.ContactNotifyModes.Email, OrgConstants.AttachmentType.PDF, "efo1@efo.com", organisation1.PK);
			AssertDocDeliveryContact("[0].Recipients[1]", instructions[0].Recipients[1], "s1", Core.Constants.ContactNotifyModes.Fax, "", "s1fax", ZGuid.Empty);
			AssertDocDeliveryContact("[0].Recipients[2]", instructions[0].Recipients[2], "s1", Core.Constants.ContactNotifyModes.Print, "", "", ZGuid.Empty);
			AssertDocDeliveryContact("[0].Recipients[3]", instructions[0].Recipients[3], "s2", Core.Constants.ContactNotifyModes.Print, "", "", ZGuid.Empty);
			// Staffs email shouldn't show if there is an override.
			AssertDocDeliveryContact("[0].Recipients[4]", instructions[0].Recipients[4], "s3", Core.Constants.ContactNotifyModes.Email, OrgConstants.AttachmentType.TIF, "efo4@efo.com", ZGuid.Empty);
			AssertEquals(instructions[0].Recipients[4].EmailCarbonCopyRecipientsAsString, "cc1@efo.com, cc2@efo.com");
			AssertEquals(instructions[0].Recipients[4].EmailBlindCarbonCopyRecipientsAsString, "bcc1@efo.com, bcc2@efo.com");
			AssertDocDeliveryContact("[0].Recipients[5]", instructions[0].Recipients[5], "c2", Core.Constants.ContactNotifyModes.Fax, "", "c2fax", organisation2.PK);
			AssertDocDeliveryContact("[0].Recipients[6]", instructions[0].Recipients[6], "c3", Core.Constants.ContactNotifyModes.Print, "", "", organisation2.PK);
			AssertDocDeliveryContact("[0].Recipients[7]", instructions[0].Recipients[7], "s4", Core.Constants.ContactNotifyModes.Fax, "", "s4fax", ZGuid.Empty);
			AssertDocDeliveryContact("[0].Recipients[8]", instructions[0].Recipients[8], "", Core.Constants.ContactNotifyModes.Email, OrgConstants.AttachmentType.XLS, "efo11@efo.com", ZGuid.Empty);
			AssertDocDeliveryContact("[0].Recipients[9]", instructions[0].Recipients[9], "s1", Core.Constants.ContactNotifyModes.Email, OrgConstants.AttachmentType.PDF, "s1@g1.com", ZGuid.Empty);

			AssertDocDeliveryContact("[1].Recipients[0]", instructions[1].Recipients[0], "s4", Core.Constants.ContactNotifyModes.Print, "", "", ZGuid.Empty);
			AssertDocDeliveryContact("[1].Recipients[1]", instructions[1].Recipients[1], "s5", Core.Constants.ContactNotifyModes.Print, "", "", ZGuid.Empty);

			string expectedErrors =
@"There were no staff members with an email address in the g5 group.
There were no staff members with a fax number in the g5 group.
There were no staff members in the g6 group.
";

			AssertEquals("ScheduleRunErrors.ToString()", expectedErrors, Notifications.AsString);
		}

		public void TestRunScheduleTaskWithDBServerInfo_MustRunOnline()
		{
			using (SystemDataRegistry.Instance.ReportingDbServerNames.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new string[] { Db.Connection.ServerName }))
			{
				SetupReportScheduleTask(out var scheduleTask, out var reportCommand, out var docPack, out var deliveryInstructions);
				reportCommand.SU_MustRunOnline = true;

				using (var report = (Report)docPack[0])
				{
					report.PrepareForRender();
					scheduleTask.S5_ScheduleState = scheduleTask.SerializeForTesting(deliveryInstructions, report);
				}

				var notifications = new NotificationBuffer();
				scheduleTask.Run(notifications);

				var note = scheduleTask.StmReportRun.Notes.FindByDescription("DB Server Info", false, SQLComparisonOperator.Equal).FirstOrDefault();
				AssertEquals("Report is set to must run online, report will be run on primary server.\r\n", note.ST_NoteDataAsText);
			}
		}

		public void TestRunScheduleTaskWithDBServerInfo_OverrideReportDbOption()
		{
			using (SystemDataRegistry.Instance.ReportingDbServerNames.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new string[] { Db.Connection.ServerName }))
			{
				SetupReportScheduleTask(out var scheduleTask, out var reportCommand, out var docPack, out var deliveryInstructions);

				using (var report = (Report)docPack[0])
				{
					report.PrepareForRender();
					report.OverrideReportDbOption = true;
					scheduleTask.S5_ScheduleState = scheduleTask.SerializeForTesting(deliveryInstructions, report);
				}

				var notifications = new NotificationBuffer();
				scheduleTask.Run(notifications);

				var note = scheduleTask.StmReportRun.Notes.FindByDescription("DB Server Info", false, SQLComparisonOperator.Equal).FirstOrDefault();
				AssertEquals("User required to run online, report will be run on primary server.\r\n", note.ST_NoteDataAsText);
			}
		}

		public void TestGetNewConnectionWrapperAddLogs_IsReportingDbEnabled()
		{
			using (SystemDataRegistry.Instance.ReportingDbServerNames.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Array.Empty<string>()))
			{
				var logs = new StringBuilder();
				var provider = SecondaryServerConnectionProviderProvider.GetProvider(null, (message) => logs.AppendLine(message));
				provider.GetNewConnectionWrapper();

				AssertEquals("The reporting database servers are not set up in the registry setting, report will be run on primary server.\r\n", logs.ToString());
			}
		}

		public void TestRunScheduleTaskDeliveryToGroupWithEmptyReportContigencyForEmails()
		{
			Env.Registry.EmailDestinationOverride = "";
			AssertEquals("Pre-confition: No emails created", Env.OutgoingMailManager.EmailsCreated.Count, 0);

			var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.EmptyAndValidTemplate.xls", "EmptyAndValidTemplate.xls");
			var excelTemplate = new ExcelTemplateForUnitTesting("EmptyAndValidTemplate.xls", Path.GetFullPath(tempFileName));
			var command = DocumentEngineTestHelper.CreateReportCommandWithExcelTemplate("Test Empty Report", excelTemplate, Factory);

			var scheduleTask = Factory.New<ReportScheduleTaskForEmptyReportContingencyTesting>();

			scheduleTask.S5_ParentID = command.PK;
			scheduleTask.S5_EndDate = scheduleTask.S5_StartDate.AddDays(10);
			scheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddMinutes(-5);

			var group = Factory.New<GlbGroup>();
			var staff = group.Staff.AddNew();
			staff.GS_EmailAddress = "UnitTest@CargoWise.com";
			staff.GS_FullName = "Staff";

			var staff1 = group.Staff.AddNew();
			staff1.GS_EmailAddress = "UnitTest1@CargoWise.com";
			staff1.GS_FullName = "Staff1";

			var staff2 = group.Staff.AddNew();
			staff2.GS_EmailAddress = "UnitTest2@CargoWise.com";
			staff2.GS_FullName = "Staff2";

			var recipient = scheduleTask.Recipients.AddNew();
			recipient.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.Group;
			recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			recipient.S6_EmptyReportDeliveryOptions = EmptyReportContingencyList.Codes.SendEmailNotification;
			recipient.S6_AttachmentType = OrgConstants.AttachmentType.XLS;
			recipient.S6_GG = group.PK;

			var docPack = new DocumentPack(command);
			using (var report = (Report)docPack[0])
			{
				report.PrepareForRender();
				scheduleTask.S5_ScheduleState = scheduleTask.SerializeForTesting(docPack.DeliveryInstructions, report);
			}

			scheduleTask.RunForTesting();

			var emailsCreated = Env.OutgoingMailManager.EmailsCreated;
			AssertEquals("X emails created, X equals to the number of recipients list", 3, emailsCreated.Count);
			foreach (var emailDef in emailsCreated)
			{
				AssertEquals("Notification email should be sent to the contact only instead of mailing group", 1, emailDef.Recipients.Count);
			}
		}

		[ExpectNoExceptions]
		public void TestRunScheduleTaskNoErrorsWithTwoDeliveryMethods()
		{
			var command = DocumentEngineTestHelper.CreateReportCommandWithExcelTemplate("Test Empty Report", SimpleTestTemplate, Factory);
			var reportScheduleTask = Factory.New<ReportScheduleTaskForEmptyReportContingencyTesting>();

			reportScheduleTask.S5_ParentID = command.PK;
			reportScheduleTask.S5_EndDate = reportScheduleTask.S5_StartDate.AddDays(10);
			reportScheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddMinutes(-5);

			var recipient1 = reportScheduleTask.Recipients.AddNew();
			recipient1.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.Contact;
			recipient1.S6_DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Email;
			recipient1.S6_EmptyReportDeliveryOptions = EmptyReportContingencyList.Codes.SendEmailNotification;
			recipient1.S6_AttachmentType = AttachmentTypeList.Codes.Xml;
			recipient1.S6_EmailToRecipientsAsString = "test@test.com";

			var recipient2 = reportScheduleTask.Recipients.AddNew();
			recipient2.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.Contact;
			recipient2.S6_DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Ftp;
			recipient2.S6_EmptyReportDeliveryOptions = EmptyReportContingencyList.Codes.SendNothing;
			recipient2.S6_AttachmentType = AttachmentTypeList.Codes.Xml;
			recipient2.S6_FtpAddress = "127.0.0.1";

			var docPack = new DocumentPack(command);
			using (var report = (Report)docPack[0])
			{
				report.PrepareForRender();
				reportScheduleTask.S5_ScheduleState = reportScheduleTask.SerializeForTesting(docPack.DeliveryInstructions, report);
			}

			Report.FireDummyGarbageCollection = true;
			reportScheduleTask.RunForTesting();
			Report.FireDummyGarbageCollection = false;
		}

		public void TestRunScheduleTaskWithFTPDestinationOverrideEmpty()
		{
			SystemDataRegistry.Instance.FTPDestinationOverride.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new FTPDestinationOverrideInfo(string.Empty, string.Empty, string.Empty));

			var instructions = CreateTestInstructions();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Bob";

			ScheduleTask.PopulateDefaultsFromDeliveryInstructions(instructions);
			ScheduleTask.S5_ScheduleDescription = "Test Schedule";
			ScheduleTask.Recipients.Add(CreateRecipient(ScheduledReportDeliveryRecipientConstants.RecipientType.Staff, Core.Constants.ContactNotifyModes.Email, staff.GS_Code));
			ScheduleTask.S5_IsActive = false;
			ScheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now;

			var recipient = ScheduleTask.Recipients.AddNew();
			recipient.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.Staff;
			recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Ftp;
			recipient.S6_EmptyReportDeliveryOptions = EmptyReportContingencyList.Codes.SendNothing;
			recipient.S6_AttachmentType = AttachmentTypeList.Codes.Xml;
			recipient.S6_FtpAddress = "127.0.1.0";

			var notifications = new NotificationBuffer();
			ScheduleTask.Run(notifications);
			AssertContains("The FTP Destination Override setting has not been set. Since this is a non-production system, the report was not delivered to the intended FTP address. Please enter a FTP Destination Override setting in the Registry at System > Testing > FTP Destination Override.", notifications.AsString);
		}

		public void TestFTPDestinationOverrideWithTwoDeliveryTypes()
		{
			SystemDataRegistry.Instance.FTPDestinationOverride.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new FTPDestinationOverrideInfo("127.1.1.1", "UserNameTest", "PasswordTest"));

			var reportScheduleTaskDeliveryStaff = Factory.New<ReportScheduleTaskForEmptyReportContingencyTesting>();
			var recipient = reportScheduleTaskDeliveryStaff.Recipients.AddNew();
			recipient.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.Staff;
			recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Ftp;
			recipient.S6_EmptyReportDeliveryOptions = EmptyReportContingencyList.Codes.SendNothing;
			recipient.S6_AttachmentType = AttachmentTypeList.Codes.Xml;
			recipient.S6_FtpAddress = "127.0.1.1";

			var contact = ScheduleTask.CreateSingleDocDeliveryContactForTesting(recipient, Notifications);
			AssertEquals(contact.FileLocation, SystemDataRegistry.Instance.FTPDestinationOverride.Value.FtpAddress);
			AssertEquals(contact.UserName, SystemDataRegistry.Instance.FTPDestinationOverride.Value.UserName);
			AssertEquals(contact.Password, SystemDataRegistry.Instance.FTPDestinationOverride.Value.Password);

			var reportScheduleTaskDeliveryContact = Factory.New<ReportScheduleTaskForEmptyReportContingencyTesting>();
			var recipient2 = reportScheduleTaskDeliveryContact.Recipients.AddNew();
			recipient2.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.Contact;
			recipient2.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Ftp;
			recipient2.S6_EmptyReportDeliveryOptions = EmptyReportContingencyList.Codes.SendNothing;
			recipient2.S6_AttachmentType = AttachmentTypeList.Codes.Xml;
			recipient2.S6_FtpAddress = "127.0.0.1";

			var contact2 = ScheduleTask.CreateSingleDocDeliveryContactForTesting(recipient2, Notifications);
			AssertEquals(contact2.FileLocation, SystemDataRegistry.Instance.FTPDestinationOverride.Value.FtpAddress);
			AssertEquals(contact2.UserName, SystemDataRegistry.Instance.FTPDestinationOverride.Value.UserName);
			AssertEquals(contact2.Password, SystemDataRegistry.Instance.FTPDestinationOverride.Value.Password);
		}

		public void TestEmailSubjectLineOverride()
		{
			var reportScheduleTask = Factory.New<ReportScheduleTaskForEmptyReportContingencyTesting>();
			var recipient = reportScheduleTask.Recipients.AddNew();
			recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			recipient.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.Contact;
			recipient.S6_FaxOverride = "wtg@test.com";
			recipient.S6_EmailSubjectLineOverride = "Test Email SubjectLine Override";

			var docContact = reportScheduleTask.CreateSingleDocDeliveryContactForTesting(recipient, Notifications);
			AssertEquals("Test Email SubjectLine Override", docContact.EmailSubjectMacro);
		}

		public void TestEdocDeliveryContactGlbStaffRecipientInfo()
		{
			var reportScheduleTask = Factory.New<ReportScheduleTaskForEmptyReportContingencyTesting>();
			var recipient = reportScheduleTask.Recipients.AddNew();
			recipient.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.EDoc;
			recipient.S6_GS_NKRecipient = ((GlbStaff)Env.Instance.CurrentUser).GS_Code;
			recipient.S6_EmailSubjectLineOverride = "Test Email SubjectLine Override";
			var stmReportRun = Factory.NewWithValidTestData<StmReportRun>();
			reportScheduleTask.StmReportRun = stmReportRun;

			var deliveryInstructions = new DeliveryInstructions(Pack);
			reportScheduleTask.AddScheduleRecipientsToInstructionsForTesting(deliveryInstructions, new ReportScheduleTaskRecipient[] { recipient }, Notifications);
			var docContact = deliveryInstructions.Recipients.Last() as DocDeliveryContact;

			AssertEquals("Test staff recipient", ((GlbStaff)Env.Instance.CurrentUser).GS_Code, docContact.StaffCode);
		}

		public void TestEdocDeliveryContactOrgContactRecipientInfo()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgContact = orgHeader.Contacts.AddNew();
			orgContact.OC_ContactName = "2334";
			Factory.Save();

			var reportScheduleTask = Factory.New<ReportScheduleTaskForEmptyReportContingencyTesting>();
			var recipient = reportScheduleTask.Recipients.AddNew();
			recipient.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.EDoc;
			recipient.S6_OH = orgHeader.PK;
			recipient.ContactName = orgContact.OC_ContactName;
			var stmReportRun = Factory.NewWithValidTestData<StmReportRun>();
			reportScheduleTask.StmReportRun = stmReportRun;

			var deliveryInstructions = new DeliveryInstructions(Pack);
			reportScheduleTask.AddScheduleRecipientsToInstructionsForTesting(deliveryInstructions, new ReportScheduleTaskRecipient[] { recipient }, Notifications);
			var docContact = deliveryInstructions.Recipients.Last() as DocDeliveryContact;

			AssertEquals("Test orgheader", orgHeader.PK, docContact.OrgHeaderPK);
			AssertEquals("Test org contact name", "2334", docContact.Name);
		}

		public void TestEmailFromAddressIsNotValid()
		{
			var printuser = Factory.New<GlbStaff>();
			printuser.GS_Code = "WTG";
			printuser.GS_FullName = "Mr.Bean";
			printuser.GS_EmailAddress = "wtg@test.com";

			var reportScheduleTask = Factory.New<ReportScheduleTaskForEmptyReportContingencyTesting>();
			reportScheduleTask.UserFK = printuser.PK;
			reportScheduleTask.S5_ScheduleDescription = "Test";

			var recipient = reportScheduleTask.Recipients.AddNew();
			recipient.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.Staff;
			recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			recipient.S6_EmptyReportDeliveryOptions = EmptyReportContingencyList.Codes.SendNothing;
			recipient.S6_AttachmentType = AttachmentTypeList.Codes.Xml;
			recipient.S6_EmailFromAddress = "from@test.com";

			// Will delete
			// ReportScheduleTask.cs:line 584, DeliveryMethod = EML and DeliveryAddress is empty, therefore an error is thrown and a null contact is returned.
			// Weird behaviour while debugging with the if statement on line 586, EnvProxy.Instance.IsProductionSystem returns false, but the code still enters the if statement. idk...
			recipient.EmailToRecipients.Value = "unit.test@cargowise.com";

			var contact = reportScheduleTask.CreateSingleDocDeliveryContactForTesting(recipient, Notifications);

			Assert("ScheduleRunErrors.ToString()", Notifications.AsString.Contains("Staff 'Mr.Bean' (WTG) does not have the email address \'from@test.com\'. The default sender address will be used. Please choose a valid sender address in schedule task 'Test'"));

			Notifications.Clear();
			printuser.EmailAddresses[0].GSE_EmailAddress = "from@test.com";
			contact = reportScheduleTask.CreateSingleDocDeliveryContactForTesting(recipient, Notifications);

			Assert(string.IsNullOrEmpty(Notifications.AsString));
			AssertEquals("from@test.com", contact.EmailFromAddress);

			recipient.S6_EmailFromAddress = "";
			contact = reportScheduleTask.CreateSingleDocDeliveryContactForTesting(recipient, Notifications);

			Assert(string.IsNullOrEmpty(Notifications.AsString));
			Assert(contact.EmailFromAddress.IsEmpty);
		}

		public void TestEmptyReportContingenciesForEmailDeliveries()
		{
			var contactEmail = "blah@blah.com";
			var emptyReportEmailBodyRegex = new Regex(@"Report '.+' has been run at [0-9]+\-[A-Z][a-z][a-z]\-[0-9]+ [0-9]+:[0-9]+:[0-9]+\. The resulting document was empty and therefore has not been delivered\.", RegexOptions.CultureInvariant | RegexOptions.Compiled);
			var emptyReportEmailSubjectRegex = new Regex(@".+ not delivered [0-9]+\-[A-Z][a-z][a-z]\-[0-9]+ [0-9]+:[0-9]+:[0-9]+", RegexOptions.CultureInvariant | RegexOptions.Compiled);

			SetUpPostMasterGroup();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			GenerateReportWithEmptyContingency(EmptyReportContingencyList.Codes.SendEmailNotification, contactEmail, Enterprise.Core.Constants.ContactNotifyModes.Email);
			StmPrintJob printJob = Factory.LoadTop1<StmPrintJob>(new ZQuery());
			Assert("Print job should not be present when empty report contingency is set to EML (do not deliver report, send email notification)", (printJob == null));
			AssertEquals("Email to client should be present when empty report contingency is set to EML (do not deliver report, send email notification)", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
			Assert("Empty report email notification body has incorrect content", emptyReportEmailBodyRegex.IsMatch(email.Body));
			Assert("Empty report email notification subject has incorrect content", emptyReportEmailSubjectRegex.IsMatch(email.Subject));
			AssertEquals(contactEmail, email.Recipients[0]);
			Env.OutgoingMailManager.EmailsCreated.Clear();

			GenerateReportWithEmptyContingency(EmptyReportContingencyList.Codes.SendNothing, contactEmail, Enterprise.Core.Constants.ContactNotifyModes.Email);
			AssertEquals("Email to client should not be present when empty report contingency is set to NTH (do not deliver report, do not send email notification)", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			printJob = Factory.LoadTop1<StmPrintJob>(new ZQuery());
			Assert("Print job should not be present when empty report contingency is set to NTH (do not deliver report, do not send email notification)", (printJob == null));
			Env.OutgoingMailManager.EmailsCreated.Clear();

			GenerateReportWithEmptyContingency(EmptyReportContingencyList.Codes.SendReport, contactEmail, Enterprise.Core.Constants.ContactNotifyModes.Email);
			AssertEquals("Email to client should not be present when empty report contingency is set to REP (send empty report anyway)", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			printJob = Factory.LoadTop1<StmPrintJob>(new ZQuery());
			Assert("Print job should be present when empty report contingency is set to REP (send empty report anyway)", (printJob != null));
			Env.OutgoingMailManager.EmailsCreated.Clear();
		}

		public void TestEmptyReportContingenciesForEmailDeliveriesWithCcAndBcc()
		{
			// Arrange
			const string contactEmail = "stone@stone.com";
			const string contactCcEmail = "stonecc@stonecc.com";
			const string contactBccEmail = "stonebcc@stonebcc.com";
			var emptyReportEmailSubjectRegex = new Regex(@".+ not delivered [0-9]+\-[A-Z][a-z][a-z]\-[0-9]+ [0-9]+:[0-9]+:[0-9]+", RegexOptions.CultureInvariant | RegexOptions.Compiled);
			var emptyReportEmailBodyRegex = new Regex(@"Report '.+' has been run at [0-9]+\-[A-Z][a-z][a-z]\-[0-9]+ [0-9]+:[0-9]+:[0-9]+\. The resulting document was empty and therefore has not been delivered\.", RegexOptions.CultureInvariant | RegexOptions.Compiled);
			SetUpPostMasterGroup();
			Env.OutgoingMailManager.EmailsCreated.Clear();
			// Act
			GenerateReportWithEmptyContingency(EmptyReportContingencyList.Codes.SendEmailNotification, contactEmail, Enterprise.Core.Constants.ContactNotifyModes.Email, contactCcEmail, contactBccEmail);
			// Assert
			var printJob = Factory.LoadTop1<StmPrintJob>(new ZQuery());
			Assert("Print job should not be present when empty report contingency is set to EML (do not deliver report, send email notification)", (printJob == null));
			AssertEquals("Email to client should be present when empty report contingency is set to EML (do not deliver report, send email notification)", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			Assert("Empty report email notification body has incorrect content", emptyReportEmailBodyRegex.IsMatch(email.Body));
			Assert("Empty report email notification subject has incorrect content", emptyReportEmailSubjectRegex.IsMatch(email.Subject));
			AssertEquals(contactEmail, email.Recipients[0].Email);
			AssertEquals(contactCcEmail, email.CCRecipients[0].Email);
			AssertEquals(contactBccEmail, email.BCCRecipients[0].Email);
			// Clean
			Env.OutgoingMailManager.EmailsCreated.Clear();
		}

		public void TestEmptyReportContingenciesForEmailDeliveries_AddressOverride()
		{
			var emailFaxOverride = "override@blah.com";
			SetUpPostMasterGroup();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			GenerateReportWithEmptyContingency(EmptyReportContingencyList.Codes.SendEmailNotification, emailFaxOverride, Enterprise.Core.Constants.ContactNotifyModes.Email);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals(emailFaxOverride, email.Recipients[0].Email);

			Env.OutgoingMailManager.EmailsCreated.Clear();
			GenerateReportWithEmptyContingency(EmptyReportContingencyList.Codes.SendNothing, emailFaxOverride, Enterprise.Core.Constants.ContactNotifyModes.Email);
			var printJob = Factory.LoadTop1<StmPrintJob>(new ZQuery());
			AssertEquals("No email notification was sent.)", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertNull("No email notification was sent.)", printJob);

			Env.OutgoingMailManager.EmailsCreated.Clear();
			GenerateReportWithEmptyContingency(EmptyReportContingencyList.Codes.SendReport, emailFaxOverride, Enterprise.Core.Constants.ContactNotifyModes.Email);
			printJob = Factory.LoadTop1<StmPrintJob>(new ZQuery());
			// Only the override should show
			AssertEquals("override@blah.com", printJob.EmailToRecipients.Value);

			Env.OutgoingMailManager.EmailsCreated.Clear();
		}

		public void TestEmptyReportContingenciesForPrinterDeliveries()
		{
			var contactEmail = "blah@blah.com";
			var emptyReportEmailBodyRegex = new Regex(@"Report '.+' has been run at [0-9]+\-[A-Z][a-z][a-z]\-[0-9]+ [0-9]+:[0-9]+:[0-9]+\. The resulting document was empty and therefore has not been delivered\.", RegexOptions.CultureInvariant | RegexOptions.Compiled);
			var emptyReportEmailSubjectRegex = new Regex(@".+ not delivered [0-9]+\-[A-Z][a-z][a-z]\-[0-9]+ [0-9]+:[0-9]+:[0-9]+", RegexOptions.CultureInvariant | RegexOptions.Compiled);

			SetUpPostMasterGroup();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			GenerateReportWithEmptyContingency(EmptyReportContingencyList.Codes.SendEmailNotification, contactEmail, Enterprise.Core.Constants.ContactNotifyModes.Print);
			StmPrintJob printJob = Factory.LoadTop1<StmPrintJob>(new ZQuery());
			Assert("Print job should not be present when empty report contingency is set to EML (do not deliver report, send email notification)", (printJob == null));
			AssertEquals("Email to client should be present when empty report contingency is set to EML (do not deliver report, send email notification)", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
			Assert("Empty report email notification body has incorrect content", emptyReportEmailBodyRegex.IsMatch(email.Body));
			Assert("Empty report email notification subject has incorrect content", emptyReportEmailSubjectRegex.IsMatch(email.Subject));

			AssertEquals("blah@blah.com", email.Recipients[0].Email);
			Env.OutgoingMailManager.EmailsCreated.Clear();

			GenerateReportWithEmptyContingency(EmptyReportContingencyList.Codes.SendNothing, contactEmail, Enterprise.Core.Constants.ContactNotifyModes.Print);
			AssertEquals("Email to client should not be present when empty report contingency is set to NTH (do not deliver report, do not send email notification)", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			printJob = Factory.LoadTop1<StmPrintJob>(new ZQuery());
			Assert("Print job should not be present when empty report contingency is set to NTH (do not deliver report, do not send email notification)", (printJob == null));
			Env.OutgoingMailManager.EmailsCreated.Clear();

			GenerateReportWithEmptyContingency(EmptyReportContingencyList.Codes.SendReport, contactEmail, Enterprise.Core.Constants.ContactNotifyModes.Print);
			AssertEquals("Email to client should not be present when empty report contingency is set to REP (send empty report anyway)", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			printJob = Factory.LoadTop1<StmPrintJob>(new ZQuery());
			Assert("Print job should be present when empty report contingency is set to REP (send empty report anyway)", (printJob != null));
			Env.OutgoingMailManager.EmailsCreated.Clear();
		}

		public void TestEmptyReportContingenciesForFaxDeliveries()
		{
			var contactEmail = "blah@blah.com";
			var emptyReportEmailBodyRegex = new Regex(@"Report '.+' has been run at [0-9]+/[0-9]+/[0-9]+ [0-9]+:[0-9]+:[0-9]+ [AP]M\. The resulting document was empty and therefore has not been delivered\.", RegexOptions.CultureInvariant | RegexOptions.Compiled);
			var emptyReportEmailSubjectRegex = new Regex(@".+ not delivered [0-9]+/[0-9]+/[0-9]+ [0-9]+:[0-9]+:[0-9]+ [AP]M", RegexOptions.CultureInvariant | RegexOptions.Compiled);

			SetUpPostMasterGroup();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			GenerateReportWithEmptyContingency(EmptyReportContingencyList.Codes.SendNothing, contactEmail, Enterprise.Core.Constants.ContactNotifyModes.Fax);
			AssertEquals("Email to client should not be present when empty report contingency is set to NTH (do not deliver report, do not send email notification)", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			StmPrintJob printJob = Factory.LoadTop1<StmPrintJob>(new ZQuery());
			Assert("Print job should not be present when empty report contingency is set to NTH (do not deliver report, do not send email notification)", (printJob == null));
			Env.OutgoingMailManager.EmailsCreated.Clear();

			GenerateReportWithEmptyContingency(EmptyReportContingencyList.Codes.SendReport, contactEmail, Enterprise.Core.Constants.ContactNotifyModes.Fax);
			AssertEquals("Email to client should not be present when empty report contingency is set to REP (send empty report anyway)", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			printJob = Factory.LoadTop1<StmPrintJob>(new ZQuery());
			Assert("Print job should be present when empty report contingency is set to REP (send empty report anyway)", (printJob != null));
			Env.OutgoingMailManager.EmailsCreated.Clear();
		}

		public void TestGetDeliveryInstructionsWithCoverNote()
		{
			DeliveryInstructions parentInstructions = new DeliveryInstructions();

			parentInstructions.CoverNote = "Squish";
			parentInstructions.IncludeCoverNote = true;

			ReportSerializationInfo reportInfo = new ReportSerializationInfo(parentInstructions, null);

			CreateRecipient(ScheduledReportDeliveryRecipientConstants.RecipientType.Staff, Core.Constants.ContactNotifyModes.Print, GlbStaff.CurrentUser.GS_Code).S6_SQ = ZGuid.NewZGuid();
			CreateRecipient(ScheduledReportDeliveryRecipientConstants.RecipientType.Staff, Core.Constants.ContactNotifyModes.Print, GlbStaff.CurrentUser.GS_Code).S6_SQ = ZGuid.NewZGuid();

			DeliveryInstructions[] instructions = ScheduleTask.GetDeliveryInstructionsForTesting(reportInfo, Pack, Notifications);
			AssertEquals("GetDeliveryInstructions().Length", 2, instructions.Length);
			AssertEquals("GetDeliveryInstructions()[0].CoverNote", "Squish", instructions[0].CoverNote);
			AssertEquals("GetDeliveryInstructions()[0].IncludeCoverNote", true, instructions[0].IncludeCoverNote);
			AssertEquals("GetDeliveryInstructions()[1].CoverNote", "Squish", instructions[1].CoverNote);
			AssertEquals("GetDeliveryInstructions()[1].IncludeCoverNote", true, instructions[1].IncludeCoverNote);
		}

		public void TestGetDeliveryInstructionsWithLanguage()
		{
			var parentInstructions = new DeliveryInstructions();

			parentInstructions.Language = Core.SharedConstants.Languages.ChineseSimplified;

			var reportInfo = new ReportSerializationInfo(parentInstructions, null);

			CreateRecipient(ScheduledReportDeliveryRecipientConstants.RecipientType.Staff, Core.Constants.ContactNotifyModes.Print, GlbStaff.CurrentUser.GS_Code).S6_SQ = ZGuid.NewZGuid();
			CreateRecipient(ScheduledReportDeliveryRecipientConstants.RecipientType.Staff, Core.Constants.ContactNotifyModes.Print, GlbStaff.CurrentUser.GS_Code).S6_SQ = ZGuid.NewZGuid();

			var instructions = ScheduleTask.GetDeliveryInstructionsForTesting(reportInfo, Pack, Notifications);
			AssertEquals("GetDeliveryInstructions().Length", 2, instructions.Length);
			AssertEquals("GetDeliveryInstructions()[0].Language", Core.SharedConstants.Languages.ChineseSimplified, instructions[0].Language);
			AssertEquals("GetDeliveryInstructions()[1].Language", Core.SharedConstants.Languages.ChineseSimplified, instructions[1].Language);

			Pack.Language = Core.SharedConstants.Languages.ChineseSimplified;
			var report = (Report)Pack.AddNew();
			report.SetParent(Pack);
			reportInfo = new ReportSerializationInfo(null, report);

			instructions = ScheduleTask.GetDeliveryInstructionsForTesting(reportInfo, Pack, Notifications);
			AssertEquals("GetDeliveryInstructions().Length", 2, instructions.Length);
			AssertEquals("GetDeliveryInstructions()[0].Language", Core.SharedConstants.Languages.ChineseSimplified, instructions[0].Language);
			AssertEquals("GetDeliveryInstructions()[1].Language", Core.SharedConstants.Languages.ChineseSimplified, instructions[1].Language);
		}

		public void TestSetAddtionalPropertiesWithEMLDeliveryMethod()
		{
			using (SystemDataRegistry.Instance.AllocateReportOverEmailAttachmentLimitToEDocs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var reportCommand = DocumentEngineTestHelper.CreateReportCommandWithExcelTemplate("TestSetAddtionalPropertiesWithEMLDeliveryMethod1", SimpleTestTemplate, Factory);
				reportCommand.SU_IsSystemDefined = true;
				using (var documentPack = new DocumentPack(reportCommand))
				{
					var report = documentPack[0] as Report;
					report.PrepareForRender();

					var deliveryInstructions = new DeliveryInstructions(documentPack);
					deliveryInstructions.Recipients.RemoveAndDeleteAll();

					var recipient = deliveryInstructions.Recipients.AddNew();
					recipient.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
					recipient.AttachmentType = "PDF";
					recipient.Name = "Test";
					recipient.Email = "test@cargowise.com";

					var reportScheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
					reportScheduleTask.PopulateDefaultsFromDeliveryInstructions(deliveryInstructions);
					reportScheduleTask.S5_ParentID = reportCommand.PK;
					reportScheduleTask.S5_ScheduleState = reportScheduleTask.SerializeForTesting(deliveryInstructions, report);

					var stmReportRun = Factory.NewWithValidTestData<StmReportRun>();
					stmReportRun.RRI_S5_Schedule = reportScheduleTask.PK;
					reportScheduleTask.StmReportRun = stmReportRun;
					Factory.Save();

					reportScheduleTask.Run();
					var printJobs = Factory.Load<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_EmailAttachmentFormat, "PDF"));
					AssertEquals("1 print job should be created", 1, printJobs.Length);
					AssertEquals("SP_ParentTableName should be", "StmReportRun", printJobs[0].SP_ParentTableName);
					AssertEquals("SP_ParentGuid should be", reportScheduleTask.StmReportRun.PK, printJobs[0].SP_ParentGuid);
					AssertEquals("SP_RelatedBusinessContext should be", "RTS", printJobs[0].SP_RelatedBusinessContext);
					AssertEquals("SP_EmailAttachmentFormat should be", "PDF", printJobs[0].SP_EmailAttachmentFormat);
					AssertEquals("SP_DocumentType should be", "SREP", printJobs[0].SP_DocumentType);
				}
			}

			using (SystemDataRegistry.Instance.AllocateReportOverEmailAttachmentLimitToEDocs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var reportCommand = DocumentEngineTestHelper.CreateReportCommandWithExcelTemplate("TestSetAddtionalPropertiesWithEMLDeliveryMethod2", SimpleTestTemplate, Factory);
				reportCommand.SU_IsSystemDefined = true;
				using (var documentPack = new DocumentPack(reportCommand))
				{
					var report = documentPack[0] as Report;
					report.PrepareForRender();

					var deliveryInstructions = new DeliveryInstructions(documentPack);
					deliveryInstructions.Recipients.RemoveAndDeleteAll();

					var recipient = deliveryInstructions.Recipients.AddNew();
					recipient.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
					recipient.AttachmentType = "XLS";
					recipient.Name = "Test";
					recipient.Email = "test@cargowise.com";

					var reportScheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
					reportScheduleTask.PopulateDefaultsFromDeliveryInstructions(deliveryInstructions);
					reportScheduleTask.S5_ParentID = reportCommand.PK;
					reportScheduleTask.S5_ScheduleState = reportScheduleTask.SerializeForTesting(deliveryInstructions, report);

					var stmReportRun = Factory.NewWithValidTestData<StmReportRun>();
					stmReportRun.RRI_S5_Schedule = reportScheduleTask.PK;
					reportScheduleTask.StmReportRun = stmReportRun;
					Factory.Save();

					reportScheduleTask.Run();
					var printJobs = Factory.Load<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_EmailAttachmentFormat, "XLS"));
					AssertEquals("1 print job should be created", 1, printJobs.Length);
					AssertNullOrEmpty("SP_ParentTableName should be empty", printJobs[0].SP_ParentTableName);
					AssertEquals("SP_ParentGuid should be empty", ZGuid.Empty, printJobs[0].SP_ParentGuid);
					AssertNullOrEmpty("SP_RelatedBusinessContext should be empty", printJobs[0].SP_RelatedBusinessContext);
					AssertNullOrEmpty("SP_DocumentType should be empty", printJobs[0].SP_DocumentType);
				}
			}
		}

		[TestDate(2024, 4, 20)]
		public void TestFillData()
		{
			var taskData = new ReportScheduleTaskData
			{
				Branch = Env.CurrentBranchPK,
				IsActive = true,
				ScheduleDescription = "test",
				UserFk = Env.CurrentUserPK,
				NextRunTimeLocal = new DateTime(2024, 4, 22),
				Recurrence = new ScheduleRecurrenceData
				{
					RecurrenceType = ScheduleRecurrenceType.Daily,
					IsWeekDayOnly = true,
					StartDateLocal = new DateTime(2024, 4, 20),
					EndDateLocal = new DateTime(2024, 5, 20),
					EndAfterCount = 2,
					RecurringStartTimeLocal = new DateTime(2024, 4, 20, 10, 20, 0),
					RecurringStartTimeZone = 8,
				},
			};

			var task = Factory.New<ReportScheduleTask>();
			task.FillData(taskData);

			AssertEquals(taskData.Branch, task.S5_GB);
			AssertEquals(taskData.IsActive, task.S5_IsActive);
			AssertEquals(taskData.ScheduleDescription, task.S5_ScheduleDescription);
			AssertEquals(taskData.UserFk, task.UserFK);
			AssertEquals(new DateTime(2024, 4, 21, 16, 0, 0), task.CalcNextRunTimeLocal);

			var recurringStartTimeUtc = new DateTimeOffset(taskData.Recurrence.RecurringStartTimeLocal.Value, TimeSpan.FromHours(taskData.Recurrence.RecurringStartTimeZone)).UtcDateTime;
			var expectedRecurringStartTimeLocal = Env.Time.GetLocalTimeFromUtc(recurringStartTimeUtc);
			AssertEquals(new DateTime(1900, 1, 1, expectedRecurringStartTimeLocal.Hour, expectedRecurringStartTimeLocal.Minute, 0), task.Recurrence.RecurringStartTimeLocal);

			AssertEquals(new DateTime(2024, 4, 19, 16, 0, 0), task.S5_StartDate);
			AssertEquals(ZDateTime.Empty, task.S5_EndDate);
			AssertEquals(taskData.Recurrence.EndAfterCount, task.Recurrence.EndAfterCount);

			taskData.Recurrence.EndAfterCount = 0;
			task.FillData(taskData);
			AssertEquals(new DateTime(2024, 5, 19, 16, 0, 0), task.S5_EndDate);
		}

		[TestDate(2024, 4, 20)]
		public void TestFillData_Daily()
		{
			var taskData = new ReportScheduleTaskData
			{
				Branch = Env.CurrentBranchPK,
				IsActive = true,
				ScheduleDescription = "test",
				UserFk = Env.CurrentUserPK,
				NextRunTimeLocal = new DateTime(2024, 4, 22),
				Recurrence = new ScheduleRecurrenceData
				{
					RecurrenceType = ScheduleRecurrenceType.Daily,
					IsWeekDayOnly = true,
					DayOfAccountingPeriod = 2,
					DayOfMonth = 3,
					IsAccountingDay = true,
					EveryMonthNumber = 4,
					IsAccountingLastDay = true,
					IsMonthlyDay = true,
					IsYearlyDay = true,
					IsMonthlyLastDay = true,
					MonthNumber = 5,
					TaskPeriodCount = 6,
					WeekDayList = new bool[7],
					WeekDayNumber = 5,
					WeekDayOccurrenceNumber = 3,
				},
			};

			var task = Factory.New<ReportScheduleTask>();
			task.FillData(taskData);

			AssertEquals(taskData.Branch, task.S5_GB);
			AssertEquals(taskData.IsActive, task.S5_IsActive);
			AssertEquals(taskData.ScheduleDescription, task.S5_ScheduleDescription);
			AssertEquals(taskData.UserFk, task.UserFK);
			AssertEquals(taskData.NextRunTimeLocal, task.CalcNextRunTimeLocal);

			AssertEquals("Daily data should be populated.", taskData.Recurrence.RecurrenceType, task.Recurrence.TaskPeriod);
			AssertEquals("Daily data should be populated.", taskData.Recurrence.IsWeekDayOnly, task.Recurrence.WeekDaysOnly);
			AssertEquals("Daily data should be populated.", 0, task.Recurrence.TaskPeriodCount);

			AssertEquals("Non Daily data should be remained as default value.", new string('N', 7), task.Recurrence.DayList);
			AssertEquals("Non Daily data should be remained as default value.", false, task.Recurrence.MonthlyDay);
			AssertEquals("Non Daily data should be remained as default value.", false, task.Recurrence.MonthLastDay);
			AssertEquals("Non Daily data should be remained as default value.", 20, task.Recurrence.DayOfMonth);
			AssertEquals("Non Daily data should be remained as default value.", "0", task.Recurrence.WeekCountAsString);
			AssertEquals("Non Daily data should be remained as default value.", "", task.Recurrence.DayName);
			AssertEquals("Non Daily data should be remained as default value.", false, task.Recurrence.AccountingDay);
			AssertEquals("Non Daily data should be remained as default value.", false, task.Recurrence.AccountingLastDay);
			AssertEquals("Non Daily data should be remained as default value.", 0, task.Recurrence.DayOfAccountingPeriod);
			AssertEquals("Non Daily data should be remained as default value.", false, task.Recurrence.YearlyEvery);
			AssertEquals("Non Daily data should be remained as default value.", "", task.Recurrence.EveryMonthNumberDayAsString);
			AssertEquals("Non Daily data should be remained as default value.", "", task.Recurrence.MonthNumberAsString);

			taskData.Recurrence.IsWeekDayOnly = false;
			task.FillData(taskData);
			AssertEquals("Daily data should be populated.", taskData.Recurrence.TaskPeriodCount, task.Recurrence.TaskPeriodCount);
		}

		[TestDate(2024, 4, 20)]
		public void TestFillData_Weekly()
		{
			var taskData = new ReportScheduleTaskData
			{
				Branch = Env.CurrentBranchPK,
				IsActive = true,
				ScheduleDescription = "test",
				UserFk = Env.CurrentUserPK,
				NextRunTimeLocal = new DateTime(2024, 4, 22),
				Recurrence = new ScheduleRecurrenceData
				{
					RecurrenceType = ScheduleRecurrenceType.Weekly,
					IsWeekDayOnly = false,
					DayOfAccountingPeriod = 2,
					DayOfMonth = 3,
					IsAccountingDay = true,
					EveryMonthNumber = 4,
					IsAccountingLastDay = true,
					IsMonthlyDay = true,
					IsYearlyDay = true,
					IsMonthlyLastDay = true,
					MonthNumber = 5,
					TaskPeriodCount = 6,
					WeekDayList = new bool[7] { false, false, false, true, false, false, false },
					WeekDayNumber = 5,
					WeekDayOccurrenceNumber = 3,
				},
			};

			var task = Factory.New<ReportScheduleTask>();
			task.FillData(taskData);

			AssertEquals(taskData.Branch, task.S5_GB);
			AssertEquals(taskData.IsActive, task.S5_IsActive);
			AssertEquals(taskData.ScheduleDescription, task.S5_ScheduleDescription);
			AssertEquals(taskData.UserFk, task.UserFK);
			AssertEquals(taskData.NextRunTimeLocal, task.CalcNextRunTimeLocal);

			AssertEquals("Weekly data should be populated.", "NNNYNNN", task.Recurrence.DayList);
			AssertEquals("Weekly data should be populated.", taskData.Recurrence.RecurrenceType, task.Recurrence.TaskPeriod);
			AssertEquals("Weekly data should be populated.", taskData.Recurrence.TaskPeriodCount, task.Recurrence.TaskPeriodCount);

			AssertEquals("Non Weekly data should be remained as default value.", true, task.Recurrence.WeekDaysOnly);
			AssertEquals("Non Weekly data should be remained as default value.", false, task.Recurrence.MonthlyDay);
			AssertEquals("Non Weekly data should be remained as default value.", false, task.Recurrence.MonthLastDay);
			AssertEquals("Non Weekly data should be remained as default value.", 20, task.Recurrence.DayOfMonth);
			AssertEquals("Non Weekly data should be remained as default value.", "0", task.Recurrence.WeekCountAsString);
			AssertEquals("Non Weekly data should be remained as default value.", "", task.Recurrence.DayName);
			AssertEquals("Non Weekly data should be remained as default value.", false, task.Recurrence.AccountingDay);
			AssertEquals("Non Weekly data should be remained as default value.", false, task.Recurrence.AccountingLastDay);
			AssertEquals("Non Weekly data should be remained as default value.", 0, task.Recurrence.DayOfAccountingPeriod);
			AssertEquals("Non Weekly data should be remained as default value.", false, task.Recurrence.YearlyEvery);
			AssertEquals("Non Weekly data should be remained as default value.", "", task.Recurrence.EveryMonthNumberDayAsString);
			AssertEquals("Non Weekly data should be remained as default value.", "", task.Recurrence.MonthNumberAsString);
		}

		[TestDate(2024, 4, 20)]
		public void TestFillData_Monthly()
		{
			var taskData = new ReportScheduleTaskData
			{
				Branch = Env.CurrentBranchPK,
				IsActive = true,
				ScheduleDescription = "test",
				UserFk = Env.CurrentUserPK,
				NextRunTimeLocal = new DateTime(2024, 4, 22),
				Recurrence = new ScheduleRecurrenceData
				{
					RecurrenceType = ScheduleRecurrenceType.Monthly,
					IsWeekDayOnly = false,
					DayOfAccountingPeriod = 2,
					DayOfMonth = 3,
					IsAccountingDay = true,
					EveryMonthNumber = 4,
					IsAccountingLastDay = true,
					IsMonthlyDay = true,
					IsYearlyDay = true,
					IsMonthlyLastDay = true,
					MonthNumber = 5,
					TaskPeriodCount = 6,
					WeekDayList = new bool[7],
					WeekDayNumber = 5,
					WeekDayOccurrenceNumber = 3,
				},
			};

			var task = Factory.New<ReportScheduleTask>();
			task.FillData(taskData);

			AssertEquals(taskData.Branch, task.S5_GB);
			AssertEquals(taskData.IsActive, task.S5_IsActive);
			AssertEquals(taskData.ScheduleDescription, task.S5_ScheduleDescription);
			AssertEquals(taskData.UserFk, task.UserFK);
			AssertEquals(taskData.NextRunTimeLocal, task.CalcNextRunTimeLocal);

			AssertEquals("Monthly data should be populated.", taskData.Recurrence.RecurrenceType, task.Recurrence.TaskPeriod);
			AssertEquals("Monthly data should be populated.", taskData.Recurrence.TaskPeriodCount, task.Recurrence.TaskPeriodCount);
			AssertEquals("Monthly data should be populated.", taskData.Recurrence.IsMonthlyDay, task.Recurrence.MonthlyDay);
			AssertEquals("Monthly data should be populated.", taskData.Recurrence.IsMonthlyLastDay, task.Recurrence.MonthLastDay);
			AssertEquals("Monthly data should be populated.", 20, task.Recurrence.DayOfMonth);
			AssertEquals("Monthly data should be populated.", "0", task.Recurrence.WeekCountAsString);
			AssertEquals("Monthly data should be populated.", "", task.Recurrence.DayName);

			AssertEquals("Non Monthly data should be remained as default value.", new string('N', 7), task.Recurrence.DayList);
			AssertEquals("Non Monthly data should be remained as default value.", true, task.Recurrence.WeekDaysOnly);
			AssertEquals("Non Monthly data should be remained as default value.", false, task.Recurrence.AccountingDay);
			AssertEquals("Non Monthly data should be remained as default value.", false, task.Recurrence.AccountingLastDay);
			AssertEquals("Non Monthly data should be remained as default value.", 0, task.Recurrence.DayOfAccountingPeriod);
			AssertEquals("Non Monthly data should be remained as default value.", false, task.Recurrence.YearlyEvery);
			AssertEquals("Non Monthly data should be remained as default value.", "", task.Recurrence.EveryMonthNumberDayAsString);
			AssertEquals("Non Monthly data should be remained as default value.", "", task.Recurrence.MonthNumberAsString);

			taskData.Recurrence.IsMonthlyLastDay = false;
			task.FillData(taskData);
			AssertEquals("Monthly data should be populated.", taskData.Recurrence.DayOfMonth, task.Recurrence.DayOfMonth);

			taskData.Recurrence.IsMonthlyDay = false;
			task.FillData(taskData);
			AssertEquals("Monthly data should be populated.", taskData.Recurrence.WeekDayOccurrenceNumber.ToString(), task.Recurrence.WeekCountAsString);
			AssertEquals("Monthly data should be populated.", "THU", task.Recurrence.DayName);
		}

		[TestDate(2018, 2, 20)]
		public void TestFillData_AccountingPeriod()
		{
			var taskData = new ReportScheduleTaskData
			{
				Branch = Env.CurrentBranchPK,
				IsActive = true,
				ScheduleDescription = "test",
				UserFk = Env.CurrentUserPK,
				NextRunTimeLocal = new DateTime(2024, 4, 22),
				Recurrence = new ScheduleRecurrenceData
				{
					RecurrenceType = ScheduleRecurrenceType.AccountingPeriod,
					IsWeekDayOnly = false,
					DayOfAccountingPeriod = 2,
					DayOfMonth = 3,
					IsAccountingDay = true,
					EveryMonthNumber = 4,
					IsAccountingLastDay = true,
					IsMonthlyDay = true,
					IsYearlyDay = true,
					IsMonthlyLastDay = true,
					MonthNumber = 5,
					TaskPeriodCount = 6,
					WeekDayList = new bool[7],
					WeekDayNumber = 5,
					WeekDayOccurrenceNumber = 3,
				},
			};

			var task = Factory.New<ReportScheduleTask>();
			task.FillData(taskData);

			AssertEquals(taskData.Branch, task.S5_GB);
			AssertEquals(taskData.IsActive, task.S5_IsActive);
			AssertEquals(taskData.ScheduleDescription, task.S5_ScheduleDescription);
			AssertEquals(taskData.UserFk, task.UserFK);
			AssertEquals(taskData.NextRunTimeLocal, task.CalcNextRunTimeLocal);

			AssertEquals("Accounting Period data should be populated.", taskData.Recurrence.RecurrenceType, task.Recurrence.TaskPeriod);
			AssertEquals("Accounting Period data should be populated.", taskData.Recurrence.TaskPeriodCount, task.Recurrence.TaskPeriodCount);

			AssertEquals("Accounting Period data should be populated.", taskData.Recurrence.IsAccountingDay, task.Recurrence.AccountingDay);
			AssertEquals("Accounting Period data should be populated.", taskData.Recurrence.IsAccountingLastDay, task.Recurrence.AccountingLastDay);
			AssertEquals("Accounting Period data should be populated.", 0, task.Recurrence.DayOfAccountingPeriod);

			AssertEquals("Non Accounting Period data should be remained as default value.", new string('N', 7), task.Recurrence.DayList);
			AssertEquals("Non Accounting Period data should be remained as default value.", true, task.Recurrence.WeekDaysOnly);
			AssertEquals("Non Accounting Period data should be remained as default value.", false, task.Recurrence.MonthlyDay);
			AssertEquals("Non Accounting Period data should be remained as default value.", false, task.Recurrence.MonthLastDay);
			AssertEquals("Non Accounting Period data should be remained as default value.", 20, task.Recurrence.DayOfMonth);
			AssertEquals("Non Accounting Period data should be remained as default value.", "0", task.Recurrence.WeekCountAsString);
			AssertEquals("Non Accounting Period data should be remained as default value.", "", task.Recurrence.DayName);
			AssertEquals("Non Accounting Period data should be remained as default value.", false, task.Recurrence.YearlyEvery);
			AssertEquals("Non Accounting Period data should be remained as default value.", "", task.Recurrence.EveryMonthNumberDayAsString);
			AssertEquals("Non Accounting Period data should be remained as default value.", "", task.Recurrence.MonthNumberAsString);

			CreateAccPeriodTestData();
			taskData.Recurrence.IsAccountingLastDay = false;
			task.FillData(taskData);
			AssertEquals("Accounting Period data should be populated.", taskData.Recurrence.DayOfAccountingPeriod, task.Recurrence.DayOfAccountingPeriod);

			taskData.Recurrence.IsAccountingDay = false;
			task.FillData(taskData);
			AssertEquals("Accounting Period data should be populated.", taskData.Recurrence.WeekDayOccurrenceNumber.ToString(), task.Recurrence.WeekCountAsString);
			AssertEquals("Accounting Period data should be populated.", "THU", task.Recurrence.DayName);
		}

		[TestDate(2024, 4, 20)]
		public void TestFillData_Yearly()
		{
			var taskData = new ReportScheduleTaskData
			{
				Branch = Env.CurrentBranchPK,
				IsActive = true,
				ScheduleDescription = "test",
				UserFk = Env.CurrentUserPK,
				NextRunTimeLocal = new DateTime(2024, 4, 22),
				Recurrence = new ScheduleRecurrenceData
				{
					RecurrenceType = ScheduleRecurrenceType.Yearly,
					IsWeekDayOnly = false,
					DayOfAccountingPeriod = 2,
					DayOfMonth = 3,
					IsAccountingDay = true,
					EveryMonthNumber = 4,
					IsAccountingLastDay = true,
					IsMonthlyDay = true,
					IsYearlyDay = true,
					IsMonthlyLastDay = true,
					MonthNumber = 5,
					TaskPeriodCount = 6,
					WeekDayList = new bool[7],
					WeekDayNumber = 5,
					WeekDayOccurrenceNumber = 3,
				},
			};

			var task = Factory.New<ReportScheduleTask>();
			task.FillData(taskData);

			AssertEquals(taskData.Branch, task.S5_GB);
			AssertEquals(taskData.IsActive, task.S5_IsActive);
			AssertEquals(taskData.ScheduleDescription, task.S5_ScheduleDescription);
			AssertEquals(taskData.UserFk, task.UserFK);
			AssertEquals(taskData.NextRunTimeLocal, task.CalcNextRunTimeLocal);

			AssertEquals("Yearly data should be populated.", taskData.Recurrence.RecurrenceType, task.Recurrence.TaskPeriod);
			AssertEquals("Yearly data should be populated.", taskData.Recurrence.TaskPeriodCount, task.Recurrence.TaskPeriodCount);
			AssertEquals("Yearly data should be populated.", true, task.Recurrence.YearlyEvery);
			AssertEquals("Yearly data should be populated.", taskData.Recurrence.EveryMonthNumber.ToString(), task.Recurrence.EveryMonthNumberDayAsString);
			AssertEquals("Yearly data should be populated.", taskData.Recurrence.DayOfMonth, task.Recurrence.DayOfMonth);
			AssertEquals("Yearly data should be populated.", "4", task.Recurrence.MonthNumberAsString);
			AssertEquals("Yearly data should be populated.", "0", task.Recurrence.WeekCountAsString);
			AssertEquals("Yearly data should be populated.", "", task.Recurrence.DayName);

			AssertEquals("Non Yearly data should be remained as default value.", new string('N', 7), task.Recurrence.DayList);
			AssertEquals("Non Yearly data should be remained as default value.", true, task.Recurrence.WeekDaysOnly);
			AssertEquals("Non Yearly data should be remained as default value.", false, task.Recurrence.MonthlyDay);
			AssertEquals("Non Yearly data should be remained as default value.", false, task.Recurrence.MonthLastDay);
			AssertEquals("Non Yearly data should be remained as default value.", false, task.Recurrence.AccountingDay);
			AssertEquals("Non Yearly data should be remained as default value.", false, task.Recurrence.AccountingLastDay);
			AssertEquals("Non Yearly data should be remained as default value.", 0, task.Recurrence.DayOfAccountingPeriod);

			taskData.Recurrence.IsYearlyDay = false;
			task.FillData(taskData);

			AssertEquals("Yearly data should be populated.", taskData.Recurrence.MonthNumber.ToString(), task.Recurrence.MonthNumberAsString);
			AssertEquals("Yearly data should be populated.", taskData.Recurrence.WeekDayOccurrenceNumber.ToString(), task.Recurrence.WeekCountAsString);
			AssertEquals("Yearly data should be populated.", "THU", task.Recurrence.DayName);
		}

		public void TestFillData_InvalidTaskPeriod()
		{
			var taskData = new ReportScheduleTaskData
			{
				Branch = Env.CurrentBranchPK,
				IsActive = true,
				ScheduleDescription = "test",
				UserFk = Env.CurrentUserPK,
				NextRunTimeLocal = new DateTime(2024, 4, 22),
				Recurrence = new ScheduleRecurrenceData
				{
					RecurrenceType = "Z",
					IsWeekDayOnly = true,
				},
			};
			var runningError = new ReportRunningError();
			var task = Factory.New<ReportScheduleTask>();
			task.FillData(taskData, runningError);

			AssertEquals(ReportServiceErrorType.ValidationError, runningError.ErrorType);
			AssertEquals("Invalid Task Period", runningError.Errors[0]);
		}

		public void TestFillRecipientsData()
		{
			var task = Factory.NewWithValidTestData<ReportScheduleTask>();
			var recipient1 = task.Recipients.AddNew();
			recipient1.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.Staff;
			recipient1.S6_GS_NKRecipient = GlbStaff.CurrentUser.GS_Code;
			recipient1.S6_AttachmentType = AttachmentTypeList.Codes.Pdf;
			recipient1.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.EDoc;

			var recipient2 = task.Recipients.AddNew();
			recipient2.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;
			recipient2.S6_AttachmentType = OrgConstants.AttachmentType.TIF;
			recipient2.ToFaxOrEmail = "12345678";

			task.Recipients.Add(recipient1);
			task.Recipients.Add(recipient2);

			var recipientsData = new List<ReportScheduleRecipientData>
			{
				new()
				{
					Identifier = recipient1.PK.ToGuid(),
					DeliveryRecipientType = ScheduledReportDeliveryRecipientConstants.RecipientType.Contact,
					S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Email,
					ToFaxOrEmail = "a@a.com",
					S6_AttachmentType = AttachmentTypeList.Codes.Xls,
				},
				new()
				{
					S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Email,
					DeliveryRecipientType = ScheduledReportDeliveryRecipientConstants.RecipientType.Staff,
					S6_GS_NKRecipient = GlbStaff.CurrentUser.GS_Code,
					ToFaxOrEmail = "test@test.com" ,
					S6_AttachmentType = AttachmentTypeList.Codes.Xlsx,
				},
			};

			task.FillRecipientsData(recipientsData);

			AssertEquals(2, task.Recipients.Count);
			CombineAssertions("Recipient 1 should be updated.",() =>
			{
				AssertEquals("PK",recipientsData[0].Identifier, recipient1.PK);
				AssertEquals("DeliveryRecipientType", recipientsData[0].DeliveryRecipientType, ScheduledReportDeliveryRecipientConstants.RecipientType.Contact);
				AssertEquals("DeliveryMethod", recipientsData[0].S6_DeliveryMethod, Core.Constants.ContactNotifyModes.Email);
				AssertEquals("ToFaxOrEmail", recipientsData[0].ToFaxOrEmail, "a@a.com");
				AssertEquals("AttachmentType", recipientsData[0].S6_AttachmentType, AttachmentTypeList.Codes.Xls);
			});

			CombineAssertions("Recipient 2 should be updated.", () =>
			{
				AssertNotEquals("Previous recipient 2 should be deleted.",recipientsData[1].Identifier, recipient2.PK);
				AssertEquals("DeliveryRecipientType", recipientsData[1].DeliveryRecipientType, ScheduledReportDeliveryRecipientConstants.RecipientType.Staff);
				AssertEquals("DeliveryMethod", recipientsData[1].S6_DeliveryMethod, Core.Constants.ContactNotifyModes.Email);
				AssertEquals("ToFaxOrEmail", recipientsData[1].ToFaxOrEmail, "test@test.com");
				AssertEquals("AttachmentType", recipientsData[1].S6_AttachmentType, AttachmentTypeList.Codes.Xlsx);
			});
		}

		[TestDate(2024, 9, 25)]
		public void TestWhenRecurrenceIsAccountingPeriodAndPeriodDoesNotExist()
		{
			TestCaseHelper.ClearTable(AccPeriodManagement.Schema.TableName);

			var reportScheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			var printerPK = ZGuid.NewZGuid();
			Instructions.PrinterDelivery.PrintQueuePK = printerPK;
			reportScheduleTask.PopulateDefaultsFromDeliveryInstructions(Instructions);
			reportScheduleTask.Branch.GB_RL_NKHomePort = "AUSYD";
			reportScheduleTask.S5_StartDate = Env.Time.GetUtcFromLocalTime(new ZDateTime(2025, 1, 1).ToDateTime());
			reportScheduleTask.Recurrence.TaskPeriod = ScheduleRecurrenceType.AccountingPeriod;
			reportScheduleTask.Recurrence.WeekDayOccurrenceNumber = 0;
			reportScheduleTask.Recurrence.DayOfAccountingPeriod = 37;
			reportScheduleTask.S5_NextScheduledPrintRunTimeUtc = Env.Time.GetUtcFromLocalTime(ZDateTime.Today.AddDays(-5).ToDateTime());
			reportScheduleTask.S5_DailyStartTime = new ZDateTime(2025, 1, 1, 10, 50, 0);
			reportScheduleTask.NextRunTime = new ZDateTime(2023, 12, 31);

			AssertNoExceptionThrown(Factory.Save);
		}

		public void TestExtractData()
		{
			var time = new DateTime(2024, 12, 23, 13, 37, 0);
			var task = Factory.NewWithValidTestData<ReportScheduleTask>();
			task.UserFK = Env.CurrentUserPK;
			task.S5_DateScheduleFirstRun = time;
			task.S5_ScheduleActualRunCount = 1;
			task.CalcNextRunTimeLocal = time;
			task.Branch.GB_RL_NKHomePort = Env.Instance.CurrentNKUNLOCO;

			var actual = task.ExtractData();
			AssertEquals(task.S5_GB, actual.Branch);
			AssertEquals(task.UserFK, actual.UserFk);
			AssertEquals(task.S5_DateScheduleFirstRun.ToDateTime(), actual.DateScheduleFirstRun);
			AssertEquals(task.CalcNextRunTimeLocal.ToDateTime(), actual.NextRunTimeLocal);
		}

		public void TestRunReportScheduleTaskNotificationsWhenNoActiveRecipient()
		{
			var licenceType = DatabaseTypes.Codes.Production;
			LicenceTypeChanger.SetSystemLicence(licenceType);
			AssertEquals(true, Env.Instance.IsProductionSystem);

			ScheduleTask.S5_ScheduleDescription = "Test Report 1";
			var emptyReportInfo = new ReportSerializationInfo(null, null);
			AssertEquals("Length", 0, ScheduleTask.GetDeliveryInstructionsForTesting(emptyReportInfo, Pack, Notifications).Length);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = CreateContact(org, "ContactName", "contact@org.com", "contactfax");

			var recipient = ScheduleTask.Recipients.AddNew();
			recipient.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.Contact;
			recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			recipient.ContactName = contact.OC_ContactName;
			recipient.S6_OC = contact.PK;

			contact.OC_IsActive = false;

			var instructions = ScheduleTask.GetDeliveryInstructionsAndAddWarningsIfEmptyForTesting(Notifications, Pack, emptyReportInfo);
			AssertEquals("No active recipients found for Scheduled Report 'Test Report 1'.", Notifications.AsString.Trim());
		}

		public void TestCreateSingleDocDeliveryContactCanGetStaffRecipient()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "WTG";
			staff.GS_FullName = "Mr.Bean";
			staff.GS_EmailAddress = "wtg@test.com";

			var reportScheduleTask = Factory.New<ReportScheduleTaskForEmptyReportContingencyTesting>();
			reportScheduleTask.S5_ScheduleDescription = "Test";

			var recipient = reportScheduleTask.Recipients.AddNew();
			recipient.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.Staff;
			recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			recipient.S6_EmptyReportDeliveryOptions = EmptyReportContingencyList.Codes.SendNothing;
			recipient.S6_AttachmentType = AttachmentTypeList.Codes.Xml;
			recipient.S6_EmailFromAddress = "from@test.com";
			recipient.S6_GS_NKRecipient = staff.GS_Code;

			var docDeliveryContact = reportScheduleTask.CreateSingleDocDeliveryContactForTesting(recipient, Notifications);
			AssertEquals(docDeliveryContact.Staff.PK, staff.PK);
		}

		public void TestCreateSingleDocDeliveryContactCanGetContactRecipient()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgContact = orgHeader.Contacts.AddNew();
			orgContact.OC_ContactName = "Test";
			orgContact.OC_Email = "wtg@test.com";

			var reportScheduleTask = Factory.New<ReportScheduleTaskForEmptyReportContingencyTesting>();
			reportScheduleTask.S5_ScheduleDescription = "Test";

			var recipient = reportScheduleTask.Recipients.AddNew();
			recipient.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.Contact;
			recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			recipient.S6_EmptyReportDeliveryOptions = EmptyReportContingencyList.Codes.SendNothing;
			recipient.S6_AttachmentType = AttachmentTypeList.Codes.Xml;
			recipient.S6_EmailFromAddress = "from@test.com";
			recipient.S6_OH = orgHeader.PK;
			recipient.ContactName = orgContact.OC_ContactName;

			var docDeliveryContact = reportScheduleTask.CreateSingleDocDeliveryContactForTesting(recipient, Notifications);
			AssertEquals(docDeliveryContact.Contact.PK, orgContact.PK);
		}

		public static ZGuid TestReportPK => new ZGuid("B244B421-D343-407A-BD09-3EF80EDB4C6B");

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		ExcelTemplateForUnitTesting simpleTestTemplate;
		ExcelTemplateForUnitTesting SimpleTestTemplate
		{
			get
			{
				if (simpleTestTemplate == null)
				{
					var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.SimpleTest.xls", "SimpleTest.xls");
					simpleTestTemplate = new ExcelTemplateForUnitTesting("SimpleTest.xls", Path.GetFullPath(tempFileName));
				}
				return simpleTestTemplate;
			}
		}

		void SetupReportScheduleTask(out ReportScheduleTask scheduleTask, out ReportCommand reportCommand, out DocumentPack docPack, out DeliveryInstructions deliveryInstructions)
		{
			reportCommand = DocumentEngineTestHelper.CreateReportCommandWithExcelTemplate("Test Report", SimpleTestTemplate, Factory);

			docPack = new DocumentPack(reportCommand);
			deliveryInstructions = new DeliveryInstructions(docPack);
			deliveryInstructions.Recipients.RemoveAndDeleteAll();

			var recipient = deliveryInstructions.Recipients.AddNew();
			recipient.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			recipient.AttachmentType = "PDF";
			recipient.Name = "Unit Test";
			recipient.Email = "unit.test@cargowise.com";

			var stmReportRun = Factory.New<StmReportRun>();
			stmReportRun.RRI_StartTimeUtc = ZDateTime.UtcNow;

			scheduleTask = Factory.New<ReportScheduleTaskForEmptyReportContingencyTesting>();
			scheduleTask.PopulateDefaultsFromDeliveryInstructions(deliveryInstructions);
			scheduleTask.S5_ParentID = reportCommand.PK;
			scheduleTask.S5_EndDate = scheduleTask.S5_StartDate.AddDays(10);
			scheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddMinutes(-5);
			scheduleTask.StmReportRun = stmReportRun;
			stmReportRun.RRI_S5_Schedule = scheduleTask.PK;
		}

		void GenerateReportWithEmptyContingency(string contingency, string contactDetail, string deliveryMethod, string cc = null, string bcc = null)
		{
			TestCaseHelper.ClearTable(StmPrintJobCopyRecipientSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StmPrintJobSchema.Constants.TableName);
			AssertEquals("Precondition: there should be no print jobs in database", 0, Factory.GetDatabaseCount(typeof(StmPrintJob)));

			var command = Factory.LoadTop1<ReportCommand>(new DocumentZQuery("RepFreightReport", "Invoice Summary Report"));

			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var contact = organisation.Contacts.AddNew();
			contact.OC_Email = "staff@blah.com";
			contact.OC_Fax = "+1234567890";
			Factory.Save();

			var scheduleTask = Factory.New<ReportScheduleTaskForEmptyReportContingencyTesting>();

			scheduleTask.S5_ParentID = command.PK;
			scheduleTask.S5_EndDate = scheduleTask.S5_StartDate.AddDays(10);
			scheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddMinutes(-5);

			var recipient = scheduleTask.Recipients.AddNew();
			recipient.S6_DeliveryMethod = deliveryMethod;

			if (deliveryMethod == Enterprise.Core.Constants.ContactNotifyModes.Fax)
			{
				recipient.S6_FaxOverride = contactDetail;
			}
			else if (deliveryMethod == Enterprise.Core.Constants.ContactNotifyModes.Email)
			{
				recipient.S6_EmailToRecipientsAsString = contactDetail;
			}

			recipient.S6_EmptyReportDeliveryOptions = contingency;
			if (!string.IsNullOrEmpty(contactDetail))
			{
				recipient.S6_EmailToRecipientsAsString = contactDetail;
			}
			if (!string.IsNullOrEmpty(cc))
			{
				recipient.S6_CarbonCopyRecipientsAsString = cc;
			}
			if (!string.IsNullOrEmpty(bcc))
			{
				recipient.S6_BlindCarbonCopyRecipientsAsString = bcc;
			}
			recipient.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.Contact;
			recipient.S6_OH = organisation.PK;
			recipient.S6_OC = contact.PK;
			if (deliveryMethod == Enterprise.Core.Constants.ContactNotifyModes.Print)
			{
				recipient.S6_SQ = SavedPrintQueue.PK;
			}

			if (deliveryMethod == Enterprise.Core.Constants.ContactNotifyModes.Email)
			{
				recipient.S6_AttachmentType = OrgConstants.AttachmentType.PDF;
			}

			var docPack = new DocumentPack(command);
			using (var report = (Report)docPack[0])
			{
				report.PrepareForRender();

				var configField = (LookupField)report.FilterCollection["Consignor"];
				var configField2 = (LookupField)report.FilterCollection["Bill To Party"];

				var org = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "A"));
				configField.Value = org.PK.ToGuid();
				configField2.Value = org.PK.ToGuid();

				report.ColumnHeadingManager.DefaultTemplateConfigurationManager.Load();
				report.SetScheduleTask(scheduleTask);

				var periodRange = report.FilterCollection["Invoice Date"] as DateRangeField;
				periodRange.LowSchedule.PeriodScope = PeriodScopeList.Codes.This;
				periodRange.HighSchedule.PeriodScope = PeriodScopeList.Codes.This;
				docPack.DeliveryInstructions.Recipients.RemoveAndDeleteAll(); //START HERE
				scheduleTask.S5_ScheduleState = scheduleTask.SerializeForTesting(docPack.DeliveryInstructions, report);
			}

			scheduleTask.RunForTesting();
		}

		StmPrintQueue savedPrintQueue;
		StmPrintQueue SavedPrintQueue
		{
			get
			{
				if (savedPrintQueue == null)
				{
					var queueFactory = new BusinessObjectFactory();
					savedPrintQueue = queueFactory.NewWithValidTestData<StmPrintQueue>();
					queueFactory.Save();
				}
				return savedPrintQueue;
			}
		}

		OrgHeader Organization
		{
			get
			{
				if (organization == null)
				{
					ZQuery query = new ZQuery();
					organization = Factory.LoadTop1<OrgHeader>(query);
					OrgContact contact = organization.Contacts.AddNew();
					contact.OC_ContactName = "BOB1234";
					contact.OC_Email = "bob@example.com";
					contact.OC_Fax = "123";
					Factory.Save();
				}
				return organization;
			}
		}

		DocumentPack Pack
		{
			get
			{
				if (pack == null)
				{
					StmMenuItem menuItem = Factory.Load<StmMenuItem>(TestReportPK);
					pack = new DocumentPack(menuItem);
				}
				return pack;
			}
		}

		DeliveryInstructions Instructions
		{
			get
			{
				if (instructions == null)
				{
					instructions = CreateTestInstructions();
				}
				return instructions;
			}
		}

		ReportScheduleTask ScheduleTask
		{
			get
			{
				if (scheduleTask == null)
				{
					scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
				}
				return scheduleTask;
			}
		}

		readonly NotificationBuffer Notifications = new NotificationBuffer();

		DeliveryInstructions CreateTestInstructions()
		{
			var menuItem = Factory.NewWithValidTestData<ReportCommand>();
			menuItem.SU_BusinessContext = "Quotation";
			menuItem.SU_ContactType = "SAL";
			Factory.Save();

			var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.NewStyleTemplate.xls", "NewStyleTemplate.xls");
			var excelTemplate = new ExcelTemplateForUnitTesting("NewStyleTemplate.xls", Path.GetFullPath(tempFileName));
			var pack = new DocumentPack(menuItem);
			pack.Add(new Report(pack, excelTemplate));
			instructions = new DeliveryInstructions(pack);

			instructions.Recipients.RemoveAndDeleteAll();
			var contact1 = instructions.Recipients.AddNew();
			contact1.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			contact1.AttachmentType = "PDF";
			contact1.OrgHeaderPK = Organization.PK;
			contact1.Name = "BOB1234";

			var contact2 = instructions.Recipients.AddNew();
			contact2.DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;
			contact2.AttachmentType = "XLS";
			contact2.OrgHeaderPK = Organization.PK;
			contact2.Name = "BOB1234";
			return instructions;
		}

		void SetUpPostMasterGroup()
		{
			GlbGroup postMastersGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			GlbStaff staff = postMastersGroup.Staff.AddNew();
			staff.GS_EmailAddress = "staff@group.com";
			staff.GS_Code = "ZAC";
			Factory.Save();
		}

		OrgContact CreateContact(OrgHeader organisation, string name, string email, string fax)
		{
			OrgContact result = organisation.Contacts.AddNew();
			result.OC_ContactName = name;
			result.OC_Email = email;
			result.OC_Fax = fax;
			return result;
		}

		GlbStaff CreateStaff(GlbGroup group, string code, string name, string email, string fax)
		{
			GlbStaff result = group.Staff.AddNew();
			result.GS_Code = code;
			result.GS_FullName = name;
			result.GS_EmailAddress = email;
			result.GS_FaxNum = fax;
			return result;
		}

		ReportScheduleTaskRecipient CreateRecipient(string deliveryToType, string deliveryMethod, IZType deliveryRecipient)
		{
			ReportScheduleTaskRecipient recipient = ScheduleTask.Recipients.AddNew();

			recipient.S6_DeliveryToType = deliveryToType;
			recipient.S6_DeliveryMethod = deliveryMethod;

			if (deliveryToType == ScheduledReportDeliveryRecipientConstants.RecipientType.Contact)
			{
				if (deliveryRecipient is ZString)
				{
					recipient.ContactName = (ZString)deliveryRecipient;
				}
				else
				{
					recipient.S6_OC = (ZGuid)deliveryRecipient;
				}
			}
			else if (deliveryToType == ScheduledReportDeliveryRecipientConstants.RecipientType.Group)
			{
				recipient.S6_GG = (ZGuid)deliveryRecipient;
			}
			else if (deliveryToType == ScheduledReportDeliveryRecipientConstants.RecipientType.Staff)
			{
				recipient.S6_GS_NKRecipient = (ZString)deliveryRecipient;
			}
			return recipient;
		}

		void AssertDocDeliveryContact(string id, DocDeliveryContact contact, string name, string deliveryMethod, string attachmentType, string deliveryAddress, ZGuid orgHeaderPK)
		{
			AssertEquals(id + ".Name", name, contact.Name);
			AssertEquals(id + ".DeliveryMethod", deliveryMethod, contact.DeliveryMethod);
			AssertEquals(id + ".AttachmentType", attachmentType, contact.AttachmentType);
			AssertEquals(id + ".DeliveryAddress", deliveryAddress, contact.DeliveryAddress);
			AssertEquals(id + ".OrgHeaderPK", orgHeaderPK, contact.OrgHeaderPK);
		}

		Report GetReportForScheduleReport()
		{
			var menuItem = Factory.LoadTop1<StmMenuItem>(new DocumentZQuery("RepOrdersReport", "Client - Order Status Summary"));
			var command = Factory.Load<ReportCommand>(menuItem.PK);
			var pack = new DocumentPack(command);
			var report = (Report)pack[0];
			report.PrepareForRender();
			var configField = (LookupField)report.FilterCollection["Client"];
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "A"));
			configField.Value = org.PK.ToGuid();
			report.ColumnHeadingManager.DefaultTemplateConfigurationManager.Load();

			return report;
		}

		DeliveryInstructions GetDeliveryInstructionsForScheduleReport(DocumentPack pack)
		{
			var instructions = new DeliveryInstructions(pack);
			instructions.Recipients.RemoveAll();

			DocDeliveryContact contact1 = instructions.Recipients.AddNew();
			contact1.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			contact1.AttachmentType = "PDF";
			contact1.OrgHeaderPK = Organization.PK;
			contact1.Name = "BOB1234";
			contact1.Email = "blah@blah.com";

			return instructions;
		}

		void CreateAccPeriodTestData()
		{
			var companyPK = GlbCompany.CurrentCompany.PK;
			AccPeriodManagement accPeriod;
			var collection = new AccPeriodManagementCollection(Factory);

			accPeriod = collection.AddNew();
			accPeriod.AM_GC_Company = companyPK;
			accPeriod.AM_Period = 201801;
			accPeriod.AM_Year = 2018;
			accPeriod.AM_StartDate = new ZDateTime(2018, 1, 1);
			accPeriod.AM_EndDate = new ZDateTime(2018, 3, 31);

			accPeriod = collection.AddNew();
			accPeriod.AM_GC_Company = companyPK;
			accPeriod.AM_Period = 201804;
			accPeriod.AM_Year = 2018;
			accPeriod.AM_StartDate = new ZDateTime(2018, 4, 1);
			accPeriod.AM_EndDate = new ZDateTime(2018, 6, 30);

			accPeriod = collection.AddNew();
			accPeriod.AM_GC_Company = companyPK;
			accPeriod.AM_Period = 201807;
			accPeriod.AM_Year = 2018;
			accPeriod.AM_StartDate = new ZDateTime(2018, 7, 1);
			accPeriod.AM_EndDate = new ZDateTime(2018, 9, 30);

			accPeriod = collection.AddNew();
			accPeriod.AM_GC_Company = companyPK;
			accPeriod.AM_Period = 201810;
			accPeriod.AM_Year = 2018;
			accPeriod.AM_StartDate = new ZDateTime(2018, 10, 1);
			accPeriod.AM_EndDate = new ZDateTime(2018, 12, 31);

			accPeriod = collection.AddNew();
			accPeriod.AM_GC_Company = companyPK;
			accPeriod.AM_Period = 201901;
			accPeriod.AM_Year = 2019;
			accPeriod.AM_StartDate = new ZDateTime(2019, 1, 1);
			accPeriod.AM_EndDate = new ZDateTime(2019, 3, 31);

			accPeriod = collection.AddNew();
			accPeriod.AM_GC_Company = companyPK;
			accPeriod.AM_Period = 201904;
			accPeriod.AM_Year = 2019;
			accPeriod.AM_StartDate = new ZDateTime(2019, 4, 1);
			accPeriod.AM_EndDate = new ZDateTime(2019, 6, 30);

			accPeriod = collection.AddNew();
			accPeriod.AM_GC_Company = companyPK;
			accPeriod.AM_Period = 201907;
			accPeriod.AM_Year = 2019;
			accPeriod.AM_StartDate = new ZDateTime(2019, 7, 1);
			accPeriod.AM_EndDate = new ZDateTime(2019, 9, 30);

			accPeriod = collection.AddNew();
			accPeriod.AM_GC_Company = companyPK;
			accPeriod.AM_Period = 201910;
			accPeriod.AM_Year = 2019;
			accPeriod.AM_StartDate = new ZDateTime(2019, 10, 1);
			accPeriod.AM_EndDate = new ZDateTime(2019, 12, 31);
		}

		static void AssertColumnContents(ExcelFile excelFile, int columnIndex, string expectedValue, string expectedFormat)
		{
			AssertEquals("Col: " + columnIndex.ToString() + " - Cell value", expectedValue, excelFile.GetStringFromCell(4, columnIndex).Value);
			AssertEquals("Col: " + columnIndex.ToString() + " - Cell format", expectedFormat, excelFile.GetFormat(excelFile.GetCellFormat(4, columnIndex)).Format);
		}

		ReportCommand GetReportCommandWithTemplate(string menuName, string templateName)
		{
			var menuItem = Factory.New<ReportCommand>();
			menuItem.SU_MenuName = menuName;
			var template = Factory.New<StmTemplate>();
			template.SO_Template = new ExcelTemplateForUnitTesting(templateName, TestFilesSubFolder.ReportTestFiles).GetAsByteArray();
			var menuTemplateLink = Factory.New<StmMenuTemplatePivot>();
			menuTemplateLink.SI_SU = menuItem.PK;
			menuTemplateLink.SI_SO = template.PK;
			return menuItem;
		}

		DocumentPack pack;
		OrgHeader organization;
		DeliveryInstructions instructions;
		ReportScheduleTask scheduleTask;

		sealed class ReportScheduleTaskForTest : ReportScheduleTask
		{
			public ReportScheduleTaskForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override PrintTask GetNewPrintTaskForRun() => new PrintTaskForTest();
		}

		sealed class PrintTaskForTest : PrintTask
		{
			public override void Run(DeliveryInstructions deliveryInstructions, INotifications notifications = null)
			{
				var pack = GetFirstDocumentPack();
				pack.OnAfterReportRun += (sender, item) =>
				{
					TestCaseHelper.ClearTable(StmDeliveryGroup.Schema.TableName);
				};

				base.Run(deliveryInstructions, notifications);
			}
		}

		sealed class ReportScheduleTaskForEmptyReportContingencyTesting : ReportScheduleTask
		{
			public ReportScheduleTaskForEmptyReportContingencyTesting(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public void RunForTesting()
			{
				INotifications notifications = new NotificationsIgnorer();
				RunCore(notifications, CancellationToken.None);
			}

			class NotificationsIgnorer : INotifications
			{
				public NotificationsIgnorer()
				{
				}

				public void Add(INotification notification)
				{
				}
			}
		}

		sealed class ReportScheduleTaskForTesting : ReportScheduleTask, IDisposable
		{
			public ReportScheduleTaskForTesting(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override PrintTask GetNewPrintTaskForRun()
			{
				PrintTaskForTesting result = new PrintTaskForTesting();
				CreatedPrintTasksForRun.Add(result);
				return result;
			}

			internal List<PrintTask> CreatedPrintTasksForRun = new List<PrintTask>();

			public int CallCountForSQLCPUTime;

			protected override int SQLCPUTime(DbConnection connection)
			{
				CallCountForSQLCPUTime++;
				return base.SQLCPUTime(connection);
			}

			#region IDisposable Members

			void IDisposable.Dispose()
			{
				foreach (PrintTaskForTesting printTask in CreatedPrintTasksForRun)
				{
					printTask.ReallyDispose();
				}
			}

			#endregion
		}

		sealed class ReportScheduleTaskForTestingInfrastructureErrors : ReportScheduleTask
		{
			public ReportScheduleTaskForTestingInfrastructureErrors(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override PrintTask GetNewPrintTaskForRun()
			{
				if (S5_IsPrivate)
				{
					DbServerName = "Test Report Server New";
				}

				return new PrintTaskForInfrastructureErrors();
			}

			class PrintTaskForInfrastructureErrors : PrintTask
			{
				public override void Run(DeliveryInstructions deliveryInstructions, INotifications notifications = null)
				{
					var error = SqlExceptionBuilder.CreateSqlError(983, 1, 1, "", "Unable to access database because its replica role is RESOLVING.", "", 1);
					var errorCollection = SqlExceptionBuilder.CreateSqlErrorCollection(error);
					var exception = SqlExceptionBuilder.CreateSqlException(errorCollection);
					throw exception;
				}
			}
		}

		sealed class ReportScheduleTaskForTestingWithReportDb : ReportScheduleTask
		{
			public ReportScheduleTaskForTestingWithReportDb(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override PrintTask GetNewPrintTaskForRun()
			{
				return new PrintTaskForReportDbTesting();
			}

			class PrintTaskForReportDbTesting : PrintTask
			{
				public override void Run(DeliveryInstructions deliveryInstructions, INotifications notifications = null)
				{
					foreach (DocumentPack documentPack in GetDocumentPacks())
					{
						foreach (var report in documentPack)
						{
							var reportDb = new ReportDbForTesting(Db.ServerName, Db.DatabaseName);
							var manager = SecondaryServerConnectionProviderProvider.GetProvider(reportDb);
							((IReportForReportDbTesting)report).SetReportDbManagerForTesting(manager);
						}
					}
					base.Run(deliveryInstructions, notifications);
				}
			}
		}

		sealed class PrintTaskForTesting : PrintTask
		{
			public override void Run(DeliveryInstructions deliveryInstructions, INotifications notifications = null)
			{
				if (DeliveryInstructionsRun != null)
				{
					DeliveryInstructionsRun.Add(deliveryInstructions);
				}
				if (DocumentsRun != null)
				{
					DocumentsRun.Add(deliveryInstructions.DocPack.ToList());
				}
				base.Run(deliveryInstructions);
			}

			protected override void Dispose(bool disposing)
			{
				//Don't dispose for testing.
			}

			public void ReallyDispose()
			{
				base.Dispose(true);
			}

			internal static List<DeliveryInstructions> DeliveryInstructionsRun;
			internal static List<List<BusinessObject>> DocumentsRun;
		}
	}
}

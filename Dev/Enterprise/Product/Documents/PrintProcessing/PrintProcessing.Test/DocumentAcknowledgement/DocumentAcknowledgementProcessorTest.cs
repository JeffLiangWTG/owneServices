using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.Integration.Freight;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.MailFilters;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.PrintProcessing.DocumentAcknowledgement.Testing
{
	sealed class DocumentAcknowledgementProcessorTest : TestCaseWithFactory
	{
		IDocumentAcknowledgementProcessor processor;
		GlbStaff staffOnPrintJob;
		StmPrintJob printJob;
		ZString abbreviatedSubjectLine;
		ZGuid printJobParentGuid;

		protected override void SetUp()
		{
			base.SetUp();
			processor = new DocumentAcknowledgementProcessor();
		}

		void SetupPrintJob(string jobType, ZByte retryAttempts)
		{
			staffOnPrintJob = Factory.New(typeof(GlbStaff)) as GlbStaff;
			staffOnPrintJob.GS_Code = "PRJ";
			staffOnPrintJob.GS_FullName = "Jane Doe";
			staffOnPrintJob.GS_EmailAddress = "jane@example";
			staffOnPrintJob.GS_LoginName = "janedoe";

			var company = Factory.New(typeof(GlbCompany)) as GlbCompany;
			company.GC_Code = "CPP";
			company.GC_Name = "A Company";

			var branch = Factory.New(typeof(GlbBranch)) as GlbBranch;
			branch.GB_Code = "BPP";
			branch.GB_GC = company.PK;
			branch.GB_BranchName = "A Branch";

			printJob = StmPrintJob.New(Factory);
			printJob.SP_GS_NKJobSubmittedBy = staffOnPrintJob.GS_Code;
			printJob.SP_GB = branch.PK;
			printJob.SP_EmailSubjectLine = "A Company (A Branch) - The document";
			printJob.SP_FaxDestination = "+61 (2) 9025-1199";
			printJob.SP_JobType = jobType;
			printJobParentGuid = staffOnPrintJob.PK;
			printJob.SP_ParentGuid = printJobParentGuid;
			printJob.SP_RetryAttempts = retryAttempts;
			printJob.SP_ParentTableName = "GlbStaff";
			printJob.SP_RunDateTime = new ZDateTime(2005, 1, 4, 13, 45, 8);

			Factory.Save();

			abbreviatedSubjectLine = printJob.SP_AbbreviatedEmailSubjectLine;
		}

		void AssertLogExists(ZString reference, ZString eventCode, ZGuid parent, ZString table)
		{
			var logFilter = new ZQuery();
			logFilter.AddToFilter(StmALogSchema.SL_Reference, reference);
			logFilter.AddToFilter(StmALogSchema.SL_SE_NKEvent, eventCode);
			var logs = (StmALog[])Factory.Load(typeof(StmALog), logFilter);

			AssertEquals("Delivered log count", 1, logs.Length);
			AssertEquals("Delivered log event", eventCode, logs[0].Event.SE_Code);
			AssertEquals("Delivered log parent", parent, logs[0].SL_Parent);
			AssertEquals("Delivered log table", table, logs[0].SL_Table);
		}

		public void TestProcessValidPrintJobSuccessWithStaffAndEmail()
		{
			SetupPrintJob("FAA", 0);
			processor.Process(printJob.PK, DocumentAcknowledgementStatus.Success);
			AssertLogExists("+61 (2) 9025-1199 - " + abbreviatedSubjectLine, AutoEvents.DocumentDelivered.Code, printJobParentGuid, "GlbStaff");
			AssertEquals("DDS", printJob.SP_JobType);
		}

		public void TestProcessValidPrintJobFailureWithStaffAndEmail()
		{
			SetupPrintJob("FAA", 0);
			TestCaseHelper.ClearTable(MailDBItemsSchema.Constants.TableName);
			processor.Process(printJob.PK, DocumentAcknowledgementStatus.Failure);

			AssertLogExists("+61 (2) 9025-1199 - " + abbreviatedSubjectLine + "|RES=FAI", AutoEvents.DocumentNotDelivered.Code, printJobParentGuid, "GlbStaff");
			AssertMailExists(abbreviatedSubjectLine, "+61 (2) 9025-1199", "04-Jan-05 13:45");
			Assert(printJob.IsDeleted);
		}

		public void TestCanRunInAnyBranch()
		{
			var shipment = (BusinessObject)Factory.New<ICommonShipment>();
			shipment.FillWithValidTestData();
			_ = new JobHeader.Loader((IJobHeaderParent)shipment).TryCreate();

			SetupPrintJob("PRN", 0);
			printJob.SP_ParentGuid = shipment.PK;
			printJob.SP_ParentTableName = "JobShipment";

			Factory.Save();

			using (Env.Instance.TemporaryServiceTaskContext(MailFilterCodes.MailProcessingTask, canRunInAnyBranch: true))
			{
				processor.Process(printJob.PK, DocumentAcknowledgementStatus.Success);
			}

			AssertNullOrEmpty("No Errors should be reported", ErrorReporter.LastMessageReported);
		}

		void AssertMailExists(string subject, string number, string dateTimeText)
		{
			AssertEquals("Should have sent one email", 1, Factory.GetDatabaseCount(typeof(MailItem)));
			MailItem failureMessage = Factory.LoadTop1(typeof(MailItem), new ZQuery()) as MailItem;
			AssertEquals("Mail subject", Core.Constants.ProductName + " Document Delivery Failure", failureMessage.MI_Subject);
			AssertEquals("Mail body", "The following fax could not be delivered:" + System.Environment.NewLine
				+ subject + System.Environment.NewLine
				+ "Fax number: " + number + System.Environment.NewLine
				+ "Sent: " + dateTimeText + System.Environment.NewLine
				+ System.Environment.NewLine
				+ "The fax number may be busy; please try re-sending the document again. If you have received this message multiple times, please check that the fax number is correct." + System.Environment.NewLine
				, failureMessage.MI_Body);
		}

		public void TestPrintJobShouldBeDeleted_WhenProcessedByDODAlready()
		{
			SetupPrintJob("FAA", 0);
			printJob.SP_EDocsProcessed = true;
			Factory.Save();

			processor.Process(printJob.PK, DocumentAcknowledgementStatus.Success);
			AssertEquals(true, printJob.IsDeleted);
		}
	}
}

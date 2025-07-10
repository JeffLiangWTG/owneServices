using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.RemotePrinting.Engine;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.PrintProcessing.Testing
{
	sealed class HardCopyProcessorTest : MergedPrintGroupProcessorTestCase
	{
		public void TestEmailGeneratedForInvalidPrinter()
		{
			UnattendedUserNotification.Instance.ClearShownOnceADayErrorKeys();
			TestAssistant.SetupEmailAddressForAllStaff(Factory);

			var printQueue = TestAssistant.CreateTestPrintQueue(Factory);
			var printGroup = new StmPrintJobMergedCollection(Factory);

			var printJob = printGroup.AddNew();
			printJob.SP_EmailSubjectLine = "Name 1";
			printJob.SP_JobType = "PRN";
			printJob.SP_SQ = printQueue.PK;
			var printJob2 = printGroup.AddNew();
			printJob2.SP_EmailSubjectLine = "Name 2";
			printJob2.SP_JobType = "PRN";
			printJob2.SP_SQ = printQueue.PK;

			using (EnvProxy.Instance.SetTemporaryUserContext("CWService", Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (EnvProxy.Instance.TemporaryServiceTaskContext("DOD", true))
			{
				var processor = new HardCopyProcessor_InvalidPrinter(printGroup);
				processor.Process();

				AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);

				var expectedBody = $@"The printer Test Printer is unavailable on server {printQueue.SQ_ServerName}.

	Service Task Code: DOD

	Job Name: Name 1
	Job Type: PRN
	Printer Name: Test Printer
	Server Name: {printQueue.SQ_ServerName}

	Job Name: Name 2
	Job Type: PRN
	Printer Name: Test Printer
	Server Name: {printQueue.SQ_ServerName}

Please check 'Control Panel' > 'Printers and Faxes' to ensure that this printer is online.";
				AssertContains(expectedBody, Env.OutgoingMailManager.EmailsCreated[0].Body);
			}
		}

		public void TestHandleWin32Exception()
		{
			var printQueue = TestAssistant.CreateTestPrintQueue(Factory);
			printQueue.SQ_QueueName = "Dummy Printer";
			var printGroup = new StmPrintJobMergedCollection(Factory);

			var printJob = printGroup.AddNew();
			printJob.SP_JobType = "PRN";
			printJob.SP_SQ = printQueue.PK;

			var processor = new HardCopyProcessor_ThrowsWin32Exception(printGroup);
			AssertExceptionThrown(typeof(Win32Exception), "Invalid Handler - The printer name is Dummy Printer", () => processor.Process());
		}

		[TestDate(2006, 01, 01)]
		public void TestRPCErrorReportedToController()
		{
			UnattendedUserNotification.Instance.ClearShownOnceADayErrorKeys();
			TestAssistant.SetupEmailAddressForAllStaff(Factory); // so we don't get warnings about not having postmaster email address

			var printQueue = TestAssistant.CreateTestPrintQueue(Factory);
			var printGroup = new StmPrintJobMergedCollection(Factory);

			var printJob = printGroup.AddNew();
			printJob.SP_JobType = "PRN";
			printJob.SP_SQ = printQueue.PK;
			var printJob2 = printGroup.AddNew();
			printJob2.SP_JobType = "PRN";
			printJob2.SP_SQ = printQueue.PK;

			var processor = new HardCopyProcessor_ThrowsRPCException(printGroup);
			processor.Process();
			AssertEquals("RPC exception encountered, should send email to the controller", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			Assert("Subject line", Env.OutgoingMailManager.EmailsCreated[0].Subject.Contains("Cannot print to printer " + printQueue.SQ_QueueName + " on server " + System.Environment.MachineName));

			processor.Process();
			AssertEquals("Should not have sent further emails in the same 24 hr period", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(24.01);
			processor.Process();
			AssertEquals("Should have sent another email if exception is encountered 24hrs later", 2, Env.OutgoingMailManager.EmailsCreated.Count);
			Assert("Subject line", Env.OutgoingMailManager.EmailsCreated[0].Subject.Contains("Cannot print to printer " + printQueue.SQ_QueueName + " on server " + System.Environment.MachineName));
		}

		[TestDate(2006, 01, 01)]
		public void TestPrinterNotFoundReportedToController()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();
			TestAssistant.SetupEmailAddressForAllStaff(Factory); // so we don't get warnings about not having postmaster email address

			UnattendedUserNotification.Instance.ClearShownOnceADayErrorKeys();

			using (SafeInstalledPrinters.OverridePrintersForTesting("hello", "world"))
			{
				var printQueue = TestAssistant.CreateTestPrintQueue(Factory);
				var printGroup = new StmPrintJobMergedCollection(Factory);

				var printJob = printGroup.AddNew();
				printJob.SP_JobType = "PRN";
				printJob.SP_SQ = printQueue.PK;

				var printJob2 = printGroup.AddNew();
				printJob2.SP_JobType = "PRN";
				printJob2.SP_SQ = printQueue.PK;

				var processor = new HardCopyProcessor(printGroup, new MockPrinterFactory());
				processor.Process();
				AssertEquals("Despite two print jobs, only email should have been sent to the controller", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				Assert("Subject line", Env.OutgoingMailManager.EmailsCreated[0].Subject.Contains("Printer " + printQueue.SQ_QueueName + " is unavailable on server"));

				Env.OutgoingMailManager.EmailsCreated.Clear();
				printGroup.RemoveAndDeleteAll();
				var printJob3 = printGroup.AddNew();
				printJob3.SP_JobType = "PRN";
				printJob3.SP_SQ = printQueue.PK;

				processor.Process();
				AssertEquals("No more emails should have been sent to the controller - not 24 hrs yet", 0, Env.OutgoingMailManager.EmailsCreated.Count);

				TestDateAttribute.Date = TestDateAttribute.Date.AddHours(24.01);
				processor.Process();
				AssertEquals("an email should have been sent now because it is more than a day since the last sending", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				Assert("Subject line", Env.OutgoingMailManager.EmailsCreated[0].Subject.Contains("Printer " + printQueue.SQ_QueueName + " is unavailable on server"));
			}
		}

		public void TestGetPrinter()
		{
			TestAssistant.SetupEmailAddressForAllStaff(Factory); // so we don't get warnings about not having postmaster email address

			var testPrintQueue = TestAssistant.CreateTestPrintQueue(Factory);
			var jobCollection = new StmPrintJobMergedCollection(Factory);
			var printJob = jobCollection.AddNew();
			printJob.SP_JobType = nameof(PrintType.PRN);
			printJob.SP_SQ = testPrintQueue.PK;

			using (SafeInstalledPrinters.OverridePrintersForTesting(TestAssistant.PrintQueueNameForTesting))
			using (var tempFile = TempFile.NewWithExtension("PDF"))
			{
				var mock = new MockPrinterFactory();
				var processor = new HardCopyProcessor(jobCollection, mock);
				printJob.StoredAttachmentFilename = tempFile.Filename;
				printJob.SP_EmailAttachments = tempFile.Filename;

				processor.Process();

				var printEngineJob = mock.PrintJobs.Single();
				AssertEquals(File.ReadAllBytes(tempFile.Filename), printEngineJob.Contents);
			}
		}

		public void TestProcessPRS()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var group = new StmPrintJobMergedCollection(Factory);
			var job1 = group.AddNew();
			job1.SP_ParentGuid = orgHeader.PK;
			job1.SP_ParentTableName = OrgHeaderSchema.Constants.TableName;
			job1.SP_JobType = nameof(PrintType.PRS);
			var job2 = group.AddNew();
			job2.SP_JobType = nameof(PrintType.PRS);
			job2.SP_ParentGuid = orgHeader.PK;
			job2.SP_ParentTableName = OrgHeaderSchema.Constants.TableName;
			Factory.Save();

			var mock = new MockPrinterFactory();
			new HardCopyProcessor(group, mock).Process();

			var logs = orgHeader.Logs.Find(x => x.SL_SE_NKEvent == Events.DocumentSentCode);
			AssertEquals(2, logs.Count());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestProcessPRN()
		{
			using (SafeInstalledPrinters.OverridePrintersForTesting(TestAssistant.PrintQueueNameForTesting))
			{
				var reportPath = PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestGeneratedReport;

				var printQueue = Factory.New<StmPrintQueue>();
				printQueue.SQ_QueueName = TestAssistant.PrintQueueNameForTesting;

				var group = new StmPrintJobMergedCollection(Factory);

				var deliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory, "Email Subject");

				var job1 = group.AddNew();
				job1.SP_JobType = nameof(PrintType.PRN);
				job1.SP_SB_DeliveryGroup = deliveryGroup.PK;
				job1.SP_SQ = printQueue.PK;
				job1.StoredAttachmentFilename = reportPath;

				var job2 = group.AddNew();
				job2.SP_JobType = nameof(PrintType.PRN);
				job2.SP_SB_DeliveryGroup = deliveryGroup.PK;
				job2.SP_SQ = printQueue.PK;
				job2.StoredAttachmentFilename = reportPath;

				Factory.Save();

				new HardCopyProcessor(group, new MockPrinterFactory()).Process();
			}
		}

		#region Implementation

		class HardCopyProcessor_ThrowsRPCException : HardCopyProcessor
		{
			public HardCopyProcessor_ThrowsRPCException(StmPrintJobMergedCollection mergedPrintGroup)
				: base(mergedPrintGroup, new MockPrinterFactory())
			{
			}

			protected override bool IsValidPrinter(string printQueueName)
			{
				throw new Win32Exception(ErrorHandler.RPC.RPC_S_SERVER_UNAVAILABLE);
			}
		}

		class HardCopyProcessor_ThrowsWin32Exception : HardCopyProcessor
		{
			public HardCopyProcessor_ThrowsWin32Exception(StmPrintJobMergedCollection mergedPrintGroup)
				: base(mergedPrintGroup, new MockPrinterFactory())
			{
			}

			protected override bool IsValidPrinter(string printQueueName)
			{
				throw new Win32Exception("Invalid Handler");
			}
		}

		class HardCopyProcessor_NoPrinterValidation : HardCopyProcessor
		{
			public HardCopyProcessor_NoPrinterValidation(StmPrintJobMergedCollection mergedPrintGroup)
				: base(mergedPrintGroup, new MockPrinterFactory())
			{
			}

			protected override bool IsValidPrinter(string printQueueName)
			{
				return true;
			}
		}

		class HardCopyProcessor_InvalidPrinter : HardCopyProcessor
		{
			public HardCopyProcessor_InvalidPrinter(StmPrintJobMergedCollection mergedPrintGroup)
				: base(mergedPrintGroup, new MockPrinterFactory())
			{
			}

			protected override bool IsValidPrinter(string printQueueName)
			{
				return false;
			}
		}

		#endregion

		internal override MergedPrintGroupProcessor GetProcessor(StmPrintJobMergedCollection printGroup)
		{
			return new HardCopyProcessor_NoPrinterValidation(printGroup);
		}
	}

	public class MockPrinterFactory : IPrinterFactory
	{
		public BasePrinter GetPrinter(PrintEngineJob job)
		{
			PrintJobsInternal.Add(job);
			return new TestPrinter(job);
		}

		List<PrintEngineJob> PrintJobsInternal { get; } = new List<PrintEngineJob>();

		public IEnumerable<PrintEngineJob> PrintJobs => PrintJobsInternal;

		class TestPrinter : BasePrinter
		{
			public TestPrinter(PrintEngineJob printJob)
				: base(printJob)
			{
			}

			protected override void ProcessAndPrintDocument()
			{
			}
		}
	}
}

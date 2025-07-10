using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI.JobInvoicing;
using Enterprise.Accounting.GUI.JobManagement;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module.Testing
{
	public class BulkJobProfitPrintingModuleHelperTest : TestCaseWithFactory
	{
		public void TestMenuItem()
		{
			IBulkJobProfitPrintingModuleHelper helper = new BulkJobProfitPrintingModuleHelper();
			bool called = false;
			MenuItem menu = (MenuItem)helper.GetMenuItem(() => called = true);
			menu.PerformClick();
			Assert(called);
			AssertEquals(Constants.MenuNameConstants.PrintJobProfitDoc, menu.Text);
		}

		public void TestSecurity()
		{
			IBulkJobProfitPrintingModuleHelper helper = new BulkJobProfitPrintingModuleHelper();
			var testObjectCreator = new TestObjectCreator(Factory);
			var job1 = testObjectCreator.CreateJob(testObjectCreator.CreateJobPlugIn(JobInvoicingConsumerTypes.Shipment), false);
			var job2 = testObjectCreator.CreateJob(testObjectCreator.CreateJobPlugIn(JobInvoicingConsumerTypes.Brokerage), false);
			MenuItem menu = (MenuItem)helper.GetMenuItem(() =>
					helper.PrintJobProfitDocument(Factory, new BusinessObject[] { job1, job2 }));
			var job1BulkPostAllCheckpoint = new JobInvoicingSecurityHelper(job1.PlugInData.InvoicingSupporter.JobInvoicingSecurity).GetInvSecurity(SecurityCore.BulkPrintJobProfitDoc);
			var job2BulkPostAllCheckpoint = new JobInvoicingSecurityHelper(job2.PlugInData.InvoicingSupporter.JobInvoicingSecurity).GetInvSecurity(SecurityCore.BulkPrintJobProfitDoc);

			Action<string> assertAction = expectedText =>
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					menu.PerformClick();

					var message = UnitTestUserNotification.Instance.LastMessage;
					AssertNotNull("Message should be shown", message);
					AssertEquals("Message", expectedText, message.Text);
				};

			string expectedMessage =
@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Operate -> Forwarding -> Shipments -> Billing -> Bulk Job Billing Actions -> Print Job Profit Document
Operate -> Customs -> Customs Declarations -> Billing -> Bulk Job Billing Actions -> Print Job Profit Document";

			job1BulkPostAllCheckpoint.IsAllowed = false;
			job2BulkPostAllCheckpoint.IsAllowed = false;
			assertAction(expectedMessage);

			expectedMessage =
@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Operate -> Forwarding -> Shipments -> Billing -> Bulk Job Billing Actions -> Print Job Profit Document";

			job1BulkPostAllCheckpoint.IsAllowed = false;
			job2BulkPostAllCheckpoint.IsAllowed = true;
			assertAction(expectedMessage);

			expectedMessage =
@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Operate -> Customs -> Customs Declarations -> Billing -> Bulk Job Billing Actions -> Print Job Profit Document";

			job1BulkPostAllCheckpoint.IsAllowed = true;
			job2BulkPostAllCheckpoint.IsAllowed = false;
			assertAction(expectedMessage);

			expectedMessage = string.Format("Are you sure you want to print Job Profit Document for following job(s): {0}, {1}", job1.JH_JobNum, job2.JH_JobNum);

			job1BulkPostAllCheckpoint.IsAllowed = true;
			job2BulkPostAllCheckpoint.IsAllowed = true;
			assertAction(expectedMessage);
		}

		public void TestPrinting()
		{
			ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			Job job1 = new Job.Loader(shipment1).TryCreateWithoutMutexForTestOnly();
			job1.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Charge charge = job1.Charges.AddNew();
			charge.JR_AC = AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value;
			charge.JR_LocalCostAmt = 100m;
			charge.JR_LocalSellAmt = 200m;

			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			Job job2 = new Job.Loader(shipment2).TryCreateWithoutMutexForTestOnly();
			job2.JH_GE = GlbDepartment.CurrentDepartment.PK;

			ForwardingShipment shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			Job job3 = new Job.Loader(shipment3).TryCreateWithoutMutexForTestOnly();
			job3.JH_GE = GlbDepartment.CurrentDepartment.PK;

			Factory.Save();

			var jobManagement1 = Factory.Load<JobManagement>(job1.PK);

			IBulkJobProfitPrintingModuleHelper helper = new BulkJobProfitPrintingModuleHelperForTest();
			MenuItem menu = (MenuItem)helper.GetMenuItem(() => helper.PrintJobProfitDocument(Factory, Array.Empty<BusinessObject>()));

			menu.PerformClick();
			AssertEquals("Message should be shown", "Please select a Job before printing.", UnitTestUserNotification.Instance.LastMessage.Text);

			menu = (MenuItem)helper.GetMenuItem(() => helper.PrintJobProfitDocument(Factory, null));
			menu.PerformClick();
			AssertEquals("Message should be shown", "Please select a Job before printing.", UnitTestUserNotification.Instance.LastMessage.Text);

			menu = (MenuItem)helper.GetMenuItem(() =>
					helper.PrintJobProfitDocument(Factory, new BusinessObject[] { jobManagement1, job2, shipment3 }));

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			menu.PerformClick();
			var expectedMessage = string.Format("Are you sure you want to print Job Profit Document for following job(s): {0}, {1}, {2}",
				job1.JH_JobNum, job2.JH_JobNum, job3.JH_JobNum);
			AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertNull("JobDocumentPrinter", ((BulkJobProfitPrintingModuleHelperForTest)helper).JobDocumentPrinter);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
			menu.PerformClick();
			JobDocumentPrintItem[] lastPrintedItems =
				((BulkJobProfitPrintingModuleHelperForTest)helper).JobDocumentPrinter.LastPrintedItems;
			AssertNull("LastPrintedItems", lastPrintedItems);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			menu.PerformClick();
			lastPrintedItems =
				((BulkJobProfitPrintingModuleHelperForTest)helper).JobDocumentPrinter.LastPrintedItems;
			AssertNotNull("LastPrintedItems", lastPrintedItems);
			AssertEquals(3, lastPrintedItems.Length);
			AssertEquals(job1, lastPrintedItems[0].Job);
			AssertEquals(job2, lastPrintedItems[1].Job);
			AssertEquals(job3, lastPrintedItems[2].Job);
		}

		class BulkJobProfitPrintingModuleHelperForTest : BulkJobProfitPrintingModuleHelper
		{
			protected override JobDocumentPrinter CreateJobDocumentPrinter(BusinessObjectFactory factory)
			{
				JobDocumentPrinter = new JobDocumentPrinterForTest(factory);
				return JobDocumentPrinter;
			}

			public JobDocumentPrinterForTest JobDocumentPrinter;
		}

		class JobDocumentPrinterForTest : JobDocumentPrinter
		{
			public JobDocumentPrinterForTest(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public override void PrintJobProfitDocuments(BusinessObjectFactory factory, params JobDocumentPrintItem[] jobDocumentPrinters)
			{
				LastPrintedItems = jobDocumentPrinters;
			}

			public JobDocumentPrintItem[] LastPrintedItems;
		}

		public void TestInvalidJobParentNotCausingException()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			var shipment1 = testObjectCreator.CreateShipment("S00001001");
			var shipment2 = testObjectCreator.CreateShipment("S00001002");
			var jobWithParent1 = testObjectCreator.CreateJob(shipment1);
			var jobWithParent2 = testObjectCreator.CreateJob(shipment2);
			var jobWithoutParent1 = testObjectCreator.CreateJob("S00001003", null, 0m, null, 0m);
			var jobWithoutParent2 = testObjectCreator.CreateJob("S00001004", null, 0m, null, 0m);
			Factory.Save();

			var expectedMsgWithOneInvalidJob = FormattableString.Invariant($@"Job Profit Document will not be generated for following job(s) with missing/invalid parent: S00001003.
You can use the filter 'Missing/Invalid Job Parent' in Job Management module to list all jobs without a valid parent.");

			var expectedMsgWithMultipleInvalidJobs = FormattableString.Invariant($@"Job Profit Document will not be generated for following job(s) with missing/invalid parent: S00001003, S00001004.
You can use the filter 'Missing/Invalid Job Parent' in Job Management module to list all jobs without a valid parent.");

			AssertNoExceptionAndMessage(jobWithParent1, jobWithParent2, jobWithoutParent1, jobWithoutParent2, expectedMsgWithOneInvalidJob, expectedMsgWithMultipleInvalidJobs);
		}

		public void TestInactiveJobCantBePrint()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			var shipment1 = testObjectCreator.CreateShipment("S00001001");
			var shipment2 = testObjectCreator.CreateShipment("S00001002");
			var shipment3 = testObjectCreator.CreateShipment("S00001003");
			var shipment4 = testObjectCreator.CreateShipment("S00001004");
			var jobWithParent1 = testObjectCreator.CreateJob(shipment1);
			var jobWithParent2 = testObjectCreator.CreateJob(shipment2);
			var jobInactive1 = testObjectCreator.CreateJob(shipment3);
			var jobInactive2 = testObjectCreator.CreateJob(shipment4);
			Factory.Save();

			jobInactive1.MarkAsInactive();
			jobInactive2.MarkAsInactive();
			Factory.Save();

			var expectedMsgWithOneInactiveJob = FormattableString.Invariant($@"Job Profit Document will not be generated for following job(s) which is inactive: S00001003.");

			var expectedMsgWithMultipleInactiveJobs = FormattableString.Invariant($@"Job Profit Document will not be generated for following job(s) which is inactive: S00001003, S00001004.");

			AssertNoExceptionAndMessage(jobWithParent1, jobWithParent2, jobInactive1, jobInactive2, expectedMsgWithOneInactiveJob, expectedMsgWithMultipleInactiveJobs);
		}

		public void TestInactiveJobCantBePrintAndInvalidJobParentNotCausingException()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			var shipment1 = testObjectCreator.CreateShipment("S00001001");
			var shipment2 = testObjectCreator.CreateShipment("S00001002");
			var shipment4 = testObjectCreator.CreateShipment("S00001004");
			var jobWithParent1 = testObjectCreator.CreateJob(shipment1);
			var jobWithParent2 = testObjectCreator.CreateJob(shipment2);
			var jobWithoutParent1 = testObjectCreator.CreateJob("S00001003", null, 0m, null, 0m);
			var jobInactive1 = testObjectCreator.CreateJob(shipment4);
			Factory.Save();

			jobInactive1.MarkAsInactive();
			Factory.Save();

			var expectedMsgWithOneInvalidJob = FormattableString.Invariant($@"Job Profit Document will not be generated for following job(s) with missing/invalid parent: S00001003.
You can use the filter 'Missing/Invalid Job Parent' in Job Management module to list all jobs without a valid parent.");

			var expectedMsgWithOneInvalidJobAndOneInactiveJob = FormattableString.Invariant($@"Job Profit Document will not be generated for following job(s) with missing/invalid parent: S00001003.
You can use the filter 'Missing/Invalid Job Parent' in Job Management module to list all jobs without a valid parent.

Job Profit Document will not be generated for following job(s) which is inactive: S00001004.");

			AssertNoExceptionAndMessage(jobWithParent1, jobWithParent2, jobWithoutParent1, jobInactive1, expectedMsgWithOneInvalidJob, expectedMsgWithOneInvalidJobAndOneInactiveJob);
		}

		void AssertNoExceptionAndMessage(Job normalJob1, Job normalJob2, Job abnormalJob1, Job abnormalJob2, string expectedMsgWithOneJob, string expectedMsgWithMultipleJobs)
		{
			var expectedMsgWithInactiveAndOneValidJobs = FormattableString.Invariant($@"{expectedMsgWithOneJob}

Job Profit Document can be generated for following job(s): S00001001.
Do you want to continue?");

			var expectedMsgWithInactiveAndMultipleValidJobs = FormattableString.Invariant($@"{expectedMsgWithOneJob}

Job Profit Document can be generated for following job(s): S00001001, S00001002.
Do you want to continue?");

			var expectedMsgWithOneValidJob = FormattableString.Invariant($@"Are you sure you want to print Job Profit Document for following job(s): S00001001");
			var expectedMsgWithMultipleValidJobs = FormattableString.Invariant($@"Are you sure you want to print Job Profit Document for following job(s): S00001001, S00001002");

			var expectedMessageWithJobs = new[]
			{
				new { Jobs = new Job[] { abnormalJob1 },  ExpectedMessage = expectedMsgWithOneJob },
				new { Jobs = new Job[] { abnormalJob1, abnormalJob2 },  ExpectedMessage = expectedMsgWithMultipleJobs },
				new { Jobs = new Job[] { abnormalJob1, normalJob1 },  ExpectedMessage = expectedMsgWithInactiveAndOneValidJobs },
				new { Jobs = new Job[] { abnormalJob1, normalJob1, normalJob2 },  ExpectedMessage = expectedMsgWithInactiveAndMultipleValidJobs },
				new { Jobs = new Job[] { normalJob1 },  ExpectedMessage = expectedMsgWithOneValidJob },
				new { Jobs = new Job[] { normalJob1, normalJob2 },  ExpectedMessage = expectedMsgWithMultipleValidJobs }
			};

			foreach (var expectedMessageWithJob in expectedMessageWithJobs)
			{
				var profitLoss = new JobProfitLoss(Factory);
				profitLoss.SetJobPKs(expectedMessageWithJob.Jobs.Select(x => x.PK).ToArray());
				IBulkJobProfitPrintingModuleHelper helper = new BulkJobProfitPrintingModuleHelperForTest();
				var menu = (MenuItem)helper.GetMenuItem(() => helper.PrintJobProfitDocument(Factory, expectedMessageWithJob.Jobs));

				using (var form = new JobManagementForm(profitLoss))
				{
					using (var control = new JobProfitLossControl())
					{
						control.SetDataBinding(profitLoss, "");
						form.Controls.Add(control);

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						AssertNoExceptionThrown(() => menu.PerformClick());
						AssertEquals(expectedMessageWithJob.ExpectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
		}
	}
}

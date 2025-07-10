using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.GUI.JobManagement;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Integration.Rating;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(JobManagementModule))]
	public class JobManagementModuleTest : JobManagementModuleBaseTest
	{
		public class MockJobManagementModule : JobManagementModule
		{
			protected override JobManagement SelectedJob
			{
				get
				{
					return fSelectedJob;
				}
			}
			public JobManagement fSelectedJob;

			protected override BusinessObject[] SelectedJobs
			{
				get
				{
					return fSelectedJobs;
				}
			}
			public BusinessObject[] fSelectedJobs;
		}

		public void TestCanNotClosedWhenJobExistDisbursementChargeWhichAlreadyPosted()
		{
			var chargeCode = TestObjectCreator.CreateChargeCode("TSB", "Test DSB charge type", Enterprise.Core.Constants.ChargeType.Disbursement, 100M, TestObjectCreator.GST1, null);
			chargeCode.AC_ChargeGroup = Enterprise.Core.Constants.ChargeType.Disbursement;
			chargeCode.AC_AG_DisbursementSurplusAccount = TestObjectCreator.GLHeader1.PK;
			chargeCode.AC_AG_DisbursementShortfallAccount = TestObjectCreator.GLHeader2.PK;

			var jobClosable1 = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var charge1 = TestObjectCreator.CreateCharge(jobClosable1, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			var transaction1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			transaction1.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, transaction1.PK));

			var jobClosable2 = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var charge2 = TestObjectCreator.CreateCharge(jobClosable2, TestObjectCreator.CC1, "charge2", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			var transaction2 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR002", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			transaction2.Lines.Add(TestObjectCreator.CreateRevenueLine(charge2, transaction2.PK));

			var jobCanNotClosable1 = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var charge3 = TestObjectCreator.CreateCharge(jobCanNotClosable1, chargeCode, "charge3", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			var transaction3 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR003", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			transaction3.Lines.Add(TestObjectCreator.CreateRevenueLine(charge3, transaction3.PK));

			var jobCanNotClosable2 = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var charge4 = TestObjectCreator.CreateCharge(jobCanNotClosable2, chargeCode, "charge4", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			var transaction4 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR004", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			transaction4.Lines.Add(TestObjectCreator.CreateRevenueLine(charge4, transaction4.PK));
			Factory.Save();

			using (AccountingConfigurationRegistry.Instance.EnableBulkDisbursementJobsClosure.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (JobManagementModule moduleToTest = new JobManagementModule())
			{
				var selectedJobs = new JobCollection(Factory) { jobClosable1, jobClosable2, jobCanNotClosable1, jobCanNotClosable2 };

				string expectedMessage1 = $@"The following jobs were closed successfully:
{ManualJobClosureHelper.GetJobNumbers(new Job[] { jobClosable1, jobClosable2 })}";

				string expectedMessage2 = $@"The following job(s) contains posted disbursement clearing balance and can only be closed via the Auto Job Closure process.
{ManualJobClosureHelper.GetJobNumbers(new Job[] { jobCanNotClosable1, jobCanNotClosable2 })}";

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				moduleToTest.CloseJobs_ForTestOnly(selectedJobs.ToArray());
				AssertContains("Message", expectedMessage1, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains("Message", expectedMessage2, UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Job should be closed", jobClosable1.IsClosed);
				Assert("Job should be closed", jobClosable2.IsClosed);
				Assert("Job shouldn't be closed", !jobCanNotClosable1.IsClosed);
				Assert("Job shouldn't be closed", !jobCanNotClosable2.IsClosed);
			}
		}

		public void TestCanNotClosedWhenSelectedJobsAreInactive()
		{
			var jobClosable = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var charge1 = TestObjectCreator.CreateCharge(jobClosable, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			var transaction1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			transaction1.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, transaction1.PK));

			var jobCanNotClosable1 = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var charge2 = TestObjectCreator.CreateCharge(jobCanNotClosable1, TestObjectCreator.CC1, "charge2", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			var transaction2 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR002", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			transaction2.Lines.Add(TestObjectCreator.CreateRevenueLine(charge2, transaction2.PK));

			var jobCanNotClosable2 = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			jobCanNotClosable2.MarkAsInactive();

			Factory.Save();

			jobCanNotClosable1.JH_Status = JobHeaderStatus.Closed.Code;
			Factory.Save();

			using (var moduleToTest = new JobManagementModule())
			{
				var selectedJobs = new JobCollection(Factory) { jobClosable, jobCanNotClosable1, jobCanNotClosable2 };
				var expectedMessage1 = $@"The following jobs were closed successfully:
{ManualJobClosureHelper.GetJobNumbers(new Job[] { jobClosable })}";

				var expectedMessage2 = $"The following job(s) are already closed:\n{ManualJobClosureHelper.GetJobNumbers(new Job[] { jobCanNotClosable1 })}";

				var expectedMessage3 = $"The following job(s) are inactive. Please activate them before closing:\n{ManualJobClosureHelper.GetJobNumbers(new Job[] { jobCanNotClosable2 })}";

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				moduleToTest.CloseJobs_ForTestOnly(selectedJobs.ToArray());
				AssertContains("When close job(s), the following message should be expected:", expectedMessage1, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains("When close job(s) which are closed, the following message should be expected:", expectedMessage2, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains("When close job(s) which are inactive, the following message should be expected:", expectedMessage3, UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Job should be closed", jobClosable.IsClosed);
				Assert("Job(s) are already closed", jobCanNotClosable1.IsClosed);
				Assert("Job(s) shouldn't be closed", !jobCanNotClosable2.IsClosed);
			}
		}

		public void TestCanNotClosedWhenOnlyContainTheJobExistDisbursementChargeWhichAlreadyPosted()
		{
			var chargeCode = TestObjectCreator.CreateChargeCode("TSB", "Test DSB charge type", Enterprise.Core.Constants.ChargeType.Disbursement, 100M, TestObjectCreator.GST1, null);
			chargeCode.AC_ChargeGroup = Enterprise.Core.Constants.ChargeType.Disbursement;
			chargeCode.AC_AG_DisbursementSurplusAccount = TestObjectCreator.GLHeader1.PK;
			chargeCode.AC_AG_DisbursementShortfallAccount = TestObjectCreator.GLHeader2.PK;

			var jobCanNotClosable1 = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var charge1 = TestObjectCreator.CreateCharge(jobCanNotClosable1, chargeCode, "charge3", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			var transaction1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR003", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			transaction1.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, transaction1.PK));

			var jobCanNotClosable2 = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var charge2 = TestObjectCreator.CreateCharge(jobCanNotClosable2, chargeCode, "charge4", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			var transaction2 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR004", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			transaction2.Lines.Add(TestObjectCreator.CreateRevenueLine(charge2, transaction2.PK));
			Factory.Save();

			using (AccountingConfigurationRegistry.Instance.EnableBulkDisbursementJobsClosure.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (JobManagementModule moduleToTest = new JobManagementModule())
			{
				var selectedJobs = new JobCollection(Factory) { jobCanNotClosable1, jobCanNotClosable2 };
				string expectedMessage1 = @"The following job(s) contains posted disbursement clearing balance and can only be closed via the Auto Job Closure process." + System.Environment.NewLine + ManualJobClosureHelper.GetJobNumbers(new Job[] { jobCanNotClosable1, jobCanNotClosable2 });

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				moduleToTest.CloseJobs_ForTestOnly(selectedJobs.ToArray());
				AssertContains("Message", expectedMessage1, UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Job shouldn't be closed", !jobCanNotClosable1.IsClosed);
				Assert("Job shouldn't be closed", !jobCanNotClosable2.IsClosed);
			}
		}

		public void TestJobCloseDBHits()
		{
			BusinessObjectFactory saveFactory = new BusinessObjectFactory();
			Enterprise.Integration.Freight.ICommonShipment[] shipments = GetShipments(saveFactory);
			Job[] jobs = GetJobsForShipments(shipments, saveFactory);
			saveFactory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			JobManagement[] jobManagements = newFactory.Load<JobManagement>(new ZQuery());
			int loadCount = newFactory.DatabaseLoadCount;
			int newLoadCount = 0;
			using (MockJobManagementModule module = new MockJobManagementModule())
			{
				module.CloseJobs_ForTestOnly(jobManagements);
				newLoadCount = module.Factory_ForTestOnly.DatabaseLoadCount;
			}
			Assert("Shouldn't be more than 20 hits to close 10 jobs", newLoadCount <= loadCount + 20);
		}

		Job[] GetJobsForShipments(Enterprise.Integration.Freight.ICommonShipment[] shipments, BusinessObjectFactory factory)
		{
			TestObjectCreator creator = new TestObjectCreator(factory);
			Job[] result = new Job[shipments.Length];
			for (int index = 0; index < shipments.Length; index++)
			{
				Job job = new Job.Loader((IJobHeaderParent)shipments[index]).TryCreateWithoutMutexForTestOnly();
				job.JH_OA_LocalChargesAddr = factory.NewWithValidTestData<OrgHeader>().Addresses[0].PK;
				job.JH_OA_AgentCollectAddr = factory.NewWithValidTestData<OrgHeader>().Addresses[0].PK;
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				job.JH_GB = GlbBranch.CurrentBranch.PK;
				Charge charge = job.Charges.AddNew();
				charge.JR_AC = creator.CC1.PK;
				charge.JR_LocalCostAmt = 100m;
				charge.JR_LocalSellAmt = 200m;
				result[index] = job;
			}
			return result;
		}

		Enterprise.Integration.Freight.ICommonShipment[] GetShipments(BusinessObjectFactory factory)
		{
			Enterprise.Integration.Freight.ICommonShipment[] result = new Enterprise.Integration.Freight.ICommonShipment[10];
			for (int index = 0; index < result.Length; index++)
			{
				result[index] = (Enterprise.Integration.Freight.ICommonShipment)factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Freight.ICommonShipment)));
			}
			return result;
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.JobManagement;
		}

		public void TestViewOperationsDetails()
		{
			BusinessObject shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Freight.ICommonShipment)));
			JobManagement testJob = Factory.NewWithValidTestData<JobManagement>();
			testJob.JH_ParentTableCode = ZArchitecture.Schema.JobShipmentSchema.Constants.Prefix;
			testJob.JH_ParentID = shipment.PK;
			Factory.Save();
			using (MockJobManagementModule moduleToTest = new MockJobManagementModule())
			{
				moduleToTest.fSelectedJob = null;
				moduleToTest.ViewOperationsDetails_ForTestOnly(null, new EventArgs());
				AssertNull("Operations Form should not be created", moduleToTest.CurrentEditForm_ForTestOnly);
				AssertNotNull("Msg about no job should be shown", UnitTestUserNotification.Instance.LastMessage.Text);
				moduleToTest.fSelectedJob = testJob;
				moduleToTest.ViewOperationsDetails_ForTestOnly(null, new EventArgs());
				AssertNotNull("Operations Form should be created", moduleToTest.CurrentEditForm_ForTestOnly);
				if (moduleToTest.CurrentEditForm_ForTestOnly != null)
				{
					moduleToTest.CurrentEditForm_ForTestOnly.Dispose();
				}
			}
		}

		public void TestMarkJobAsInactiveWhenJobHasTransactions()
		{
			var testJob = Factory.NewWithValidTestData<JobManagement>();
			testJob.JH_JobNum = "S00001001";

			var wip = Factory.NewWithValidTestData<WIP>();
			var charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_AL_ARLine = wip.PK;
			charge.JR_JH = testJob.PK;
			wip.AL_JH = testJob.PK;
			wip.AL_AG = TestObjectCreator.GLHeader1.PK;
			Factory.Save();

			using (var moduleToTest = new MockJobManagementModule())
			{
				moduleToTest.fSelectedJob = testJob;
				moduleToTest.fSelectedJobs = new JobManagement[] { testJob };
				moduleToTest.MarkJobHeaderAsInactiveEventHandler_ForTestOnly(null, new EventArgs());

				AssertNotNull("Job should not be mark as inactive", Factory.Load(typeof(Job), testJob.PK));
				var expectedText = @"This job: S00001001 cannot be deactivated.
Accounting Transaction Line(s) have been saved against this Invoicing Job Header.";
				Assert("Message should be shown", UnitTestUserNotification.Instance.LastMessage.Text.StartsWith(expectedText));
			}
		}

		public void TestMarkJobAsInactiveWhenJobHasNoTransactions()
		{
			var testJob = Factory.NewWithValidTestData<JobManagement>();
			testJob.JH_JobNum = "S00001001";
			Factory.Save();

			using (var moduleToTest = new MockJobManagementModule())
			{
				moduleToTest.fSelectedJob = testJob;
				moduleToTest.fSelectedJobs = new JobManagement[] { testJob };
				moduleToTest.MarkJobHeaderAsInactiveEventHandler_ForTestOnly(null, new EventArgs());

				AssertNotNull("Job should not be mark as inactive", Factory.Load(typeof(Job), testJob.PK));
				ZString expectedText = "Are you sure you want to mark following Invoicing Job Header(s) as inactive? S00001001";
				Assert("Message should be shown", UnitTestUserNotification.Instance.LastMessage.Text.StartsWith(expectedText));
			}
		}

		public void TestMarkJobsAsInactiveWhenJobHasNoTransactions()
		{
			var testJob1 = Factory.NewWithValidTestData<JobManagement>();
			var testJob2 = Factory.NewWithValidTestData<JobManagement>();
			var testJob3 = Factory.NewWithValidTestData<JobManagement>();
			testJob1.JH_JobNum = "S00001001";
			testJob2.JH_JobNum = "S00001002";
			testJob3.JH_JobNum = "S00001003";
			Factory.Save();

			var testJobs = new List<JobManagement>() { testJob1, testJob2, testJob3 };
			using (var moduleToTest = new MockJobManagementModule())
			{
				moduleToTest.fSelectedJobs = testJobs.ToArray();
				moduleToTest.MarkJobHeaderAsInactiveEventHandler_ForTestOnly(null, new EventArgs());

				foreach (var testJob in testJobs)
				{
					AssertNotNull("Job should not be mark as inactive", Factory.Load(typeof(Job), testJob.PK));
				}
				ZString expectedText = "Are you sure you want to mark following Invoicing Job Header(s) as inactive? S00001001, S00001002, S00001003";
				Assert("Message should be shown", UnitTestUserNotification.Instance.LastMessage.Text.StartsWith(expectedText));
			}
		}

		public void TestMarkJobsAsInactiveWhenJobHasNoTransactionsWithErrorMessages()
		{
			var testJob1 = Factory.NewWithValidTestData<JobManagement>();
			var testJob2 = Factory.NewWithValidTestData<JobManagement>();
			var testJob3 = Factory.NewWithValidTestData<JobManagement>();
			var testJob4 = Factory.NewWithValidTestData<JobManagement>();
			var testJob5 = Factory.NewWithValidTestData<JobManagement>();
			var testJob6 = Factory.NewWithValidTestData<JobManagement>();
			testJob1.JH_JobNum = "S00001001";
			testJob2.JH_JobNum = "S00001002";
			testJob3.JH_JobNum = "S00001003";
			testJob4.JH_JobNum = "S00001004";
			testJob5.JH_JobNum = "S00001005";
			testJob6.JH_JobNum = "S00001006";
			var wip = Factory.NewWithValidTestData<WIP>();
			var charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_AL_ARLine = wip.PK;
			charge.JR_JH = testJob6.PK;
			wip.AL_JH = testJob6.PK;
			wip.AL_AG = TestObjectCreator.GLHeader1.PK;
			Factory.Save();

			var testJobs1 = new List<JobManagement>() { testJob1, testJob2, testJob3, testJob4, testJob5, testJob6 };
			var testJobs2 = new List<JobManagement>() { testJob4, testJob5 };
			using (var moduleToTest1 = new MockJobManagementModule())
			{
				using (var moduleToTest2 = new MockJobManagementModule())
				{
					moduleToTest2.fSelectedJobs = testJobs2.ToArray();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					moduleToTest2.MarkJobHeaderAsInactiveEventHandler_ForTestOnly(null, new EventArgs());
					ZString expectedText2 = "The following job(s) has been marked as inactive. S00001004, S00001005";
					Assert("Message should be shown", UnitTestUserNotification.Instance.LastMessage.Text.StartsWith(expectedText2));
				}
				moduleToTest1.fSelectedJobs = testJobs1.ToArray();
				moduleToTest1.MarkJobHeaderAsInactiveEventHandler_ForTestOnly(null, new EventArgs());
				ZString expectedText1 = @"Error loading job(s), the following job(s) might have been marked as inactive by another user. S00001004, S00001005

This job: S00001006 cannot be deactivated.
Accounting Transaction Line(s) have been saved against this Invoicing Job Header.

Are you sure you want to mark following Invoicing Job Header(s) as inactive? S00001001, S00001002, S00001003";
				Assert("Message should be shown", UnitTestUserNotification.Instance.LastMessage.Text.StartsWith(expectedText1));
			}
		}

		public void TestMarkJobAsInactive_AllowInvoiceDeletion()
		{
			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateJobPlugIn(JobInvoicingConsumerTypes.Shipment), false);
			Factory.Save();

			using (var moduleToTest = new MockJobManagementModule())
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // are you sure you want to delete

				var testJobManagement = Factory.Load<JobManagement>(job.PK);
				moduleToTest.fSelectedJob = testJobManagement;
				moduleToTest.fSelectedJobs = new JobManagement[] { testJobManagement };

				moduleToTest.MarkJobHeaderAsInactiveEventHandler_ForTestOnly(null, new EventArgs());
				Assert("Job should be mark as inactive.", !(Factory.Load<Job>(job.PK)).JH_IsActive);
			}
		}

		public void TestMarkJobsAsInactive_AllowInvoiceDeletion()
		{
			var job1 = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001001"), false);
			var job2 = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001002"), false);
			var job3 = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001003"), false);
			Factory.Save();

			var testJobs = new List<JobManagement>() { Factory.Load<JobManagement>(job1.PK), Factory.Load<JobManagement>(job2.PK), Factory.Load<JobManagement>(job3.PK) };
			using (var moduleToTest = new MockJobManagementModule())
			{
				moduleToTest.fSelectedJobs = testJobs.ToArray();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // are you sure you want to delete

				moduleToTest.MarkJobHeaderAsInactiveEventHandler_ForTestOnly(null, new EventArgs());
				ZString expectedText = "The following job(s) has been marked as inactive. S00001001, S00001002, S00001003";
				Assert("Jobs should be mark as inactive.", !(Factory.Load<Job>(job1.PK)).JH_IsActive && !(Factory.Load<Job>(job2.PK)).JH_IsActive && !(Factory.Load<Job>(job3.PK)).JH_IsActive);
				Assert("Message should be shown", UnitTestUserNotification.Instance.LastMessage.Text.StartsWith(expectedText));
			}
		}

		public void TestMarkJobAsInactive_DoNotAllowInvoiceDeletion()
		{
			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateJobPlugIn(JobInvoicingConsumerTypes.WarehouseStorage), false);
			Factory.Save();

			using (var moduleToTest = new MockJobManagementModule())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				var testJobManagement = Factory.Load<JobManagement>(job.PK);
				moduleToTest.fSelectedJob = testJobManagement;
				moduleToTest.fSelectedJobs = new JobManagement[] { testJobManagement };

				moduleToTest.MarkJobHeaderAsInactiveEventHandler_ForTestOnly(null, new EventArgs());
				AssertNotNull("Job should not be mark as inactive.", (Factory.Load<Job>(job.PK)).JH_IsActive);
				Assert("Message should be shown", UnitTestUserNotification.Instance.LastMessage.Text.StartsWith("The following invoice(s) cannot be marked as inactive. " + ManualJobClosureHelper.GetJobNumbers(new Job[] { job })));
			}
		}

		public void TestMarkJobAsInactiveWhenJobNotSelected()
		{
			var testJob = Factory.NewWithValidTestData<JobManagement>();
			Factory.Save();

			using (var moduleToTest = new MockJobManagementModule())
			{
				moduleToTest.fSelectedJob = null;
				try
				{
					moduleToTest.MarkJobHeaderAsInactiveEventHandler_ForTestOnly(null, new EventArgs());
					Assert("No notifications are shown and no actions are performed when no jobs are select", true);
				}
				catch
				{
					Fail("Should be no errors. Error while deactive event should be handled correctly if job is not selected.");
				}
			}
		}

		public void TestToolbarButtons()
		{
			using (MockJobManagementModule moduleToTest = new MockJobManagementModule())
			{
				Assert(moduleToTest.ToolBarButtons.Length >= 2);
			}
		}

		public void TestReopenJobForInactiveJob()
		{
			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateJobPlugIn(JobInvoicingConsumerTypes.Shipment), false);

			job.JH_Status = JobHeaderStatus.Closed.Code;
			Factory.Save();

			using (var moduleToTest = new MockJobManagementModule())
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				var testJobManagement = Factory.Load<JobManagement>(job.PK);
				moduleToTest.fSelectedJob = testJobManagement;
				moduleToTest.fSelectedJobs = new JobManagement[] { testJobManagement };

				moduleToTest.MarkJobHeaderAsInactiveEventHandler_ForTestOnly(null, new EventArgs());
				moduleToTest.ReOpenJobEventHandler_ForTestOnly(null, new EventArgs());

				var expectedText = $"The following job is inactive. Please activate it before closing: \r\n{job.JH_JobNum}";
				AssertEquals("Message should be shown", expectedText, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestReopenJobForClosedJob()
		{
			JobManagement testJob = Factory.NewWithValidTestData<JobManagement>();
			testJob.JH_Status = JobHeaderStatus.Closed.Code;
			Factory.Save();
			using (MockJobManagementModule moduleToTest = new MockJobManagementModule())
			{
				moduleToTest.fSelectedJob = testJob;
				moduleToTest.ReOpenJobEventHandler_ForTestOnly(null, new EventArgs());

				ZString expectedText = "Are you sure you want to reopen this job?";
				Assert("Message should be shown", UnitTestUserNotification.Instance.LastMessage.Text.StartsWith(expectedText));
			}
		}

		public void TestReopenJobIfJobIsNotSelected()
		{
			JobManagement testJob = Factory.NewWithValidTestData<JobManagement>();
			testJob.JH_Status = JobHeaderStatus.Closed.Code;
			Factory.Save();
			using (MockJobManagementModule moduleToTest = new MockJobManagementModule())
			{
				moduleToTest.fSelectedJob = null;
				try
				{
					moduleToTest.ReOpenJobEventHandler_ForTestOnly(null, new EventArgs());
					Assert(true);
				}
				catch
				{
					Fail("Should be no errors. Error while reopen of job should be handled correctly if job is not selected");
				}
			}
		}

		public void TestReopenJobForOpenedJob()
		{
			JobManagement testJob = Factory.NewWithValidTestData<JobManagement>();
			Factory.Save();
			using (MockJobManagementModule moduleToTest = new MockJobManagementModule())
			{
				moduleToTest.fSelectedJob = testJob;
				moduleToTest.ReOpenJobEventHandler_ForTestOnly(null, new EventArgs());

				ZString expectedText = "This job is already open";
				Assert("Message should be shown", UnitTestUserNotification.Instance.LastMessage.Text.StartsWith(expectedText));
			}
		}

		public void TestReopenJobSecurityCheck()
		{
			JobManagement testJob = Factory.NewWithValidTestData<JobManagement>();
			Factory.Save();
			using (MockJobManagementModule moduleToTest = new MockJobManagementModule())
			{
				Env.Security.ReopenJob.IsAllowed = false;
				moduleToTest.fSelectedJob = testJob;
				moduleToTest.ReOpenJobEventHandler_ForTestOnly(null, new EventArgs());

				ZString expectedText = Env.Security.ReopenJob.ErrorMessageForNotAllowed;
				Assert("Message should be shown", UnitTestUserNotification.Instance.LastMessage.Text.StartsWith(expectedText));
			}
		}

		[TestDate(2020, 01, 01)]
		public void TestReopenJobPastAllowedReOpenPeriodSecurityCheck()
		{
			JobManagement testJob = Factory.NewWithValidTestData<JobManagement>();
			Factory.Save();
			using (var moduleToTest = new MockJobManagementModule())
			{
				var shipment = TestObjectCreator.CreateShipment("S00001", "CNSHA", "AUSYD", transportMode: TransportModes.Air);
				Job job = TestObjectCreator.CreateJob(shipment);
				job.JH_A_JOP = new ZDateTime(2019, 12, 10);
				Factory.Save();

				var newValue = new JobClosureConfigurationHeader();
				var configLine = newValue.ConfigurationCollection.AddNew();
				configLine.JobType = "SHP";
				configLine.DirectionCode = FreightShipmentDirection.Code.Import;
				configLine.Mode = TransportModes.Air;
				configLine.JobClosureDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.JobOpenDate;
				configLine.Offset = 5;
				configLine.ReopenRestrictionOffset = 5;

				AccountingConfigurationRegistry.Instance.JobClosureConfigurationSetup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newValue);

				testJob = Factory.Load<JobManagement>(job.PK);

				Env.Security.ReopenJobPastAllowedReOpenPeriod.IsAllowed = false;
				moduleToTest.fSelectedJob = testJob;
				moduleToTest.ReOpenJobEventHandler_ForTestOnly(null, new EventArgs());

				ZString expectedText = Env.Security.ReopenJobPastAllowedReOpenPeriod.ErrorMessageForNotAllowed;
				Assert("Message should be shown", UnitTestUserNotification.Instance.LastMessage.Text.StartsWith(expectedText));

				Env.Security.ReopenJob.IsAllowed = false;
				Env.Security.ReopenJobPastAllowedReOpenPeriod.IsAllowed = false;
				moduleToTest.ReOpenJobEventHandler_ForTestOnly(null, new EventArgs());

				expectedText = Env.Security.ReopenJob.ErrorMessageForNotAllowed;
				Assert("Message should be shown", UnitTestUserNotification.Instance.LastMessage.Text.StartsWith(expectedText));
			}
		}

		public void TestCloseMultipleJobsRequiringProfitLossReasonCode()
		{
			using (JobManagementModule moduleToTest = new JobManagementModule())
			{
				TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
				Job testJob = testObjectCreator.CreateJob(null, 0, null, 0);

				Charge testCharge = testJob.Charges.AddNew();
				testCharge.JR_AC = testObjectCreator.CC1.PK;
				testCharge.JR_LocalCostAmt = 200;
				testCharge.JR_LocalSellAmt = 211;

				testJob.JH_ProfitLossReasonCode = string.Empty;

				Factory.Save();

				JobManagement testJobManagement = Factory.Load<JobManagement>(testJob.PK);

				JobProfitLossReasonCodeCollection plReasonCodes = new JobProfitLossReasonCodeCollection();
				JobProfitLossReasonCode plReasonCode = plReasonCodes.AddNew();
				plReasonCode.Code = "TST";
				plReasonCode.Description = (NoResString)"Test";
				AccountingConfigurationRegistry.Instance.JobProfitLossReasonCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, plReasonCodes);

				JobProfitLossRequiringReasonParameters plRequiringReasonParameters = new JobProfitLossRequiringReasonParameters();
				plRequiringReasonParameters.ProfitThreshold = 5M;
				plRequiringReasonParameters.JobStatusCollection.AddNew().Code = JobHeaderStatus.Closed.Code;
				AccountingConfigurationRegistry.Instance.JobProfitLossRequiringReasonParameters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, plRequiringReasonParameters);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				moduleToTest.CloseJobs_ForTestOnly(new[] { testJobManagement });
				Assert("Error should be raised", UnitTestUserNotification.Instance.LastMessage.WasError);
				string expectedErrorMessage = string.Format("The following job(s) require a Reason Code because their Profit Margin falls outside the tolerated margin threshold.\r\nPlease assign a Job Profit / Loss Reason Code to these jobs.\r\n{0}\n", testJobManagement.JH_JobNum);
				AssertEquals("Error message", expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Job shouldn't be closed", !testJob.IsClosed);

				plRequiringReasonParameters.ProfitThreshold = 10M;
				AccountingConfigurationRegistry.Instance.JobProfitLossRequiringReasonParameters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, plRequiringReasonParameters);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				moduleToTest.CloseJobs_ForTestOnly(new[] { testJobManagement });
				Assert("Error should not be raised", !UnitTestUserNotification.Instance.LastMessage.WasError);
				Assert("Job should be closed", testJob.IsClosed);
			}
		}

		public void TestCloseSingleSecurityCheck()
		{
			JobManagement testJob = Factory.NewWithValidTestData<JobManagement>();
			Factory.Save();
			using (MockJobManagementModule moduleToTest = new MockJobManagementModule())
			{
				Env.Security.CloseSingleJob.IsAllowed = false;
				Env.Security.CloseMultipleJobs.IsAllowed = false;
				moduleToTest.fSelectedJob = testJob;
				moduleToTest.CloseJobs_ForTestOnly(new BusinessObject[] { testJob });

				ZString expectedText = Env.Security.CloseSingleJob.ErrorMessageForNotAllowed;
				Assert("Message should be shown", UnitTestUserNotification.Instance.LastMessage.Text.StartsWith(expectedText));
			}
		}

		public void TestCloseMultipleSingleSecurityCheck()
		{
			JobManagement testJob = Factory.NewWithValidTestData<JobManagement>();
			JobManagement testJob2 = Factory.NewWithValidTestData<JobManagement>();
			Factory.Save();
			using (MockJobManagementModule moduleToTest = new MockJobManagementModule())
			{
				Env.Security.CloseMultipleJobs.IsAllowed = false;
				moduleToTest.fSelectedJob = testJob;

				moduleToTest.CloseJobs_ForTestOnly(new BusinessObject[] { testJob, testJob2 });

				ZString expectedText = Env.Security.CloseMultipleJobs.ErrorMessageForNotAllowed;
				Assert("Message should be shown", UnitTestUserNotification.Instance.LastMessage.Text.StartsWith(expectedText));
			}
		}

		#region BulkJobClose
		public void TestCloseJobsInBulkSecurityCheck()
		{
			JobManagement testJob = Factory.NewWithValidTestData<JobManagement>();
			Factory.Save();
			using (MockJobManagementModule moduleToTest = new MockJobManagementModule())
			{
				Env.Security.CloseJobsInBulk.IsAllowed = false;
				moduleToTest.BulkJobCloseEventHandler_ForTestOnly(null, null);
				ZString expectedText = Env.Security.CloseJobsInBulk.ErrorMessageForNotAllowed;
				Assert("Message should be shown", UnitTestUserNotification.Instance.LastMessage.Text.StartsWith(expectedText));
			}
		}

		public void TestCloseJobsInBulkExcludesJobWithOpenWIPAndACR()
		{
			using (MockJobManagementModule moduleToTest = new MockJobManagementModule())
			{
				var chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
				chargeCode1.AC_GC = Env.CurrentCompany.PK;
				var job1 = CreateJob(JobInvoicingConsumerTypes.Shipment, Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.Time.CurrentLocalDate.AddDays(-10));
				var job2 = CreateJob(JobInvoicingConsumerTypes.Brokerage, Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.Time.CurrentLocalDate.AddDays(-15));

				var charge1 = CreateCharge(job1, chargeCode1, "DESC001", TestObjectCreator.AUD, 15.0m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 15.0m, TestObjectCreator.Debtor, InvoiceTypesList.Codes.FinalInvoice, Env.CurrentBranch.PK);
				TestObjectCreator.CreateAccrual(charge1);
				CreateAndPostCharge(job2, chargeCode1, "TR2", "DESC002", Env.CurrentCompany.PK, Env.CurrentBranch.PK);

				var chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
				chargeCode2.AC_GC = Env.CurrentCompany.PK;
				var job3 = CreateJob(JobInvoicingConsumerTypes.CFSLoadList, Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.Time.CurrentUtcDate.AddDays(-20));
				var job4 = CreateJob(JobInvoicingConsumerTypes.ForwardingConsol, Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.Time.CurrentUtcDate.AddDays(-15));

				var charge3 = CreateCharge(job3, chargeCode2, "DESC003", TestObjectCreator.AUD, 17.0m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 17.0m, TestObjectCreator.Debtor, InvoiceTypesList.Codes.FinalInvoice, Env.CurrentBranch.PK);
				TestObjectCreator.CreateAccrual(charge3);
				CreateAndPostCharge(job4, chargeCode2, "TR4", "DESC004", Env.CurrentCompany.PK, Env.CurrentBranch.PK);
				Factory.Save();

				job4.JH_Status = JobHeaderStatus.Working.Code;
				Factory.Save();

				AssertNumbeOfJobs(moduleToTest,
								"Job1 and Job3 have open ACR and WIP. So these Jobs are excluded by the additional filter of BulkJobCloseProcessor and only Job2 should be found for closing",
								4,
								job2.PK, job4.PK);
			}
		}

		#region Filter Tests

		public void TestCloseJobsInBulkExcludesForwardingConsolsJob()
		{
			using (MockJobManagementModule moduleToTest = new MockJobManagementModule())
			{
				ForwardingConsol forwardingConsol = TestObjectCreator.CreateConsol();
				JobHeader forwardingConsolJob = Factory.NewJobForTesting<Job>();
				forwardingConsolJob.Parent = forwardingConsol;
				forwardingConsolJob.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-20);

				ForwardingConsol forwardingConsol2 = TestObjectCreator.CreateConsol(consolNum: "C002");
				JobHeader legacyGatewayJob = TestObjectCreator.CreateJobForLegacyGateway(forwardingConsol2);
				legacyGatewayJob.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-20);

				CommonConsol cfsConsol = (CommonConsol)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(CFS.ICFSLoadListConsol)));
				JobHeader cfsConsolJob = new Job.Loader((IJobHeaderParent)cfsConsol).TryLoadOrCreateWithoutMutexForTestOnly();
				cfsConsolJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
				cfsConsol.JK_IsCFS = true;
				cfsConsolJob.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-20);

				Factory.Save();
				AssertNumbeOfJobs(moduleToTest,
								  "forwardingConsolJob should be excluded by the additional filter of BulkJobCloseProcessor and only cfsConsolJob should be found for closing",
								  3,
								  cfsConsolJob.PK, legacyGatewayJob.PK);
			}
		}

		public void TestCloseJobsInBulkExcludesSpotQuotesJob()
		{
			using (MockJobManagementModule moduleToTest = new MockJobManagementModule())
			{
				JobHeader spotQuoteJob = Factory.NewJobForTesting<JobHeader>();
				spotQuoteJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
				spotQuoteJob.JH_GB = GlbBranch.CurrentBranch.PK;
				spotQuoteJob.JH_ParentTableCode = RatingHeaderSchema.Constants.Prefix;
				spotQuoteJob.JH_JobNum = "Job1";
				spotQuoteJob.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-20);
				Factory.Save();

				AssertNumbeOfJobs(moduleToTest,
								  "Expecting collection to not contain spotQuote",
								  1,
								  null);
			}
		}

		#region TestTransportBookingQuoteJobHeaderExcluded

		public void TestCloseJobsInBulkExcludesTransportBookingQuoteJobs()
		{
			using (MockJobManagementModule moduleToTest = new MockJobManagementModule())
			{
				// Consolidation "Quote" type
				var consolidationBooking1 = Factory.New<IDtbBookingConsolidation>();
				consolidationBooking1.KB_JobType = TransportConsolidationJobTypes.Codes.QuotedBooking;

				var booking1 = CreateDtbBookingWithConsolidationPK(consolidationBooking1.PK);
				var booking2 = CreateDtbBookingWithConsolidationPK(consolidationBooking1.PK);
				var jobHeader1 = CreateJobHeaderOnDtbBooking(booking1, "Job1");
				var jobHeader2 = CreateJobHeaderOnDtbBooking(booking2, "Job2");
				jobHeader1.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-20);
				jobHeader2.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-10);

				// Consolidation "Booking" type
				var consolidationBooking2 = Factory.New<IDtbBookingConsolidation>();
				consolidationBooking2.KB_JobType = TransportConsolidationJobTypes.Codes.Booking;
				var booking3 = CreateDtbBookingWithConsolidationPK(consolidationBooking2.PK);
				var booking4 = CreateDtbBookingWithConsolidationPK(consolidationBooking2.PK);
				var jobHeader3 = CreateJobHeaderOnDtbBooking(booking3, "Job3");
				var jobHeader4 = CreateJobHeaderOnDtbBooking(booking4, "Job4");
				jobHeader3.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-10);
				jobHeader3.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-8);

				Factory.Save();

				AssertNumbeOfJobs(moduleToTest,
								  "Expecting collection to not contain jobHeader3, jobHeader4. jobHeader1 and jobHeader2 should be excluded as they are Transport Booking quote Job Header",
								  4,
								  jobHeader3.PK, jobHeader4.PK);
			}
		}

		public void TestTransportBookingJobHeaderWorkflowInTwoCompanies_NoCrossCompanyJobHeaderValidation()
		{
			EnvProxy.Instance.Registry.LightValidationEnabled = false;

			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "TC1";
			company1.GC_Name = "TEST COMP1";
			company1.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			company1.GC_RN_NKCountryCode = CountryCodes.Australia;

			var branch1 = company1.Branches.AddNew();
			branch1.GB_Code = "TB1";
			branch1.GB_BranchName = "Branch 1";
			branch1.GB_OH_OrgProxy = company1.GC_OH_OrgProxy;

			var company2 = Factory.New<GlbCompany>();
			company2.GC_Code = "TC2";
			company2.GC_Name = "TEST COMP2";
			company2.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			company2.GC_RN_NKCountryCode = CountryCodes.Australia;

			var branch2 = company2.Branches.AddNew();
			branch2.GB_Code = "TB2";
			branch2.GB_BranchName = "Branch 2";
			branch2.GB_OH_OrgProxy = company2.GC_OH_OrgProxy;

			var testOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var testAddress = testOrgHeader.Addresses.AddNew();
			testAddress.Address1 = "line1";

			var consolidationBooking = Factory.New<IDtbBookingConsolidation>();
			consolidationBooking.KB_JobType = TransportConsolidationJobTypes.Codes.QuotedBooking;

			var transportbooking = (DtbBooking)CreateDtbBookingWithConsolidationPK(consolidationBooking.PK);

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var factory = new BusinessObjectFactory();
				transportbooking = factory.Load<DtbBooking>(transportbooking.PK);

				var job1 = CreateJobHeaderOnDtbBooking(transportbooking, "JobBranch1");
				job1.JH_Description = "Branch 1 description";

				factory.Save();
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch2.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var factory = new BusinessObjectFactory();
				transportbooking = factory.Load<DtbBooking>(transportbooking.PK);

				var job2 = CreateJobHeaderOnDtbBooking(transportbooking, "JobBranch2");
				job2.JH_Description = "Branch 2 description";
				job2.JH_OA_LocalChargesAddr = testAddress.PK;

				factory.Save();

				transportbooking.Reload();
				AssertEquals("Pre-condition: Local Charges Addr was saved into db.", testOrgHeader.PK, transportbooking.Job.LocalChargesAddr.OA_OH);
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var factory = new BusinessObjectFactory();
				transportbooking = factory.Load<DtbBooking>(transportbooking.PK);

				AssertEquals("Should load job of branch 1", "Branch 1 description", transportbooking.Job.JH_Description);

				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch2.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					transportbooking = Factory.Load<DtbBooking>(transportbooking.PK);

					var criteria = (ColumnValueRanker)((IWorkflowProviderCore)transportbooking).GetTemplateSelectionCriteria();
					AssertEquals("Try load Job 2 without setting parent-child relationship", testOrgHeader.PK, criteria.GetValues(ProcessTaskTemplateSchema.P0_OH_Client)[0]);
				}

				transportbooking.KM_TransportReference = transportbooking.Job.JH_Description;
				AssertNoExceptionThrown(() => { transportbooking.RunPreSaveValidation(); });
			}
		}

		JobHeader CreateJobHeaderOnDtbBooking(IDtbBooking booking, string jobNum)
		{
			JobHeader jobHeader = new Job.Loader((IJobHeaderParent)booking).TryLoadOrCreateWithoutMutexForTestOnly();
			jobHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;
			jobHeader.JH_GB = GlbBranch.CurrentBranch.PK;
			jobHeader.JH_ParentTableCode = DtbBookingSchema.Constants.Prefix;
			jobHeader.JH_JobNum = jobNum;

			return jobHeader;
		}

		IDtbBooking CreateDtbBookingWithConsolidationPK(ZGuid consolidationPK)
		{
			IDtbBooking booking = Factory.New<IDtbBooking>();
			booking.KM_KB_Booking = consolidationPK;

			return booking;
		}

		#endregion

		public void TestCloseJobsInBulkIncludesGatewayJobs()
		{
			using (MockJobManagementModule moduleToTest = new MockJobManagementModule())
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				consol.JK_UniqueConsignRef = "JB007";
				consol.JK_AgentType = Core.Constants.AgentType.Agent;
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
				consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

				var port = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
				port.O5_PortOrCountry = "AUSYD";
				port.O5_OA_AgentOfficeAddress = consol.JK_OA_SendingForwarderAddress;
				port.O5_AgentDirection = AgentDirectionList.Codes.Both;
				port.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;
				consol.SendingForwarder.AppointedGatewayAgentPorts.Add(port);

				var consolJob = new Job.Loader(consol).TryLoadOrCreateWithoutMutexForTestOnly();
				consolJob.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-8);
				Factory.Save();

				Assert("Pre-condition", consol.IsGateway());
				AssertEquals("JH_JobNum", "JB007", consolJob.JH_JobNum);

				AssertNumbeOfJobs(moduleToTest,
								  "Expecting collection to contain consolJob",
								  1,
								  consolJob.PK);
			}
		}

		#endregion

		Job CreateJob(JobInvoicingConsumerType jobType, ZGuid companyPK, ZGuid branchPK, ZDateTime jOP)
		{
			var isLegacyGatewayJob = jobType.Code == JobInvoicingConsumerTypes.ForwardingConsol.Code;
			var plugin = isLegacyGatewayJob ? TestObjectCreator.CreateConsol() : TestObjectCreator.CreateJobPlugIn(jobType);
			var job = isLegacyGatewayJob ? TestObjectCreator.CreateJobForLegacyGateway((ForwardingConsol)plugin) : TestObjectCreator.CreateJob(plugin, false);
			job.JH_GC = companyPK;
			job.JH_GB = branchPK;
			job.JH_A_JOP = jOP;

			return job;
		}

		Charge CreateCharge(Job parentJob, AccChargeCode chargeCode, string desc, RefCurrency costCurrency, ZDecimal oSCostAmt, OrgHeader creditor, RefCurrency sellCurrency, ZDecimal oSSellAmt, OrgHeader debtor, string invoiceType, ZGuid branchPK)
		{
			Charge charge = parentJob.Charges.AddNew();
			charge.JR_AC = chargeCode.PK;
			if (desc != null)
			{
				charge.JR_Desc = desc;
			}

			charge.JR_OH_CostAccount = creditor != null ? creditor.PK : ZGuid.Empty;
			charge.JR_RX_NKCostCurrency = costCurrency != null ? costCurrency.RX_Code : ZString.Empty;
			charge.JR_OSCostAmt = oSCostAmt;

			charge.JR_OH_SellAccount = debtor.PK;
			charge.JR_RX_NKSellCurrency = sellCurrency.RX_Code;
			charge.JR_InvoiceType = invoiceType;

			charge.JR_OSSellAmt = oSSellAmt;
			charge.JR_GE = TestObjectCreator.FESDepartment.PK;
			charge.JR_GB = branchPK;
			return charge;
		}

		void CreateAndPostCharge(Job job, AccChargeCode code, string transactionNo, string description, ZGuid companyPK, ZGuid branchPK)
		{
			var charge = CreateCharge(job, code, description, TestObjectCreator.AUD, 16.0m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 16.0m, TestObjectCreator.Debtor, InvoiceTypesList.Codes.FinalInvoice, branchPK);
			charge.ChargeCode.AC_GC = companyPK;

			var apInvoice = TestObjectCreator.CreateAPInvoice<APInvoice>(transactionNo, TestObjectCreator.AUD, 1.0m, 16m, 0m, 0m, 16m, 0m, 0m);
			var apline = TestObjectCreator.CreateAPInvoiceLine(apInvoice, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, description, 16.0m);

			apline.AL_AT = charge.JR_AT_CostGSTRate = ZGuid.Empty;
			apline.AL_A9_VATClass = charge.JR_A9_CostVATClass = ZGuid.Empty;
			charge.JR_AL_APLine = apline.PK;

			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>(transactionNo + "_AR", TestObjectCreator.AUD, 1.0m, TestObjectCreator.Debtor);
			var arline = TestObjectCreator.CreateARInvoiceLine(arInvoice, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, description, 16.0m);

			arline.AL_AT = charge.JR_AT_SellGSTRate = ZGuid.Empty;
			apline.AL_A9_VATClass = charge.JR_A9_SellVATClass = ZGuid.Empty;
			charge.JR_AL_ARLine = arline.PK;
		}

		void AssertNumbeOfJobs(MockJobManagementModule moduleToTest, string message, int expectedNumberOfTotalJobs, params ZGuid[] expectedNumberOfJobsToClose)
		{
			Env.Security.CloseJobsInBulk.IsAllowed = true;
			moduleToTest.BulkJobCloseEventHandler_ForTestOnly(null, null);

			AssertNotNull(moduleToTest.bulkJobCloseForm_TestOnly);
			var processor = moduleToTest.bulkJobCloseForm_TestOnly.BusinessEntityForPersistingForm as BulkJobCloseProcessor;
			AssertNotNull(processor);

			processor.JobStatusFilter = "WRK";
			processor.JobOpenDateFilter.PropertySearch = "Date range";
			processor.JobLastEditDateFilter.PropertySearch = "Date range";
			processor.JobOpenDateFilter.Property1 = ZDateTime.Today.AddDays(-30);
			processor.JobOpenDateFilter.Property2 = ZDateTime.Today;

			using (var cmd = CargoWise.Data.Db.Connection.Command("SELECT COUNT(*) FROM dbo.JobHeader"))
			{
				var ob = cmd.ExecuteScalar();
				AssertEquals("Total Number of Jobs in the JobHeader Table", expectedNumberOfTotalJobs, Convert.ToInt32(ob));
			}

			var msg = processor.Find();
			if (expectedNumberOfJobsToClose != null)
			{
				AssertEquals("Total Number of Jobs to be closed", expectedNumberOfJobsToClose.Length, processor.JobPKs.Count);
				AssertContainsExactElementsInAnyOrder(message, processor.JobPKs, expectedNumberOfJobsToClose);
			}
			else
			{
				AssertEquals("No Job should be found", 0, processor.JobPKs.Count);
			}

			AssertEquals("No Error Message", string.Empty, msg);
			moduleToTest.bulkJobCloseForm_TestOnly.Dispose();
		}

		#endregion

		public void TestCloseSingleEvent()
		{
			JobManagement testJob = Factory.NewWithValidTestData<JobManagement>();
			Factory.Save();
			using (MockJobManagementModule moduleToTest = new MockJobManagementModule())
			{
				moduleToTest.fSelectedJob = testJob;
				moduleToTest.CloseJobs_ForTestOnly(new BusinessObject[] { testJob });

				ZString expectedText = "You are about to close the following jobs and reverse all related WIPs and ACRs";
				Assert("Message should be shown", UnitTestUserNotification.Instance.LastMessage.Text.StartsWith(expectedText));
			}
		}

		public void TestCloseSingleSecurityOverridenByMultiple()
		{
			JobManagement testJob = Factory.NewWithValidTestData<JobManagement>();
			Factory.Save();
			using (MockJobManagementModule moduleToTest = new MockJobManagementModule())
			{
				Env.Security.CloseSingleJob.IsAllowed = false;
				Env.Security.CloseMultipleJobs.IsAllowed = true;

				moduleToTest.fSelectedJob = testJob;
				moduleToTest.CloseJobs_ForTestOnly(new BusinessObject[] { testJob });

				ZString expectedText = "You do not have the appropriate security rights to run this function";
				Assert("Message should be shown", UnitTestUserNotification.Instance.LastMessage.Text.StartsWith(expectedText));
			}
		}

		[ExpectNoExceptions]
		public void TestCloseJobsHandlesOnSavingCriticalCheckException()
		{
			JobManagement testJob = Factory.NewWithValidTestData<JobManagement>();
			var job = Factory.Load<Job>(testJob.PK);
			Factory.Save();
			using (MockJobManagementModule moduleToTest = new MockJobManagementModule())
			{
				moduleToTest.fSelectedJob = testJob;

				Factory.Saving += new BusinessObjectFactory.SavingEventHandler(Factory_ThrowsCriticalException);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				var helper = ManualJobClosureHelper.CreateHelper(new[] { job });
				moduleToTest.SetFactoryForSaveCloseJobs(Factory);
				moduleToTest.CloseJobsCore_ForTestOnly(new[] { job });

				Factory.Saving -= new BusinessObjectFactory.SavingEventHandler(Factory_ThrowsCriticalException);

				ZString expectedText = "User Friendly Message";
				Assert("Message should be shown", UnitTestUserNotification.Instance.LastMessage.Text.StartsWith(expectedText));
			}
		}

		void Factory_ThrowsCriticalException(BusinessObjectFactory factory)
		{
			AccTransactionHeader header = Factory.NewWithValidTestData<AccTransactionHeader>();
			throw new OnSavingCriticalCheckException<AccTransactionHeader>(header, CriticalValidationErrorType.DummyErrorKeyForTest, "User Friendly Message", "Developer Message");
		}

		public void TestJob_OnCloseJobYesNoQuestion()
		{
			JobManagement testJob = Factory.NewWithValidTestData<JobManagement>();
			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "1234", TestObjectCreator.AUD, 1M, 100M, 0M, 100M, 0M, TestObjectCreator.AALSHI, TestObjectCreator.CC1.PK);
			var line = invoice.Lines[0];
			line.AL_JH = testJob.PK;
			line.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			var charge = Factory.NewWithValidTestData<Charge>();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_JH = testJob.PK;
			charge.JR_OSCostAmt = 100M;
			charge.JR_AL_APLine = line.PK;
			line.AL_AT = ZGuid.Empty;
			line.AL_OSAmount = line.AL_LineAmount = -charge.JR_OSCostAmt;
			Factory.Save();
			AssertEquals("Precondition: line shouldn't be recognized.", ZDateTime.Empty, line.AL_ReverseDate);

			using (MockJobManagementModule moduleToTest = new MockJobManagementModule())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				moduleToTest.fSelectedJob = testJob;
				moduleToTest.CloseJobs_ForTestOnly(new BusinessObject[] { testJob });

				ZString expectedText = "contains unrecognized revenue and cannot be closed or completed.\r\nPlease recognize revenue first and try again.";
				AssertContains("Error should be shown", expectedText, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				moduleToTest.CloseJobs_ForTestOnly(new BusinessObject[] { testJob });

				expectedText = "The following jobs were closed successfully";
				AssertContains("Message should be shown", expectedText, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPostingSecurity()
		{
			var job1 = TestObjectCreator.CreateJob(TestObjectCreator.CreateJobPlugIn(JobInvoicingConsumerTypes.Shipment), false);
			var job2 = TestObjectCreator.CreateJob(TestObjectCreator.CreateJobPlugIn(JobInvoicingConsumerTypes.Brokerage), false);
			Factory.Save();

			var job1BulkPostAllCheckpoint = new JobInvoicingSecurityHelper(job1.PlugInData.InvoicingSupporter.JobInvoicingSecurity).GetInvSecurity(SecurityCore.BulkPostAll);
			var job2BulkPostAllCheckpoint = new JobInvoicingSecurityHelper(job2.PlugInData.InvoicingSupporter.JobInvoicingSecurity).GetInvSecurity(SecurityCore.BulkPostAll);
			job1BulkPostAllCheckpoint.IsAllowed = false;
			job2BulkPostAllCheckpoint.IsAllowed = false;

			using (var moduleToTest = (JobManagementModule)ZModuleFactory.Instance.Create(GetModuleID()))
			using (ZForm form = new ZForm())
			{
				form.Controls.Add(moduleToTest.EmbeddedControl);
				form.Show();
				moduleToTest.PerformSearch_ForTest();

				var grid = (ZDisplayGrid)moduleToTest.DisplayGrid;
				grid.SelectAllElements();
				var selectedObjects = moduleToTest.GetSelectedBusinessObjects();
				AssertEquals("Jobs should be selected.", 2, selectedObjects.Length);

				var postMenuItem = MenuAssertion.AssertHasMenu(moduleToTest.GetNewActionMenuItems_ForTestOnly().FindByText("&Post"), "Post All Charges and Costs");

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				postMenuItem.PerformClick();
				string expected =
@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

" +
(selectedObjects[0].PK == job1.PK ?
@"Operate -> Forwarding -> Shipments -> Billing -> Bulk Job Billing Actions -> Post All Charges and Cost
Operate -> Customs -> Customs Declarations -> Billing -> Bulk Job Billing Actions -> Post All Charges and Cost"
:
@"Operate -> Customs -> Customs Declarations -> Billing -> Bulk Job Billing Actions -> Post All Charges and Cost
Operate -> Forwarding -> Shipments -> Billing -> Bulk Job Billing Actions -> Post All Charges and Cost");

				AssertEquals("Should be Access Denied", expected, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCloseJobsLinkedWithApportionedConsolCost()
		{
			new AccountingPeriodTestHelper().SetupPeriods();

			var shipment1 = TestObjectCreator.CreateShipment("S0001", "AUSYD", "USLAX");
			shipment1.JS_ActualWeight = 151.73m;
			shipment1.JS_ActualChargeable = 13m;
			var consol = TestObjectCreator.CreateConsol("AUSYD", "USLAX", "C001");
			consol.Shipments.Add(shipment1);

			ApportionmentListing listing = new ApportionmentListing(Factory, consol);
			JobConsolCost costCC1 = listing.CostsCollection.TryAddNew();
			costCC1.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			costCC1.E6_OSCostAmount = 50m;

			Factory.Save();

			((Job)consol.Shipments[0].Job).JH_GB = GlbBranch.CurrentBranch.PK;
			((Job)consol.Shipments[0].Job).JH_GE = GlbDepartment.CurrentDepartment.PK;
			((Job)consol.Shipments[0].Job).Charges[0].JR_GB = GlbBranch.CurrentBranch.PK;
			((Job)consol.Shipments[0].Job).Charges[0].JR_GE = GlbDepartment.CurrentDepartment.PK;
			((Job)consol.Shipments[0].Job).Charges[0].ParentConsolCost.ApportionmentCharges[0].JR_IsUsedForApportionment = true;

			Factory.Save();

			var factoryForJobReopening = new BusinessObjectFactory();
			var creator = new TestObjectCreator(factoryForJobReopening);
			var melbourneBranch = creator.CreateBranch("BR3", "Test Branch 3", GlbCompany.CurrentCompany);
			melbourneBranch.GB_IsActive = true;
			var melbourneDepartment = creator.CreateDepartment("DEP");
			melbourneDepartment.GE_IsActive = true;

			var oldBranch = GlbBranch.CurrentBranch;
			var oldDept = GlbDepartment.CurrentDepartment;

			factoryForJobReopening.Save();

			Job job1 = factoryForJobReopening.Load(typeof(Job), consol.Shipments[0].Job.PK) as Job;
			Assert("Job status should be not be closed", !job1.IsClosed);

			GlbStaff testUser = TestObjectCreator.CreateStaffWithSecurityRights("Lionel Messi", "LM", Env.Security.None.Code);

			testUser.GS_IsController = false;
			testUser.GS_IsOperational = true;

			using (Env.SetTemporaryUserContext(testUser.GS_LoginName, melbourneBranch.PK.ToGuid(), melbourneDepartment.PK.ToGuid()))
			{
				using (JobManagementModule moduleToTest = new JobManagementModule())
				{
					var securityFactory = new BusinessObjectFactory();

					SecurityCore security = new UserLoginController().GetSecurityForUser(testUser.GS_LoginName, oldBranch.PK.ToGuid(), oldDept.PK.ToGuid());
					security.CloseSingleJob.IsAllowed = true;
					Env.Security.CloseSingleJob.IsAllowed = true;
					security.Login.IsAllowed = false;
					security.MaintainShipmentJobInvoicing.IsAllowed = false;

					var loginSecurity = securityFactory.New<GlbSecurity>();
					loginSecurity.GU_GB = oldBranch.PK;
					loginSecurity.GU_GE = oldDept.PK;
					loginSecurity.GU_GC = oldBranch.Company.PK;
					loginSecurity.GU_GS = Env.CurrentUser.PK;
					loginSecurity.GU_SecurityRight = security.Login.Code;
					loginSecurity.GU_SecurityItemIsAllowed = false;

					var maintainShipmentSecurity = securityFactory.New<GlbSecurity>();
					maintainShipmentSecurity.GU_GB = oldBranch.PK;
					maintainShipmentSecurity.GU_GE = oldDept.PK;
					maintainShipmentSecurity.GU_GC = oldBranch.Company.PK;
					maintainShipmentSecurity.GU_GS = Env.CurrentUser.PK;
					maintainShipmentSecurity.GU_SecurityRight = security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowEnterModifyCharges).Code;
					security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowEnterModifyCharges).IsAllowed = true;
					maintainShipmentSecurity.GU_SecurityItemIsAllowed = true;

					securityFactory.Save();

					var selectedJobs = new JobCollection(factoryForJobReopening) { job1 };
					selectedJobs[0].Parent = null;
					selectedJobs[0].JH_ParentID = ZGuid.Empty;

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					moduleToTest.CloseJobs_ForTestOnly(selectedJobs.ToArray());
					AssertContains("Message", "The following jobs were closed successfully:\r\nS0001", UnitTestUserNotification.Instance.LastMessage.Text);
					Assert("Error shouldn't be raised", !UnitTestUserNotification.Instance.LastMessage.WasError);
					Assert("Job should be closed", job1.IsClosed);
				}
			}
		}

		[SuspendCriticalValidation]
		public void TestCloseMultipleJobsLinkedWithUnpostedConsolCost()
		{
			//Preparing Shipment				
			var shipment1 = TestObjectCreator.CreateShipment("S0001", "AUSYD", "USLAX");
			shipment1.JS_ActualWeight = 151.73m;
			shipment1.JS_ActualChargeable = 13m;

			var shipment2 = TestObjectCreator.CreateShipment("S0002", "AUSYD", "USLAX");
			shipment2.JS_ActualWeight = 251.73m;
			shipment2.JS_ActualChargeable = 23m;

			var shipment3 = TestObjectCreator.CreateShipment("S0003", "AUSYD", "USLAX");
			shipment3.JS_ActualWeight = 351.73m;
			shipment3.JS_ActualChargeable = 33m;

			var shipment4 = TestObjectCreator.CreateShipment("S0004", "AUSYD", "USLAX");
			shipment4.JS_ActualWeight = 451.73m;
			shipment4.JS_ActualChargeable = 43m;

			ForwardingShipment[] shipments = new ForwardingShipment[] { shipment1, shipment2, shipment3, shipment4 };

			//Creating Consol Test
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.Shipments.Add(shipments[0]);
			consol.Shipments.Add(shipments[1]);
			consol.Shipments.Add(shipments[2]);
			consol.Shipments.Add(shipments[3]);

			ApportionmentListing listing = new ApportionmentListing(Factory, consol);

			//ConsolCost1
			JobConsolCost costCC1 = listing.CostsCollection.TryAddNew();
			costCC1.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			costCC1.E6_OSCostAmount = 50m;

			//ConsolCost2
			JobConsolCost costCC2 = listing.CostsCollection.TryAddNew();
			costCC2.E6_AC_ChargeCode = TestObjectCreator.CC2.PK;
			costCC2.E6_OSCostAmount = 100m;

			Factory.Save();

			//s1
			((Job)consol.Shipments[0].Job).Charges[0].ParentConsolCost.ApportionmentCharges[0].JR_IsUsedForApportionment = true;
			//s2
			((Job)consol.Shipments[1].Job).Charges[0].ParentConsolCost.ApportionmentCharges[1].JR_IsUsedForApportionment = true;
			//s3
			((Job)consol.Shipments[2].Job).Charges[0].ParentConsolCost.ApportionmentCharges[2].JR_IsUsedForApportionment = true;
			//s4
			((Job)consol.Shipments[3].Job).Charges[0].ParentConsolCost.ApportionmentCharges[3].JR_IsUsedForApportionment = false;

			//s1
			((Job)consol.Shipments[0].Job).Charges[1].ParentConsolCost.ApportionmentCharges[0].JR_IsUsedForApportionment = false;
			//s2
			((Job)consol.Shipments[1].Job).Charges[1].ParentConsolCost.ApportionmentCharges[1].JR_IsUsedForApportionment = true;
			//s3
			((Job)consol.Shipments[2].Job).Charges[1].ParentConsolCost.ApportionmentCharges[2].JR_IsUsedForApportionment = true;
			//s4
			((Job)consol.Shipments[3].Job).Charges[1].ParentConsolCost.ApportionmentCharges[3].JR_IsUsedForApportionment = false;

			Factory.Save();

			Job job1 = Factory.Load(typeof(Job), consol.Shipments[0].Job.PK) as Job;
			Job job2 = Factory.Load(typeof(Job), consol.Shipments[1].Job.PK) as Job;
			Job job3 = Factory.Load(typeof(Job), consol.Shipments[2].Job.PK) as Job;

			using (JobManagementModule moduleToTest = new JobManagementModule())
			{
				//For Incorrect Selection
				var selectedJobs = new JobCollection(Factory) { job1, job2 };
				string expectedMessage = string.Format("Following job(s) contain apportioned charges.\r\nTo close Job: {0} please select {2}\r\nTo close Job: {1} please select {2}\r\n\n", consol.Shipments[0].Job.JH_JobNum, consol.Shipments[1].Job.JH_JobNum, consol.Shipments[2].Job.JH_JobNum);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				moduleToTest.CloseJobs_ForTestOnly(selectedJobs.ToArray());
				Assert("Error should be raised", UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Error message", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Job shouldn't be closed", !job1.IsClosed);
				Assert("Job shouldn't be closed", !job2.IsClosed);
				Assert("Job shouldn't be closed", !job3.IsClosed);

				//For Correct Selection
				selectedJobs = new JobCollection(Factory) { job1, job2, job3 };
				expectedMessage = string.Format(@"jobs were closed successfully");

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				moduleToTest.CloseJobs_ForTestOnly(selectedJobs.ToArray());
				Assert("Error shouldn't be raised", !UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertContains("Message", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				Assert("Job should be closed", job1.IsClosed);
				Assert("Job should be closed", job2.IsClosed);
				Assert("Job should be closed", job3.IsClosed);
			}
		}

		public void TestCloseMultipleJobsLinkedWithoutConsolCost()
		{
			var shipment1 = TestObjectCreator.CreateShipment("S0001", "AUSYD", "USLAX");
			shipment1.JS_ActualWeight = 151.73m;
			shipment1.JS_ActualChargeable = 13m;
			var job1 = TestObjectCreator.CreateJob(shipment1, TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);

			var shipment2 = TestObjectCreator.CreateShipment("S0002", "AUSYD", "USLAX");
			shipment2.JS_ActualWeight = 251.73m;
			shipment2.JS_ActualChargeable = 23m;
			var job2 = TestObjectCreator.CreateJob(shipment2, TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);

			var shipment3 = TestObjectCreator.CreateShipment("S0003", "AUSYD", "USLAX");
			shipment3.JS_ActualWeight = 351.73m;
			shipment3.JS_ActualChargeable = 33m;
			var job3 = TestObjectCreator.CreateJob(shipment3, TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);

			var shipment4 = TestObjectCreator.CreateShipment("S0004", "AUSYD", "USLAX");
			shipment4.JS_ActualWeight = 451.73m;
			shipment4.JS_ActualChargeable = 43m;
			var job4 = TestObjectCreator.CreateJob(shipment4, TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);

			Factory.Save();

			using (JobManagementModule moduleToTest = new JobManagementModule())
			{
				//For Correct Selection
				var selectedJobs = new JobCollection(Factory) { job1, job2, job3 };
				string expectedMessage = string.Format(@"jobs were closed successfully");

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				moduleToTest.CloseJobs_ForTestOnly(selectedJobs.ToArray());
				Assert("Error shouldn't be raised", !UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertContains("Message", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				Assert("Job should be closed", job1.IsClosed);
				Assert("Job should be closed", job2.IsClosed);
				Assert("Job should be closed", job3.IsClosed);
			}
		}

		public void TestCloseMultipleJobsAllCombinations()
		{
			//Creating Jobs That linked with Consols				
			var shipment1 = TestObjectCreator.CreateShipment("S0001", "AUSYD", "USLAX");
			shipment1.JS_ActualWeight = 151.73m;
			shipment1.JS_ActualChargeable = 33m;

			var shipment2 = TestObjectCreator.CreateShipment("S0002", "AUSYD", "USLAX");
			shipment2.JS_ActualWeight = 251.73m;
			shipment2.JS_ActualChargeable = 33m;

			var shipment3 = TestObjectCreator.CreateShipment("S0003", "AUSYD", "USLAX");
			shipment3.JS_ActualWeight = 351.73m;
			shipment3.JS_ActualChargeable = 33m;

			var shipment4 = TestObjectCreator.CreateShipment("S0004", "AUSYD", "USLAX");
			shipment4.JS_ActualWeight = 451.73m;
			shipment4.JS_ActualChargeable = 43m;

			ForwardingShipment[] shipments = new ForwardingShipment[] { shipment1, shipment2, shipment3, shipment4 };

			//Creating Consol Test
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.Shipments.Add(shipments[0]);
			consol.Shipments.Add(shipments[1]);
			consol.Shipments.Add(shipments[2]);
			consol.Shipments.Add(shipments[3]);

			ApportionmentListing listing = new ApportionmentListing(Factory, consol);

			//ConsolCost1
			JobConsolCost costCC1 = listing.CostsCollection.TryAddNew();
			costCC1.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			costCC1.E6_OSCostAmount = 50m;

			//ConsolCost2
			JobConsolCost costCC2 = listing.CostsCollection.TryAddNew();
			costCC2.E6_AC_ChargeCode = TestObjectCreator.CC2.PK;
			costCC2.E6_OSCostAmount = 100m;

			Factory.Save();

			//s1
			((Job)consol.Shipments[0].Job).Charges[0].ParentConsolCost.ApportionmentCharges[0].JR_IsUsedForApportionment = true;
			//s2
			((Job)consol.Shipments[1].Job).Charges[0].ParentConsolCost.ApportionmentCharges[1].JR_IsUsedForApportionment = true;
			//s3
			((Job)consol.Shipments[2].Job).Charges[0].ParentConsolCost.ApportionmentCharges[2].JR_IsUsedForApportionment = true;
			//s4
			((Job)consol.Shipments[3].Job).Charges[0].ParentConsolCost.ApportionmentCharges[3].JR_IsUsedForApportionment = false;

			//s1
			((Job)consol.Shipments[0].Job).Charges[1].ParentConsolCost.ApportionmentCharges[0].JR_IsUsedForApportionment = false;
			//s2
			((Job)consol.Shipments[1].Job).Charges[1].ParentConsolCost.ApportionmentCharges[1].JR_IsUsedForApportionment = true;
			//s3
			((Job)consol.Shipments[2].Job).Charges[1].ParentConsolCost.ApportionmentCharges[2].JR_IsUsedForApportionment = true;
			//s4
			((Job)consol.Shipments[3].Job).Charges[1].ParentConsolCost.ApportionmentCharges[3].JR_IsUsedForApportionment = false;

			Factory.Save();

			//Closed Job
			var testJobClosed = TestObjectCreator.CreateJob(TestObjectCreator.Creditor1, 0, null, 0);
			testJobClosed.JH_Status = JobHeaderStatus.Closed.Code;

			//Complete Job
			Env.Security.ChangeStatusOfCompleteJobs.IsAllowed = false;
			var testJobCompleted = TestObjectCreator.CreateJob(TestObjectCreator.Creditor1, 0, null, 0);
			testJobCompleted.JH_Status = JobHeaderStatus.Complete.Code;

			//Requires Profit and Loss reason
			var testJobRequiresReasonCode = TestObjectCreator.CreateJob(null, 0, null, 0);
			var testCharge = testJobRequiresReasonCode.Charges.AddNew();
			testCharge.JR_AC = TestObjectCreator.CC1.PK;
			testCharge.JR_LocalCostAmt = 200;
			testCharge.JR_LocalSellAmt = 211;
			testJobRequiresReasonCode.JH_ProfitLossReasonCode = string.Empty;

			var plReasonCodes = new JobProfitLossReasonCodeCollection();
			var plReasonCode = plReasonCodes.AddNew();
			plReasonCode.Code = "TST";
			plReasonCode.Description = (NoResString)"Test";
			AccountingConfigurationRegistry.Instance.JobProfitLossReasonCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, plReasonCodes);

			var plRequiringReasonParameters = new JobProfitLossRequiringReasonParameters();
			plRequiringReasonParameters.ProfitThreshold = 5M;
			plRequiringReasonParameters.JobStatusCollection.AddNew().Code = JobHeaderStatus.Closed.Code;
			AccountingConfigurationRegistry.Instance.JobProfitLossRequiringReasonParameters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, plRequiringReasonParameters);

			//Job That Can be Closed
			var jobClosable = TestObjectCreator.CreateJob(TestObjectCreator.Creditor1, 0, null, 0);

			Factory.Save();

			var job1 = Factory.Load(typeof(Job), consol.Shipments[0].Job.PK) as Job;
			var job2 = Factory.Load(typeof(Job), consol.Shipments[1].Job.PK) as Job;
			var job3 = Factory.Load(typeof(Job), consol.Shipments[2].Job.PK) as Job;

			using (JobManagementModule moduleToTest = new JobManagementModule())
			{
				string reasonCodeRequiredJobNumbers = ManualJobClosureHelper.GetJobNumbers(new Job[] { testJobRequiresReasonCode, job1, job2 });

				//For Correct Selection
				var selectedJobs = new JobCollection(Factory) { job1, job2, testJobClosed, testJobCompleted, testJobRequiresReasonCode, jobClosable };
				string expectedMessage = string.Format("The following jobs were closed successfully:\r\n{0}\r\n\r\nThe following job(s) are already closed:\n{1}\n\r\nThe following job(s) require a Reason Code because their Profit Margin falls outside the tolerated margin threshold.\r\nPlease assign a Job Profit / Loss Reason Code to these jobs.\r\n{2}\n\r\nThe following job(s) are complete and you are not allowed to change their status due to security rights:\r\n{5}\n\r\nFollowing job(s) contain apportioned charges.\r\nTo close Job: {3} please select {6}\r\nTo close Job: {4} please select {6}\r\n\n",
					jobClosable.JH_JobNum, testJobClosed.JH_JobNum, reasonCodeRequiredJobNumbers, job1.JH_JobNum, job2.JH_JobNum, testJobCompleted.JH_JobNum, job3.JH_JobNum);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				moduleToTest.CloseJobs_ForTestOnly(selectedJobs.ToArray());
				Assert("Error shouldn't be raised", !UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertContains("Message", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				Assert("Job shouldn't be closed", !job1.IsClosed);
				Assert("Job shouldn't be closed", !job2.IsClosed);
				Assert("Job shouldn't be closed", !job3.IsClosed);
				Assert("Job shouldn't be closed", !testJobCompleted.IsClosed);
				Assert("Job shouldn't be closed", !testJobRequiresReasonCode.IsClosed);
				Assert("Job should be closed", jobClosable.IsClosed);
			}
		}

		public void TestJCSDiagnosisAction()
		{
			var shipment1 = TestObjectCreator.CreateShipment("S0001", "AUSYD", "USLAX");
			shipment1.JS_ActualWeight = 151.73m;
			shipment1.JS_ActualChargeable = 13m;
			var job1 = TestObjectCreator.CreateJob(shipment1, TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);
			Factory.Save();

			using (JobManagementModule moduleToTest = new JobManagementModule())
			using (ZForm form = new ZForm())
			{
				form.Controls.Add(moduleToTest.EmbeddedControl);
				form.Show();

				//For Correct Selection
				var jcsDiagnosisMenuItem = MenuAssertion.AssertHasMenu(moduleToTest.GetNewActionMenuItems_ForTestOnly(), "Diagnose Automatic Job Closure");
				jcsDiagnosisMenuItem.PerformClick();
				AssertContains("Message", "No job is selected for diagnosis.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();

				moduleToTest.PerformSearch_ForTest();
				var grid = (ZDisplayGrid)moduleToTest.DisplayGrid;
				grid.SelectAllElements();
				var selectedObjects = moduleToTest.GetSelectedBusinessObjects();
				AssertEquals("Jobs should be selected.", 1, selectedObjects.Length);

				jcsDiagnosisMenuItem.PerformClick();
				AssertType(typeof(AutoJobClosureDiagnosisForm), ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestCloseJobsWithCanNotDeletedUnpostedConsolCosts()
		{
			var testJob1 = Factory.NewWithValidTestData<JobManagement>();
			testJob1.JH_Status = JobHeaderStatus.Working.Code;
			var shipment1 = TestObjectCreator.CreateShipment("S0001", "AUSYD", "USLAX");
			shipment1.JS_ActualWeight = 151.73m;
			shipment1.JS_ActualChargeable = 13m;
			var testJob2 = TestObjectCreator.CreateJob(shipment1, TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);
			testJob2.Charges.RemoveAll();
			testJob2.JH_Status = JobHeaderStatus.JobReadyForFinancialClosure.Code;

			var consol = Factory.New<ForwardingConsol>();
			consol.Shipments.Add(shipment1);

			var listing = new ApportionmentListing(Factory, consol);

			var costCC1 = listing.CostsCollection.TryAddNew();
			costCC1.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			costCC1.E6_OSCostAmount = 50m;

			costCC1.ApportionmentCharges[0].JR_IsUsedForApportionment = true;
			costCC1.ApportionmentCharges[0].JR_E6 = costCC1.PK;

			var job1 = Factory.Load<Job>(testJob1.PK);
			var job2 = Factory.Load<Job>(testJob2.PK);

			Factory.Save();

			job2.Charges[0].OnFactorySavingBeforeTransactionCore_InvokeTestOnly += (JobCharge jobCharge) => jobCharge.Job.JH_Status = JobHeaderStatus.Closed.Code;

			Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = false;

			using (var moduleToTest = new MockJobManagementModule())
			{
				moduleToTest.fSelectedJob = testJob1;

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				moduleToTest.CloseJobsCore_ForTestOnly(new[] { job1, job2 });

				var expectedText1 = @$"The following jobs were closed successfully:
{job1.JH_JobNum}";
				Assert("Message should be shown", UnitTestUserNotification.Instance.LastMessage.Text.StartsWith(expectedText1));

				var expectedText2 = @$"The following job(s) contains unposted consol costs which can't be deleted.
{job2.JH_JobNum}";
				Assert("Message should be shown", UnitTestUserNotification.Instance.LastMessage.Text.Contains(expectedText2));
			}

			AssertEquals(JobHeaderStatus.Closed.Code, job1.JH_Status);
			AssertEquals(JobHeaderStatus.JobReadyForFinancialClosure.Code, job2.JH_Status);
		}

		#region Invalid Booking With Quote Tests

		public void TestCloseJobs_WhenParentIsInvalidBooking()
		{
			var job = CreateInvalidQBJob("0001", "S0001");
			Factory.Save();

			var shipment = Factory.Load<CommonShipment>(job.JH_ParentID);
			shipment.JS_UniqueConsignRef = "0001/A";
			Factory.Save();

			using (var jobModule = new MockJobManagementModule())
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				jobModule.CloseJobs_ForTestOnly(new Job[] { job });
				Assert("Should have recieved Error", UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Prompt Message", $"This Job Billing header (0001/A) is not currently valid as the Booking has not been converted into a Shipment. Consolidate the Booking before proceeding with Job Closure.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCloseJobs_WhenParentIsInvalidBooking_MultipleBookings()
		{
			var job1 = CreateInvalidQBJob("0001", "S0001");
			var job2 = CreateInvalidQBJob("0002", "S0002");
			Factory.Save();

			var shipment = Factory.Load<CommonShipment>(job1.JH_ParentID);
			shipment.JS_UniqueConsignRef = "0001/A";
			Factory.Save();

			using (var jobModule = new MockJobManagementModule())
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				jobModule.CloseJobs_ForTestOnly(new Job[] { job1, job2 });
				Assert("Should have recieved Error", UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Prompt Message", "This Job Billing header (0001/A) is not currently valid as the Booking has not been converted into a Shipment. Consolidate the Booking before proceeding with Job Closure.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		Job CreateInvalidQBJob(string quoteNum, string bookingNum)
		{
			var booking = Factory.New<CommonShipment>();
			booking.JS_IsForwardRegistered = false;
			booking.JS_IsCFSRegistered = false;
			booking.JS_IsShipping = false;
			booking.JS_IsBooking = true;
			booking.JS_UniqueConsignRef = bookingNum;

			var quotedBooking = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.SpotQuote, Factory);
			var quote = quotedBooking.Quote as IQuote;
			quote.TH_QuoteNumber = quoteNum;

			var bookingWithQuote = (IJobInvoicingPlugIn)ObjectFactory.Get<IQuotedBookingBuilder>().InitializeFrom(((BusinessObject)quote).PK, booking.PK, Factory);

			Factory.Save();

			TestObjectCreator.CreateJob(bookingWithQuote);
			var job2 = TestObjectCreator.CreateJob(booking);
			return job2;
		}

		#endregion

		#region Concurrency Tests

		public void TestDeactivateEventHandlerForConcurrencyException_WithSeperateFactoryForDeactivatingJob()
		{
			var shipment = TestObjectCreator.CreateShipment("S10000");
			var job = TestObjectCreator.CreateJob(shipment);
			Factory.Save();

			using (var module1 = new MockJobManagementModule())
			{
				module1.Factory_ForTestOnly.RefreshEnabled = false;
				module1.fSelectedJob = module1.Factory_ForTestOnly.Load<JobManagement>(job.PK);
				module1.fSelectedJobs = new JobManagement[] { module1.fSelectedJob };

				using (var module2 = new MockJobManagementModule())
				{
					module2.Factory_ForTestOnly.RefreshEnabled = false;
					module2.fSelectedJob = module2.Factory_ForTestOnly.Load<JobManagement>(job.PK);
					module2.fSelectedJobs = new JobManagement[] { module2.fSelectedJob };
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					module2.MarkJobHeaderAsInactiveEventHandler_ForTestOnly(null, new EventArgs());
					Assert("Message should be shown", UnitTestUserNotification.Instance.LastMessage.Text.StartsWith("The following job(s) has been marked as inactive. S10000"));
				}

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				module1.MarkJobHeaderAsInactiveEventHandler_ForTestOnly(null, new EventArgs());
				Assert("Message should be shown", UnitTestUserNotification.Instance.LastMessage.Text.StartsWith(JobManagementModule.MissingDeactivatedJobMessage + "S10000"));
			}
		}

		public void TestDeactivateEventHandlerForConcurrencyException_WithSeperateFactoryForDeactivatingJobAndWithingSmallTimeFrameBeforeSave()
		{
			var shipment = TestObjectCreator.CreateShipment("S10000");
			var job = TestObjectCreator.CreateJob(shipment);
			Factory.Save();

			using (var module = new MockJobManagementModuleForConcurrencyTesting())
			{
				module.Factory_ForTestOnly.RefreshEnabled = false;
				module.fSelectedJob = module.Factory_ForTestOnly.Load<JobManagement>(job.PK);
				module.fSelectedJobs = new JobManagement[] { module.fSelectedJob };
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				module.MarkJobHeaderAsInactiveEventHandler_ForTestOnly(null, new EventArgs());
				Assert("Message should be shown", UnitTestUserNotification.Instance.LastMessage.Text.StartsWith(JobManagementModule.UserFriendlyConcurrencyMessage + "S10000"));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1099:ResGetStringDefaultTextMustBeStringLiteral", Justification = "Testing")]
		public void TestDeactivateEventHandlerForConcurrencyException_WithSeperateFactoryForDeactivatingJobsAndWithingSmallTimeFrameBeforeSave()
		{
			var job1 = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001001"), false);
			var job2 = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001002"), false);
			var job3 = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001003"), false);
			Factory.Save();

			var testJobs = new List<JobManagement>() { Factory.Load<JobManagement>(job1.PK), Factory.Load<JobManagement>(job2.PK), Factory.Load<JobManagement>(job3.PK) };
			using (var moduleToTest = new MockJobManagementModuleForConcurrencyTesting())
			{
				moduleToTest.fSelectedJobs = testJobs.ToArray();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				moduleToTest.MarkJobHeaderAsInactiveEventHandler_ForTestOnly(null, new EventArgs());
				ZString expectedText = JobManagementModule.UserFriendlyConcurrencyMessage + ManualJobClosureHelper.GetJobNumbers(new Job[] { job1, job2, job3 });
				Assert("Message should be shown", UnitTestUserNotification.Instance.LastMessage.Text.StartsWith(expectedText));
			}
		}

		public void TestReopenJobEventHandlerForConcurrencyException_WithSeperateFactoryForJobReopening()
		{
			var job = Factory.NewWithValidTestData<JobManagement>();
			job.JH_Status = JobHeaderStatus.Closed.Code;
			Factory.Save();

			using (var module1 = new MockJobManagementModule())
			{
				module1.Factory_ForTestOnly.RefreshEnabled = false;
				module1.fSelectedJob = module1.Factory_ForTestOnly.Load<JobManagement>(job.PK);

				using (var module2 = new MockJobManagementModule())
				{
					module2.Factory_ForTestOnly.RefreshEnabled = false;
					module2.fSelectedJob = module2.Factory_ForTestOnly.Load<JobManagement>(job.PK);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					module2.ReOpenJobEventHandler_ForTestOnly(null, new EventArgs());
				}

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				module1.ReOpenJobEventHandler_ForTestOnly(null, new EventArgs());
				AssertEquals("Job should be already opened", "This job is already open", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestReopenJobEventHandlerForConcurrencyException_WithSeperateFactoryForJobReopeningAndWithingSmallTimeFrameBeforeSave()
		{
			var job = Factory.NewWithValidTestData<JobManagement>();
			job.JH_Status = JobHeaderStatus.Closed.Code;
			Factory.Save();

			using (var module = new MockJobManagementModuleForConcurrencyTesting())
			{
				module.fSelectedJob = module.Factory_ForTestOnly.Load<JobManagement>(job.PK);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				module.ReOpenJobEventHandler_ForTestOnly(null, new EventArgs());
				AssertEquals("User freindly message must be shown", JobManagementModule.UserFriendlyConcurrencyMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public class MockJobManagementModuleForConcurrencyTesting : MockJobManagementModule
		{
			protected override void ReopenJobCore(Job jobToReOpen)
			{
				MakeChangestoJobInNewFactory(jobToReOpen, (jobToChange) => jobToChange.JH_Status = JobHeaderStatus.Working.Code);
				base.ReopenJobCore(jobToReOpen);
			}

			protected override void DeleteJobCore(Job jobToDelete)
			{
				MakeChangestoJobInNewFactory(jobToDelete, (jobToChange) => jobToChange.Delete());
				base.DeleteJobCore(jobToDelete);
			}

			protected override void DeactiveJobCore(Job jobToDeactive)
			{
				MakeChangestoJobInNewFactory(jobToDeactive, (jobToChange) => { throw new ZSaveConcurrencyException(new ZDataConcurrencyException(new Exception(), null, Db.Connection), Factory); });
				base.DeactiveJobCore(jobToDeactive);
			}

			void MakeChangestoJobInNewFactory(Job jobToChange, Action<Job> makeChangesToJob)
			{
				jobToChange.Factory.RefreshEnabled = false;

				var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var job = newFactory.Load<Job>(jobToChange.PK);
				makeChangesToJob(job);
				newFactory.Save();
			}
		}

		#endregion

		#region Implementation

		TestObjectCreator TestObjectCreator
		{
			get { return TestObjectCreator_cached ?? (TestObjectCreator_cached = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator TestObjectCreator_cached;

		#endregion
	}
}

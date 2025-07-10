using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class InvoicingPluginToFreightReopenClosedInternalJobDataProviderTest : TestCaseWithFactory
	{
		public void TestConstructorException_WhenJobIsNull()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: job", () => new InvoicingPluginToFreightReopenClosedInternalJobDataProvider(null));
			AssertNoExceptionThrown(() => new InvoicingPluginToFreightReopenClosedInternalJobDataProvider(Factory.NewJobForTesting<Job>()));
		}

		public void TestGetAllJobs_EmptyList_WhenNoChargeLines()
		{
			var job = Factory.NewJobForTesting<Job>();

			AssertEquals("Precondition: job charges", 0, job.Charges.Count);

			IReOpenClosedJobDataProvider reOpenClosedJobDataProvider = new InvoicingPluginToFreightReopenClosedInternalJobDataProvider(job);
			var allRelatedJobList = reOpenClosedJobDataProvider.GetAllJobs();

			AssertEquals("allRelatedJobList count", 0, allRelatedJobList.Count);
		}

		public void TestGetAllJobs_ValidJobsList_WhenHasChargeLines()
		{
			using (AccountingConfigurationRegistry.Instance.EnableAutoJobRevenueJournals.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.AutoJobRevenueJournalCodes.Yes))
			{
				var shipmentJob1 = Factory.NewJobForTesting<Job>();
				var shipmentJob2 = Factory.NewJobForTesting<Job>();
				var shipmentJob3 = Factory.NewJobForTesting<Job>();
				var shipmentJob4 = Factory.NewJobForTesting<Job>();

				var job = Factory.NewJobForTesting<Job>();

				var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, GlbBranch.CurrentBranch.OrgProxy, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
				var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.RevenueChargeCode, "charge2", TestObjectCreator.AUD, 0m, TestObjectCreator.ABIGAS, TestObjectCreator.AUD, 10m, GlbBranch.CurrentBranch.OrgProxy);
				var charge3 = TestObjectCreator.CreateCharge(job, TestObjectCreator.RevenueChargeCode, "charge3", TestObjectCreator.AUD, 0m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 0m, GlbBranch.CurrentBranch.OrgProxy);
				var charge4 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC4, "charge4", TestObjectCreator.AUD, 40m, GlbBranch.CurrentBranch.OrgProxy, TestObjectCreator.AUD, 30m, GlbBranch.CurrentBranch.OrgProxy);

				AssertEquals("Precondition: lines count", 4, job.Charges.Count);

				IReOpenClosedJobDataProvider reOpenClosedJobDataProvider = new InvoicingPluginToFreightReopenClosedInternalJobDataProvider(job);
				var allRelatedJobList = reOpenClosedJobDataProvider.GetAllJobs();
				AssertEquals("allRelatedJobList", 0, allRelatedJobList.Count);

				charge1.JR_JH_InternalJob = shipmentJob1.PK;
				charge1.JR_GB_InternalBranch = GlbBranch.CurrentBranch.PK;
				charge1.JR_GE_InternalDept = GlbDepartment.CurrentDepartment.PK;
				Assert("Charge1 should create a cost JRJ as it has an org proxy creditor and cost amount > 0", charge1.ShouldCreateCostJRJ);

				charge2.JR_JH_InternalJob = shipmentJob2.PK;
				charge2.JR_GB_InternalBranch = GlbBranch.CurrentBranch.PK;
				charge2.JR_GE_InternalDept = GlbDepartment.CurrentDepartment.PK;
				Assert("Charge2 should create a sell JRJ as it has a Org proxy debitor and sell amount > 0", charge2.ShouldCreateSellJRJ);

				charge3.JR_JH_InternalJob = shipmentJob3.PK;
				charge3.JR_GB_InternalBranch = GlbBranch.CurrentBranch.PK;
				charge3.JR_GE_InternalDept = GlbDepartment.CurrentDepartment.PK;
				Assert("Charge3 should not create a cost or sell JRJ as it has 0 cost/sell amounts", !(charge3.ShouldCreateSellJRJ && charge3.ShouldCreateCostJRJ));

				charge4.JR_JH_InternalJob = shipmentJob4.PK;
				charge4.JR_GB_InternalBranch = GlbBranch.CurrentBranch.PK;
				charge4.JR_GE_InternalDept = GlbDepartment.CurrentDepartment.PK;
				Assert("Charge4 should create a cost and sell JRJ as it has both cost/sell amounts > 0 and also has org proxy debitor and creditor", charge4.ShouldCreateSellJRJ && charge4.ShouldCreateCostJRJ);

				allRelatedJobList = reOpenClosedJobDataProvider.GetAllJobs();
				AssertEquals("allRelatedJobList", 3, allRelatedJobList.Count);
				AssertContainsExactElementsInAnyOrder("allRelatedJobList", new[] { shipmentJob1, shipmentJob2, shipmentJob4 }, allRelatedJobList);

				charge3.JR_OSSellAmt = 20m;
				Assert("Charge3 should create a sell JRJ as it now has a Org proxy debitor and sell amount > 0", charge2.ShouldCreateSellJRJ);

				allRelatedJobList = reOpenClosedJobDataProvider.GetAllJobs();
				AssertEquals("allRelatedJobList", 4, allRelatedJobList.Count);
				AssertContainsExactElementsInAnyOrder("allRelatedJobList", new[] { shipmentJob1, shipmentJob2, shipmentJob3, shipmentJob4 }, allRelatedJobList);
			}
		}

		public void TestJobReopenLogText()
		{
			var jobNumber = "1000";
			var job = Factory.NewJobForTesting<Job>();
			AssertEquals("Precondition: job number", ZString.Empty, job.JH_JobNum);

			IReOpenClosedJobDataProvider reOpenClosedJobDataProvider = new InvoicingPluginToFreightReopenClosedInternalJobDataProvider(job);
			var jobReopenLogText = reOpenClosedJobDataProvider.JobReopenLogText();
			AssertEquals(" -  Auto JRJ", jobReopenLogText);

			job.JH_JobNum = jobNumber;

			jobReopenLogText = reOpenClosedJobDataProvider.JobReopenLogText();
			AssertEquals($" - {jobNumber} Auto JRJ", jobReopenLogText);
		}

		public void TestFactoryReturnsInvoicingPluginToFreightFactory()
		{
			var job = Factory.NewJobForTesting<Job>();

			IReOpenClosedJobDataProvider reOpenClosedJobDataProvider = new InvoicingPluginToFreightReopenClosedInternalJobDataProvider(job);

			AssertNotNull(reOpenClosedJobDataProvider.Factory);
			AssertEquals(Factory, reOpenClosedJobDataProvider.Factory);
		}

		TestObjectCreator testObjectCreator;
		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
	}
}

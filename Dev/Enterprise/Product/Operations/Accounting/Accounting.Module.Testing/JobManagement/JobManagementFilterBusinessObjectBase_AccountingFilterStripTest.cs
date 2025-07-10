using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module.Testing
{
	public class JobManagementFilterBusinessObjectBase_AccountingFilterStripTest : AccountingFilterStripTest<JobManagement>
	{
		protected override JobHeader GetJobForBusinessObjectFromFilterCollection(JobManagement bizo)
		{
			return bizo;
		}

		protected override JobManagement GetNewBusinessObjectForFilterCollection()
		{
			return Factory.NewWithValidTestData<JobManagement>();
		}

		protected override ModuleIdentifier FilterStripModuleID
		{
			get { return ModuleIDs.JobHeader; }
		}

		protected override bool ShouldUseBillingFilters
		{
			get { return false; }
		}

		public new void TestControlForAmountFilters()
		{
			Assert("There are not Amount filters in this module.", true);
		}

		public new void TestJobProfitFilter()
		{
			Assert("There are not Amount filters in this module.", true);
		}

		public new void TestJobMarginPercentFilter()
		{
			Assert("There are not Amount filters in this module.", true);
		}

		public new void TestJobRevenueAmountFilter()
		{
			Assert("There are not Amount filters in this module.", true);
		}

		public new void TestJobWIPAmountFilter()
		{
			Assert("There are not Amount filters in this module.", true);
		}

		public new void TestJobWIPAmountExcludingDeferredChargesFilter()
		{
			Assert("There are not Amount filters in this module.", true);
		}

		public new void TestJobWIPAmountDeferredChargesOnlyFilter()
		{
			Assert("There are not Amount filters in this module.", true);
		}

		public new void TestJobCostAmountFilter()
		{
			Assert("There are not Amount filters in this module.", true);
		}

		public new void TestJobAccrualAmountFilter()
		{
			Assert("There are not Amount filters in this module.", true);
		}

		public new void TestAmountFiltersWhenJobsDoesntHaveTransactionLines()
		{
			Assert("There are not Amount filters in this module.", true);
		}

		public new void TestJobManagementFiltersWithMaxAmounts()
		{
			Assert(true);
		}

		public new void TestAmountFiltersTogether()
		{
			Assert("There are not Amount filters in this module.", true);
		}

		public new void TestAddJobManagementFiltersOnlyIfSecurityIsAllowed()
		{
			Assert("This condition not sutible here.", true);
		}
	}
}

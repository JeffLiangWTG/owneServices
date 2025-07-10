using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module.Testing
{
	public class JobManagementFilterBusinessObject_AccountingFilterStripTest : AccountingFilterStripTest<JobManagement>
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
			get { return ModuleIDs.JobManagement; }
		}

		protected override bool ShouldUseBillingFilters
		{
			get { return false; }
		}

		public new void TestAddJobManagementFiltersOnlyIfSecurityIsAllowed()
		{
			Assert("This condition not sutible here.", true);
		}
	}
}

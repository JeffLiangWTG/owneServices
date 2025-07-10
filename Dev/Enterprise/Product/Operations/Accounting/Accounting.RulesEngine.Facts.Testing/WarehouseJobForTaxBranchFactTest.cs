using Enterprise.MasterFiles.Business;
using WTG.ProductionRules.Business.JobBillingDefaulting;

namespace Enterprise.Accounting.RulesEngine.Facts.Testing
{
	public class WarehouseJobForTaxBranchFactTest : WarehouseJobFactTest
	{
		protected override IJobBranchDepartmentFact GetJobBranchDepartmentFact(IJobInvoicingPlugIn jobInvoicingPlugIn, IEnvironmentFact environmentFact, JobBranchDepartmentFact jobBranchDepartmentFact)
		{
			return new WarehouseJobForTaxBranchFact(jobInvoicingPlugIn, environmentFact, jobBranchDepartmentFact);
		}
	}
}

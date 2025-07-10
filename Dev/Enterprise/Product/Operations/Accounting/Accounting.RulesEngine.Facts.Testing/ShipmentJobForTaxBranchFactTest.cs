using Enterprise.MasterFiles.Business;
using WTG.ProductionRules.Business.JobBillingDefaulting;

namespace Enterprise.Accounting.RulesEngine.Facts.Testing
{
	public class ShipmentJobForTaxBranchFactTest : ShipmentJobFactTest
	{
		protected override IJobBranchDepartmentFact GetJobBranchDepartmentFact(IJobInvoicingPlugIn jobInvoicingPlugIn, IEnvironmentFact environmentFact, JobBranchDepartmentFact jobBranchDepartmentFact)
		{
			return new ShipmentJobForTaxBranchFact(jobInvoicingPlugIn, environmentFact, jobBranchDepartmentFact);
		}
	}
}

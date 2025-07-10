using Enterprise.MasterFiles.Business;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Business.JobBillingDefaulting;

namespace Enterprise.Accounting.RulesEngine.Facts
{
	public class WarehouseJobFact : JobFact, IWarehouseJobFact
	{
		public WarehouseJobFact(IJobInvoicingPlugIn jobPlugin,
			IEnvironmentFact environmentFact,
			IOrganisationWithMainAddressFact localClientFact = null,
			IStaffFact salesRepFact = null)
			: base(jobPlugin?.InvoicingSupporter?.Job, environmentFact, localClientFact, salesRepFact)
		{
		}
	}
}

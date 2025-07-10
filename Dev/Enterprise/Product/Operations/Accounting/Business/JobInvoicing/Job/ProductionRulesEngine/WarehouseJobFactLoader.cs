using System.Collections.Generic;
using Enterprise.Accounting.RulesEngine.Facts;
using Enterprise.MasterFiles.Business;
using WTG.ProductionRules.Core;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class WarehouseJobFactLoader : BaseJobFactLoader, IWarehouseJobFactLoader
	{
		public WarehouseJobFactLoader(RulesContextType rulesContextType) : base(rulesContextType)
		{
		}

		protected override IEnumerable<IInputFact> GetFactsCore(IJobInvoicingPlugIn parentPlugin, FactAccumulator factAccumulator)
		{
			var jobFact = RulesContextType == RulesContextType.JobBillingTaxBranchDefaulting
				? new WarehouseJobForTaxBranchFact(parentPlugin, EnvironmentFact, JobBranchDepartmentFact, LocalClientFact, SalesRepFact)
				: new WarehouseJobFact(parentPlugin, EnvironmentFact, LocalClientFact, SalesRepFact);

			return new[] { jobFact };
		}
	}
}

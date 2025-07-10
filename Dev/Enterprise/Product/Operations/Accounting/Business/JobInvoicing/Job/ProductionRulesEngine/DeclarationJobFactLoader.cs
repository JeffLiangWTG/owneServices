using System.Collections.Generic;
using Enterprise.Accounting.RulesEngine.Facts;
using Enterprise.MasterFiles.Business;
using WTG.ProductionRules.Core;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class DeclarationJobFactLoader : BaseJobFactLoader, IDeclarationJobFactLoader
	{
		public DeclarationJobFactLoader(RulesContextType rulesContextType) : base(rulesContextType)
		{
		}

		protected override IEnumerable<IInputFact> GetFactsCore(IJobInvoicingPlugIn parentPlugin, FactAccumulator factAccumulator)
		{
			var jobFact = RulesContextType == RulesContextType.JobBillingTaxBranchDefaulting
				? new DeclarationJobForTaxBranchFact(parentPlugin, EnvironmentFact, JobBranchDepartmentFact, LocalClientFact, SalesRepFact)
				: new DeclarationJobFact(parentPlugin, EnvironmentFact, LocalClientFact, SalesRepFact);

			return new[] { jobFact };
		}
	}
}

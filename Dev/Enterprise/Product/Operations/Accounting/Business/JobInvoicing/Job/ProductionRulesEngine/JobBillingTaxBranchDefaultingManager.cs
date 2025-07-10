using System.Linq;
using CargoWise.Types;
using WTG.ProductionRules.Business.JobBillingDefaulting;
using WTG.ProductionRules.Core;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class JobBillingTaxBranchDefaultingManager : JobBillingDefaultingManager, IJobBillingTaxBranchDefaultingManager
	{
		public JobBillingTaxBranchDefaultingManager(IFactLoaderProvider factLoaderProvider)
			: base(factLoaderProvider)
		{
		}

		protected override RulesContextType ConetxtType => RulesContextType.JobBillingTaxBranchDefaulting;

		protected override void SetDefaultValue(ProductionRulesEngineResult result)
		{
			var branchResultFact = result.Facts.OfType<BranchResultFact>()?.FirstOrDefault();
			if (branchResultFact != null)
			{
				DefaultValue = new ZGuid(branchResultFact.BranchToDefault.Fact.PK);
			}
		}
	}
}

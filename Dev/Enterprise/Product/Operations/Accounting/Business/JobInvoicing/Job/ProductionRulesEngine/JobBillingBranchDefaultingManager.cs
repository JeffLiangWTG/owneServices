using System.Linq;
using CargoWise.Types;
using WTG.ProductionRules.Business.JobBillingDefaulting;
using WTG.ProductionRules.Core;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class JobBillingBranchDefaultingManager : JobBillingDefaultingManager, IJobBillingBranchDefaultingManager
	{
		public JobBillingBranchDefaultingManager(IFactLoaderProvider factLoaderProvider)
			: base(factLoaderProvider)
		{
		}

		protected override RulesContextType ConetxtType => RulesContextType.JobBillingBranchDefaulting;

		protected override void SetDefaultValue(ProductionRulesEngineResult result)
		{
			var branchResultFact = result.Facts.OfType<BranchResultFact>()?.FirstOrDefault();
			if (branchResultFact != null)
			{
				DefaultValue = new ZGuid(branchResultFact.BranchToDefault.Fact.PK);
			}
			else
			{
				var branchByJobResultFact = result.Facts.OfType<BranchByJobResultFact>()?.FirstOrDefault();
				if (branchByJobResultFact != null)
				{
					DefaultValue = (ZString)branchByJobResultFact.BranchByJobToDefault.Fact.Code;
				}
			}
		}
	}
}

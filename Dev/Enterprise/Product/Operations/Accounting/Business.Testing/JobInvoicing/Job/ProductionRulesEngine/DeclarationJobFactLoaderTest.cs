using System;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.RulesEngine.Facts;
using WTG.ProductionRules.Core;

namespace Enterprise.Accounting.Business.Testing.JobInvoicing
{
	public class DeclarationJobFactLoaderTest : FactLoaderBaseTest<DeclarationJobFact>
	{
		protected override Type JobTypeForTaxBranchFact => typeof(DeclarationJobForTaxBranchFact);

		protected override IFactLoader GetFactLoader(RulesContextType contextType)
		{
			return new DeclarationJobFactLoader(contextType);
		}
	}
}

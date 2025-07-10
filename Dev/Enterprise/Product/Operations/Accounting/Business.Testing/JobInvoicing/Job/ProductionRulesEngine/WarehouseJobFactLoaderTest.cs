using System;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.RulesEngine.Facts;
using WTG.ProductionRules.Core;

namespace Enterprise.Accounting.Business.Testing.JobInvoicing
{
	public class WarehouseJobFactLoaderTest : FactLoaderBaseTest<WarehouseJobFact>
	{
		protected override Type JobTypeForTaxBranchFact => typeof(WarehouseJobForTaxBranchFact);

		protected override IFactLoader GetFactLoader(RulesContextType contextType)
		{
			return new WarehouseJobFactLoader(contextType);
		}
	}
}

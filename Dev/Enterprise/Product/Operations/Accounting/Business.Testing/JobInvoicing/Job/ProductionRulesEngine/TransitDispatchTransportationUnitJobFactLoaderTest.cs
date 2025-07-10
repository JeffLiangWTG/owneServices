using System;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.RulesEngine.Facts;
using WTG.ProductionRules.Core;

namespace Enterprise.Accounting.Business.Testing.JobInvoicing
{
	public class TransitDispatchTransportationUnitJobFactLoaderTest : FactLoaderBaseTest<TransitDispatchTransportationUnitJobFact>
	{
		protected override Type JobTypeForTaxBranchFact => null;

		protected override IFactLoader GetFactLoader(RulesContextType contextType)
		{
			return new TransitDispatchTransportationUnitJobFactLoader(contextType);
		}
	}
}

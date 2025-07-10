using System;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.RulesEngine.Facts;
using WTG.ProductionRules.Core;

namespace Enterprise.Accounting.Business.Testing.JobInvoicing
{
	public class WorkItemJobFactLoaderTest : FactLoaderBaseTest<WorkItemJobFact>
	{
		protected override Type JobTypeForTaxBranchFact => typeof(WorkItemJobFact);

		protected override IFactLoader GetFactLoader(RulesContextType contextType)
		{
			return new WorkItemJobFactLoader(contextType);
		}
	}
}

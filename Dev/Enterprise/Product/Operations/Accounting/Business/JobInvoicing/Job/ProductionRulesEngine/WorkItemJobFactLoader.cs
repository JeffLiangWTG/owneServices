using System.Collections.Generic;
using Enterprise.Accounting.RulesEngine.Facts;
using Enterprise.MasterFiles.Business;
using WTG.ProductionRules.Core;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class WorkItemJobFactLoader : BaseJobFactLoader, IWorkItemJobFactLoader
	{
		public WorkItemJobFactLoader(RulesContextType rulesContextType) : base(rulesContextType)
		{
		}

		protected override IEnumerable<IInputFact> GetFactsCore(IJobInvoicingPlugIn parentPlugin, FactAccumulator factAccumulator)
		{
			var workItemFact = new WorkItemJobFact(parentPlugin, EnvironmentFact, JobBranchDepartmentFact, LocalClientFact, SalesRepFact);
			return new[] { workItemFact };
		}
	}
}

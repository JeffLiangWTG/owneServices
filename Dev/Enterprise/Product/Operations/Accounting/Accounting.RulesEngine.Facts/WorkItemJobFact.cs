using Enterprise.MasterFiles.Business;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Business.JobBillingDefaulting;
using WTG.ProductionRules.Core;

namespace Enterprise.Accounting.RulesEngine.Facts
{
	public class WorkItemJobFact : JobFact, IWorkItemJobFact
	{
		public WorkItemJobFact(IJobInvoicingPlugIn jobPlugin,
			IEnvironmentFact environmentFact,
			IJobBranchDepartmentFact jobBranchDepartmentFact,
			IOrganisationWithMainAddressFact localClientFact = null,
			IStaffFact salesRepFact = null)
			: base(jobPlugin?.InvoicingSupporter?.Job, environmentFact, localClientFact, salesRepFact)
		{
			JobBranch = jobBranchDepartmentFact?.JobBranch;
			JobDepartment = jobBranchDepartmentFact?.JobDepartment;
		}

		public FactLeftJoin<IBranchFact> JobBranch { get; }

		public FactLeftJoin<IDepartmentFact> JobDepartment { get; }
	}
}

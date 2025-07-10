using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Business.JobBillingDefaulting;
using WTG.ProductionRules.Core;

namespace Enterprise.Accounting.RulesEngine.Facts
{
	public class JobBranchDepartmentFact : IJobBranchDepartmentFact
	{
		public FactLeftJoin<IBranchFact> JobBranch { get; }
		public FactLeftJoin<IDepartmentFact> JobDepartment { get; }

		public JobBranchDepartmentFact(IBranchFact branchFact, IDepartmentFact departmentFact)
		{
			JobBranch = new FactLeftJoin<IBranchFact>(branchFact);
			JobDepartment = new FactLeftJoin<IDepartmentFact>(departmentFact);
		}
	}
}

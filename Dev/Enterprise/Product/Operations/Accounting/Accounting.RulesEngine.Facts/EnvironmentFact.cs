using Enterprise.ZArchitecture.Environment;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Business.JobBillingDefaulting;
using WTG.ProductionRules.Core;

namespace Enterprise.Accounting.RulesEngine.Facts
{
	public class EnvironmentFact : IEnvironmentFact
	{
		public EnvironmentFact(ICompany company, IBranchFact branchFact, IDepartmentFact departmentFact)
		{
			CurrentBranch = new FactJoin<IBranchFact>(branchFact);
			CurrentDepartment = new FactJoin<IDepartmentFact>(departmentFact);
			CurrentCompanyCountry = company.Country.Code;
		}

		public FactJoin<IBranchFact> CurrentBranch { get; }

		public FactJoin<IDepartmentFact> CurrentDepartment { get; }

		public string CurrentCompanyCountry { get; }
	}
}

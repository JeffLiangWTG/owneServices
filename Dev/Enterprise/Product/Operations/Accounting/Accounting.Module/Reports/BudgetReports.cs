using Enterprise.DocumentEngine.Module;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class BudgetReports : ZReportModule
	{
		public BudgetReports()
		{
		}

		public override ModuleIdentifier ID
		{
			get
			{
				return ModuleIDs.BudgetReports;
			}
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get
			{
				return Env.Security.BudgetsReports;
			}
		}
	}
}

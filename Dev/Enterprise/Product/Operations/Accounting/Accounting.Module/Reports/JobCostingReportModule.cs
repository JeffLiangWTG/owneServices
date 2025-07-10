using Enterprise.DocumentEngine.Module;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class JobCostingReportModule : ZReportModule
	{
		public JobCostingReportModule()
		{
		}

		public override ModuleIdentifier ID
		{
			get
			{
				return ModuleIDs.JobCostingReport;
			}
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get
			{
				return Env.Security.JobCostingReports;
			}
		}
	}
}

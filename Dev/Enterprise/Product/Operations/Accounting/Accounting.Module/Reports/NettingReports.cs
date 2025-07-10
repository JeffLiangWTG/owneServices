using Enterprise.DocumentEngine.Module;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class NettingReports : ZReportModule
	{
		public NettingReports()
		{
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.NettingReports; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.NettingReports; }
		}
	}
}

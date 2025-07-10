using Enterprise.DocumentEngine.Module;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class EISReports : ZReportModule
	{
		public EISReports()
		{
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.EISReports; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.None; }
		}
	}
}

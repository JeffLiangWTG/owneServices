using Enterprise.DocumentEngine.Module;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentScanning.Module
{
	public class DocManagerReports : ZReportModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.DocManagerReports; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.DocManagerReports; }
		}
	}
}

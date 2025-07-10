using Enterprise.DocumentEngine.Module;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.UPE.Module
{
	public class UPEReportsModule : ZReportModule
	{
		public override ModuleIdentifier ID
		{
			get { return ClientModuleRegistration.Reports; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.None; }
		}
	}
}

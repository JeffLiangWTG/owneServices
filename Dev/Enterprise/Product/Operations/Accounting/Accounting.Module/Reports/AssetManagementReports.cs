using Enterprise.DocumentEngine.Module;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class AssetManagementReports : ZReportModule
	{
		public AssetManagementReports()
		{
		}

		public override ModuleIdentifier ID => ModuleIDs.AssetManagementReports;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.AssetReports;
	}
}

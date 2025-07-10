using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public partial class CNReconciliationExportModule : ZPopupModule
	{
		public CNReconciliationExportModule()
		{
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.CNReconciliationExport; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.ChinaReconciliationExport; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		protected override ZPopupController GetNewController()
		{
			return new CNReconciliationExportController();
		}
	}
}

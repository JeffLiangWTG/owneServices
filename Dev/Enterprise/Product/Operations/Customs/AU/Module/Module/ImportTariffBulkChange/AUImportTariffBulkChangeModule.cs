using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Module
{
	public class AUImportTariffBulkChangeModule : ZPopupModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.ImportTariffBulkChange; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Broker; }
			// get { return Env.Licence.TariffBulkUpdater; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.ImportTariffBulkChange; }
		}

		protected override ZPopupController GetNewController()
		{
			return new AUImportTariffBulkChangeController();
		}
	}
}

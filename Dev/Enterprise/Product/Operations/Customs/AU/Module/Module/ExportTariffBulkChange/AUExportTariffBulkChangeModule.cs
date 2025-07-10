using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Module
{
	public class AUExportTariffBulkChangeModule : ZPopupModule
	{
		public override ModuleIdentifier ID => ModuleIDs.ExportTariffBulkChange;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.ExportTariffBulkChange;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Broker;

		protected override ZPopupController GetNewController() => new AUExportTariffBulkChangeController();
	}
}

using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CA.Module
{
	public class CAHTSTariffBulkChangeModule : ZPopupModule
	{
		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.HTSTariffBulkChange;

		public override ModuleIdentifier ID => ModuleIDs.Customs.CA.HTSTariffBulkChange;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Broker;

		protected override ZPopupController GetNewController() => new CAHTSTariffBulkChangeController();
	}
}

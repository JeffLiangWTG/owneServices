using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CA.Module
{
	public class TransactionNumberSettingModule : ZPopupModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Customs.CA.CATransactionNumberSetting; }
		}

		protected override ZPopupController GetNewController()
		{
			return new TransactionNumberSettingController();
		}

		public override Security.SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.SystemRegistry; }
		}

		protected override Licensing.LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Broker; }
		}
	}
}

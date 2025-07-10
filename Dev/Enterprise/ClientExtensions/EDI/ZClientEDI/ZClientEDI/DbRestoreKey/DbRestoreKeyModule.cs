using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.DbRestoreKey
{
	public class DbRestoreKeyModule : ZPopupModule
	{
		public override ModuleIdentifier ID
		{
			get { return Modules.ClientModuleRegistration.DbRestoreKey; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.AlwaysAllow; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		protected override ZPopupController GetNewController()
		{
			return new DbRestoreKeyController();
		}
	}
}

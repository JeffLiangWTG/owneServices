using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Registry.GUI
{
	public class RegistryModule : ZPopupModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Registry; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.AlwaysAllow; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.SystemRegistry; }
		}

		protected override ZPopupController GetNewController()
		{
			return new RegistryController();
		}
	}
}

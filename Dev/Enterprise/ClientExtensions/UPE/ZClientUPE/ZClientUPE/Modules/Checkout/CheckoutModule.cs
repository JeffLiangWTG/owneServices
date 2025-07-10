using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.UPE.Module
{
	public class CheckoutModule : ZPopupModule
	{
		public override ModuleIdentifier ID
		{
			get { return ClientModuleRegistration.Checkout; }
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
			return (ZPopupController)ZControllerFactory.Create(ClientControllerRegistration.Checkout);
		}
	}
}

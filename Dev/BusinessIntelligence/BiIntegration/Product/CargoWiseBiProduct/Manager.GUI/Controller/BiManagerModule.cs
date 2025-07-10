namespace CargoWise.Bi.Product.Manager.Controller
{
	using Enterprise.Environment;
	using Enterprise.Licensing;
	using Enterprise.Security;
	using Enterprise.ZArchitecture.Modules;

	public class BiManagerModule : ZPopupModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.BiManager; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		protected override ZPopupController GetNewController()
		{
			return (ZPopupController)ZControllerFactory.Create(ControllerIDs.BiManager);
		}
	}
}

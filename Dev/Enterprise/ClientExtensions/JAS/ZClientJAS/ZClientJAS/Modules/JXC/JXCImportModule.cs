using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.JAS.Module
{
	public class JXCImportModule : ZPopupModule
	{
		public override ModuleIdentifier ID
		{
			get { return ClientModuleRegistration.ImportJXCFile; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return JASSecurityCheckpoints.ImportJXCFile; }
		}

		#region Implementation

		protected override ZPopupController GetNewController()
		{
			return (ZSingletonController)ZControllerFactory.Create(ClientControllerRegistration.ImportJXCFile);
		}

		#endregion
	}
}

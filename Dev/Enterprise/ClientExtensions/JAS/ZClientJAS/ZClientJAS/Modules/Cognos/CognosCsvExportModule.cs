using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.JAS.Module
{
	public class CognosCsvExportModule : ZPopupModule
	{
		public CognosCsvExportModule()
		{
		}

		public override ModuleIdentifier ID
		{
			get { return ClientModuleRegistration.ExportCognosCsv; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return JASSecurityCheckpoints.ExportCognosFile; }
		}

		#region Implementation

		protected override ZPopupController GetNewController()
		{
			return (ZSingletonController)ZControllerFactory.Create(ClientControllerRegistration.ExportCognosCsv);
		}

		#endregion
	}
}

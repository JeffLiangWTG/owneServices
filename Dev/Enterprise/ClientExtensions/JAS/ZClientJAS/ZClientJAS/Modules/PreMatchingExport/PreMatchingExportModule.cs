using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.JAS.Module
{
	public class PreMatchingExportModule : ZPopupModule
	{
		public PreMatchingExportModule()
		{
		}

		public override ModuleIdentifier ID
		{
			get { return ClientModuleRegistration.ExportPrematchingTransactions; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return JASSecurityCheckpoints.ExportPrematchingTransactions; }
		}

		#region Implementation

		protected override ZPopupController GetNewController()
		{
			return (ZSingletonController)ZControllerFactory.Create(ClientControllerRegistration.ExportAccountingDataForPreMatching);
		}

		#endregion
	}
}

#region Implementation
#region ExposedPreMatchingExportModule class
#endregion
#endregion

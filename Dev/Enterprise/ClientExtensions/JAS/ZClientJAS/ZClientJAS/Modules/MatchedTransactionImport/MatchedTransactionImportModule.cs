using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.JAS.Module
{
	public class MatchedTransactionImportModule : ZPopupModule
	{
		public MatchedTransactionImportModule()
		{
		}

		public override ModuleIdentifier ID
		{
			get { return ClientModuleRegistration.ImportMatchedTransactions; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return JASSecurityCheckpoints.ImportMatchedTransactions; }
		}

		#region Implementation

		protected override ZPopupController GetNewController()
		{
			return (ZSingletonController)ZControllerFactory.Create(ClientControllerRegistration.ImportMatchedTransactions);
		}

		#endregion
	}
}

#region Implementation
#region ExposedPreMatchingExportModule class
#endregion
#endregion

using Enterprise.Client.JAS.Module;
using Enterprise.Core.Environment;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.JAS
{
	class JASSecurityCheckpoints
	{
		#region JASSpecific

		internal static SecurityCheckpoint JASSpecific
		{
			get { return FindCheckpoint(ClientModuleRegistration.IDs.JASSpecific); }
		}

		internal SecurityCheckpoint AddJASSpecificSecurityCheckpoint(IZSecurity securityInstance)
		{
			SecurityCore securityDefinitions = ((SecurityCore)securityInstance);
			SecurityCheckpoint result = securityDefinitions.AddClientSpecificSecurityCheckpoint(
				ClientModuleRegistration.IDs.JASSpecific,
				(NoResString)ClientModuleRegistration.Names.JASSpecific,
				securityDefinitions.Operations);

			return result;
		}

		#endregion

		#region ExportPrematchingTransactions

		internal static SecurityCheckpoint ExportPrematchingTransactions
		{
			get { return FindCheckpoint(ClientModuleRegistration.IDs.ExportPrematchingTransactions); }
		}

		internal SecurityCheckpoint AddExportPrematchingTransactionsSecurityCheckpoint(IZSecurity securityInstance)
		{
			SecurityCore securityDefinitions = ((SecurityCore)securityInstance);
			SecurityCheckpoint result = securityDefinitions.AddClientSpecificSecurityCheckpoint(
				ClientModuleRegistration.IDs.ExportPrematchingTransactions,
				(NoResString)ClientModuleRegistration.Names.ExportPrematchingTransactions,
				FindCheckpoint(securityDefinitions, ClientModuleRegistration.IDs.JASSpecific));

			return result;
		}

		#endregion

		#region ImportMatchedTransactions

		internal static SecurityCheckpoint ImportMatchedTransactions
		{
			get { return FindCheckpoint(ClientModuleRegistration.IDs.ImportMatchedTransactions); }
		}

		internal SecurityCheckpoint AddImportMatchedTransactionsSecurityCheckpoint(IZSecurity securityInstance)
		{
			SecurityCore securityDefinitions = ((SecurityCore)securityInstance);
			SecurityCheckpoint result = securityDefinitions.AddClientSpecificSecurityCheckpoint(
				ClientModuleRegistration.IDs.ImportMatchedTransactions,
				(NoResString)ClientModuleRegistration.Names.ImportMatchedTransactions,
				FindCheckpoint(securityDefinitions, ClientModuleRegistration.IDs.JASSpecific));

			return result;
		}

		#endregion

		#region ImportJXCFile

		internal static SecurityCheckpoint ImportJXCFile
		{
			get { return FindCheckpoint(ClientModuleRegistration.IDs.ImportJXCFile); }
		}

		internal SecurityCheckpoint AddImportJXCFileSecurityCheckpoint(IZSecurity securityInstance)
		{
			SecurityCore securityDefinitions = ((SecurityCore)securityInstance);
			SecurityCheckpoint result = securityDefinitions.AddClientSpecificSecurityCheckpoint(
				ClientModuleRegistration.IDs.ImportJXCFile,
				(NoResString)ClientModuleRegistration.Names.ImportJXCFile,
				FindCheckpoint(securityDefinitions, ClientModuleRegistration.IDs.JASSpecific));

			return result;
		}

		#endregion

		#region ExportCognosFile

		internal static SecurityCheckpoint ExportCognosFile
		{
			get { return FindCheckpoint(ClientModuleRegistration.IDs.ExportCognosFile); }
		}

		internal SecurityCheckpoint AddExportCognosFileSecurityCheckpoint(IZSecurity securityInstance)
		{
			SecurityCore securityDefinitions = ((SecurityCore)securityInstance);
			SecurityCheckpoint result = securityDefinitions.AddClientSpecificSecurityCheckpoint(
				ClientModuleRegistration.IDs.ExportCognosFile,
				(NoResString)ClientModuleRegistration.Names.ExportCognosFile,
				FindCheckpoint(securityDefinitions, ClientModuleRegistration.IDs.JASSpecific));

			return result;
		}

		#endregion

		static SecurityCheckpoint FindCheckpoint(string code)
		{
			return FindCheckpoint(null, code);
		}

		static SecurityCheckpoint FindCheckpoint(SecurityCore security, string code)
		{
			SecurityCore securityToUse = security ?? Env.Security;
			return securityToUse.FindCheckPoint(new CheckpointLookupKey(code));
		}
	}
}

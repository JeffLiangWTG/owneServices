
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.JAS.Module
{
	public static class ClientModuleRegistration
	{
		public enum JASModuleId
		{
			JASSpecific,
			ExportPrematchingTransactions,
			ImportMatchedTransactions,
			ExportCognosFile,
			ImportJXCFile
		}

		#region Constants

		public static class IDs
		{
			public const string JASSpecific = "JASSpecific";
			public const string ExportPrematchingTransactions = "ExportPrematchingTrans";
			public const string ImportMatchedTransactions = "ImportMatchedTransactions";
			public const string ExportCognosFile = "ExportCognosFile";
			public const string ImportJXCFile = "ImportJXCFile";
		}

		public static class Names
		{
			public const string JASSpecific = "JAS Specific";
			public const string ExportPrematchingTransactions = "Export Data for Pre-Matching";
			public const string ImportMatchedTransactions = "Import Matched Transactions";
			public const string ExportCognosFile = "Export Cognos Csv File";
			public const string ImportJXCFile = "Import JXC File";
		}

		#endregion

		public static readonly ClientModuleIdentifier ExportPrematchingTransactions = new ClientModuleIdentifier(JASModuleId.ExportPrematchingTransactions, Names.ExportPrematchingTransactions);
		public static readonly ClientModuleIdentifier ImportMatchedTransactions = new ClientModuleIdentifier(JASModuleId.ImportMatchedTransactions, Names.ImportMatchedTransactions);
		public static readonly ClientModuleIdentifier ExportCognosCsv = new ClientModuleIdentifier(JASModuleId.ExportCognosFile, Names.ExportCognosFile);
		public static readonly ClientModuleIdentifier ImportJXCFile = new ClientModuleIdentifier(JASModuleId.ImportJXCFile, Names.ImportJXCFile);

		public static class Subcategory
		{
			public static ModuleTreeLoaderConstant.Entry JAS
			{
				get
				{
					return new ModuleTreeLoaderConstant.Entry("JAS", (NoResString)"JAS", "J");
				}
			}
		}
	}
}

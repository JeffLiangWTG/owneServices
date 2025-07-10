using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.JP.Common
{
	public sealed class JPRegistry : RegistryItemSet
	{
		#region constructor

		public static JPRegistry Instance
		{
			get { return instance ?? (instance = new JPRegistry()); }
		}

		[ThreadStatic]
		static JPRegistry instance;

		JPRegistry() { }

		#endregion

		public override bool IsForProductivityWise => false;

		#region Categories

		public abstract class Categories : RawDataRegistry.Categories
		{
			public static MultilingualString NACCSMessaging => CombineCategories(RawDataRegistry.Categories.Customs_Japan, ResString.GetMultilingualString("65DC0084-E9D3-4D95-B6C6-3FD480E7D43E", "NACCS Messaging"));
		}

		#endregion

		#region Enable Forwarder Manifest

		public BooleanRegistryItem EnableHCHForwarderManifest
		{
			get
			{
				return GetItem("EnableHCHForwarderManifest", () =>
					new BooleanRegistryItem(
						"EnableHCHForwarderManifest",
						RawDataRegistry.Categories.Customs_Japan,
						ResString.GetMultilingualString("31EB5AF7-5830-4EC9-BE9B-008D0679532C", "Enable HCH"),
						ResString.GetMultilingualString("41862FD2-0EEC-4423-860F-661E6E917FC1", "Set this value to 'Yes' to enable HCH (IMP AIR House B/L Registration) related functionalities."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false
				));
			}
		}

		public BooleanRegistryItem EnableHDFForwarderManifest
		{
			get
			{
				return GetItem("EnableHDFForwarderManifest", () =>
					new BooleanRegistryItem(
						"EnableHDFForwarderManifest",
						RawDataRegistry.Categories.Customs_Japan,
						ResString.GetMultilingualString("BA285F7A-E105-4CB7-AD15-482E522865FF", "Enable HDF"),
						ResString.GetMultilingualString("1764E957-4497-4822-88CE-2FCA0CE8FF46", "Set this value to 'Yes' to enable HDF (EXP AIR Consolidation Registration) related functionalities."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false
				));
			}
		}

		public BooleanRegistryItem EnableNVCForwarderManifest
		{
			get
			{
				return GetItem("EnableNVCForwarderManifest", () =>
					new BooleanRegistryItem(
						"EnableNVCForwarderManifest",
						RawDataRegistry.Categories.Customs_Japan,
						ResString.GetMultilingualString("24B6664B-C0D8-4E91-8335-B82D9F920B5A", "Enable NVC"),
						ResString.GetMultilingualString("5EF1083E-86FC-424A-8B20-107D5DCB966D", "Set this value to 'Yes' to enable NVC (IMP SEA House B/L Registration) related functionalities."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false
				));
			}
		}

		public BooleanRegistryItem EnableVANForwarderManifest
		{
			get
			{
				return GetItem("EnableVANForwarderManifest", () =>
					new BooleanRegistryItem(
						"EnableVANForwarderManifest",
						RawDataRegistry.Categories.Customs_Japan,
						ResString.GetMultilingualString("CB2DC5D1-EC63-4942-A101-BA33E6D9E42A", "Enable VAN"),
						ResString.GetMultilingualString("E6A58BC2-E3C3-46D8-8207-0B89ACC9A366", "Set this value to 'Yes' to enable VAN (EXP SEA House B/L Registration) related functionalities."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false
					));
			}
		}

		#endregion

		public DefaultBrokerAndCredentialRegistryItem DefaultBrokerAndCredential
		{
			get
			{
				return GetItem("JPDefaultBrokerandCredential", delegate
				{
					return new DefaultBrokerAndCredentialRegistryItem(
						"JPDefaultBrokerandCredential",
						Categories.NACCSMessaging,
						ResString.GetMultilingualString("CD865B15-34BC-4074-849B-8867C1F37307", "Default Broker and Credential"),
						ResString.GetMultilingualString("52BE5546-8F02-4BE3-9DBF-CCEED2995E2D", "Default Broker and Credential"),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch | RegistryStorageFlags.BranchDepartment)
					{
						CountryFilterPKs = CountryFilterPKs.Japan
					};
				});
			}
		}

		public StringRegistryItem DefaultFolderForExportingMessages
		{
			get
			{
				return GetItem("JPFolderForExportingMessages", () =>
					new StringRegistryItem
					(
						"JPFolderForExportingMessages",
						Categories.NACCSMessaging,
						ResString.GetMultilingualString("6B8BEA40-2F51-4C00-8FF4-98AD5B743D4D", "Default Folder for Exporting Messages"),
						ResString.GetMultilingualString("9260908A-50F2-44EA-963F-18EE4011552B", "Enter the default folder where messages will be exported to."),
						new DefaultFolderForExportingMessagesDataType(),
						RegistryStorageFlags.System,
						RegistryOptions.Default
					)
					{ EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser) });
			}
		}

		public MailboxAndRemoteWebPrintClientCredentialsRegistryItem MailboxAndRemoteWebPrintClientCredentials
		{
			get
			{
				return GetItem("JPMailboxAndRemoteWebPrintClientCredentials", delegate
				{
					return new MailboxAndRemoteWebPrintClientCredentialsRegistryItem(
						"JPMailboxAndRemoteWebPrintClientCredentials",
						Categories.NACCSMessaging,
						ResString.GetMultilingualString("3951BCF5-77E7-4DF9-86F6-8F6D3C11A898", "Remote WebPrint Client Configurations"),
						ResString.GetMultilingualString("858A347D-B2E8-4D87-95D4-0CD1C30AB07C", "Enter the same 'Local Computer Alias' as the one entered in the 'WebPrint Client Configurations'."),
						RegistryStorageFlags.System);
				});
			}
		}

		#region FTP Settings

		public FTPSettingsRegistryItem FTPSettings
		{
			get
			{
				return GetItem("FTPSettingsRegistryItem", delegate
				{
					return new FTPSettingsRegistryItem(
						"FTPSettingsRegistryItem",
						Categories.Customs_Japan,
						ResString.GetMultilingualString("5D11C367-BA22-4D4F-9633-F28E30F92BA4", "FTP Settings"),
						ResString.GetMultilingualString("C40D2538-8FA8-4A8D-B87A-01FC8EC8776C", "Please enter the settings for the SIMGATE server FTP gateway and the folders used for download and upload messages by FTP."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.IsOnlyForSupport)
					{
						CountryFilterPKs = CountryFilterPKs.Japan
					};
				});
			}
		}

		public bool IsMailboxAndRemoteWebPrintClientCredentialsEmpty
		{
			get
			{
				var settings = MailboxAndRemoteWebPrintClientCredentials.Value;
				return settings == null || settings.IsEmpty;
			}
		}

		#endregion
	}
}

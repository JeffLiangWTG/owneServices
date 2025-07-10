using System;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using static Enterprise.Customs.JP.Common.JPRegistry;

namespace Enterprise.Customs.JP.Common.Testing;

[TestedType(typeof(JPRegistry))]
sealed class JPCustomsRegistryTest : RegistryItemSetTestCaseWithFactory<JPRegistry>
{
	public void TestEnableHCHForwarderManifest()
	{
		TestGenericRegistryItem(ItemSet.EnableHCHForwarderManifest,
			"EnableHCHForwarderManifest",
			RawDataRegistry.Categories.Customs_Japan,
			"Enable HCH",
			"Set this value to 'Yes' to enable HCH (IMP AIR House B/L Registration) related functionalities.",
			RegistryStorageFlags.System,
			RegistryOptions.IsOnlyForSupport,
			false);
	}

	public void TestEnableHDFForwarderManifest()
	{
		TestGenericRegistryItem(ItemSet.EnableHDFForwarderManifest,
			"EnableHDFForwarderManifest",
			RawDataRegistry.Categories.Customs_Japan,
			"Enable HDF",
			"Set this value to 'Yes' to enable HDF (EXP AIR Consolidation Registration) related functionalities.",
			RegistryStorageFlags.System,
			RegistryOptions.IsOnlyForSupport,
			false);
	}

	public void TestEnableNVCForwarderManifest()
	{
		TestGenericRegistryItem(ItemSet.EnableNVCForwarderManifest,
			"EnableNVCForwarderManifest",
			RawDataRegistry.Categories.Customs_Japan,
			"Enable NVC",
			"Set this value to 'Yes' to enable NVC (IMP SEA House B/L Registration) related functionalities.",
			RegistryStorageFlags.System,
			RegistryOptions.IsOnlyForSupport,
			false);
	}

	public void TestEnableVANForwarderManifest()
	{
		TestGenericRegistryItem(ItemSet.EnableVANForwarderManifest,
			"EnableVANForwarderManifest",
			RawDataRegistry.Categories.Customs_Japan,
			"Enable VAN",
			"Set this value to 'Yes' to enable VAN (EXP SEA House B/L Registration) related functionalities.",
			RegistryStorageFlags.System,
			RegistryOptions.IsOnlyForSupport,
			false);
	}

	public void TestMailboxAndRemoteWebPrintClientCredentials()
	{
		TestGenericRegistryItem(ItemSet.MailboxAndRemoteWebPrintClientCredentials,
			"JPMailboxAndRemoteWebPrintClientCredentials",
			Categories.NACCSMessaging,
			"Remote WebPrint Client Configurations",
			"Enter the same 'Local Computer Alias' as the one entered in the 'WebPrint Client Configurations'.",
			RegistryStorageFlags.System);
	}

	public void TestDefaultBrokerandCredential()
	{
		TestGenericRegistryItem(ItemSet.DefaultBrokerAndCredential,
			"JPDefaultBrokerandCredential",
			Categories.NACCSMessaging,
			"Default Broker and Credential",
			"Default Broker and Credential",
			RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch | RegistryStorageFlags.BranchDepartment);
	}

	public void TestDefaultFolderforExportingMessages()
	{
		TestGenericRegistryItem(ItemSet.DefaultFolderForExportingMessages,
			"JPFolderForExportingMessages",
			Categories.NACCSMessaging,
			"Default Folder for Exporting Messages",
			"Enter the default folder where messages will be exported to.",
			RegistryStorageFlags.System);
	}

	public void TestFTPSettings()
	{
		TestGenericRegistryItem(ItemSet.FTPSettings,
			"FTPSettingsRegistryItem",
			Categories.Customs_Japan,
			"FTP Settings",
			"Please enter the settings for the SIMGATE server FTP gateway and the folders used for download and upload messages by FTP.",
			RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
			RegistryOptions.IsOnlyForSupport);
	}

	public void IsMailboxAndRemoteWebPrintClientCredentialsEmpty()
	{
		Assert("Default to true", ItemSet.IsMailboxAndRemoteWebPrintClientCredentialsEmpty);

		var settings = new MailboxAndRemoteWebPrintClientCredentials { LocalComputerAlias = "dummy", DomainName = "dummy", Status = XtCredentialStatusList.Codes.Unregistered };
		ItemSet.MailboxAndRemoteWebPrintClientCredentials.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, settings);

		Assert("Should be false with a valid Credential Settings.", !ItemSet.IsMailboxAndRemoteWebPrintClientCredentialsEmpty);
	}
}

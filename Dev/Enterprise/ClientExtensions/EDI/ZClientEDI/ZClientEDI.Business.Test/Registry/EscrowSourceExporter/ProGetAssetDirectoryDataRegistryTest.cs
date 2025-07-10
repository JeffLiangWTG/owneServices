using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Test
{
	partial class EDIDataRegistryTest
	{
		public void TestProGetAssetDirectoryPathUrl()
		{
			TestRegistryItem(
				ItemSet.ProGetAssetDirectoryPathUrl,
				"ProGetAssetDirectoryPathUrl",
				"WiseTech Global Client Extensions/Escrow Source Exporter/ProGet Asset Directory",
				"Endpoint URL",
				"API endpoint URL of the ProGet asset directory.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				TextEditorType.Url,
				"https://proget.wtg.zone/endpoints/EscrowExport/",
				"https://proget.wtg.zone/assets/LogisticServices/");
		}

		public void TestProGetAssetDirectoryPathUrlApiKey()
		{
			TestRegistryItem(
				ItemSet.ProGetAssetDirectoryApiKey,
				"ProGetAssetDirectoryApiKey",
				"WiseTech Global Client Extensions/Escrow Source Exporter/ProGet Asset Directory",
				"API key",
				@"ProGet API key used to authenticate requests.
API key or User name / password pair should be configured.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				TextEditorType.Password,
				string.Empty);
		}

		public void TestProGetAssetDirectoryPathUrlUserName()
		{
			TestRegistryItem(
				ItemSet.ProGetAssetDirectoryUserName,
				"ProGetAssetDirectoryUserName",
				"WiseTech Global Client Extensions/Escrow Source Exporter/ProGet Asset Directory",
				"User name",
				@"The user name to supply when using basic authentication.
API key or User name / password pair should be configured.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				TextEditorType.TextBox,
				string.Empty);
		}

		public void TestProGetAssetDirectoryPathUrlPassword()
		{
			TestRegistryItem(
				ItemSet.ProGetAssetDirectoryPassword,
				"ProGetAssetDirectoryPassword",
				"WiseTech Global Client Extensions/Escrow Source Exporter/ProGet Asset Directory",
				"User password",
				@"The password to supply when using basic authentication.
API key or User name / password pair should be configured.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				TextEditorType.Password,
				string.Empty);
		}
	}
}

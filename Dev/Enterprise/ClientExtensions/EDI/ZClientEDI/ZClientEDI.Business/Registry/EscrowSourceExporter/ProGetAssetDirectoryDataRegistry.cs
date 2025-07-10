using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI
{
	partial class EDIDataRegistry
	{
		public StringRegistryItem ProGetAssetDirectoryPathUrl =>
			GetItem("ProGetAssetDirectoryPathUrl",
				() => new StringRegistryItem(
					"ProGetAssetDirectoryPathUrl",
					(NoResString)ProGetAssetDirectoryCategory,
					(NoResString)"Endpoint URL",
					(NoResString)"API endpoint URL of the ProGet asset directory.",
					new StringRegistryDataType(CharacterCase.Normal),
					new TextRegistryEditorInfo(TextEditorType.Url),
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					"https://proget.wtg.zone/endpoints/EscrowExport/"));

		public StringRegistryItem ProGetAssetDirectoryApiKey =>
			GetItem("ProGetAssetDirectoryApiKey",
				() => new StringRegistryItem(
					"ProGetAssetDirectoryApiKey",
					(NoResString)ProGetAssetDirectoryCategory,
					(NoResString)"API key",
					(NoResString)"ProGet API key used to authenticate requests.\r\nAPI key or User name / password pair should be configured.",
					new StringRegistryDataType(CharacterCase.Normal),
					new TextRegistryEditorInfo(TextEditorType.Password),
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					string.Empty));

		public StringRegistryItem ProGetAssetDirectoryUserName =>
			GetItem("ProGetAssetDirectoryUserName",
				() => new StringRegistryItem(
					"ProGetAssetDirectoryUserName",
					(NoResString)ProGetAssetDirectoryCategory,
					(NoResString)"User name",
					(NoResString)"The user name to supply when using basic authentication.\r\nAPI key or User name / password pair should be configured.",
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					string.Empty));

		public StringRegistryItem ProGetAssetDirectoryPassword =>
			GetItem("ProGetAssetDirectoryPassword",
				() => new StringRegistryItem(
					"ProGetAssetDirectoryPassword",
					(NoResString)ProGetAssetDirectoryCategory,
					(NoResString)"User password",
					(NoResString)"The password to supply when using basic authentication.\r\nAPI key or User name / password pair should be configured.",
					new StringRegistryDataType(CharacterCase.Normal),
					new TextRegistryEditorInfo(TextEditorType.Password),
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					string.Empty));

		const string ProGetAssetDirectoryCategory = EscrowExporterCategory + "/ProGet Asset Directory";
	}
}

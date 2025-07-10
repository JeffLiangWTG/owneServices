using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Registry.Testing
{
	[TestedType(typeof(McpIslCredentialsSettingCollectionRegistryItem))]
	public class McpIslCredentialsSettingCollectionRegistryItemTests : StronglyTypedRegistryItemTestCase<McpIslCredentialsSettingCollection>
	{
		protected override StronglyTypedRegistryItem<McpIslCredentialsSettingCollection, McpIslCredentialsSettingCollection> GetNewRegistryItem()
			=> new McpIslCredentialsSettingCollectionRegistryItem("", (NoResString)"", "", "", RegistryStorageFlags.Branch);
	}
}

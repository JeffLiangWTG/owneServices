using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestedType(typeof(ClientTariffAndLevelCollectionRegistryItem))]
	sealed class ClientTariffAndLevelCollectionRegistryItemTest : ClientAndAgentBrandingRegistryItemTestCase
	{
		protected override StronglyTypedRegistryItem<ClientAndAgentBrandingCollection, ClientAndAgentBrandingCollection> GetNewRegistryItem()
		{
			return new ClientTariffAndLevelCollectionRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
	}
}

using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestedType(typeof(HybridDocumentBrandCollectionRegistryItem))]
	public class HybridDocumentBrandCollectionRegistryItemTest : ClientAndAgentBrandingRegistryItemTestCase
	{
		protected override StronglyTypedRegistryItem<ClientAndAgentBrandingCollection, ClientAndAgentBrandingCollection> GetNewRegistryItem()
		{
			return new HybridDocumentBrandCollectionRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
	}
}

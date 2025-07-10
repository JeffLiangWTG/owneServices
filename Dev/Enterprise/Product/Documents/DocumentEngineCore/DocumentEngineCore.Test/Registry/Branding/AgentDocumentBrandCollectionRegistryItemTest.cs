using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestedType(typeof(AgentDocumentBrandCollectionRegistryItem))]
	public class AgentDocumentBrandCollectionRegistryItemTest : ClientAndAgentBrandingRegistryItemTestCase
	{
		protected override StronglyTypedRegistryItem<ClientAndAgentBrandingCollection, ClientAndAgentBrandingCollection> GetNewRegistryItem()
		{
			return new AgentDocumentBrandCollectionRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
	}
}

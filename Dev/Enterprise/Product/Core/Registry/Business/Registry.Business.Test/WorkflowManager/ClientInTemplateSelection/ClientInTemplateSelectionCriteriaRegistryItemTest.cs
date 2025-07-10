using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ClientInTemplateSelectionCriteriaCollectionRegistryItem))]
	sealed class ClientInTemplateSelectionCriteriaRegistryItemTest : StronglyTypedRegistryItemTestCase<ClientInTemplateSelectionCriteriaCollection>
	{
		protected override StronglyTypedRegistryItem<ClientInTemplateSelectionCriteriaCollection, ClientInTemplateSelectionCriteriaCollection> GetNewRegistryItem()
		{
			return new ClientInTemplateSelectionCriteriaCollectionRegistryItem("", null, null, null, RegistryStorageFlags.System, new ClientInTemplateSelectionCriteriaCollection());
		}
	}
}

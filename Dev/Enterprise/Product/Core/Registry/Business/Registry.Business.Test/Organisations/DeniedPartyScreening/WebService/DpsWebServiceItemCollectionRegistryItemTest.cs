using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(DpsWebServiceItemCollectionRegistryItem))]
	sealed class DpsWebServiceItemCollectionRegistryItemTest : StronglyTypedRegistryItemTestCase<DpsWebServiceItemCollection>
	{
		protected override StronglyTypedRegistryItem<DpsWebServiceItemCollection, DpsWebServiceItemCollection> GetNewRegistryItem()
		{
			return new DpsWebServiceItemCollectionRegistryItem("name", null, (NoResString)"caption", (NoResString)"hint", RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, new DpsWebServiceItemCollection());
		}
	}
}

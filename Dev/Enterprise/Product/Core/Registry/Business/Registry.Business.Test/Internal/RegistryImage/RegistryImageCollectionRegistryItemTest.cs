using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(RegistryImageCollectionRegistryItem))]
	sealed class RegistryImageCollectionRegistryItemTest
			: RegistryImageCollectionRegistryItemTest<RegistryImageCollectionRegistryItem, RegistryImageCollection>
	{
		protected override StronglyTypedRegistryItem<RegistryImageCollection, RegistryImageCollection> GetNewRegistryItem()
		{
			return new RegistryImageCollectionRegistryItem("TEST_HBOL_IMAGE", null, null, null, RegistryStorageFlags.All);
		}

		protected override RegistryImageCollection GetNewCollection()
		{
			return new RegistryImageCollection();
		}
	}
}

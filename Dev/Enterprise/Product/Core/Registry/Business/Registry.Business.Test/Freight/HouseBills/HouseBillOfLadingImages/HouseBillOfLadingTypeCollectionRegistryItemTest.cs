using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(HouseBillOfLadingTypeCollectionRegistryItem))]
	sealed class HouseBillOfLadingTypeCollectionRegistryItemTest : StronglyTypedRegistryItemTestCase<HouseBillOfLadingTypeCollection>
	{
		protected override StronglyTypedRegistryItem<HouseBillOfLadingTypeCollection, HouseBillOfLadingTypeCollection> GetNewRegistryItem()
		{
			return new HouseBillOfLadingTypeCollectionRegistryItem("", null, null, null, RegistryStorageFlags.System, new HouseBillOfLadingTypeCollection());
		}
	}
}

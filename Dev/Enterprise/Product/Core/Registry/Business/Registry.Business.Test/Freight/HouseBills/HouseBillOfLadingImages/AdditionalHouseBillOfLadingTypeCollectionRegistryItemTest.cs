using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(AdditionalHouseBillOfLadingTypeCollectionRegistryItem))]
	sealed class AdditionalHouseBillOfLadingTypeCollectionRegistryItemTest : StronglyTypedRegistryItemTestCase<AdditionalHouseBillOfLadingTypeCollection>
	{
		protected override StronglyTypedRegistryItem<AdditionalHouseBillOfLadingTypeCollection, AdditionalHouseBillOfLadingTypeCollection> GetNewRegistryItem()
		{
			return new AdditionalHouseBillOfLadingTypeCollectionRegistryItem("", null, null, null, RegistryStorageFlags.System, new AdditionalHouseBillOfLadingTypeCollection());
		}
	}
}

using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(LegTypeCollectionRegistryItem))]
	sealed class LegTypeCollectionRegistryItemTest : StronglyTypedRegistryItemTestCase<LegTypeCollection>
	{
		protected override StronglyTypedRegistryItem<LegTypeCollection, LegTypeCollection> GetNewRegistryItem()
		{
			return new LegTypeCollectionRegistryItem("", null, null, null, RegistryStorageFlags.System, new LegTypeCollection());
		}
	}
}

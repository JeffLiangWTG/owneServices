using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(AWBRoundingRegistryItem))]
	sealed class AWBRoundingRegistryItemTest : StronglyTypedRegistryItemTestCase<AWBRoundingCollection>
	{
		protected override StronglyTypedRegistryItem<AWBRoundingCollection, AWBRoundingCollection> GetNewRegistryItem()
		{
			return new AWBRoundingRegistryItem(string.Empty,
					null, null, null, RegistryStorageFlags.System, new AWBRoundingCollection());
		}
	}
}

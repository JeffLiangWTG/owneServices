using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(DepotAddressColorSoundRegistryItem))]
	sealed class TestDepotAddressColorSoundRegistryItem : StronglyTypedRegistryItemTestCase<DepotAddressColorSoundCollection>
	{
		protected override StronglyTypedRegistryItem<DepotAddressColorSoundCollection, DepotAddressColorSoundCollection> GetNewRegistryItem()
		{
			return new DepotAddressColorSoundRegistryItem("", null, null, null, RegistryStorageFlags.All);
		}
	}
}

using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(AddressListRegistryItem))]
	sealed class AddressListRegistryItemTest : StronglyTypedRegistryItemTestCase<AddressListCollection>
	{
		protected override StronglyTypedRegistryItem<AddressListCollection, AddressListCollection> GetNewRegistryItem()
		{
			return new AddressListRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, new AddressListCollection());
		}
	}
}

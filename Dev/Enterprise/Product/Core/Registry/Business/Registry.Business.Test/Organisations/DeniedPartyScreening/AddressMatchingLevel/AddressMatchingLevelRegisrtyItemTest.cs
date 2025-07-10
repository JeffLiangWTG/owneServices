using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(AddressMatchingLevelRegisrtyItem))]
	sealed class AddressMatchingLevelRegisrtyItemTest : StronglyTypedRegistryItemTestCase<AddressMatchingLevelBusinessObject>
	{
		protected override StronglyTypedRegistryItem<AddressMatchingLevelBusinessObject, AddressMatchingLevelBusinessObject> GetNewRegistryItem()
		{
			return new AddressMatchingLevelRegisrtyItem("name", null, (NoResString)"caption", (NoResString)"hint", RegistryStorageFlags.System, RegistryOptions.Default, new AddressMatchingLevelBusinessObject());
		}
	}
}

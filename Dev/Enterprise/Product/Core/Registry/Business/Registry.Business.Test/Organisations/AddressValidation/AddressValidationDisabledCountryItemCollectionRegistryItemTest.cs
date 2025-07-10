using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(AddressValidationDisabledCountryItemCollectionRegistryItem))]
	sealed class AddressValidationDisabledCountryItemCollectionRegistryItemTest : StronglyTypedRegistryItemTestCase<AddressValidationDisabledCountryItemCollection>
	{
		protected override StronglyTypedRegistryItem<AddressValidationDisabledCountryItemCollection, AddressValidationDisabledCountryItemCollection> GetNewRegistryItem()
		{
			return new AddressValidationDisabledCountryItemCollectionRegistryItem("1", (NoResString)"b", (NoResString)"c", (NoResString)"d", RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport);
		}
	}
}

using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class AddressValidationDisabledCountryItemCollectionRegistryItem : StronglyTypedRegistryItem<AddressValidationDisabledCountryItemCollection>
	{
		public AddressValidationDisabledCountryItemCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new AddressValidationDisabledCountryItemRegistryDataType(), storage, options, new AddressValidationDisabledCountryItemCollection()))
		{
		}
	}
}

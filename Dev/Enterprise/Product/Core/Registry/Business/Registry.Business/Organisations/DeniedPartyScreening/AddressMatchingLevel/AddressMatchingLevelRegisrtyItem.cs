using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class AddressMatchingLevelRegisrtyItem : StronglyTypedRegistryItem<AddressMatchingLevelBusinessObject>
	{
		public AddressMatchingLevelRegisrtyItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, AddressMatchingLevelBusinessObject defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new AddressMatchingLevelRegistryDataType(), storage, options, defaultValue))
		{
		}
	}
}

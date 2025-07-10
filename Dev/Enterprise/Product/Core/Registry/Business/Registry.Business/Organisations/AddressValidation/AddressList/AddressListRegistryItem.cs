using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class AddressListRegistryItem : StronglyTypedRegistryItem<AddressListCollection>
	{
		public AddressListRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, AddressListCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new AddressListRegistryDataType(), storage, options, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.AddressListRegistryItemEditor, Enterprise.Registry.GUI")]
	public class AddressListRegistryDataType : NonPersistentBusinessObjectRegistryDataType<AddressListCollection>
	{
	}
}

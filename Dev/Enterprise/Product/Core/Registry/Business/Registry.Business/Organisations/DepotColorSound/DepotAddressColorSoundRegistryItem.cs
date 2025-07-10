using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class DepotAddressColorSoundRegistryItem : StronglyTypedRegistryItem<DepotAddressColorSoundCollection>
	{
		public DepotAddressColorSoundRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new DepotAddressColorSoundDataType(), storage))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.DepotAddressColorSoundRegistryItemEditor, Enterprise.Registry.GUI")]
	class DepotAddressColorSoundDataType : NonPersistentBusinessObjectRegistryDataType<DepotAddressColorSoundCollection>
	{
	}
}

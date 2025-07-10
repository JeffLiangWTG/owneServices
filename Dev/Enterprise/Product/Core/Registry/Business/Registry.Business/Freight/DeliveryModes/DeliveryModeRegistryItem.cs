using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class DeliveryModeRegistryItem : StronglyTypedRegistryItem<DeliveryModeCollection>
	{
		public DeliveryModeRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, DeliveryModeCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new DeliveryModeRegistryDataType(), storage, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.DeliveryModeRegistryItemEditor, Enterprise.Registry.GUI")]
	public class DeliveryModeRegistryDataType : NonPersistentBusinessObjectRegistryDataType<DeliveryModeCollection>
	{
	}
}

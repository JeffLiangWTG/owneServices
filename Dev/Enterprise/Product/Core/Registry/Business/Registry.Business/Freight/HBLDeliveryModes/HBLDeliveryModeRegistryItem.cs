using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class HBLDeliveryModeRegistryItem : StronglyTypedRegistryItem<HBLDeliveryModes>
	{
		public HBLDeliveryModeRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, HBLDeliveryModes defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new HBLDeliveryModeRegistryDataType(defaultValue), storage, options, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.HBLDeliveryModeRegistryItemEditor, Enterprise.Registry.GUI")]
	public class HBLDeliveryModeRegistryDataType : NonPersistentBusinessObjectRegistryDataType<HBLDeliveryModes>
	{
		public HBLDeliveryModeRegistryDataType()
		{
		}

		public HBLDeliveryModeRegistryDataType(HBLDeliveryModes defaultValue)
			: base(defaultValue)
		{
		}
	}
}

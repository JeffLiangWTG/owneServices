using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class HBLDeliveryPriorityRegistryItem : StronglyTypedRegistryItem<HBLDeliveryPriorityConfigCollection>
	{
		public HBLDeliveryPriorityRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new HBLDeliveryPriorityRegistryDataType(), storage, options))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.HBLDeliveryPriorityRegistryItemEditor, Enterprise.Registry.GUI")]
	class HBLDeliveryPriorityRegistryDataType : NonPersistentBusinessObjectRegistryDataType<HBLDeliveryPriorityConfigCollection>
	{
		public HBLDeliveryPriorityRegistryDataType()
		{
		}
	}
}

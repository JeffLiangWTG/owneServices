using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class GlobalTrackingShipmentVisibilityOptionsRegistryItem : StronglyTypedRegistryItem<GlobalTrackingShipmentVisibilityOptions>
	{
		public GlobalTrackingShipmentVisibilityOptionsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions registryOptions, GlobalTrackingShipmentVisibilityOptions defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new GlobalTrackingShipmentVisibilityOptionsRegistryItemDataType(defaultValue), storage, registryOptions))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.GlobalTrackingShipmentVisibilityOptionsRegistryItemEditor, Enterprise.Registry.GUI")]
	public class GlobalTrackingShipmentVisibilityOptionsRegistryItemDataType : NonPersistentBusinessObjectRegistryDataType<GlobalTrackingShipmentVisibilityOptions>
	{
		public GlobalTrackingShipmentVisibilityOptionsRegistryItemDataType(GlobalTrackingShipmentVisibilityOptions defaultValue)
			: base(defaultValue)
		{
		}
	}
}

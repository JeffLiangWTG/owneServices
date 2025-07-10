using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public class ShipmentMilestoneEventUpdatesRegistryItem : MilestoneEventUpdatesRegistryItem<ShipmentMilestoneEventUpdatesCollection>
	{
		public ShipmentMilestoneEventUpdatesRegistryItem(string name, MultilingualString caption, MultilingualString hint, MultilingualString category, RegistryStorageFlags storage, RegistryOptions options, MilestoneEventUpdatesCollection defaultValue)
			: base(name, caption, hint, category, new ShipmentMilestoneEventUpdatesRegistryDataType(), storage, options, defaultValue)
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.ShipmentMilestoneEventUpdatesRegistryItemEditor, Enterprise.Registry.GUI")]
	class ShipmentMilestoneEventUpdatesRegistryDataType : MilestoneEventUpdatesRegistryDataType<ShipmentMilestoneEventUpdatesCollection>
	{
		public ShipmentMilestoneEventUpdatesRegistryDataType()
		{
		}
	}
}

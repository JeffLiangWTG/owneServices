using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public class WarehouseOrderMilestoneEventUpdatesRegistryItem : MilestoneEventUpdatesRegistryItem<WarehouseOrderMilestoneEventUpdatesCollection>
	{
		public WarehouseOrderMilestoneEventUpdatesRegistryItem(string name, MultilingualString caption, MultilingualString hint, MultilingualString category, RegistryStorageFlags storage, RegistryOptions options, MilestoneEventUpdatesCollection defaultValue)
			: base(name, caption, hint, category, new WarehouseOrderMilestoneEventUpdatesRegistryDataType(), storage, options, defaultValue)
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.WarehouseOrderMilestoneEventUpdatesRegistryItemEditor, Enterprise.Registry.GUI")]
	class WarehouseOrderMilestoneEventUpdatesRegistryDataType : MilestoneEventUpdatesRegistryDataType<WarehouseOrderMilestoneEventUpdatesCollection>
	{
		public WarehouseOrderMilestoneEventUpdatesRegistryDataType()
		{
		}
	}
}

using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public class WarehouseReceiveMilestoneEventUpdatesRegistryItem : MilestoneEventUpdatesRegistryItem<WarehouseReceiveMilestoneEventUpdatesCollection>
	{
		public WarehouseReceiveMilestoneEventUpdatesRegistryItem(string name, MultilingualString caption, MultilingualString hint, MultilingualString category, RegistryStorageFlags storage, RegistryOptions options, MilestoneEventUpdatesCollection defaultValue)
			: base(name, caption, hint, category, new WarehouseReceiveMilestoneEventUpdatesRegistryDataType(), storage, options, defaultValue)
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.WarehouseReceiveMilestoneEventUpdatesRegistryItemEditor, Enterprise.Registry.GUI")]
	class WarehouseReceiveMilestoneEventUpdatesRegistryDataType : MilestoneEventUpdatesRegistryDataType<WarehouseReceiveMilestoneEventUpdatesCollection>
	{
		public WarehouseReceiveMilestoneEventUpdatesRegistryDataType()
		{
		}
	}
}

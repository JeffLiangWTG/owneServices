using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public class OrderMilestoneEventUpdatesRegistryItem : MilestoneEventUpdatesRegistryItem<OrderMilestoneEventUpdatesCollection>
	{
		public OrderMilestoneEventUpdatesRegistryItem(string name, MultilingualString caption, MultilingualString hint, MultilingualString category, RegistryStorageFlags storage, RegistryOptions options, MilestoneEventUpdatesCollection defaultValue)
			: base(name, caption, hint, category, new OrderMilestoneEventUpdatesRegistryDataType(), storage, options, defaultValue)
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.OrderMilestoneEventUpdatesRegistryItemEditor, Enterprise.Registry.GUI")]
	class OrderMilestoneEventUpdatesRegistryDataType : MilestoneEventUpdatesRegistryDataType<OrderMilestoneEventUpdatesCollection>
	{
		public OrderMilestoneEventUpdatesRegistryDataType()
		{
		}
	}
}

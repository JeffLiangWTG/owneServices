using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public class CartageMilestoneEventUpdatesRegistryItem : MilestoneEventUpdatesRegistryItem<CartageMilestoneEventUpdatesCollection>
	{
		public CartageMilestoneEventUpdatesRegistryItem(string name, MultilingualString caption, MultilingualString hint, MultilingualString category, RegistryStorageFlags storage, RegistryOptions options, MilestoneEventUpdatesCollection defaultValue)
			: base(name, caption, hint, category, new CartageMilestoneEventUpdatesRegistryDataType(), storage, options, defaultValue)
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.CartageMilestoneEventUpdatesRegistryItemEditor, Enterprise.Registry.GUI")]
	class CartageMilestoneEventUpdatesRegistryDataType : MilestoneEventUpdatesRegistryDataType<CartageMilestoneEventUpdatesCollection>
	{
		public CartageMilestoneEventUpdatesRegistryDataType()
		{
		}
	}
}

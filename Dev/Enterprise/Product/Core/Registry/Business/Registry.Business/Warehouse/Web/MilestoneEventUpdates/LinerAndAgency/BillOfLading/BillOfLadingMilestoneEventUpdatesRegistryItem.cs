using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public class BillOfLadingMilestoneEventUpdatesRegistryItem : MilestoneEventUpdatesRegistryItem<BillOfLadingMilestoneEventUpdatesCollection>
	{
		public BillOfLadingMilestoneEventUpdatesRegistryItem(string name, MultilingualString caption, MultilingualString hint, MultilingualString category, RegistryStorageFlags storage, RegistryOptions options, MilestoneEventUpdatesCollection defaultValue)
			: base(name, caption, hint, category, new BillOfLadingMilestoneEventUpdatesRegistryDataType(), storage, options, defaultValue)
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.BillOfLadingMilestoneEventUpdatesRegistryItemEditor, Enterprise.Registry.GUI")]
	class BillOfLadingMilestoneEventUpdatesRegistryDataType : MilestoneEventUpdatesRegistryDataType<BillOfLadingMilestoneEventUpdatesCollection>
	{
		public BillOfLadingMilestoneEventUpdatesRegistryDataType()
		{
		}
	}
}

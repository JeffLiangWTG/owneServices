using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public class LinerAndAgencyContainerMilestoneEventUpdatesRegistryItem : MilestoneEventUpdatesRegistryItem<LinerAndAgencyContainerMilestoneEventUpdatesCollection>
	{
		public LinerAndAgencyContainerMilestoneEventUpdatesRegistryItem(string name, MultilingualString caption, MultilingualString hint, MultilingualString category, RegistryStorageFlags storage, RegistryOptions options, MilestoneEventUpdatesCollection defaultValue)
			: base(name, caption, hint, category, new LinerAndAgencyContainerMilestoneEventUpdatesRegistryDataType(), storage, options, defaultValue)
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.LinerAndAgencyContainerMilestoneEventUpdatesRegistryItemEditor, Enterprise.Registry.GUI")]
	class LinerAndAgencyContainerMilestoneEventUpdatesRegistryDataType : MilestoneEventUpdatesRegistryDataType<LinerAndAgencyContainerMilestoneEventUpdatesCollection>
	{
		public LinerAndAgencyContainerMilestoneEventUpdatesRegistryDataType()
		{
		}
	}
}

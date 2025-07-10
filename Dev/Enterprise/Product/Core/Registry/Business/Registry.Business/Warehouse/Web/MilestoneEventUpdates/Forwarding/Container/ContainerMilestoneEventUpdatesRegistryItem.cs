using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public class ContainerMilestoneEventUpdatesRegistryItem : MilestoneEventUpdatesRegistryItem<ContainerMilestoneEventUpdatesCollection>
	{
		public ContainerMilestoneEventUpdatesRegistryItem(string name, MultilingualString caption, MultilingualString hint, MultilingualString category, RegistryStorageFlags storage, RegistryOptions options, MilestoneEventUpdatesCollection defaultValue)
			: base(name, caption, hint, category, new ContainerMilestoneEventUpdatesRegistryDataType(), storage, options, defaultValue)
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.ContainerMilestoneEventUpdatesRegistryItemEditor, Enterprise.Registry.GUI")]
	class ContainerMilestoneEventUpdatesRegistryDataType : MilestoneEventUpdatesRegistryDataType<ContainerMilestoneEventUpdatesCollection>
	{
		public ContainerMilestoneEventUpdatesRegistryDataType()
		{
		}
	}
}

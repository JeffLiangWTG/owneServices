using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public class ISFMilestoneEventUpdatesRegistryItem : MilestoneEventUpdatesRegistryItem<ISFMilestoneEventUpdatesCollection>
	{
		public ISFMilestoneEventUpdatesRegistryItem(string name, MultilingualString caption, MultilingualString hint, MultilingualString category, RegistryStorageFlags storage, RegistryOptions options, MilestoneEventUpdatesCollection defaultValue)
			: base(name, caption, hint, category, new ISFMilestoneEventUpdatesRegistryDataType(), storage, options, defaultValue)
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.ISFMilestoneEventUpdatesRegistryItemEditor, Enterprise.Registry.GUI")]
	class ISFMilestoneEventUpdatesRegistryDataType : MilestoneEventUpdatesRegistryDataType<ISFMilestoneEventUpdatesCollection>
	{
		public ISFMilestoneEventUpdatesRegistryDataType()
		{
		}
	}
}

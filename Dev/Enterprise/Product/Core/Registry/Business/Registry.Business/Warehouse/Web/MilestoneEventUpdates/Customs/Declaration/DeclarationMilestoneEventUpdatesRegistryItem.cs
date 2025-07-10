using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public class DeclarationMilestoneEventUpdatesRegistryItem : MilestoneEventUpdatesRegistryItem<DeclarationMilestoneEventUpdatesCollection>
	{
		public DeclarationMilestoneEventUpdatesRegistryItem(string name, MultilingualString caption, MultilingualString hint, MultilingualString category, RegistryStorageFlags storage, RegistryOptions options, MilestoneEventUpdatesCollection defaultValue)
			: base(name, caption, hint, category, new DeclarationMilestoneEventUpdatesRegistryDataType(), storage, options, defaultValue)
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.DeclarationMilestoneEventUpdatesRegistryItemEditor, Enterprise.Registry.GUI")]
	class DeclarationMilestoneEventUpdatesRegistryDataType : MilestoneEventUpdatesRegistryDataType<DeclarationMilestoneEventUpdatesCollection>
	{
		public DeclarationMilestoneEventUpdatesRegistryDataType()
		{
		}
	}
}

using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class BranchLevelPostingConfigurationRegistryItem : StronglyTypedRegistryItem<BranchLevelPostingConfiguration>
	{
		public BranchLevelPostingConfigurationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions option, object defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new BranchLevelPostingConfigurationRegistryDataType(), storage, option, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.BranchLevelPostingConfigurationRegistryItemEditor, Enterprise.Accounting.GUI")]
	public class BranchLevelPostingConfigurationRegistryDataType : NonPersistentBusinessObjectRegistryDataType<BranchLevelPostingConfiguration>
	{
		public BranchLevelPostingConfigurationRegistryDataType()
		{
		}
	}
}

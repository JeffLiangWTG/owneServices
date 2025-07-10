using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class ConsolImportBranchRuleRegistryItem : StronglyTypedRegistryItem<ImportBranchRule>
	{
		public ConsolImportBranchRuleRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, ImportBranchRule defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new ConsolImportBranchRuleRegistryDataType(), storage, options, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.ConsolImportBranchRuleItemEditor, Enterprise.Registry.GUI")]
	public class ConsolImportBranchRuleRegistryDataType : NonPersistentBusinessObjectRegistryDataType<ImportBranchRule>
	{
	}
}

using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class ShipmentImportBranchRuleRegistryItem : StronglyTypedRegistryItem<ImportBranchRule>
	{
		public ShipmentImportBranchRuleRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, ImportBranchRule defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new ShipmentImportBranchRuleRegistryDataType(), storage, options, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.ShipmentImportBranchRuleItemEditor, Enterprise.Registry.GUI")]
	public class ShipmentImportBranchRuleRegistryDataType : NonPersistentBusinessObjectRegistryDataType<ImportBranchRule>
	{
	}
}

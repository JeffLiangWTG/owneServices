using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class SalesRelationDirectionRulesRegistryItem : StronglyTypedRegistryItem<SalesRelationDirectionRuleCollection>
	{
		public SalesRelationDirectionRulesRegistryItem(
				string name,
				MultilingualString category,
				MultilingualString caption,
				MultilingualString hint,
				RegistryStorageFlags storage,
				SalesRelationDirectionRuleCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new SalesRelationDirectionRulesRegistryDataType(), storage, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.SalesRelationRulesRegistryItemEditor, Enterprise.Registry.GUI")]
	public class SalesRelationDirectionRulesRegistryDataType : NonPersistentBusinessObjectRegistryDataType<SalesRelationDirectionRuleCollection>
	{
	}
}

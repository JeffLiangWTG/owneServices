using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class SubscriptionRulesRegistryItem : StronglyTypedRegistryItem<SubscriptionRuleCollection>
	{
		public SubscriptionRulesRegistryItem(
				string name,
				MultilingualString category,
				MultilingualString caption,
				MultilingualString hint,
				RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new SubscriptionRulesRegistryDataType(), storage))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.SubscriptionRulesRegistryItemEditor, Enterprise.Registry.GUI")]
	public class SubscriptionRulesRegistryDataType : NonPersistentBusinessObjectRegistryDataType<SubscriptionRuleCollection>
	{
	}
}

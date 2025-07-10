using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.eHub
{
	public class BillingTransactionsTransformationSettingsRegistryItem : StronglyTypedRegistryItem<BillingTransactionsTransformationSettings>
	{
		public BillingTransactionsTransformationSettingsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new BillingTransactionsTransformationSettingsRegistryDataType(), storage, options))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.eHub.BillingTransactionsTransformationSettingsRegistryItemEditor, Enterprise.Registry.GUI")]
	public class BillingTransactionsTransformationSettingsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<BillingTransactionsTransformationSettings>
	{
	}
}

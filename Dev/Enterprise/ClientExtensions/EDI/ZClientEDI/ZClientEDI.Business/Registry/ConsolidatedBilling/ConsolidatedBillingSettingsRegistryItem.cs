using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	public class ConsolidatedBillingSettingsRegistryItem : StronglyTypedRegistryItem<ConsolidatedBillingSettingCollection, ConsolidatedBillingSettingCollection>
	{
		public ConsolidatedBillingSettingsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new ConsolidatedBillingSettingsRegistryDataType(), storage, RegistryOptions.Default))
		{
		}
	}

	[RegistryEditor("Enterprise.Client.EDI.Registry.GUI.ConsolidatedBillingSettingsRegistryEditor, ZClientEDI")]
	public class ConsolidatedBillingSettingsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<ConsolidatedBillingSettingCollection>
	{
	}
}

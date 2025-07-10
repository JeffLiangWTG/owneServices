using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	public class UsageBillingSettingsRegistryItem : StronglyTypedRegistryItem<UsageBillingSettings, UsageBillingSettings>
	{
		public UsageBillingSettingsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new UsageBillingSettingsRegistryDataType(), storage, RegistryOptions.Default))
		{
		}
	}

	[RegistryEditor("Enterprise.Client.EDI.Registry.GUI.UsageBillingSettingsRegistryEditor, ZClientEDI")]
	public class UsageBillingSettingsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<UsageBillingSettings>
	{
	}
}

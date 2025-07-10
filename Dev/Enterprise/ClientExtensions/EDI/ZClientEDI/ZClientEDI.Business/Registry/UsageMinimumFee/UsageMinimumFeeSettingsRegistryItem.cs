using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	public class UsageMinimumFeeSettingsRegistryItem : StronglyTypedRegistryItem<UsageMinimumFeeSettings, UsageMinimumFeeSettings>
	{
		public UsageMinimumFeeSettingsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new UsageMinimumFeeSettingsRegistryDataType(), storage, RegistryOptions.Default))
		{
		}
	}

	[RegistryEditor("Enterprise.Client.EDI.Registry.GUI.UsageMinimumFeeSettingsRegistryEditor, ZClientEDI")]
	public class UsageMinimumFeeSettingsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<UsageMinimumFeeSettings>
	{
	}
}

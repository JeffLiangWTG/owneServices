using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class MexicoNotificationRemainingFolioConfigurationRegistryItem : StronglyTypedRegistryItem<MexicoNotificationRemainingFolioConfiguration>
	{
		public MexicoNotificationRemainingFolioConfigurationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, MexicoNotificationRemainingFolioConfiguration defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new MexicoNotificationRemainingFolioConfigurationRegistryDataType(), storage, options, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.MexicoNotificationRemainingFolioConfigurationRegistryItemEditor, Enterprise.Accounting.GUI")]
	class MexicoNotificationRemainingFolioConfigurationRegistryDataType : NonPersistentBusinessObjectRegistryDataType<MexicoNotificationRemainingFolioConfiguration>
	{
		public MexicoNotificationRemainingFolioConfigurationRegistryDataType()
		{
		}
	}
}

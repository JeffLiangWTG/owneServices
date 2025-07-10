using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.eServices.HealthCheckSettings
{
	public class NotificationFrequencyRegistryItem : StronglyTypedRegistryItem<NotificationFrequency>
	{
		public NotificationFrequencyRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new NotificationFrequencyRegistryDataType(), storage, RegistryOptions.Default)) { }

		public NotificationFrequencyRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, NotificationFrequency defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new NotificationFrequencyRegistryDataType(), storage, defaultValue)) { }

		public NotificationFrequencyRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, NotificationFrequency defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new NotificationFrequencyRegistryDataType(), storage, options, defaultValue)) { }
	}
}

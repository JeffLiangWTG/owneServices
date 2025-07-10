using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class TrackAndTraceNotificationsRegistryItem : StronglyTypedRegistryItem<TrackAndTraceNotificationsRule>
	{
		public TrackAndTraceNotificationsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, TrackAndTraceNotificationsRule defaultValue, TrackAndTraceNotificationsRuleVisibilityProvider visibilityProvider)
			: base(new RegistryItemImpl(name, category, caption, hint, new TrackAndTraceNotificationsRegistryDataType(visibilityProvider), storage, defaultValue))
		{
		}

		public TrackAndTraceNotificationsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, TrackAndTraceNotificationsRule defaultValue, TrackAndTraceNotificationsRuleVisibilityProvider visibilityProvider)
			: base(new RegistryItemImpl(name, category, caption, hint, new TrackAndTraceNotificationsRegistryDataType(visibilityProvider), storage, options, defaultValue))
		{
		}
	}
}

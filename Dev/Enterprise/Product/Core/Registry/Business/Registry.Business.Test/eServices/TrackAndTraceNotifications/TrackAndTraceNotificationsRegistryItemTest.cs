using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(TrackAndTraceNotificationsRegistryItem))]
	sealed class TrackAndTraceNotificationsRegistryItemTest : StronglyTypedRegistryItemTestCase<TrackAndTraceNotificationsRule>
	{
		protected override StronglyTypedRegistryItem<TrackAndTraceNotificationsRule, TrackAndTraceNotificationsRule> GetNewRegistryItem()
		{
			return new TrackAndTraceNotificationsRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, new TrackAndTraceNotificationsRule(), new TrackAndTraceNotificationsRuleVisibilityProvider());
		}
	}
}

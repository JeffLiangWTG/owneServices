using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	sealed class TrackAndTraceNotificationsRuleVisibilityProviderTest : TestCase
	{
		public void TestProperties()
		{
			TrackAndTraceNotificationsRuleVisibilityProvider visibilityProvider = new TrackAndTraceNotificationsRuleVisibilityProvider();

			AssertEquals(true, visibilityProvider.NotifyGroupOnDiscrepancyEnabled);
			AssertEquals(true, visibilityProvider.NotifySenderOnDiscrepancyEnabled);
			AssertEquals(true, visibilityProvider.NotifySenderOnErrorEnabled);
			AssertEquals(true, visibilityProvider.NotifySenderOnSuccessOrAcknowledgementEnabled);

			visibilityProvider.NotifyGroupOnDiscrepancyEnabled = false;
			visibilityProvider.NotifySenderOnDiscrepancyEnabled = false;
			visibilityProvider.NotifySenderOnErrorEnabled = false;
			visibilityProvider.NotifySenderOnSuccessOrAcknowledgementEnabled = false;

			AssertEquals(false, visibilityProvider.NotifyGroupOnDiscrepancyEnabled);
			AssertEquals(false, visibilityProvider.NotifySenderOnDiscrepancyEnabled);
			AssertEquals(false, visibilityProvider.NotifySenderOnErrorEnabled);
			AssertEquals(false, visibilityProvider.NotifySenderOnSuccessOrAcknowledgementEnabled);
		}
	}
}

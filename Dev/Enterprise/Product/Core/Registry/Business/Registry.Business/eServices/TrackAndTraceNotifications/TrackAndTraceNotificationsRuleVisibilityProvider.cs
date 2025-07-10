using CargoWise.Types;

namespace Enterprise.Registry.Business
{
	public class TrackAndTraceNotificationsRuleVisibilityProvider
	{
		public TrackAndTraceNotificationsRuleVisibilityProvider()
		{
			NotifyGroupOnDiscrepancyEnabled = true;
			NotifySenderOnDiscrepancyEnabled = true;
			NotifySenderOnErrorEnabled = true;
			NotifySenderOnSuccessOrAcknowledgementEnabled = true;
		}

		public ZBool NotifySenderOnSuccessOrAcknowledgementEnabled { get; set; }

		public ZBool NotifySenderOnErrorEnabled { get; set; }

		public ZBool NotifySenderOnDiscrepancyEnabled { get; set; }

		public ZBool NotifyGroupOnDiscrepancyEnabled { get; set; }
	}
}

using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.eServices.HealthCheckSettings
{
	public static class HealthCheckConstants
	{
		public static ResourceString HealthCheck
		{
			get { return ResString.GetMultilingualString("7507bb71-4750-4412-b20a-ffdda595afc4", "Health Check"); }
		}

		public static ResourceString FailedEDIInterchange
		{
			get { return ResString.GetMultilingualString("0e24230b-8cbf-400f-acb0-e4197bb237ab", "Failed EDI Interchange"); }
		}

		public static ResourceString EmailRecipients
		{
			get { return ResString.GetMultilingualString("b6164d27-8157-453d-a102-1ad0d418b2a6", "Email Recipients"); }
		}

		public static ResourceString EmailRecipientsDescription
		{
			get
			{
				var multilingualRegistryItem = (IMultilingualRegistryItem)RawDataRegistry.Instance.NotificationGroup;
				var multilingualString = ResString.GetMultilingualString("1c1a85a3-eb5e-42a2-ad9b-d29980ef7db2", "[System] > [Registry] > [{0}] > [{1}]", multilingualRegistryItem.CategoryMultilingual.Replace("/", "] > ["), multilingualRegistryItem.CaptionMultilingual);
				return ResString.GetMultilingualString("56dc594e-54c3-4fb0-97c9-4ae9dbfaac92", @"The staff group that will be notified when EDI Interchange fail. The fallback level rule applied when: 
	- If the eHub Error Notification Group doesn't contain a valid Email address for this company, it will look for Email addresses for this {0} System.
	- If the eHub Error Notification Group doesn't contain a valid Email address for this {0} System, it will look for System Notification Group ({1}).
	- If the System Notification Group doesn't contain a valid Email address, the notification will send to all the staff listed in the {0} System.", Core.Constants.ProductName, multilingualString);
			}
		}

		public static ResourceString EmailRecipientsDescriptionXT
		{
			get
			{
				var multilingualRegistryItem = (IMultilingualRegistryItem)RawDataRegistry.Instance.NotificationGroup;
				var multilingualString = ResString.GetMultilingualString("1c1a85a3-eb5e-42a2-ad9b-d29980ef7db2", "[System] > [Registry] > [{0}] > [{1}]", multilingualRegistryItem.CategoryMultilingual.Replace("/", "] > ["), multilingualRegistryItem.CaptionMultilingual);
				return ResString.GetMultilingualString("98392F12-1C54-46D3-BEC8-C2E691F10643", @"The staff group that will be notified when EDI Interchange fail. The fallback level rule applied when: 
	- If the xT Error Notification Group doesn't contain a valid Email address for this company, it will look for Email addresses for this {0} System.
	- If the xT Error Notification Group doesn't contain a valid Email address for this {0} System, it will look for System Notification Group ({1}).
	- If the System Notification Group doesn't contain a valid Email address, the notification will send to all the staff listed in the {0} System.", Core.Constants.ProductName, multilingualString);
			}
		}

		public static ResourceString NotificationFrequency
		{
			get { return ResString.GetMultilingualString("e53a22e4-feab-4c1b-803b-6ce967fa288f", "Notification Frequency"); }
		}

		public static ResourceString NotificationFrequencyDescription
		{
			get { return ResString.GetMultilingualString("e01845e9-2004-4e59-bd35-3c8558affd49", "Configure how often notifications will be sent out, notifications are only sent when there are failed interchanges to report on."); }
		}

		public static CodeDescriptionPairList GetFailedEDIInterchangeNotificationFrequencyOptions()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(NotificationFrequencyConstants.Periodically, ResString.GetMultilingualString("d2efef5e-0614-42aa-b896-04b75880489f", "Send notifications no more often than a configurable time interval."));
			result.AddPair(NotificationFrequencyConstants.SPAM, ResString.GetMultilingualString("c0193939-3f72-43cd-807e-91132c9f948f", "Send notifications every 5 minutes."));
			result.AddPair(NotificationFrequencyConstants.Disable, ResString.GetMultilingualString("030c0594-ed97-4ad3-b203-6033824def8f", "Do not send notifications."));
			return result;
		}

		public static ResourceString Hidden
		{
			get { return ResString.GetMultilingualString("5e2258de-a5b6-4ad0-b24e-e717262868de", "HIDDEN"); }
		}

		public static class NotificationFrequencyConstants
		{
			public const string Periodically = "PERIODICALLY";
			public const string SPAM = "SPAM";
			public const string Disable = "DISABLE";
		}
	}
}

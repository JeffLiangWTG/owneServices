using System;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.eHubMessaging.ServiceTasks.HealthChecks.FailedEDIInterchangeHealthCheck;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.eServices.HealthCheckSettings;
using Enterprise.xTMessaging.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.xTMessaging.ServiceTasks
{
	public class XTOutboundFailedEDIInterchangeCheckJob : FailedEDIInterchangeCheckJob
	{
		public XTOutboundFailedEDIInterchangeCheckJob(INotifications notifier) : base(ServiceTaskCodeList.Descriptions.XTO, ServiceTaskCodeList.Codes.XTO, notifier) { }

		public override string TransportType => EDIInterchangeTransportTypeList.Codes.xT;

		public override int IntervalInMinute => DirectxTMessagingRegistry.Instance.xTFailedEDIInterchangeNotificationFrequency.Value.TimeInterval;

		public override string FrequencySetting => DirectxTMessagingRegistry.Instance.xTFailedEDIInterchangeNotificationFrequency.Value.Settings;

		public override ZDateTime LastCheckedTimeForFailedEDIInterchange
		{
			get => DirectxTMessagingRegistry.Instance.xTLastCheckedTimeForFailedEDIInterchange.Value;
			set => DirectxTMessagingRegistry.Instance.xTLastCheckedTimeForFailedEDIInterchange.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.IsValid ? value.ToDateTime() : DateTime.MinValue);
		}

		public override ZDateTime LastReportedTimeForFailedEDIInterchange
		{
			get => DirectxTMessagingRegistry.Instance.xTLastReportedTimeForFailedEDIInterchange.Value;
			set => DirectxTMessagingRegistry.Instance.xTLastReportedTimeForFailedEDIInterchange.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.IsValid ? value.ToDateTime() : DateTime.MinValue);
		}

		public override StringRegistryItem FailedEDIInterchangesCountInPeriod => DirectxTMessagingRegistry.Instance.xTCountFailedEDIInterchangesInPeriodically;

		public override GuidRegistryItem NotificationGroup => DirectxTMessagingRegistry.Instance.xTFailedEDIInterchangeNotificationGroup;

		public override NotificationFrequencyRegistryItem NotificationFrequencyRegistryItem => DirectxTMessagingRegistry.Instance.xTFailedEDIInterchangeNotificationFrequency;

		public override string FailedEDIInterchangesStatusCode => EDIInterchangeStatusList.Codes.Error;

		protected override string HealthCheckName => Res.GetString("8DCDDAB2-19F3-42BD-9323-05D5AF2FCB7D", "Failed Direct xT Interchange");
	}
}

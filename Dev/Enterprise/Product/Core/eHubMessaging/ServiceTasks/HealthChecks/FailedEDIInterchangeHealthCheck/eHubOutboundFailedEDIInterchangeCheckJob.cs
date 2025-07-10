using System;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.eHubMessaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.eServices.HealthCheckSettings;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.eHubMessaging.ServiceTasks.HealthChecks.FailedEDIInterchangeHealthCheck
{
	// ReSharper disable once InconsistentNaming
	class eHubOutboundFailedEDIInterchangeCheckJob : FailedEDIInterchangeCheckJob
	{
		public eHubOutboundFailedEDIInterchangeCheckJob(INotifications notifier) : base(ServiceTaskNames.EHubOutboundMessages, ServiceTaskCodes.EHO, notifier) { }

		public override string TransportType => EDIInterchangeTransportTypeList.Codes.eHub;

		public override int IntervalInMinute => eHubMessagingRegistry.Instance.eHubFailedEDIInterchangeNotificationFrequency.Value.TimeInterval;

		public override string FrequencySetting => eHubMessagingRegistry.Instance.eHubFailedEDIInterchangeNotificationFrequency.Value.Settings;

		public override ZDateTime LastCheckedTimeForFailedEDIInterchange
		{
			get => eHubMessagingRegistry.Instance.eHubLastCheckedTimeForFailedEDIInterchange.Value;
			set => eHubMessagingRegistry.Instance.eHubLastCheckedTimeForFailedEDIInterchange.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.IsValid ? value.ToDateTime() : DateTime.MinValue);
		}

		public override ZDateTime LastReportedTimeForFailedEDIInterchange
		{
			get => eHubMessagingRegistry.Instance.eHubLastReportedTimeForFailedEDIInterchange.Value;
			set => eHubMessagingRegistry.Instance.eHubLastReportedTimeForFailedEDIInterchange.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.IsValid ? value.ToDateTime() : DateTime.MinValue);
		}

		public override StringRegistryItem FailedEDIInterchangesCountInPeriod => eHubMessagingRegistry.Instance.eHubCountFailedEDIInterchangesInPeriodically;

		public override GuidRegistryItem NotificationGroup => eHubMessagingRegistry.Instance.eHubFailedEDIInterchangeNotificationGroup;

		public override NotificationFrequencyRegistryItem NotificationFrequencyRegistryItem => eHubMessagingRegistry.Instance.eHubFailedEDIInterchangeNotificationFrequency;
		public override string FailedEDIInterchangesStatusCode => EDIInterchangeStatusList.Codes.Failed;

		protected override string HealthCheckName => Res.GetString("760a49b5-a579-422a-8c72-e54d7aeba171", "Failed eHub Interchange");
	}
}

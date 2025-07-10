using System;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.eHubMessaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.Registry.Business.eServices.HealthCheckSettings;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.eHubMessaging.ServiceTasks.HealthChecks.FailedEDIInterchangeHealthCheck
{
	// ReSharper disable once InconsistentNaming
	class eAdaptorOutboundFailedEDIInterchangeCheckJob : FailedEDIInterchangeCheckJob
	{
		public eAdaptorOutboundFailedEDIInterchangeCheckJob(INotifications notifier) : base(ServiceTaskNames.EAdaptorOutboundMessages, ServiceTaskCodes.EAM, notifier) { }

		public override string TransportType => EDIInterchangeTransportTypeList.Codes.eAdaptor;

		public override int IntervalInMinute => eAdaptorRegistry.Instance.eAdaptorFailedEDIInterchangeNotificationFrequency.Value.TimeInterval;

		public override string FrequencySetting => eAdaptorRegistry.Instance.eAdaptorFailedEDIInterchangeNotificationFrequency.Value.Settings;

		public override ZDateTime LastCheckedTimeForFailedEDIInterchange
		{
			get => eAdaptorRegistry.Instance.eAdaptorLastCheckedTimeForFailedEDIInterchange.Value;
			set => eAdaptorRegistry.Instance.eAdaptorLastCheckedTimeForFailedEDIInterchange.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.IsValid ? value.ToDateTime() : DateTime.MinValue);
		}

		public override ZDateTime LastReportedTimeForFailedEDIInterchange
		{
			get => eAdaptorRegistry.Instance.eAdaptorLastReportedTimeForFailedEDIInterchange.Value;
			set => eAdaptorRegistry.Instance.eAdaptorLastReportedTimeForFailedEDIInterchange.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.IsValid ? value.ToDateTime() : DateTime.MinValue);
		}

		public override StringRegistryItem FailedEDIInterchangesCountInPeriod => eAdaptorRegistry.Instance.eAdaptorCountFailedEDIInterchangesInPeriodically;

		public override GuidRegistryItem NotificationGroup => eAdaptorRegistry.Instance.eAdaptorFailedEDIInterchangeNotificationGroup;

		public override NotificationFrequencyRegistryItem NotificationFrequencyRegistryItem => eAdaptorRegistry.Instance.eAdaptorFailedEDIInterchangeNotificationFrequency;

		public override string FailedEDIInterchangesStatusCode => EDIInterchangeStatusList.Codes.Failed;

		protected override string HealthCheckName => Res.GetString("c067d817-3d9c-4908-9d72-aa20f71a1a18", "Failed eAdaptor Interchange");
	}
}

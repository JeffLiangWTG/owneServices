using System;
using System.Collections.Generic;
using Enterprise.eHubMessaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	ServiceTaskCodes.EHI,
	ServiceTaskNames.EHubInboundMessages,
	"ESV",
	typeof(Enterprise.eHubMessaging.ServiceTasks.InboundServiceTask),
	IsMandatory = true,
	MinimumPeriod = "1minute",
	MaximumPeriod = "1hour",
	AllowsMultipleInstances = true,
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "1minute"
	)]

//There is no need to add HostedServiceBusinessObjectBinding to nudge this service task. This service task should be triggered by eService Nudging.
namespace Enterprise.eHubMessaging.ServiceTasks
{
	class InboundServiceTask : eHubServiceTaskWithAdaptor
	{
		public InboundServiceTask()
			: base()
		{
		}

#if DEBUG
		public
#endif
		InboundServiceTask(IAdaptorFactory adaptorFactory, IEHubCommunicationDiagnosterFactory diagnosterFactory)
			: base(adaptorFactory, diagnosterFactory)
		{
		}

		[HostedServiceRequirement]
		public static string CheckProductioneHubGatewayReceiveEnabled() =>
	eHubMessagingRegistry.Instance.eHubEnableReceiveFromProductionGateway.Value ? string.Empty : (NoResString)"Registry Setting 'eServices -> eHub -> Enable Receive From The eHub Production Gateway' is currently disabled";

		[HostedServiceNudged]
		public static bool CheckIsNudged() => !string.IsNullOrEmpty(eHubMessagingRegistry.Instance.EHINudgeURL.Value);

		internal override IEnumerable<IeHubServiceTaskJob> GetJobs()
		{
			yield return new InboundServiceTaskJob(this, Notifier, AdaptorFactory);
		}

		public override string DefaultServerAddress => eHubMessagingRegistry.Instance.eHubGatewayServerAddressList.Value;
		public override string ServiceTaskName => ServiceTaskNames.EHubInboundMessages;
		internal override bool ReportKnownExceptionsAsIssues => true;

		internal override DateTime OutageStartTime
		{
			get => eHubMessagingRegistry.Instance.EHIOutageStartTime.Value;
			set => eHubMessagingRegistry.Instance.EHIOutageStartTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
		}
	}
}

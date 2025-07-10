using System;
using System.Collections.Generic;
using Enterprise.eHubMessaging.Business;
using Enterprise.Registry.Business;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	ServiceTaskCodes.THI,
	ServiceTaskNames.TestEHubInboundMessages,
	"ESV",
	typeof(Enterprise.eHubMessaging.ServiceTasks.TesteHubInboundServiceTask),
	IsMandatory = true,
	MinimumPeriod = "1minute",
	MaximumPeriod = "1hour",
	AllowsMultipleInstances = true,
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "1minute"
	)]

namespace Enterprise.eHubMessaging.ServiceTasks
{
	class TesteHubInboundServiceTask : eHubServiceTaskWithAdaptor
	{
		public TesteHubInboundServiceTask()
			: base()
		{
		}

#if DEBUG
		public
#endif
		TesteHubInboundServiceTask(IAdaptorFactory adaptorFactory, IEHubCommunicationDiagnosterFactory diagnosterFactory)
			: base(adaptorFactory, diagnosterFactory)
		{
		}

		[HostedServiceRequirement]
		public static string CheckNotProductionSystem() => !CompanyTypeHelper.IsProductionSystem() ? string.Empty : "This is a production system.";

		[HostedServiceNudged]
		public static bool CheckIsNudged() => !string.IsNullOrEmpty(eHubMessagingRegistry.Instance.EHINudgeURL.Value);

		internal override DateTime OutageStartTime { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		internal override bool ReportKnownExceptionsAsIssues => false;
		public override string ServiceTaskName => ServiceTaskNames.TestEHubInboundMessages;
		public override string DefaultServerAddress => eHubMessagingRegistry.Instance.eHubTestGatewayServerAddressList.Value;

		internal override IEnumerable<IeHubServiceTaskJob> GetJobs()
		{
			yield return new InboundServiceTaskJob(this, Notifier, AdaptorFactory);
		}
	}
}

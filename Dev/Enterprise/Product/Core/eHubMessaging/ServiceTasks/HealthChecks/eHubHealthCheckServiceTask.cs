using System.Collections.Generic;
using Enterprise.eHubMessaging.Business;
using Enterprise.eHubMessaging.ServiceTasks.HealthChecks.FailedEDIInterchangeHealthCheck;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	ServiceTaskCodes.EHC,
	ServiceTaskNames.EServicesHealthCheck,
	"ESV",
	typeof(Enterprise.eHubMessaging.ServiceTasks.eHubHealthCheckServiceTask),
	AllowsMultipleInstances = false,
	IsMandatory = true,
	MinimumPeriod = "1minute",
	IsScheduleReadOnly = true,
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "1minute"
	)]

namespace Enterprise.eHubMessaging.ServiceTasks
{
	class eHubHealthCheckServiceTask : eServicesHealthCheckServiceTask
	{
		public override IEnumerable<IEHubServiceTaskHealthCheckJob> GetJobs()
		{
			yield return new eAdaptorOutboundFailedEDIInterchangeCheckJob(Notifier);
			yield return new eHubOutboundFailedEDIInterchangeCheckJob(Notifier);
		}

		public override string ServiceTaskCode => ServiceTaskCodes.EHC;
	}
}

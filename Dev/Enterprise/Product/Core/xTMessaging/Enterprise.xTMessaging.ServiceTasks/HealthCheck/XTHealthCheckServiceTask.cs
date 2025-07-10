using System.Collections.Generic;
using Enterprise.eHubMessaging.Business;
using Enterprise.eHubMessaging.ServiceTasks;
using Enterprise.xTMessaging.ServiceTasks;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	ServiceTaskCodeList.Codes.XHC,
	ServiceTaskCodeList.Descriptions.XHC,
	"ESV",
	typeof(XTHealthCheckServiceTask),
	AllowsMultipleInstances = false,
	IsMandatory = true,
	MinimumPeriod = "1minute",
	IsScheduleReadOnly = true,
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "1minute"
)]

namespace Enterprise.xTMessaging.ServiceTasks
{
	public class XTHealthCheckServiceTask : eServicesHealthCheckServiceTask
	{
		public override IEnumerable<IEHubServiceTaskHealthCheckJob> GetJobs()
		{
			yield return new XTOutboundFailedEDIInterchangeCheckJob(Notifier);
		}

		public override string ServiceTaskCode => ServiceTaskCodeList.Codes.XHC;
	}
}

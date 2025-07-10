using System.Threading;
using Enterprise.Client.EDI.ServiceTasks.IncidentLinkHealthCheck;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(Enterprise.Client.EDI.IncidentLinkHealthCheckServiceTask.Code,
	"Incident  autoresponder link health check",
	"SYS",
	typeof(Enterprise.Client.EDI.IncidentLinkHealthCheckServiceTask),
	AllowsMultipleInstances = false,
	CanRunInAnyBranch = true,
	MinimumPeriod = "1hour",
	MaximumPeriod = "1day",
	DefaultScheduleRunEvery = "1day"
	)]

namespace Enterprise.Client.EDI
{
	public class IncidentLinkHealthCheckServiceTask : ServiceProviderImpl
	{
		public const string Code = "IHC";

		public override void RunTask(CancellationToken cancellationToken)
		{
			new IncidentLinkHealthCheckRunner(ServiceLogger).Run(cancellationToken);
		}
	}
}

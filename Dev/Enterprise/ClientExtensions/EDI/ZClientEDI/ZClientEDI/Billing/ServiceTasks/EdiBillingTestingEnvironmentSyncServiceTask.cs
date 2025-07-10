using System.Threading;
using Enterprise.Integration;
using ServiceManager.Integration.ServiceTasks.CW;
using ZClientEDI.Business.Billing.TestingEnvironment;

[assembly: HostedService(
	Enterprise.Client.EDI.Billing.ServiceTasks.EdiBillingTestingEnvironmentSyncServiceTask.Code,
	"Edi Billing Testing Sync",
	"CSP",
	typeof(Enterprise.Client.EDI.Billing.ServiceTasks.EdiBillingTestingEnvironmentSyncServiceTask),
	IsMandatory = false,
	AllowsMultipleInstances = false,
	IsScheduleReadOnly = false,
	MinimumPeriod = "1month",
	DefaultScheduleRunEvery = "1month",
	DefaultScheduleDayOfMonth = 1,
	DefaultScheduleStartAtLocal = "9hours"
	)]

namespace Enterprise.Client.EDI.Billing.ServiceTasks
{
	public class EdiBillingTestingEnvironmentSyncServiceTask : ServiceProviderImpl
	{
		public const string Code = "EBS";

		public override void RunTask(CancellationToken token)
		{
			ServiceLogger.Log(LogType.Information, "Begin processing");
			new TestingEnvironmentSynchronizer(ServiceLogger).Synchronise();
			ServiceLogger.Log(LogType.Information, "End processing");
		}
	}
}

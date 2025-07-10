using System.Threading;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	"SUC",
	"Scheduled Universal Copy Task Runner",
	"SYS",
	typeof(Enterprise.UniversalCopy.Service.UniversalCopyScheduleTaskRunner),
	IsMandatory = true,
	MinimumPeriod = "5minutes",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "5minutes",
	ActiveByDefault = true
	)]

namespace Enterprise.UniversalCopy.Service
{
	class UniversalCopyScheduleTaskRunner : ServiceProviderImpl
	{
		public override void RunTask(CancellationToken token)
		{
			new ScheduleTaskRunner().Process(StmUniversalCopySchema.Constants.Prefix, false, ServiceLogger.GetTaskNotificationSubscriber(), token);
		}
	}
}

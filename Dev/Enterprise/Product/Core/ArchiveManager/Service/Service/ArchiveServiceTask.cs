using System.Threading;
using CargoWise.Application;
using Enterprise.ArchiveManager.Business;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.Scheduler.Business;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(ArchiveManagerConstants))]
[assembly: HostedService(ArchiveManagerConstants.ARCServiceTaskCode,
	"Archive Manager",
	"SYS",
	typeof(Enterprise.ArchiveManager.Service.ArchiveManagerServiceTask),
	MinimumPeriod = "10minutes",
	CanRunInAnyBranch = true,
	AllowsMultipleInstances = true,
	DefaultScheduleRunEvery = "10minutes"
	)]

namespace Enterprise.ArchiveManager.Service
{
	public class ArchiveManagerServiceTask : ServiceProviderImpl
	{
		public override void RunTask(CancellationToken youMustReactToThisToken)
		{
			var scheduleTaskRunner = new ScheduleTaskRunner();
			var queueSize = scheduleTaskRunner.GetPendingJobsQueue(Constants.ArchiveManager.ParentTableCode).QueueSize;

			scheduleTaskRunner.ProcessWithoutLock(Constants.ArchiveManager.ParentTableCode, useStmReportRun: false, ServiceLogger.GetTaskNotificationSubscriber(), youMustReactToThisToken);

			if (queueSize > 0 )
			{
				ServiceLogger.Log(LogType.Information, "Nudging archive manager cleanup.");
				ObjectFactory.Get<IServiceTaskNudger>().NudgeServiceTask(ArchiveManagerConstants.ACLServiceTaskCode);
			}
		}
	}
}

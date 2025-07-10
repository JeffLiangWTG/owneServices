using System.Threading;
using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Scheduler.Business;
using Enterprise.ServiceManager.Tasks.PrintJobProcessor;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	"SRR",
	"Scheduled Report Runner",
	"DOC",
	typeof(ScheduledReportRunner),
	AllowsMultipleInstances = true,
	IsMandatory = true,
	CanRunInAnyBranch = true,
	MinimumPeriod = "1minute",
	DefaultScheduleRunEvery = "1minute",
	ActiveByDefault = true,
	TaskSpecificValidationType = typeof(SRRSpecificValidation))
]
[assembly: HostedServiceQueueProvider("SRR", "Scheduled Report Runner", typeof(ScheduledReportRunner))]
namespace Enterprise.ServiceManager.Tasks.PrintJobProcessor
{
	class ScheduledReportRunner : ServiceProviderImpl, IHostedServiceQueueProvider
	{
		public override void RunTask(CancellationToken token)
		{
			var branch = GlbBranch.GetFirstActiveBranch();
			using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			using (DocumentEngine.Report.TemporarilySetReportRunSource(DocumentEngine.Report.ReportRunSource.ServiceTask))
			{
				ObjectFactory.Get<IScheduleTaskRunner>().Process(StmMenuItemSchema.Constants.Prefix, true, ServiceLogger.GetTaskNotificationSubscriber(), token);
			}
		}

		QueueResult IHostedServiceQueueProvider.QueueResult => ObjectFactory.Get<IScheduleTaskRunner>().GetPendingJobsQueue(StmMenuItemSchema.Constants.Prefix);
	}
}

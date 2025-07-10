using System.Threading;
using CargoWise.Data;
using Enterprise.DbHealth.IndexUpdate;
using Enterprise.Integration;
using Enterprise.Scheduler.Business;
using Enterprise.ServiceManager.Tasks.DbMaintenance;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(ScheduleTypeConstants.IndexStatsUpdateCode, "Index Update Service", "DBM", typeof(IndexUpdateServiceTask),
	IsMandatory = true,
	MinimumPeriod = "1day",
	MaximumPeriod = "1week",
	IsReadOnlyForWiseCloudClient = true,
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "1day",
	DefaultScheduleStartAtLocal = "2hours")
]

namespace Enterprise.ServiceManager.Tasks.DbMaintenance
{
	class IndexUpdateServiceTask : ServiceProviderImpl
	{
		public IndexUpdateServiceTask()
			: base()
		{
		}

		public override void RunTask(CancellationToken iDoNotNeedToReactToThisToken)
		{
			RunIndexStatsUpdate();
		}

		void RunIndexStatsUpdate()
		{
			var indexUpdater = new IndexUpdateRunner();
			indexUpdater.Run(Db.ServerName, Db.DatabaseName, ServiceLogger);
			ServiceLogger.Log(LogType.Information, "Index Update completed");
		}
	}
}

using System.Threading;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.TimeEngineScheduler.ServiceTask;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	ActionSchedulerPurgeTask.Code,
	ActionSchedulerPurgeTask.Description,
	ActionSchedulerPurgeTask.Category,
	typeof(ActionSchedulerPurgeTask),
	IsMandatory = true,
	AllowsMultipleInstances = false,
	MinimumPeriod = "1minute",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "1minute",
	DefaultScheduleStartAtLocal = "0seconds"
	)]
namespace Enterprise.TimeEngineScheduler.ServiceTask
{
	public class ActionSchedulerPurgeTask : ServiceProviderImpl
	{
		public const string Code = "TAO";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "not to be translated")]
		public const string Description = "Task Action Scheduler Purge";
		public const string Category = "BMS";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "It's sql")]
		public override void RunTask(CancellationToken youMustReactToThisToken)
		{
			var monthsToSubtract = ObjectFactory.Get<IBMSRegistry>().TimeBeforeDeletingOldScheduledTasks;
			var dateToDeleteBeforeUtc = ZDateTime.UtcNow.AddMonths(-monthsToSubtract);
			var totalRowsDeleted = 0;
			var deletedRowCount = 0;
			do
			{
				deletedRowCount = Db.Connection.ExecuteScalar<int>("DELETE TOP (1000) FROM dbo.TimeActionSchedule WHERE TAS_ExecutionDateTimeUtc <= @dateToDeleteBeforeUtc and TAS_ExecutionStatus = 'CLS' SELECT @@ROWCOUNT",
					parameters => parameters.AddParameter("@dateToDeleteBeforeUtc", System.Data.SqlDbType.DateTime, dateToDeleteBeforeUtc));
				totalRowsDeleted += deletedRowCount;
			} while (deletedRowCount > 0);

			if (totalRowsDeleted > 0)
			{
				ServiceLogger.Log(LogType.Information, $"{totalRowsDeleted} old schedule(s) were purged.");
			}
		}
	}
}

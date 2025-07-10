using System;
using System.Globalization;
using System.Threading;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ServiceManager.Tasks.SystemServices;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	PerformanceCollectorTask.Code,
	PerformanceCollectorTask.Desciption,
	"SYS", typeof(PerformanceCollectorTask),
	MinimumPeriod = "5minutes",
	MaximumPeriod = "1hour",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "1hour"
	)
]

namespace Enterprise.ServiceManager.Tasks.SystemServices
{
	public class PerformanceCollectorTask : ServiceProviderImpl
	{
		public const string Desciption = "Performance Statistics Collector";
		public const string Code = "PFC";

		public override void RunTask(CancellationToken iDoNotNeedToReactToThisToken)
		{
			CollectStatistics();
		}

		#region Implementation

		void CollectStatistics()
		{
			int totalBacklog = 0;
			int addedToQueue = 0;
			int periodMinutes = GetPeriodInMinutes();
			ZDateTime oldestJob = ZDateTime.MinSmallDateTimeValue;
			ZString oldestJobString = ZString.Empty;
			ZDateTime utcNow = ZDateTime.UtcNow;

			var stmaLogQueueBacklog = Db.Connection.ExecuteScalar<int>("select COUNT(*) from dbo.StmALogQueue");
			var stmaLogQueueWTEBacklog = Db.Connection.ExecuteScalar<int>("select COUNT(*) from dbo.StmALogQueueWTE");
			totalBacklog = Db.Connection.ExecuteScalar<int>("select COUNT(*) from dbo.stmjobqueue WHERE SJ_STATUS = 'QUE' and SJ_SE_NKEvent = 'WTE'");

			using (DbCommand cmd = Db.Connection.Command("select COUNT(*) from dbo.stmjobqueue WHERE SJ_PostedTimeUtc > @LastPeriod and SJ_SE_NKEvent = 'WTE'"))
			{
				cmd.AddParameterBasedOnDbColumn("@LastPeriod", utcNow.AddMinutes(-periodMinutes).ToDateTime(), StmJobQueueSchema.SJ_PostedTimeUtc);
				addedToQueue = (int)cmd.ExecuteScalar();
			}

			using (DbCommand cmd = Db.Connection.Command("select top 1 SJ_PostedTimeUtc  from dbo.stmjobqueue WHERE SJ_STATUS = 'QUE' and SJ_SE_NKEvent = 'WTE' ORDER BY SJ_PostedTimeUtc asc"))
			{
				object date = cmd.ExecuteScalar();
				if (date != null && date != DBNull.Value)
				{
					try
					{
						oldestJob = new ZDateTime(date);
					}
					catch (ZTypeValueException)
					{
						oldestJobString = date.ToString();
					}
				}
			}
			if (oldestJobString.IsEmpty)
			{
				oldestJobString = oldestJob == ZDateTime.MinSmallDateTimeValue ? "N/A" : oldestJob.ToString("G");
			}
			ServiceLogger.Log(LogType.Information, string.Format(CultureInfo.CurrentCulture, "LWK Statistics:\r\nCurrent backlog: {0}\r\nAdded to the queue in last period: {1}\r\nLast Period (min.): {2}\r\nOldest unprocessed job: {3}",
				totalBacklog, addedToQueue, periodMinutes, oldestJobString));
			ServiceLogger.Log(LogType.Information, string.Format(CultureInfo.CurrentCulture, "Logs not yet converted from dbo.StmALogQueue to StmJobQueue : {0}", stmaLogQueueBacklog));
			ServiceLogger.Log(LogType.Information, string.Format(CultureInfo.CurrentCulture, "Logs not yet converted from dbo.StmALogQueueWTE to StmJobQueue : {0}", stmaLogQueueWTEBacklog));
		}

		int GetPeriodInMinutes()
		{
			return 60;
		}

		#endregion
	}
}

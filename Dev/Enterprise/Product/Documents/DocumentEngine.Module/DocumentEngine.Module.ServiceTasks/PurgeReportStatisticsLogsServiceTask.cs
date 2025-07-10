using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Module.ServiceTasks;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	PurgeReportStatisticsLogsServiceTask.Code,
	PurgeReportStatisticsLogsServiceTask.Description,
	PurgeReportStatisticsLogsServiceTask.Category,
	typeof(PurgeReportStatisticsLogsServiceTask),
	CanRunInAnyBranch = true,
	MinimumPeriod = "1day",
	IsMandatory = true,
	DefaultScheduleRunEvery = "1day",
	DefaultScheduleStartAtLocal = "4hours",
	ActiveByDefault = true
)]

namespace Enterprise.DocumentEngine.Scheduler.Module.ServiceTasks
{
	public class PurgeReportStatisticsLogsServiceTask : ServiceProviderImpl
	{
		public const string Code = "PRS";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service Task Description")]
		public const string Description = "Purge Report Statistics Logs";
		public const string Category = "DOC";

		public override void RunTask(CancellationToken token)
		{
			ServiceLogger.Log(LogType.Information, String.Format(CultureInfo.InvariantCulture, "Purging Report Statistics Logs older than {0} days", DaysOld));

			var branch = GlbBranch.GetOneActiveBranchPerCompany().FirstOrDefault();
			if (branch != null)
			{
				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					try
					{
						int deleted = 0;

						var query = new ZDBOnlyQuery(typeof(StmReportRun));
						query.MaximumRows = 1000;
						query.AddToFilter(StmReportRunSchema.RRI_StartTimeUtc, SQLComparisonOperator.LessThan, ZDateTime.UtcNow.AddDays(-DaysOld));
						var factory = new BusinessObjectFactory();
						var rows = factory.Load<StmReportRun>(query);
						while (rows.Length > 0)
						{
							foreach (var stmReportRun in rows)
							{
								++deleted;
								stmReportRun.Delete();
							}
							factory.Save();
							factory = new BusinessObjectFactory();
							rows = factory.Load<StmReportRun>(query);
						}

						ServiceLogger.Log(LogType.Information, String.Format(CultureInfo.InvariantCulture, "Run completed, deleted {0} logs", deleted));
					}
					catch (Exception e) when (!e.IsCriticalException())
					{
						ServiceLogger.Log(LogType.Error, String.Format(CultureInfo.InvariantCulture, "Exception occurred: {0}", e.ToString()));
					}
				}
			}
		}

		static int DaysOld => DataRegistry.Instance.PurgeReportStatisticsLogs;
	}
}

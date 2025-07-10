using System;
using CargoWise.Data;
using Enterprise.LogWalker;
using Enterprise.ServiceManager.Tasks.LogWalker;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(LogWalkerMasterServiceTask.CODE, "Event Log Walker Master Service", "SYS", typeof(LogWalkerMasterServiceTask),
	IsMandatory = true,
	AllowsMultipleInstances = false,
	MinimumPeriod = "10seconds",
	MaximumPeriod = "1day",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true)]
[assembly: HostedServiceBusinessObjectBinding(LogWalkerMasterServiceTask.CODE, StmALogSchema.Constants.TableName, new string[] { }, null)]
[assembly: HostedServiceQueueProvider(LogWalkerMasterServiceTask.CODE, "Log Walker Master", typeof(LogWalkerMasterQueue))]
namespace Enterprise.ServiceManager.Tasks.LogWalker
{
	public class LogWalkerMasterQueue : IHostedServiceQueueProvider
	{
		QueueResult IHostedServiceQueueProvider.QueueResult
		{
			get
			{
				var result = QueueResult.Error;
				Db.Connection.ExecuteReader(QueueSizeQuery, record => result = new QueueResult(record.GetInt32(0), TimeSpan.FromSeconds(record.GetInt32(1))));
				return result;
			}
		}

		const string QueueSizeQuery = @"
SELECT
	SUM(Qty),
	MAX(OldestItemAge)
FROM
(
	SELECT COUNT(*) Qty, ISNULL(MAX(DATEDIFF(second, SLQ_PostedTimeUtc, GETUTCDATE())), 0) OldestItemAge FROM dbo.StmALogQueue
	UNION ALL
	SELECT COUNT(*) Qty, ISNULL(MAX(DATEDIFF(second, WTE_PostedTimeUtc, GETUTCDATE())), 0) OldestItemAge FROM dbo.StmALogQueueWTE
) IQ";
	}

	public class LogWalkerMasterServiceTask : LogWalkerServiceTaskBase
	{
		public const string CODE = "LWM";

		protected override LogWalkerRunner Runner => runner ?? (runner = LogWalkerRunner.Master());
		LogWalkerRunner runner;
	}
}

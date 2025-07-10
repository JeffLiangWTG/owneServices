using System;
using CargoWise.Data;
using Enterprise.LogWalker;
using Enterprise.ServiceManager.Tasks.LogWalker;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(LogWalkerServiceTask.CODE, "Event Log Walker Service", "SYS", typeof(LogWalkerServiceTask),
	IsMandatory = true,
	AllowsMultipleInstances = true,
	MinimumPeriod = "10seconds",
	MaximumPeriod = "1day",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true)]
[assembly: HostedServiceBusinessObjectBinding(LogWalkerServiceTask.CODE, StmJobQueueSchema.Constants.TableName, new string[] { StmJobQueueSchema.Constants.SJ_Status + "=QUE" }, null)]
[assembly: HostedServiceQueueProvider(LogWalkerServiceTask.CODE, "Log Walker", typeof(LogWalkerQueue))]
namespace Enterprise.ServiceManager.Tasks.LogWalker
{
	public class LogWalkerQueue : IHostedServiceQueueProvider
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
	COUNT(*),
	ISNULL(MAX(DATEDIFF(second, SJ_ProcessOnOrAfterUtc, GetUTCDate())), 0)
FROM
	dbo.StmJobQueue
WHERE
	SJ_Status = 'QUE'
AND
	SJ_ProcessOnOrAfterUtc <= GetUtcDate()";
	}

	public class LogWalkerServiceTask : LogWalkerServiceTaskBase
	{
		public const string CODE = "LWK";

		protected override LogWalkerRunner Runner => runner ?? (runner = LogWalkerRunner.Default());
		LogWalkerRunner runner;
	}
}

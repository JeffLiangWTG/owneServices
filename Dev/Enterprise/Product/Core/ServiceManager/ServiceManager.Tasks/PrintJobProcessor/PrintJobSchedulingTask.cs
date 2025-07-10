using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Integration;
using Enterprise.ServiceManager.Tasks.PrintJobProcessor;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	PrintJobSchedulingTask.Code,
	"Print Jobs Scheduling",
	"DOC",
	typeof(PrintJobSchedulingTask),
	IsMandatory = true,
	ActiveByDefault = true,
	AllowsMultipleInstances = false,
	CanRunInAnyBranch = true,
	MinimumPeriod = "1minute",
	DefaultScheduleRunEvery = "15minutes"
)]

[assembly: HostedServiceBusinessObjectBinding(PrintJobSchedulingTask.Code, StmPrintJobSchema.Constants.TableName, new[] { StmPrintJobSchema.Constants.SP_IsScheduled + "=0", StmPrintJobSchema.Constants.SP_Status + "!=FAL", StmPrintJobSchema.Constants.SP_SignBy + "!=" + DocumentsSignBy.DOS }, "Print Jobs Scheduling")]
[assembly: HostedServiceBusinessObjectBinding(PrintJobSchedulingTask.Code, StmPrintJobSchema.Constants.TableName, new[] { StmPrintJobSchema.Constants.SP_IsScheduled + "=0", StmPrintJobSchema.Constants.SP_Status + "!=FAL", StmPrintJobSchema.Constants.SP_SignBy + "=" + DocumentsSignBy.DOS, StmPrintJobSchema.Constants.SP_IsSigned + "=1" }, "Print Jobs Scheduling with Signed DOS")]

[assembly: HostedServiceQueueProvider(PrintJobSchedulingTask.Code, "Unscheduled Print Jobs Queue", typeof(PrintJobSchedulingQueue))]

namespace Enterprise.ServiceManager.Tasks.PrintJobProcessor;

public class PrintJobSchedulingTask : ServiceProviderImpl
{
	public const string Code = "PJQ";

	const int MaxBatchSize = 50;

	public override void RunTask(CancellationToken token)
	{
		ServiceLogger.Log(LogType.Information, "Service task started.");

		RunTaskCore(token);

		ServiceLogger.Log(LogType.Information, "Service task finished.");
	}

	void RunTaskCore(CancellationToken token)
	{
		var hasJobsToProcess = false;
		var globalSequence = GetCurrentMaxGlobalSequence();

		do
		{
			token.ThrowIfCancellationRequested();

			ServiceLogger.Log(LogType.Information, "Starting to load print jobs.");

			var printJobsQueue = new DbOnlyBusinessObjectQueue<StmPrintJob>(GetReadyPrintJobsQuery(), true);
			var factory = new BusinessObjectFactory { RefreshEnabled = false };

			var batchCount = 0;
			var totalCount = 0;
			hasJobsToProcess = false;

			var currentDeliveryGroupGuid = ZGuid.Empty;
			var newPrintJobQueues = new List<StmPrintJobQueue>();

			printJobsQueue.Process((printJobPk, e) =>
			{
				e.Cancel |= token.IsCancellationRequested;
				if (e.Cancel)
				{
					ServiceLogger.Log(LogType.Warning, "Cancellation was requested.");
					return;
				}

				var printJob = factory.Load<StmPrintJob>(printJobPk);
				if (printJob == null)
				{
					return;
				}

				batchCount++;
				totalCount++;
				hasJobsToProcess = true;

				if (!currentDeliveryGroupGuid.IsEmpty && currentDeliveryGroupGuid != printJob.SP_SB_DeliveryGroup)
				{
					globalSequence = UpdateGlobalSequence(newPrintJobQueues, globalSequence);
					ZExceptionReporting.ProcessWithConcurrencyHandling(() => factory.Save(), null);
					ServiceLogger.Log(LogType.Information, $"Added {newPrintJobQueues.Count} print jobs of a same delivery group into the queue.");
					newPrintJobQueues.Clear();

					if (batchCount >= MaxBatchSize)
					{
						factory = new BusinessObjectFactory { RefreshEnabled = false };
						batchCount = 1;
					}
				}
				currentDeliveryGroupGuid = printJob.SP_SB_DeliveryGroup;

				var printJobQueue = QueuePrintJob(printJob, factory);
				newPrintJobQueues.Add(printJobQueue);
			});

			if (newPrintJobQueues.Count > 0)
			{
				globalSequence = UpdateGlobalSequence(newPrintJobQueues, globalSequence);
				ZExceptionReporting.ProcessWithConcurrencyHandling(() => factory.Save(), null);
				ServiceLogger.Log(LogType.Information, $"Added {newPrintJobQueues.Count} print jobs of a same delivery group into the queue.");
				newPrintJobQueues.Clear();
			}

			ServiceLogger.Log(LogType.Information, $"Finished adding {totalCount} print jobs into the queue.");
		} while (hasJobsToProcess);
	}

	public static StmPrintJobQueue QueuePrintJob(StmPrintJob printJob, BusinessObjectFactory factory)
	{
		var printJobQueue = factory.New<StmPrintJobQueue>();
		printJobQueue.SPQ_SP_PrintJob = printJob.PK;
		printJobQueue.SPQ_JobType = printJob.SP_JobType;
		printJobQueue.SPQ_EDocsProcessed = printJob.SP_EDocsProcessed;
		printJobQueue.SPQ_SB_DeliveryGroup = printJob.SP_SB_DeliveryGroup;

		printJobQueue.SPQ_DeliveryGroupForLock = printJob.SP_SB_DeliveryGroup.ToString().ToUpperInvariant();
		printJobQueue.SPQ_ParentGuidForLock = (printJob.SP_ParentGuid.IsEmpty ? printJob.PK : printJob.SP_ParentGuid).ToString().ToUpperInvariant();

		var printQueue = printJob.PrintQueue;
		if (printQueue != null)
		{
			printJobQueue.SPQ_SPS_PrintServer = printQueue.SQ_SPS_Server;
			printJobQueue.SPQ_SQ_PrintQueue = printQueue.PK;
		}

		return printJobQueue;
	}

	long UpdateGlobalSequence(List<StmPrintJobQueue> newPrintJobQueues, long globalSequence)
	{
		foreach (var printeJobQueue in newPrintJobQueues.OrderBy(pjq => pjq.PrintJob.SP_Group).ThenBy(pjq => pjq.PrintJob.SP_Sequence))
		{
			globalSequence++;
			printeJobQueue.SPQ_Sequence = globalSequence;
		}

		return globalSequence;
	}

	long GetCurrentMaxGlobalSequence()
	{
#pragma warning disable CW1107
		using var command = Db.Connection.Command("SELECT MAX(SPQ_Sequence) FROM dbo.StmPrintJobQueue");

		var result = command.ExecuteScalar();
		return (result != null && result != DBNull.Value) ? Convert.ToInt64(result) : 0;
#pragma warning restore CW1107
	}

	internal static ZNonPersistentDataQuery GetReadyPrintJobsQuery()
	{
		const string sqlText = "dbo.GetReadyPrintJobsForQueueTable";

		return new ZNonPersistentDataQuery(sqlText);
	}
}

public class PrintJobSchedulingQueue : IHostedServiceQueueProvider
{
	public QueueResult QueueResult
	{
		get
		{
			var result = QueueResult.Error;
			Db.Connection.ExecuteReader(QueueSizeQuery, reader => result = new QueueResult(reader.GetInt32(0), TimeSpan.FromSeconds(reader.GetInt32(1))));
			return result;
		}
	}

	const string QueueSizeQuery =
		@"
SELECT COUNT(*), ISNULL(MAX(DATEDIFF(second, SP_RunDateTime, GETUTCDATE())), 0)
  FROM dbo.StmPrintJob
 WHERE SP_IsScheduled = 0
   AND SP_RunDateTime <= GetUtcDate()
   AND SP_Status <> 'FAL'
";
}

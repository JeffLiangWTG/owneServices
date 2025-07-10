using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Integration;
using Enterprise.ServiceManager.Tasks.PrintJobProcessor;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	WebPrintNudgingTask.Code,
	"WebPrint Nudging",
	"DOC",
	typeof(WebPrintNudgingTask),
	IsMandatory = false,
	ActiveByDefault = true,
	AllowsMultipleInstances = false,
	CanRunInAnyBranch = true,
	IsScheduleReadOnly = true,
	MinimumPeriod = "1minute",
	DefaultScheduleRunEvery = "1hour" // Nudging service task should run on nudging, so setting longer delay between unnecessary scheduled runs
)]

[assembly: HostedServiceBusinessObjectBinding(WebPrintNudgingTask.Code, StmPrintJobQueueSchema.Constants.TableName, new[] { StmPrintJobQueueSchema.Constants.SPQ_JobType + "=PRN" }, "WebPrint Jobs Nudging")]

namespace Enterprise.ServiceManager.Tasks.PrintJobProcessor;

public class WebPrintNudgingTask : ServiceProviderImpl
{
	public const string Code = "WPN";

	public override void RunTask(CancellationToken youMustReactToThisToken)
	{
		var nudgeablePrintQueues = new DbOnlyBusinessObjectQueue<StmPrintQueue>(GetNudgeablePrintQueuesQuery(), true);
		nudgeablePrintQueues.Process((printQueue, e) =>
		{
			e.Cancel |= youMustReactToThisToken.IsCancellationRequested;
			if (e.Cancel)
			{
				return;
			}

			SendNudgeForPrintQueue(printQueue);
		}, 100);
	}

	ZNonPersistentDataQuery GetNudgeablePrintQueuesQuery()
	{
		const string sqlText = "dbo.GetNudgeablePrintQueues";

		return new ZNonPersistentDataQuery(sqlText);
	}

	void SendNudgeForPrintQueue(StmPrintQueue printQueue)
	{
		var printJobQueue = GetLastQueuedPrintJobForPrintQueue(printQueue);
		if (printJobQueue == null)
		{
			return;
		}

		UpdateWebPrintNudgeWaterMark(printQueue, printJobQueue);

		NudgePrintServerCore(printQueue, printJobQueue.SPQ_SP_PrintJob);
	}

	protected virtual void NudgePrintServerCore(StmPrintQueue printQueue, ZGuid printJobPk)
	{
		ServiceLogger.Log(LogType.Information, $"Nudging WebPrint for print queue '{printQueue.SQ_QueueName}' on server {printQueue.SQ_ServerName}.");

		WebRequestHelper.NudgePrintServerAsync(printQueue, printJobPk);
	}

	StmPrintJobQueue GetLastQueuedPrintJobForPrintQueue(StmPrintQueue printQueue)
	{
		var query = new ZQuery(StmPrintJobQueueSchema.SPQ_JobType, "PRN");
		query.AddToFilter(StmPrintJobQueueSchema.SPQ_SQ_PrintQueue, printQueue.PK);
		query.OrderBy = StmPrintJobQueueSchema.Constants.SPQ_Sequence + " DESC";

		return printQueue.Factory.LoadTop1<StmPrintJobQueue>(query);
	}

	void UpdateWebPrintNudgeWaterMark(StmPrintQueue printQueue, StmPrintJobQueue printJobQueue)
	{
		if (printJobQueue == null)
		{
			return;
		}

		var query = new ZQuery(StmDataSchema.SD_Name, WebPrintNudgeWaterMark);
		query.AddToFilter(StmDataSchema.SD_Owner, printQueue.SQ_SPS_Server);
		query.AddToFilter(StmDataSchema.SD_DepartmentGuid, printQueue.PK);

		var stmData = printQueue.Factory.LoadTop1<StmData>(query);

		if (stmData == null)
		{
			stmData = printQueue.Factory.New<StmData>();
			stmData.SD_Name = WebPrintNudgeWaterMark;
			stmData.SD_Owner = printQueue.SQ_SPS_Server;
			stmData.SD_DepartmentGuid  = printQueue.PK;
		}

		stmData.SD_GuidValue = printJobQueue.PK;
		stmData.Factory.Save();
	}

	public const string WebPrintNudgeWaterMark = "WebPrintNudgeWaterMark";
}

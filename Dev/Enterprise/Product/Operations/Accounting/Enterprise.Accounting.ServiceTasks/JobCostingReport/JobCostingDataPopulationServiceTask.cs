using System;
using System.Globalization;
using System.Threading;
using CargoWise.Data;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.ServiceTasks;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedServiceBusinessObjectBinding("JCD", AccTransactionLinesSchema.Constants.TableName, new string[] { }, null)]

[assembly: HostedService(
	JobCostingDataPopulationServiceTask.Code,
	"Job Costing Data Queue Service Task",
	"ACC",
	typeof(JobCostingDataPopulationServiceTask),
	IsMandatory = true,
	CanRunInAnyBranch = true,
	MinimumPeriod = "15minutes",
	DefaultScheduleRunEvery = "1hour")
]

[assembly: HostedServiceQueueProvider("JCD", "Job Costing Data", typeof(JobCostingDataQueue))]

namespace Enterprise.Accounting.ServiceTasks
{
	public class JobCostingDataQueue : IHostedServiceQueueProvider
	{
		QueueResult IHostedServiceQueueProvider.QueueResult
		{
			get
			{
				var result = QueueResult.Error;

				//please note that JCQ_PostDate and JCQ_ReverseDate are local time.
				//although we are not considering time zone while doing datediff in the following sql, the result is accurate enough for the purpose.
				Db.Connection.ExecuteReader("select count(*), abs(datediff(second, isnull(isnull(min(JCQ_PostDate), min(JCQ_ReverseDate)), getutcdate()), getutcdate())) from dbo.JobCostingDataQueue", reader =>
				{
					result = new QueueResult(reader.GetInt32(0), TimeSpan.FromSeconds(reader.GetInt32(1)));
				});
				return result;
			}
		}
	}

	public class JobCostingDataPopulationServiceTask : ServiceProviderImpl
	{
		public const string Code = "JCD";

		[HostedServiceRequirement]
		public static string IsJCDInitialized() =>
			HostedServiceRequirementAttribute.CheckValueIsNotEqualTo(AccountingConfigurationRegistry.Instance.JCDServiceTaskController, JCDActionList.Codes.NotInitialized);

		public override void RunTask(CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			RunTaskCore();
		}

		protected virtual void RunTaskCore()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				InitializeAndExecuteStrategey(connection);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Log Message. Not Displayed in an UI")]
		protected void InitializeAndExecuteStrategey(DbConnection connection)
		{
			var regStatus = AccountingConfigurationRegistry.Instance.JCDServiceTaskController.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);

			JCDActionStrategy strategy = null;
			var actionMsg = string.Empty;

			if (regStatus == JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask
				|| regStatus == JCDActionList.Codes.CompletedAllDatabaseObjectsForJobCostingDataQueueHaveBeenCreated)
			{
				actionMsg = "Intializing Job Costing Data Queue service task";
				strategy = new JCDStartActionStrategy(connection, ServiceLogger);
			}
			else if (regStatus == JCDActionList.Codes.RemoveJobCostingDataQueueServiceTask)
			{
				actionMsg = "Removing Job Costing Data Queue service task related Database objects";
				strategy = new JCDDeleteActionStrategy(connection, ServiceLogger);
			}
			else if (regStatus == JCDActionList.Codes.ReInitializeJobCostingDataQueueServiceTask)
			{
				actionMsg = "Re-initializing Job Costing Data Queue service task. This action will remove all existing Job Costing Report related processed data and prepare the service task to reprocess.";
				strategy = new JCDResetActionStrategy(connection, ServiceLogger);
			}
			else if (regStatus == JCDActionList.Codes.ProcessingOldTransactionLines
					 || regStatus == JCDActionList.Codes.CompletedOldTransactionLinesHaveBeenProcessed)
			{
				actionMsg = "Starting Job Costing Report related data processing";
				strategy = new JCDDefaultActionStrategy(connection, ServiceLogger);
			}
			else if (regStatus != JCDActionList.Codes.NotInitialized)
			{
				actionMsg = string.Format(CultureInfo.InvariantCulture, "Unrecognized value '{0}'. Please verify that you have a valid value in Job Costing Data Queue Service task Controller Registry", regStatus);
			}

			if (strategy != null)
			{
				ServiceLogger.Log(LogType.Debug, actionMsg);
				strategy.Process();
			}
			else if (!string.IsNullOrEmpty(actionMsg))
			{
				ServiceLogger.Log(LogType.Error, actionMsg);
			}
		}
	}
}

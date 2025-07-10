using System.Threading;
using CargoWise.Application;
using CargoWise.Bi.Common;
using CargoWise.Bi.Product.DataLoad;
using CargoWise.Data;
using Enterprise.ChangeDataCapture.Common;
using Enterprise.Integration;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	CargoWise.Bi.Product.ServiceTask.EdwEtlExecutionTask.ServiceTaskCode,
	CargoWise.Bi.Product.ServiceTask.EdwEtlExecutionTask.ServiceTaskName,
	"BI",
	typeof(CargoWise.Bi.Product.ServiceTask.EdwEtlExecutionTask),
	CanRunInAnyBranch = true,
	AllowsMultipleInstances = false,
	IsMandatory = true,
	MinimumPeriod = "5minutes",
	MaximumPeriod = "1day",
	MutuallyExclusiveTaskGroup = MutuallyExclusiveServiceTaskGroups.BiEdw,
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true)
]

[assembly: HostedServiceQueueProvider(
	CargoWise.Bi.Product.ServiceTask.EdwEtlExecutionTask.ServiceTaskCode,
	CargoWise.Bi.Product.ServiceTask.EdwEtlExecutionTask.ServiceTaskName,
	typeof(CargoWise.Bi.Product.ServiceTask.EdwEtlExecutionTaskQueue))]

namespace CargoWise.Bi.Product.ServiceTask
{
	#region SuppressResourceStringsCheckRegion

	public class EdwEtlExecutionTask : EtlExecutionTask, IBiNotificationSource
	{
		public const string ServiceTaskCode = "EET";
		public const string ServiceTaskName = "EDW ETL Execution Task"; // BI system service task.

		[HostedServiceRequirement]
		public static string CheckCdcIsEnabled() => CdcDatabase.CheckIsEnabled();

		[HostedServiceRequirement]
		public static string IsEdwEnabled() => BiServiceTaskHelpers.CheckEdwEnabledForHostedServiceRequirements();

		[HostedServiceRequirement]
		public static string CheckCdcShouldBeDisabled() => BiServiceTaskHelpers.IsCdcDisableFlagTrue();

		protected override string BiServerName
		{
			get
			{
				return dataWarehouseServer ??
					(dataWarehouseServer = BiServers.LoadDataWarehouseServerUsingCacheIfPossible(Db.Connection));
			}
		}
		string dataWarehouseServer;

		protected override string BiDatabaseName => Db.EdwDatabaseName;

		protected override void Run(CancellationToken iDoNotNeedToReactToThisToken)
		{
			if (!string.IsNullOrEmpty(BiServerName))
			{
				using (var biConnection = Db.NewExtraUnrestrictedWriterConnection(BiServerName, BiDatabaseName))
				{
					var etlExecutionManager = EtlExecutionManagerFactory.NewForEdwRecurringExecution(Db.Connection, biConnection, ServiceLogger);
					var shouldNudgeBiDeployment = etlExecutionManager.ExecuteEdwEtlProcess();

					if (shouldNudgeBiDeployment)
					{
						ServiceLogger.Log(LogType.Debug, "Nudging BI Deployment task");
						ObjectFactory.Get<IServiceTaskNudger>().NudgeServiceTask(BiDeploymentTask.ServiceTaskCode);
					}
				}
			}
			else
			{
				ServiceLogger.Log(LogType.Warning, "Check BI Data Warehouse Server registry is set before running EDW ETL Execution Task.");
			}
		}

		#region IBiNotificationSource Members

		string IBiNotificationSource.Code => ServiceTaskCode;
		string IBiNotificationSource.Description => ServiceTaskName;

		#endregion
	}

	#endregion
}

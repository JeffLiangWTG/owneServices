using System;
using CargoWise.Bi.Common;
using CargoWise.Bi.Maintenance;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Integration;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	CargoWise.Bi.Product.ServiceTask.Maintenance.BiMaintenanceTask.ServiceTaskCode,
	CargoWise.Bi.Product.ServiceTask.Maintenance.BiMaintenanceTask.ServiceTaskName,
	"BI",
	typeof(CargoWise.Bi.Product.ServiceTask.Maintenance.BiMaintenanceTask),
	AllowsMultipleInstances = false,
	CanRunInAnyBranch = true,
	IsMandatory = true,
	MinimumPeriod = "1day",
	MaximumPeriod = "1week",
	IsReadOnlyForWiseCloudClient = true,
	MutuallyExclusiveTaskGroup = MutuallyExclusiveServiceTaskGroups.BiEdw,
	DefaultScheduleRunEvery = "1day",
	DefaultScheduleStartAtLocal = "1hour",
	ActiveByDefault = true)
]

namespace CargoWise.Bi.Product.ServiceTask.Maintenance
{
	public class BiMaintenanceTask : BaseMaintenanceTask, IBiNotificationSource
	{
		#region SuppressResourceStringsCheckRegion

		public const string ServiceTaskCode = "BIM";
		public const string ServiceTaskName = "Business Intelligence Maintenance"; // Service task name

		[HostedServiceRequirement]
		public static string CheckCanLoadDataWarehouseServer() => BiServiceTaskHelpers.CheckEdwEnabledForHostedServiceRequirements();

		protected override string DatabaseServerName
		{
			get
			{
				return dataWarehouseServer ??
					(dataWarehouseServer = BiServers.LoadDataWarehouseServerUsingCacheIfPossible(Db.Connection));
			}
		}
		string dataWarehouseServer;

		protected override string DatabaseName => Db.EdwDatabaseName;

		protected override string NudgeTimeParamName => BiConstants.BimNudgeTimeParamName;

		protected override void RunMaintenanceTasks(DbConnection dbConnection)
		{
			RunFileGrowthScriptRunner(dbConnection);
			RunIndexMaintenance(dbConnection);
		}

		void RunIndexMaintenance(DbConnection biConnection)
		{
			try
			{
				var maintenance = EdwIndexMaintenance.New(biConnection, ServiceLogger);
				maintenance.RunIndexMaintenance();
			}
			catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.LockTimeoutExpired)
			{
				ServiceLogger.Log(LogType.Warning, "Lock timeout for index maintenance expired. ETL might have been running at the same time. Retrying in the next run.");
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Exceptions.Add(ex);
			}
		}

		void RunFileGrowthScriptRunner(DbConnection connection)
		{
			try
			{
				ServiceLogger.Log(LogType.Debug, "Optimizing FILEGROWTH value for EDW database.");
				new FileGrowthScriptRunner(connection).Run(Db.EdwDatabaseName);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Exceptions.Add(ex);
			}
		}

		#endregion

		#region IBiNotificationSource Members

		public override string Code => ServiceTaskCode;

		public override string Description => ServiceTaskName;

		#endregion
	}
}

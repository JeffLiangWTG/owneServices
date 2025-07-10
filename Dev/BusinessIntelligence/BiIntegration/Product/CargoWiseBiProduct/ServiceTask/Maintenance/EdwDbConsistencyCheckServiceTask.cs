using System;
using System.Threading;
using CargoWise.Bi.Common;
using CargoWise.Bi.Maintenance;
using CargoWise.Data;
using CargoWise.Data.Utils;
using Enterprise.Integration;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	CargoWise.Bi.Product.ServiceTask.Maintenance.EdwDbConsistencyCheckServiceTask.ServiceTaskCode,
	CargoWise.Bi.Product.ServiceTask.Maintenance.EdwDbConsistencyCheckServiceTask.ServiceTaskName,
	"BI",
	typeof(CargoWise.Bi.Product.ServiceTask.Maintenance.EdwDbConsistencyCheckServiceTask),
	CanRunInAnyBranch = true,
	IsMandatory = true,
	MinimumPeriod = "1day",
	MaximumPeriod = "1week",
	IsReadOnlyForWiseCloudClient = true,
	AllowsMultipleInstances = false,
	DefaultScheduleRunEvery = "1week",
	DefaultScheduleDaysOfWeek = new DayOfWeek[] { DayOfWeek.Sunday },
	DefaultScheduleStartAtLocal = "5hours",
	ActiveByDefault = true)
]

namespace CargoWise.Bi.Product.ServiceTask.Maintenance
{
	public class EdwDbConsistencyCheckServiceTask : ServiceProviderImpl, IBiNotificationSource
	{
		#region SuppressResourceStringsCheckRegion

		public const string ServiceTaskCode = "ICE";
		public const string ServiceTaskName = "EDW Database Consistency Check Service";

		[HostedServiceRequirement]
		public static string CheckCanLoadDataWarehouseServer() => BiServiceTaskHelpers.CheckEdwEnabledForHostedServiceRequirements();

		public override void RunTask(CancellationToken iDoNotNeedToReactToThisToken)
		{
			try
			{
				RunDbHealthCheck();
			}
			catch (SqlLockLostException ex)
			{
				ServiceLogger.Log(LogType.Debug, ex.Message);
			}
		}

		string DataWarehouseServer
		{
			get
			{
				return dwServer ?? (dwServer = BiServers.LoadDataWarehouseServerUsingCacheIfPossible(Db.Connection));
			}
		}
		string dwServer;

		void RunDbHealthCheck()
		{
			if (!string.IsNullOrEmpty(DataWarehouseServer))
			{
				var dbChecker = new BiDatabaseChecker(DataWarehouseServer, Db.EdwDatabaseName, ServiceLogger);
				var healthWarnings = dbChecker.Check();
			}
		}

		#endregion

		#region IBiNotificationSource Members

		string IBiNotificationSource.Code => ServiceTaskCode;
		string IBiNotificationSource.Description => ServiceTaskName;

		#endregion
	}
}

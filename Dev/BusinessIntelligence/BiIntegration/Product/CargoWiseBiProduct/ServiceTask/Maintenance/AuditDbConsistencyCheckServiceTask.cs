using System;
using System.Threading;
using CargoWise.Bi.Common;
using CargoWise.Bi.Maintenance;
using CargoWise.Data;
using CargoWise.Data.Utils;
using Enterprise.Integration;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	CargoWise.Bi.Product.ServiceTask.Maintenance.AuditDbConsistencyCheckServiceTask.ServiceTaskCode,
	CargoWise.Bi.Product.ServiceTask.Maintenance.AuditDbConsistencyCheckServiceTask.ServiceTaskName,
	"BI",
	typeof(CargoWise.Bi.Product.ServiceTask.Maintenance.AuditDbConsistencyCheckServiceTask),
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
	public class AuditDbConsistencyCheckServiceTask : ServiceProviderImpl, IBiNotificationSource
	{
		#region SuppressResourceStringsCheckRegion

		public const string ServiceTaskCode = "ICA";
		public const string ServiceTaskName = "Audit Database Consistency Check Service";

		[HostedServiceRequirement]
		public static string CheckCanLoadAuditServer() => BiServiceTaskHelpers.CheckAuditEnabledForHostedServiceRequirements();

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

		string AuditServer
		{
			get
			{
				return auditServer ?? (auditServer = BiServers.LoadAuditServerUsingCacheIfPossible(Db.Connection));
			}
		}
		string auditServer;

		void RunDbHealthCheck()
		{
			if (!string.IsNullOrEmpty(AuditServer))
			{
				var dbChecker = new BiDatabaseChecker(AuditServer, Db.AuditDatabaseName, ServiceLogger);
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

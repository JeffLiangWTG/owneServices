using System.Threading;
using CargoWise.Application;
using CargoWise.Bi.Common;
using CargoWise.Bi.Product.DataLoad;
using CargoWise.Data;
using Enterprise.AuditDataServices.Notification;
using Enterprise.ChangeDataCapture.Common;
using Enterprise.Integration;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	CargoWise.Bi.Product.ServiceTask.AuditEtlExecutionTask.ServiceTaskCode,
	CargoWise.Bi.Product.ServiceTask.AuditEtlExecutionTask.ServiceTaskName,
	"BI",
	typeof(CargoWise.Bi.Product.ServiceTask.AuditEtlExecutionTask),
	CanRunInAnyBranch = true,
	AllowsMultipleInstances = false,
	IsMandatory = true,
	MinimumPeriod = "5minutes",
	MaximumPeriod = "1day",
	MutuallyExclusiveTaskGroup = MutuallyExclusiveServiceTaskGroups.BiAudit,
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true)
]

[assembly: WTG.StaticAnalysis.Annotation.UsesConstants(typeof(Enterprise.AuditDataServices.Notification.AuditSubscriberProcessorTask))]

[assembly: HostedServiceQueueProvider(
	CargoWise.Bi.Product.ServiceTask.AuditEtlExecutionTask.ServiceTaskCode,
	CargoWise.Bi.Product.ServiceTask.AuditEtlExecutionTask.ServiceTaskName,
	typeof(CargoWise.Bi.Product.ServiceTask.AuditEtlExecutionTaskQueue))]

namespace CargoWise.Bi.Product.ServiceTask
{
	#region SuppressResourceStringsCheckRegion

	public class AuditEtlExecutionTask : EtlExecutionTask, IBiNotificationSource
	{
		public const string ServiceTaskCode = "AET";
		public const string ServiceTaskName = "Audit ETL Execution Task"; // BI system service task.

		[HostedServiceRequirement]
		public static string CheckCdcIsEnabled() => CdcDatabase.CheckIsEnabled();

		[HostedServiceRequirement]
		public static string IsAuditEnabled() => BiServiceTaskHelpers.CheckAuditEnabledForHostedServiceRequirements();

		[HostedServiceRequirement]
		public static string CheckCdcShouldBeDisabled() => BiServiceTaskHelpers.IsCdcDisableFlagTrue();

		protected override string BiServerName
		{
			get
			{
				return auditServer ??
					(auditServer = BiServers.LoadAuditServerUsingCacheIfPossible(Db.Connection));
			}
		}
		string auditServer;

		protected override string BiDatabaseName => Db.AuditDatabaseName;

		protected override void Run(CancellationToken iDoNotNeedToReactToThisToken)
		{
			if (!string.IsNullOrEmpty(BiServerName))
			{
				using (var biConnection = Db.NewExtraUnrestrictedWriterConnection(BiServerName, BiDatabaseName))
				{
					var etlExecutionManager = EtlExecutionManagerFactory.NewForAuditRecurringExecution(Db.Connection, biConnection, ServiceLogger);
					var shouldNudgeAuditNotification = etlExecutionManager.ExecuteAuditEtlProcess();

					if (shouldNudgeAuditNotification)
					{
						ServiceLogger.Log(LogType.Debug, "Nudging Audit Subscriber Processor Service");
						ObjectFactory.Get<IServiceTaskNudger>().NudgeServiceTask(AuditSubscriberProcessorTask.ServiceTaskCode);
					}
				}
			}
			else
			{
				ServiceLogger.Log(LogType.Warning, "Check BI Audit Server registry is set before running Audit ETL Execution Task.");
			}
		}

		#region IBiNotificationSource Members

		string IBiNotificationSource.Code => ServiceTaskCode;
		string IBiNotificationSource.Description => ServiceTaskName;

		#endregion
	}

	#endregion
}

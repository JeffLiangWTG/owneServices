using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.Bi.Common;
using CargoWise.Bi.Maintenance;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.Types;
using Enterprise.AuditDataServices.Subscription;
using Enterprise.ChangeDataCapture.Common;
using Enterprise.Integration;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly:
	HostedService(
		Enterprise.AuditDataServices.Notification.AuditSubscriberProcessorTask.ServiceTaskCode,
		Enterprise.AuditDataServices.Notification.AuditSubscriberProcessorTask.ServiceTaskDescription,
		"BI",
		typeof(Enterprise.AuditDataServices.Notification.AuditSubscriberProcessorTask),
		CanRunInAnyBranch = true,
		IsMandatory = true,
		AllowsMultipleInstances = false,
		MinimumPeriod = "1minute",
		MaximumPeriod = "30minutes",
		MutuallyExclusiveTaskGroup = MutuallyExclusiveServiceTaskGroups.BiAudit,
		DefaultScheduleRunEvery = "5minutes",
		ActiveByDefault = true
		)
]

namespace Enterprise.AuditDataServices.Notification
{
	public class AuditSubscriberProcessorTask : ServiceProviderImpl
	{
		#region SuppressResourceStringsCheckRegion

		public const string ServiceTaskCode = "ASP";
		public const string ServiceTaskDescription = "Audit Subscriber Processor Service";

		[HostedServiceRequirement]
		public static string CheckCdcIsEnabled() => CdcDatabase.CheckIsEnabled();

		[HostedServiceRequirement]
		public static string CheckCanLoadAuditServer() => BiServiceTaskHelpers.CheckAuditEnabledForHostedServiceRequirements();

		[HostedServiceRequirement]
		public static string CheckCdcShouldBeDisabled() => BiServiceTaskHelpers.IsCdcDisableFlagTrue();

		public override void RunTask(CancellationToken token)
		{
			var nudgeSubscriberTasks = RunAuditMaintenanceIfRequired();
			if (nudgeSubscriberTasks)
			{
				NudgeAuditSubscriberServiceTasks(token);
			}
		}

		bool RunAuditMaintenanceIfRequired()
		{
			var success = true;

			using (var biConnection = Db.NewAdminConnection(AuditServer, Db.AuditDatabaseName))
			{
				if (ShouldRunMaintenance(biConnection))
				{
					success = false;
					using (var mainDbConnection = Db.NewExtraUnrestrictedWriterConnection(Db.ServerName, Db.DatabaseName))
					using (TryAndAcquireAuditSubscriberLocks(mainDbConnection, out var locksAcquired))
					{
						if (locksAcquired)
						{
							RunMaintenanceTasks(biConnection);
							success = true;
						}
					}
				}
			}

			return success;
		}

		string AuditServer
		{
			get
			{
				return auditServer ?? (auditServer = BiServers.LoadAuditServerUsingCacheIfPossible(Db.Connection));
			}
		}
		string auditServer;

		public IEnumerable<AuditSubscriberTask> AuditServiceTasks
		{
			get
			{
				return auditServiceTasks ?? (auditServiceTasks = new SubscriberLoader().EnumerateAllSubscriberTasks());
			}
			internal set
			{
				auditServiceTasks = value;
			}
		}
		IEnumerable<AuditSubscriberTask> auditServiceTasks;

		void NudgeAuditSubscriberServiceTasks(CancellationToken token)
		{
			foreach (var subscriberTask in AuditServiceTasks)
			{
				if (!token.IsCancellationRequested)
				{
					subscriberTask.NudgeAuditSubscriberTask(ServiceLogger);
				}
			}
		}

		#region Audit Maintenance

		internal virtual bool ShouldRunMaintenance(DbConnection biConnection)
		{
			var lastIndexRebuildDate = BiMasterState.GetParameterDate(biConnection, BiConstants.LastIndexRebuildUtcDt);
			return
				(lastIndexRebuildDate == null) ||
				(lastIndexRebuildDate != null && ZDateTime.UtcNow - lastIndexRebuildDate >= TimeSpan.FromHours(24));
		}

		internal static TimeSpan MaintenanceRunMinimumInterval => TimeSpan.FromHours(24);

		protected IDisposable TryAndAcquireAuditSubscriberLocks(DbConnection mainDbConnection, out bool locksAcquired)
		{
			var aspLocks = new List<SqlApplicationLock>();

			foreach (var serviceTask in AuditServiceTasks)
			{
				SqlApplicationLock aspLock;
				var lockKey = BiConstants.AspMaintainanceLockKey + serviceTask.ServiceTaskCode;
				ServiceLogger.Log(LogType.Debug, $"Acquiring lock for {lockKey}");
				if (!mainDbConnection.TryGetLock(lockKey, out aspLock))
				{
					locksAcquired = false;
					ServiceLogger.Warning($"Acquiring lock for {lockKey} Failed.");
					foreach (var acquiredLock in aspLocks)
					{
						acquiredLock.Dispose();
					}
					return null;
				}
				else
				{
					aspLocks.Add(aspLock);
				}
			}

			locksAcquired = true;
			return new DisposableAction(() =>
			{
				foreach (var aspLock in aspLocks)
				{
					aspLock.Dispose();
				}
			});
		}

		protected List<Exception> Exceptions
		{
			get
			{
				return exceptions ?? (exceptions = new List<Exception>());
			}
		}
		List<Exception> exceptions;

		protected void RunMaintenanceTasks(DbConnection biConnection)
		{
			try
			{
				ServiceLogger.Debug("Running Audit database maintenance");
				RunDropOldAuditColumns(biConnection);
				RunFileGrowthScriptRunner(biConnection);
				RunAuditPartitioning(biConnection);
				RunIndexMaintenance(biConnection);
				RunCdcHistorySummaryPopulator(biConnection);
				SubscriberManager.RemoveObsoleteSubscribers(biConnection);
			}
			catch (BiServiceLockException ex)
			{
				ServiceLogger.Log(LogType.Warning, ex.Message);
			}
		}

		void RunDropOldAuditColumns(DbConnection biConnection)
		{
			try
			{
				ServiceLogger.Log(LogType.Debug, "Dropping legacy columns in the Audit database.");
				new DropOldAuditColumnsScriptRunner(biConnection, ServiceLogger).Run();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Exceptions.Add(ex);
			}
		}

		void RunFileGrowthScriptRunner(DbConnection biConnection)
		{
			try
			{
				ServiceLogger.Log(LogType.Debug, "Optimizing FILEGROWTH value for Audit database.");
				new FileGrowthScriptRunner(biConnection).Run(Db.AuditDatabaseName);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Exceptions.Add(ex);
			}
		}

		protected virtual AuditPartitionScriptRunner GetAuditPartitionScriptRunner(DbConnection biConnection)
		{
			return new AuditPartitionScriptRunner(biConnection, ServiceLogger);
		}

		void RunAuditPartitioning(DbConnection biConnection)
		{
			try
			{
				GetAuditPartitionScriptRunner(biConnection).Run();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Exceptions.Add(ex);
			}
		}

		void RunIndexMaintenance(DbConnection biConnection)
		{
			try
			{
				var maintenance = AuditIndexMaintenance.New(biConnection, ServiceLogger);
				maintenance.RunIndexMaintenance();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Exceptions.Add(ex);
			}
		}

		void RunCdcHistorySummaryPopulator(DbConnection biConnection)
		{
			try
			{
				var populator = new CdcHistorySummaryPopulator(biConnection, ServiceLogger);
				populator.Run();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Exceptions.Add(ex);
			}
		}

		#endregion

		#endregion
	}
}

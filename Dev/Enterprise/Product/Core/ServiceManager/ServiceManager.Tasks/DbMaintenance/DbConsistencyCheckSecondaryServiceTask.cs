using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Data;
using CargoWise.Data.SqlServer;
using CargoWise.Data.Utils;
using Enterprise.DbBackup.Engine;
using Enterprise.DbHealth.Check;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ServiceManager.Tasks.DbMaintenance;
using Enterprise.ZArchitecture.AlwaysOnHelper;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	code: DbMaintenanceTasks.DbConsistencyCheckSecondaryCode,
	description: "Database Consistency Check on Secondaries",
	category: "DBM",
	type: typeof(DbConsistencyCheckSecondaryServiceTask),
	IsMandatory = false,
	MinimumPeriod = "1day",
	IsReadOnlyForWiseCloudClient = true,
	DefaultScheduleRunEvery = "1week",
	DefaultScheduleDaysOfWeek = new DayOfWeek[] { DayOfWeek.Sunday },
	DefaultScheduleStartAtLocal = "5hours",
	DefaultScheduleRandomStartOffset = "720minutes",
	CanRunInAnyBranch = true)
]

namespace Enterprise.ServiceManager.Tasks.DbMaintenance
{
	class DbConsistencyCheckSecondaryServiceTask : ServiceProviderImpl
	{
		public DbConsistencyCheckSecondaryServiceTask()
		{
		}

		public override void RunTask(CancellationToken iDoNotNeedToReactToThisToken)
		{
			try
			{
				if (AlwaysOn.IsDbPartOfAlwaysOn(Db.Connection, Db.DatabaseName))
				{
					CheckSingleReferenceDatabase();

					if (Env.Registry.DbccRunSecondariesInParallel)
					{
						RunCheckInParallel();
					}
					else
					{
						RunCheckInALoop();
					}
				}
				else
				{
					ServiceLogger.Information($"Skip database consistency check due to main database [{Db.DatabaseName}] is not a part of AlwaysOn");
				}
			}
			catch (SqlLockLostException ex)
			{
				ServiceLogger.Information(ex.Message);
			}
		}

		void CheckSingleReferenceDatabase()
		{
			if (AlwaysOn.IsDbPartOfAlwaysOn(Db.Connection, RefDbTableNameResolver.SingleRefDatabaseName))
			{
				ServiceLogger.Warning($"{RefDbTableNameResolver.SingleRefDatabaseName} must NOT be in AlwaysOn group.");
			}
		}

		#region Implementation

		void RunCheckInALoop()
		{
			ServiceLogger.Information($"Checking secondary servers in a loop");

			var dbChecker = new DbCheckRunner();
			var replicas = AlwaysOnHelper.GetAlwaysOnSecondaryReplicaNames(Db.DatabaseName, useCache: false);

			ServiceLogger.Log(LogType.Information, $"replicas: {string.Join(",", replicas)}");
			foreach (var replica in replicas)
			{
				var logger = new LoggerProxy(ServiceLogger, replica, lockObj: null);

				RunCheck(dbChecker, replica, logger);
			}
		}

		void RunCheckInParallel()
		{
			ServiceLogger.Information($"Checking secondary servers in parallel");

			var dbChecker = new DbCheckRunner();
			var tasks = new List<Task>();
			var lockObj = new object();
			var replicas = AlwaysOnHelper.GetAlwaysOnSecondaryReplicaNames(Db.DatabaseName, useCache: false);

			ServiceLogger.Log(LogType.Information, $"replicas: {string.Join(",", replicas)}");
			foreach (var replica in replicas)
			{
				var logger = new LoggerProxy(ServiceLogger, replica, lockObj);

				tasks.Add(Task.Run(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						RunCheck(dbChecker, replica, logger);
					}
				}));
			}

			Task.WaitAll(tasks.ToArray());
		}

		void RunCheck(DbCheckRunner dbChecker, string replica, ILogger logger)
		{
			try
			{
				using (var connection = Db.NewAdminConnection(replica, Db.DatabaseName))
				{
					var healthWarnings = dbChecker.CheckDatabaseConsistencyOnSecondaries(Db.Connection, connection, Db.DatabaseName, logger);
					SendEmailIfNeeded(healthWarnings, logger);
				}
			}
			catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.AlwaysOnAccessError)
			{
				logger.Information(ex.Message);
			}
			catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.CannotOpenDbRequestedInLogin)
			{
				logger.Information(ex.Message);
			}
		}

		void SendEmailIfNeeded(DbHealthWarningList healthWarnings, ILogger logger)
		{
			if (healthWarnings.Count > 0)
			{
				var emailSubject = Res.GetString("760CAFAE-B97B-4F80-8EFA-AD5FE2BFAD6D", "Database Consistency Check Secondary ({0})", healthWarnings.GetFormattedLicenceInfo(", "));
				var emailBody = healthWarnings.ToHtmlMessage();

				new EmailNotificationSender().SendNotificationToDatabaseAdministrator(emailSubject, emailBody, logger);
				foreach (var warning in healthWarnings)
				{
					logger.Log(LogType.Warning, warning.Description);
				}
			}
		}

		#endregion // Implementation

		#region Helper classes

		class LoggerProxy : ILogger
		{
			public LoggerProxy(ILogger inner, string replicaName, object lockObj)
			{
				this.inner = inner;
				ReplicaName = replicaName;
				LockObj = lockObj;
			}

			public void Log(LogType type, string message)
			{
				var newMessage = $"Server [{ReplicaName}]: {message}";
				if (LockObj != null)
				{
					lock (LockObj)
					{
						inner.Log(type, newMessage);
					}
				}
				else
				{
					inner.Log(type, newMessage);
				}
			}

			public void Log(LogType type, string message, Exception ex)
			{
				var newMessage = $"Server [{ReplicaName}]: {message}";
				if (LockObj != null)
				{
					lock (LockObj)
					{
						inner.Log(type, newMessage, ex);
					}
				}
				else
				{
					inner.Log(type, newMessage, ex);
				}
			}

			// Implementation

			readonly ILogger inner;
			string ReplicaName { get; }
			object LockObj { get; }

			// /Implementation
		}

		#endregion // Helper classes
	}
}

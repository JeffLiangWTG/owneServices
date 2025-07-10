using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ServiceManager.Business;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace CargoWise.Bi.Product.ServiceTask.Maintenance.Testing
{
	[TestedType(typeof(AuditMaintenanceTask))]
	class AuditMaintenanceTaskTest : ServiceTaskTestCase<AuditMaintenanceTask>
	{
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		public void TestInitialiseSchedule()
		{
			var testTask = new AuditMaintenanceTask();
			var hostedServiceAttribute = GetHostedServiceAttributes().FirstOrDefault();
			InitialiseTaskSchedule(testTask, out StmServiceTask taskSchedule);
			Assert("TaskPeriod", taskSchedule.Recurrence.MonthsRange);
			AssertEquals("MinimumPeriod", "1month", hostedServiceAttribute.MinimumPeriod);
			AssertEquals("MaximumPeriod", "1month", hostedServiceAttribute.MaximumPeriod);
			AssertEquals("TaskPeriodCount", 1, taskSchedule.Recurrence.Period);
			AssertEquals("DailyStartTime", true, taskSchedule.Recurrence.RecurringStartTimeUtc.Equals(ZDateTime.MinSmallDateTimeValue));
		}

		public void TestShrinkAuditDatabase()
		{
			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					connection.CreateDatabase(TestDbName, dataInitialSizeMb: 30);
					using (((ICurrentDbControl)connection).UseDatabase(TestDbName))
					{
						var initialDbSize = GetDatabaseFileSize(connection);

						var logger = new LoggerForTest();
						var task = new AuditMaintenanceTaskForTesting();
						task.ServiceLogger = logger;

						task.RunTask();
						var newDbSize = GetDatabaseFileSize(connection);

						Assert($"DB size needs to be smaller after shrinking.\r\nInitial size: {initialDbSize} MB, New Size: {newDbSize} MB", newDbSize < initialDbSize);
					}
				}
				finally
				{
					connection.ExecuteNonQuery($"DROP DATABASE [{TestDbName}]");
				}
			}
		}

		public void TestShrinkAuditDatabaseFailed_ShouldReschedule()
		{
			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					connection.CreateDatabase(TestDbName, dataInitialSizeMb: 30);
					using (((ICurrentDbControl)connection).UseDatabase(TestDbName))
					{
						var logger = new LoggerForTest();
						var task = new AuditMaintenanceTaskForTesting();
						task.ServiceLogger = logger;
						task.ThrowDBCCFailureErrorForTest = true;

						AssertNoExceptionThrown(task.RunTask);
						Assert("Should catch exception and log warning when DBCC Shirnk fild failed due to file is been locked.", (logger.LogEntries as List<string>).Exists(x =>
							x.Contains("DBCC Failure to adjust size. Task will be rescheduled in five minutes.")));
					}
				}
				finally
				{
					connection.ExecuteNonQuery($"DROP DATABASE [{TestDbName}]");
				}
			}
		}

		public void TestShrinkAuditDatabaseDidntWork()
		{
			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					connection.CreateDatabase(TestDbName, dataInitialSizeMb: 30);
					using (((ICurrentDbControl)connection).UseDatabase(TestDbName))
					{
						var logger = new LoggerForTest();
						var task = new AuditMaintenanceTaskForTesting();
						task.ServiceLogger = logger;
						task.DoNotRunShrinkDatabase = true;
						var initialDbSize = GetDatabaseFileSize(connection);

						task.RunTask();
						var newDbSize = GetDatabaseFileSize(connection);
						Assert("DB should not shrink", newDbSize >= initialDbSize);
						AssertCollectionContains(
							"Should log if database size does not change after running shrink",
							"Shrinking Audit database file failed - no change in space used",
							logger.LogEntries
						);
						AssertCollectionNotContains(
							"Should not log that it finished shrinking",
							"Shrinking Audit database file completed",
							logger.LogEntries
						);
					}
				}
				finally
				{
					connection.ExecuteNonQuery($"DROP DATABASE [{TestDbName}]");
				}
			}
		}

		public void TestShrinkDatabaseUsesAuditDbAfterReconnection()
		{
			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					connection.CreateDatabase(TestDbName, dataInitialSizeMb: 30);
					using (((ICurrentDbControl)connection).UseDatabase(TestDbName))
					{
						var logger = new LoggerForTest();
						var task = new AuditMaintenanceTaskForTesting();
						task.ServiceLogger = logger;
						task.KillConnectionDuringShrinkDatabase = true;

						AssertNoExceptionThrown("Should not use master database after reconnection, thus should not throw audit db file not found.", () => task.RunTask());
					}
				}
				finally
				{
					connection.ExecuteNonQuery($"DROP DATABASE [{TestDbName}]");
				}
			}
		}

		const string TestDbName = "TestDb";

		double GetDatabaseFileSize(DbConnection connection)
		{
			var sqlText = @"SELECT TOP 1 size * 8.0 / 1024.0 FROM sys.database_files WHERE type = 0";
			return Convert.ToDouble(connection.ExecuteScalar(sqlText));
		}

		class AuditMaintenanceTaskForTesting : AuditMaintenanceTask
		{
			protected override string AuditDatabaseName => TestDbName;
			protected override float MinimumDatabaseSizeForShrinkingMb => 0;
			protected override float IncrementalShrinkingSize => 10;

			public bool ThrowDBCCFailureErrorForTest;
			public bool DoNotRunShrinkDatabase;
			public bool KillConnectionDuringShrinkDatabase;

			protected override void ShrinkDatabase(AdminConnection biConnection, string fileName, long targetFileSize)
			{
				if (ThrowDBCCFailureErrorForTest)
				{
					var exception = SqlExceptionBuilder.CreateSqlException(3140, $"Could not adjust the space allocation for file '{fileName}'");
					throw exception;
				}

				if (!DoNotRunShrinkDatabase)
				{
					if (KillConnectionDuringShrinkDatabase)
					{
						AdoTestUtils.KillConnection(biConnection);
					}
					base.ShrinkDatabase(biConnection, fileName, targetFileSize);
				}
			}
		}
	}
}

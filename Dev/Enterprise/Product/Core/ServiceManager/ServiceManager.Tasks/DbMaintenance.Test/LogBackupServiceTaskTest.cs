using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Types;
using Enterprise.DbBackup.Engine;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ServiceManager.Tasks.DbMaintenance.Testing
{
	[TestedType(typeof(LogBackupServiceTask))]
	sealed class LogBackupServiceTaskTest : ServiceTaskTestCase<LogBackupServiceTask>
	{
		public void TestServiceTaskCanRunInAnyBranch()
		{
			Assert(GetHostedServiceAttributes().All(x => x.CanRunInAnyBranch));
		}

		public void TestLogBackupRunsUnconditionally()
		{
			CombineAssertions(() =>
			{
				Test(DatabaseTypes.Codes.Test, hostedOnWiseCloud: true);
				Test(DatabaseTypes.Codes.Test, hostedOnWiseCloud: false);
				Test(DatabaseTypes.Codes.Production, hostedOnWiseCloud: true);
				Test(DatabaseTypes.Codes.Production, hostedOnWiseCloud: false);
				Test(DatabaseTypes.Codes.Demo, hostedOnWiseCloud: true);
				Test(DatabaseTypes.Codes.Demo, hostedOnWiseCloud: false);
				Test(DatabaseTypes.Codes.Education, hostedOnWiseCloud: true);
				Test(DatabaseTypes.Codes.Education, hostedOnWiseCloud: false);
				Test(DatabaseTypes.Codes.Training, hostedOnWiseCloud: true);
				Test(DatabaseTypes.Codes.Training, hostedOnWiseCloud: false);
				Test(DatabaseTypes.Codes.WisecloudTrial, hostedOnWiseCloud: true);
				Test(DatabaseTypes.Codes.WisecloudTrial, hostedOnWiseCloud: false);
			});

			void Test(string databaseType, bool hostedOnWiseCloud)
			{
				// Arrange
				var keyForTest = ObjectFactory.Get<IProductRegistration>().KeyForTest;
				var savedData = keyForTest.DatabaseTypeForTest;
				var savedIsWiseTechGlobalDatabaseServerForTest = DataUtils.IsWiseTechGlobalDatabaseServerForTest;

				var loggerMock = new Mock<ILogger>();
				var dbMaintenanceProxyMock = new Mock<IDbMaintenanceProxy>(MockBehavior.Strict);
				dbMaintenanceProxyMock.Setup(x => x
					.RunLogBackup(It.IsAny<ILogger>(), It.IsAny<IEmailNotificationSender>())).Verifiable();

				using (ObjectFactory.Substitute(dbMaintenanceProxyMock.Object))
				using (new DisposableAction(() =>
				{
					keyForTest.DatabaseTypeForTest = savedData;
					DataUtils.IsWiseTechGlobalDatabaseServerForTest = savedIsWiseTechGlobalDatabaseServerForTest;
				}))
				{
					keyForTest.DatabaseTypeForTest = databaseType;
					DataUtils.IsWiseTechGlobalDatabaseServerForTest = hostedOnWiseCloud;

					var backupTask = new LogBackupServiceTask { ServiceLogger = loggerMock.Object };

					// Act
					backupTask.RunTask();

					// Assert
					AssertNoExceptionThrown(() =>
					{
						dbMaintenanceProxyMock.Verify(x => x
							.RunLogBackup(It.IsAny<ILogger>(), It.IsAny<IEmailNotificationSender>()), Times.Once);
						dbMaintenanceProxyMock.VerifyNoOtherCalls();
					});

					AssertNoExceptionThrown(() =>
					{
						loggerMock
							.Verify(logger => logger.Log(LogType.Information, It.IsRegex(@"Start process [0-9]+")), Times.Once);

						loggerMock
							.Verify(logger => logger.Log(LogType.Information, It.IsRegex(@"End process [0-9]+")), Times.Once);
					});
				}
			}
		}

		[TestUtcOffset(2, 0, 0)]
		public void TestInitialiseSchedule()
		{
			var logBackupTask = new LogBackupServiceTask();
			var attribute = GetHostedServiceAttributes().SingleOrDefault();

			InitialiseTaskSchedule(logBackupTask, out StmServiceTask taskSchedule);
			AssertEquals("ScheduleTask - IsActive", attribute.ActiveByDefault, taskSchedule.SST_Active);
			Assert("ScheduleTask - TaskPeriod", taskSchedule.Recurrence.MinutesRange);
			AssertEquals("ScheduleTask - TaskPeriodCount", 15, taskSchedule.Recurrence.Period);
			AssertEquals("ScheduleTask - WeekDaysOnly", ZBool.False, taskSchedule.Recurrence.WeekDaysOnly);
		}

		public void TestWrongParameters()
		{
			var result = AssertExceptionThrown<ArgumentNullException>(() => new LogBackupServiceTask(null));
			AssertEquals("emailNotificationSender", result.ParamName);
		}

		public void TestRunMoreThanOneSecond()
		{
			var logBackupTask = new LogBackupServiceTask();
			var loggerMock = new Mock<ILogger>();
			logBackupTask.ServiceLogger = loggerMock.Object;
			var startTime = DateTime.Now;
			logBackupTask.RunTask();
			var endTime = DateTime.Now;

			Assert(endTime - startTime > TimeSpan.FromSeconds(1));

			AssertNoExceptionThrown(() =>
			{
				loggerMock
					.Verify(logger => logger.Log(LogType.Information, It.IsRegex(@"Start process [0-9]+")), Times.Once);

				loggerMock
					.Verify(logger => logger.Log(LogType.Information, It.IsRegex(@"End process [0-9]+")), Times.Once);
			});
		}

		[UseSnapshotProtection]
		[ExpectNoExceptions]
		public void TestLogBackupWhenFullRecoveryModel()
		{
			// Arrange
			Env.Registry.BackupDirectoryPath = "BackupDirectoryPathMock";
			DbRegistry.BiAuditServer.SaveValue(Db.ServerName, Db.Connection);
			DbRegistry.BiDataWarehouseServer.SaveValue(Db.ServerName, Db.Connection);
			var serviceLoggerMock = new Mock<ILogger>();
			var logBackupTask = new LogBackupServiceTask()
			{
				ServiceLogger = serviceLoggerMock.Object
			};
			var allDatabase = GetAllDatabase();
			SetDatabasesRecoveryModel(allDatabase, DbRecoveryModel.Full);

			// Act
			logBackupTask.RunTask();

			// Assert
			allDatabase.ForEach(x =>
			{
				serviceLoggerMock
					.Verify(logger => logger.Log(LogType.Information, It.IsRegex($@"^Log Backup \[{x}\] - BackupDirectoryPathMock\\{x}_[0-9]{{14}}.trn")), Times.Once);
			});

			serviceLoggerMock
				.Verify(logger => logger.Log(LogType.Information, It.IsRegex(@"Start process [0-9]+")), Times.Once);

			serviceLoggerMock
				.Verify(logger => logger.Log(LogType.Information, It.IsRegex(@"End process [0-9]+")), Times.Once);
		}

		[UseSnapshotProtection]
		[ExpectNoExceptions]
		public void TestSkipLogBackupWhenSimpleRecoveryModel()
		{
			// Arrange
			Env.Registry.BackupDirectoryPath = "BackupDirectoryPathMock";
			DbRegistry.BiAuditServer.SaveValue(Db.ServerName, Db.Connection);
			DbRegistry.BiDataWarehouseServer.SaveValue(Db.ServerName, Db.Connection);
			var serviceLoggerMock = new Mock<ILogger>();
			var logBackupTask = new LogBackupServiceTask()
			{
				ServiceLogger = serviceLoggerMock.Object
			};

			var allDatabase = GetAllDatabase();
			SetDatabasesRecoveryModel(allDatabase, DbRecoveryModel.Simple);

			// Act
			logBackupTask.RunTask();

			// Assert
			allDatabase.ForEach(x =>
			{
				serviceLoggerMock.Verify(logger => logger.Log(LogType.Information, It.IsRegex($@"^Skipped Log Backup \[{x}\] - Backup is not applicable : Database is in simple recovery model")), Times.Once);
			});

			serviceLoggerMock
				.Verify(logger => logger.Log(LogType.Information, It.IsRegex(@"Start process [0-9]+")), Times.Once);

			serviceLoggerMock
				.Verify(logger => logger.Log(LogType.Information, It.IsRegex(@"End process [0-9]+")), Times.Once);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		void SetDatabasesRecoveryModel(IEnumerable<string> dbNames, DbRecoveryModel recoveryModel)
		{
			using (var connection = Db.NewAdminConnection())
			{
				foreach (var dbName in dbNames)
				{
					AdoTestUtils.SetDbRecoveryModelForTest(connection, dbName, recoveryModel);
				}
			}
		}

		public ICollection<string> GetAllDatabase()
		{
			return Db.Connection.GetDatabases(DatabaseType.AllExclusive).ToList();
		}
	}
}

using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Startup.Testing
{
	sealed class ServiceTasksSchedulesCleanerTest : TransactionedTestCase
	{
		public void TestDeleteSchedulesOfDecommissionedServiceTask()
		{
			var cleaner = new ServiceTasksSchedulesCleaner();
			var allCodes = cleaner.GetAllCurrentServiceCodes();
			Assert(!allCodes.Contains("AAA"));
			Assert(!allCodes.Contains("BBB"));

			Db.Connection.ExecuteNonQuery(@"
			INSERT INTO dbo.StmServiceTask (SST_PK,SST_ServiceTaskCode,SST_Active,SST_Configuration,SST_SystemCreateTimeUtc,SST_SystemCreateUser,SST_SystemLastEditTimeUtc,SST_SystemLastEditUser,SST_NextRunTime) VALUES (NEWID(),'AAA',1,'',GetUtcDate(),'~BP',GetUtcDate(),'~BP',GetUtcDate())
			INSERT INTO dbo.StmServiceTask (SST_PK,SST_ServiceTaskCode,SST_Active,SST_Configuration,SST_SystemCreateTimeUtc,SST_SystemCreateUser,SST_SystemLastEditTimeUtc,SST_SystemLastEditUser,SST_NextRunTime) VALUES (NEWID(),'BBB',0,'',GetUtcDate(),'~BP',GetUtcDate(),'~BP',GetUtcDate())");

			AssertEquals(2, Db.Connection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.StmServiceTask WHERE SST_ServiceTaskCode IN ('AAA','BBB')"));

			var logger = new MemoryLoggerForTest();
			cleaner.DeleteSchedulesOfDecommissionedServiceTask(logger);
			AssertEquals(0, Db.Connection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.StmServiceTask WHERE SST_ServiceTaskCode IN ('AAA','BBB')"));

			CombineAssertions(() =>
			{
				AssertStartsWith("Logger should begin with expected message.", @"Beginning to delete old schedules for decommissioned service tasks.
> Decommissioned schedule(s): ", logger.ToString());
				AssertContains("AAA", logger.ToString());
				AssertContains("BBB", logger.ToString());
			});
		}

		public void TestNoServiceTasksToBeDeleted()
		{
			var cleaner = new ServiceTasksSchedulesCleaner();
			var allCodes = cleaner.GetAllCurrentServiceCodes();
			Assert(allCodes.Contains("SRR"));

			Db.Connection.ExecuteNonQuery(@"
			INSERT INTO dbo.StmServiceTask (SST_PK,SST_ServiceTaskCode,SST_Active,SST_Configuration,SST_SystemCreateTimeUtc,SST_SystemCreateUser,SST_SystemLastEditTimeUtc,SST_SystemLastEditUser,SST_NextRunTime) VALUES (NEWID(),'SRR',1,'',GetUtcDate(),'~BP',GetUtcDate(),'~BP',GetUtcDate())");

			var logger = new MemoryLoggerForTest();
			cleaner.DeleteSchedulesOfDecommissionedServiceTask(logger);
			AssertMultilineASCIIEquals(@"Beginning to delete old schedules for decommissioned service tasks.
> No decommissioned schedules deleted.", logger.ToString());
		}

		public void TestDeleteSchedulesOfDecommissionedServiceTaskInOldTable()
		{
			var cleaner = new ServiceTasksSchedulesCleaner();
			var allCodes = cleaner.GetAllCurrentServiceCodes();
			Assert(!allCodes.Contains("AAA"));
			Assert(!allCodes.Contains("BBB"));

			Db.Connection.ExecuteNonQuery(@"
			INSERT INTO dbo.StmScheduleTask (S5_PK, S5_ParentTableCode,S5_ScheduleType,S5_ScheduleDescription,S5_TaskPeriod) VALUES (NEWID(), 'SH','AAA','Description A','D')
			INSERT INTO dbo.StmScheduleTask (S5_PK, S5_ParentTableCode,S5_ScheduleType,S5_ScheduleDescription,S5_TaskPeriod) VALUES (NEWID(), 'SH','BBB','Description B','D')");

			AssertEquals(2, Db.Connection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.StmScheduleTask WHERE S5_ScheduleType IN ('AAA','BBB')"));

			var logger = new MemoryLoggerForTest();
			cleaner.DeleteSchedulesOfDecommissionedServiceTask(logger);
			AssertEquals(0, Db.Connection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.StmScheduleTask WHERE S5_ScheduleType IN ('AAA','BBB')"));
		}

		public void TestNoServiceTasksToBeDeletedInOldTable()
		{
			var cleaner = new ServiceTasksSchedulesCleaner();
			var allCodes = cleaner.GetAllCurrentServiceCodes();
			Assert(allCodes.Contains("SRR"));

			Db.Connection.ExecuteNonQuery(@"
			INSERT INTO dbo.StmScheduleTask (S5_PK, S5_ParentTableCode,S5_ScheduleType,S5_TaskPeriod) VALUES (NEWID(), 'SH','SRR','D')");

			var logger = new MemoryLoggerForTest();
			cleaner.DeleteSchedulesOfDecommissionedServiceTask(logger);
			AssertMultilineASCIIEquals(@"Beginning to delete old schedules for decommissioned service tasks.
> No decommissioned schedules deleted.", logger.ToString());
		}

		[UseSnapshotProtection]
		class ClientSpecificTest : TestCase
		{
			public void TestDeleteScheduleOfUnmatchClientSpecificServiceTask()
			{
				// Arrange
				var cleaner = new ServiceTasksSchedulesCleaner();
				var allCodes = cleaner.GetAllCurrentServiceCodes();
				Assert(!allCodes.Contains("ZN1"));

				Db.Connection.ExecuteNonQuery(@"
				INSERT INTO dbo.StmServiceTask (SST_PK,SST_ServiceTaskCode,SST_Active,SST_Configuration,SST_SystemCreateTimeUtc,SST_SystemCreateUser,SST_SystemLastEditTimeUtc,SST_SystemLastEditUser,SST_NextRunTime) VALUES (NEWID(),'ZN1',1,'',GetUtcDate(),'~BP',GetUtcDate(),'~BP',GetUtcDate())");

				var logger = new MemoryLoggerForTest();

				// Act
				cleaner.DeleteSchedulesOfDecommissionedServiceTask(logger);

				// Assert
				AssertMultilineASCIIEquals(@"Beginning to delete old schedules for decommissioned service tasks.
> Decommissioned schedule(s): ZN1 are deleted.", logger.ToString());
			}

			public void TestDoNotDeleteScheduleOfMatchedClientSpecificServiceTask()
			{
				// Arrange
				var clientHookMock = new Mock<IClientHook>();
				clientHookMock.SetupGet(c => c.UniqueId).Returns("NIP");

				using (ObjectFactory.Get<IClientHookLoader>().OverrideClientHookForTest(clientHookMock.Object, false, true))
				{
					var cleaner = new ServiceTasksSchedulesCleaner();
					var allCodes = cleaner.GetAllCurrentServiceCodes();
					Assert(allCodes.Contains("ZN1"));

					Db.Connection.ExecuteNonQuery(@"
					INSERT INTO dbo.StmServiceTask (SST_PK,SST_ServiceTaskCode,SST_Active,SST_Configuration,SST_SystemCreateTimeUtc,SST_SystemCreateUser,SST_SystemLastEditTimeUtc,SST_SystemLastEditUser,SST_NextRunTime) VALUES (NEWID(),'ZN1',1,'',GetUtcDate(),'~BP',GetUtcDate(),'~BP',GetUtcDate())");

					var logger = new MemoryLoggerForTest();

					// Act
					cleaner.DeleteSchedulesOfDecommissionedServiceTask(logger);

					// Assert
					AssertMultilineASCIIEquals(@"Beginning to delete old schedules for decommissioned service tasks.
> No decommissioned schedules deleted.", logger.ToString());
				}
			}

			public void TestDeleteScheduleOfUnmatchClientSpecificServiceTaskInOldTable()
			{
				// Arrange
				var cleaner = new ServiceTasksSchedulesCleaner();
				var allCodes = cleaner.GetAllCurrentServiceCodes();
				Assert(!allCodes.Contains("ZN1"));

				Db.Connection.ExecuteNonQuery(@"
					INSERT INTO dbo.StmScheduleTask (S5_PK, S5_ParentTableCode,S5_ScheduleType,S5_ScheduleDescription,S5_TaskPeriod) VALUES (NEWID(), 'SH','ZN1','Description ZN1','D')");

				var logger = new MemoryLoggerForTest();

				// Act
				cleaner.DeleteSchedulesOfDecommissionedServiceTask(logger);

				// Assert
				AssertEquals(0, Db.Connection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.StmScheduleTask WHERE S5_ScheduleType = 'ZN1'"));
			}

			public void TestDoNotDeleteScheduleOfMatchedClientSpecificServiceTaskInOldTable()
			{
				// Arrange
				var clientHookMock = new Mock<IClientHook>();
				clientHookMock.SetupGet(c => c.UniqueId).Returns("NIP");

				using (ObjectFactory.Get<IClientHookLoader>().OverrideClientHookForTest(clientHookMock.Object, false, true))
				{
					var cleaner = new ServiceTasksSchedulesCleaner();
					var allCodes = cleaner.GetAllCurrentServiceCodes();
					Assert(allCodes.Contains("ZN1"));

					Db.Connection.ExecuteNonQuery(@"
					INSERT INTO dbo.StmScheduleTask (S5_PK, S5_ParentTableCode,S5_ScheduleType,S5_ScheduleDescription,S5_TaskPeriod) VALUES (NEWID(), 'SH','ZN1','Description ZN1','D')");

					var logger = new MemoryLoggerForTest();

					// Act
					cleaner.DeleteSchedulesOfDecommissionedServiceTask(logger);

					// Assert
					AssertMultilineASCIIEquals(@"Beginning to delete old schedules for decommissioned service tasks.
> No decommissioned schedules deleted.", logger.ToString());
				}
			}
		}
	}
}

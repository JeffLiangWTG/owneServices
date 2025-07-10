using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using CargoWise.Application;
using CargoWise.Bi.Common;
using CargoWise.Bi.Maintenance;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.ChangeDataCapture.Common;
using Enterprise.ChangeDataCapture.Common.Testing;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Moq;
using ServiceManager.Integration.Abstractions;

namespace CargoWise.Bi.Product.ServiceTask.Testing
{
	[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
	class AuditEtlExecutionTaskTest : BaseAuditEtlExecutionTaskTest
	{
		public void TestCanAccessLinkedServer()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				EnsureDbReadyForAudit(adminConnection);
				var testLinkedServer = "LOCALHOST";
				var logger = new LoggerForTest();
				var linkedServerCreator = new LinkedServerCreatorForTest(adminConnection, testLinkedServer, logger);
				linkedServerCreator.EnsureLinkedServerNotExists();
				try
				{
					var registration = ObjectFactory.Get<IProductRegistration>();
					registration.KeyForTest.IsInternalSystemForTest = false;

					linkedServerCreator.CreateLinkedServer();

					using (var biConnection = Db.NewExtraUnrestrictedWriterConnection(Db.ServerName, Db.DatabaseName))
					{
						var testTask = new AuditEtlExecutionTask();
						var testLogger = new LoggerForTest();
						testTask.ServiceLogger = testLogger;
						AssertNoExceptionThrown(() => testTask.RunTask(CancellationToken.None));
					}
				}
				finally
				{
					linkedServerCreator.EnsureLinkedServerNotExists();
				}
			}
		}

		public void TestNudgingAuditNotificationTask()
		{
			using (var connection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				EnsureDbReadyForAudit(connection);

				var auditEtlExecutionTask = new AuditEtlExecutionTask();
				var testLogger = new LoggerForTest();
				auditEtlExecutionTask.ServiceLogger = testLogger;
				auditEtlExecutionTask.RunTask(CancellationToken.None);
				AssertCollectionNotContains("AET should not nudge Audit Subscriber task", "Nudging Audit Subscriber Processor Service", testLogger.LogEntries);

				connection.ExecuteNonQuery("INSERT INTO dbo.GlbStaff(GS_PK, GS_Code, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES(newid(), 'TST', GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
				var scanner = new CdcScannerForTest();
				scanner.ScanUntilNoTransactionsToProcess();

				testLogger.ClearLog();
				auditEtlExecutionTask.RunTask(CancellationToken.None);
				AssertCollectionContains($"AET should nudge Audit Subscriber task\r\nLogs:\r\n{string.Join("\r\n", testLogger.LogEntries)}", "Nudging Audit Subscriber Processor Service", testLogger.LogEntries);
			}
		}

		public void TestRun_DifferentSourceSchema()
		{
			var auditDbName = Db.AuditDatabaseName;

			using (var mainDbConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				TruncateTables(mainDbConnection, auditDbName);
				CreateTestTable(mainDbConnection, auditDbName, testTableName);
				EnableCdc(mainDbConnection);

				#region Master Audit Load

				var pk = Guid.NewGuid();

				InsertRow(mainDbConnection, pk);
				UpdateRow(mainDbConnection, pk);
				DeferredUpdateRow(mainDbConnection, pk);
				DeleteRow(mainDbConnection, pk);

				var scanner = new CdcScannerForTest();
				scanner.ScanUntilNoTransactionsToProcess(shouldAbort: () => false);

				var auditEtlExecutionTask = new AuditEtlExecutionTask();
				var testLogger = new LoggerForTest();
				auditEtlExecutionTask.ServiceLogger = testLogger;
				auditEtlExecutionTask.RunTask(CancellationToken.None);

				CheckAuditTable(mainDbConnection, auditDbName);
				CheckCdcHistorySummary(mainDbConnection, auditDbName);

				#endregion
			}
		}

		public void TestOnlyTablesWithChangesUpdateLastStateModifiedTime()
		{
			var testTable1 = "testTable1";
			var testTable2 = "testTable2";
			using (var connection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				if (!CdcDatabase.IsEnabled(connection, Db.DatabaseName))
				{
					CdcDatabase.Enable(connection, Db.DatabaseName);
				}
				TruncateTables(connection, Db.AuditDatabaseName);
				CreateAndEnableCdc(connection, testTable1);
				CreateAndEnableCdc(connection, testTable2);

				var scanner = new CdcScannerForTest();
				scanner.ScanUntilNoTransactionsToProcess(shouldAbort: () => false);

				var auditEtlExecutionTask = new AuditEtlExecutionTask();
				auditEtlExecutionTask.ServiceLogger = new LoggerForTest();
				auditEtlExecutionTask.RunTask(CancellationToken.None);

				DateTime lastMaxLsnTimeProcssed;
				using (((ICurrentDbControl)connection).UseDatabase(Db.AuditDatabaseName))
				{
					FormattableString getLastModifiedTime = $"select ParamValue from {BiConstants.BiAdminSchemaName}.MasterState where ParamName = '{BiConstants.LastMaxLsnTimeProcessed}'";
					lastMaxLsnTimeProcssed = Convert.ToDateTime(connection.ExecuteScalar(getLastModifiedTime.ToString(CultureInfo.InvariantCulture)) as string);
					FormattableString insert = $"insert into {Db.DatabaseName}.{testSchemaName}.{testTable1}(Column1ID, Column2Int, Column3Char) VALUES ('{new Guid()}','{0}','{"123"}')";
					connection.RunInTransaction(() => connection.ExecuteNonQuery(insert.ToString(CultureInfo.InvariantCulture)));

					scanner.ScanUntilNoTransactionsToProcess(shouldAbort: () => false);
					auditEtlExecutionTask.RunTask(CancellationToken.None);

					var actual = new List<KeyValuePair<string, DateTime>>();
					FormattableString assert = $"select SourceSchemaName, SourceTableName, StateModifiedTimestamp from {BiConstants.BiAdminSchemaName}.TableState where StateModifiedTimestamp > '{lastMaxLsnTimeProcssed:yyyy-MM-dd HH:mm:ss.fff}'";

					connection.ExecuteReader(assert.ToString(CultureInfo.InvariantCulture), record =>
					{
						var row = new KeyValuePair<string, DateTime>((string)record[0] + "." + (string)record[1], (DateTime)record[2]);
						actual.Add(row);
					});
					CombineAssertions(() =>
					{
						AssertEquals("Only ONE table state modified after last scan.", 1, actual.Count);
						AssertEquals($"{testSchemaName}.{testTable1}", actual[0].Key);
						AssertGreaterThan("Table last modified time should be later than the last scan time.", actual[0].Value, lastMaxLsnTimeProcssed);
					});
				}
			}
		}

		void CreateAndEnableCdc(AdminConnection connection, string tableName)
		{
			CreateTestTable(connection, Db.AuditDatabaseName, tableName);
			var cdcTable = new CdcTable(testSchemaName, tableName);
			if (!cdcTable.IsCdcEnabled(connection))
			{
				cdcTable.EnableCdc(connection);
			}
		}

		void EnsureDbReadyForAudit(AdminConnection connection)
		{
			if (!CdcDatabase.IsEnabled(connection, Db.DatabaseName))
			{
				CdcDatabase.Enable(connection, Db.DatabaseName);
				var cdcTable = new CdcTable("dbo", "GlbStaff");
				cdcTable.EnableCdc(connection);
			}
		}
	}
}

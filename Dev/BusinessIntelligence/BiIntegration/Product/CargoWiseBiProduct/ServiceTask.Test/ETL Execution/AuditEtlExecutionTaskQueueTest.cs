using System;
using System.Threading;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.ChangeDataCapture.Common.Testing;
using Enterprise.Integration;
using Moq;
using ServiceManager.Integration.Abstractions;

namespace CargoWise.Bi.Product.ServiceTask.Testing
{
	class AuditEtlExecutionTaskQueueTest : BaseAuditEtlExecutionTaskTest
	{
		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestAuditServiceTaskBacklog()
		{
			var auditDbName = Db.AuditDatabaseName;
			using (var connection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				TruncateTables(connection, auditDbName);
				CreateTestTable(connection, auditDbName, testTableName);
				EnableCdc(connection);

				var pk = Guid.NewGuid();

				InsertRow(connection, pk);
				UpdateRow(connection, pk);
				DeleteRow(connection, pk);

				var scanner = new CdcScannerForTest();
				scanner.ScanUntilNoTransactionsToProcess(shouldAbort: () => false);

				IHostedServiceQueueProvider provider = new AuditEtlExecutionTaskQueue();
				var auditEtlExecutionTask = new AuditEtlExecutionTask();
				var testLogger = new LoggerForTest();
				auditEtlExecutionTask.ServiceLogger = testLogger;

				auditEtlExecutionTask.RunTask(CancellationToken.None);
				AssertEquals(string.Join("\r\n", testLogger.LogEntries), 0, provider.QueueResult.QueueSize);
				var actualAge = provider.QueueResult.MaximumItemAge;
				AssertEquals(TimeSpan.Zero, actualAge);

				InsertRow(connection, pk);
				UpdateRow(connection, pk);
				scanner.ScanUntilNoTransactionsToProcess();
				AssertEquals(string.Join("\r\n", testLogger.LogEntries), 2, provider.QueueResult.QueueSize);

				auditEtlExecutionTask.RunTask(CancellationToken.None);
				AssertEquals(string.Join("\r\n", testLogger.LogEntries), 0, provider.QueueResult.QueueSize);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestAuditServiceTaskBacklog_AuditServerNotSet()
		{
			var auditDbName = Db.AuditDatabaseName;
			using (var connection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				TruncateTables(connection, auditDbName);
				CreateTestTable(connection, auditDbName, testTableName);
				EnableCdc(connection);

				var pk = Guid.NewGuid();

				InsertRow(connection, pk);
				UpdateRow(connection, pk);
				DeleteRow(connection, pk);

				var scanner = new CdcScannerForTest();
				scanner.ScanUntilNoTransactionsToProcess(shouldAbort: () => false);

				IHostedServiceQueueProvider provider = new AuditEtlExecutionTaskQueueForTesting() { IsAuditServerSet = false };
				var auditEtlExecutionTask = new AuditEtlExecutionTask();
				var testLogger = new LoggerForTest();
				auditEtlExecutionTask.ServiceLogger = testLogger;

				auditEtlExecutionTask.RunTask(CancellationToken.None);
				AssertEquals(string.Join("\r\n", testLogger.LogEntries), QueueResult.Zero.QueueSize, provider.QueueResult.QueueSize);

				InsertRow(connection, pk);
				UpdateRow(connection, pk);
				scanner.ScanUntilNoTransactionsToProcess();
				AssertEquals(string.Join("\r\n", testLogger.LogEntries), QueueResult.Zero.QueueSize, provider.QueueResult.QueueSize);

				auditEtlExecutionTask.RunTask(CancellationToken.None);
				AssertEquals(string.Join("\r\n", testLogger.LogEntries), QueueResult.Zero.QueueSize, provider.QueueResult.QueueSize);
			}
		}

		#region Implementation

		class AuditEtlExecutionTaskQueueForTesting : AuditEtlExecutionTaskQueue
		{
			public bool IsAuditServerSet = true;

			protected override string BiServer
			{
				get
				{
					return IsAuditServerSet ? base.BiServer : null;
				}
			}
		}

		#endregion
	}
}

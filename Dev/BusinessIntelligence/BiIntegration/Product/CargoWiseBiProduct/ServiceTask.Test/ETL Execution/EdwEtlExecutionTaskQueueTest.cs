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
	class EdwEtlExecutionTaskQueueTest : BaseEdwEtlExecutionTaskTest
	{
		[UseSnapshotProtection]
		[RequiresLargeDatabase(databaseNameSuffix: Db.EdwDatabaseSuffix)]
		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestEdwServiceTaskBacklog()
		{
			using (var mainDbConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				TruncateTables(mainDbConnection);
				DisableCdc(mainDbConnection);
				EnableCdc(mainDbConnection);
				new DbSecurity().RefreshSchemaDbRoles(mainDbConnection, msgs => { });

				var logger = new LoggerForTest();
				var testTask = new EdwEtlExecutionTaskForTesting(logger);
				var scanner = new CdcScannerForTest();

				IHostedServiceQueueProvider provider = new EdwEtlExecutionTaskQueue();
				AssertEquals(QueueResult.Zero.QueueSize, provider.QueueResult.QueueSize);
				AssertEquals(QueueResult.Zero.MaximumItemAge, provider.QueueResult.MaximumItemAge);

				scanner.ScanUntilNoTransactionsToProcess();
				testTask.RunTask(CancellationToken.None);

				#region Backlog initial load

				CreateChangeOnATable(mainDbConnection);
				scanner.ScanUntilNoTransactionsToProcess();
				testTask.RunTask(CancellationToken.None);

				CreateChangeOnATable(mainDbConnection);
				scanner.ScanUntilNoTransactionsToProcess();

				AssertEquals(1, provider.QueueResult.QueueSize);

				#endregion

				#region Backlog with change

				testTask.RunTask(CancellationToken.None);
				CreateChangeOnATable(mainDbConnection);
				CreateChangeOnATable(mainDbConnection);

				scanner.ScanUntilNoTransactionsToProcess();
				var resultFirst = provider.QueueResult;
				AssertEquals(2, resultFirst.QueueSize);
				Thread.Sleep(2000);
				var resultSecond = provider.QueueResult;
				AssertGreaterThan(resultSecond.MaximumItemAge.TotalSeconds, resultFirst.MaximumItemAge.TotalSeconds);

				#endregion

				#region Backlog with no change

				testTask.RunTask(CancellationToken.None);
				AssertEquals(0, provider.QueueResult.QueueSize);

				#endregion

			}
		}

		[UseSnapshotProtection]
		[RequiresLargeDatabase(databaseNameSuffix: Db.EdwDatabaseSuffix)]
		public void TestEdwServiceTaskBacklog_DataWarehouseServerNotSet()
		{
			using (var mainDbConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				TruncateTables(mainDbConnection);
				DisableCdc(mainDbConnection);
				EnableCdc(mainDbConnection);
				new DbSecurity().RefreshSchemaDbRoles(mainDbConnection, msgs => { });

				var logger = new LoggerForTest();
				var testTask = new EdwEtlExecutionTaskForTesting(logger);
				var scanner = new CdcScannerForTest();

				scanner.ScanUntilNoTransactionsToProcess();
				testTask.RunTask(CancellationToken.None);

				#region Backlog initial load

				CreateChangeOnATable(mainDbConnection);
				scanner.ScanUntilNoTransactionsToProcess();
				testTask.RunTask(CancellationToken.None);

				CreateChangeOnATable(mainDbConnection);
				scanner.ScanUntilNoTransactionsToProcess();

				IHostedServiceQueueProvider provider = new EdwEtlExecutionTaskQueueForTesting() { IsDataWarehouseServerSet = false };
				AssertEquals(QueueResult.Zero.QueueSize, provider.QueueResult.QueueSize);
				AssertEquals(QueueResult.Zero.MaximumItemAge, provider.QueueResult.MaximumItemAge);

				#endregion

			}
		}

		#region Implementation

		class EdwEtlExecutionTaskQueueForTesting : EdwEtlExecutionTaskQueue
		{
			public bool IsDataWarehouseServerSet = true;

			protected override string BiServer
			{
				get
				{
					return IsDataWarehouseServerSet ? base.BiServer : null;
				}
			}
		}

		#endregion
	}
}

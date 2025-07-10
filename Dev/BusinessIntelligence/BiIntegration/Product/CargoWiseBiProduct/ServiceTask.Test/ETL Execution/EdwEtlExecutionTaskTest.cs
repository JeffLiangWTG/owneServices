using System.Threading;
using CargoWise.Application;
using CargoWise.Bi.Maintenance;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.ChangeDataCapture.Common.Testing;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace CargoWise.Bi.Product.ServiceTask.Testing
{
	class EdwEtlExecutionTaskTest : BaseEdwEtlExecutionTaskTest
	{
		[UseSnapshotProtection]
		public void TestCanAccessLinkedServer()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				var testLinkedServer = "LOCALHOST";
				var logger = new LoggerForTest();
				var linkedServerCreator = new LinkedServerCreatorForTest(adminConnection, testLinkedServer, logger);
				linkedServerCreator.EnsureLinkedServerNotExists();
				try
				{
					var registration = ObjectFactory.Get<IProductRegistration>();
					registration.KeyForTest.IsInternalSystemForTest = false;

					linkedServerCreator.CreateLinkedServer();
					using (var biConnection = Db.NewAdminConnection(Db.ServerName, Db.DatabaseName))
					{
						var exists = linkedServerCreator.LinkedServerExistsAndIsAccessible(biConnection, testLinkedServer);
						Assert($"Linked server {testLinkedServer} does not exist or is not accessible from biConnection", exists);

						DisableCdc(adminConnection);
						EnableCdc(adminConnection);
						var testTask = new EdwEtlExecutionTaskForTesting(logger);
						AssertNoExceptionThrown(() => testTask.RunTask(CancellationToken.None));
					}
				}
				finally
				{
					linkedServerCreator.EnsureLinkedServerNotExists();
				}
			}
		}

		[UseSnapshotProtection]
		[RequiresLargeDatabase(databaseNameSuffix: Db.EdwDatabaseSuffix)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNudgingBiDeployment()
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

				#region Initial Load

				testTask.RunTask(CancellationToken.None);
				CreateChangeOnATable(mainDbConnection);
				scanner.ScanUntilNoTransactionsToProcess();
				testTask.RunTask(CancellationToken.None);
				AssertCollectionContains($"Initial load should nudge BI deployment task\r\nActual Logs:\r\n{string.Join("\r\n", logger.LogEntries)}", "Nudging BI Deployment task", logger.LogEntries);

				#endregion

				#region Incremental Load with no change

				logger.ClearLog();
				testTask.RunTask(CancellationToken.None);
				AssertCollectionNotContains($"Incremental load with no change should not nudge BI deployment task\r\nActual Logs:\r\n{string.Join("\r\n", logger.LogEntries)}", "Nudging BI Deployment task", logger.LogEntries);

				#endregion

				#region Incremental Load with change

				logger.ClearLog();
				CreateChangeOnATable(mainDbConnection);
				scanner.ScanUntilNoTransactionsToProcess();
				testTask.RunTask(CancellationToken.None);
				AssertCollectionContains($"Incremental load should nudge BI deployment task\r\nActual Logs:\r\n{string.Join("\r\n", logger.LogEntries)}", "Nudging BI Deployment task", logger.LogEntries);

				#endregion
			}
		}
	}
}

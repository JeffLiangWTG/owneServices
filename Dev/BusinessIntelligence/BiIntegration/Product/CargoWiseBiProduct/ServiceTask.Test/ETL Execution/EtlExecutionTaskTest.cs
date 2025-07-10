using System.Threading;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.ChangeDataCapture.Common;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace CargoWise.Bi.Product.ServiceTask.Testing
{
	class EtlExecutionTaskTest : TestCase
	{
		public void TestDisableApplicationLogin()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				try
				{
					((IDbLockout)adminConnection).DisableApplicationDbLogins();

					var testLogger = new LoggerForTest();
					var etlTask = new UpgradeInProgressEtlExecutionTask();
					etlTask.ServiceLogger = testLogger;
					etlTask.RunTask(CancellationToken.None);

					AssertCollectionContains("Expected log", "Last database upgrade terminated unsuccessfully. Check logs for errors.", testLogger.LogEntries);
				}
				finally
				{
					((IDbLoginRepair)adminConnection).EnableApplicationDbLogins();
				}
			}
		}

		[UseSnapshotProtection]
		public void TestMissingCdcTables()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				try
				{
					CdcDatabase.Disable(adminConnection, Db.DatabaseName);

					var testLogger = new LoggerForTest();
					var etlTask = new DummyEtlExecutionTask();
					etlTask.ServiceLogger = testLogger;
					etlTask.RunTask(CancellationToken.None);

					AssertCollectionContains("Expected log", "CDC tables are missing. Skipping ETL execution.", testLogger.LogEntries);
				}
				finally
				{
					((IDbLoginRepair)adminConnection).EnableApplicationDbLogins();
				}
			}
		}

		public void TestHandleCannotInitOleDbDataSourceObjForLinkedServerExceptionThrown()
		{
			EnvProxy.SetHostedLocationForTest("");
			var etlTask = new CannotInitOleDbDataSourceEtlExecutionTask();
			try
			{
				etlTask.RunTask(CancellationToken.None);
				Fail("Expected HostedServiceException to be thrown");
			}
			catch (HostedServiceException ex)
			{
				Assert(ex.LogException);
			}
		}

		public void TestHandleCannotInitOleDbDataSourceObjForLinkedServerExceptionNotThrown()
		{
			EnvProxy.SetHostedLocationForTest("SYD");
			var etlTask = new CannotInitOleDbDataSourceEtlExecutionTask();
			try
			{
				etlTask.RunTask(CancellationToken.None);
				Fail("Expected CannotInitOleDbDataSourceObjForLinkedServer exception.");
			}
			catch (SqlException ex)
			{
				AssertEquals(new DbErrorMatch(ex).ExceptionType, DbErrorType.CannotInitOleDbDataSourceObjForLinkedServer);
			}
		}

		public void TestHandleTCPProviderConnectionAttemptFailedExceptionThrown()
		{
			EnvProxy.SetHostedLocationForTest("");
			var etlTask = new TCPProviderConnectionAttemptFailedEtlExecutionTask();
			try
			{
				etlTask.RunTask(CancellationToken.None);
				Fail("Expected HostedServiceException to be thrown");
			}
			catch (HostedServiceException ex)
			{
				Assert(ex.LogException);
			}
		}

		public void TestHandleTCPProviderConnectionAttemptFailedExceptionNotThrown()
		{
			EnvProxy.SetHostedLocationForTest("SYD");
			var etlTask = new TCPProviderConnectionAttemptFailedEtlExecutionTask();
			try
			{
				etlTask.RunTask(CancellationToken.None);
				Fail("Expected TCPProviderConnectionAttemptFailed exception.");
			}
			catch (SqlException ex)
			{
				AssertEquals(new DbErrorMatch(ex).ExceptionType, DbErrorType.TCPProviderConnectionAttemptFailed);
			}
		}

		public void TestHandleInsufficientMemoryInBufferPoolExceptionThrown()
		{
			EnvProxy.SetHostedLocationForTest("");
			var etlTask = new InsufficientMemoryInBufferPoolEtlExecutionTask();
			try
			{
				etlTask.RunTask(CancellationToken.None);
				Fail("Expected HostedServiceException to be thrown");
			}
			catch (HostedServiceException ex)
			{
				Assert(ex.LogException);
			}
		}

		public void TestHandleInsufficientMemoryInBufferPoolNotThrown()
		{
			EnvProxy.SetHostedLocationForTest("SYD");
			var etlTask = new InsufficientMemoryInBufferPoolEtlExecutionTask();
			try
			{
				etlTask.RunTask(CancellationToken.None);
				Fail("Expected InsufficientMemoryInBufferPool exception.");
			}
			catch (SqlException ex)
			{
				AssertEquals(new DbErrorMatch(ex).ExceptionType, DbErrorType.InsufficientMemoryInBufferPool);
			}
		}

		public void TestHandleCouldNotLocateDbInSysdatabasesExceptionThrown()
		{
			EnvProxy.SetHostedLocationForTest("SYD");
			var etlTask = new CouldNotLocateDbInSysdatabasesEtlExecutionTask();
			try
			{
				etlTask.RunTask(CancellationToken.None);
				Fail("Expected CouldNotLocateDbInSysdatabases exception.");
			}
			catch (SqlException ex)
			{
				AssertEquals(new DbErrorMatch(ex).ExceptionType, DbErrorType.CouldNotLocateDbInSysdatabases);
			}
		}

		class DummyEtlExecutionTask : EtlExecutionTask
		{
			protected override string BiServerName => Db.ServerName;
			protected override string BiDatabaseName => Db.AuditDatabaseName;

			protected override void Run(CancellationToken iDoNotNeedToReactToThisToken)
			{
				using (var mainDbConnection = Db.NewExtraConnectionToMainDb())
				{
					mainDbConnection.DatabaseExists(Db.DatabaseName);
					mainDbConnection.ExecuteNonQuery("SELECT * FROM cdc.change_tables");
				}
			}
		}

		class UpgradeInProgressEtlExecutionTask : EtlExecutionTask
		{
			protected override string BiServerName => Db.ServerName;
			protected override string BiDatabaseName => Db.AuditDatabaseName;

			protected override void Run(CancellationToken iDoNotNeedToReactToThisToken)
			{
				throw new DatabaseUpgradeInProgressException();
			}
		}

		class CannotInitOleDbDataSourceEtlExecutionTask : EtlExecutionTask
		{
			protected override string BiServerName => Db.ServerName;
			protected override string BiDatabaseName => Db.AuditDatabaseName;

			protected override void Run(CancellationToken iDoNotNeedToReactToThisToken)
			{
				var sqlException = SqlExceptionBuilder.CreateSqlException(7303, "Cannot initialize the data source object of OLE DB provider");
				throw sqlException;
			}
		}

		class TCPProviderConnectionAttemptFailedEtlExecutionTask : EtlExecutionTask
		{
			protected override string BiServerName => Db.ServerName;
			protected override string BiDatabaseName => Db.AuditDatabaseName;

			protected override void Run(CancellationToken iDoNotNeedToReactToThisToken)
			{
				var sqlException = SqlExceptionBuilder.CreateSqlException(10060, "TCP Provider: A connection attempt failed because the connected party did not properly respond after a period of time, or established connection failed because connected host has failed to respond.");
				throw sqlException;
			}
		}

		class InsufficientMemoryInBufferPoolEtlExecutionTask : EtlExecutionTask
		{
			protected override string BiServerName => Db.ServerName;
			protected override string BiDatabaseName => Db.AuditDatabaseName;

			protected override void Run(CancellationToken iDoNotNeedToReactToThisToken)
			{
				var sqlException = SqlExceptionBuilder.CreateSqlException(802, "There is insufficient memory available in the buffer pool.");
				throw sqlException;
			}
		}

		class CouldNotLocateDbInSysdatabasesEtlExecutionTask : EtlExecutionTask
		{
			protected override string BiServerName => Db.ServerName;
			protected override string BiDatabaseName => Db.AuditDatabaseName;

			protected override void Run(CancellationToken iDoNotNeedToReactToThisToken)
			{
				var sqlException = SqlExceptionBuilder.CreateSqlException(911, "Database " + BiDatabaseName + " does not exist. Make sure that the name is entered correctly.");
				throw sqlException;
			}
		}
	}
}

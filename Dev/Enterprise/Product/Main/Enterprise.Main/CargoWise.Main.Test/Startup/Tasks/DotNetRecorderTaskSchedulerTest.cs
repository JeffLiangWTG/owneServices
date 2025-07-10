using CargoWise.Data;
using CargoWise.Definitions;
using Enterprise.Upgrades;
using Enterprise.ZArchitecture.Core.Test.Utilities;

namespace Enterprise.Startup.Testing
{
	sealed class DotNetRecorderTaskSchedulerTest : AbstractApplicationStartupTaskTest<DotNetRecorderTaskScheduler>
	{
		public void TestExecuteRecordsVersion()
		{
			Db.Connection.EnsureIsOpen();

			var fullyQualifiedComputerName = System.Net.Dns.GetHostEntry("")?.HostName;
			var dbInternals = (IDbConnectionInternals)Db.Connection;
			var sqlConnection = (SqlConnection)dbInternals.InternalDbConnection;
			var sqlTransaction = (SqlTransaction)dbInternals.InternalDbTransaction;
			var dbHandler = new DbHandlerForDotNet(new UpgradeSqlContext(sqlConnection, sqlConnection.DataSource, sqlConnection.Database, sqlTransaction));

			dbHandler.DeleteDotNetVersionRecord(fullyQualifiedComputerName);
			AssertNull("There must be no records of the test PC", dbHandler.GetDotNetVersionRecord(fullyQualifiedComputerName));
			DotNetRecorderTaskScheduler.Instance.Execute(null);
			AssertNotNull("It should record the .Net version", dbHandler.GetDotNetVersionRecord(fullyQualifiedComputerName));
		}

		public void TestExecuteRecordsRuntimes()
		{
			Db.Connection.EnsureIsOpen();

			var fullyQualifiedComputerName = System.Net.Dns.GetHostEntry("")?.HostName;
			var dbInternals = (IDbConnectionInternals)Db.Connection;
			var sqlConnection = (SqlConnection)dbInternals.InternalDbConnection;
			var sqlTransaction = (SqlTransaction)dbInternals.InternalDbTransaction;
			var dbHandler = new DbHandlerForDotNet(new UpgradeSqlContext(sqlConnection, sqlConnection.DataSource, sqlConnection.Database, sqlTransaction));

			dbHandler.DeleteDotNetCoreRuntimesRecord(fullyQualifiedComputerName);
			AssertNull("There must be no records of the test PC", dbHandler.GetDotNetCoreRuntimesRecord(fullyQualifiedComputerName));
			DotNetRecorderTaskScheduler.Instance.Execute(null);
			AssertNotNull("It should record the .Net runtimes", dbHandler.GetDotNetCoreRuntimesRecord(fullyQualifiedComputerName));
		}

		public override int DefaultErrorExitCode => ExitCodes.DotNetRecorderTaskSchedulerError;
	}
}

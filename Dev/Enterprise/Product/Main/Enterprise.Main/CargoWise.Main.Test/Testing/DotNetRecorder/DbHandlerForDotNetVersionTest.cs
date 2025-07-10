using System;
using CargoWise.Data;
using Enterprise.Upgrades;
using NUnit.Framework;

namespace Enterprise.Startup.Testing
{
	sealed class DbHandlerForDotNetTest : TestCase
	{
		public void TestSaveAndGetDotNetVersionRecordFromDb()
		{
			Db.Connection.EnsureIsOpen();

			var dbInternals = (IDbConnectionInternals)Db.Connection;
			var sqlConnection = (SqlConnection)dbInternals.InternalDbConnection;
			var sqlTransaction = (SqlTransaction)dbInternals.InternalDbTransaction;
			var dbHandler = new DbHandlerForDotNet(new UpgradeSqlContext(sqlConnection, sqlConnection.DataSource, sqlConnection.Database, sqlTransaction));
			var pcName = "MYPC";
			var version = new DotNetVersion("4.5", 1234);
			var expectedDotNetRecord = new PcWithDotNetVersionRecord(pcName, version, new DateTime(1, 1, 1, 1, 1, 1));

			dbHandler.DeleteDotNetVersionRecord("MYPC");
			AssertNull("This .Net record should not exist", dbHandler.GetDotNetVersionRecord("MYPC"));
			AssertNoExceptionThrown("It should be available to save the record in the DB", () => dbHandler.SaveDotNetRecord(expectedDotNetRecord));

			var retrievedDotNetRecord = dbHandler.GetDotNetVersionRecord(pcName);
			AssertEquals("The name of the pc retrieved should be the same", pcName, retrievedDotNetRecord.FullyQualifiedName);
			AssertEquals("The version of the pc retrieved should be the same", version.ToString(), retrievedDotNetRecord.DotNetVersion.ToString());
			AssertEquals("The pc retrieved should be the same", expectedDotNetRecord.ToString(), retrievedDotNetRecord.ToString());
		}

		public void TestSaveAndGetDotNetRuntimesRecordFromDb()
		{
			Db.Connection.EnsureIsOpen();

			var dbInternals = (IDbConnectionInternals)Db.Connection;
			var sqlConnection = (SqlConnection)dbInternals.InternalDbConnection;
			var sqlTransaction = (SqlTransaction)dbInternals.InternalDbTransaction;
			var dbHandler = new DbHandlerForDotNet(new UpgradeSqlContext(sqlConnection, sqlConnection.DataSource, sqlConnection.Database, sqlTransaction));
			var pcName = "MYPC";

			var expectedDotNetRecord = new PcWithDotNetCoreRuntimesRecord(pcName, new []
			{
				new DotNetCoreRuntime(new Version("6.0.31"), DotNetCoreRuntimeType.AspNetCoreRuntime),
				new DotNetCoreRuntime(new Version("8.0.6"), DotNetCoreRuntimeType.AspNetCoreRuntime),
				new DotNetCoreRuntime(new Version("6.0.31"), DotNetCoreRuntimeType.DotNetRuntime),
				new DotNetCoreRuntime(new Version("8.0.6"), DotNetCoreRuntimeType.DotNetRuntime),
				new DotNetCoreRuntime(new Version("6.0.31"), DotNetCoreRuntimeType.DotNetDesktopRuntime),
				new DotNetCoreRuntime(new Version("8.0.6"), DotNetCoreRuntimeType.DotNetDesktopRuntime),
			}, true, new Version("1.2.3"), new DateTime(2024, 1, 2, 3, 4, 5));

			dbHandler.DeleteDotNetCoreRuntimesRecord("MYPC");
			AssertNull("This .Net record should not exist", dbHandler.GetDotNetCoreRuntimesRecord("MYPC"));
			AssertNoExceptionThrown("It should be available to save the record in the DB", () => dbHandler.SaveDotNetRecord(expectedDotNetRecord));

			var retrievedDotNetRecord = dbHandler.GetDotNetCoreRuntimesRecord(pcName);
			AssertEquals("The name of the pc retrieved should be the same", pcName, retrievedDotNetRecord.FullyQualifiedName);
			AssertEquals("The IISAspNetCoreModuleVersion of the pc retrieved should be the same", expectedDotNetRecord.IISAspNetCoreModuleVersion, retrievedDotNetRecord.IISAspNetCoreModuleVersion);
			AssertEquals("The pc retrieved should be the same", expectedDotNetRecord.ToDbValue(), retrievedDotNetRecord.ToDbValue());
		}
	}
}

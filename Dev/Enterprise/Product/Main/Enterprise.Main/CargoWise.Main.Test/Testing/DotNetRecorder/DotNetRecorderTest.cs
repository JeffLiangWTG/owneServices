using System;
using System.Linq;
using CargoWise.Data;
using Enterprise.Upgrades;
using NUnit.Framework;

namespace Enterprise.Startup.Testing
{
	sealed class DotNetRecorderTest : TransactionedTestCase
	{
		public void TestRecordDotNetVersion()
		{
			const string PC_NAME = "HOST.DOMAIN";
			var dbInternals = (IDbConnectionInternals)Db.Connection;
			var sqlConnection = (SqlConnection)dbInternals.InternalDbConnection;
			var sqlTransaction = (SqlTransaction)dbInternals.InternalDbTransaction;
			var dbHandler = new DbHandlerForDotNet(new UpgradeSqlContext(sqlConnection, sqlConnection.DataSource, sqlConnection.Database, sqlTransaction));
			var recorder = new DotNetRecorder(dbHandler);
			var initialRecordingDate = DateTime.UtcNow;

			dbHandler.DeleteDotNetVersionRecord(PC_NAME);
			AssertNull("There must be no records of the test PC", dbHandler.GetDotNetVersionRecord(PC_NAME));

			var pcWithDotNewRecord = new PcWithDotNetVersionRecord(PC_NAME, new DotNetVersion("4.6", 123), initialRecordingDate);
			recorder.RecordDotNetVersion(pcWithDotNewRecord);
			AssertEquals("This .net record should be saved in the DB", pcWithDotNewRecord.GetRecordingDateInISO8601(), dbHandler.GetDotNetVersionRecord(PC_NAME)?.GetRecordingDateInISO8601());

			var ignoredPcWithDotNewRecord = new PcWithDotNetVersionRecord(PC_NAME, new DotNetVersion("4.6", 123), initialRecordingDate.AddHours(1));
			recorder.RecordDotNetVersion(ignoredPcWithDotNewRecord);
			AssertEquals("The record should be ignored because the record in the DB is not that old(24 h)", pcWithDotNewRecord.GetRecordingDateInISO8601(), dbHandler.GetDotNetVersionRecord(PC_NAME)?.GetRecordingDateInISO8601());
			AssertNotEquals("The record should be ignored because the record in the DB is not that old(24 h)", ignoredPcWithDotNewRecord.GetRecordingDateInISO8601(), dbHandler.GetDotNetVersionRecord(PC_NAME)?.GetRecordingDateInISO8601());

			var newPcWithDotNewRecord = new PcWithDotNetVersionRecord(PC_NAME, new DotNetVersion("4.6", 123), initialRecordingDate.AddHours(25));
			recorder.RecordDotNetVersion(newPcWithDotNewRecord);
			AssertEquals("The record should be not ignored because the record in the DB is older than 24 h", newPcWithDotNewRecord.GetRecordingDateInISO8601(), dbHandler.GetDotNetVersionRecord(PC_NAME)?.GetRecordingDateInISO8601());
			AssertNotEquals("The record should be ignored because the record in the DB is not that old(24 h)", pcWithDotNewRecord.GetRecordingDateInISO8601(), dbHandler.GetDotNetVersionRecord(PC_NAME)?.GetRecordingDateInISO8601());
		}

		public void TestRecordDotNetRuntimes()
		{
			const string PC_NAME = "HOST.DOMAIN";
			var dbInternals = (IDbConnectionInternals)Db.Connection;
			var sqlConnection = (SqlConnection)dbInternals.InternalDbConnection;
			var sqlTransaction = (SqlTransaction)dbInternals.InternalDbTransaction;
			var dbHandler = new DbHandlerForDotNet(new UpgradeSqlContext(sqlConnection, sqlConnection.DataSource, sqlConnection.Database, sqlTransaction));
			var recorder = new DotNetRecorder(dbHandler);
			var initialRecordingDate = DateTime.UtcNow;
			var runtimes =  new[]
			{
				new DotNetCoreRuntime(new Version("6.0.31"), DotNetCoreRuntimeType.AspNetCoreRuntime),
				new DotNetCoreRuntime(new Version("8.0.6"), DotNetCoreRuntimeType.AspNetCoreRuntime),
				new DotNetCoreRuntime(new Version("6.0.31"), DotNetCoreRuntimeType.DotNetRuntime),
				new DotNetCoreRuntime(new Version("8.0.6"), DotNetCoreRuntimeType.DotNetRuntime),
				new DotNetCoreRuntime(new Version("6.0.31"), DotNetCoreRuntimeType.DotNetDesktopRuntime),
				new DotNetCoreRuntime(new Version("8.0.6"), DotNetCoreRuntimeType.DotNetDesktopRuntime),
			};

			dbHandler.DeleteDotNetCoreRuntimesRecord(PC_NAME);
			AssertNull("There must be no records of the test PC", dbHandler.GetDotNetCoreRuntimesRecord(PC_NAME));

			var pcWithDotNewRecord = new PcWithDotNetCoreRuntimesRecord(PC_NAME, runtimes, true, new Version("1.2.3"), initialRecordingDate);
			recorder.RecordDotNetCoreRuntimes(pcWithDotNewRecord);
			AssertEquals("This .net record should be saved in the DB", pcWithDotNewRecord.GetRecordingDateInISO8601(), dbHandler.GetDotNetCoreRuntimesRecord(PC_NAME)?.GetRecordingDateInISO8601());

			var ignoredPcWithDotNewRecord = new PcWithDotNetCoreRuntimesRecord(PC_NAME, runtimes, true, new Version("1.2.3"), initialRecordingDate.AddHours(1));
			recorder.RecordDotNetCoreRuntimes(ignoredPcWithDotNewRecord);
			AssertEquals("The record should be ignored because the record in the DB is not that old(24 h)", pcWithDotNewRecord.GetRecordingDateInISO8601(), dbHandler.GetDotNetCoreRuntimesRecord(PC_NAME)?.GetRecordingDateInISO8601());
			AssertNotEquals("The record should be ignored because the record in the DB is not that old(24 h)", ignoredPcWithDotNewRecord.GetRecordingDateInISO8601(), dbHandler.GetDotNetCoreRuntimesRecord(PC_NAME)?.GetRecordingDateInISO8601());

			var newPcWithDotNewRecord = new PcWithDotNetCoreRuntimesRecord(PC_NAME, runtimes, true, new Version("1.2.3"), initialRecordingDate.AddHours(25));
			recorder.RecordDotNetCoreRuntimes(newPcWithDotNewRecord);
			AssertEquals("The record should be not ignored because the record in the DB is older than 24 h", newPcWithDotNewRecord.GetRecordingDateInISO8601(), dbHandler.GetDotNetCoreRuntimesRecord(PC_NAME)?.GetRecordingDateInISO8601());
			AssertNotEquals("The record should be ignored because the record in the DB is not that old(24 h)", pcWithDotNewRecord.GetRecordingDateInISO8601(), dbHandler.GetDotNetCoreRuntimesRecord(PC_NAME)?.GetRecordingDateInISO8601());
		}

		[TestDate(2016, 11, 11, 11, 11, 11)]
		public void TestRecordDotNetVersionAndRuntimes()
		{
			var dbInternals = (IDbConnectionInternals)Db.Connection;
			var sqlConnection = (SqlConnection)dbInternals.InternalDbConnection;
			var sqlTransaction = (SqlTransaction)dbInternals.InternalDbTransaction;
			var dbHandler = new DbHandlerForDotNet(new UpgradeSqlContext(sqlConnection, sqlConnection.DataSource, sqlConnection.Database, sqlTransaction));
			var recorder = new DotNetRecorder(dbHandler);

			var fullyQualifiedComputerName = System.Net.Dns.GetHostEntry("")?.HostName;
			AssertNotNull(fullyQualifiedComputerName);

			dbHandler.DeleteDotNetVersionRecord(fullyQualifiedComputerName);
			dbHandler.DeleteDotNetCoreRuntimesRecord(fullyQualifiedComputerName);
			AssertNull("There must be no records of the test PC", dbHandler.GetDotNetVersionRecord(fullyQualifiedComputerName));
			AssertNull("There must be no records of the test PC", dbHandler.GetDotNetCoreRuntimesRecord(fullyQualifiedComputerName));

			var retriever = new ClientDotNetRetriever();
			var expectedVersionRecord = new PcWithDotNetVersionRecord(fullyQualifiedComputerName, retriever.GetDotNetVersion(), new DateTime(2024, 1, 2, 3, 4, 5));
			var expectedRuntimesRecord = new PcWithDotNetCoreRuntimesRecord(fullyQualifiedComputerName, retriever.GetDotNetCoreRuntimes(), ClientDotNetRetriever.IsIISInstalled(), ClientDotNetRetriever.GetIISAspNetCoreModuleVersion(), new DateTime(2024, 1, 2, 3, 4, 5));
			recorder.RecordDotNetVersionAndRuntimes();

			var actualVersionRecord = dbHandler.GetDotNetVersionRecord(fullyQualifiedComputerName);
			var actualRuntimesRecord = dbHandler.GetDotNetCoreRuntimesRecord(fullyQualifiedComputerName);

			var versionRecordWithAdjustedDate = new PcWithDotNetVersionRecord(fullyQualifiedComputerName, actualVersionRecord.DotNetVersion, new DateTime(2024, 1, 2, 3, 4, 5));
			var runtimesRecordWithAdjustedDate = new PcWithDotNetCoreRuntimesRecord(fullyQualifiedComputerName, actualRuntimesRecord.DotNetCoreRuntimes.ToArray(), actualRuntimesRecord.IsIISInstalled, actualRuntimesRecord.IISAspNetCoreModuleVersion, new DateTime(2024, 1, 2, 3, 4, 5));

			AssertEquals("This .net version record should be saved in the DB", expectedVersionRecord.ToString(), versionRecordWithAdjustedDate.ToString());
			AssertEquals("This .net runtimes record should be saved in the DB", expectedRuntimesRecord.ToString(), runtimesRecordWithAdjustedDate.ToString());
		}
	}
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using Enterprise.Upgrades;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Startup.BeforeAndAfter.DotNetPrerequisiteCheck.Testing
{
	sealed class DotNetCorePrerequisiteCheckerTest : TransactionedTestCase
	{
		public void TestCheckDotNetCoreVersions_NoPCVersionProblems()
		{
			// Setup
			var dataProviderNoPcs = DataProviderForTestFactory.GetProviderWithoutPCVersionProblems();
			var dotNetCorePrerequisiteViolatingPCsProvider = new DotNetCorePrerequisiteViolatingPCsProvider();

			// Test / Assert
			var dotNetCorePrerequisiteChecker = new DotNetCorePrerequisiteChecker(dotNetCorePrerequisiteViolatingPCsProvider, dataProviderNoPcs);
			AssertNoExceptionThrown("No exception should be thrown because there were no PCs with errors", () => { dotNetCorePrerequisiteChecker.CheckDotNetCoreRuntimes(Db.Connection); });
		}

		public void TestCheckDotNetCoreVersions_HasPCVersionProblems_HappyPath()
		{
			// Setup
			var dataProviderHasPcs = DataProviderForTestFactory.GetProviderWithPCVersionProblems();
			var dotNetCorePrerequisiteViolatingPCsProvider = new DotNetCorePrerequisiteViolatingPCsProvider();
			var dotNetCorePrerequisiteChecker = new DotNetCorePrerequisiteChecker(dotNetCorePrerequisiteViolatingPCsProvider, dataProviderHasPcs);

			var nowFormatedString = ((DotNetCorePrerequisiteDataProviderForTest)dataProviderHasPcs).FormatedNowDateTime;

			// Test // Assert
			var expectedExceptionMessage =
				$"""
				The following PCs in your environment do not have the required versions of the .NET runtimes:
				Computer [MYPC.DOMAIN.COM] is missing the following runtimes (last refreshed {nowFormatedString}):
					Missing .NET Desktop Runtime 8.0
				Computer [MYPC2.DOMAIN2.COM] is missing the following runtimes (last refreshed {nowFormatedString}):
					Missing ASP.NET Core Runtime (Hosting Bundle) 8.0

				Please update these computers and run the current version of CargoWise on each of these computers so that the system can record the new .NET runtimes before starting this upgrade.

				For more details please see the update note:
					- https://wisetechacademy.com/search?quickstart=6edd4257-dd13-4c51-b677-8c8d195599e4

				The runtimes can be downloaded directly from Microsoft from:
					- https://dotnet.microsoft.com/en-us/download/dotnet/8.0
				""";

			var exception = AssertExceptionThrown<PlatformNotSupportedException>(() => { dotNetCorePrerequisiteChecker.CheckDotNetCoreRuntimes(Db.Connection); });
			AssertContains(expectedExceptionMessage, exception.Message);
		}

		public void TestCheckDotNetCoreVersions_HasPCVersion_ProblemsWithWhiteList()
		{
			// Setup
			var dataProviderHasPcs = DataProviderForTestFactory.GetProviderWithPCVersionProblems();
			var dotNetCorePrerequisiteViolatingPCsProvider = new DotNetCorePrerequisiteViolatingPCsProvider();

			DataRegistry.Instance.DotNetPreUpgradeCheckWhitelist = "localhost,MYPC.DOMAIN.COM";     // localhost isn't on the list but

			var nowFormatedString = ((DotNetCorePrerequisiteDataProviderForTest)dataProviderHasPcs).FormatedNowDateTime;

			// Test / Assert
			var dotNetCorePrerequisiteChecker = new DotNetCorePrerequisiteChecker(dotNetCorePrerequisiteViolatingPCsProvider, dataProviderHasPcs);

			var expectedExceptionMessage =
				$"""
				The following PCs in your environment do not have the required versions of the .NET runtimes:
				Computer [MYPC2.DOMAIN2.COM] is missing the following runtimes (last refreshed {nowFormatedString}):
					Missing ASP.NET Core Runtime (Hosting Bundle) 8.0

				Please update these computers and run the current version of CargoWise on each of these computers so that the system can record the new .NET runtimes before starting this upgrade.

				For more details please see the update note:
					- https://wisetechacademy.com/search?quickstart=6edd4257-dd13-4c51-b677-8c8d195599e4

				The runtimes can be downloaded directly from Microsoft from:
					- https://dotnet.microsoft.com/en-us/download/dotnet/8.0
				""";

			AssertExceptionThrown("Exception should be thrown because there were PCs with errors", typeof(PlatformNotSupportedException), expectedExceptionMessage, () => { dotNetCorePrerequisiteChecker.CheckDotNetCoreRuntimes(Db.Connection); });
		}

		public void TestCheckDotNetCoreVersions_HasPCVersionProblems_WithWhiteListJustMachineName()
		{
			// Setup
			var dataProviderHasPcs = DataProviderForTestFactory.GetProviderWithPCVersionProblems();
			var dotNetCorePrerequisiteViolatingPCsProvider = new DotNetCorePrerequisiteViolatingPCsProvider();

			DataRegistry.Instance.DotNetPreUpgradeCheckWhitelist = "MYPC";  // shouldn't match on only subsets

			var nowFormatedString = ((DotNetCorePrerequisiteDataProviderForTest)dataProviderHasPcs).FormatedNowDateTime;
			
			// Test / Assert
			var dotNetCorePrerequisiteChecker = new DotNetCorePrerequisiteChecker(dotNetCorePrerequisiteViolatingPCsProvider, dataProviderHasPcs);

			var expectedExceptionMessage =
				$"""
				The following PCs in your environment do not have the required versions of the .NET runtimes:
				Computer [MYPC.DOMAIN.COM] is missing the following runtimes (last refreshed {nowFormatedString}):
					Missing .NET Desktop Runtime 8.0
				Computer [MYPC2.DOMAIN2.COM] is missing the following runtimes (last refreshed {nowFormatedString}):
					Missing ASP.NET Core Runtime (Hosting Bundle) 8.0

				Please update these computers and run the current version of CargoWise on each of these computers so that the system can record the new .NET runtimes before starting this upgrade.

				For more details please see the update note:
					- https://wisetechacademy.com/search?quickstart=6edd4257-dd13-4c51-b677-8c8d195599e4

				The runtimes can be downloaded directly from Microsoft from:
					- https://dotnet.microsoft.com/en-us/download/dotnet/8.0
				""";

			AssertExceptionThrown("Exception should be thrown because there were PCs with errors", typeof(PlatformNotSupportedException), expectedExceptionMessage, () => { dotNetCorePrerequisiteChecker.CheckDotNetCoreRuntimes(Db.Connection); });
		}

		public void TestCheckDotNetCoreVersions_HasPCVersionProblems_WithWhiteListCaseInsensitiveAndTrimAndBlank()
		{
			// Setup
			var dataProviderHasPcs = DataProviderForTestFactory.GetProviderWithPCVersionProblems();
			var dotNetCorePrerequisiteViolatingPCsProvider = new DotNetCorePrerequisiteViolatingPCsProvider();

			DataRegistry.Instance.DotNetPreUpgradeCheckWhitelist = "somedomain, ,   mypc.DomAin.COm  ,,";  // lots of messy whitespace and case insensitivity

			var nowFormatedString = ((DotNetCorePrerequisiteDataProviderForTest)dataProviderHasPcs).FormatedNowDateTime;
			
			// Test / Assert
			var dotNetCorePrerequisiteChecker = new DotNetCorePrerequisiteChecker(dotNetCorePrerequisiteViolatingPCsProvider, dataProviderHasPcs);

			var expectedExceptionMessage =
				$"""
				The following PCs in your environment do not have the required versions of the .NET runtimes:
				Computer [MYPC2.DOMAIN2.COM] is missing the following runtimes (last refreshed {nowFormatedString}):
					Missing ASP.NET Core Runtime (Hosting Bundle) 8.0

				Please update these computers and run the current version of CargoWise on each of these computers so that the system can record the new .NET runtimes before starting this upgrade.

				For more details please see the update note:
					- https://wisetechacademy.com/search?quickstart=6edd4257-dd13-4c51-b677-8c8d195599e4

				The runtimes can be downloaded directly from Microsoft from:
					- https://dotnet.microsoft.com/en-us/download/dotnet/8.0
				""";

			AssertExceptionThrown("Exception should be thrown because there were PCs with errors", typeof(PlatformNotSupportedException), expectedExceptionMessage, () => { dotNetCorePrerequisiteChecker.CheckDotNetCoreRuntimes(Db.Connection); });
		}

		public void TestCheckDotNetCoreVersions_HasPCVersionProblems_WithWhiteListWithBadValues()
		{
			// Setup
			var dataProviderHasPcs = DataProviderForTestFactory.GetProviderWithPCVersionProblems();
			var dotNetCorePrerequisiteViolatingPCsProvider = new DotNetCorePrerequisiteViolatingPCsProvider();

			DataRegistry.Instance.DotNetPreUpgradeCheckWhitelist = "MYPC  ,  &*sk2, alla, domain.com, ,,\\,mypc,DomAin.COm, mypc-DomAin.COm,,*,%";  // lots of test bad inputs including common wildcards, nothing should match

			var nowFormatedString = ((DotNetCorePrerequisiteDataProviderForTest)dataProviderHasPcs).FormatedNowDateTime;

			// Test / Assert
			var dotNetCorePrerequisiteChecker = new DotNetCorePrerequisiteChecker(dotNetCorePrerequisiteViolatingPCsProvider, dataProviderHasPcs);

			var expectedExceptionMessage =
				$"""
				The following PCs in your environment do not have the required versions of the .NET runtimes:
				Computer [MYPC.DOMAIN.COM] is missing the following runtimes (last refreshed {nowFormatedString}):
					Missing .NET Desktop Runtime 8.0
				Computer [MYPC2.DOMAIN2.COM] is missing the following runtimes (last refreshed {nowFormatedString}):
					Missing ASP.NET Core Runtime (Hosting Bundle) 8.0

				Please update these computers and run the current version of CargoWise on each of these computers so that the system can record the new .NET runtimes before starting this upgrade.

				For more details please see the update note:
					- https://wisetechacademy.com/search?quickstart=6edd4257-dd13-4c51-b677-8c8d195599e4

				The runtimes can be downloaded directly from Microsoft from:
					- https://dotnet.microsoft.com/en-us/download/dotnet/8.0
				""";

			AssertExceptionThrown("Exception should be thrown because there were PCs with errors", typeof(PlatformNotSupportedException), expectedExceptionMessage, () => { dotNetCorePrerequisiteChecker.CheckDotNetCoreRuntimes(Db.Connection); });
		}

		public void TestCheckDotNetCoreVersions_HasPCVersionProblemsWithDisableCheck()
		{
			// This flag is done higher in the stack, we do NOT expect it to be done at this level unlike the whitelist. Not sure if this is a good test or not, but making sure it remains working as expected as it is a bit inconsistent with the whitelist setting.

			// Setup
			DataRegistry.Instance.EnableDotNetPreUpgradeCheck = false;

			// Test / Assert
			TestCheckDotNetCoreVersions_HasPCVersionProblems_HappyPath();
		}

		public void TestOldDotNetCoreVersionsRecordsRemoved()
		{
			// Setup
			var dataProvider = new DotNetCorePrerequisiteDataProvider();
			var dotNetCorePrerequisiteViolatingPCsProvider = new DotNetCorePrerequisiteViolatingPCsProvider();

			const string nameRecordWith25Days = PcWithDotNetCoreRuntimesRecord.RegistryPrefix + "|SYDCO-WTES-25.wtg.zone";
			const string nameRecordWith20Days = PcWithDotNetCoreRuntimesRecord.RegistryPrefix + "|SYDCO-WTES-20.wtg.zone";
			const string nameRecordWith16Days = PcWithDotNetCoreRuntimesRecord.RegistryPrefix + "|SYDCO-WTES-16.wtg.zone";

			AddDotNetCoreVersionRecord(nameRecordWith25Days, 25);
			AddDotNetCoreVersionRecord(nameRecordWith20Days, 20);
			AddDotNetCoreVersionRecord(nameRecordWith16Days, 16);
			AssertEquals(true, DotNetCoreVersionRecordExists(nameRecordWith25Days));
			AssertEquals(true, DotNetCoreVersionRecordExists(nameRecordWith20Days));
			AssertEquals(true, DotNetCoreVersionRecordExists(nameRecordWith16Days));

			var dotNetCorePrerequisiteChecker = new DotNetCorePrerequisiteChecker(dotNetCorePrerequisiteViolatingPCsProvider, dataProvider);
			dotNetCorePrerequisiteChecker.CheckDotNetCoreRuntimes(Db.Connection);

			AssertEquals(false, DotNetCoreVersionRecordExists(nameRecordWith25Days));
			AssertEquals(false, DotNetCoreVersionRecordExists(nameRecordWith20Days));
			AssertEquals(false, DotNetCoreVersionRecordExists(nameRecordWith16Days));
		}

		public void TestCurrentDotNetCoreVersionsRecordsNotRemoved()
		{
			// Setup
			var dataProvider = new DotNetCorePrerequisiteDataProvider();
			var dotNetCorePrerequisiteViolatingPCsProvider = new DotNetCorePrerequisiteViolatingPCsProvider();

			const string nameRecordWith5Days = PcWithDotNetCoreRuntimesRecord.RegistryPrefix + "|SYDCO-WTES-5.wtg.zone";
			const string nameRecordWith10Days = PcWithDotNetCoreRuntimesRecord.RegistryPrefix + "|SYDCO-WTES-10.wtg.zone";
			const string nameRecordWith15Days = PcWithDotNetCoreRuntimesRecord.RegistryPrefix + "|SYDCO-WTES-15.wtg.zone";

			AddDotNetCoreVersionRecord(nameRecordWith5Days, 5);
			AddDotNetCoreVersionRecord(nameRecordWith10Days, 10);
			AddDotNetCoreVersionRecord(nameRecordWith15Days, 15);
			AssertEquals(true, DotNetCoreVersionRecordExists(nameRecordWith5Days));
			AssertEquals(true, DotNetCoreVersionRecordExists(nameRecordWith10Days));
			AssertEquals(true, DotNetCoreVersionRecordExists(nameRecordWith15Days));

			var dotNetCorePrerequisiteChecker = new DotNetCorePrerequisiteChecker(dotNetCorePrerequisiteViolatingPCsProvider, dataProvider);
			dotNetCorePrerequisiteChecker.CheckDotNetCoreRuntimes(Db.Connection);

			AssertEquals(true, DotNetCoreVersionRecordExists(nameRecordWith5Days));
			AssertEquals(true, DotNetCoreVersionRecordExists(nameRecordWith10Days));
			AssertEquals(true, DotNetCoreVersionRecordExists(nameRecordWith15Days));
		}

		[ExpectNoExceptions]
		public void TestIgnoresStaleDataForCurrentMachine()
		{
			var recordForCurrentMachine = DotNetRecorder.GetRuntimeRecordForCurrentComputer();
			var staleRecordForCurrentMachine = new PcWithDotNetCoreRuntimesRecord(recordForCurrentMachine.FullyQualifiedName, "DotNetCoreRuntimes:DotNetRuntime-7.0.3,DotNetDesktopRuntime-7.0.6,AspNetCoreRuntime-7.0.3|IISAspNetCoreModuleVersion:|RecordingDate:" + DateTime.UtcNow.Date.AddDays(-1).ToString("s"));
			SaveRecord(staleRecordForCurrentMachine);

			var dotNetCorePrerequisiteChecker = new DotNetCorePrerequisiteChecker(new DotNetCorePrerequisiteViolatingPCsProvider(), new DotNetCorePrerequisiteDataProvider());

			// Expects the stale record has been updated.
			dotNetCorePrerequisiteChecker.CheckDotNetCoreRuntimes(Db.Connection);
		}

		void AddDotNetCoreVersionRecord(string sdName, int numberOfDays)
		{
			var pcName = sdName.Split('|')[1];
			var dotNetCoreRuntimeRecord = new PcWithDotNetCoreRuntimesRecord(pcName, "DotNetCoreRuntimes:DotNetRuntime-8.0.3,DotNetDesktopRuntime-8.0.6,AspNetCoreRuntime-8.0.3|IIS:Installed|IISAspNetCoreModuleVersion:13.0.19218.0|RecordingDate:" + DateTime.UtcNow.Date.AddDays(numberOfDays * -1).ToString("s"));
			SaveRecord(dotNetCoreRuntimeRecord);
		}

		void SaveRecord(PcWithDotNetCoreRuntimesRecord dotCoreRuntimeRecord)
		{
			var sqlConnection = ((IDbConnectionInternals)Db.Connection).ADOConnection;
			var sqlTransaction = ((IDbConnectionInternals)Db.Connection).ADOTransaction;
			var dbHandler = new DbHandlerForDotNet(new UpgradeSqlContext(sqlConnection, sqlConnection.DataSource, sqlConnection.Database, sqlTransaction));
			dbHandler.SaveDotNetRecord(dotCoreRuntimeRecord);
		}

		bool DotNetCoreVersionRecordExists(string name)
		{
			using (var cmd = Db.Connection.Command("select count(*) from dbo.StmData where SD_NAME = @Name"))
			{
				cmd.AddParameterBasedOnDbColumn("@Name", name, StmDataSchema.SD_Name);
				return (int)cmd.ExecuteScalar() > 0;
			}
		}

		static class DataProviderForTestFactory
		{
			public static IDotNetPrerequisiteDataProvider GetProviderWithoutPCVersionProblems()
			{
				return new DotNetCorePrerequisiteDataProviderForTest(true);
			}

			public static IDotNetPrerequisiteDataProvider GetProviderWithPCVersionProblems()
			{
				return new DotNetCorePrerequisiteDataProviderForTest(false);
			}
		}

		sealed class DotNetCorePrerequisiteDataProviderForTest : IDotNetPrerequisiteDataProvider
		{
			readonly DataTable data;

			public DateTime NowDateTime => DateTime.UtcNow;

#pragma warning disable CW1103  // No access to ZDateTime.ShortDateFormat at this stage.
			public string FormatedNowDateTime => NowDateTime.ToString("dd-MMM-yy", CultureInfo.InvariantCulture);
#pragma warning restore CW1103
	
			public string Prefix
			{
				get
				{
					return PcWithDotNetCoreRuntimesRecord.RegistryPrefix;
				}
			}

			public DotNetCorePrerequisiteDataProviderForTest(bool noPCsInList)
			{
				data = new DataTable();
				data.Columns.Add("SD_Name", typeof(string));
				data.Columns.Add("value", typeof(string));  // SD_BinaryValue
				data.Columns.Add("LastUpdated", typeof(DateTime));

				if (noPCsInList)
				{
					return;
				}

				var records = new List<PcWithDotNetCoreRuntimesRecord>
				{
					new("MYPC.DOMAIN.COM", [
						new DotNetCoreRuntime(new Version("8.0.0"), DotNetCoreRuntimeType.DotNetRuntime),
						new DotNetCoreRuntime(new Version("6.0.0"), DotNetCoreRuntimeType.DotNetDesktopRuntime),
						new DotNetCoreRuntime(new Version("8.0.0"), DotNetCoreRuntimeType.AspNetCoreRuntime)
					], false, new Version("13.0.19218.0"), NowDateTime),
					new("MYPC2.DOMAIN2.COM", [
						new DotNetCoreRuntime(new Version("8.0.0"), DotNetCoreRuntimeType.DotNetRuntime),
						new DotNetCoreRuntime(new Version("8.0.0"), DotNetCoreRuntimeType.DotNetDesktopRuntime),
						new DotNetCoreRuntime(new Version("6.0.0"), DotNetCoreRuntimeType.AspNetCoreRuntime)
					], true, new Version("13.0.19218.0"), NowDateTime)
				};

				foreach (var rec in records)
				{
					data.Rows.Add(
						Prefix + "|" + rec.FullyQualifiedName,
						rec.ToDbValue() + "|RecordingDate:" + rec.RecordingDate.ToString("s", CultureInfo.InvariantCulture),
						rec.RecordingDate
					);
				}
			}

			public DataTable GetVersionsRecords(DbConnection connection)
			{
				return data;
			}

			public void RemoveOldVersionsRecords()
			{
				if (data == null || data.Rows.Count == 0)
				{
					return;
				}

				var rowsToRemove = (from DataRow row in data.Rows
								   where ((string)row["SD_Name"]).Contains(Prefix) && (DateTime)row["LastUpdated"] < DateTime.UtcNow.AddDays(-15)
								   select row).ToArray();

				foreach (var row in rowsToRemove)
				{
					data.Rows.Remove(row);
				}
			}
		}
	}
}

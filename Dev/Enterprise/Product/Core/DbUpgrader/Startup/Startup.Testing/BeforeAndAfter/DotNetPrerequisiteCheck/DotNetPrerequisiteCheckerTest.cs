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
	sealed class DotNetPrerequisiteCheckerTest : TransactionedTestCase
	{
		public void TestCheckDotNetVersions_NoPCVersionProblems()
		{
			// Setup
			var dataProviderNoPcs = DataProviderForTestFactory.GetProviderWithoutPCVersionProblems();
			var minimumDotNetVersionDefinition = new MinimumDotNetVersionDefinition();
			var dotNetPrerequisiteViolatingPCsProvider = new DotNetPrerequisiteViolatingPCsProvider(minimumDotNetVersionDefinition);

			// Test / Assert
			var dotNetPrerequisiteChecker = new DotNetPrerequisiteChecker(dotNetPrerequisiteViolatingPCsProvider, minimumDotNetVersionDefinition, dataProviderNoPcs);
			AssertNoExceptionThrown("No exception should be thrown because there were no PCs with errors", () => { dotNetPrerequisiteChecker.CheckDotNetVersions(Db.Connection); });
		}

		public void TestCheckDotNetVersions_HasPCVersionProblems()
		{
			// Setup
			var dataProviderWithPcs = DataProviderForTestFactory.GetProviderWithPCVersionProblems();
			var minimumDotNetVersionDefinition = new MinimumDotNetVersionDefinition();
			var dotNetPrerequisiteViolatingPCsProvider = new DotNetPrerequisiteViolatingPCsProvider(minimumDotNetVersionDefinition);

			// Expect PCs with .Net version problems
			var dotNetPrerequisiteChecker = new DotNetPrerequisiteChecker(dotNetPrerequisiteViolatingPCsProvider, minimumDotNetVersionDefinition, dataProviderWithPcs);

			var expectedExceptionMessage =
				"""
				The following PCs in your environment do not have the required version of the .NET Framework (v4.8):
				Computer [MYPC.DOMAIN.COM] has .NET Framework v4.3.1
				Computer [MYPC2.DOMAIN2.COM] has .NET Framework v4.5
				Please update these computers and run the current version of CargoWise on each of these computers so that the system can record the new .NET framework version before starting this upgrade.
				""";

			AssertExceptionThrown("Exception should be thrown because there were PCs with errors", typeof(PlatformNotSupportedException), expectedExceptionMessage, () => { dotNetPrerequisiteChecker.CheckDotNetVersions(Db.Connection); });
		}

		public void TestCheckDotNetVersions_HasPCVersionProblemsWithWhiteList()
		{
			// Setup
			var dataProviderWithPcs = DataProviderForTestFactory.GetProviderWithPCVersionProblems();
			var minimumDotNetVersionDefinition = new MinimumDotNetVersionDefinition();
			var dotNetPrerequisiteViolatingPCsProvider = new DotNetPrerequisiteViolatingPCsProvider(minimumDotNetVersionDefinition);

			DataRegistry.Instance.DotNetPreUpgradeCheckWhitelist = "localhost,MYPC.DOMAIN.COM";

			// Expect a single PCs with .Net version problems
			var dotNetPrerequisiteChecker = new DotNetPrerequisiteChecker(dotNetPrerequisiteViolatingPCsProvider, minimumDotNetVersionDefinition, dataProviderWithPcs);

			var expectedExceptionMessage =
				"""
				The following PCs in your environment do not have the required version of the .NET Framework (v4.8):
				Computer [MYPC2.DOMAIN2.COM] has .NET Framework v4.5
				Please update these computers and run the current version of CargoWise on each of these computers so that the system can record the new .NET framework version before starting this upgrade.
				""";
			AssertExceptionThrown("Exception should be thrown because there were PCs with errors", typeof(PlatformNotSupportedException), expectedExceptionMessage, () => { dotNetPrerequisiteChecker.CheckDotNetVersions(Db.Connection); });
		}

		public void TestCheckDotNetVersions_HasPCVersionProblemsWithDisableCheck()
		{
			// This flag is done higher in the stack, we do NOT expect it to be done at this level unlike the whitelist. Not sure if this is a good test or not, but making sure it remains working as expected as it is a bit inconsistent with the whitelist setting.
			// Setup
			DataRegistry.Instance.EnableDotNetPreUpgradeCheck = false;

			// Expect no PCs with .Net version problems because we disabled the check
			TestCheckDotNetVersions_HasPCVersionProblems();
		}

		public void TestOldDotNetVersionsRecordsRemoved()
		{
			// Setup
			var dataProvider = new DotNetPrerequisiteDataProvider();
			var minimumDotNetVersionDefinition = new MinimumDotNetVersionDefinition();
			var dotNetPrerequisiteViolatingPCsProvider = new DotNetPrerequisiteViolatingPCsProvider(minimumDotNetVersionDefinition);

			const string nameRecordWith25Days = "DOTNET_VERSION|SYDCO-WTES-25.wtg.zone";
			const string nameRecordWith20Days = "DOTNET_VERSION|SYDCO-WTES-20.wtg.zone";
			const string nameRecordWith16Days = "DOTNET_VERSION|SYDCO-WTES-16.wtg.zone";

			AddDotNetVersionRecord(nameRecordWith25Days, 25);
			AddDotNetVersionRecord(nameRecordWith20Days, 20);
			AddDotNetVersionRecord(nameRecordWith16Days, 16);

			var dotNetPrerequisiteChecker = new DotNetPrerequisiteChecker(dotNetPrerequisiteViolatingPCsProvider, minimumDotNetVersionDefinition, dataProvider);
			dotNetPrerequisiteChecker.CheckDotNetVersions(Db.Connection);

			AssertEquals(false, DotNetVersionRecordExists(nameRecordWith25Days));
			AssertEquals(false, DotNetVersionRecordExists(nameRecordWith20Days));
			AssertEquals(false, DotNetVersionRecordExists(nameRecordWith16Days));
		}

		public void TestCurrentDotNetVersionsRecordsNotRemoved()
		{
			// Setup
			var dataProvider = new DotNetPrerequisiteDataProvider();
			var minimumDotNetVersionDefinition = new MinimumDotNetVersionDefinitionForTest();   // This is a test class that always returns true for IsDotNetSupportedVersion
			var dotNetPrerequisiteViolatingPCsProvider = new DotNetPrerequisiteViolatingPCsProvider(minimumDotNetVersionDefinition);

			const string nameRecordWith5Days = "DOTNET_VERSION|SYDCO-WTES-5.wtg.zone";
			const string nameRecordWith10Days = "DOTNET_VERSION|SYDCO-WTES-10.wtg.zone";
			const string nameRecordWith15Days = "DOTNET_VERSION|SYDCO-WTES-15.wtg.zone";

			AddDotNetVersionRecord(nameRecordWith5Days, 5);
			AddDotNetVersionRecord(nameRecordWith10Days, 10);
			AddDotNetVersionRecord(nameRecordWith15Days, 15);

			var dotNetPrerequisiteChecker = new DotNetPrerequisiteChecker(dotNetPrerequisiteViolatingPCsProvider, minimumDotNetVersionDefinition, dataProvider);
			dotNetPrerequisiteChecker.CheckDotNetVersions(Db.Connection);

			AssertEquals(true, DotNetVersionRecordExists(nameRecordWith5Days));
			AssertEquals(true, DotNetVersionRecordExists(nameRecordWith10Days));
			AssertEquals(true, DotNetVersionRecordExists(nameRecordWith15Days));
		}

		[ExpectNoExceptions]
		public void TestIgnoresStaleDataForCurrentMachine()
		{
			// Setup
			var dataProvider = new DotNetPrerequisiteDataProvider();
			var minimumDotNetVersionDefinition = new MinimumDotNetVersionDefinition();
			var dotNetPrerequisiteViolatingPCsProvider = new DotNetPrerequisiteViolatingPCsProvider(minimumDotNetVersionDefinition);

			var recordForCurrentMachine = DotNetRecorder.GetVersionRecordForCurrentComputer();
			var staleRecordForCurrentMachine = new PcWithDotNetVersionRecord(recordForCurrentMachine.FullyQualifiedName, new DotNetVersion("4.5", 378389), DateTime.UtcNow.AddDays(-1));
			SaveRecord(staleRecordForCurrentMachine);

			var dotNetPrerequisiteChecker = new DotNetPrerequisiteChecker(dotNetPrerequisiteViolatingPCsProvider, minimumDotNetVersionDefinition, dataProvider);

			dotNetPrerequisiteChecker.CheckDotNetVersions(Db.Connection);
		}

		void AddDotNetVersionRecord(string name, int numberOfDays)
		{
			var dotNetVersionRecord = new PcWithDotNetVersionRecord(new string[] { name, "DotNetVersion:4.6.2|ReleaseNumber:394802|RecordingDate:" + DateTime.UtcNow.Date.AddDays(numberOfDays * -1).ToString("s") });
			SaveRecord(dotNetVersionRecord);
		}

		void SaveRecord(PcWithDotNetVersionRecord dotNetVersionRecord)
		{
			var sqlConnection = ((IDbConnectionInternals)Db.Connection).ADOConnection;
			var sqlTransaction = ((IDbConnectionInternals)Db.Connection).ADOTransaction;
			var dbHandler = new DbHandlerForDotNet(new UpgradeSqlContext(sqlConnection, sqlConnection.DataSource, sqlConnection.Database, sqlTransaction));
			dbHandler.SaveDotNetRecord(dotNetVersionRecord);
		}

		bool DotNetVersionRecordExists(string name)
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
				return new DotNetPrerequisiteDataProviderForTest(true);
			}

			public static IDotNetPrerequisiteDataProvider GetProviderWithPCVersionProblems()
			{
				return new DotNetPrerequisiteDataProviderForTest(false);
			}
		}

		sealed class DotNetPrerequisiteDataProviderForTest : IDotNetPrerequisiteDataProvider
		{
			readonly DataTable data;

			public string Prefix
			{
				get
				{
					return PcWithDotNetVersionRecord.RegistryPrefix;
				}
			}

			readonly IEnumerable<PcWithDotNetVersionRecord> records =
				new List<PcWithDotNetVersionRecord>
				{
					new("MYPC.DOMAIN.COM", new DotNetVersion("4.3.1", 0)),
					new("MYPC2.DOMAIN2.COM", new DotNetVersion("4.5", 23134))
				};

			public DotNetPrerequisiteDataProviderForTest(bool noPCsInList)
			{
				data = new DataTable();
				data.Columns.Add("SD_Name", typeof(string));
				data.Columns.Add("value", typeof(string));  // SD_BinaryValue
				data.Columns.Add("LastUpdated", typeof(DateTime));

				if (!noPCsInList)
				{
					foreach (var rec in records)
					{
						data.Rows.Add(
							Prefix + "|" + rec.FullyQualifiedName,
							$"DotNetVersion:{rec.DotNetVersion.VersionNumber}|ReleaseNumber:{rec.DotNetVersion.ReleaseNumber}|RecordingDate:{rec.RecordingDate.ToString("s", CultureInfo.InvariantCulture)}",
							rec.RecordingDate
						);
					}
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

				var rowsToRemove = from DataRow row in data.Rows
								   where ((string)row["SD_Name"]).Contains(Prefix) && (DateTime)row["LastUpdated"] < DateTime.UtcNow.AddDays(-15)
								   select row;

				foreach (var row in rowsToRemove)
				{
					data.Rows.Remove(row);
				}
			}
		}

		sealed class MinimumDotNetVersionDefinitionForTest : IMinimumDotNetVersionDefinition
		{
			public string MinimumDotNetVersionRequired => "4.0";

			public bool IsDotNetSupportedVersion(DotNetVersion dotNetVersion)
			{
				return true;
			}

			public bool IsAnOldRecord(PcWithDotNetVersionRecord record)
			{
				return false;
			}
		}
	}
}

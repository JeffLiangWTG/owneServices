using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using Dat.Integration.Deployment;
using NUnit.Framework;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Dat.Implementation.Testing
{
	public class TestedShelfDeploymentOptionsTest : TestCase
	{
		public void TestDefaultOptions()
		{
			var options = new TestedShelfDeploymentOptions(new TaskInfo("shelf", "owner", "TestRigRestoreFromBackup: {backupPath}"));
			AssertNotNullOrEmpty(options.SqlServer);
			AssertNotNullOrEmpty(options.SqlServerDataFilePath);
			AssertNotNullOrEmpty(options.SqlServerLogFilePath);
			AssertNotNullOrEmpty(options.RegistrationSqlServer);
			AssertEquals(true, options.CreateIcon);
			AssertEquals("SH0shelf", options.DatabaseName);
			AssertEquals("shelf", options.IconName);
			AssertEquals(0, options.WebSites.Length);
			AssertEquals(null, options.ClientCode);
			AssertEquals(null, options.Origin);
			AssertEquals(options.WebDomain, "shelf.testrig.sand.wtg.zone");
			AssertEquals(false, options.IncludeSystemPackage);
			AssertEquals("WUT", options.RegistrationEnterpriseCode);
			AssertEquals(true, options.Register);
			AssertEquals(true, options.LaunchDbUpgrade);
			AssertEquals("https://shelf.blazor.sand.wtg.zone/backchannel", options.DefaultBlazorUrl);
			AssertEquals(false, options.PermitDuplicateWebSites);
			AssertEquals(false, options.Verbose);
			AssertEquals(false, options.ScheduleUPG);
			AssertEquals("CUR", options.PackageStatus);
			AssertEquals(false, options.DeployWinzor);
			AssertEquals(false, options.RunWinzorE2ETests);
			AssertEquals(TimeSpan.FromMinutes(10), options.VersionBrokerWarmupTimeout);
			AssertEquals(false, options.SetupAlwaysOn);
			AssertEquals(0, options.AddStaffRecords.Length);
		}

		public void TestAnyOptionsFalseWithNoComments()
		{
			var options = new TestedShelfDeploymentOptions(new TaskInfo("shelf", "owner", string.Empty));
			Assert(!options.AnyOptionsSet);
		}

		public void TestAnyOptionsFalseWithNoUsefulComments()
		{
			var options = new TestedShelfDeploymentOptions(new TaskInfo("shelf", "owner", "I don't want a test rig today"));
			Assert(!options.AnyOptionsSet);
		}

		public void TestNoOtherOptionsParsedWhenNoRestoreServerOrDBNameGiven()
		{
			var options = new TestedShelfDeploymentOptions(new TaskInfo("shelf", "owner", string.Empty));
			AssertNull(options.SqlServerDataFilePath);
			AssertNull(options.SqlServerLogFilePath);
			AssertNull(options.IconName);
			AssertNull(options.WebSites);
			AssertNull(options.ClientCode);
			AssertNull(options.WebDomain);
			AssertNull(options.RegistrationEnterpriseCode);
			AssertNull(options.RegistrationSqlServer);
			AssertNull(options.DbUpgradeServer);
			AssertNull(options.PackageStatus);
			AssertNull(options.AddStaffRecords);
		}

		public void TestDefaultServerNeedsDatabaseName()
		{
			AssertExceptionThrown<ArgumentException>(() => TestedShelfDeploymentOptions.GetDefaultServerFromDatabaseName(null));
			AssertExceptionThrown<ArgumentException>(() => TestedShelfDeploymentOptions.GetDefaultServerFromDatabaseName(string.Empty));
		}

		public void TestDefaultServerSpreadOverMultipleServersEvenly()
		{
			AssertRandomBinning(
				trialCount: 100,
				binCount: 4,
				sigmaLimit: 3.0,
				i => TestedShelfDeploymentOptions.GetDefaultServerFromDatabaseName($"dbName{i:D6}"));
		}

		public void TestDefaultWebServerNeedsDatabaseName()
		{
			AssertExceptionThrown<ArgumentException>(() => TestedShelfDeploymentOptions.GetDefaultWebServerFromDatabaseName(null));
			AssertExceptionThrown<ArgumentException>(() => TestedShelfDeploymentOptions.GetDefaultWebServerFromDatabaseName(string.Empty));
			AssertExceptionThrown<ArgumentException>(() => TestedShelfDeploymentOptions.GetDefaultWebRootDomainFromDatabaseName(null));
			AssertExceptionThrown<ArgumentException>(() => TestedShelfDeploymentOptions.GetDefaultWebRootDomainFromDatabaseName(string.Empty));
		}

		public void TestDefaultWebServerSpreadOverMultipleServersEvenly()
		{
			AssertRandomBinning(
				trialCount: 100,
				binCount: 6,
				sigmaLimit: 3.0,
				i =>
				{
					var dbName = $"dbName{i:D6}";
					var serverName = TestedShelfDeploymentOptions.GetDefaultWebServerFromDatabaseName(dbName);
					var domainName = TestedShelfDeploymentOptions.GetDefaultWebRootDomainFromDatabaseName(dbName);
					return $"{serverName}+{domainName}";
				});
		}

		void AssertRandomBinning(uint trialCount, uint binCount, double sigmaLimit, Func<int, string> getKey)
		{
			var p = 1.0 / binCount;
			var np = trialCount * p;
			var variance = np * (1 - p);
			var standardDeviation = Math.Sqrt(variance);

			var observedBinCounts = new Dictionary<string, uint>();
			for (var i = 0; i != trialCount; ++i)
			{
				var key = getKey(i);
				observedBinCounts[key] = observedBinCounts.ContainsKey(key) ? observedBinCounts[key] + 1 : 1;
			}

			foreach (var observedBinCount in observedBinCounts.Keys)
			{
				var sigma = Math.Abs(np - observedBinCounts[observedBinCount]) / standardDeviation;
				AssertLessThan("Expected count of servers should be within a given standard deviations", sigma, sigmaLimit);
			}

			AssertEquals(observedBinCounts.Count, binCount);
		}

		public void TestDefaultWebServerMatchesKnownPairWhenWebDomainSpecified()
		{
			var options = new TestedShelfDeploymentOptions(new TaskInfo("testshelf", "owner", @"TestRigRestoreFromBackup: whatever
TestRigWebDomain: testshelf.testrig.sand.wtg.zone"));
			AssertEquals("Au2sp-tweb-403a.sand.wtg.zone", options.WebServer);
		}
		public void TestDefaultWebDomainMatchesKnownPairWhenServerSpecified()
		{
			var webServers = new string[] {
				"Au2sp-tweb-403a.sand.wtg.zone",
				"Au2sp-tweb-403b.sand.wtg.zone",
				"Au2sp-tweb-404a.sand.wtg.zone",
				"Au2sp-tweb-404b.sand.wtg.zone",
				"Au2sp-tweb-405a.sand.wtg.zone",
				"Au2sp-tweb-405b.sand.wtg.zone"
			};

			foreach (var webServer in webServers)
			{
				var options = new TestedShelfDeploymentOptions(new TaskInfo("testshelf", "owner", $@"TestRigRestoreFromBackup: whatever
TestRigWebServer: {webServer}"));
				AssertEquals("testshelf.testrig.sand.wtg.zone", options.WebDomain);
			}
		}

		public void TestDefaultWebDomainMatchesKnownPairWhenServerSpecifiedIgnoreCase()
		{
			var webServers = new string[] {
				"AU2SP-TWEB-403A.sand.wtg.zone",
				"AU2SP-TWEB-403B.sand.wtg.zone",
				"AU2SP-TWEB-404A.sand.wtg.zone",
				"AU2SP-TWEB-404B.sand.wtg.zone",
				"AU2SP-TWEB-405A.sand.wtg.zone",
				"AU2SP-TWEB-405B.sand.wtg.zone"
			};

			foreach (var webServer in webServers)
			{
				var options = new TestedShelfDeploymentOptions(new TaskInfo("testshelf", "owner", $@"TestRigRestoreFromBackup: whatever
TestRigWebServer: {webServer}"));
				AssertEquals("testshelf.testrig.sand.wtg.zone", options.WebDomain);
			}
		}

		public void TestUnknownServerWithoutDomainWillUserServer()
		{
			var options = new TestedShelfDeploymentOptions(new TaskInfo("testshelf", "owner", @"TestRigRestoreFromBackup: whatever
TestRigWebServer: unknown-server.sand.wtg.zone"));
			AssertEquals("testshelf.unknown-server.sand.wtg.zone", options.WebDomain);

			options = new TestedShelfDeploymentOptions(new TaskInfo("LCDShelfName", "owner", @"TestRigRestoreFromBackup: whatever
TestRigWebServer: SYDCO-WLCD-1.wtg.zone"));
			AssertEquals("lcdshelfname.SYDCO-WLCD-1.wtg.zone", options.WebDomain);
		}

		public void TestUnknownDomainRejectedWithoutServer()
		{
			var exception = AssertExceptionThrown<ArgumentException>(() => new TestedShelfDeploymentOptions(new TaskInfo("testshelf", "owner", @"TestRigRestoreFromBackup: whatever
TestRigWebDomain: testshelf.unknownDomain.sand.wtg.zone")));
			AssertEquals("Cannot match WebDomain testshelf.unknownDomain.sand.wtg.zone in known configurations [Au2sp-tweb-403a.sand.wtg.zone/*.testrig.sand.wtg.zone, Au2sp-tweb-403b.sand.wtg.zone/*.testrig.sand.wtg.zone, Au2sp-tweb-404a.sand.wtg.zone/*.testrig.sand.wtg.zone, Au2sp-tweb-404b.sand.wtg.zone/*.testrig.sand.wtg.zone, Au2sp-tweb-405a.sand.wtg.zone/*.testrig.sand.wtg.zone, Au2sp-tweb-405b.sand.wtg.zone/*.testrig.sand.wtg.zone] Please use known configurations or specify TestRigWebServer", exception.Message);
		}

		public void TestUnknownServerRejectedWithKnownDomain()
		{
			var exception = AssertExceptionThrown<ArgumentException>(() => new TestedShelfDeploymentOptions(new TaskInfo("testshelf", "owner", @"TestRigRestoreFromBackup: whatever
TestRigWebServer: unknown-server.sand.wtg.zone
TestRigWebDomain: testshelf.testrig.sand.wtg.zone")));
			AssertEquals("Error with WebServer/WebDomain unknown-server.sand.wtg.zone/testshelf.testrig.sand.wtg.zone. WebServer does not match the WebDomain found in known configurations [Au2sp-tweb-403a.sand.wtg.zone/*.testrig.sand.wtg.zone, Au2sp-tweb-403b.sand.wtg.zone/*.testrig.sand.wtg.zone, Au2sp-tweb-404a.sand.wtg.zone/*.testrig.sand.wtg.zone, Au2sp-tweb-404b.sand.wtg.zone/*.testrig.sand.wtg.zone, Au2sp-tweb-405a.sand.wtg.zone/*.testrig.sand.wtg.zone, Au2sp-tweb-405b.sand.wtg.zone/*.testrig.sand.wtg.zone, Au2sp-sweb-407.sand.wtg.zone/*.testrig.sand.wtg.zone]", exception.Message);
		}

		public void TestUnknownServerAndUnknownDomainOk()
		{
			var options = new TestedShelfDeploymentOptions(new TaskInfo("testshelf", "owner", @"TestRigRestoreFromBackup: whatever
TestRigWebDomain: testshelf.unknownDomain.sand.wtg.zone
TestRigWebServer: unknownServer.wtg.zone"));
			AssertEquals("testshelf.unknownDomain.sand.wtg.zone", options.WebDomain);
			AssertEquals("unknownServer.wtg.zone", options.WebServer);
		}

		public void TestKnownServerAndUnknownDomainOk()
		{
			var options = new TestedShelfDeploymentOptions(new TaskInfo("testshelf", "owner", @"TestRigRestoreFromBackup: whatever
TestRigWebDomain: testshelf.someAliaslDomain.sand.wtg.zone
TestRigWebServer: Au2sp-tweb-403a.sand.wtg.zone"));
			AssertEquals("testshelf.someAliaslDomain.sand.wtg.zone", options.WebDomain);
			AssertEquals("Au2sp-tweb-403a.sand.wtg.zone", options.WebServer);
		}

		public void TestKnownServerAndKnownDomainOk()
		{
			var webServers = new string[] {
				"Au2sp-tweb-403a.sand.wtg.zone",
				"Au2sp-tweb-403b.sand.wtg.zone",
				"Au2sp-tweb-404a.sand.wtg.zone",
				"Au2sp-tweb-404b.sand.wtg.zone",
				"Au2sp-tweb-405a.sand.wtg.zone",
				"Au2sp-tweb-405b.sand.wtg.zone"
			};

			foreach (var webServer in webServers)
			{
				var options = new TestedShelfDeploymentOptions(new TaskInfo("testshelf", "owner", $@"TestRigRestoreFromBackup: whatever
TestRigWebDomain: testshelf.testrig.sand.wtg.zone
TestRigWebServer: {webServer}"));

				AssertEquals("testshelf.testrig.sand.wtg.zone", options.WebDomain);
				AssertEquals(webServer, options.WebServer);
			}
		}

		public void TestDbUpgradeServerChosenRandomly()
		{
			// Arrange
			var expectedServers = new List<string>
			{
				"AU2SP-TUPG-401.sand.wtg.zone",
				"AU2SP-TUPG-402.sand.wtg.zone",
			};
			var actualServers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			// Act
			for (int i = 0; i < 10; i++)
			{
				_ = actualServers.Add(new TestedShelfDeploymentOptions(new TaskInfo("shelf", "owner", "TestRigRestoreFromBackup: {backupPath}")).DbUpgradeServer);
			}

			// Assert
			AssertContainsExactElementsInAnyOrder(expectedServers, actualServers);
		}

		public void TestDefaultFileLocationFromServerWhenNoMasterProperties()
		{
			var expectedDataLocation = TestedShelfDeploymentOptions.GetDefaultFileLocationFromServer(TestedShelfDeploymentOptions.DatabaseFileTypes.Data);
			var expectedLogLocation = TestedShelfDeploymentOptions.GetDefaultFileLocationFromServer(TestedShelfDeploymentOptions.DatabaseFileTypes.Log);
			using (TemporarySetExtendedPropertiesForFilePathsInMasterDB(null, null))
			{
				var dataLocation = TestedShelfDeploymentOptions.GetDefaultFileLocations(TestedShelfDeploymentOptions.DatabaseFileTypes.Data);
				var logsLocation = TestedShelfDeploymentOptions.GetDefaultFileLocations(TestedShelfDeploymentOptions.DatabaseFileTypes.Log);
				AssertEquals("with no master.sys.extended_properties, server defaults should be used", expectedDataLocation, dataLocation);
				AssertEquals("with no master.sys.extended_properties, server defaults should be used", expectedLogLocation, logsLocation);
			}
		}

		public void TestDefaultFileLocationFromMasterDatabase()
		{
			var server = System.Environment.MachineName;
			var expectedDataLocation = @"A:\New\Data\Path";
			var expectedLogLocation = @"A:\New\Log\Path";
			var dataLocationFromServer = TestedShelfDeploymentOptions.GetDefaultFileLocationFromServer(TestedShelfDeploymentOptions.DatabaseFileTypes.Data);
			var logsLocationFromServer = TestedShelfDeploymentOptions.GetDefaultFileLocationFromServer(TestedShelfDeploymentOptions.DatabaseFileTypes.Log);
			using (TemporarySetExtendedPropertiesForFilePathsInMasterDB(expectedDataLocation, expectedLogLocation))
			{
				var dataLocation = TestedShelfDeploymentOptions.GetDefaultFileLocations(TestedShelfDeploymentOptions.DatabaseFileTypes.Data);
				var logsLocation = TestedShelfDeploymentOptions.GetDefaultFileLocations(TestedShelfDeploymentOptions.DatabaseFileTypes.Log);
				AssertEquals("use master.sys.extended_properties for file location defaults", expectedDataLocation, dataLocation);
				AssertEquals("use master.sys.extended_properties for file location defaults", expectedLogLocation, logsLocation);
				AssertNotEquals("value from master.sys.extended_properties should differ from server defaults ", dataLocationFromServer, dataLocation);
				AssertNotEquals("value from master.sys.extended_properties should differ from server defaults ", logsLocationFromServer, logsLocation);
			}
		}

		IDisposable TemporarySetExtendedPropertiesForFilePathsInMasterDB(string dataPath, string logPath)
		{
			var adminConnection = Db.NewAdminConnection();

			var sqlText = @"
DECLARE
	@IsValidPath int = 1,
	@Command nvarchar(300),
	@DataPath nvarchar(300) = NULL,
	@LogPath nvarchar(300) = NULL

IF (RIGHT(@DefaultDataPath, 1) = N'\')
	SET @DefaultDataPath = LEFT(@DefaultDataPath, LEN(@DefaultDataPath) - 1)
IF (RIGHT(@DefaultLogPath, 1) = N'\')
	SET @DefaultLogPath = LEFT(@DefaultLogPath, LEN(@DefaultLogPath) - 1)

SELECT @DataPath = CAST(value AS nvarchar(max)) FROM master.sys.extended_properties WHERE name = N'MainDbDataFile'
SELECT @LogPath = CAST(value AS nvarchar(max)) FROM master.sys.extended_properties WHERE name = N'MainDbLogFile'

IF (@DataPath IS NOT NULL) BEGIN
	SET @Command = N'dir ' + @DataPath
	EXEC @IsValidPath = xp_cmdshell @Command

	IF (@IsValidPath <> 0) BEGIN
		IF EXISTS (SELECT * FROM master.sys.extended_properties WHERE name = N'MainDbDataFile_temp')
			EXEC master.sys.sp_updateextendedproperty @name = N'MainDbDataFile_temp', @value = @DataPath
		ELSE
			EXEC master.sys.sp_addextendedproperty @name = N'MainDbDataFile_temp', @value = @DataPath

		EXEC master.sys.sp_updateextendedproperty @name = N'MainDbDataFile', @value = @DefaultDataPath
	END
END
ELSE BEGIN
	IF EXISTS (SELECT * FROM master.sys.extended_properties WHERE name = N'MainDbDataFile_temp')
		EXEC master.sys.sp_updateextendedproperty @name = N'MainDbDataFile_temp', @value = NULL
	ELSE
		EXEC master.sys.sp_addextendedproperty @name = N'MainDbDataFile_temp', @value = NULL

	EXEC master.sys.sp_addextendedproperty @name = N'MainDbDataFile', @value = @DefaultDataPath
END

IF (@LogPath IS NOT NULL) BEGIN
	SET @Command = N'dir ' + @DataPath
	EXEC @IsValidPath = xp_cmdshell @Command

	IF (@IsValidPath <> 0) BEGIN
		IF EXISTS (SELECT * FROM master.sys.extended_properties WHERE name = N'MainDbLogFile_temp')
			EXEC master.sys.sp_updateextendedproperty @name = N'MainDbLogFile_temp', @value = @LogPath
		ELSE
			EXEC master.sys.sp_addextendedproperty @name = N'MainDbLogFile_temp', @value = @LogPath

		EXEC master.sys.sp_updateextendedproperty @name = N'MainDbLogFile', @value = @DefaultLogPath
	END
END
ELSE BEGIN
	IF EXISTS (SELECT * FROM master.sys.extended_properties WHERE name = N'MainDbLogFile_temp')
		EXEC master.sys.sp_updateextendedproperty @name = N'MainDbLogFile_temp', @value = NULL
	ELSE
		EXEC master.sys.sp_addextendedproperty @name = N'MainDbLogFile_temp', @value = NULL

	EXEC master.sys.sp_addextendedproperty @name = N'MainDbLogFile', @value = @DefaultLogPath
END
			";
			using (var command = adminConnection.Command(sqlText))
			{
				command.AddParameter("DefaultDataPath", System.Data.SqlDbType.NVarChar, (object)dataPath ?? DBNull.Value);
				command.AddParameter("DefaultLogPath", System.Data.SqlDbType.NVarChar, (object)logPath ?? DBNull.Value);
				command.ExecuteNonQuery();
			}

			return new DisposableAction(() =>
			{
				adminConnection.ExecuteNonQuery(@"
DECLARE
	@DataPath nvarchar(300) = NULL,
	@LogPath nvarchar(300) = NULL

IF EXISTS (SELECT * FROM master.sys.extended_properties WHERE name = N'MainDbDataFile_temp') BEGIN
	SELECT @DataPath = CAST(value AS nvarchar(max)) FROM master.sys.extended_properties WHERE name = N'MainDbDataFile_temp'
	EXEC master.sys.sp_dropextendedproperty @name = N'MainDbDataFile_temp'

	IF (@DataPath IS NULL) BEGIN
		IF EXISTS (SELECT * FROM master.sys.extended_properties WHERE name = N'MainDbDataFile')
			EXEC master.sys.sp_dropextendedproperty @name = N'MainDbDataFile'
	END
	ELSE IF EXISTS (SELECT * FROM master.sys.extended_properties WHERE name = N'MainDbDataFile')
		EXEC master.sys.sp_updateextendedproperty @name = N'MainDbDataFile', @value = @DataPath
END

IF EXISTS (SELECT * FROM master.sys.extended_properties WHERE name = N'MainDbLogFile_temp') BEGIN
	SELECT @LogPath = CAST(value AS nvarchar(max)) FROM master.sys.extended_properties WHERE name = N'MainDbLogFile_temp'
	EXEC master.sys.sp_dropextendedproperty @name = N'MainDbLogFile_temp'

	IF (@LogPath IS NULL) BEGIN
		IF EXISTS (SELECT * FROM master.sys.extended_properties WHERE name = N'MainDbLogFile')
			EXEC master.sys.sp_dropextendedproperty @name = N'MainDbLogFile'
	END
	ELSE IF EXISTS (SELECT * FROM master.sys.extended_properties WHERE name = N'MainDbLogFile')
		EXEC master.sys.sp_updateextendedproperty @name = N'MainDbLogFile', @value = @LogPath
END

");
				adminConnection.Dispose();
			});
		}

		public void TestDefaultAONSettingsFromTaskinfo()
		{
			var shelfName = "shelf";
			var options = new TestedShelfDeploymentOptions(new TaskInfo(shelfName, "owner", "TestRigSetupAlwaysOn: true"));
			AssertEquals(true, options.SetupAlwaysOn);

			AssertEquals("Group name is the same as shelf name by default", shelfName, options.AONGroupName);
		}

		public void TestDefaultDatabaseFromTaskInfo()
		{
			var expectedDataLocation = @"A:\New\Data\Path";
			var expectedLogLocation = @"A:\New\Log\Path";
			using (TemporarySetExtendedPropertiesForFilePathsInMasterDB(expectedDataLocation, expectedLogLocation))
			{
				var options = new TestedShelfDeploymentOptions(new TaskInfo("shelf", "owner", "TestRigRestoreFromBackup: {backupPath}"));
				AssertCollectionContains(TestedShelfDeploymentOptions.DefaultDatabaseServerConfigurations, x => x.Name == options.SqlServer);
				AssertEquals(expectedDataLocation, options.SqlServerDataFilePath);
				AssertEquals(expectedLogLocation, options.SqlServerLogFilePath);
			}
		}

		public void TestDefaultRegistrationSqlServer()
		{
			var expectedDefaultRegistrationSqlServer = Db.Connection.ExecuteScalar("SELECT @@SERVERNAME").ToString();
			var options = new TestedShelfDeploymentOptions(new TaskInfo("shelf", "owner", "TestRigRestoreFromBackup: {backupPath}"));
			AssertEquals(expectedDefaultRegistrationSqlServer, options.DefaultRegistrationSqlServer);
			AssertEquals(expectedDefaultRegistrationSqlServer, options.RegistrationSqlServer);
		}

		public void TestAngleBracketsOnRestoreFromBackupPath()
		{
			var options = new TestedShelfDeploymentOptions(new TaskInfo("shelf", "owner", @"TestRigRestoreFromBackup: <\\sydsp-ssql-1.sand.wtg.zone\SQL_Backups\OdysseyTrainingModel\OdysseyTrainingModel.bak>"));
			AssertEquals(@"\\sydsp-ssql-1.sand.wtg.zone\SQL_Backups\OdysseyTrainingModel\OdysseyTrainingModel.bak", options.RestoreFromBackup);
		}

		public void TestClientCode()
		{
			var options = new TestedShelfDeploymentOptions(new TaskInfo("shelf", "owner", @"TestRigRestoreFromBackup: <\\sydsp-ssql-1.sand.wtg.zone\SQL_Backups\OdysseyTrainingModel\OdysseyTrainingModel.bak>
TestRigClientCode: EDI"));
			AssertEquals("EDI", options.ClientCode);
		}

		public void TestShelfNameWithSpace()
		{
			var options = new TestedShelfDeploymentOptions(new TaskInfo("test shelf", "owner", @"TestRigRestoreFromBackup: whatever"));
			AssertEquals(options.DatabaseName, "SH0testshelf");
			AssertEquals(options.WebDomain, "testshelf.testrig.sand.wtg.zone");
		}

		public void TestShelfNameWithQuote()
		{
			var options = new TestedShelfDeploymentOptions(new TaskInfo("test's", "owner", @"TestRigRestoreFromBackup: whatever"));
			AssertEquals(options.DatabaseName, "SH0tests");
			AssertEquals(options.WebDomain, "tests.testrig.sand.wtg.zone");
		}

		public void TestShelfNameLongerThan35Characters()
		{
			var options = new TestedShelfDeploymentOptions(new TaskInfo("CargoWiseOneDoesNotAllowTheMainDatabaseNameToBeLongerThan35Characters", "owner", @"TestRigRestoreFromBackup: whatever"));
			AssertEquals(35, options.DatabaseName.Length);
		}

		public void TestTestRigOrigin()
		{
			var expectedOrigin = "WI00761176";
			var taskComments = $@"
TestRigOrigin: {expectedOrigin}
TestRigRestoreFromBackup: whatever";
			var options = new TestedShelfDeploymentOptions(new TaskInfo("shelf", "owner", taskComments));

			AssertEquals(expectedOrigin, options.Origin);
		}

		public void TestReuseDatabaseOptionsAccepted()
		{
			var options = new TestedShelfDeploymentOptions(new TaskInfo("shelf", "owner", @"
TestRigDatabaseName: SH0shelf
TestRigCreateIcon: False
"));
			AssertEquals(true, options.AnyOptionsSet);
		}

		public void TestWebApplicationDefaultsToDatabaseName()
		{
			var options = new TestedShelfDeploymentOptions(new TaskInfo("shelf", "owner", @"
TestRigRestoreFromBackup: whatever
TestRigDatabaseName: SH0MyDb"));
			AssertEquals(options.WebDomain, "mydb.testrig.sand.wtg.zone");
		}

		public void TestIconNameDefaultsToDatabaseName()
		{
			var options = new TestedShelfDeploymentOptions(new TaskInfo("shelf", "owner", @"
TestRigRestoreFromBackup: whatever
TestRigDatabaseName: SH0MyDb"));
			AssertEquals("MyDb", options.IconName);
		}

		public void TestPermitDuplicateWebSitesFalse()
		{
			var options = new TestedShelfDeploymentOptions(new TaskInfo("shelf", "owner", @"
TestRigRestoreFromBackup: whatever
TestRigPermitDuplicateWebSites: false"));
			AssertEquals(false, options.PermitDuplicateWebSites);
		}

		public void TestPermitDuplicateWebSitesTrue()
		{
			var options = new TestedShelfDeploymentOptions(new TaskInfo("shelf", "owner", @"
TestRigRestoreFromBackup: whatever
TestRigPermitDuplicateWebSites: true"));
			AssertEquals(true, options.PermitDuplicateWebSites);
		}

		public void TestPermitDuplicateWebSitesCannotParse()
		{
			var options = new TestedShelfDeploymentOptions(new TaskInfo("shelf", "owner", @"
TestRigRestoreFromBackup: whatever
TestRigPermitDuplicateWebSites: invalidboolean"));
			AssertEquals(false, options.PermitDuplicateWebSites);
		}

		public void TestVerboseModeFalse()
		{
			var options = new TestedShelfDeploymentOptions(new TaskInfo("shelf", "owner", @"TestRigRestoreFromBackup: <\\sydsp-ssql-1.sand.wtg.zone\SQL_Backups\OdysseyTrainingModel\OdysseyTrainingModel.bak>"));
			AssertEquals(false, options.Verbose);
		}

		public void TestVerboseModeTrue()
		{
			var options = new TestedShelfDeploymentOptions(new TaskInfo("shelf", "owner", @"TestRigRestoreFromBackup: <\\sydsp-ssql-1.sand.wtg.zone\SQL_Backups\OdysseyTrainingModel\OdysseyTrainingModel.bak>
TestRigVerbose: true
"));
			AssertEquals(true, options.Verbose);
		}

		public void TestVerboseModeOther()
		{
			var options = new TestedShelfDeploymentOptions(new TaskInfo("shelf", "owner", @"TestRigRestoreFromBackup: <\\sydsp-ssql-1.sand.wtg.zone\SQL_Backups\OdysseyTrainingModel\OdysseyTrainingModel.bak>
TestRigVerbose: somethingnottrueorfalse
"));
			AssertEquals(false, options.Verbose);
		}

		public void TestTestRigPackageStatusCUR() => AssertTestRigPackageStatusParsed("CUR");
		public void TestTestRigPackageStatusRDY() => AssertTestRigPackageStatusParsed("RDY");
		public void TestTestRigPackageStatusXXX() => AssertTestRigPackageStatusParsed("XXX");

		public void TestTestRigScheduleUPGTrue()
		{
			var options = new TestedShelfDeploymentOptions(new TaskInfo("shelf", "owner", @"
TestRigRestoreFromBackup: whatever
TestRigScheduleUPG: true"));
			AssertEquals(true, options.ScheduleUPG);
		}

		public void TestTestRigScheduleUPGFalse()
		{
			var options = new TestedShelfDeploymentOptions(new TaskInfo("shelf", "owner", @"
TestRigRestoreFromBackup: whatever
TestRigScheduleUPG: false"));
			AssertEquals(false, options.ScheduleUPG);
		}

		public void TestTestRigScheduleUPGCannotParse()
		{
			var options = new TestedShelfDeploymentOptions(new TaskInfo("shelf", "owner", @"
TestRigRestoreFromBackup: whatever
TestRigScheduleUPG: invalidboolean"));
			AssertEquals(false, options.ScheduleUPG);
		}

		public void TestTestRigDeployWinzorTrue()
		{
			var options = new TestedShelfDeploymentOptions(new TaskInfo("shelf", "owner", @"
TestRigRestoreFromBackup: whatever
TestRigDeployWinzor: true"));
			AssertEquals(true, options.DeployWinzor);
		}

		public void TestTestRigDeployWinzorFalse()
		{
			var options = new TestedShelfDeploymentOptions(new TaskInfo("shelf", "owner", @"
TestRigRestoreFromBackup: whatever
TestRigDeployWinzor: false"));
			AssertEquals(false, options.DeployWinzor);
		}

		public void TestTestRigDeployWinzorCannotParse()
		{
			var options = new TestedShelfDeploymentOptions(new TaskInfo("shelf", "owner", @"
TestRigRestoreFromBackup: whatever
TestRigDeployWinzor: invalidboolean"));
			AssertEquals(false, options.DeployWinzor);
		}

		public void TestTestRigRunWinzorE2ETestsTrue()
		{
			var options = new TestedShelfDeploymentOptions(new TaskInfo("shelf", "owner", @"
TestRigRestoreFromBackup: whatever
TestRigDeployWinzor: true
TestRigRunWinzorE2ETests: true"));
			AssertEquals(true, options.RunWinzorE2ETests);
		}

		public void TestTestRigRunWinzorE2ETestsFalse()
		{
			var options = new TestedShelfDeploymentOptions(new TaskInfo("shelf", "owner", @"
TestRigRestoreFromBackup: whatever
TestRigDeployWinzor: true
TestRigRunWinzorE2ETests: false"));
			AssertEquals(false, options.RunWinzorE2ETests);
		}

		public void TestTestRigRunWinzorE2ETestsCannotParse()
		{
			var options = new TestedShelfDeploymentOptions(new TaskInfo("shelf", "owner", @"
TestRigRestoreFromBackup: whatever
TestRigDeployWinzor: true
TestRigRunWinzorE2ETests: invalidboolean"));
			AssertEquals(false, options.RunWinzorE2ETests);
		}

		public void TestTestRigRunWinzorE2ETestsFalseIfDeployWinzorFalse()
		{
			var options = new TestedShelfDeploymentOptions(new TaskInfo("shelf", "owner", @"
TestRigRestoreFromBackup: whatever
TestRigDeployWinzor: false
TestRigRunWinzorE2ETests: true"));
			AssertEquals(false, options.RunWinzorE2ETests);
		}

		public void TestTestRigVersionBrokerWarmupTimeoutValid()
		{
			var options = new TestedShelfDeploymentOptions(new TaskInfo("shelf", "owner", @"
TestRigRestoreFromBackup: whatever
TestRigVersionBrokerWarmupTimeout: 00:00:10"));
			AssertEquals(TimeSpan.FromSeconds(10), options.VersionBrokerWarmupTimeout);
		}

		public void TestTestRigVersionBrokerWarmupTimeoutInValid()
		{
			var options = new TestedShelfDeploymentOptions(new TaskInfo("shelf", "owner", @"
TestRigRestoreFromBackup: whatever
TestRigVersionBrokerWarmupTimeout: invalidbooleanTimeSpan"));
			AssertEquals(TimeSpan.FromMinutes(10), options.VersionBrokerWarmupTimeout);
		}

		public void TestAddStaffRecords()
		{
			var options = new TestedShelfDeploymentOptions(new TaskInfo("whatever", "whoever", @"
TestRigRestoreFromBackup: whatever
TestRigAddStaffRecords: Andy.Li, David.James, Hunter.Yang"));
			AssertContainsExactElementsInExactOrder(new[] { "Andy.Li", "David.James", "Hunter.Yang" }, options.AddStaffRecords);
		}

		public void TestAddStaffRecordsWithEmptyElement()
		{
			var options = new TestedShelfDeploymentOptions(new TaskInfo("whatever", "whoever", @"
TestRigRestoreFromBackup: whatever
TestRigAddStaffRecords: Andy.Li, David.James, , Hunter.Yang"));
			AssertContainsExactElementsInExactOrder(new[] { "Andy.Li", "David.James", "Hunter.Yang" }, options.AddStaffRecords);
		}

		public void TestAddStaffRecordsWithDuplicateElement()
		{
			var options = new TestedShelfDeploymentOptions(new TaskInfo("whatever", "whoever", @"
TestRigRestoreFromBackup: whatever
TestRigAddStaffRecords: Andy.Li, David.James, Hunter.Yang, Hunter.Yang"));
			AssertContainsExactElementsInExactOrder(new[] { "Andy.Li", "David.James", "Hunter.Yang" }, options.AddStaffRecords);
		}

		public void TestTestRigSingleRefDatabaseNameParse()
		{
			var sRDbName = "CW-RefDatabase-Test";
			var options = new TestedShelfDeploymentOptions(new TaskInfo("shelf", "owner", $@"
TestRigRestoreFromBackup: whatever
TestRigSingleRefDatabaseName: {sRDbName}"));
			AssertEquals(sRDbName, options.SingleRefDatabaseName);
		}

		public void TestTestRigRewindTransformVersionNumber()
		{
			var rewindTransformVersionNumber = 10;
			var options = new TestedShelfDeploymentOptions(new TaskInfo("shelf", "owner", $@"
TestRigRestoreFromBackup: whatever
TestRigRewindTransformVersionNumber: {rewindTransformVersionNumber}"));
			AssertEquals(rewindTransformVersionNumber, options.RewindTransformVersionNumber);
		}

		public void TestTestRigRegisterDefaultsToFalseWhenNoRestoreFromBackup()
		{
			var options = new TestedShelfDeploymentOptions(new TaskInfo("shelf", "owner", $@"
TestRigSqlServer: server
TestRigDatabaseName: database"));
			AssertEquals(false, options.Register);
		}

		public void TestTestRigRegisterDefaultsToTrueWhenRestoreFromBackup()
		{
			var options = new TestedShelfDeploymentOptions(new TaskInfo("shelf", "owner", $@"
TestRigRestoreFromBackup: something"));
			AssertEquals(true, options.Register);
		}

		public void TestRegistryEntriesInvalid() => AssertRegistryEntries(
			@"
TestRigRestoreFromBackup: whatever
TestRigRegistryEntries: whatever",
			new Dictionary<string, object>());

		public void TestRegistryEntriesIsEmpty() => AssertRegistryEntries(
			@"
TestRigRestoreFromBackup: whatever
TestRigRegistryEntries: ",
			new Dictionary<string, object>());

		public void TestRegistryEntriesWeirdSpacing() => AssertRegistryEntries(
			@"
TestRigRestoreFromBackup: whatever
TestRigRegistryEntries: key1|value1,key2 | value2",
			new Dictionary<string, object>() { { "key1", "value1" }, { "key2", "value2" } });

		public void TestRegistryEntriesSpacingAndInvalidSetting() => AssertRegistryEntries(
			@"
TestRigRestoreFromBackup: whatever
TestRigRegistryEntries: key1|value1,key2 ,  key3 |    value3",
			new Dictionary<string, object>() { { "key1", "value1" }, { "key3", "value3" } });

		public void TestRegistryEntriesConfiguredStringLowercase() => AssertRegistryEntries(
			@"
TestRigRestoreFromBackup: whatever
TestRigRegistryEntries: key1|str|value1",
			new Dictionary<string, object>() { { "key1", "value1" } });

		public void TestRegistryEntriesConfiguredStringMixedCase() => AssertRegistryEntries(
			@"
TestRigRestoreFromBackup: whatever
TestRigRegistryEntries: key1|sTr|value1",
			new Dictionary<string, object>() { { "key1", "value1" } });

		public void TestRegistryEntriesConfiguredStringUppercase() => AssertRegistryEntries(
			@"
TestRigRestoreFromBackup: whatever
TestRigRegistryEntries: key1|STR|value1",
			new Dictionary<string, object>() { { "key1", "value1" } });

		public void TestRegistryEntriesConfiguredTypeWithSpacing() => AssertRegistryEntries(
			@"
TestRigRestoreFromBackup: whatever
TestRigRegistryEntries: key1| str |value1",
			new Dictionary<string, object>() { { "key1", "value1" } });

		public void TestRegistryEntriesConfiguredTypeWithSpacingAndMorePipes() => AssertRegistryEntries(
			@"
TestRigRestoreFromBackup: whatever
TestRigRegistryEntries: key1| str |  value1 | in | side ",
			new Dictionary<string, object>() { { "key1", "value1 | in | side" } });

		public void TestRegistryEntriesConfiguredBoolTrueLowercase() => AssertRegistryEntries(
			@"
TestRigRestoreFromBackup: whatever
TestRigRegistryEntries: key1|bool|true",
			new Dictionary<string, object>() { { "key1", true } });

		public void TestRegistryEntriesConfiguredBoolTrueMixedCase() => AssertRegistryEntries(
			@"
TestRigRestoreFromBackup: whatever
TestRigRegistryEntries: key1|bOOl|true",
			new Dictionary<string, object>() { { "key1", true } });

		public void TestRegistryEntriesConfiguredBoolTrueUppercase() => AssertRegistryEntries(
			@"
TestRigRestoreFromBackup: whatever
TestRigRegistryEntries: key1|BOOL|true",
			new Dictionary<string, object>() { { "key1", true } });

		public void TestRegistryEntriesConfiguredBoolFalseLowercase() => AssertRegistryEntries(
			@"
TestRigRestoreFromBackup: whatever
TestRigRegistryEntries: key1|bool|false",
			new Dictionary<string, object>() { { "key1", false } });

		public void TestRegistryEntriesConfiguredBoolFalseMixedCase() => AssertRegistryEntries(
			@"
TestRigRestoreFromBackup: whatever
TestRigRegistryEntries: key1|bOOl|false",
			new Dictionary<string, object>() { { "key1", false } });

		public void TestRegistryEntriesConfiguredBoolFalseUppercase() => AssertRegistryEntries(
			@"
TestRigRestoreFromBackup: whatever
TestRigRegistryEntries: key1|BOOL|false",
			new Dictionary<string, object>() { { "key1", false } });

		public void TestRegistryEntriesStringBoolDefaultStrArray() => AssertRegistryEntries(
			@"
TestRigRestoreFromBackup: whatever
TestRigRegistryEntries: key1|str|value1, key2|bool|true, key3|default, key4|str_array| value1 ; value2 ",
			new Dictionary<string, object>() { { "key1", "value1" }, { "key2", true }, { "key3", "default" }, { "key4", new string[] { "value1", "value2" } } });

		public void TestRegistryEntriesConfiguredInvalidBoolValue() =>
			AssertExceptionThrown(
				"Key 'key1' requires a boolean value, but value 'invalid' could not be converted to a boolean value",
				typeof(InvalidOperationException),
				() => new TestedShelfDeploymentOptions(new TaskInfo("shelfName", "owner", @"
TestRigRestoreFromBackup: whatever
TestRigRegistryEntries: key1|bool|invalid")));

		public void TestRegistryEntriesConfiguredStrArrayLength0Lowercase() => AssertRegistryEntries(
			@"
TestRigRestoreFromBackup: whatever
TestRigRegistryEntries: key1|str_array| ",
			new Dictionary<string, object>() { { "key1", Array.Empty<string>() } });

		public void TestRegistryEntriesConfiguredStrArrayLength0MixedCase() => AssertRegistryEntries(
			@"
TestRigRestoreFromBackup: whatever
TestRigRegistryEntries: key1|sTR_aRRaY| ",
			new Dictionary<string, object>() { { "key1", Array.Empty<string>() } });

		public void TestRegistryEntriesConfiguredStrArrayLength0Uppercase() => AssertRegistryEntries(
			@"
TestRigRestoreFromBackup: whatever
TestRigRegistryEntries: key1|STR_ARRAY| ",
			new Dictionary<string, object>() { { "key1", Array.Empty<string>() } });

		public void TestRegistryEntriesConfiguredStrArrayLength1Lowercase() => AssertRegistryEntries(
			@"
TestRigRestoreFromBackup: whatever
TestRigRegistryEntries: key1|str_array| value1 ",
			new Dictionary<string, object>() { { "key1", new string[] { "value1" } } });

		public void TestRegistryEntriesConfiguredStrArrayLength1MixedCase() => AssertRegistryEntries(
			@"
TestRigRestoreFromBackup: whatever
TestRigRegistryEntries: key1|sTR_aRRaY| value1 ",
			new Dictionary<string, object>() { { "key1", new string[] { "value1" } } });

		public void TestRegistryEntriesConfiguredStrArrayLength1Uppercase() => AssertRegistryEntries(
			@"
TestRigRestoreFromBackup: whatever
TestRigRegistryEntries: key1|STR_ARRAY| value1 ",
			new Dictionary<string, object>() { { "key1", new string[] { "value1" } } });

		public void TestRegistryEntriesConfiguredStrArrayLength2Lowercase() => AssertRegistryEntries(
			@"
TestRigRestoreFromBackup: whatever
TestRigRegistryEntries: key1|str_array| value1 ; value2 ",
			new Dictionary<string, object>() { { "key1", new string[] { "value1", "value2" } } });

		public void TestRegistryEntriesConfiguredStrArrayLength2MixedCase() => AssertRegistryEntries(
			@"
TestRigRestoreFromBackup: whatever
TestRigRegistryEntries: key1|sTR_aRRaY| value1 ; value2 ",
			new Dictionary<string, object>() { { "key1", new string[] { "value1", "value2" } } });

		public void TestRegistryEntriesConfiguredStrArrayLength2Uppercase() => AssertRegistryEntries(
			@"
TestRigRestoreFromBackup: whatever
TestRigRegistryEntries: key1|STR_ARRAY| value1 ; value2 ",
			new Dictionary<string, object>() { { "key1", new string[] { "value1", "value2" } } });

		public void TestRegistryEntriesConfiguredInvalidType() =>
			AssertExceptionThrown(
				"Key 'key1' has unknown datatype 'InV', only str, str_array and bool are supported\r\n",
				typeof(InvalidOperationException),
				() => new TestedShelfDeploymentOptions(new TaskInfo("shelfName", "owner", @"
TestRigRestoreFromBackup: whatever
TestRigRegistryEntries: key1|InV|value1")));

		public void TestRegistryEntriesReportMultipleErrors() =>
			AssertExceptionThrown(
@"Key 'key1' has unknown datatype 'InV', only str, str_array and bool are supported
Key 'key2' requires a boolean value, but value 'invalid' could not be converted to a boolean value
",
				typeof(InvalidOperationException),
				() => new TestedShelfDeploymentOptions(new TaskInfo("shelfName", "owner", @"
TestRigRestoreFromBackup: whatever
TestRigRegistryEntries: key1|InV|value1, key2|bool|invalid")));

		public void TestRegistryEntriesAutomaticServices() => AssertRegistryEntries(
@"TestRigRestoreFromBackup: whatever
TestRigWebDomain: syd.web.domain
TestRigWebServer: whatever
TestRigWebSites: Services,Glow,GlowWebClient",
			new Dictionary<string, object>
			{
				["GlowEnterpriseServicesRootUri"] = "https://syd.web.domain/Services",
				["GlowServiceUri"] = "https://syd.web.domain/Glow",
				["GlowPortalsUri"] = "https://syd.web.domain/Portals",
			});

		public void TestRegistryEntriesAutomaticServicesIncludingNeoDoesNotBreak() => AssertRegistryEntries(
@"TestRigRestoreFromBackup: whatever
TestRigWebDomain: syd.web.domain
TestRigWebServer: whatever
TestRigWebSites: Services,Glow,GlowWebClient,Neo,NeoWebClient",
			new Dictionary<string, object>
			{
				["GlowEnterpriseServicesRootUri"] = "https://syd.web.domain/Services",
				["GlowServiceUri"] = "https://syd.web.domain/Glow",
				["GlowPortalsUri"] = "https://syd.web.domain/Portals",
			});

		public void TestRegistryEntriesAutomaticServicesOnlyNeoIsEmpty() => AssertRegistryEntries(
@"TestRigRestoreFromBackup: whatever
TestRigWebDomain: syd.web.domain
TestRigWebServer: whatever
TestRigWebSites: Neo,NeoWebClient",
			new Dictionary<string, object>
			{
			});

		public void TestRegistryEntriesUserDefinedValueServices() => AssertRegistryEntries(
@"TestRigRestoreFromBackup: whatever
TestRigRegistryEntries: GLOWENTERPRISESERVICESROOTURI|userDefinedValue, GlowServiceUri|customValue, GlowPortalsUri|str|anotherCustomValue
TestRigWebDomain: syd.web.domain
TestRigWebServer: whatever
TestRigWebSites: Services,Glow,GlowWebClient",
			new Dictionary<string, object>
			{
				["GlowEnterpriseServicesRootUri"] = "userDefinedValue",
				["GlowServiceUri"] = "customValue",
				["GlowPortalsUri"] = "anotherCustomValue",
			});

		static void AssertRegistryEntries(string inputString, IDictionary<string, object> keyValuePairs)
		{
			var options = new TestedShelfDeploymentOptions(new TaskInfo("shelfName", "owner", inputString));
			AssertEquals(keyValuePairs.Count, options.RegistryEntries.Count);
			foreach (var kvp in keyValuePairs)
			{
				if (kvp.Value is Array kvpValue)
				{
					AssertContainsExactElementsInAnyOrder(kvpValue, (Array)options.RegistryEntries[kvp.Key]);
				}
				else
				{
					AssertEquals(kvp.Value, options.RegistryEntries[kvp.Key]);
				}
			}
		}

		static void AssertTestRigPackageStatusParsed(string status)
		{
			AssertEquals(status, new TestedShelfDeploymentOptions(new TaskInfo("shelf", "owner", $@"
TestRigRestoreFromBackup: whatever
TestRigPackageStatus: {status}")).PackageStatus);
		}

		public void TestRigEnableAuditIsMissing() => AssertEnableAudit()
			.WithInputString("TestRigRestoreFromBackup: whatever")
			.ExpectServiceTask(false)
			.ExpectEnableAudit(false);

		public void TestRigEnableAuditIsMissingServiceTasksIsTrue() => AssertEnableAudit()
			.WithInputString(@"
TestRigRestoreFromBackup: whatever
TestRigServiceTasks:true")
			.ExpectServiceTask(true)
			.ExpectEnableAudit(false);

		public void TestRigEnableAuditIsMissingServiceTasksIsTrueRegistryEntryIsConfigured() => AssertEnableAudit()
			.WithInputString(
@"TestRigServiceTasks:true
TestRigRestoreFromBackup: whatever
TestRigRegistryEntries:BiAuditServer|regentry")
			.ExpectServiceTask(true)
			.ExpectRegistryEntry("BiAuditServer").HasValue("regentry")
			.ExpectEnableAudit(false);

		public void TestRigEnableAuditIsFalse() => AssertEnableAudit()
			.WithInputString(@"
TestRigRestoreFromBackup: whatever
TestRigEnableAudit:false")
			.ExpectServiceTask(false)
			.ExpectEnableAudit(false);

		public void TestRigEnableAuditIsFalseServiceTasksIsTrue() => AssertEnableAudit()
			.WithInputString(
@"TestRigRestoreFromBackup: whatever
TestRigEnableAudit:false
TestRigServiceTasks:true")
			.ExpectServiceTask(true)
			.ExpectEnableAudit(false);

		public void TestRigEnableAuditIsFalseServiceTasksIsTrueRegistryEntryIsConfigured() => AssertEnableAudit()
			.WithInputString(
@"TestRigEnableAudit:false
TestRigServiceTasks:true
TestRigRestoreFromBackup: whatever
TestRigRegistryEntries:BiAuditServer|regentry")
			.ExpectServiceTask(true)
			.ExpectRegistryEntry("BiAuditServer").HasValue("regentry")
			.ExpectEnableAudit(false);

		public void TestRigEnableAuditIsTrue() => AssertEnableAudit()
			.WithInputString(@"
TestRigRestoreFromBackup: whatever
TestRigEnableAudit:true")
			.ExpectServiceTask(true)
			.ExpectRegistryEntry("BiAuditServer").HasAnyValue(TestedShelfDeploymentOptions.DefaultDatabaseServerConfigurations.Select(x => x.Name))
			.ExpectEnableAudit(true);

		public void TestRigEnableAuditIsTrueServiceTasksIsFalse() => AssertEnableAudit()
			.WithInputString(
@"TestRigRestoreFromBackup: whatever
TestRigEnableAudit:true
TestRigServiceTasks:false")
			.ExpectServiceTask(false)
			.ExpectRegistryEntry("BiAuditServer").HasAnyValue(TestedShelfDeploymentOptions.DefaultDatabaseServerConfigurations.Select(x => x.Name))
			.ExpectEnableAudit(true);

		public void TestRigEnableAuditIsTrueSqlServerIsConfigured() => AssertEnableAudit()
			.WithInputString(
@"TestRigEnableAudit:true
TestRigSqlServer:sqlserver")
			.ExpectServiceTask(true)
			.ExpectRegistryEntry("BiAuditServer").HasValue("sqlserver")
			.ExpectEnableAudit(true);

		public void TestRigEnableAuditIsTrueRegistryEntryIsConfigured() => AssertEnableAudit()
			.WithInputString(
@"TestRigEnableAudit:true
TestRigRestoreFromBackup: whatever
TestRigRegistryEntries:BiAuditServer|regentry")
			.ExpectServiceTask(true)
			.ExpectRegistryEntry("BiAuditServer").HasValue("regentry")
			.ExpectEnableAudit(true);

		public void TestRigEnableAuditIsTrueRegistryEntryIsConfiguredSqlServerIsConfigured() => AssertEnableAudit()
			.WithInputString(
@"TestRigEnableAudit:true
TestRigRestoreFromBackup: whatever
TestRigRegistryEntries:BiAuditServer|regentry
TestRigSqlServer:sqlserver")
			.ExpectServiceTask(true)
			.ExpectRegistryEntry("BiAuditServer").HasValue("regentry")
			.ExpectEnableAudit(true);

		public void TestRigEnableAuditIsTrueServiceTaskIsFalseSqlServerIsConfigured() => AssertEnableAudit()
			.WithInputString(
@"TestRigEnableAudit:true
TestRigServiceTasks:false
TestRigSqlServer:sqlserver")
			.ExpectServiceTask(false)
			.ExpectRegistryEntry("BiAuditServer").HasValue("sqlserver")
			.ExpectEnableAudit(true);

		public void TestRigEnableAuditIsTrueServiceTaskIsFalseRegistryEntryIsConfigured() => AssertEnableAudit()
			.WithInputString(
@"TestRigEnableAudit:true
TestRigServiceTasks:false
TestRigRestoreFromBackup: whatever
TestRigRegistryEntries:BiAuditServer|regentry")
			.ExpectServiceTask(false)
			.ExpectRegistryEntry("BiAuditServer").HasValue("regentry")
			.ExpectEnableAudit(true);

		public void TestRigEnableAuditIsTrueServiceTaskIsFalseRegistryEntryIsConfiguredSqlServerIsConfigured() => AssertEnableAudit()
			.WithInputString(
@"TestRigEnableAudit:true
TestRigServiceTasks:false
TestRigRestoreFromBackup: whatever
TestRigRegistryEntries:BiAuditServer|regentry
TestRigSqlServer:sqlserver")
			.ExpectServiceTask(false)
			.ExpectRegistryEntry("BiAuditServer").HasValue("regentry")
			.ExpectEnableAudit(true);

		static AssertEnableAuditHelper.InputStringSupplier AssertEnableAudit() => new AssertEnableAuditHelper.InputStringSupplier();

		[CodeAlive("Used By Test")]
		sealed class AssertEnableAuditHelper
		{
			public string Key { get; }
			public IEnumerable<string> Values { get; }

			public bool IsMatched(KeyValuePair<string, object> kvp) => Key.Equals(kvp.Key) && Values.Any(v => v.Equals(kvp.Value.ToString()));

			public sealed class InputStringSupplier
			{
				public ServiceTaskExpectation WithInputString(string inputString) => new ServiceTaskExpectation { InputString = inputString };
			}

			public sealed class ServiceTaskExpectation
			{
				public string InputString { get; set; }
				public AuditRegistryExpectation ExpectServiceTask(bool serviceTask) => new AuditRegistryExpectation { InputString = InputString, ServiceTask = serviceTask };
			}

			public sealed class AuditRegistryExpectation
			{
				public AuditRegistryExpectation()
				{
				}

				public AuditRegistryExpectation(AuditRegistryExpectation that, string key, IEnumerable<object> values)
				{
					InputString = that.InputString;
					ServiceTask = that.ServiceTask;
					AuditRegistryEntries = that.AuditRegistryEntries
						.Concat(new[] { new KeyValuePair<string, IEnumerable<object>>(key, values) })
						.ToDictionary(x => x.Key, x => x.Value);
				}

				public string InputString { get; set; }
				public bool ServiceTask { get; set; }
				public IDictionary<string, IEnumerable<object>> AuditRegistryEntries { get; set; } = new Dictionary<string, IEnumerable<object>>();
				public AuditRegistryValueExpectation ExpectRegistryEntry(string key) => new AuditRegistryValueExpectation { AuditRegistryExpectation = this, Key = key };

				public void ExpectEnableAudit(bool enableAudit)
				{
					var options = new TestedShelfDeploymentOptions(new TaskInfo("shelfName", "owner", InputString));
					AssertEquals("ServiceTask", options.ServiceTasks, ServiceTask);
					AssertEquals("EnableAudit", options.EnableAudit, enableAudit);
					AssertEquals("RegistryEntries.Count", options.RegistryEntries.Count, AuditRegistryEntries.Count);
					foreach (var auditRegistryEntry in AuditRegistryEntries)
					{
						Assert("RegistryEntries has key", options.RegistryEntries.TryGetValue(auditRegistryEntry.Key, out var registryValue));
						Assert("RegistryEntries item has value", auditRegistryEntry.Value.Any(auditRegistryEntryValue => auditRegistryEntryValue.Equals(registryValue)));
					}
				}
			}

			public sealed class AuditRegistryValueExpectation
			{
				public AuditRegistryExpectation AuditRegistryExpectation { get; set; }
				public string Key { get; set; }

				public AuditRegistryExpectation HasValue(string value) => new AuditRegistryExpectation(AuditRegistryExpectation, Key, new object[] { value });
				public AuditRegistryExpectation HasAnyValue(IEnumerable<string> values) => new AuditRegistryExpectation(AuditRegistryExpectation, Key, values.ToArray());
			}
		}

		public void TestRigEnableAnalysisIsMissing() => AssertEnableAnalysisServer()
			.WithInputString(@"
TestRigRestoreFromBackup: whatever
TestRigServiceTasks:true
")
			.ExpectEnableAnalysis(false)
			.ExpectAnalysisServer(null)
			.Verify();

		public void TestRigEnableAnalysisIsPresentAsFalseAnalysisServerIsMissing() => AssertEnableAnalysisServer()
			.WithInputString(@"
TestRigRestoreFromBackup: whatever
TestRigEnableAnalysis:false
")
			.ExpectEnableAnalysis(false)
			.ExpectAnalysisServer(null)
			.Verify();

		public void TestRigEnableAnalysisIsMissingAnalysisServerIsPresent() => AssertEnableAnalysisServer()
			.WithInputString(@"
TestRigRestoreFromBackup: whatever
TestRigAnalysisServer:testHost
")
			.ExpectEnableAnalysis(false)
			.ExpectAnalysisServer(null)
			.Verify();

		public void TestRigEnableAnalysisIsPresentAsTrueAnalysisServerIsMissing() => AssertEnableAnalysisServer()
			.WithInputString(@"
TestRigRestoreFromBackup: whatever
TestRigEnableAnalysis:true
")
			.ExpectEnableAnalysis(true)
			.ExpectAnalysisServer(null)
			.Verify();
		public void TestRigEnableAnalysisIsPresentAsTrueAnalysisServerIsPresent() => AssertEnableAnalysisServer()
			.WithInputString(@"
TestRigRestoreFromBackup: whatever
TestRigEnableAnalysis:true
TestRigAnalysisServer:testHost
")
			.ExpectEnableAnalysis(true)
			.ExpectAnalysisServer("testHost")
			.Verify();

		public void TestRigEnableAnalysisIsPresentAsFalseAnalysisServerIsPresent() => AssertEnableAnalysisServer()
			.WithInputString(@"
TestRigRestoreFromBackup: whatever
TestRigEnableAnalysis:false
TestRigAnalysisServer:testHost
")
			.ExpectEnableAnalysis(false)
			.ExpectAnalysisServer(null)
			.Verify();

		public void TestRigEnableAnalysisIsPresentAsTrueAnalysisServerIsPresentInReverseOrder() => AssertEnableAnalysisServer()
			.WithInputString(@"
TestRigRestoreFromBackup: whatever
TestRigAnalysisServer: testHost
TestRigEnableAnalysis: true
")
			.ExpectEnableAnalysis(true)
			.ExpectAnalysisServer("testHost")
			.Verify();

		public void TestRigEnableAnalysisIsPresentAsFalseAnalysisServerIsPresentInReverseOrder() => AssertEnableAnalysisServer()
			.WithInputString(@"
TestRigRestoreFromBackup: whatever
TestRigAnalysisServer: testHost
TestRigEnableAnalysis: false
")
			.ExpectEnableAnalysis(false)
			.ExpectAnalysisServer(null)
			.Verify();

		public AssertEnableAnalysisServerHelper AssertEnableAnalysisServer() => new AssertEnableAnalysisServerHelper();
		[CodeAlive("Used By Test")]
		public sealed class AssertEnableAnalysisServerHelper
		{
			const string RegistrySettingBiAnalysisServer = "BiAnalysisServer";
			string inputString;
			bool enableAnalysis;
			string analysisServer;

			public AssertEnableAnalysisServerHelper WithInputString(string inputString)
			{
				this.inputString = inputString;
				return this;
			}

			public AssertEnableAnalysisServerHelper ExpectEnableAnalysis(bool enableAnalysis)
			{
				this.enableAnalysis = enableAnalysis;
				return this;
			}

			public AssertEnableAnalysisServerHelper ExpectAnalysisServer(string analysisServer)
			{
				this.analysisServer = analysisServer;
				return this;
			}
			public void Verify()
			{
				TestedShelfDeploymentOptions options = new TestedShelfDeploymentOptions(new TaskInfo("shelfName", "owner", inputString));
				AssertEquals("EnableAnalysis", enableAnalysis, options.EnableAnalysis);
				options.RegistryEntries.TryGetValue(RegistrySettingBiAnalysisServer, out object regAnalysisServerVal);
				AssertEquals("Registry Entry for Analysis Server", analysisServer, regAnalysisServerVal);
			}
		}

		public void TestRigDataWarehouseServerIsMissing() => AssertDataWarehouseServer()
			.WithInputString(
@"TestRigEnableAudit:true
TestRigServiceTasks:false
TestRigRestoreFromBackup: whatever
TestRigSqlServer:sqlserver
")
			.ExpectDataWarehouseServer(null)
			.Verify();

		public void TestRigDataWarehouseServerIsPresent() => AssertDataWarehouseServer()
		.WithInputString(
@"TestRigEnableAudit:true
TestRigServiceTasks:false
TestRigRestoreFromBackup: whatever
TestRigSqlServer:sqlserver
TestRigDataWarehouseServer: hostName
")
		.ExpectDataWarehouseServer("hostName")
		.Verify();

		public DataWarehouseServerExpectation AssertDataWarehouseServer() => new DataWarehouseServerExpectation();

		public sealed class DataWarehouseServerExpectation
		{
			readonly string RegistrySettingBiDataWarehouseServer = "BiDataWarehouseServer";
			string InputString;
			string DataWarehouseServer;
			public DataWarehouseServerExpectation WithInputString(string inputString)
			{
				this.InputString = inputString;
				return this;
			}
			public DataWarehouseServerExpectation ExpectDataWarehouseServer(string dataWarehouseServer)
			{
				this.DataWarehouseServer = dataWarehouseServer;
				return this;
			}
			public void Verify()
			{
				TestedShelfDeploymentOptions options = new TestedShelfDeploymentOptions(new TaskInfo("shelfName", "owner", InputString));
				AssertEquals("DataWareHouseServer", DataWarehouseServer, options.DataWarehouseServer);
				options.RegistryEntries.TryGetValue(RegistrySettingBiDataWarehouseServer, out object dataWarehouseServerRegistryEntry);
				AssertEquals("DataWareHouseServerRegistry", DataWarehouseServer, (string)dataWarehouseServerRegistryEntry);
			}
		}

		public void TestAllowsDeploymentToExplicitlyRequestedExcludedServer() => AssertWebServer()
		.WithInputString(
@"TestRigRestoreFromBackup:whatever
TestRigWebSites: Services,Glow
TestRigWebServer:au2sp-sweb-407.sand.wtg.zone
")
		.ExpectWebServer(new[] { "au2sp-sweb-407.sand.wtg.zone" })
		.Verify();

		public void TestAllowsDeploymentToAllowedServers() => AssertWebServer()
		.WithInputString(
@"TestRigRestoreFromBackup:whatever
TestRigWebSites: Services,Glow
")
		.ExpectWebServer(new[]
		{
			"Au2sp-tweb-403a.sand.wtg.zone",
			"Au2sp-tweb-403b.sand.wtg.zone",
			"Au2sp-tweb-404a.sand.wtg.zone",
			"Au2sp-tweb-404b.sand.wtg.zone",
			"Au2sp-tweb-405a.sand.wtg.zone",
			"Au2sp-tweb-405b.sand.wtg.zone"
		})
		.Verify();

		public void TestDisallowsDeploymentToInvalidServer() => AssertWebServer()
		.WithInputString(
@"TestRigRestoreFromBackup:whatever
TestRigWebSites: Services,Glow
TestRigWebServer:SomeWebServer
")
		.ExpectWebServer(null)
		.Verify();

		public WebServerExpectation AssertWebServer() => new WebServerExpectation();

		public sealed class WebServerExpectation
		{
			string InputString;
			string[] AllowedWebServers;
			public WebServerExpectation WithInputString(string inputString)
			{
				this.InputString = inputString;
				return this;
			}
			public WebServerExpectation ExpectWebServer(string[] allowedWebServers)
			{
				this.AllowedWebServers = allowedWebServers;
				return this;
			}
			public void Verify()
			{
				var options = new TestedShelfDeploymentOptions(new TaskInfo("shelfName", "owner", InputString));
				if (AllowedWebServers != null)
				{
					Assert(
						$"WebServer '{options.WebServer}' is not in the list of allowed servers.",
						AllowedWebServers.Contains(options.WebServer)
					);
				}
				else
				{
					Assert("Invalid server supplied!", AllowedWebServers is null);
				}
			}
		}
	}
}

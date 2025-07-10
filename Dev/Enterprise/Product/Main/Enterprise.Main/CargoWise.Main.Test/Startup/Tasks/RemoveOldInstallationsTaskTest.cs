using System;
using System.IO;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Client.Common;
using Enterprise.Environment;
using Enterprise.Environment.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Upgrades;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;

#if !WINZOR && NETFRAMEWORK
using System.Reflection;
#endif

namespace Enterprise.Startup.Testing
{
	sealed class RemoveOldInstallationsTaskTest : BackgroundApplicationStartupTaskTest<RemoveOldInstallationsTask>
	{
#if !WINZOR && NETFRAMEWORK // NETFRAMEWORK Should be fixed in WI00669071: Remove usages of AppDomains

		public void TestDeleteOldInstalledVersions()
		{
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			var rootDir = Temp.TempPath;
			var tempDir = Path.Combine(rootDir, Guid.NewGuid().ToString());
			using (InstallationEnvironmentForTest.TempDirectoryForTest(tempDir))
			{
				string currentPath = InstallationEnvironment.Instance.GetTargetInstallPath(ReleaseInfo.Instance.VersionNumber);
				var currentVersionFile = Path.Combine(Path.GetDirectoryName(currentPath), "CurrentVersion");
				File.WriteAllText(currentVersionFile, Db.ServerName + "," + Db.DatabaseName + "," + ReleaseInfo.Instance.VersionNumber);

				string originalAsm = typeof(ZString).Assembly.Location;
				string parentPath = Path.GetDirectoryName(currentPath);
				File.WriteAllText(Path.Combine(currentPath, "current.txt"), "current");
				string lockedPath = Path.Combine(parentPath, "1.0.0");
				Directory.CreateDirectory(lockedPath);
				Directory.SetCreationTimeUtc(lockedPath, DateTime.UtcNow.Subtract(TimeSpan.FromDays(2)));
				File.WriteAllText(Path.Combine(lockedPath, "one.txt"), "one");
				File.WriteAllText(Path.Combine(lockedPath, "two.txt"), "two");
				File.Copy(originalAsm, Path.Combine(lockedPath, Path.GetFileName(originalAsm)));
				string unlockedPath = Path.Combine(parentPath, "1.1.0");
				Directory.CreateDirectory(unlockedPath);
				Directory.SetCreationTimeUtc(unlockedPath, DateTime.UtcNow.Subtract(TimeSpan.FromDays(2)));
				File.WriteAllText(Path.Combine(unlockedPath, "one.txt"), "one");
				File.WriteAllText(Path.Combine(unlockedPath, "two.txt"), "two");
				File.Copy(originalAsm, Path.Combine(unlockedPath, Path.GetFileName(originalAsm)));
				AppDomain appDomain = AppDomain.CreateDomain("TestDeleteOldInstalledVersions");

				try
				{
					appDomain.SetData("asm", Path.Combine(lockedPath, Path.GetFileName(originalAsm)));
					appDomain.DoCallBack(delegate
					{
						Assembly.LoadFile((string)AppDomain.CurrentDomain.GetData("asm"));
					});
					new RemoveOldInstallationsTask().DoExecute();

					AssertContainsExactElementsInAnyOrder(new string[] { Path.Combine(parentPath, "CurrentVersion"), Path.Combine(parentPath, CurrentVersionConfigFileName) }, Directory.GetFiles(parentPath));
					AssertContainsExactElementsInAnyOrder(new string[] { currentPath, lockedPath }, Directory.GetDirectories(parentPath));
					AssertContainsExactElementsInAnyOrder(new string[] { Path.Combine(currentPath, "current.txt") }, Directory.GetFiles(currentPath));
					AssertContainsExactElementsInAnyOrder(new string[] { Path.Combine(lockedPath, Path.GetFileName(originalAsm)), Path.Combine(lockedPath, "one.txt"), Path.Combine(lockedPath, "two.txt") }, Directory.GetFiles(lockedPath));
				}
				finally
				{
					AppDomain.Unload(appDomain);
				}
			}
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
		}

		public void TestExecutingRootFilesPreserved()
		{
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			var rootDir = Temp.TempPath;
			var tempDir = Path.Combine(rootDir, Guid.NewGuid().ToString());
			using (InstallationEnvironmentForTest.TempDirectoryForTest(tempDir))
			{
				string currentPath = InstallationEnvironment.Instance.GetTargetInstallPath(ReleaseInfo.Instance.VersionNumber);
				Directory.SetCreationTimeUtc(currentPath, DateTime.UtcNow.Subtract(TimeSpan.FromDays(2)));
				string parentPath = Path.GetDirectoryName(currentPath);
				File.WriteAllText(Path.Combine(parentPath, "one.txt"), "one");
				File.WriteAllText(Path.Combine(parentPath, "two.txt"), "two");
				string originalAsm = typeof(ZString).Assembly.Location;
				string loadedAsm = Path.Combine(parentPath, Path.GetFileName(originalAsm));
				string configFile = Path.Combine(parentPath, CurrentVersionConfigFileName);
				File.Copy(originalAsm, loadedAsm);
				AppDomain appDomain = AppDomain.CreateDomain("TestExecutingRootFilesPreserved");
				try
				{
					appDomain.SetData("asm", loadedAsm);
					appDomain.DoCallBack(delegate
					{
						Assembly.LoadFile((string)AppDomain.CurrentDomain.GetData("asm"));
					});
					new RemoveOldInstallationsTask().DoExecute();

					AssertContainsExactElementsInAnyOrder(new string[] { loadedAsm, configFile, Path.Combine(parentPath, "one.txt"), Path.Combine(parentPath, "two.txt") }, Directory.GetFiles(parentPath));
				}
				finally
				{
					AppDomain.Unload(appDomain);
				}
			}
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
		}

#endif

		public void TestDeleteOldInstalledVersionsPreservesServerFiles()
		{
			var rootDir = Temp.TempPath;
			var tempDir = Path.Combine(rootDir, Guid.NewGuid().ToString());
			using (InstallationEnvironmentForTest.TempDirectoryForTest(tempDir))
			{
				string currentPath = InstallationEnvironment.Instance.GetTargetInstallPath(ReleaseInfo.Instance.VersionNumber);
				Directory.SetCreationTimeUtc(currentPath, DateTime.UtcNow.Subtract(TimeSpan.FromDays(2)));
				var currentVersionFile = Path.Combine(Path.GetDirectoryName(currentPath), "CurrentVersion");
				File.WriteAllText(currentVersionFile, Db.ServerName + "," + Db.DatabaseName + "," + ReleaseInfo.Instance.VersionNumber);
				string parentPath = Path.GetDirectoryName(currentPath);
				File.WriteAllText(Path.Combine(parentPath, "ediLoad.exe"), "ediload");
				File.WriteAllText(Path.Combine(parentPath, "ediLoad.ini"), "ediload");
				string versionDir = Path.Combine(parentPath, "1.0.0");
				Directory.CreateDirectory(versionDir);
				Directory.SetCreationTimeUtc(versionDir, DateTime.UtcNow.Subtract(TimeSpan.FromDays(2)));
				string distributeDir = Path.Combine(parentPath, "Distribute");
				Directory.CreateDirectory(distributeDir);
				Directory.SetCreationTimeUtc(distributeDir, DateTime.UtcNow.Subtract(TimeSpan.FromDays(2)));

				new RemoveOldInstallationsTask().DoExecute();

				AssertContainsExactElementsInAnyOrder(new string[]
					{
						Path.Combine(parentPath, "ediLoad.exe"),
						Path.Combine(parentPath, "ediLoad.ini"),
						Path.Combine(parentPath, "CurrentVersion"),
						Path.Combine(parentPath, CurrentVersionConfigFileName),
					},
					Directory.GetFiles(parentPath));
				AssertContainsExactElementsInAnyOrder(new string[]
					{
						currentPath,
						Path.Combine(parentPath, "Distribute"),
					},
					Directory.GetDirectories(parentPath));
			}
		}

		public void TestShouldExecute()
		{
			RemoveOldInstallationsTask task = new RemoveOldInstallationsTask();
			Assert(!task.ShouldExecute(new ApplicationArguments(Array.Empty<string>())));
			Assert(task.ShouldExecute(new ApplicationArguments(new string[] { "-IAmDoingTheWrongThingByRunningEnterpriseWithoutLoader" })));
			Assert(task.ShouldExecute(new ApplicationArguments(new string[] { "-ScheduledDbUpgrader" })));
			Assert(!task.ShouldExecute(new ApplicationArguments(new string[] { "-SkipVersionCheck" })));
			Assert(!task.ShouldExecute(new ApplicationArguments(new string[] { "-IAmDoingTheWrongThingByRunningEnterpriseWithoutLoader", "-SkipVersionCheck" })));
			Assert(!task.ShouldExecute(new ApplicationArguments(new string[] { "-ScheduledDbUpgrader", "-SkipVersionCheck" })));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFilesPreservedWhileInstallationHandleIsHeld()
		{
			var rootDir = Temp.TempPath;
			var tempDir = Path.Combine(rootDir, Guid.NewGuid().ToString());
			using (InstallationEnvironmentForTest.TempDirectoryForTest(tempDir))
			{
				var upgradeManager = Upgrader.NewUpgradeManager();
				var version = new Version(1, 2, 3, 4);
				var info = upgradeManager.UploadUpgradePackage(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\MasterFiles\Business\MasterFiles.Business\System\StmUpgrade\testing.edp"), version, DateTime.UtcNow, "RDY", string.Empty, null);
				var targetPath = InstallationEnvironment.Instance.GetTargetInstallPath(new VersionNumber(version));
				using (upgradeManager.InstallUpgradePackage(info, targetPath))
				{
					Directory.SetCreationTimeUtc(targetPath, DateTime.UtcNow.Subtract(TimeSpan.FromDays(2)));
					var files = Directory.GetFiles(targetPath);
					Assert(files.Length > 5);

					SetNextCleanupIsOverdue(targetPath);
					new RemoveOldInstallationsTask().DoExecute();
					AssertContainsExactElementsInAnyOrder(files, Directory.GetFiles(targetPath));
				}

				SetNextCleanupIsOverdue(targetPath);
				new RemoveOldInstallationsTask().DoExecute();
				Assert(!Directory.Exists(targetPath));
			}

			void SetNextCleanupIsOverdue(string targetPath)
			{
				string baseInstallationPath = Directory.GetParent(targetPath).FullName;
				var configManager = new CurrentVersionCleanerConfigManager();
				var config = configManager.LoadConfiguration(baseInstallationPath) as CurrentVersionCleanerConfig;
				config.LastStartTime = DateTime.UtcNow.AddDays(-2);
				config.CurrentVersionCleanupIntervalInDays = TimeSpan.FromDays(1);
				configManager.SaveConfigurationViaAppManager(baseInstallationPath, config);
			}
		}

		public void TestCleanUpCurrentVersionFile()
		{
			AssertCleanUpCurrentVersionFileTask("server", "database");
		}

		public void TestCleanUpCurrentVersionFileWithServerNameWithNonStandardSqlPort()
		{
			AssertCleanUpCurrentVersionFileTask("server,1526", "database");
		}

		public void TestCleanUpCurrentVersionFile_WithDatabaseNameWithNonStandardSqlPort()
		{
			AssertCleanUpCurrentVersionFileTask("server", "database,2508");
		}

		public void TestCleanUpCurrentVersionFile_WithServerAndDatabaseNameWithNonStandardSqlPort()
		{
			AssertCleanUpCurrentVersionFileTask("server,1526", "database,2508");
		}

		public void AssertCleanUpCurrentVersionFileTask(string serverName, string databaseName)
		{
			var logPath = ApplicationUsageLogFile.EnterpriseLogStoragePath;
			var logBackUpPath = Path.Combine(Path.GetDirectoryName(logPath), "ApplicationUsageLogBackUp");
			var needToRecoverLog = File.Exists(logPath);

			var currentVersionPath = Path.Combine(InstallationEnvironment.Instance.BaseInstallPath, "CurrentVersion");
			var currentVersionBackUpPath = Path.Combine(InstallationEnvironment.Instance.BaseInstallPath, "CurrentVersionBackUp");
			var needToRecoverCurrentVersion = File.Exists(currentVersionPath);

			if (needToRecoverLog)
			{
				File.Copy(logPath, logBackUpPath, true);
				File.Delete(logPath);
			}
			if (needToRecoverCurrentVersion)
			{
				File.Copy(currentVersionPath, currentVersionBackUpPath, true);
				File.Delete(currentVersionPath);
			}
			try
			{
				var applicationUsageLogFile = new ApplicationUsageLogFile();
				AssertEquals("There should not be any logs in the file before starting the test", 0, applicationUsageLogFile.RetrieveAllTheLogs().Count);
				var currentVersionFile = new CurrentVersionFile(currentVersionPath);
				AssertEquals("There should not be any versions in the file before starting the test", 0, currentVersionFile.RetrieveAllVersionNumbers().Length);

				applicationUsageLogFile.LogUsage(new ApplicationUsageLog(serverName, databaseName, ZDateTime.BrettsBirthday.ToDateTime()));
				currentVersionFile.RecordVersionNumber(serverName, databaseName, "10.1");

				var manager = new CurrentVersionCleanerConfigManager();
				var config = manager.LoadConfiguration(InstallationEnvironment.Instance.BaseInstallPath);
				config.LastStartTime = DateTime.MinValue;
				manager.SaveConfigurationViaAppManager(InstallationEnvironment.Instance.BaseInstallPath, config);

				new RemoveOldInstallationsTask().DoExecute();

				AssertEquals("There should be not versions in the file because the record that was there was inactive", 0, currentVersionFile.RetrieveAllVersionNumbers().Length);
			}
			finally
			{
				if (needToRecoverLog)
				{
					File.Copy(logBackUpPath, logPath, true);
					File.Delete(logBackUpPath);
				}
				else
				{
					File.Delete(logPath);
				}

				if (needToRecoverCurrentVersion)
				{
					File.Copy(currentVersionBackUpPath, currentVersionPath, true);
					File.Delete(currentVersionBackUpPath);
				}
				else
				{
					File.Delete(currentVersionPath);
				}

				File.Delete(Path.Combine(InstallationEnvironment.Instance.BaseInstallPath, CurrentVersionConfigFileName));
			}
		}

		const string CurrentVersionConfigFileName = "CurrentVersionConfig.xml";

		public override int DefaultErrorExitCode => ExitCodes.RemoveOldInstallationsTaskError;
	}
}

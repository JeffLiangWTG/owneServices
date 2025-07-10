using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using CargoWise.Data;
using CargoWise.IO;
using CargoWise.Loader.Common;
using CargoWise.Loader.Common.Testing;
using Enterprise.Upgrades;
using Moq;
using NUnit.Framework;

namespace Enterprise.Loader.Testing.Uninstall
{
	class UninstallerTest : TestCase
	{
		public void TestUninstallDeletesCurrentVersionFile()
		{
			// Arrange
			using (var tempDirectory = new TempDirectory())
			{
				var baseTargetPath = tempDirectory.DirectoryName;
				var currentVersionFilePath = Path.Combine(baseTargetPath, "CurrentVersion");

				// Act
				_ = TestUninstall(baseTargetPath, new[] { "21.8.30.1" });

				// Assert
				Assert(!File.Exists(currentVersionFilePath));
			}
		}

		public void TestUninstallDeletesAllVersionFolders()
		{
			// Arrange
			using (var tempDirectory = new TempDirectory())
			{
				var baseTargetPath = tempDirectory.DirectoryName;
				var currentVersionFilePath = Path.Combine(baseTargetPath, "CurrentVersion");

				// Act
				_ = TestUninstall(baseTargetPath, new[] { "21.8.30.1", "19.11.13.100", "21.5.19.1", "21.2.19.17", "21.8.25.3" });
				var subDirectories = Directory
					.EnumerateDirectories(baseTargetPath, "*.*", SearchOption.TopDirectoryOnly)
					.ToList();

				// Assert
				CombineAssertions(() =>
				{
					Assert(subDirectories.All(x => !Version.TryParse(x, out _)));
					Assert(!subDirectories.Any());
				});
			}
		}

		public void TestUninstallDoesNotDeleteNonVersionFolders()
		{
			// Arrange
			using (var tempDirectory = new TempDirectory())
			{
				var baseTargetPath = tempDirectory.DirectoryName;
				var currentVersionFilePath = Path.Combine(baseTargetPath, "CurrentVersion");

				// Act
				var nonVersionedFolders = new[] { "Debug", "Bin", "Data" };
				_ = TestUninstall(baseTargetPath, nonVersionedFolders.Concat(new[] { "21.8.30.1", "19.11.13.100", "21.8.25.3" }));
				var subDirectories = Directory
					.EnumerateDirectories(baseTargetPath, "*.*", SearchOption.TopDirectoryOnly)
					.Select(Path.GetFileName);

				// Assert
				CombineAssertions(() =>
				{
					Assert(subDirectories.All(x => !Version.TryParse(x, out _)));
					AssertContainsExactElementsInAnyOrder(nonVersionedFolders, subDirectories);
				});
			}
		}

		InstallationResultCollection TestUninstall(string baseTargetPath, IEnumerable<string> subDirectories)
		{
			// Arrange
			const string databaseName = "TestDatabaseName";

			var currentVersionFilePath = Path.Combine(baseTargetPath, "CurrentVersion");
			var currentVersionFile = new CurrentVersionFile(currentVersionFilePath);

			var installedVersion = subDirectories.First(x => Version.TryParse(x, out _));
			var recordedVersions = new List<(string ServerName, string DatabaseName, string Version)>
			{
				(Dns.GetHostName(), databaseName, installedVersion),
				(Dns.GetHostName(), Db.DatabaseName, installedVersion),
				(Dns.GetHostEntry(Dns.GetHostName()).AddressList.First().ToString(), databaseName, installedVersion),
				(Dns.GetHostEntry(Dns.GetHostName()).AddressList.First().ToString(), Db.DatabaseName, installedVersion),
				("localhost", databaseName, installedVersion),
				("localhost", Db.DatabaseName, installedVersion),
				("127.0.0.1", databaseName, installedVersion),
				("127.0.0.1", Db.DatabaseName, installedVersion),
			};

			subDirectories
				.Where(x => x != installedVersion && Version.TryParse(x, out _))
				.ToList()
				.ForEach(
					x => recordedVersions.Add(("localhost", Db.DatabaseName, x)));
			subDirectories.ToList().ForEach(
				x => Directory.CreateDirectory(Path.Combine(baseTargetPath, x)));
			recordedVersions.ForEach(
				x => currentVersionFile.RecordVersionNumber(x.ServerName, x.DatabaseName, x.Version));

			Assert(subDirectories.All(x => Directory.Exists(Path.Combine(baseTargetPath, x))));
			Assert(File.Exists(currentVersionFilePath));

			var configuration = new MockConfiguration(baseTargetPath, baseTargetPath);
			var installationMock = new Mock<Installation>(configuration);
			var results = new InstallationResultCollection();
			var uninstaller = new Uninstaller(installationMock.Object);

			// Act
			uninstaller.Install(results);
			return results;
		}

		[TestRequiresAdministrativePrivileges("Avoid Remote AppManager Invoke")]
		public void TestWaitsForMutexRelease()
		{
			// Arrange
			using (var tempDirectory = new TempDirectory())
			{
				var mutex = new UpgraderMutex("Global\\CargoWiseOneOldVersionsRemover");
				var versionDirectory = Path.Combine(tempDirectory.DirectoryName, "21.5.7.13");
				Directory.CreateDirectory(versionDirectory);
				Assert(mutex.IsCreatedNew && mutex.WaitOne(TimeSpan.Zero));
				Assert(Directory.Exists(versionDirectory));

				var installationMock = new Mock<Installation>(
					new MockConfiguration(tempDirectory, tempDirectory)
					{
						TargetVersion = new Version("1.2.0"),
					});

				var results = new InstallationResultCollection();

				var thread = new Thread(() =>
				{
					var remover = new Uninstaller(installationMock.Object);
					remover.Install(results);
				});
				thread.Start();
				AssertEquals("Uninstall thread should be still running", false, thread.Join(TimeSpan.FromSeconds(10)));

				// Act
				mutex.Dispose();

				// Assert
				CombineAssertions(() =>
				{
					AssertEquals("Uninstall thread should finish", true, thread.Join(TimeSpan.FromSeconds(10)));
					var result = results.Select(installationResult => installationResult.Message).ToList();
					AssertCollectionNotContains("Should be no log about skipping the cleanup", "Removing old installations skipped, due to the other instance is running", result);
					AssertEquals("Should be a successful result", 3, results.OKCount);
					AssertContainsExactElementsInAnyOrder("Should be a successful result", new[] { string.Empty, string.Empty, string.Empty }, result);
				});
			}
		}
	}
}

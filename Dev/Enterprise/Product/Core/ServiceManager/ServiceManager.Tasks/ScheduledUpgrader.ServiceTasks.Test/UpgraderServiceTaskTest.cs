	
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
#if NETFRAMEWORK
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CargoWise.Types;
using Enterprise.Environment.Testing;
using Enterprise.ServiceManager.Business;
using Microsoft.CodeAnalysis.CSharp;
#endif
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Data.Utils;
using CargoWise.IO;
using Enterprise.Integration;
using Enterprise.MailManager.FileDownload;
using Enterprise.MasterFiles.Business;
using Enterprise.Upgrades;
using Enterprise.Upgrades.UpgradePackageServices;
using Enterprise.ZArchitecture.Core;
using Microsoft.CodeAnalysis;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static System.FormattableString;

namespace Enterprise.ServiceManager.Tasks.ScheduledUpgrader.ServiceTasks.Testing
{
	[TestedType(typeof(UpgraderServiceTask))]
	sealed class UpgraderServiceTaskTest : ServiceTaskTestCase<UpgraderServiceTask>
	{
		readonly Version OnlineVersionPackedForTestDownload = new Version(1, 2, 3, 4);

		[UseSnapshotProtection]
		public void TestDbIsLockedoutExceptionLogTheMessage()
		{
			using var adminConnection = Db.NewAdminConnection();
			_ = adminConnection.ResetLockout();
			AssertEquals(DbLockoutState.NoLockout, DbLockout.CheckLockoutState(adminConnection));

			try
			{
				// Arrange
				_ = DbLockout.AcquireLockout(adminConnection, LockoutReason.Upgrade);
				AssertEquals(DbLockoutState.ValidLockout, adminConnection.CheckLockoutState());

				var loggerMock = new Mock<ILogger>();

				// Act
				var upgServiceTask = new UpgraderServiceTask { ServiceLogger = loggerMock.Object };
				AssertNoExceptionThrown(() => upgServiceTask.RunTask());

				// Assert
				AssertNoExceptionThrown(() =>
				{
					loggerMock.Verify(logger => logger.Log(LogType.Information, $"Database {Db.DatabaseName} is locked out for {LockoutReason.Upgrade}."), Times.Once);
				});
			}
			finally
			{
				_ = DbLockout.ResetLockout(adminConnection);
			}
		}

		public void TestServiceTaskCanRunInAnyBranch()
		{
			Assert(GetHostedServiceAttributes().All(x => x.CanRunInAnyBranch));
		}

		public void TestOptions()
		{
			var attributes = GetHostedServiceAttributes().FirstOrDefault();

			AssertEquals("1week", attributes.DefaultScheduleRunEvery);
			AssertEquals("24hours", attributes.DefaultScheduleDoNotRunTillNextDueTimeIfOverdue);
		}

		public void TestUpgrader_NewLocalVersion_UpgradeApplied()
		{
			// Arrange
			var currentVersionNumber = ReleaseInfo.Instance.VersionNumber;
			var localUpgradeVersion = currentVersionNumber.AddRelease(1);
			var localStmUpgradeVersions = new List<(VersionNumber versionNumber, string status, DateTime versionDate)>
			{
				(currentVersionNumber, StmUpgrade.StmUpgradeStatus.CurrentVersion, DateTime.UtcNow),
				(currentVersionNumber.AddRelease(-1), StmUpgrade.StmUpgradeStatus.Ready, DateTime.UtcNow),
				(localUpgradeVersion, StmUpgrade.StmUpgradeStatus.Ready, DateTime.UtcNow)
			};

			var expectedUpgradeVersion = localUpgradeVersion.ToVersion();

			_ = CreateStmUpgradeVersions_ForTest(localStmUpgradeVersions);

			var configScenarios = new List<ScheduledUpgraderConfig>
			{
				new ScheduledUpgraderConfig("") { AutoDownload = false, PatchOnly = false },
				new ScheduledUpgraderConfig("") { AutoDownload = true, PatchOnly = false }
			};

			using (var httpTestHelper = new HttpTestHelper())
			{
				httpTestHelper.Start();

				foreach (ScheduledUpgraderConfig config in configScenarios)
				{
					var expectedLogs = new List<string>
					{
						Invariant($"Information|Auto-Download is set to '{(config.AutoDownload ? "true" : "false")}'."),
						Invariant($"Information|Patch-Only is set to '{(config.PatchOnly ? "true" : "false")}'."),
						Invariant($"Information|Current version is {currentVersionNumber}."),
						"Information|Checking locally for a newer upgrade package...",
						Invariant($"Information|Upgrade package {localUpgradeVersion.ToVersion()} found.")
					};
					if (config.AutoDownload)
					{
						expectedLogs.Add("Information|Checking online for a newer upgrade package...");
						expectedLogs.Add("Information|No new online upgrade package available.");
					}
					expectedLogs.Add(Invariant($"Information|Applying upgrade {expectedUpgradeVersion}."));

					TestUpgraderAndAssertServiceTaskLogs(httpTestHelper, config, expectedLogs, expectedUpgradeVersion);
				}
			}
		}

		public void TestUpgrader_NewLocalVersion_UpgradeNotApplied()
		{
			// Arrange
			var currentVersionNumber = ReleaseInfo.Instance.VersionNumber;
			var localUpgradeVersion = currentVersionNumber.AddRelease(1);
			var localStmUpgradeVersions = new List<(VersionNumber versionNumber, string status, DateTime versionDate)>
			{
				(currentVersionNumber, StmUpgrade.StmUpgradeStatus.CurrentVersion, DateTime.UtcNow),
				(currentVersionNumber.AddRelease(-1), StmUpgrade.StmUpgradeStatus.Ready, DateTime.UtcNow),
				(localUpgradeVersion, StmUpgrade.StmUpgradeStatus.Ready, DateTime.UtcNow)
			};

			Version expectedUpgradeVersion = null;

			_ = CreateStmUpgradeVersions_ForTest(localStmUpgradeVersions);

			var configScenarios = new List<ScheduledUpgraderConfig>
			{
				new ScheduledUpgraderConfig("") { AutoDownload = false, PatchOnly = true },
				new ScheduledUpgraderConfig("") { AutoDownload = true, PatchOnly = true }
			};

			using (var httpTestHelper = new HttpTestHelper())
			{
				httpTestHelper.Start();

				foreach (ScheduledUpgraderConfig config in configScenarios)
				{
					var expectedLogs = new List<string>
					{
						Invariant($"Information|Auto-Download is set to '{(config.AutoDownload ? "true" : "false")}'."),
						Invariant($"Information|Patch-Only is set to '{(config.PatchOnly ? "true" : "false")}'."),
						Invariant($"Information|Current version is {currentVersionNumber}."),
						"Information|Checking locally for a newer upgrade package...",
						Invariant($"Information|Upgrade package {localUpgradeVersion.ToVersion()} found, not applying because Patch-Only is set to 'true'."),
						"Information|No new local upgrade package available."
					};
					TestUpgraderAndAssertServiceTaskLogs(httpTestHelper, config, expectedLogs, expectedUpgradeVersion);
				}
			}
		}

		public void TestUpgrader_NewLocalPatch_PatchApplied()
		{
			// Arrange
			var currentVersionNumber = ReleaseInfo.Instance.VersionNumber;
			var localUpgradeVersion = currentVersionNumber.AddPatch(10);
			var localStmUpgradeVersions = new List<(VersionNumber versionNumber, string status, DateTime versionDate)>
			{
				(currentVersionNumber, StmUpgrade.StmUpgradeStatus.CurrentVersion, DateTime.UtcNow),
				(currentVersionNumber.AddRelease(-1), StmUpgrade.StmUpgradeStatus.Ready, DateTime.UtcNow),
				(localUpgradeVersion, StmUpgrade.StmUpgradeStatus.Ready, DateTime.UtcNow)
			};

			Version expectedUpgradeVersion = localUpgradeVersion.ToVersion();

			_ = CreateStmUpgradeVersions_ForTest(localStmUpgradeVersions);

			var configScenarios = new List<ScheduledUpgraderConfig>
			{
				new ScheduledUpgraderConfig("") { AutoDownload = false, PatchOnly = false },
				new ScheduledUpgraderConfig("") { AutoDownload = false, PatchOnly = true },
				new ScheduledUpgraderConfig("") { AutoDownload = true, PatchOnly = false },
				new ScheduledUpgraderConfig("") { AutoDownload = true, PatchOnly = true }
			};

			using (var httpTestHelper = new HttpTestHelper())
			{
				httpTestHelper.Start();

				foreach (ScheduledUpgraderConfig config in configScenarios)
				{
					var expectedLogs = new List<string>
					{
						Invariant($"Information|Auto-Download is set to '{(config.AutoDownload ? "true" : "false")}'."),
						Invariant($"Information|Patch-Only is set to '{(config.PatchOnly ? "true" : "false")}'."),
						Invariant($"Information|Current version is {currentVersionNumber}."),
						"Information|Checking locally for a newer upgrade package...",
						Invariant($"Information|Upgrade package {localUpgradeVersion.ToVersion()} found.")
					};
					if (config.AutoDownload)
					{
						expectedLogs.Add("Information|Checking online for a newer upgrade package...");
						expectedLogs.Add("Information|No new online upgrade package available.");
					}
					expectedLogs.Add(Invariant($"Information|Applying upgrade {expectedUpgradeVersion}."));

					TestUpgraderAndAssertServiceTaskLogs(httpTestHelper, config, expectedLogs, expectedUpgradeVersion);
				}
			}
		}

		public void TestUpgrader_NewLocalVersionAndPatch_UpgradeApplied()
		{
			// Arrange
			var currentVersionNumber = ReleaseInfo.Instance.VersionNumber;
			var localUpgradeVersion = currentVersionNumber.AddRelease(1);
			var localPatchVersion = currentVersionNumber.AddPatch(10);
			var localStmUpgradeVersions = new List<(VersionNumber versionNumber, string status, DateTime versionDate)>
			{
				(currentVersionNumber, StmUpgrade.StmUpgradeStatus.CurrentVersion, DateTime.UtcNow),
				(currentVersionNumber.AddRelease(-1), StmUpgrade.StmUpgradeStatus.Ready, DateTime.UtcNow),
				(localUpgradeVersion, StmUpgrade.StmUpgradeStatus.Ready, DateTime.UtcNow.AddDays(2)),
				(localPatchVersion, StmUpgrade.StmUpgradeStatus.Ready, DateTime.UtcNow.AddDays(1))
			};

			Version expectedUpgradeVersion = localUpgradeVersion.ToVersion();

			_ = CreateStmUpgradeVersions_ForTest(localStmUpgradeVersions);

			var configScenarios = new List<ScheduledUpgraderConfig>
			{
				new ScheduledUpgraderConfig("") { AutoDownload = false, PatchOnly = false },
				new ScheduledUpgraderConfig("") { AutoDownload = true, PatchOnly = false }
			};

			using (var httpTestHelper = new HttpTestHelper())
			{
				httpTestHelper.Start();

				foreach (ScheduledUpgraderConfig config in configScenarios)
				{
					var expectedLogs = new List<string>
					{
						Invariant($"Information|Auto-Download is set to '{(config.AutoDownload ? "true" : "false")}'."),
						Invariant($"Information|Patch-Only is set to '{(config.PatchOnly ? "true" : "false")}'."),
						Invariant($"Information|Current version is {currentVersionNumber}."),
						"Information|Checking locally for a newer upgrade package...",
						Invariant($"Information|Upgrade package {localUpgradeVersion.ToVersion()} found.")
					};
					if (config.AutoDownload)
					{
						expectedLogs.Add("Information|Checking online for a newer upgrade package...");
						expectedLogs.Add("Information|No new online upgrade package available.");
					}
					expectedLogs.Add(Invariant($"Information|Applying upgrade {expectedUpgradeVersion}."));

					TestUpgraderAndAssertServiceTaskLogs(httpTestHelper, config, expectedLogs, expectedUpgradeVersion);
				}
			}
		}

		public void TestUpgrader_NewLocalVersionAndPatch_PatchApplied()
		{
			// Arrange
			var currentVersionNumber = ReleaseInfo.Instance.VersionNumber;
			var localUpgradeVersion = currentVersionNumber.AddRelease(1);
			var localPatchVersion = currentVersionNumber.AddPatch(10);
			var localStmUpgradeVersions = new List<(VersionNumber versionNumber, string status, DateTime versionDate)>
			{
				(currentVersionNumber, StmUpgrade.StmUpgradeStatus.CurrentVersion, DateTime.UtcNow),
				(currentVersionNumber.AddRelease(-1), StmUpgrade.StmUpgradeStatus.Ready, DateTime.UtcNow),
				(localUpgradeVersion, StmUpgrade.StmUpgradeStatus.Ready, DateTime.UtcNow.AddDays(2)),
				(localPatchVersion, StmUpgrade.StmUpgradeStatus.Ready, DateTime.UtcNow.AddDays(1))
			};

			Version expectedUpgradeVersion = localPatchVersion.ToVersion();

			_ = CreateStmUpgradeVersions_ForTest(localStmUpgradeVersions);

			var configScenarios = new List<ScheduledUpgraderConfig>
			{
				new ScheduledUpgraderConfig("") { AutoDownload = false, PatchOnly = true },
				new ScheduledUpgraderConfig("") { AutoDownload = true, PatchOnly = true }
			};

			using (var httpTestHelper = new HttpTestHelper())
			{
				httpTestHelper.Start();

				foreach (ScheduledUpgraderConfig config in configScenarios)
				{
					var expectedLogs = new List<string>
					{
						Invariant($"Information|Auto-Download is set to '{(config.AutoDownload ? "true" : "false")}'."),
						Invariant($"Information|Patch-Only is set to '{(config.PatchOnly ? "true" : "false")}'."),
						Invariant($"Information|Current version is {currentVersionNumber}."),
						"Information|Checking locally for a newer upgrade package...",
						Invariant($"Information|Upgrade package {localPatchVersion.ToVersion()} found.")
					};
					if (config.AutoDownload)
					{
						expectedLogs.Add("Information|Checking online for a newer upgrade package...");
						expectedLogs.Add("Information|No new online upgrade package available.");
					}
					expectedLogs.Add(Invariant($"Information|Applying upgrade {expectedUpgradeVersion}."));

					TestUpgraderAndAssertServiceTaskLogs(httpTestHelper, config, expectedLogs, expectedUpgradeVersion);
				}
			}
		}

		public void TestUpgrader_NewOnlineVersion_UpgradeApplied()
		{
			// Arrange
			var currentVersionNumber = ReleaseInfo.Instance.VersionNumber;
			var localStmUpgradeVersions = new List<(VersionNumber versionNumber, string status, DateTime versionDate)>
			{
				(currentVersionNumber, StmUpgrade.StmUpgradeStatus.CurrentVersion, DateTime.UtcNow),
				(currentVersionNumber.AddRelease(-1), StmUpgrade.StmUpgradeStatus.Ready, DateTime.UtcNow)
			};

			Version onlineUpgradeVersion = currentVersionNumber.AddRelease(1).ToVersion();
			Version expectedUpgradeVersion = OnlineVersionPackedForTestDownload;

			_ = CreateStmUpgradeVersions_ForTest(localStmUpgradeVersions);

			var configScenarios = new List<ScheduledUpgraderConfig>
			{
				new ScheduledUpgraderConfig("") { AutoDownload = true, PatchOnly = false }
			};

			using (var httpTestHelper = new HttpTestHelper())
			{
				httpTestHelper.Start();

				foreach (ScheduledUpgraderConfig config in configScenarios)
				{
					var expectedLogs = new List<string>
					{
						Invariant($"Information|Auto-Download is set to '{(config.AutoDownload ? "true" : "false")}'."),
						Invariant($"Information|Patch-Only is set to '{(config.PatchOnly ? "true" : "false")}'."),
						Invariant($"Information|Current version is {currentVersionNumber}."),
						"Information|Checking locally for a newer upgrade package...",
						"Information|No new local upgrade package available.",
						"Information|Checking online for a newer upgrade package...",
						Invariant($"Information|Upgrade package {onlineUpgradeVersion} found, downloading from {httpTestHelper.ServerAddress}testing.edp"),
						Invariant($"Information|Applying upgrade {expectedUpgradeVersion}.")
					};

					TestUpgraderAndAssertServiceTaskLogs(httpTestHelper, config, expectedLogs, expectedUpgradeVersion, onlineUpgradeVersion);
				}
			}
		}

		public void TestUpgrader_NewOnlineVersion_CorruptFile()
		{
			// Arrange
			var currentVersionNumber = ReleaseInfo.Instance.VersionNumber;
			var localStmUpgradeVersions = new List<(VersionNumber versionNumber, string status, DateTime versionDate)>
			{
				(currentVersionNumber, StmUpgrade.StmUpgradeStatus.CurrentVersion, DateTime.UtcNow),
				(currentVersionNumber.AddRelease(-1), StmUpgrade.StmUpgradeStatus.Ready, DateTime.UtcNow)
			};

			Version onlineUpgradeVersion = currentVersionNumber.AddRelease(1).ToVersion();

			_ = CreateStmUpgradeVersions_ForTest(localStmUpgradeVersions);

			var configScenarios = new List<ScheduledUpgraderConfig>
			{
				new ScheduledUpgraderConfig("") { AutoDownload = true, PatchOnly = false }
			};

			using (var httpTestHelper = new HttpTestHelper())
			{
				httpTestHelper.Start();

				foreach (ScheduledUpgraderConfig config in configScenarios)
				{
					var expectedLogs = new List<string>
					{
						Invariant($"Information|Auto-Download is set to '{(config.AutoDownload ? "true" : "false")}'."),
						Invariant($"Information|Patch-Only is set to '{(config.PatchOnly ? "true" : "false")}'."),
						Invariant($"Information|Current version is {currentVersionNumber}."),
						"Information|Checking locally for a newer upgrade package...",
						"Information|No new local upgrade package available.",
						"Information|Checking online for a newer upgrade package...",
						Invariant($"Information|Upgrade package {onlineUpgradeVersion} found, downloading from {httpTestHelper.ServerAddress}testing.edp"),
						"Information|Downloaded 3 of 3 bytes",
						"Information|Uploading package to database",
						Invariant($"Error|The upgrade package file is corrupt. Check that the file has finished downloading, and that the file size is correct. If the problem persists, try downloading a new copy of the file.")
					};

					TestUpgraderAndAssertServiceTaskLogs(httpTestHelper, config, expectedLogs, null, onlineUpgradeVersion, true);

					AssertEquals("The upgrade package file is corrupt. Check that the file has finished downloading, and that the file size is correct. If the problem persists, try downloading a new copy of the file.", ErrorReporter.LastMessageReported);
					ErrorReporter.Clear();
				}
			}
		}

		public void TestUpgrader_NewOnlineVersion_UpgradeNotApplied()
		{
			// Arrange
			var currentVersionNumber = ReleaseInfo.Instance.VersionNumber;
			var localStmUpgradeVersions = new List<(VersionNumber versionNumber, string status, DateTime versionDate)>
			{
				(currentVersionNumber, StmUpgrade.StmUpgradeStatus.CurrentVersion, DateTime.UtcNow),
				(currentVersionNumber.AddRelease(-1), StmUpgrade.StmUpgradeStatus.Ready, DateTime.UtcNow)
			};

			Version onlineUpgradeVersion = currentVersionNumber.AddRelease(1).ToVersion();
			Version expectedUpgradeVersion = null;

			_ = CreateStmUpgradeVersions_ForTest(localStmUpgradeVersions);

			var configScenarios = new List<ScheduledUpgraderConfig>
			{
				new ScheduledUpgraderConfig("") { AutoDownload = false, PatchOnly = false },
				new ScheduledUpgraderConfig("") { AutoDownload = false, PatchOnly = true },
				new ScheduledUpgraderConfig("") { AutoDownload = true, PatchOnly = true }
			};

			using (var httpTestHelper = new HttpTestHelper())
			{
				httpTestHelper.Start();

				foreach (ScheduledUpgraderConfig config in configScenarios)
				{
					var expectedLogs = new List<string>
					{
						Invariant($"Information|Auto-Download is set to '{(config.AutoDownload ? "true" : "false")}'."),
						Invariant($"Information|Patch-Only is set to '{(config.PatchOnly ? "true" : "false")}'."),
						Invariant($"Information|Current version is {currentVersionNumber}."),
						"Information|Checking locally for a newer upgrade package...",
						"Information|No new local upgrade package available."
					};
					if (config.AutoDownload)
					{
						expectedLogs.Add("Information|Checking online for a newer upgrade package...");
						expectedLogs.Add($"Information|Upgrade package {onlineUpgradeVersion} found, not downloading because Patch-Only is set to 'true'.");
					}

					TestUpgraderAndAssertServiceTaskLogs(httpTestHelper, config, expectedLogs, expectedUpgradeVersion, onlineUpgradeVersion);
				}
			}
		}

		public void TestUpgrader_NewOnlinePatch_PatchApplied()
		{
			// Arrange
			var currentVersionNumber = ReleaseInfo.Instance.VersionNumber;
			var localStmUpgradeVersions = new List<(VersionNumber versionNumber, string status, DateTime versionDate)>
			{
				(currentVersionNumber, StmUpgrade.StmUpgradeStatus.CurrentVersion, DateTime.UtcNow),
				(currentVersionNumber.AddRelease(-1), StmUpgrade.StmUpgradeStatus.Ready, DateTime.UtcNow)
			};

			Version onlineUpgradeVersion = currentVersionNumber.AddPatch(10).ToVersion();
			Version expectedUpgradeVersion = OnlineVersionPackedForTestDownload;

			_ = CreateStmUpgradeVersions_ForTest(localStmUpgradeVersions);

			var configScenarios = new List<ScheduledUpgraderConfig>
			{
				new ScheduledUpgraderConfig("") { AutoDownload = true, PatchOnly = false },
				new ScheduledUpgraderConfig("") { AutoDownload = true, PatchOnly = true }
			};

			using (var httpTestHelper = new HttpTestHelper())
			{
				httpTestHelper.Start();

				foreach (ScheduledUpgraderConfig config in configScenarios)
				{
					var expectedLogs = new List<string>
					{
						Invariant($"Information|Auto-Download is set to '{(config.AutoDownload ? "true" : "false")}'."),
						Invariant($"Information|Patch-Only is set to '{(config.PatchOnly ? "true" : "false")}'."),
						Invariant($"Information|Current version is {currentVersionNumber}."),
						"Information|Checking locally for a newer upgrade package...",
						"Information|No new local upgrade package available.",
						"Information|Checking online for a newer upgrade package...",
						Invariant($"Information|Upgrade package {onlineUpgradeVersion} found, downloading from {httpTestHelper.ServerAddress}testing.edp"),
						Invariant($"Information|Applying upgrade {expectedUpgradeVersion}.")
					};

					TestUpgraderAndAssertServiceTaskLogs(httpTestHelper, config, expectedLogs, expectedUpgradeVersion, onlineUpgradeVersion);
				}
			}
		}

		public void TestUpgrader_NewOnlinePatch_PatchNotApplied()
		{
			// Arrange
			var currentVersionNumber = ReleaseInfo.Instance.VersionNumber;
			var localStmUpgradeVersions = new List<(VersionNumber versionNumber, string status, DateTime versionDate)>
			{
				(currentVersionNumber, StmUpgrade.StmUpgradeStatus.CurrentVersion, DateTime.UtcNow),
				(currentVersionNumber.AddRelease(-1), StmUpgrade.StmUpgradeStatus.Ready, DateTime.UtcNow)
			};

			Version onlineUpgradeVersion = currentVersionNumber.AddPatch(10).ToVersion();
			Version expectedUpgradeVersion = null;

			_ = CreateStmUpgradeVersions_ForTest(localStmUpgradeVersions);

			var configScenarios = new List<ScheduledUpgraderConfig>
			{
				new ScheduledUpgraderConfig("") { AutoDownload = false, PatchOnly = false },
				new ScheduledUpgraderConfig("") { AutoDownload = false, PatchOnly = true }
			};

			using (var httpTestHelper = new HttpTestHelper())
			{
				httpTestHelper.Start();

				foreach (ScheduledUpgraderConfig config in configScenarios)
				{
					var expectedLogs = new List<string>
					{
						Invariant($"Information|Auto-Download is set to '{(config.AutoDownload ? "true" : "false")}'."),
						Invariant($"Information|Patch-Only is set to '{(config.PatchOnly ? "true" : "false")}'."),
						Invariant($"Information|Current version is {currentVersionNumber}."),
						"Information|Checking locally for a newer upgrade package...",
						"Information|No new local upgrade package available."
					};

					TestUpgraderAndAssertServiceTaskLogs(httpTestHelper, config, expectedLogs, expectedUpgradeVersion, onlineUpgradeVersion);
				}
			}
		}

		public void TestUpgrader_NewOnlineVersionNewerThanNewLocalVersion_LocalUpgradeApplied()
		{
			// Arrange
			var currentVersionNumber = ReleaseInfo.Instance.VersionNumber;
			var localUpgradeVersion = currentVersionNumber.AddRelease(1);
			var localStmUpgradeVersions = new List<(VersionNumber versionNumber, string status, DateTime versionDate)>
			{
				(currentVersionNumber, StmUpgrade.StmUpgradeStatus.CurrentVersion, DateTime.UtcNow),
				(currentVersionNumber.AddRelease(-1), StmUpgrade.StmUpgradeStatus.Ready, DateTime.UtcNow),
				(localUpgradeVersion, StmUpgrade.StmUpgradeStatus.Ready, DateTime.UtcNow)
			};

			Version onlineUpgradeVersion = currentVersionNumber.AddRelease(2).ToVersion();
			Version expectedUpgradeVersion = localUpgradeVersion.ToVersion();

			_ = CreateStmUpgradeVersions_ForTest(localStmUpgradeVersions);

			var configScenarios = new List<ScheduledUpgraderConfig>
			{
				new ScheduledUpgraderConfig("") { AutoDownload = false, PatchOnly = false }
			};

			using (var httpTestHelper = new HttpTestHelper())
			{
				httpTestHelper.Start();

				foreach (ScheduledUpgraderConfig config in configScenarios)
				{
					var expectedLogs = new List<string>
					{
						Invariant($"Information|Auto-Download is set to '{(config.AutoDownload ? "true" : "false")}'."),
						Invariant($"Information|Patch-Only is set to '{(config.PatchOnly ? "true" : "false")}'."),
						Invariant($"Information|Current version is {currentVersionNumber}."),
						"Information|Checking locally for a newer upgrade package...",
						Invariant($"Information|Upgrade package {localUpgradeVersion.ToVersion()} found."),
						Invariant($"Information|Applying upgrade {expectedUpgradeVersion}.")
					};

					TestUpgraderAndAssertServiceTaskLogs(httpTestHelper, config, expectedLogs, expectedUpgradeVersion, onlineUpgradeVersion);
				}
			}
		}

		public void TestUpgrader_NewOnlineVersionNewerThanNewLocalVersion_OnlineUpgradeApplied()
		{
			// Arrange
			var currentVersionNumber = ReleaseInfo.Instance.VersionNumber;
			var localUpgradeVersion = currentVersionNumber.AddRelease(1);
			var localStmUpgradeVersions = new List<(VersionNumber versionNumber, string status, DateTime versionDate)>
			{
				(currentVersionNumber, StmUpgrade.StmUpgradeStatus.CurrentVersion, DateTime.UtcNow),
				(currentVersionNumber.AddRelease(-1), StmUpgrade.StmUpgradeStatus.Ready, DateTime.UtcNow),
				(localUpgradeVersion, StmUpgrade.StmUpgradeStatus.Ready, DateTime.UtcNow)
			};

			Version onlineUpgradeVersion = currentVersionNumber.AddRelease(2).ToVersion();
			Version expectedUpgradeVersion = OnlineVersionPackedForTestDownload;

			_ = CreateStmUpgradeVersions_ForTest(localStmUpgradeVersions);

			var configScenarios = new List<ScheduledUpgraderConfig>
			{
				new ScheduledUpgraderConfig("") { AutoDownload = true, PatchOnly = false }
			};

			using (var httpTestHelper = new HttpTestHelper())
			{
				httpTestHelper.Start();

				foreach (ScheduledUpgraderConfig config in configScenarios)
				{
					var expectedLogs = new List<string>
					{
						Invariant($"Information|Auto-Download is set to '{(config.AutoDownload ? "true" : "false")}'."),
						Invariant($"Information|Patch-Only is set to '{(config.PatchOnly ? "true" : "false")}'."),
						Invariant($"Information|Current version is {currentVersionNumber}."),
						"Information|Checking locally for a newer upgrade package...",
						Invariant($"Information|Upgrade package {localUpgradeVersion.ToVersion()} found."),
						"Information|Checking online for a newer upgrade package...",
						Invariant($"Information|Upgrade package {onlineUpgradeVersion} found, downloading from {httpTestHelper.ServerAddress}testing.edp"),
						Invariant($"Information|Applying upgrade {expectedUpgradeVersion}.")
					};

					TestUpgraderAndAssertServiceTaskLogs(httpTestHelper, config, expectedLogs, expectedUpgradeVersion, onlineUpgradeVersion);
				}
			}
		}

		public void TestUpgrader_NewOnlineVersionNewerThanNewLocalVersion_NoUpgradeApplied()
		{
			// Arrange
			var currentVersionNumber = ReleaseInfo.Instance.VersionNumber;
			var localUpgradeVersion = currentVersionNumber.AddRelease(1);
			var localStmUpgradeVersions = new List<(VersionNumber versionNumber, string status, DateTime versionDate)>
			{
				(currentVersionNumber, StmUpgrade.StmUpgradeStatus.CurrentVersion, DateTime.UtcNow),
				(currentVersionNumber.AddRelease(-1), StmUpgrade.StmUpgradeStatus.Ready, DateTime.UtcNow),
				(localUpgradeVersion, StmUpgrade.StmUpgradeStatus.Ready, DateTime.UtcNow)
			};

			Version onlineUpgradeVersion = currentVersionNumber.AddRelease(2).ToVersion();
			Version expectedUpgradeVersion = null;

			_ = CreateStmUpgradeVersions_ForTest(localStmUpgradeVersions);

			var configScenarios = new List<ScheduledUpgraderConfig>
			{
				new ScheduledUpgraderConfig("") { AutoDownload = false, PatchOnly = true },
				new ScheduledUpgraderConfig("") { AutoDownload = true, PatchOnly = true }
			};

			using (var httpTestHelper = new HttpTestHelper())
			{
				httpTestHelper.Start();

				foreach (ScheduledUpgraderConfig config in configScenarios)
				{
					var expectedLogs = new List<string>
					{
						Invariant($"Information|Auto-Download is set to '{(config.AutoDownload ? "true" : "false")}'."),
						Invariant($"Information|Patch-Only is set to '{(config.PatchOnly ? "true" : "false")}'."),
						Invariant($"Information|Current version is {currentVersionNumber}."),
						"Information|Checking locally for a newer upgrade package...",
						Invariant($"Information|Upgrade package {localUpgradeVersion.ToVersion()} found, not applying because Patch-Only is set to 'true'.")
					};
					if (config.AutoDownload)
					{
						expectedLogs.Add("Information|Checking online for a newer upgrade package...");
						expectedLogs.Add(Invariant($"Information|Upgrade package {onlineUpgradeVersion} found, not downloading because Patch-Only is set to 'true'."));
					}

					TestUpgraderAndAssertServiceTaskLogs(httpTestHelper, config, expectedLogs, expectedUpgradeVersion, onlineUpgradeVersion);
				}
			}
		}

		public void TestUpgrader_NewLocalVersionNewerThanNewOnlineVersion_LocalUpgradeApplied()
		{
			// Arrange
			var currentVersionNumber = ReleaseInfo.Instance.VersionNumber;
			var localUpgradeVersion = currentVersionNumber.AddRelease(2);
			var localStmUpgradeVersions = new List<(VersionNumber versionNumber, string status, DateTime versionDate)>
			{
				(currentVersionNumber, StmUpgrade.StmUpgradeStatus.CurrentVersion, DateTime.UtcNow),
				(currentVersionNumber.AddRelease(-1), StmUpgrade.StmUpgradeStatus.Ready, DateTime.UtcNow),
				(localUpgradeVersion, StmUpgrade.StmUpgradeStatus.Ready, DateTime.UtcNow)
			};

			Version onlineUpgradeVersion = currentVersionNumber.AddRelease(1).ToVersion();
			Version expectedUpgradeVersion = localUpgradeVersion.ToVersion();

			_ = CreateStmUpgradeVersions_ForTest(localStmUpgradeVersions);

			var configScenarios = new List<ScheduledUpgraderConfig>
			{
				new ScheduledUpgraderConfig("") { AutoDownload = false, PatchOnly = false },
				new ScheduledUpgraderConfig("") { AutoDownload = true, PatchOnly = false }
			};

			using (var httpTestHelper = new HttpTestHelper())
			{
				httpTestHelper.Start();

				foreach (ScheduledUpgraderConfig config in configScenarios)
				{
					var expectedLogs = new List<string>
					{
						Invariant($"Information|Auto-Download is set to '{(config.AutoDownload ? "true" : "false")}'."),
						Invariant($"Information|Patch-Only is set to '{(config.PatchOnly ? "true" : "false")}'."),
						Invariant($"Information|Current version is {currentVersionNumber}."),
						"Information|Checking locally for a newer upgrade package...",
						Invariant($"Information|Upgrade package {localUpgradeVersion.ToVersion()} found.")
					};
					if (config.AutoDownload)
					{
						expectedLogs.Add("Information|Checking online for a newer upgrade package...");
						expectedLogs.Add("Information|No new online upgrade package available.");
					}
					expectedLogs.Add($"Information|Applying upgrade {expectedUpgradeVersion}.");

					TestUpgraderAndAssertServiceTaskLogs(httpTestHelper, config, expectedLogs, expectedUpgradeVersion, onlineUpgradeVersion);
				}
			}
		}

		public void TestUpgrader_NewLocalVersionNewerThanNewOnlineVersion_NoUpgradeApplied()
		{
			// Arrange
			var currentVersionNumber = ReleaseInfo.Instance.VersionNumber;
			var localUpgradeVersion = currentVersionNumber.AddRelease(2);
			var localStmUpgradeVersions = new List<(VersionNumber versionNumber, string status, DateTime versionDate)>
			{
				(currentVersionNumber, StmUpgrade.StmUpgradeStatus.CurrentVersion, DateTime.UtcNow),
				(currentVersionNumber.AddRelease(-1), StmUpgrade.StmUpgradeStatus.Ready, DateTime.UtcNow),
				(localUpgradeVersion, StmUpgrade.StmUpgradeStatus.Ready, DateTime.UtcNow)
			};

			Version onlineUpgradeVersion = currentVersionNumber.AddRelease(1).ToVersion();
			Version expectedUpgradeVersion = null;

			_ = CreateStmUpgradeVersions_ForTest(localStmUpgradeVersions);

			var configScenarios = new List<ScheduledUpgraderConfig>
			{
				new ScheduledUpgraderConfig("") { AutoDownload = false, PatchOnly = true },
				new ScheduledUpgraderConfig("") { AutoDownload = true, PatchOnly = true }
			};

			using (var httpTestHelper = new HttpTestHelper())
			{
				httpTestHelper.Start();

				foreach (ScheduledUpgraderConfig config in configScenarios)
				{
					var expectedLogs = new List<string>
					{
						Invariant($"Information|Auto-Download is set to '{(config.AutoDownload ? "true" : "false")}'."),
						Invariant($"Information|Patch-Only is set to '{(config.PatchOnly ? "true" : "false")}'."),
						Invariant($"Information|Current version is {currentVersionNumber}."),
						"Information|Checking locally for a newer upgrade package...",
						Invariant($"Information|Upgrade package {localUpgradeVersion.ToVersion()} found, not applying because Patch-Only is set to 'true'.")
					};
					if (config.AutoDownload)
					{
						expectedLogs.Add("Information|Checking online for a newer upgrade package...");
						expectedLogs.Add(Invariant($"Information|Upgrade package {onlineUpgradeVersion} found, not downloading because Patch-Only is set to 'true'."));
					}

					TestUpgraderAndAssertServiceTaskLogs(httpTestHelper, config, expectedLogs, expectedUpgradeVersion, onlineUpgradeVersion);
				}
			}
		}

		public void TestUpgrader_NewLocalPatchNewOnlineVersion_LocalPatchApplied()
		{
			// Arrange
			var currentVersionNumber = ReleaseInfo.Instance.VersionNumber;
			var localUpgradeVersion = currentVersionNumber.AddPatch(10);
			var localStmUpgradeVersions = new List<(VersionNumber versionNumber, string status, DateTime versionDate)>
			{
				(currentVersionNumber, StmUpgrade.StmUpgradeStatus.CurrentVersion, DateTime.UtcNow),
				(currentVersionNumber.AddRelease(-1), StmUpgrade.StmUpgradeStatus.Ready, DateTime.UtcNow),
				(localUpgradeVersion, StmUpgrade.StmUpgradeStatus.Ready, DateTime.UtcNow)
			};

			Version onlineUpgradeVersion = currentVersionNumber.AddRelease(1).ToVersion();
			Version expectedUpgradeVersion = localUpgradeVersion.ToVersion();

			_ = CreateStmUpgradeVersions_ForTest(localStmUpgradeVersions);

			var configScenarios = new List<ScheduledUpgraderConfig>
			{
				new ScheduledUpgraderConfig("") { AutoDownload = false, PatchOnly = false },
				new ScheduledUpgraderConfig("") { AutoDownload = false, PatchOnly = true },
				new ScheduledUpgraderConfig("") { AutoDownload = true, PatchOnly = true }
			};

			using (var httpTestHelper = new HttpTestHelper())
			{
				httpTestHelper.Start();

				foreach (ScheduledUpgraderConfig config in configScenarios)
				{
					var expectedLogs = new List<string>
					{
						Invariant($"Information|Auto-Download is set to '{(config.AutoDownload ? "true" : "false")}'."),
						Invariant($"Information|Patch-Only is set to '{(config.PatchOnly ? "true" : "false")}'."),
						Invariant($"Information|Current version is {currentVersionNumber}."),
						"Information|Checking locally for a newer upgrade package...",
						Invariant($"Information|Upgrade package {localUpgradeVersion.ToVersion()} found.")
					};
					if (config.AutoDownload)
					{
						expectedLogs.Add("Information|Checking online for a newer upgrade package...");
						expectedLogs.Add(Invariant($"Information|Upgrade package {onlineUpgradeVersion} found, not downloading because Patch-Only is set to 'true'."));
					}
					expectedLogs.Add($"Information|Applying upgrade {expectedUpgradeVersion}.");

					TestUpgraderAndAssertServiceTaskLogs(httpTestHelper, config, expectedLogs, expectedUpgradeVersion, onlineUpgradeVersion);
				}
			}
		}

		public void TestUpgrader_NewLocalPatchNewOnlineVersion_OnlineUpgradeApplied()
		{
			// Arrange
			var currentVersionNumber = ReleaseInfo.Instance.VersionNumber;
			var localUpgradeVersion = currentVersionNumber.AddPatch(10);
			var localStmUpgradeVersions = new List<(VersionNumber versionNumber, string status, DateTime versionDate)>
			{
				(currentVersionNumber, StmUpgrade.StmUpgradeStatus.CurrentVersion, DateTime.UtcNow),
				(currentVersionNumber.AddRelease(-1), StmUpgrade.StmUpgradeStatus.Ready, DateTime.UtcNow),
				(localUpgradeVersion, StmUpgrade.StmUpgradeStatus.Ready, DateTime.UtcNow)
			};

			Version onlineUpgradeVersion = currentVersionNumber.AddRelease(1).ToVersion();
			Version expectedUpgradeVersion = OnlineVersionPackedForTestDownload;

			_ = CreateStmUpgradeVersions_ForTest(localStmUpgradeVersions);

			var configScenarios = new List<ScheduledUpgraderConfig>
			{
				new ScheduledUpgraderConfig("") { AutoDownload = true, PatchOnly = false }
			};

			using (var httpTestHelper = new HttpTestHelper())
			{
				httpTestHelper.Start();

				foreach (ScheduledUpgraderConfig config in configScenarios)
				{
					var expectedLogs = new List<string>
					{
						Invariant($"Information|Auto-Download is set to '{(config.AutoDownload ? "true" : "false")}'."),
						Invariant($"Information|Patch-Only is set to '{(config.PatchOnly ? "true" : "false")}'."),
						Invariant($"Information|Current version is {currentVersionNumber}."),
						"Information|Checking locally for a newer upgrade package...",
						Invariant($"Information|Upgrade package {localUpgradeVersion.ToVersion()} found."),
						"Information|Checking online for a newer upgrade package...",
						Invariant($"Information|Upgrade package {onlineUpgradeVersion} found, downloading from {httpTestHelper.ServerAddress}testing.edp"),
						Invariant($"Information|Applying upgrade {expectedUpgradeVersion}.")
					};

					TestUpgraderAndAssertServiceTaskLogs(httpTestHelper, config, expectedLogs, expectedUpgradeVersion, onlineUpgradeVersion);
				}
			}
		}

		public void TestUpgrader_NewOnlinePatchNewLocalVersion_OnlinePatchApplied()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var testingEdpPath = resourceRetriever.SaveResourceToFile("Enterprise.ServiceManager.Tasks.ScheduledUpgrader.ServiceTasks.Testing.testing.edp");
				var testingEdpPathSize = File.ReadAllBytes(testingEdpPath).Length;

				// Arrange
				var currentVersionNumber = ReleaseInfo.Instance.VersionNumber;
				var localUpgradeVersion = currentVersionNumber.AddRelease(1);
				var localStmUpgradeVersions = new List<(VersionNumber versionNumber, string status, DateTime versionDate)>
			{
				(currentVersionNumber, StmUpgrade.StmUpgradeStatus.CurrentVersion, DateTime.UtcNow),
				(currentVersionNumber.AddRelease(-1), StmUpgrade.StmUpgradeStatus.Ready, DateTime.UtcNow),
				(localUpgradeVersion, StmUpgrade.StmUpgradeStatus.Ready, DateTime.UtcNow)
			};

				Version onlineUpgradeVersion = currentVersionNumber.AddPatch(10).ToVersion();
				Version expectedUpgradeVersion = OnlineVersionPackedForTestDownload;

				_ = CreateStmUpgradeVersions_ForTest(localStmUpgradeVersions);

				var configScenarios = new List<ScheduledUpgraderConfig>
			{
				new ScheduledUpgraderConfig("") { AutoDownload = true, PatchOnly = true }
			};

				using (var httpTestHelper = new HttpTestHelper())
				{
					httpTestHelper.Start();

					foreach (ScheduledUpgraderConfig config in configScenarios)
					{
						var expectedLogs = new List<string>
					{
						Invariant($"Information|Auto-Download is set to '{(config.AutoDownload ? "true" : "false")}'."),
						Invariant($"Information|Patch-Only is set to '{(config.PatchOnly ? "true" : "false")}'."),
						Invariant($"Information|Current version is {currentVersionNumber}."),
						"Information|Checking locally for a newer upgrade package...",
						Invariant($"Information|Upgrade package {localUpgradeVersion.ToVersion()} found, not applying because Patch-Only is set to 'true'."),
						"Information|No new local upgrade package available.",
						"Information|Checking online for a newer upgrade package...",
						Invariant($"Information|Upgrade package {onlineUpgradeVersion} found, downloading from {httpTestHelper.ServerAddress}testing.edp"),
						Invariant($"Information|Downloaded {testingEdpPathSize} of {testingEdpPathSize} bytes"),
						"Information|Uploading package to database",
						Invariant($"Information|Applying upgrade {expectedUpgradeVersion}.")
					};

						TestUpgraderAndAssertServiceTaskLogs(httpTestHelper, config, expectedLogs, expectedUpgradeVersion, onlineUpgradeVersion);
					}
				}
			}
		}

		public void TestUpgrader_NewOnlinePatchNewLocalVersion_LocalUpgradeApplied()
		{
			// Arrange
			var currentVersionNumber = ReleaseInfo.Instance.VersionNumber;
			var localUpgradeVersion = currentVersionNumber.AddRelease(1);
			var localStmUpgradeVersions = new List<(VersionNumber versionNumber, string status, DateTime versionDate)>
			{
				(currentVersionNumber, StmUpgrade.StmUpgradeStatus.CurrentVersion, DateTime.UtcNow),
				(currentVersionNumber.AddRelease(-1), StmUpgrade.StmUpgradeStatus.Ready, DateTime.UtcNow),
				(localUpgradeVersion, StmUpgrade.StmUpgradeStatus.Ready, DateTime.UtcNow)
			};

			Version onlineUpgradeVersion = currentVersionNumber.AddPatch(10).ToVersion();
			Version expectedUpgradeVersion = localUpgradeVersion.ToVersion();

			_ = CreateStmUpgradeVersions_ForTest(localStmUpgradeVersions);

			var configScenarios = new List<ScheduledUpgraderConfig>
			{
				new ScheduledUpgraderConfig("") { AutoDownload = false, PatchOnly = false },
				new ScheduledUpgraderConfig("") { AutoDownload = true, PatchOnly = false }
			};

			using (var httpTestHelper = new HttpTestHelper())
			{
				httpTestHelper.Start();

				foreach (ScheduledUpgraderConfig config in configScenarios)
				{
					var expectedLogs = new List<string>
					{
						Invariant($"Information|Auto-Download is set to '{(config.AutoDownload ? "true" : "false")}'."),
						Invariant($"Information|Patch-Only is set to '{(config.PatchOnly ? "true" : "false")}'."),
						Invariant($"Information|Current version is {currentVersionNumber}."),
						"Information|Checking locally for a newer upgrade package...",
						Invariant($"Information|Upgrade package {localUpgradeVersion.ToVersion()} found.")
					};
					if (config.AutoDownload)
					{
						expectedLogs.Add("Information|Checking online for a newer upgrade package...");
						expectedLogs.Add("Information|No new online upgrade package available.");
					}
					expectedLogs.Add($"Information|Applying upgrade {expectedUpgradeVersion}.");

					TestUpgraderAndAssertServiceTaskLogs(httpTestHelper, config, expectedLogs, expectedUpgradeVersion, onlineUpgradeVersion);
				}
			}
		}

		public void TestUpgrader_NewOnlinePatchNewLocalVersion_NoUpgradeApplied()
		{
			// Arrange
			var currentVersionNumber = ReleaseInfo.Instance.VersionNumber;
			var localUpgradeVersion = currentVersionNumber.AddRelease(1);
			var localStmUpgradeVersions = new List<(VersionNumber versionNumber, string status, DateTime versionDate)>
			{
				(currentVersionNumber, StmUpgrade.StmUpgradeStatus.CurrentVersion, DateTime.UtcNow),
				(currentVersionNumber.AddRelease(-1), StmUpgrade.StmUpgradeStatus.Ready, DateTime.UtcNow),
				(localUpgradeVersion, StmUpgrade.StmUpgradeStatus.Ready, DateTime.UtcNow)
			};

			Version onlineUpgradeVersion = currentVersionNumber.AddPatch(10).ToVersion();
			Version expectedUpgradeVersion = null;

			_ = CreateStmUpgradeVersions_ForTest(localStmUpgradeVersions);

			var configScenarios = new List<ScheduledUpgraderConfig>
			{
				new ScheduledUpgraderConfig("") { AutoDownload = false, PatchOnly = true }
			};

			using (var httpTestHelper = new HttpTestHelper())
			{
				httpTestHelper.Start();

				foreach (ScheduledUpgraderConfig config in configScenarios)
				{
					var expectedLogs = new List<string>
					{
						Invariant($"Information|Auto-Download is set to '{(config.AutoDownload ? "true" : "false")}'."),
						Invariant($"Information|Patch-Only is set to '{(config.PatchOnly ? "true" : "false")}'."),
						Invariant($"Information|Current version is {currentVersionNumber}."),
						"Information|Checking locally for a newer upgrade package...",
						Invariant($"Information|Upgrade package {localUpgradeVersion.ToVersion()} found, not applying because Patch-Only is set to 'true'."),
						"Information|No new local upgrade package available."
					};

					TestUpgraderAndAssertServiceTaskLogs(httpTestHelper, config, expectedLogs, expectedUpgradeVersion, onlineUpgradeVersion);
				}
			}
		}

		public void TestUpgrader_NoUpgradesAvailable_NoUpgradeApplied()
		{
			// Arrange
			var currentVersionNumber = ReleaseInfo.Instance.VersionNumber;
			var localStmUpgradeVersions = new List<(VersionNumber versionNumber, string status, DateTime versionDate)>
			{
				(currentVersionNumber, StmUpgrade.StmUpgradeStatus.CurrentVersion, DateTime.UtcNow),
				(currentVersionNumber.AddRelease(-1), StmUpgrade.StmUpgradeStatus.Ready, DateTime.UtcNow)
			};

			Version expectedUpgradeVersion = null;

			_ = CreateStmUpgradeVersions_ForTest(localStmUpgradeVersions);

			var configScenarios = new List<ScheduledUpgraderConfig>
			{
				new ScheduledUpgraderConfig("") { AutoDownload = false, PatchOnly = false },
				new ScheduledUpgraderConfig("") { AutoDownload = false, PatchOnly = true },
				new ScheduledUpgraderConfig("") { AutoDownload = true, PatchOnly = false },
				new ScheduledUpgraderConfig("") { AutoDownload = true, PatchOnly = true }
			};

			using (var httpTestHelper = new HttpTestHelper())
			{
				httpTestHelper.Start();

				foreach (ScheduledUpgraderConfig config in configScenarios)
				{
					var expectedLogs = new List<string>
					{
						Invariant($"Information|Auto-Download is set to '{(config.AutoDownload ? "true" : "false")}'."),
						Invariant($"Information|Patch-Only is set to '{(config.PatchOnly ? "true" : "false")}'."),
						Invariant($"Information|Current version is {currentVersionNumber}."),
						"Information|Checking locally for a newer upgrade package...",
						"Information|No new local upgrade package available."
					};
					if (config.AutoDownload)
					{
						expectedLogs.Add("Information|Checking online for a newer upgrade package...");
						expectedLogs.Add("Information|No new online upgrade package available.");
					}

					TestUpgraderAndAssertServiceTaskLogs(httpTestHelper, config, expectedLogs, expectedUpgradeVersion);
				}
			}
		}

		public void TestUpgrader_SkipDownloadingNewOnlineVersionForRolledOutToCWCloud_LocalUpgradeApplied()
		{
			// Arrange
			var currentVersionNumber = ReleaseInfo.Instance.VersionNumber;
			var localUpgradeVersion = currentVersionNumber.AddRelease(1);
			var localStmUpgradeVersions = new List<(VersionNumber versionNumber, string status, DateTime versionDate)>
			{
				(currentVersionNumber, StmUpgrade.StmUpgradeStatus.CurrentVersion, DateTime.UtcNow),
				(currentVersionNumber.AddRelease(-1), StmUpgrade.StmUpgradeStatus.Ready, DateTime.UtcNow),
				(localUpgradeVersion, StmUpgrade.StmUpgradeStatus.Ready, DateTime.UtcNow)
			};

			var onlineUpgradeVersion = currentVersionNumber.AddRelease(2);

			_ = CreateStmUpgradeVersions_ForTest(localStmUpgradeVersions);

			var config = new ScheduledUpgraderConfig("") { AutoDownload = true, PatchOnly = false };

			var responses = new UpgradePackageUrlResponse
			{
				ResponseClass = UpgradePackageUrlResponse.ResponseClassType.Success,
				URL = "",
				ErrorMessage = $"The package {onlineUpgradeVersion} has been rolled out to CargoWise Cloud",
				VersionNumber = onlineUpgradeVersion.ToString()
			};

			var mockClient = new Mock<IUpgradePackageService>(MockBehavior.Strict);
			mockClient.Setup(a => a.GetPackageUrl(It.IsAny<UpgradePackageUrlRequest>()))
			.Returns(responses);

			using (var httpTestHelper = new HttpTestHelper())
			{
				httpTestHelper.Start();

				var expectedLogs = new List<string>
					{
						Invariant($"Information|Auto-Download is set to '{(config.AutoDownload ? "true" : "false")}'."),
						Invariant($"Information|Patch-Only is set to '{(config.PatchOnly ? "true" : "false")}'."),
						Invariant($"Information|Current version is {currentVersionNumber}."),
						"Information|Checking locally for a newer upgrade package...",
						Invariant($"Information|Upgrade package {localUpgradeVersion.ToVersion()} found."),
						"Information|Checking online for a newer upgrade package...",
						Invariant($"Information|The upgrade package {onlineUpgradeVersion.ToVersion()} was found, but downloading skipped: The package {onlineUpgradeVersion.ToVersion()} has been rolled out to CargoWise Cloud"),
						Invariant($"Information|Applying upgrade {localUpgradeVersion.ToVersion()}."),
					};

				var serviceTask = new UpgraderServiceTaskForTest(httpTestHelper, onlineUpgradeVersion.ToVersion());
				serviceTask.ConfigString = config.ConfigString;
				serviceTask.IsCorruptFile = false;
				serviceTask.OverwrittenUpgradePackageServiceClient = mockClient.Object;

				var logger = new Mock<ILogger>();
				var actualLogs = new List<string>();
				logger
					.Setup(x => x.Log(It.IsAny<LogType>(), It.IsAny<string>()))
					.Callback<LogType, string>((type, message) => actualLogs.Add(Invariant($"{type}|{message}")));
				serviceTask.ServiceLogger = logger.Object;

				// Act
				serviceTask.RunTask();

				// Assert
				AssertEquals(expectedLogs.Count, expectedLogs.Count);
				AssertEquals(string.Join(System.Environment.NewLine, expectedLogs), string.Join(System.Environment.NewLine, actualLogs));
			}
		}

		void TestUpgraderAndAssertServiceTaskLogs(HttpTestHelper httpTestHelper, ScheduledUpgraderConfig config, List<string> expectedLogs, Version expectedUpgradeVersion, Version onlineUpgradeVersion = null, bool isCorruptFile = false)
		{
			try
			{
				UpgraderServiceTaskForTest serviceTask;

				if (onlineUpgradeVersion == null)
				{
					serviceTask = new UpgraderServiceTaskForTest(httpTestHelper);
				}
				else
				{
					serviceTask = new UpgraderServiceTaskForTest(httpTestHelper, onlineUpgradeVersion);
				}
				serviceTask.ConfigString = config.ConfigString;
				serviceTask.IsCorruptFile = isCorruptFile;

				var logger = new Mock<ILogger>();
				var actualLogs = new List<string>();
				logger
					.Setup(x => x.Log(It.IsAny<LogType>(), It.IsAny<string>()))
					.Callback<LogType, string>((type, message) => actualLogs.Add(Invariant($"{type}|{message}")));
				serviceTask.ServiceLogger = logger.Object;

				// Act
				serviceTask.RunTask();

				// Assert
				CombineAssertions($@"
Test(autoDownload: '{config.AutoDownload}', patchOnly: '{config.PatchOnly}') failed:

expectedLogs:
-------------
{string.Join(System.Environment.NewLine, expectedLogs)}

actualLogs:
-------------
{string.Join(System.Environment.NewLine, actualLogs)}
", () =>
				{
					if (expectedUpgradeVersion == null)
					{
						AssertNull(serviceTask.upgradeToApply);
					}
					else
					{
						AssertEquals(expectedUpgradeVersion, serviceTask.upgradeToApply.Version);
					}
					AssertEquals(expectedLogs.Count, actualLogs.Intersect(expectedLogs).Count());
				});
			}
			finally
			{
				DeleteIfExists(Path.Combine(httpTestHelper.LocalDirectory, "testing.edp"));
			}
		}

		[ExpectNoExceptions]
		public void TestUpgradeHandlesSqlLockLostExceptionAfterFailedUpgrade()
		{
			var upgrade1 = Factory.New<StmUpgrade>();
			var upgrade2 = Factory.New<StmUpgrade>();
			var upgrade3 = Factory.New<StmUpgrade>();
			var versionNumber = ReleaseInfo.Instance.VersionNumber;
			upgrade1.VersionNumber = versionNumber.AddRelease(-1);
			upgrade1.SZ_Status = StmUpgrade.StmUpgradeStatus.Ready;
			upgrade1.SZ_ExeVersionDate = DateTime.UtcNow;
			upgrade2.VersionNumber = versionNumber;
			upgrade2.SZ_Status = StmUpgrade.StmUpgradeStatus.CurrentVersion;
			upgrade2.SZ_ExeVersionDate = DateTime.UtcNow;
			upgrade3.VersionNumber = versionNumber.AddRelease(1);
			upgrade3.SZ_Status = StmUpgrade.StmUpgradeStatus.Ready;
			upgrade3.SZ_ExeVersionDate = DateTime.UtcNow;
			Factory.Save();

			using (var httpTestHelper = new HttpTestHelper())
			{
				httpTestHelper.Start();
				var serviceTask = new UpgraderServiceTaskForTestBase(httpTestHelper);
				var logger = new Mock<ILogger>();
				logger.Setup(m => m.Log(LogType.Error, It.IsAny<string>())).Throws(new TargetInvocationException(new SqlLockLostException()));
				serviceTask.ServiceLogger = logger.Object;
				serviceTask.RunUpgradeProcessForTest = () => new Tuple<bool, int>(true, -1);
				serviceTask.RunTask();
			}
		}

		public void TestUpgrader_NewOnlinePackage_DownloadUpgradeShouldLogUrl()
		{
			var stmUpgrade = Factory.New<StmUpgrade>();
			var currentVersion = ReleaseInfo.Instance.VersionNumber;
			stmUpgrade.VersionNumber = currentVersion;
			stmUpgrade.SZ_Status = StmUpgrade.StmUpgradeStatus.CurrentVersion;
			stmUpgrade.SZ_ExeVersionDate = DateTime.UtcNow;
			Factory.Save();

			using (var httpTestHelper = new HttpTestHelper())
			{
				httpTestHelper.Start();

				var upgradeVersion = currentVersion.AddMinor(1).ToVersion();
				var serviceTask = new UpgraderServiceTaskForTest(httpTestHelper, upgradeVersion);
				serviceTask.ConfigString = new ScheduledUpgraderConfig("") { AutoDownload = true }.ConfigString;

				var logger = new Mock<ILogger>();
				var logs = new List<string>();
				logger.Setup(x => x.Log(It.IsAny<LogType>(), It.IsAny<string>())).Callback<LogType, string>((type, message) => logs.Add(type + "|" + message));
				serviceTask.ServiceLogger = logger.Object;

				serviceTask.RunTask();

				AssertEquals(OnlineVersionPackedForTestDownload, serviceTask.upgradeToApply.Version);

				Assert(logs.Any(x => x == $"Information|Upgrade package {upgradeVersion} found, downloading from {httpTestHelper.ServerAddress}testing.edp"));
			}
		}

		public void TestUpgrader_NewOnlinePackage_DownloadUpgradePackageFails()
		{
			var versionNumber = ReleaseInfo.Instance.VersionNumber;

			var stmUpgrade = Factory.New<StmUpgrade>();
			stmUpgrade.VersionNumber = versionNumber;
			stmUpgrade.SZ_Status = StmUpgrade.StmUpgradeStatus.CurrentVersion;
			stmUpgrade.SZ_ExeVersionDate = DateTime.UtcNow;
			Factory.Save();

			using (var httpTestHelper = new HttpTestHelper())
			{
				var responses = new[]
				{
					new UpgradePackageUrlResponse { ResponseClass = UpgradePackageUrlResponse.ResponseClassType.Error, ErrorMessage = "Download Error (1)" },
					new UpgradePackageUrlResponse { ResponseClass = UpgradePackageUrlResponse.ResponseClassType.Error, ErrorMessage = "Download Error (2)" },
					new UpgradePackageUrlResponse { ResponseClass = UpgradePackageUrlResponse.ResponseClassType.Error, ErrorMessage = "Download Error (3)" },
					new UpgradePackageUrlResponse { ResponseClass = UpgradePackageUrlResponse.ResponseClassType.Error, ErrorMessage = "Download Error (4)" }
				};

				httpTestHelper.Start();
				var upgradeVersion = versionNumber.AddMinor(1).ToVersion();
				var serviceTask = new UpgraderServiceTaskForTest(httpTestHelper, upgradeVersion);
				serviceTask.ConfigString = new ScheduledUpgraderConfig("") { AutoDownload = true }.ConfigString;

				var mockClient = new Mock<IUpgradePackageService>(MockBehavior.Strict);
				mockClient.SetupSequence(a => a.GetPackageUrl(It.IsAny<UpgradePackageUrlRequest>()))
					.Returns(responses[0])
					.Returns(responses[1])
					.Returns(responses[2])
					.Returns(responses[3]);
				serviceTask.OverwrittenUpgradePackageServiceClient = mockClient.Object;

				var logger = new Mock<ILogger>();
				var logs = new List<string>();
				logger.Setup(x => x.Log(It.IsAny<LogType>(), It.IsAny<string>())).Callback<LogType, string>((type, message) => logs.Add(type + "|" + message));
				serviceTask.ServiceLogger = logger.Object;

				serviceTask.RunTask();

				AssertNull(serviceTask.upgradeToApply);

				Assert(logs.Any(x => x == "Information|Auto-Download is set to 'true'."));
				Assert(logs.Any(x => x == "Information|Patch-Only is set to 'false'."));
				Assert(logs.Any(x => x == $"Information|Current version is {versionNumber}."));
				Assert(logs.Any(x => x == "Information|Checking locally for a newer upgrade package..."));
				Assert(logs.Any(x => x == "Information|No new local upgrade package available."));
				Assert(logs.Any(x => x == "Information|Checking online for a newer upgrade package..."));
				Assert(logs.Any(x => x == "Warning|Getting package url failed with 'Download Error (1)'. Retrying..."));
				Assert(logs.Any(x => x == "Warning|Getting package url failed with 'Download Error (2)'. Retrying..."));
				Assert(logs.Any(x => x == "Warning|Getting package url failed with 'Download Error (3)'. Retrying..."));
				Assert(logs.Any(x => x == "Error|Error checking for latest upgrade package online: Download Error (4)"));
				mockClient.VerifyAll();
				logger.VerifyAll();
			}
		}
#if NETFRAMEWORK

		public void TestCommandLineIsSuitableForWiseCloudOrchestrator()
		{
			// Arrange
			var upgradeInfo = UploadUpgradePack();
			var loggerMock = new Mock<ILogger>();
			var task = new UpgraderServiceTask { ServiceLogger = loggerMock.Object };

			using (InstallationEnvironmentForTest.TempDirectoryForTest())
			{
				try
				{
					// Act
					task.DoUpgrade(upgradeInfo);
				}
				finally
				{
					if (task.UpgradeProcess != null)
					{
						try
						{
							if (!task.UpgradeProcess.HasExited)
							{
								task.UpgradeProcess.Kill();
							}
							task.UpgradeProcess.WaitForExit();
						}
						catch (InvalidOperationException)
						{
							// Handle gracefully when the process has already exited
						}
						finally
						{
							task.UpgradeProcess.Dispose();
						}
					}
				}
			}

			// Assert
			AssertNoExceptionThrown(() =>
			{
				loggerMock.Verify(logger => logger.Log(LogType.Debug, It.IsAny<string>()), Times.Once);
				loggerMock.Verify(logger => logger.Log(
						LogType.Debug,
						It.IsRegex($@".?CargoWise.WindowsDesktop.exe\s{Db.ServerName.Replace("\\", "\\\\")}\s{Db.DatabaseName}\s.*-Upgrade:[0-9a-f]{{8}}-[0-9a-f]{{4}}-[0-9a-f]{{4}}-[0-9a-f]{{4}}-[0-9a-f]{{12}}", RegexOptions.IgnoreCase)),
					Times.Once);
			});
		}

		[TestDate(2014, 10, 11, 21, 0, 0)]
		public void TestScheduleIsNotUpdatedDuringUpgradeExecution()
		{
			var schedule = Factory.New<ServiceTaskSchedule>();
			schedule.S5_ScheduleType = "UPG";
			schedule.S5_StartDate = ZDateTime.UtcNow;
			schedule.S5_TaskPeriod = "W";
			schedule.S5_TaskPeriodCount = 1;
			schedule.S5_DayList = "NNNNNNY";
			schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow;
			Factory.Save();
			UploadUpgradePack();

			using (InstallationEnvironmentForTest.TempDirectoryForTest())
			{
				var loggerFactoryMock = new Mock<ILoggerFactory>();
				loggerFactoryMock
					.Setup(x => x.NewServiceTaskLogger(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
					.Returns(new TestServiceLogger());

				var task = new UpgraderServiceTask() { ServiceLogger = loggerFactoryMock.Object.NewServiceTaskLogger(Db.ServerName, Db.DatabaseName, "test") };

				var monitoringTask = Task.Run(async () =>
				{
					while (task.UpgradeProcess == null)
					{
						await Task.Delay(100); // Non-blocking wait
					}

					await Task.Delay(TimeSpan.FromSeconds(2)); // Give process time before verification
				});

				try
				{
					task.RunTask();
					Assert("Next run time should not be changed by running a task", schedule.S5_NextScheduledPrintRunTimeUtc == ZDateTime.UtcNow);
				}
				finally
				{
					if (task.UpgradeProcess != null)
					{
						try
						{
							// Check if the process is still running before attempting to kill it
							if (!task.UpgradeProcess.HasExited)
							{
								task.UpgradeProcess.Kill();
							}
							task.UpgradeProcess.WaitForExit();
						}
						catch (InvalidOperationException)
						{
							// Handle gracefully when the process has already exited
						}
						finally
						{
							task.UpgradeProcess.Dispose();
						}
					}

					monitoringTask.Wait();
				}
			}
		}
#endif
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
#if NETFRAMEWORK
		UpgradeInfo UploadUpgradePack()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var testingEdpPath = resourceRetriever.SaveResourceToFile("Enterprise.ServiceManager.Tasks.ScheduledUpgrader.ServiceTasks.Testing.testing.edp");
				var upgradeManager = UpgradeManagerFactory.NewUpgradeManager();
				upgradeManager.UploadUpgradePackage(testingEdpPath, new Version(1, 0, 0, 0), DateTime.UtcNow, "CUR", "", null);
				using (var tempFile = TempFile.New())
				using (var packageTempDir = new TempDirectory())
				{
					EdpFile.Unpack(testingEdpPath, packageTempDir);
					var appDir = Path.Combine(packageTempDir, "Distribution", "Application");
					var version = new Version(15, 1, 1, 1);
					CreateSleepyExe(Path.Combine(appDir, ExeFileNames.CargoWiseOneExeForVersionInfo), version.ToString());
					CreateSleepyExe(Path.Combine(appDir, ExeFileNames.CargoWiseWindowsDesktopExe), version.ToString());
					ZipCompression.Zip(packageTempDir.DirectoryName, tempFile.Filename);
					Db.Connection.Command("truncate table StmUpgrade");
					return upgradeManager.UploadUpgradePackage(tempFile.Filename, version, DateTime.UtcNow, "RDY", "", null);
				}
			}
		}
#endif
#if NETFRAMEWORK
		const string AppName = "sleepy";
		public static void CreateSleepyExe(string path, string version)
		{
			string sourceCode = string.Format(@"
        using System;
        using System.Threading;

        [assembly: System.Reflection.AssemblyFileVersion(""{0}"")]
        public static class Program
        {{
            public static void Main(string[] cmd)
            {{
                System.Threading.Thread.Sleep(System.TimeSpan.FromSeconds(3)); // Reduce sleep
            }}
        }}", version);

			var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);

			var compilation = CSharpCompilation.Create(
				assemblyName: AppName,
				syntaxTrees: new[] { syntaxTree },
				references: new[]
				{
						MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
				},
				options: new CSharpCompilationOptions(OutputKind.ConsoleApplication)
			);
			using (var file = File.Open(path,FileMode.Create))
			using (var win32resStream = compilation.CreateDefaultWin32Resources(
																			versionResource: true,
																			noManifest: false,
																			manifestContents: null,
																			iconInIcoFormat: null))
			{
				var output = compilation.Emit(peStream: file, win32Resources: win32resStream);
			}
		}
#endif

		class UpgraderServiceTaskForTestBase : UpgraderServiceTask
		{
			public UpgraderServiceTaskForTestBase(HttpTestHelper httpTestHelper)
				: this(httpTestHelper, null)
			{
			}

			public bool IsCorruptFile;

			public UpgraderServiceTaskForTestBase(HttpTestHelper httpTestHelper, Version latestAvilableVersion)
			{
				var loggerFactoryMock = new Mock<ILoggerFactory>();
				loggerFactoryMock
					.Setup(x => x.NewServiceTaskLogger(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
					.Returns(new TestServiceLogger());

				this.httpTestHelper = httpTestHelper;
				this.ServiceLogger = loggerFactoryMock.Object.NewServiceTaskLogger(Db.ServerName, Db.DatabaseName, "test");
				this.latestAvilableVersion = latestAvilableVersion;
			}

			internal override IUpgradePackageService UpgradePackageServiceClient
			{
				get
				{
					return new UpgradePackageServiceForTest(httpTestHelper, latestAvilableVersion) { OverridenUpgradePackageResponseFileName = this.OverridenUpgradePackageResponseFileName, IsCorruptFile = IsCorruptFile };
				}
			}

			readonly HttpTestHelper httpTestHelper;
			readonly Version latestAvilableVersion;
			public string OverridenUpgradePackageResponseFileName { get; set; }

			class UpgradePackageServiceForTest : IUpgradePackageService
			{
				internal UpgradePackageServiceForTest(HttpTestHelper httpTestHelper, Version latestAvilableVersion)
				{
					this.httpTestHelper = httpTestHelper;
					this.latestAvilableVersion = latestAvilableVersion;
				}

				readonly HttpTestHelper httpTestHelper;
				readonly Version latestAvilableVersion;
				public bool IsCorruptFile;

				public UpgradePackageUrlResponse GetPackageUrl(UpgradePackageUrlRequest request)
				{
					using (var resourceRetriever = new EmbeddedResourceRetriever())
					{
						var testingEdpPath = resourceRetriever.SaveResourceToFile("Enterprise.ServiceManager.Tasks.ScheduledUpgrader.ServiceTasks.Testing.testing.edp");

						var response = new UpgradePackageUrlResponse();
						if (latestAvilableVersion != null)
						{
							response = new UpgradePackageUrlResponse();
							response.ResponseClass = UpgradePackageUrlResponse.ResponseClassType.Success;
							response.VersionNumber = latestAvilableVersion.ToString();
							if (!IsCorruptFile)
							{
								File.Copy(testingEdpPath, Path.Combine(httpTestHelper.LocalDirectory, "testing.edp"));
							}
							else
							{
								File.WriteAllBytes(Path.Combine(httpTestHelper.LocalDirectory, "testing.edp"), new byte[] { 1, 2, 3 });
							}
							new FileInfo(Path.Combine(httpTestHelper.LocalDirectory, "testing.edp")).IsReadOnly = false;
							response.URL = new Uri(httpTestHelper.ServerAddress, OverridenUpgradePackageResponseFileName ?? "testing.edp").ToString();
						}
						else
						{
							response.URL = string.Empty;
							response.ResponseClass = UpgradePackageUrlResponse.ResponseClassType.Success;
							response.VersionNumber = "0.0.0.0";
							response.ErrorMessage = "No available release build found";
						}
						return response;
					}
				}

				public string OverridenUpgradePackageResponseFileName { get; set; }
			}
		}

		public void TestUpgrader_OnlinePackageDownloadError_RetryDownload()
		{
			var versionNumber = ReleaseInfo.Instance.VersionNumber;

			var stmUpgrade = Factory.New<StmUpgrade>();
			stmUpgrade.VersionNumber = versionNumber;
			stmUpgrade.SZ_Status = StmUpgrade.StmUpgradeStatus.CurrentVersion;
			stmUpgrade.SZ_ExeVersionDate = DateTime.UtcNow;
			Factory.Save();

			using (var httpTestHelper = new HttpTestHelper())
			{
				var responses = new[]
				{
					new UpgradePackageUrlResponse { ResponseClass = UpgradePackageUrlResponse.ResponseClassType.Error, ErrorMessage = "Error" },
					new UpgradePackageUrlResponse { ResponseClass = UpgradePackageUrlResponse.ResponseClassType.Failed, ErrorMessage = "Failed" },
					new UpgradePackageUrlResponse { ResponseClass = UpgradePackageUrlResponse.ResponseClassType.Success, VersionNumber = versionNumber.ToString() },
				};
				httpTestHelper.Start();

				var upgradeVersion = versionNumber.AddMinor(1).ToVersion();
				var serviceTask = new UpgraderServiceTaskForTest(httpTestHelper, upgradeVersion);
				serviceTask.ConfigString = new ScheduledUpgraderConfig("") { AutoDownload = true }.ConfigString;
				var mockClient = new Mock<IUpgradePackageService>(MockBehavior.Strict);
				mockClient.SetupSequence(a => a.GetPackageUrl(It.IsAny<UpgradePackageUrlRequest>()))
					.Returns(responses[0])
					.Returns(responses[1])
					.Returns(responses[2]);
				serviceTask.OverwrittenUpgradePackageServiceClient = mockClient.Object;
				var logger = new TestServiceLogger();
				var retryPolicy = new UpgradeRetryPolicyForTest(logger);
				serviceTask.OverridenUpgradeRetryPolicy = retryPolicy;

				serviceTask.RunTask();
				AssertEquals("We're expecting the Error response to have been thrown by the retry policy.", responses[0], ((UpgradeRetryException)retryPolicy.LastThrownException).Response);
				AssertEquals("Warning|Getting package url failed with 'Error'. Retrying...", logger.ToString().Trim());

				logger.ClearLog();
				retryPolicy.ResetException();
				serviceTask.RunTask();
				AssertEquals("We're expecting the Failed response to have been thrown by the retry policy.", responses[1], ((UpgradeRetryException)retryPolicy.LastThrownException).Response);
				AssertEquals("Warning|Getting package url failed with 'Failed'. Retrying...", logger.ToString().Trim());

				logger.ClearLog();
				retryPolicy.ResetException();
				serviceTask.RunTask();
				AssertNull("We're expecting it to succeed. There should be no exception.", retryPolicy.LastThrownException);
				AssertEquals(string.Empty, logger.ToString());
				mockClient.VerifyAll();

				retryPolicy.ResetException();
				var blahException = new Exception("blah");
				mockClient = new Mock<IUpgradePackageService>(MockBehavior.Strict);
				mockClient.Setup(m => m.GetPackageUrl(It.IsAny<UpgradePackageUrlRequest>())).Throws(blahException);
				serviceTask.OverwrittenUpgradePackageServiceClient = mockClient.Object;
				serviceTask.RunTask();
				AssertEquals("We expect that the blah exception was thrown, not the response fail exception.", blahException, retryPolicy.LastThrownException);
				AssertEquals("Warning|Getting package url failed with 'blah'. Retrying...", logger.ToString().Trim());
				mockClient.VerifyAll();
			}
		}

		public void TestRetryPolicy_FailThrough()
		{
			var logger = new TestServiceLogger();
			var policy = new UpgraderServiceTask.UpgradeRetryPolicy(logger);
			var finalBlah = new InvalidOperationException("blah4");
			var throwList = new[]
			{
				new UpgradeRetryException(new UpgradePackageUrlResponse() { ErrorMessage = "blah1" }),
				new UpgradeRetryException(new UpgradePackageUrlResponse() { ErrorMessage = "blah2" }),
				new Exception("blah3"),
				finalBlah
			};
			var throwCount = 0;
			var exception = AssertExceptionThrown<InvalidOperationException>(() => policy.ExecuteAction(() =>
			{
				throw throwList[throwCount++];
			}));
			AssertEquals(finalBlah, exception);
			AssertEquals(@"Warning|Getting package url failed with 'blah1'. Retrying...
Warning|Getting package url failed with 'blah2'. Retrying...
Warning|Getting package url failed with 'blah3'. Retrying...", logger.ToString().Trim());

			AssertEquals(UpgradePackageUrlResponse.ResponseClassType.Error, policy.Response.ResponseClass);
			AssertEquals("blah4", policy.Response.ErrorMessage);
		}

		public void TestRetryPolicy_FailTwiceThenSucceed()
		{
			var logger = new TestServiceLogger();
			var policy = new UpgraderServiceTask.UpgradeRetryPolicy(logger);
			var throwCount = 0;
			policy.ExecuteAction(() =>
			{
				if (throwCount > 1)
				{
					return;
				}
				throwCount++;
				throw new Exception($"blahblah{throwCount}");
			});
			AssertEquals(
@"Warning|Getting package url failed with 'blahblah1'. Retrying...
Warning|Getting package url failed with 'blahblah2'. Retrying..."
				, logger.ToString().Trim());
		}

		public void TestFileDownloaderWithRetry_Failed()
		{
			var currentVersionNumber = ReleaseInfo.Instance.VersionNumber;
			var stmUpgrade = Factory.New<StmUpgrade>();
			var currentVersion = ReleaseInfo.Instance.VersionNumber;
			stmUpgrade.VersionNumber = currentVersion;
			stmUpgrade.SZ_Status = StmUpgrade.StmUpgradeStatus.CurrentVersion;
			stmUpgrade.SZ_ExeVersionDate = DateTime.UtcNow;
			Factory.Save();

			using (var httpTestHelper = new HttpTestHelper())
			{
				httpTestHelper.Start();

				var upgradeVersion = currentVersion.AddMinor(1).ToVersion();
				var serviceTask = new UpgraderServiceTaskForTest(httpTestHelper, upgradeVersion) { OverridenUpgradePackageResponseFileName = "testing2.edp" };
				serviceTask.ConfigString = new ScheduledUpgraderConfig("") { AutoDownload = true }.ConfigString;

				var logger = new Mock<ILogger>();
				var logs = new List<string>();
				logger.Setup(x => x.Log(It.IsAny<LogType>(), It.IsAny<string>())).Callback<LogType, string>((type, message) => logs.Add(type + "|" + message));
				serviceTask.ServiceLogger = logger.Object;

				serviceTask.RunTask();
				AssertEquals($@"Information|Auto-Download is set to 'true'.
Information|Patch-Only is set to 'false'.
Information|Current version is {currentVersionNumber}.
Information|Checking locally for a newer upgrade package...
Information|No new local upgrade package available.
Information|Checking online for a newer upgrade package...
Information|Upgrade package {upgradeVersion} found, downloading from http://localhost:{httpTestHelper.Port}/testing2.edp
Error|Error downloading upgrade package: The remote server returned an error: (404) Not Found.
Error|Error downloading upgrade package: The remote server returned an error: (404) Not Found.
Error|Error downloading upgrade package: The remote server returned an error: (404) Not Found.
Error|Error downloading upgrade package: The remote server returned an error: (404) Not Found.
Error|Failed to download the upgrade package.", string.Join("\r\n", logs));
				AssertNull(serviceTask.upgradeToApply);
			}
		}

		public void TestFileDownloaderWithRetry_Resumed()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var testingEdpPath = resourceRetriever.SaveResourceToFile("Enterprise.ServiceManager.Tasks.ScheduledUpgrader.ServiceTasks.Testing.testing.edp");
				var testingEdpPathSize = File.ReadAllBytes(testingEdpPath).Length;

				var currentVersionNumber = ReleaseInfo.Instance.VersionNumber;
				var expectedUpgradeVersion = OnlineVersionPackedForTestDownload;
				var stmUpgrade = Factory.New<StmUpgrade>();
				var currentVersion = ReleaseInfo.Instance.VersionNumber;
				stmUpgrade.VersionNumber = currentVersion;
				stmUpgrade.SZ_Status = StmUpgrade.StmUpgradeStatus.CurrentVersion;
				stmUpgrade.SZ_ExeVersionDate = DateTime.UtcNow;
				Factory.Save();

				using (var httpTestHelper = new HttpTestHelper())
				{
					httpTestHelper.Start();
					httpTestHelper.RequestCompleted += (_, __) =>
					{
						if (httpTestHelper.RequestCount == 2)
						{
							File.Move(Path.Combine(httpTestHelper.LocalDirectory, "testing.edp"), Path.Combine(httpTestHelper.LocalDirectory, "testing2.edp"));
						}
					};

					var upgradeVersion = currentVersion.AddMinor(1).ToVersion();
					var serviceTask = new UpgraderServiceTaskForTest(httpTestHelper, upgradeVersion) { OverridenUpgradePackageResponseFileName = "testing2.edp" };
					serviceTask.ConfigString = new ScheduledUpgraderConfig("") { AutoDownload = true }.ConfigString;

					var logger = new Mock<ILogger>();
					var logs = new List<string>();
					logger.Setup(x => x.Log(It.IsAny<LogType>(), It.IsAny<string>())).Callback<LogType, string>((type, message) => logs.Add(type + "|" + message));
					serviceTask.ServiceLogger = logger.Object;

					serviceTask.RunTask();
					AssertEquals($@"Information|Auto-Download is set to 'true'.
Information|Patch-Only is set to 'false'.
Information|Current version is {currentVersionNumber}.
Information|Checking locally for a newer upgrade package...
Information|No new local upgrade package available.
Information|Checking online for a newer upgrade package...
Information|Upgrade package {upgradeVersion} found, downloading from http://localhost:{httpTestHelper.Port}/testing2.edp
Error|Error downloading upgrade package: The remote server returned an error: (404) Not Found.
Error|Error downloading upgrade package: The remote server returned an error: (404) Not Found.
Information|Downloaded {testingEdpPathSize} of {testingEdpPathSize} bytes
Information|Uploading package to database
Information|Applying upgrade {expectedUpgradeVersion}.", string.Join("\r\n", logs));
					AssertNotNull(serviceTask.upgradeToApply);
				}
			}
		}

		public void TestUpgradeRetryStrategyRetryInterval()
		{
			var strategy = new UpgradeRetryStrategyForIntervalTest();
			strategy.UpdateErrorStrategyRetryCount = delegate
			{ };
			var shouldRetry = strategy.GetShouldRetry();

			TimeSpan ts;
			AssertEquals(true, shouldRetry.Invoke(0, new Exception(), out ts));
			AssertEquals(1d, ts.TotalMinutes);

			AssertEquals(true, shouldRetry.Invoke(1, new Exception(), out ts));
			AssertEquals(5d, ts.TotalMinutes);

			AssertEquals(true, shouldRetry.Invoke(2, new Exception(), out ts));
			AssertEquals(10d, ts.TotalMinutes);

			AssertEquals(false, shouldRetry.Invoke(3, new Exception(), out ts));
		}

		public void TestUpgrader_ServicePointManager()
		{
			var stmUpgrade = Factory.New<StmUpgrade>();
			var currentVersion = ReleaseInfo.Instance.VersionNumber;
			stmUpgrade.VersionNumber = currentVersion;
			stmUpgrade.SZ_Status = StmUpgrade.StmUpgradeStatus.CurrentVersion;
			stmUpgrade.SZ_ExeVersionDate = DateTime.UtcNow;
			Factory.Save();

			using (var httpTestHelper = new HttpTestHelper())
			{
				httpTestHelper.Start();

				var upgradeVersion = currentVersion.AddMinor(1).ToVersion();
				var serviceTask = new UpgraderServiceTaskForServicePointManagerTest(httpTestHelper, upgradeVersion);
				serviceTask.ConfigString = new ScheduledUpgraderConfig("") { AutoDownload = true }.ConfigString;

				var logger = new Mock<ILogger>();
				var logs = new List<string>();
				logger.Setup(x => x.Log(It.IsAny<LogType>(), It.IsAny<string>())).Callback<LogType, string>((type, message) => logs.Add(type + "|" + message));
				serviceTask.ServiceLogger = logger.Object;

				var originalExpect100Continue = ServicePointManager.Expect100Continue;
				ServicePointManager.Expect100Continue = true;

				try
				{
					serviceTask.ServiceLogger.Information($"ServicePointManager.BeforeRunTask.Expect100Continue={ServicePointManager.Expect100Continue}");
					serviceTask.ServiceLogger.Information($"ServicePointManager.BeforeRunTask.SecurityProtocol={ServicePointManager.SecurityProtocol}");
					serviceTask.RunTask();
				}
				finally
				{
					serviceTask.ServiceLogger.Information($"ServicePointManager.AfterRunTask.Expect100Continue={ServicePointManager.Expect100Continue}");
					serviceTask.ServiceLogger.Information($"ServicePointManager.AfterRunTask.SecurityProtocol={ServicePointManager.SecurityProtocol}");

					ServicePointManager.Expect100Continue = originalExpect100Continue;
				}

				AssertEquals(OnlineVersionPackedForTestDownload, serviceTask.upgradeToApply.Version);

				Assert(logs.Any(x => x == $"Information|Upgrade package {upgradeVersion} found, downloading from {httpTestHelper.ServerAddress}testing.edp"));

				var servicePointManagerLog = string.Join("\r\n", logs.Where(x => x.Contains("ServicePointManager")));
				AssertEquals(@"Information|ServicePointManager.BeforeRunTask.Expect100Continue=True
Information|ServicePointManager.BeforeRunTask.SecurityProtocol=SystemDefault
Information|GetRetryPolicy.ServicePointManager.Expect100Continue=False
Information|GetRetryPolicy.ServicePointManager.SecurityProtocol=SystemDefault
Information|ServicePointManager.AfterRunTask.Expect100Continue=True
Information|ServicePointManager.AfterRunTask.SecurityProtocol=SystemDefault", servicePointManagerLog);
			}
		}

		class UpgradeRetryStrategyForIntervalTest : UpgraderServiceTask.UpgradeRetryStrategy
		{
			public UpgradeRetryStrategyForIntervalTest() : base()
			{
			}

			protected override bool IsTest => false;
		}

		public class UpgradeRetryPolicyForTest : UpgraderServiceTask.UpgradeRetryPolicy
		{
			public UpgradeRetryPolicyForTest(ILogger logger) : base(new TestRetryStrategy(), logger) { }

			public Exception LastThrownException => ((TestRetryStrategy)RetryStrategy).LastThrownException;

			public void ResetException() => ((TestRetryStrategy)RetryStrategy).LastThrownException = null;

			class TestRetryStrategy : RetryStrategy
			{
				public TestRetryStrategy() : base("", false) { }
				public override ShouldRetry GetShouldRetry() => Retry;
				public Exception LastThrownException { get; set; }
				bool Retry(int retryCount, Exception exception, out TimeSpan delay)
				{
					delay = TimeSpan.Zero;
					LastThrownException = exception;
					return false;
				}
			}
		}

		class UpgraderServiceTaskForTest : UpgraderServiceTaskForTestBase
		{
			public UpgraderServiceTaskForTest(HttpTestHelper httpTestHelper) : base(httpTestHelper) { }

			public UpgraderServiceTaskForTest(HttpTestHelper httpTestHelper, Version latestAvilableVersion) : base(httpTestHelper, latestAvilableVersion) { }

			internal override bool DoUpgrade(UpgradeInfo upgradeToApply)
			{
				ServiceLogger.Information(Invariant($"Applying upgrade {upgradeToApply.Version}."));

				this.upgradeToApply = upgradeToApply;

				return true;
			}

			internal UpgradeInfo upgradeToApply;

			internal override IUpgradePackageService UpgradePackageServiceClient => OverwrittenUpgradePackageServiceClient ?? base.UpgradePackageServiceClient;

			internal IUpgradePackageService OverwrittenUpgradePackageServiceClient { get; set; }

			internal override UpgradeRetryPolicy GetRetryPolicy() => OverridenUpgradeRetryPolicy ?? base.GetRetryPolicy();

			public UpgradeRetryPolicy OverridenUpgradeRetryPolicy { get; set; }
			protected override TimeSpan DownloaderRetryInterval => TimeSpan.FromMilliseconds(100);
		}

		class UpgraderServiceTaskForServicePointManagerTest : UpgraderServiceTaskForTest
		{
			public UpgraderServiceTaskForServicePointManagerTest(HttpTestHelper httpTestHelper, Version latestAvilableVersion) : base(httpTestHelper, latestAvilableVersion) { }

			internal override UpgradeRetryPolicy GetRetryPolicy()
			{
				ServiceLogger.Information($"GetRetryPolicy.ServicePointManager.Expect100Continue={ServicePointManager.Expect100Continue}");
				ServiceLogger.Information($"GetRetryPolicy.ServicePointManager.SecurityProtocol={ServicePointManager.SecurityProtocol}");
				return base.GetRetryPolicy();
			}
		}

		IEnumerable<StmUpgrade> CreateStmUpgradeVersions_ForTest(IEnumerable<(VersionNumber versionNumber, string status, DateTime versionDate)> stmUpgradeVersions)
		{
			var stmUpgrades = new List<StmUpgrade>();
			foreach (var (versionNumber, status, versionDate) in stmUpgradeVersions)
			{
				var stmUpgrade = Factory.New<StmUpgrade>();
				stmUpgrade.VersionNumber = versionNumber;
				stmUpgrade.SZ_Status = status;
				stmUpgrade.SZ_ExeVersionDate = versionDate;

				stmUpgrades.Add(stmUpgrade);
			}

			Factory.Save();

			return stmUpgrades;
		}
	}
}

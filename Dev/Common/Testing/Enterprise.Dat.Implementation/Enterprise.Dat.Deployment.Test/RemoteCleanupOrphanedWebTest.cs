using System;
using System.Linq;
using System.Text;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.Data;
using Dat.Integration;
using Microsoft.Web.Administration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Dat.Implementation.Testing
{
	class RemoteCleanupOrphanedWebTest : TestCase
	{
		[Property("DAT:CapabilityRequirements", "ADMIN")]
		[Property("DAT:CapabilityRequirements", "VM")]
		public void TestGeneralCleanupOrphanedWebSites()
		{
			TestHelpers.WinrmQuickConfigAndWaitForExit();

			const string targetHost = "localhost";
			var tornDownTestDatabaseName = $"TearDownWebSiteTaskTest-{Guid.NewGuid():N}";
			var wiseRatesDatabaseNames = new[]
			{
				"SH0WiseRatesWI00734219",
				"WiseRatesWI00734219",
				"WiseRates",
				"WI00734219WISERATES",
			};

			using (TestHelpers.InstallTestSite(Db.ServerName, tornDownTestDatabaseName, out var orphanedSite, out var applicationPoolName))
			using (TestHelpers.InstallTestSite(Db.ServerName, wiseRatesDatabaseNames[0], out var rateSite1, out var _))
			using (TestHelpers.InstallTestSite(Db.ServerName, wiseRatesDatabaseNames[1], out var rateSite2, out var _))
			using (TestHelpers.InstallTestSite(Db.ServerName, wiseRatesDatabaseNames[2], out var rateSite3, out var _))
			using (TestHelpers.InstallTestSite(Db.ServerName, wiseRatesDatabaseNames[3], out var rateSite4, out var _))
			{
				using (var serverManager = new ServerManager())
				{
					new[] { orphanedSite, rateSite1, rateSite2, rateSite3, rateSite4 }
					.ForEach(site =>
					{
						AssertEquals(
							$"Site {site} has been installed successfully",
							true,
							serverManager.Sites.Any(x => x.Name == site));
					});
				}

				// Arrange - drop target database as if torn down
				using (var adminConnection = Db.NewAdminConnection())
				{
					AdoTestUtils.DropDbIfExists(adminConnection, tornDownTestDatabaseName);

					foreach (var wiseRateDatabase in wiseRatesDatabaseNames)
					{
						AdoTestUtils.DropDbIfExists(adminConnection, wiseRateDatabase);
					}
				}

				var stringBuilder = new StringBuilder();
				var loggerMock = new Mock<ITaskLogger>();
				var logger = loggerMock.Object;
				_ = loggerMock.Setup(x => x.RecordInfo(It.IsAny<string>())).Callback((string message) =>
				{
					_ = stringBuilder.AppendLine($"{DateTime.Now:O} {message}");
				});

				try
				{
					// Act
					new RemoteCleanupOrphanedWeb().Invoke(targetHost, portableWebServer.InstallPath, logger, verbose: true);
				}
				catch (Exception ex)
				{
					Fail($"{ex}\\n{stringBuilder}");
				}

				// Assert
				var logs = stringBuilder.ToString();
				CombineAssertions(logs, () =>
				{
					using var serverManager = new ServerManager();
					AssertEquals(
						$"Orphaned site {orphanedSite} should have been cleaned up but still exists",
						false,
						serverManager.Sites.Any(x => x.Name == orphanedSite));

					AssertEquals(
						$"Orphaned Application pool {applicationPoolName} should have been cleaned up but still exists",
						false,
						serverManager.ApplicationPools.Any(x => x.Name == applicationPoolName));

					AssertContains("Databases not found on", logs);
					AssertContains(tornDownTestDatabaseName, logs);

					// rates site are excluded from the cleanup
					new[] { rateSite1, rateSite2, rateSite3, rateSite4 }
					.ForEach(site =>
					{
						AssertEquals(
							$"WiseRate site {site} is excluded from cleanup even if the database is torn down",
							true,
							serverManager.Sites.Any(x => x.Name == site));
					});

					foreach (var wiseRateDatabase in wiseRatesDatabaseNames)
					{
						AssertContains(wiseRateDatabase, logs);
					}
				});
			}

			AsyncHelper.WaitAllActiveTasksForTest();
		}

		PortableWebServer portableWebServer;
		protected override void SetUp()
		{
			base.SetUp();
			portableWebServer = new PortableWebServer();
		}

		protected override void TearDown()
		{
			portableWebServer.Dispose();
			base.TearDown();
		}
	}
}

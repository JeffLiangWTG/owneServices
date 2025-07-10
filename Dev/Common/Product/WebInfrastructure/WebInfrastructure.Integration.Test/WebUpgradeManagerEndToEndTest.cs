using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWiseOne.WebInfrastructure.Integration.Strong.Test.Helpers;
using CargoWiseOne.WebInfrastructure.Integration.Test.Helpers;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.ZArchitecture.Core;
using NLog;
using NLog.Config;
using NLog.Targets;
using NUnit.Framework;
using static System.FormattableString;

namespace CargoWiseOne.WebInfrastructure.Integration.Test
{
	[UseSnapshotProtection]
	class WebUpgradeManagerEndToEndTest : TestCase
	{
		[DeveloperOnlyTest]
		public void TestMultipleWebSites_UpgradeLifeCycle()
		{
			Step1_TestSitesAreRunning_AfterInitialSiteInstall();
			Step2_TestSiteResponses_DatabaseUpgradeIsInProgress();
			Step3_TestSiteResponses_DatabaseUpgraded();
			Step4_TestSiteResponses_WebUpgradeIsInProgress();
			Step5_TestSitesAreRunning_AfterWebUpgrade();

			void Step1_TestSitesAreRunning_AfterInitialSiteInstall()
			{
				TestSitesAreUpAndRunning(new CancellationTokenSource(TimeSpan.FromMinutes(10)).Token);
			}

			void Step2_TestSiteResponses_DatabaseUpgradeIsInProgress()
			{
				var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));

				// Step 2.1 - FullSilentUpgrade
				var fullUpgradeThread = new Thread(FullSilentUpgrade);
				fullUpgradeThread.Start(cancellationTokenSource.Token);

				// Step 2.2 - Bombard your sites while another task is doing a full upgrade, can you handle that?
				DDay(() => cancellationTokenSource.IsCancellationRequested);

				fullUpgradeThread.Join();

				try
				{
					Db.Connection.EnsureIsOpen();
				}
				catch
				{
					// ignored;
				}
			}

			void Step3_TestSiteResponses_DatabaseUpgraded()
			{
				// Step 3.1 - Bump up package version
				IncrementPackageVersion();

				// Step 3.2 - Bump up major schema version
				IncrementSchemaVersion();

				// Step 3.3 - How do you handle when database has been upgraded?
				var responsesGetServerPaths = new List<WebResponseWithDetails>();
				Parallel.ForEach(sitesWebAddress, site =>
				{
					responsesGetServerPaths.Add(WebClient.WebGet(site, "ServerPath.aspx", 1));
				});

				CombineAssertions(() =>
				{
					responsesGetServerPaths.ForEach(x => Logger.Info($"{x}"));
					responsesGetServerPaths.ForEach(x =>
					{
						// Assert(x.Details, x.IsOk);
					});
				});

				// Step 3.4 - Bombard your sites, can you handle that?
				DDay(() => EventBag.Contains(WebUpgradeManagerRunningUpdater));

				void IncrementPackageVersion()
				{
					var newVersion = DbVersionsHelper.IncrementPackageVersion(ApplicationVersion);
					Logger.Info("=== DbVersionsHelper.IncrementPackageVersion() ");
					Logger.Info($"Incremented package version from: {ApplicationVersion} to {newVersion}");
					upgradedVersionDir = Path.Combine(Path.GetDirectoryName(installedVersionDir), newVersion.ToString());
				}

				void IncrementSchemaVersion()
				{
					var anySite = sitesWebAddress.First();
					var getSchemaVersionsResponse = WebClient.WebGet(anySite, "DbVersions/GetSchemaVersions");
					Version.TryParse(getSchemaVersionsResponse.Details.GetKeyValue("DbSchemaVersion"), out var initialVersion);
					var updatedVersion = DbVersionsHelper.IncrementSchemaMajorVersion();

					CombineAssertions(() =>
					{
						AssertNotNull(initialVersion);
						AssertNotNull(updatedVersion);
						AssertGreaterThan(updatedVersion, initialVersion);

						Logger.Info("=== DbVersionsHelper.IncrementSchemaMajorVersion() ");
						Logger.Info($"Incremented db schema version from: {initialVersion} to {updatedVersion}");
					});
				}
			}

			void Step4_TestSiteResponses_WebUpgradeIsInProgress()
			{
				// Decrement the schema version to let it settle
				DecrementSchemaMajorVersion();

				// Not knowing, even I *do* know, you are doing binary upgrade and switching the sites physical path
				// as of client, why can't I visit your sites?
				DDay(() => EventBag.Contains(UpdaterUpdatingPhysicalPathsTo));
			}

			void Step5_TestSitesAreRunning_AfterWebUpgrade()
			{
				TestSitesAreUpAndRunning(new CancellationTokenSource(TimeSpan.FromMinutes(3)).Token);
			}

			void TestSitesAreUpAndRunning(CancellationToken cancellationToken)
			{
				var webResponses = new List<WebResponseWithDetails>();

				while (!cancellationToken.IsCancellationRequested)
				{
					webResponses.Clear();

					sitesWebAddress.ForEach(webAddress =>
					{
						webResponses.Add(WebClient.WebGetServerPath(webAddress));
						webResponses.Add(WebClient.WebGet(webAddress, "DbConnection"));
						webResponses.Add(WebClient.WebGet(webAddress, "DbVersions/GetSchemaVersions"));
					});

					webResponses.ForEach(x => Logger.Info($"{x}"));
					if (webResponses.All(x => x.IsOk))
					{
						break;
					}
				}

				CombineAssertions(() =>
				{
					webResponses.ForEach(x =>
					{
						Assert(x.Details, x.IsOk);
					});
				});
			}

			void DDay(Func<bool> shallWeCancelNow)
			{
				var cancellationTokenSource = new CancellationTokenSource();
				var siteUrls = new List<(string webAddress, string webPage)>();
				sitesWebAddress.ForEach(webAddress =>
				{
					siteUrls.Add((webAddress, "ServerPath.aspx"));
					siteUrls.Add((webAddress, "DbConnection"));
					siteUrls.Add((webAddress, "DbVersions/GetSchemaVersions"));
				});

				var bandOfBrothers = new List<Thread>();
				for (var i = 0; i < 10; i++)
				{
					bandOfBrothers.Add(new Thread(Airborne));
				}

				bandOfBrothers.ForEach(x => x.Start());
				while (!cancellationTokenSource.IsCancellationRequested)
				{
					if (shallWeCancelNow())
					{
						Logger.Info("Cancelled Airborne because we have hit the target of the expected message");
						cancellationTokenSource.Cancel();
					}

					Thread.Sleep(100);
				}
				bandOfBrothers.ForEach(x => x.Join());

				void Airborne()
				{
					while (!cancellationTokenSource.IsCancellationRequested)
					{
						Parallel.ForEach(siteUrls, (siteUrl, state) =>
						{
							if (cancellationTokenSource.IsCancellationRequested)
							{
								state.Break();
							}

							Logger.Info($"{WebClient.WebGet(siteUrl.webAddress, siteUrl.webPage, 1)}");
						});

						Thread.Sleep(100);
					}
				}
			}

			void FullSilentUpgrade(object state)
			{
				var cancellationToken = (CancellationToken)state;
				using var disposable = Db.DisposableUpgrade_ForTest(acquireLockOut: true, killOtherConnections: true, updateSchemaVersion: false);
				Logger.Info(">>> Full upgrade has acquired lockout and killed other connections.");

				while (!cancellationToken.IsCancellationRequested)
				{
					Thread.Sleep(100);
				}
			}

			void DecrementSchemaMajorVersion()
			{
				var updatedVersion = DbVersionsHelper.DecrementSchemaMajorVersion();
				Logger.Info("=== DbVersionsHelper.DecrementSchemaMajorVersion() ");
				Logger.Info($"Decremented db schema version to {updatedVersion}");
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			SetupLogger();
			AssertEquals(DatabaseSchemaVersion, ApplicationSchemaVersion);
			Logger.Info($"{nameof(WebUpgradeManagerEndToEndTest)}->{nameof(SetUp)}");
			sitesInstaller = new WithSitesInstaller(testConfiguration);

			RemoveOtherPackagesIfAny();
			CreateAndUploadDebugPackage();
			InstallCurrentVersion();
			SelectSitesForTest();
			StartReadWindowsEventLogsThread();

			static void RemoveOtherPackagesIfAny()
			{
				Db.Connection.ExecuteNonQuery("DELETE [StmUpgrade] WHERE 1=1");
			}

			static void CreateAndUploadDebugPackage()
			{
				using var packageMaker = new DebugBuildPackageMaker();
				packageMaker.CreateAndUploadDebugBuildPackage(Db.ServerName, ((IDbConnectionInternals)Db.Connection).ADOConnection, ApplicationVersion);
			}

			void InstallCurrentVersion()
			{
				installedVersionDir = sitesInstaller.InstallSites(Db.ServerName, Db.DatabaseName);
				Assert(Directory.Exists(installedVersionDir));
			}

			void SelectSitesForTest()
			{
				sitesWebAddress =
					sitesInstaller.InstalledSites.Where(x => x.Install).Select(x => x.WebAddress).ToList();
			}

			void SetupLogger()
			{
				var config = new LoggingConfiguration();
				var fileTarget = new FileTarget
				{
					Encoding = Encoding.UTF8,
					Layout = "${longdate} ${level:uppercase=true} ${message}",
					FileName = @"${basedir}\Logs\WebInfrastructure.Integration.Test.log",
					ArchiveDateFormat = "yyyyMMdd_HHmmss",
					ArchiveAboveSize = 8388608,
					ArchiveNumbering = ArchiveNumberingMode.DateAndSequence,
				};

				config.AddTarget("fileTarget", fileTarget);
				config.LoggingRules.Add(new LoggingRule("*", LogLevel.Trace, fileTarget));
				LogManager.Configuration = config;
			}
		}

		protected override void TearDown()
		{
			exitEvent?.Set();
			readWindowsEventLogsThread?.Join();

			exitEvent?.Dispose();
			eventTriggered?.Dispose();

			sitesInstaller.Dispose();

			if (!string.IsNullOrEmpty(upgradedVersionDir) && Directory.Exists(upgradedVersionDir))
			{
				SiteUninstallerViaAppManager.ExecuteOrInvoke(upgradedVersionDir);
			}

			Logger.Info($"{nameof(WebUpgradeManagerEndToEndTest)}->{nameof(TearDown)}");
			base.TearDown();
		}

		void StartReadWindowsEventLogsThread()
		{
			exitEvent = new AutoResetEvent(false);
			eventTriggered = new AutoResetEvent(false);
			readWindowsEventLogsThread = new Thread(() =>
			{
				while (!exitEvent.WaitOne(1))
				{
					EventLogHelper.ReadEventLogs().OrderTimeCreated().ToList()
						.ForEach(eventRecord =>
						{
							var eventDescription = Invariant($"{eventRecord.FormatDescription()}");
							EventKeyWords.ForEach(keyword =>
							{
								if (eventDescription.StartsWith(keyword, StringComparison.InvariantCultureIgnoreCase))
								{
									EventBag.Add(keyword);
									eventTriggered.Set();
									Logger.Info($">>> {keyword}: {eventDescription} ");
								}
							});

							Logger.Info(Invariant($"[{eventRecord.ProviderName}:{eventRecord.Id}] {eventDescription}"));
						});
				}

				Thread.Sleep(500);
			});

			readWindowsEventLogsThread.Start();
		}

		LoggerHelper Logger => logger ??= new LoggerHelper(nameof(WebUpgradeManagerEndToEndTest));
		LoggerHelper logger;

		WithSitesInstaller sitesInstaller;
		string installedVersionDir;
		string upgradedVersionDir;
		List<string> sitesWebAddress;
		Thread readWindowsEventLogsThread;
		AutoResetEvent exitEvent;
		AutoResetEvent eventTriggered;

		static Version ApplicationVersion => ReleaseInfo.Instance.VersionNumber.ToVersion();
		static Version DatabaseSchemaVersion =>
			new (DbRegistry.DatabaseMajorSchemaVersion.LoadValue(Db.Connection),
				DbRegistry.DatabaseMinorSchemaVersion.LoadValue(Db.Connection),
				0, 0);
		static Version ApplicationSchemaVersion =>
			new (SchemaVersion.Application.Major, SchemaVersion.Application.Minor, 0, 0);

		readonly InstallationConfigurationForTest testConfiguration = new InstallationConfigurationForTest(Guid.NewGuid());
		static readonly ConcurrentBag<string> EventBag = new ();
		static readonly List<string> EventKeyWords = new ()
		{
			WebUpgradeManagerStartingUpgradeOf,
			WebUpgradeManagerInstallingFilesFor,
			WebUpgradeManagerRunningUpdater,
			UpdaterUpdatingPhysicalPathsTo,
		};
		const string DbUpgradeManagerReleasedLockForUpgrade = "Released lock for upgrade";
		const string WebUpgradeManagerStartingUpgradeOf = "Starting upgrade of";
		const string WebUpgradeManagerInstallingFilesFor = "Installing files for";
		const string WebUpgradeManagerRunningUpdater = "Running updater";
		const string UpdaterUpdatingPhysicalPathsTo = "Updating physical paths to";
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWiseOne.WebInfrastructure;
using CargoWiseOne.WebInfrastructure.ErrorReporting;
using CargoWiseOne.WebInfrastructure.TestFramework;
using Enterprise.Environment;
using Enterprise.Upgrades;
using Microsoft.Web.Administration;
using Moq;
using NUnit.Framework;
using static System.FormattableString;

namespace Enterprise.ZArchitecture.Web
{
	[UseSnapshotProtection]
	[TestRequiresAdministrativePrivileges("Requires admin privilege to GetAdminSection")]
	public class WebSiteUpgradeTest : TestCase
	{
		[ExpectNoExceptions]
		[DeveloperOnlyTest]
		public void TestSiteUpgraded_OnDatabaseUpgradedException()
		{
			if (suspendWebUpgradeManagerCleanup == null)
			{
				// Test does not run reliably if cleanup is not suspended
				return;
			}

			var serviceUnavailable = new string[]
			{
				"(503)",
				"(500)"
			};

			// Arrange
			var expectedSchemaVersions = Invariant($"{Db.ServerName}, {Db.DatabaseName}, SchemaVersion: {Env.Registry.DatabaseMajorSchemaVersion}.{Env.Registry.DatabaseMinorSchemaVersion}");
			AssertEquals(Path.Combine(sitePath, TestApplicationName), GetWebPage(testSite, "ServerPath.aspx")?.TrimEnd('\\'));
			AssertEquals("Test site returned correct db schema versions", expectedSchemaVersions, GetSchemaVersions(testSite));

			UploadPackage(versionNumber: 2, status: "CUR");
			BumpUpSchemaVersion();

			AssertExceptionThrown<DatabaseUpgradedException>(() =>
			{
				using (Db.DisposableActionForDbConnection())
				using (var connection = Db.NewExtraConnectionToMainDb())
				{
					connection.EnsureIsOpen();
				}
			});

			// Act
			var errorResponse = GetSchemaVersions(testSite);

			// Assert
			Assert(errorResponse, serviceUnavailable.Any(errorCode => errorResponse.Contains(errorCode)));

			// Wait for upgrade to complete
			var timeoutCancellation = new CancellationTokenSource(TimeSpan.FromMinutes(5));
			while (!timeoutCancellation.IsCancellationRequested)
			{
				Thread.Sleep(TimeSpan.FromSeconds(1));
				if (!initialServerPath.Equals(GetServerPath(testSite.Name), StringComparison.OrdinalIgnoreCase))
				{
					break;
				}
			}

			// Assert site upgraded to version 2
			var upgradedSiteServerPath = GetServerPath(testSite.Name);
			if (upgradedSiteServerPath != null)
			{
				localPaths.Add(Directory.GetParent(upgradedSiteServerPath)?.FullName);
			}

			AssertNotNullOrEmpty(upgradedSiteServerPath);
			Assert("Upgraded path must contain the new version number '2.0.0.0'", upgradedSiteServerPath.Contains("2.0.0.0"));
			AssertNotEquals(initialServerPath, upgradedSiteServerPath);
		}

		#region Implementations

		static string GetSchemaVersions(Site site)
		{
			return SendWebRequest($"http://localhost:{site.Bindings[0].EndPoint.Port}/db/versions/schema");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		static string GetWebPage(Site site, string page, int timeoutInSeconds = 120)
		{
			return SendWebRequest($"http://localhost:{site.Bindings[0].EndPoint.Port}/{page}", timeoutInSeconds);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		static string SendWebRequest(string url, int timeoutInSeconds = 120)
		{
			try
			{
				var request = WebRequest.CreateHttp(url);
				request.Timeout = timeoutInSeconds * 1000;

				using (var response = request.GetResponse())
				using (var reader = new StreamReader(response.GetResponseStream()))
				{
					return reader.ReadToEnd();
				}
			}
			catch (WebException ex)
			{
				if (ex.Response == null)
				{
					return string.Empty;
				}

				using (var reader = new StreamReader(ex.Response.GetResponseStream()))
				{
					return $"{ex.Status}: {ex}{System.Environment.NewLine}{reader.ReadToEnd()}";
				}
			}
		}

		string GetServerPath(string siteName)
		{
			using (var newSeverManager = new ServerManager())
			{
				return newSeverManager.Sites[siteName] == null
					? string.Empty
					: WebSiteTestManager.GetPhysicalPath(newSeverManager.Sites[siteName]);
			}
		}

		void BumpUpSchemaVersion()
		{
			const string sql = @"
UPDATE dbo.StmData SET
	SD_BinaryValue = CONVERT(varbinary(max), CONVERT(nvarchar(max), (SELECT CONVERT(int, CONVERT(nvarchar(max), SD_BinaryValue))) + 1))
WHERE 1 = 1
	AND SD_Name = 'DATABASE_SCHEMA_VERSION'
	AND SD_Owner is NULL
	AND SD_DepartmentGuid is NULL
";
			Db.Connection.ExecuteNonQuery(sql);
		}

		protected override void TearDown()
		{
			try
			{
				siteManager.Dispose();
				TearDownSite(TestAppPoolName, TestSiteName);
			}
			finally
			{
				var timoutCancellation = new CancellationTokenSource(TimeSpan.FromSeconds(2));
				using (var serverManager = new ServerManager())
				{
					while (!timoutCancellation.IsCancellationRequested
						&& serverManager.Sites.Any(x => x.Name == TestSiteName))
					{
						Thread.Sleep(100);
					}
				}

				var retry = 0;
				while (localPaths.Any(Directory.Exists) && retry++ < 3)
				{
					try
					{
						Thread.Sleep(100);
						localPaths.ToList().ForEach(x =>
						{
							if (Directory.Exists(x))
							{
								Directory.Delete(x, true);
							}
						});
					}
					catch
					{
						// ignored because some site been notified to clean up old version
					}
				}

				suspendWebUpgradeManagerCleanup?.Dispose();
				suspendWebUpgradeManagerCleanup = null;
			}

			base.TearDown();
		}

		void TearDownSite(string appPoolName, string siteName)
		{
			using (var serverManager = new ServerManager())
			{
				var appPool = serverManager.ApplicationPools.SingleOrDefault(x => x.Name == appPoolName);
				var site = serverManager.Sites.SingleOrDefault(x => x.Name == siteName);
				if (site != null)
				{
					WebDbConfiguration.DeleteAllConfigurations(WebAppPath.For(site));
					serverManager.Sites.Remove(site);
				}

				if (appPool != null)
				{
					appPool.Stop();
					serverManager.ApplicationPools.Remove(appPool);
				}

				if (site != null && appPool != null)
				{
					serverManager.CommitChanges();
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			suspendWebUpgradeManagerCleanup = TrySuspendWebUpgradeManagerCleanup();

			EmptyUpgradePackages();
			SetupNewTestWebSite(UploadPackage());

			if (string.IsNullOrEmpty(initialServerPath))
			{
				Fail($"Test site: {testSite.Name} appears not running.");
			}

			void EmptyUpgradePackages()
			{
				Db.Connection.ExecuteNonQuery("DELETE dbo.StmUpgrade;");
				if (Directory.Exists(rootTargetDirectoryPath))
				{
					Directory.Delete(rootTargetDirectoryPath, true);
				}
			}
		}

		UpgraderMutex TrySuspendWebUpgradeManagerCleanup()
		{
			UpgraderMutex mutex = null;
			try
			{
				mutex = new UpgraderMutex(webInstallConfig.CleanupMutexName);
				if (!mutex.WaitOne(0))
				{
					mutex.Dispose();
					mutex = null;
				}
			}
			catch (Exception)
			{
				mutex?.Dispose();
				mutex = null;
			}

			return mutex;
		}

		UpgradeInfoExtended UploadPackage(int versionNumber = 1, string status = "APL")
		{
			var packageMaker = new PackageMaker(((IDbConnectionInternals)Db.Connection).ADOConnection);
			var zipFileName = PackageMaker.EnterpriseWebDeployZipName;

			packageMaker.AddWebFile(zipFileName, TestApplicationName, "Global.asax", GlobalAsax);
			packageMaker.AddWebFile(zipFileName, TestApplicationName, "Web.config", lazyGetWebConfig.Value);
			packageMaker.AddWebFile(zipFileName, TestApplicationName, "ServerPath.aspx", ServerPath);
			packageMaker.AddWebFile(zipFileName, TestApplicationName, "Error.aspx", ErrorAspx);

			var version = new Version(versionNumber, 0, 0, 0);
			return packageMaker.UploadPackage(version, status);
		}

		void SetupNewTestWebSite(UpgradeInfoExtended packageInfo)
		{
			using var sqlContext = new WebUpgradeSqlContext(Db.ServerName, Db.DatabaseName, () => ((IDbConnectionInternals)Db.Connection).ADOConnection);
			using var upgradeManager = new WebUpgradeManagerForTest(webInstallConfig, new Version(1, 2, 3, 4), sqlContext, new Mock<IErrorReporter>().Object, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
			var localPath = upgradeManager.InstallWebFilesForTest(packageInfo);
			var serviceFolder = Path.Combine(localPath, "Services");
			CopyDebugBuildAssembliesToBin(serviceFolder);

			sitePath = localPath;
			localPaths.Add(Directory.GetParent(localPath).FullName);

			siteManager = new WebSiteTestManager(HtmlFail);
			testSite = siteManager.AddWebSite((string name) => Path.Combine(localPath, TestApplicationName));
			siteNames.Add(testSite.Name);

			siteManager.AppPool.ManagedPipelineMode = ManagedPipelineMode.Integrated;
			siteManager.AppPool.ProcessModel.IdleTimeout = TimeSpan.FromHours(48);
			siteManager.AppPool.AutoStart = true;
			siteManager.AppPool.Failure.RapidFailProtectionInterval = UpgradeTimerInterval;
			siteManager.AppPool.Failure.RapidFailProtectionMaxCrashes = RapidFailProtectionMaxCrashes;

			siteManager.ServerManager.CommitChanges();

			TestAppPoolName = siteManager.AppPool.Name;
			TestSiteName = testSite.Name;

			WebDbConfiguration.SaveConfiguration(
				new WebDbConfigurationInfo()
				{
					ApplicationPath = WebAppPath.For(testSite),
					ServerName = Db.ServerName,
					DatabaseName = Db.DatabaseName
				});

			initialServerPath = GetServerPath(testSite.Name);
			using (var timeoutCancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
			{
				while (string.IsNullOrEmpty(initialServerPath) && !timeoutCancellation.IsCancellationRequested)
				{
					Thread.Sleep(100);
					initialServerPath = GetServerPath(testSite.Name);

					if (string.IsNullOrEmpty(initialServerPath))
					{
						break;
					}
				}
			}
		}

		static void CopyDebugBuildAssembliesToBin(string serviceFolder)
		{
			var binFolder = Path.Combine(serviceFolder, "bin");

			if (Directory.Exists(binFolder))
			{
				Directory.Delete(binFolder);
			}

			Directory.CreateDirectory(binFolder);

			var resolvedAssemblies = new HashSet<string>();
			var sourcePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var dependencyFiles = GetReferencedAssemblies(Assembly.GetExecutingAssembly().Location);

			IEnumerable<string> GetReferencedAssemblies(string assemblyPath)
			{
				var assembly = Assembly.ReflectionOnlyLoadFrom(assemblyPath);
				foreach (var referenceAssembly in assembly.GetReferencedAssemblies())
				{
					if (!resolvedAssemblies.Contains(referenceAssembly.Name))
					{
						var referenceAssemblyFilePath = Path.Combine(sourcePath, $"{referenceAssembly.Name}.dll");
						if (File.Exists(referenceAssemblyFilePath))
						{
							foreach (var x in GetReferencedAssemblies(referenceAssemblyFilePath))
							{
								yield return x;
							}
						}

						resolvedAssemblies.Add(referenceAssembly.Name);
					}
				}

				yield return assemblyPath;
			}

			foreach (var dllFile in dependencyFiles)
			{
				var fileName = Path.GetFileName(dllFile);
				var destination = Path.Combine(binFolder, fileName);

				File.Copy(dllFile, destination, true);

				var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(dllFile);
				fileName = $"{fileNameWithoutExtension}.XmlSerializers.dll";
				var sourceFile = Path.Combine(sourcePath, fileName);
				if (File.Exists(sourceFile))
				{
					destination = Path.Combine(binFolder, fileName);
					File.Copy(sourceFile, destination, true);
				}
			}
		}

		static string GetWebConfig()
		{
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var configFile = XDocument.Load(Path.Combine(binPath, WebConfigFileName));
			configFile.XPathSelectElement(@"//appSettings/add[@key='ServerName']").Attribute("value").Value = Db.ServerName;
			configFile.XPathSelectElement(@"//appSettings/add[@key='DatabaseName']").Attribute("value").Value = Db.DatabaseName;
			var webConfig = configFile.ToString();

			return webConfig.Replace(@"httpGetEnabled=""false""", @"httpGetEnabled=""true""");
		}

		string ErrorAspx => "Error.aspx";

		string GlobalAsax => @"<%@ Application Codebehind=""Global.asax.cs"" Inherits=""Enterprise.ZArchitecture.Web.Tests.Global"" Language=""C#"" %>";

		string ServerPath => @"<%=System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath%>";

		const string WebConfigFileName = "Enterprise.ZArchitecture.Web.Tests.dll.config";

		static readonly Lazy<string> lazyGetWebConfig = new Lazy<string>(GetWebConfig);
		readonly List<string> siteNames = new List<string>();
		readonly HashSet<string> localPaths = new HashSet<string>();
		readonly string rootTargetDirectoryPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData), "WiseTech Global", $"{nameof(WebSiteUpgradeTest)}_91A060A9-9414-4065-A372-753FD25F0112");
		readonly IInstallationConfiguration webInstallConfig = new InstallationConfigurationForTest(Guid.NewGuid());

		string TestAppPoolName { get; set; }
		string TestSiteName { get; set; }

		int RapidFailProtectionMaxCrashes => 1;
		TimeSpan UpgradeTimerInterval => TimeSpan.FromMinutes(1);
		string TestApplicationName => "Services";

		WebSiteTestManager siteManager;
		Site testSite;
		string sitePath;
		string initialServerPath;
		IDisposable suspendWebUpgradeManagerCleanup;

		#endregion
	}
}

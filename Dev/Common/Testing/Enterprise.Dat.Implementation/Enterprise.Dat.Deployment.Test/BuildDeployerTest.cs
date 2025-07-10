using System;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using CargoWise.ActiveDirectory;
using CargoWise.Application;
using CargoWise.BuildTools;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.IO;
using CargoWiseOne.WebInfrastructure;
using Dat.Integration;
using Dat.Integration.Deployment;
using Enterprise.Client.EDI.AutoDeploy;
using Enterprise.Client.EDI.ReleaseBuilds;
using Enterprise.Dat.Implementation;
using Enterprise.Dat.Implementation.Testing;
using Enterprise.DataTools.DbBackupAndRestore.Testing.Restore;
using Enterprise.Integration.Licensing;
using Enterprise.ProductRegistration.Client;
using Enterprise.ProductRegistration.Common;
using Enterprise.Upgrades;
using Enterprise.ZArchitecture.Core;
using Microsoft.CSharp;
using Microsoft.Web.Administration;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using static System.FormattableString;

namespace Enterprise.Dat.Deployment.Test
{
	sealed partial class BuildDeployerTest : TestCase
	{
		public void TestLoginsPrefixedWithDatabaseNamePatternAreDropped()
		{
			TestDropLogins((databaseName) => $"{databaseName}_");
		}

		public void TestLoginsPrefixedWithEnterpriseDbUserPatternAreDropped()
		{
			TestDropLogins((databaseName) => $"EnterpriseDbUser_{databaseName}_");
		}

		public void TestLoginsPrefixedWithDatabaseNamePatternWithoutUnderscoreAreNotDropped()
		{
			var otherLogin = string.Empty;

			TestDropLogins(
				(databaseName) => $"{databaseName}_",
				(loginPrefix, _) =>
				{
					otherLogin = $"{loginPrefix.Replace('_', 'x')}1234";
					CreateTestLogin(otherLogin);
				});

			try
			{
				using var adminConnection = Db.NewAdminConnection(Db.SqlMasterDb);
				AssertEquals($"Login [{otherLogin}] should not have been dropped.", true, adminConnection.Exists($"FROM sys.server_principals WHERE name LIKE '{otherLogin}%'"));
			}
			finally
			{
				DropTestLoginIfExists(otherLogin);
			}
		}

		public void TestLoginsPrefixedWithEnterpriseDbUserPatternWithoutUnderscoreAreNotDropped()
		{
			var otherLogin = string.Empty;

			TestDropLogins(
				(databaseName) => $"EnterpriseDbUser_{databaseName}_",
				(loginPrefix, _) =>
				{
					otherLogin = $"{loginPrefix.Replace('_', 'x')}1234";
					CreateTestLogin(otherLogin);
				});

			try
			{
				using var adminConnection = Db.NewAdminConnection(Db.SqlMasterDb);
				AssertEquals($"Login [{otherLogin}] should not have been dropped.", true, adminConnection.Exists($"FROM sys.server_principals WHERE name LIKE '{otherLogin}%'"));
			}
			finally
			{
				DropTestLoginIfExists(otherLogin);
			}
		}

		public void TestDroppingLoginsExceptionsAreInformativeWithTargetServerNameAndLoginPatterns()
		{
			var testDatabase = string.Empty;
			var otherLogin = string.Empty;
			DbConnection extraConnection = null;

			try
			{
				var exception = AssertExceptionThrown<Exception>(() =>
				{
					TestDropLogins(
					(databaseName) =>
					{
						testDatabase = databaseName;
						return $"{databaseName}_";
					},
					(_, testLogin) =>
					{
						AddDatabaseDependency(testLogin);
						otherLogin = testLogin;
						extraConnection = Db.NewExtraConnection(Db.ServerName, Db.SqlMasterDb, testLogin, "1234");
						extraConnection.EnsureIsOpen();
					});
				});

				CombineAssertions(() =>
				{
					AssertNotNull(exception);
					AssertStartsWith("Inner SqlException is wrapped up", $"Failed to drop logins from server {Db.Connection.ServerName}", exception.Message);
					AssertContains("Login failed to drop is included in the exception message", $"{otherLogin}", exception.Message);
				});
			}
			finally
			{
				extraConnection?.Dispose();
				DropDatabaseDependency(otherLogin);
				DropTestLoginIfExists(otherLogin);
			}
		}

		void AddDatabaseDependency(string testLogin)
		{
			var sql = @$"
CREATE DATABASE AuthTestDB;
ALTER AUTHORIZATION ON DATABASE::AuthTestDB TO [{testLogin}];";
			using var adminConnection = Db.NewAdminConnection();
			_ = adminConnection.ExecuteNonQuery(sql);
		}

		void DropDatabaseDependency(string testLogin)
		{
			var sql = @$"DROP DATABASE AuthTestDB;";
			using var adminConnection = Db.NewAdminConnection();
			_ = adminConnection.ExecuteNonQuery(sql);
		}

		static void CreateTestLogin(string testLogin)
		{
			var sql = $"CREATE LOGIN [{testLogin}] WITH PASSWORD = '1234', CHECK_POLICY = OFF, DEFAULT_LANGUAGE = us_english, DEFAULT_DATABASE = master";
			using var adminConnection = Db.NewAdminConnection();
			_ = adminConnection.ExecuteNonQuery(sql);
		}

		static void DropTestLoginIfExists(string testLogin)
		{
			var dropLoginSql = $@"
DECLARE @sqlCmd VARCHAR(MAX) = '';
SELECT @sqlCmd = @sqlCmd + 'DROP LOGIN ' + QUOTENAME(Logins.name) + '; ' FROM (SELECT name FROM sys.server_principals WHERE name LIKE @loginPattern) Logins;
IF (@sqlCmd != '') EXEC (@sqlCmd);";
			using var adminConnection = Db.NewAdminConnection();
			using var command = adminConnection.Command(dropLoginSql);
			command.AddParameter("loginPattern", SqlDbType.VarChar, testLogin);
			_ = command.ExecuteNonQuery();
		}

		void TestDropLogins(Func<string, string> getLoginPrefix, Action<string, string> arrangeAction = null)
		{
			var testDatabaseName = $"TearDownTestDb-{Guid.NewGuid():N}";
			var loginPrefix = getLoginPrefix(testDatabaseName);

			using (var disposable = AdoTestUtils.CreateDbDropExistingDisposable(testDatabaseName, Db.DatabaseName))
			{
				var testLogin = $"{loginPrefix}{Guid.NewGuid():N}";
				CreateTestLogin(testLogin);
				arrangeAction?.Invoke(loginPrefix, testLogin);

				using var adminConnection = Db.NewAdminConnection(testDatabaseName);
				BuildDeployer.DropLogins(adminConnection, testDatabaseName);
			}

			using (var adminConnection = Db.NewAdminConnection(Db.SqlMasterDb))
			{
				AssertEquals(false, adminConnection.Exists($"FROM sys.server_principals WHERE name LIKE '{EscapeUnderscore(loginPrefix)}%'"));
			}
		}

		public void TestDropLoginWithActiveSession()
		{
			var testDatabaseName = $"TearDownTestDb-{Guid.NewGuid():N}";
			var loginPrefix = $"{testDatabaseName}_";

			using (var disposable = AdoTestUtils.CreateDbDropExistingDisposable(testDatabaseName, Db.DatabaseName))
			{
				var testLogin = $"{loginPrefix}{Guid.NewGuid():N}";
				CreateTestLogin(testLogin);
				GrantConnect(testLogin);
				var userConnection = EstablishSessionForUser(testLogin);
				try
				{
					using var adminConnection = Db.NewAdminConnection(testDatabaseName);
					BuildDeployer.DropLogins(adminConnection, testDatabaseName);
				}
				finally
				{
					if (userConnection.State != ConnectionState.Closed)
					{
						userConnection.CloseConnection();
					}
					userConnection.Dispose();
					userConnection = null;
					DropTestLoginIfExists(testLogin);
				}
			}

			using (var adminConnection = Db.NewAdminConnection(Db.SqlMasterDb))
			{
				AssertEquals(false, adminConnection.Exists($"FROM sys.server_principals WHERE name LIKE '{EscapeUnderscore(loginPrefix)}%'"));
			}
		}

		void GrantConnect(string testLogin)
		{
			using var adminConnection = Db.NewAdminConnection(databaseName: Db.SqlMasterDb);
			using var command = adminConnection.Command($"GRANT CONNECT SQL TO [{testLogin}]");
			_ = command.ExecuteNonQuery();
		}

		DbConnection EstablishSessionForUser(string testLogin)
		{
			var testConn = Db.NewExtraConnection(Db.ServerName, Db.SqlMasterDb, testLogin, "1234");
			testConn.EnsureIsOpen();

			return testConn;
		}

		static string EscapeUnderscore(string input)
		{
			const string singleCharUnderscorePattern = @"(?<![_])_(?![_])";
			const string escapedUnderscore = "[_]";

			return Regex.Replace(input, singleCharUnderscorePattern, escapedUnderscore);
		}

		public void TestAutoDeployLatestBuildMasterPackage_EdpExists()
		{
			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			using (var archivePath = new TempDirectory())
			using (var packageArchivePath = new TempDirectory())
			{
				var mockHandler = new Mock<HttpMessageHandler>();
				mockHandler
					.Protected()
					.Setup<Task<HttpResponseMessage>>(
						"SendAsync",
						ItExpr.Is<HttpRequestMessage>(m =>
							m.RequestUri.AbsoluteUri == "http://nothing/Services/NudgeServiceTask?code=IBP&key=9183AC7A-59C3-47DC-A0C4-D96874BA5D6F"
							&& m.Method.Method == "GET"
						),
						ItExpr.IsAny<CancellationToken>())
					.Returns(Task.FromResult(new HttpResponseMessage { StatusCode = HttpStatusCode.OK }))
					.Verifiable();
				var nudgeClient = CreateNudgeClient(mockHandler);

				SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP", buildMasterEdp: true);
				var buildDeployer = new MockAutoDeployLatestBuildDeployer(new Mock<ITaskLogger>().Object, archivePath, packageArchivePath, nudgeClient);
				buildDeployer.AutoDeployLatestBuild("RELEASE", ReleaseBuildContent.IBPMasterPackageDeploymentConfiguration, sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName);

				// can remove archive creation after ediProd is upgraded to read from package archive
				var expectedArchiveDirectory = Path.Combine(archivePath.DirectoryName, "ALP_16.4.1.69_20160401110614");
				AssertEquals("Test", File.ReadAllText(Path.Combine(expectedArchiveDirectory, "Test.dll")));
				var releaseInfo = new ReleaseInfo(Path.Combine(expectedArchiveDirectory, BuildConstants.ReleaseInfoXmlFileName));
				AssertEquals(new VersionNumber("16.4.1.69"), releaseInfo.VersionNumber);
				AssertEquals("ALP", releaseInfo.ReleaseRing);
				AssertEquals(File.ReadAllText(Path.Combine(binPathDirectory.DirectoryName, BuildConstants.BuildXmlFileName)), File.ReadAllText(Path.Combine(expectedArchiveDirectory, BuildConstants.BuildXmlFileName)));
				Assert(File.Exists(Path.Combine(expectedArchiveDirectory, "ArchiveComplete")));
				mockHandler.VerifyAll();
			}
		}

		public void TestAutoDeployLatestBuildMasterPackage_EdpNotExists()
		{
			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			using (var archivePath = new TempDirectory())
			using (var packageArchivePath = new TempDirectory())
			{
				var mockHandler = new Mock<HttpMessageHandler>();
				mockHandler
					.Protected()
					.Setup<Task<HttpResponseMessage>>(
						"SendAsync",
						ItExpr.Is<HttpRequestMessage>(m =>
							m.RequestUri.AbsoluteUri == "http://nothing/Services/NudgeServiceTask?code=IBP&key=9183AC7A-59C3-47DC-A0C4-D96874BA5D6F"
							&& m.Method.Method == "GET"
						),
						ItExpr.IsAny<CancellationToken>())
					.Returns(Task.FromResult(new HttpResponseMessage { StatusCode = HttpStatusCode.OK }))
					.Verifiable();
				var nudgeClient = CreateNudgeClient(mockHandler);

				CompileExe(Path.Combine(binPathDirectory, ExeFileNames.CargoWiseOneExeForVersionInfo), "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");

				var buildDeployer = new MockAutoDeployLatestBuildDeployer(new Mock<ITaskLogger>().Object, archivePath, packageArchivePath, nudgeClient);

				// package-path.txt does not exist
				AssertExceptionThrown<FileNotFoundException>(
					() => buildDeployer.AutoDeployLatestBuild("RELEASE", ReleaseBuildContent.IBPMasterPackageDeploymentConfiguration, sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName));

				File.WriteAllText(Path.Combine(binPathDirectory, "package-path.txt"), "not-existing-path.edp");

				// package-path.txt exists but the file it points to does not exist
				AssertExceptionThrown<FileNotFoundException>(
					() => buildDeployer.AutoDeployLatestBuild("RELEASE", ReleaseBuildContent.IBPMasterPackageDeploymentConfiguration, sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName));
			}
		}

		public void TestAutoDeployLatestBuildMasterPackage_WhenUnsuccessfulResponseFromNudge()
		{
			var logger = new TestLogger();
			var mockHandler = new Mock<HttpMessageHandler>();
			mockHandler
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.Is<HttpRequestMessage>(m =>
						m.RequestUri.AbsoluteUri == "http://nothing/Services/NudgeServiceTask?code=IBP&key=9183AC7A-59C3-47DC-A0C4-D96874BA5D6F"
						&& m.Method.Method == "GET"
					),
					ItExpr.IsAny<CancellationToken>())
				.Returns(Task.FromResult(new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.InternalServerError,
					Content = new StringContent("Error from server"),
				}))
				.Verifiable();

			RunAutoDeployLatestBuildMasterPackageLoggingTest(mockHandler, logger);
			AssertContains("Unsuccessful response from NudgeServiceTask: InternalServerError, Error from server", logger.ToString());
		}

		public void TestAutoDeployLatestBuildMasterPackage_WhenRequestThrowsException()
		{
			var logger = new TestLogger();
			var mockHandler = new Mock<HttpMessageHandler>();
			mockHandler
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.Is<HttpRequestMessage>(m =>
						m.RequestUri.AbsoluteUri == "http://nothing/Services/NudgeServiceTask?code=IBP&key=9183AC7A-59C3-47DC-A0C4-D96874BA5D6F"
						&& m.Method.Method == "GET"
					),
					ItExpr.IsAny<CancellationToken>())
				.Throws(new HttpRequestException("Exception failure message"))
				.Verifiable();

			RunAutoDeployLatestBuildMasterPackageLoggingTest(mockHandler, logger);
			AssertContains("Exception from NudgeServiceTask: System.Net.Http.HttpRequestException: Exception failure message", logger.ToString());
		}

		void RunAutoDeployLatestBuildMasterPackageLoggingTest(Mock<HttpMessageHandler> mockHandler, ITaskLogger logger)
		{
			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			using (var archivePath = new TempDirectory())
			using (var packageArchivePath = new TempDirectory())
			{
				var nudgeClient = CreateNudgeClient(mockHandler);

				SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP", buildMasterEdp: true);
				var buildDeployer = new MockAutoDeployLatestBuildDeployer(logger, archivePath, packageArchivePath, nudgeClient);
				buildDeployer.AutoDeployLatestBuild("RELEASE", ReleaseBuildContent.IBPMasterPackageDeploymentConfiguration, sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName);
			}
		}

		[UseSnapshotProtection]
		[RequiresSoftware(RequiredSoftware.IsVM)]
		[RequiresAdminPrivileges()]
		public void TestAllWebSitesHavingTheSameWebDbConfigAreUninstalledOnTeardownTestedShelf()
		{
			var databaseServer = Db.ServerName;
			var webServer = "localhost";
			var workItemNumber = "WI00644263";
			var webDomain1 = $"{workItemNumber}1";
			var webDomain2 = $"{workItemNumber}2";
			var testDatabaseName = $"SH0{workItemNumber}";
			var exeDate = DateTime.Now;
			var versionNumber = $"{exeDate.Year.ToString().Substring(2, 2)}.{exeDate.Month}.{exeDate.Day}.{exeDate.Hour}";
			var releaseRing = "ALP";

			var logger = new TestLogger();
			var buildDeployer = CreateDeployer(logger, Mock.Of<ICargoWiseOneInstanceClass>());

			using (var sourceDir = new TempDirectory())
			using (var binDir = new TempDirectory())
			{
				// Arrange
				var dbBackupFilePath = PrepareDbBackupFile();
				Step1_PrepareBinPath(binDir);
				Step2_CreateWebDeployZipFiles(binDir);
				TaskInfo lastTaskInfo = null;

				try
				{
					// Act
					// Assert
					for (var i = 1; i < 3; i++)
					{
						var taskInfo = new TaskInfo("shelf", "owner", $@"
TestRigRestoreFromBackup: {dbBackupFilePath}
TestRigIconName: {workItemNumber}
TestRigSqlServer: {databaseServer}
TestRigDatabaseName: {testDatabaseName}
TestRigWebDomain: {workItemNumber}{i}
TestRigWebSites: {string.Join(",", InstallSiteItem.GetSitesToInstall().Select(x => x.FolderName))}
TestRigWebServer: {webServer}
TestRigWebServerInstallPath: {webServerInstallPath}
TestRigCreateIcon: false
TestRigRegister: false
TestRigLaunchDbUpgrade: false
TestRigPermitDuplicateWebSites: false
");
						AssertNoExceptionThrown(logger.ToString(), () =>
						{
							buildDeployer.AutoDeployTestedShelf("DEBUG", string.Empty, sourceDir, binDir, taskInfo);
						});

						lastTaskInfo = taskInfo;
					}
				}
				finally
				{
					if (lastTaskInfo != null)
					{
						AssertNoExceptionThrown(() =>
						{
							buildDeployer.TeardownTestedShelf("DEBUG", string.Empty, sourceDir, binDir, lastTaskInfo);
						});
					}
				}
			}

			string PrepareDbBackupFile()
			{
				var backupFilePath = CopyTestFileResource("OdysseyNoDescription.bak");
				Assert($"Database backup file: {backupFilePath} has been extracted from embedded resource.", File.Exists(backupFilePath));

				return backupFilePath;
			}

			void Step1_PrepareBinPath(string binPath)
			{
				var cargoWiseOneExe = Path.Combine(binPath, ExeFileNames.CargoWiseOneExeForVersionInfo);
				CompileExe(cargoWiseOneExe, versionNumber, exeDate, releaseRing);

				File.WriteAllText(
					Path.Combine(binPath, BuildConstants.BuildXmlFileName),
					$@"<Build xmlns=""http://www.edi.com.au/build.xsd"">
<Solutions>
	<Solution Filename=""whatever.sln"">
		<Bin>{ExeFileNames.CargoWiseOneExeForVersionInfo}</Bin>
		<Bin>Test.dll</Bin>
		<Bin>ZClientEDI.dll</Bin>
	</Solution>
	<OtherFiles>
		<Filename>EnterpriseWebDeploy.zip</Filename>
	</OtherFiles>
</Solutions>
</Build>",
					Encoding.UTF8);

				_ = Directory.CreateDirectory(Path.Combine(binPath, "DocumentXmls"));

				foreach (var assemblyFile in new[] { "Test.dll" })
				{
					File.WriteAllText(Path.Combine(binPath, assemblyFile), assemblyFile, Encoding.UTF8);
				}
			}

			void Step2_CreateWebDeployZipFiles(string binPath)
			{
				var enterpriseWebDeployZipFile = Path.Combine(binPath, "EnterpriseWebDeploy.zip");

				using (var tempDir = new TempDirectory())
				{
					_ = Directory.CreateDirectory(Path.Combine(tempDir, "shared-bin"));
					foreach (var site in InstallSiteItem.GetSitesToInstall())
					{
						var subfolder = Path.Combine(tempDir, site.FolderName);
						_ = Directory.CreateDirectory(subfolder);

						var serverPathAspxFile = Path.Combine(subfolder, "ServerPath.aspx");
						File.WriteAllText(serverPathAspxFile, "<%=System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath%>", Encoding.UTF8);

						var webConfigFile = Path.Combine(subfolder, "web.config");
						var webConfig = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <system.web>
    <customErrors mode=""Off"" />
    <httpRuntime targetFramework=""4.8"" />
    <compilation debug=""true"">
      <assemblies>
        <remove assembly=""*""/>
        <add assembly=""System.Core, Version=3.5.0.0, Culture=neutral, PublicKeyToken=B77A5C561934E089""/>
        <add assembly=""System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35""/>
        <add assembly=""System.Xml.Linq, Version=3.5.0.0, Culture=neutral, PublicKeyToken=B77A5C561934E089""/>
        <add assembly=""System.Data.DataSetExtensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=B77A5C561934E089""/>
      </assemblies>
    </compilation>
  </system.web>
</configuration>
";
						File.WriteAllText(webConfigFile, webConfig, Encoding.UTF8);
					}

					ZipFile.CreateFromDirectory(tempDir, enterpriseWebDeployZipFile);
				}

				Assert($"EnterpriseWebDeployZipFile {enterpriseWebDeployZipFile} has been created", File.Exists(enterpriseWebDeployZipFile));
			}
		}

		[UseSnapshotProtection]
		public void TestTeardownTestedShelfLeaveDbDesconstructionToTheLast()
		{
			// Arrange
			var shelfName = "1A26D3C02E9E48C68996";
			var dbName = "SH0" + shelfName;
			var webRootDomain = shelfName;
			var sid = new Guid("6060E847-C6B3-4405-930A-46C1FA2545B9").ToString("N", CultureInfo.InvariantCulture);
			var login = $"EnterpriseDbUser_{dbName}_{sid}";
			try
			{
				using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					using (var command = connection.Command(Invariant($"CREATE LOGIN [{login}] WITH PASSWORD = '', CHECK_POLICY = OFF, DEFAULT_LANGUAGE = us_english, SID = 0x{sid}")))
					{
						command.ExecuteNonQuery();
					}
				}

				var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");
				var taskComments = $@"JH6 11-Jan-16 16:33:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigCreateIcon: false
TestRigRegister: false
TestRigLaunchDbUpgrade: false
TestRigClientCode: EDI

TestRigWebSites: ZClientWebEDI
TestRigWebServer: localhost
TestRigWebDomain: {webRootDomain}
TestRigOverrideSanityCheck: true
TestRigWebServerInstallPath: {webServerInstallPath}
";

				var mockCw1InstanceClass = new Mock<ICargoWiseOneInstanceClass>();
				mockCw1InstanceClass
					.Setup(c => c.FindInstance(shelfName, It.IsAny<DirectorySearchOptions>()))
					.Returns(Mock.Of<ICargoWiseOneInstanceSearchResult>(x => x.GetDirectoryEntry() == Mock.Of<ICargoWiseOneInstanceEntry>()));
				var disposeCallSequence = new ConcurrentQueue<string>();

				var unInstallingWebMock = new Mock<IDisposable>();
				unInstallingWebMock
					.Setup(x => x.Dispose())
					.Callback(() => disposeCallSequence.Enqueue("unInstallingWebMock"));
				var droppingDbMock = new Mock<IDisposable>();
				droppingDbMock
					.Setup(x => x.Dispose())
					.Callback(() => disposeCallSequence.Enqueue("droppingDbMock"));

				var logger = new Mock<ITaskLogger>();
				logger
					.Setup(x => x.RecordTask("Uninstalling web applications"))
					.Callback<string>(y =>
					{
						Thread.Sleep(10000);
					})
					.Returns(() => unInstallingWebMock.Object);
				logger
					.Setup(x => x.RecordTask("Dropping Databases"))
					.Returns(() => droppingDbMock.Object);
				var buildDeployer = CreateDeployer(logger.Object, mockCw1InstanceClass.Object);
				using (var sourcePathDirectory = new TempDirectory())
				using (var binPathDirectory = new TempDirectory())
				{
					SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");

					// Act
					AssertNoExceptionThrown(() => buildDeployer.TeardownTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments)));

					// Assert
					var expectedDeconstructionOrder = new[] { "unInstallingWebMock", "droppingDbMock" };
					unInstallingWebMock.Verify(x => x.Dispose(), Times.Once);
					droppingDbMock.Verify(x => x.Dispose(), Times.Once);
					AssertSequencesEqual(expectedDeconstructionOrder, disposeCallSequence);
				}
			}
			finally
			{
				using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					var dropLogin = $@"IF EXISTS (
SELECT
	loginname
FROM
	master.dbo.syslogins
WHERE
	name = '{login}')

	BEGIN
		DROP login {login}
	END";

					connection.ExecuteNonQuery(dropLogin);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestRetryTearDownWebsiteWhenErrorHappens()
		{
			var shelfName = "1A26D3C02E9E48C68996";
			var dbName = "SH0" + shelfName;
			var sid = new Guid("6060E847-C6B3-4405-930A-46C1FA2545B9").ToString("N", CultureInfo.InvariantCulture);
			var login = $"EnterpriseDbUser_{dbName}_{sid}";
			try
			{
				// Arrange
				var unexpectedException = new InvalidOperationException("Unexpected whatever type exception");
				var webRootDomain = shelfName;

				using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					using (var command = connection.Command(Invariant($"CREATE LOGIN [{login}] WITH PASSWORD = '', CHECK_POLICY = OFF, DEFAULT_LANGUAGE = us_english, SID = 0x{sid}")))
					{
						command.ExecuteNonQuery();
					}
				}

				var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");
				var taskComments = $@"JH6 11-Jan-16 16:33:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigCreateIcon: false
TestRigRegister: false
TestRigLaunchDbUpgrade: false
TestRigClientCode: EDI

TestRigWebSites: ZClientWebEDI
TestRigWebServer: localhost
TestRigWebDomain: {webRootDomain}
TestRigOverrideSanityCheck: true
TestRigWebServerInstallPath: {webServerInstallPath}
";

				var mockCw1InstanceClass = new Mock<ICargoWiseOneInstanceClass>();
				mockCw1InstanceClass
					.Setup(c => c.FindInstance(shelfName, It.IsAny<DirectorySearchOptions>()))
					.Returns(Mock.Of<ICargoWiseOneInstanceSearchResult>(x => x.GetDirectoryEntry() == Mock.Of<ICargoWiseOneInstanceEntry>()));

				var logger = new Mock<ITaskLogger>();
				logger
					.Setup(x => x.RecordTask("Uninstalling web applications"))
					.Throws(unexpectedException);
				var buildDeployer = CreateDeployer(logger.Object, mockCw1InstanceClass.Object);
				using (var sourcePathDirectory = new TempDirectory())
				using (var binPathDirectory = new TempDirectory())
				{
					SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");

					//Act
					AssertNoExceptionThrown(() => buildDeployer.TeardownTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments)));

					// Assert
					const int expectedRetries = 3;
					logger.Verify(x => x.RecordTask("Uninstalling web applications"), Times.Exactly(expectedRetries));
				}
			}
			finally
			{
				using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					var dropLogin = $@"IF EXISTS (
SELECT
	loginname
FROM
	master.dbo.syslogins
WHERE
	name = '{login}')

	BEGIN
		DROP login {login}
	END";

					connection.ExecuteNonQuery(dropLogin);
				}
			}
		}

		public void TestAutoDeployLatestBuildArchiveKeepsLastVersionForEachRing()
		{
			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			using (var archivePath = new TempDirectory())
			using (var packageArchivePath = new TempDirectory())
			{
				var binDir = binPathDirectory.CreateChildDirectory("bin.16.4.1.69");
				SetupTestFiles(sourcePathDirectory.DirectoryName, binDir, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP", buildMasterEdp: true);
				var buildDeployer = new MockAutoDeployLatestBuildDeployer(new Mock<ITaskLogger>().Object, archivePath, packageArchivePath);
				buildDeployer.AutoDeployLatestBuild("RELEASE", ReleaseBuildContent.IBPMasterPackageDeploymentConfiguration, sourcePathDirectory.DirectoryName, binDir);
				Assert(Directory.Exists(Path.Combine(archivePath.DirectoryName, "ALP_16.4.1.69_20160401110614")));

				binDir = binPathDirectory.CreateChildDirectory("bin.16.4.1.70");
				SetupTestFiles(sourcePathDirectory.DirectoryName, binDir, "16.4.1.70", new DateTime(2016, 4, 1, 11, 6, 14), "ALP", buildMasterEdp: true);
				buildDeployer = new MockAutoDeployLatestBuildDeployer(new Mock<ITaskLogger>().Object, archivePath, packageArchivePath);
				buildDeployer.AutoDeployLatestBuild("RELEASE", ReleaseBuildContent.IBPMasterPackageDeploymentConfiguration, sourcePathDirectory.DirectoryName, binDir);
				Assert(Directory.Exists(Path.Combine(archivePath.DirectoryName, "ALP_16.4.1.70_20160401110614")));
				Assert(!Directory.Exists(Path.Combine(archivePath.DirectoryName, "ALP_16.4.1.69_20160401110614")));

				binDir = binPathDirectory.CreateChildDirectory("bin.16.1.2.3");
				SetupTestFiles(sourcePathDirectory.DirectoryName, binDir, "16.1.2.3", new DateTime(2016, 4, 1, 11, 6, 14), "DPR", buildMasterEdp: true);
				buildDeployer = new MockAutoDeployLatestBuildDeployer(new Mock<ITaskLogger>().Object, archivePath, packageArchivePath);
				buildDeployer.AutoDeployLatestBuild("RELEASE", ReleaseBuildContent.IBPMasterPackageDeploymentConfiguration, sourcePathDirectory.DirectoryName, binDir);
				Assert(Directory.Exists(Path.Combine(archivePath.DirectoryName, "DPR_16.1.2.3_20160401110614")));
				Assert(Directory.Exists(Path.Combine(archivePath.DirectoryName, "ALP_16.4.1.70_20160401110614")));
			}
		}

		public void TestAutoDeployLatestBuildDoesNotThrowWhenFailingToDeleteOldArchive()
		{
			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			using (var archivePath = new TempDirectory())
			using (var packageArchivePath = new TempDirectory())
			{
				var binDir = binPathDirectory.CreateChildDirectory("bin.16.4.1.69");
				SetupTestFiles(sourcePathDirectory.DirectoryName, binDir, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP", buildMasterEdp: true);
				var buildDeployer = new MockAutoDeployLatestBuildDeployer(new Mock<ITaskLogger>().Object, archivePath, packageArchivePath);
				buildDeployer.AutoDeployLatestBuild("RELEASE", ReleaseBuildContent.IBPMasterPackageDeploymentConfiguration, sourcePathDirectory.DirectoryName, binDir);
				Assert(Directory.Exists(Path.Combine(archivePath.DirectoryName, "ALP_16.4.1.69_20160401110614")));

				using (var lockedFile = File.Open(Directory.GetFiles(Path.Combine(archivePath.DirectoryName, "ALP_16.4.1.69_20160401110614")).First(), FileMode.Open, FileAccess.ReadWrite, FileShare.None))
				{
					binDir = binPathDirectory.CreateChildDirectory("bin.16.4.1.70");
					SetupTestFiles(sourcePathDirectory.DirectoryName, binDir, "16.4.1.70", new DateTime(2016, 4, 1, 11, 6, 14), "ALP", buildMasterEdp: true);
					buildDeployer = new MockAutoDeployLatestBuildDeployer(new Mock<ITaskLogger>().Object, archivePath, packageArchivePath);
					buildDeployer.AutoDeployLatestBuild("RELEASE", ReleaseBuildContent.IBPMasterPackageDeploymentConfiguration, sourcePathDirectory.DirectoryName, binDir);
					Assert(Directory.Exists(Path.Combine(archivePath.DirectoryName, "ALP_16.4.1.70_20160401110614")));
					Assert(Directory.Exists(Path.Combine(archivePath.DirectoryName, "ALP_16.4.1.69_20160401110614")));

					binDir = binPathDirectory.CreateChildDirectory("bin.16.4.1.71");
					SetupTestFiles(sourcePathDirectory.DirectoryName, binDir, "16.4.1.71", new DateTime(2016, 4, 1, 11, 6, 14), "ALP", buildMasterEdp: true);
					buildDeployer = new MockAutoDeployLatestBuildDeployer(new Mock<ITaskLogger>().Object, archivePath, packageArchivePath);
					buildDeployer.AutoDeployLatestBuild("RELEASE", ReleaseBuildContent.IBPMasterPackageDeploymentConfiguration, sourcePathDirectory.DirectoryName, binDir);
					Assert(Directory.Exists(Path.Combine(archivePath.DirectoryName, "ALP_16.4.1.71_20160401110614")));
					Assert(!Directory.Exists(Path.Combine(archivePath.DirectoryName, "ALP_16.4.1.70_20160401110614")));
					Assert(Directory.Exists(Path.Combine(archivePath.DirectoryName, "ALP_16.4.1.69_20160401110614")));
				}
			}
		}

		public void TestAutoDeployTestedShelfAndTearDown()
		{
			var shelfName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var dbName = "SH0" + shelfName;

			var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");
			var taskComments = $@"BRE 11-Jan-16 16:33:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigRegister: false
TestRigLaunchDbUpgrade: false";

			var mockCw1InstanceClass = new Mock<ICargoWiseOneInstanceClass>();
			mockCw1InstanceClass.Setup(c => c.AddNewInstance(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<System.DirectoryServices.DirectoryEntry>())).Returns(new Mock<ICargoWiseOneInstanceEntry>().Object);
			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			{
				try
				{
					SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");

					var logger = new TestLogger();
					CreateDeployer(logger, mockCw1InstanceClass.Object).AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
					var log = logger.ToString();

					AssertDatabaseExists(true, dbName);
					AssertDeploymentIDLogged(log, dbName);
					AssertRestrictedLoginExists(dbName);
					AssertDatabaseLoginDoesNotExist($"EnterpriseDbUser_{dbName}_%", "Staff Login should not be created by Auto Deployment.");

					using (var connection = Db.NewAdminConnection(dbName))
					{
						var sqlConnection = ((IDbConnectionInternals)connection).ADOConnection;
						var upgradeManager = new SqlUpgradeManager(new UpgradeSqlContext(sqlConnection, sqlConnection.DataSource, sqlConnection.Database));

						AssertEquals(log, new Version(16, 4, 1, 69), upgradeManager.QueryCurrentVersion().Version);
					}

					mockCw1InstanceClass.Verify(c => c.AddNewInstance(shelfName, Db.ServerName, dbName, It.Is<System.DirectoryServices.DirectoryEntry>(e => e.Path == "LDAP://CN=SH0,CN=WiseTech Global,CN=Program Data,DC=sand,DC=wtg,DC=zone")));
				}
				finally
				{
					var mockCw1SearchResult = new Mock<ICargoWiseOneInstanceSearchResult>();
					var mockCw1Entry = new Mock<ICargoWiseOneInstanceEntry>();
					mockCw1InstanceClass.Setup(c => c.FindInstance(shelfName, It.Is<DirectorySearchOptions>(o => o.DomainName == "sand.wtg.zone"))).Returns(mockCw1SearchResult.Object);
					mockCw1SearchResult.Setup(r => r.GetDirectoryEntry()).Returns(mockCw1Entry.Object);
					MockStaffLoginCreationDuringFunctionalTesting();

					var logger = new TestLogger();
					CreateDeployer(logger, mockCw1InstanceClass.Object).TeardownTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));

					AssertDatabaseExists(false, dbName);
					AssertDatabaseLoginDoesNotExist($"{dbName}_%");
					AssertDatabaseLoginDoesNotExist($"EnterpriseDbUser_{dbName}_%");
					mockCw1Entry.Verify(e => e.Delete(), Times.Once());
				}
			}

			void AssertRestrictedLoginExists(string dbName)
			{
				using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					CombineAssertions(() =>
					{
						AssertDatabaseUserExists(connection, dbName, $"{dbName}_RestrictedWriterLogin", expectedResult: true);
						AssertDatabaseUserExists(connection, dbName, $"{dbName}_RestrictedReaderLogin", expectedResult: true);
					});
				}
			}

			void MockStaffLoginCreationDuringFunctionalTesting()
			{
				using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					string sid = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
					using (var command = connection.Command(Invariant($"CREATE LOGIN [EnterpriseDbUser_{dbName}_{sid}] WITH PASSWORD = '', CHECK_POLICY = OFF, DEFAULT_LANGUAGE = us_english, SID = 0x{sid}")))
					{
						command.ExecuteNonQuery();
					}
				}
			}
		}

		public void TestRegistryEntriesCheck()
		{
			RegistryEntriesTest((buildDeployer, dbName) =>
			{
				using (var connection = Db.NewAdminConnection(dbName))
				{
					AssertRegistryEntry(connection, "key1", "value1");
					AssertRegistryEntry(connection, "key3", "value3");
				}
			},
			"key1|value1, , key3 |  value3");
		}

		public void TestRegistryEntriesStringUpdatingDefault() => RegistryEntriesUpdating("AUCustomsSenderID|should change to this");
		public void TestRegistryEntriesStringUpdatingSTR() => RegistryEntriesUpdating("AUCustomsSenderID|str|should change to this");

		public void RegistryEntriesUpdating(string setting, string key = "AUCustomsSenderID")
		{
			RegistryEntriesTest((buildDeployer, dbName) =>
			{
				using (var connection = Db.NewAdminConnection(dbName))
				{
					// check updating existing db registry entry
					AssertRegistryEntry(connection, key, "should change to this");

					// check function directly
					buildDeployer.SetRegistryValueString(connection, key, "then this");
					AssertRegistryEntry(connection, key, "then this");

					buildDeployer.SetRegistryValueString(connection, key, "finally this");
					AssertRegistryEntry(connection, key, "finally this");
				}
			},
			setting);
		}

		public void TestRegistryEntriesBoolTrue() => RegistryEntriesBool("SomeBoolKey|bool|true", true, "SomeboolKey");
		public void TestRegistryEntriesBoolFalse() => RegistryEntriesBool("SomeBoolOtherKey|bool|false", false, "SomeboolOtherKey");

		public void TestRegistryEntriesStrArrayLength0()
		{
			var expectedValues1 = Array.Empty<string>();
			var expectedValues2 = Array.Empty<string>();
			RegistryEntriesStrArray("Somestr_arrayKey|str_array| ", expectedValues1, expectedValues2, "Somestr_arrayKey");
			RegistryEntriesStrArray("Somestr_arrayOtherKey|str_array| ", expectedValues1, expectedValues2, "Somestr_arrayOtherKey");
		}

		public void TestRegistryEntriesStrArrayLength1()
		{
			var expectedValues1 = new string[] { "should change to this" };
			var expectedValues2 = new string[] { "then this" };
			RegistryEntriesStrArray("Somestr_arrayKey|str_array| should change to this ", expectedValues1, expectedValues2, "Somestr_arrayKey");
			RegistryEntriesStrArray("Somestr_arrayOtherKey|str_array| should change to this ", expectedValues1, expectedValues2, "Somestr_arrayOtherKey");
		}

		public void TestRegistryEntriesStrArrayLength2()
		{
			var expectedValues1 = new string[] { "should change to this", "and this" };
			var expectedValues2 = new string[] { "then this", "and also this" };
			RegistryEntriesStrArray("Somestr_arrayKey|str_array| should change to this ; and this ", expectedValues1, expectedValues2, "Somestr_arrayKey");
			RegistryEntriesStrArray("Somestr_arrayOtherKey|str_array| should change to this ; and this ", expectedValues1, expectedValues2, "Somestr_arrayOtherKey");
		}

		public void RegistryEntriesBool(string setting, bool expectedValue, string key)
		{
			RegistryEntriesTest((buildDeployer, dbName) =>
			{
				using (var connection = Db.NewAdminConnection(dbName))
				{
					// check updating existing db registry entry
					AssertRegistryEntryBool(connection, key, expectedValue);

					// check function directly
					buildDeployer.SetRegistryValueBool(connection, key, !expectedValue);
					AssertRegistryEntryBool(connection, key, !expectedValue);
				}
			},
			setting);
		}

		public void RegistryEntriesStrArray(string setting, string[] expectedValues1, string[] expectedValues2, string key)
		{
			RegistryEntriesTest((buildDeployer, dbName) =>
			{
				using (var connection = Db.NewAdminConnection(dbName))
				{
					// check updating existing db registry entry
					AssertResistryEntryStrArray(connection, key, expectedValues1);

					// check function directly
					buildDeployer.SetRegistryValueStringArray(connection, key, expectedValues2);
					AssertResistryEntryStrArray(connection, key, expectedValues2);
				}
			},
			setting);
		}

		[UseSnapshotProtection]
		[ExpectNoExceptions]
		public void TestRegisterProductDisposesOfConnection()
		{
			// Arrange
			mockProductKeyService
				.Setup(o => o.GetInternalTestKey("Test", "Server1", "Database1"))
				.Returns("TestProductKey");

			mockProductRegistrationDisposable?.Dispose(); // Don't use mock, testing the main implementation
			mockProductRegistrationDisposable = null;

			var mockRegistrationServiceClient = new Mock<IRegistrationServiceClient>();
			var outputStatusCode = HttpStatusCode.OK;
			mockRegistrationServiceClient
				.Setup(x =>
					x.Register(
						It.IsAny<RegisterRequest>(),
						out outputStatusCode,
						It.IsAny<CancellationToken>(),
						It.IsAny<int>()))
				.Returns(new RegisterResponse { Key = "Response-Key" });

			var productRegister = new ProductRegister { Client = mockRegistrationServiceClient.Object };
			var errorReporterMock = new Mock<IErrorReporter>();

			using (ObjectFactory.Substitute<IProductRegistration>(productRegister))
			using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
			{
				var buildDeployer = CreateDeployer(Mock.Of<ITaskLogger>());

				// Act
				Task.Run(() =>
				{
					buildDeployer.RegisterProduct("Test", "Server1", "Database1");
				}).Wait();

				// Assert
				errorReporterMock
					.Verify(e => e
						.ReportDeveloperExceptionOrHandleSilently(
							It.IsAny<string>(),
							It.Is<string>(str => str.Contains("Attempt to use Db.Connection without using Db.DisposableActionForDbConnection()")),
							It.IsAny<Exception>())
						, Times.Never);
			}
		}

		public void TestProductRegistrationFailureFailsTheDeployment()
		{
			CombineAssertions(() =>
			{
				TestProductRegistrationFailureFailsTheDeployment(ProductRegistrationRegisterResult.ProductKeyNotFound);
				TestProductRegistrationFailureFailsTheDeployment(ProductRegistrationRegisterResult.ProductKeyUnavailable);
				TestProductRegistrationFailureFailsTheDeployment(ProductRegistrationRegisterResult.Fail);
				TestProductRegistrationFailureFailsTheDeployment(ProductRegistrationRegisterResult.Timeout);
				TestProductRegistrationFailureFailsTheDeployment(ProductRegistrationRegisterResult.Error);
			});

			void TestProductRegistrationFailureFailsTheDeployment(ProductRegistrationRegisterResult productRegistrationRegisterResult)
			{
				const string testEnterpriseCode = "Test";
				const string testServerName = "Server1";
				const string testDatabaseName = "Database1";
				const string testProductKey = "TestProductKey";

				// Arrange
				mockProductKeyService
					.Setup(o => o.GetInternalTestKey(testEnterpriseCode, testServerName, testDatabaseName))
					.Returns(testProductKey);

				mockProductRegistration
					.Setup(o => o.Register("TestProductKey", CancellationToken.None, BuildDeployer.DefaultRegistrationTimeoutMs))
					.Returns(productRegistrationRegisterResult);

				var buildDeployer = CreateDeployer(Mock.Of<ITaskLogger>());

				// Act + Assert
				AssertExceptionThrown<DeploymentFailedException>("Deployment failed exception is expected.", () => buildDeployer.RegisterProduct(testEnterpriseCode, testServerName, testDatabaseName));
			}
		}

		void AssertResistryEntryStrArray(DbConnection connection, string code, string[] expectedValues)
		{
			var stringBuilder = new StringBuilder();
			using (var output = new StringWriter(stringBuilder))
			using (var writer = XmlWriter.Create(output))
			{
				new DataContractSerializer(typeof(string[])).WriteObject(writer, expectedValues);
				writer.Flush();
				var expectedXml = stringBuilder.ToString();

				AssertRegistryEntry(connection, code, expectedXml);
			}
		}

		void AssertRegistryEntryBool(DbConnection connection, string code, bool expectedValue) => AssertRegistryEntry(connection, code, expectedValue ? bool.TrueString : bool.FalseString);

		void AssertRegistryEntryInteger(DbConnection connection, string code, int expectedValue) => AssertRegistryEntry(connection, code, expectedValue.ToString());

		void AssertRegistryEntry(DbConnection connection, string code, object expectedValue)
		{
			AssertEquals(expectedValue, GetRegistryValue(connection, code));
		}

		object GetRegistryValue(DbConnection connection, string code)
		{
			var selectCommand = @"select cast(SD_BinaryValue as nvarchar(max)) from dbo.StmData where SD_Name = @code";

			using var command = connection.Command(selectCommand);
			command.AddParameter("@code", SqlDbType.VarChar, code);

			return command.ExecuteScalar();
		}

		public void RegistryEntriesTest(Action<BuildDeployer, string> test, string registryEntries = "")
		{
			var shelfName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var dbName = "SH0" + shelfName;
			var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");
			var taskComments = $@"DML 25-Jul-16 12:06:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigCreateIcon: false
TestRigRegister: false
TestRigLaunchDbUpgrade: false
TestRigClientCode: EDI";

			if (!string.IsNullOrEmpty(registryEntries))
			{
				taskComments += "\r\nTestRigRegistryEntries: " + registryEntries;
			}

			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			using (var unpackDirectory = new TempDirectory())
			{
				try
				{
					SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");

					var logger = new TestLogger();
					var buildDeployer = CreateDeployer(logger);
					buildDeployer.AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
					var log = logger.ToString();

					test(buildDeployer, dbName);
				}
				finally
				{
					var logger = new TestLogger();
					CreateDeployer(logger).TeardownTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));

					AssertDatabaseExists(false, dbName);
				}
			}
		}

		public void TestAutoDeployTestedShelfWithTestRigOrigin()
		{
			var expectedOrigin = "WI00761176";
			var shelfName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var dbName = "SH0" + shelfName;
			var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");
			var taskComments = $@"TestRigOrigin: {expectedOrigin}
DML 25-Jul-16 12:06:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigCreateIcon: false
TestRigRegister: false
TestRigLaunchDbUpgrade: false
TestRigClientCode: EDI";

			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			using (var unpackDirectory = new TempDirectory())
			{
				try
				{
					SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");

					var logger = new TestLogger();
					var buildDeployer = CreateDeployer(logger);
					buildDeployer.AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
					var log = logger.ToString();

					using (var connection = Db.NewAdminConnection(dbName))
					{
						AssertRegistryEntry(connection, "TEST_RIG_ORIGIN", expectedOrigin);
					}
				}
				finally
				{
					var logger = new TestLogger();
					CreateDeployer(logger).TeardownTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));

					AssertDatabaseExists(false, dbName);
				}
			}
		}

		public void TestAutoDeployClientSpecificTestedShelfAndTearDown()
		{
			var shelfName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var dbName = "SH0" + shelfName;
			var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");
			var taskComments = $@"DML 25-Jul-16 12:06:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigCreateIcon: false
TestRigRegister: false
TestRigLaunchDbUpgrade: false
TestRigClientCode: EDI";

			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			using (var unpackDirectory = new TempDirectory())
			{
				try
				{
					SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");

					var logger = new TestLogger();
					CreateDeployer(logger).AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
					var log = logger.ToString();
					AssertDeploymentIDLogged(log, dbName);

					AssertDatabaseExists(true, dbName);

					using (var connection = Db.NewAdminConnection(dbName))
					{
						var sqlConnection = ((IDbConnectionInternals)connection).ADOConnection;
						var upgradeManager = new SqlUpgradeManager(new UpgradeSqlContext(sqlConnection, sqlConnection.DataSource, sqlConnection.Database));

						UpgradeInfo upgrade = upgradeManager.QueryCurrentVersion();

						AssertEquals(log, new Version(16, 4, 1, 69), upgrade.Version);

						using (var tempFile = TempFile.New())
						{
							upgradeManager.DownloadUpgradePackageFile(upgrade.PK, tempFile.Filename);
							EdpFile.Unpack(tempFile.Filename, unpackDirectory);
							string applicationPath = Path.Combine(unpackDirectory, "Distribution", "Application");
							Assert("ZClientEDI dll should exist", File.Exists(Path.Combine(applicationPath, "ZClientEDI.dll")));
						}

						using (var cmd = connection.Command("select cast(SD_BinaryValue as nvarchar(max)) from dbo.StmData where SD_Name = 'EXPECTED_CLIENT_DLL'"))
						{
							AssertEquals("ZClientEDI", cmd.ExecuteScalar());
						}

						using (var cmd = connection.Command("select cast(SD_BinaryValue as nvarchar(max)) from dbo.StmData where SD_Name = 'ClientDocumentName'"))
						{
							AssertEquals("EDI", cmd.ExecuteScalar());
						}
					}
				}
				finally
				{
					var logger = new TestLogger();
					CreateDeployer(logger).TeardownTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));

					AssertDatabaseExists(false, dbName);
				}
			}
		}

		[RequiresSoftware(RequiredSoftware.IsVM)]
		public void TestAutoDeployTestedShelfWithWebClientApplication()
		{
			var shelfName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var dbName = "SH0" + shelfName;
			var webRootDomain = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");
			var taskComments = $@"DML 25-Jul-16 12:06:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigCreateIcon: false
TestRigRegister: false
TestRigLaunchDbUpgrade: false
TestRigClientCode: EDI

TestRigWebSites: ZClientWebEDI
TestRigWebServer: localhost
TestRigWebDomain: {webRootDomain}
TestRigOverrideSanityCheck: true
TestRigWebServerInstallPath: {webServerInstallPath}
";

			Process.Start("winrm", "quickconfig -quiet").WaitForExit();

			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			using (var unpackDirectory = new TempDirectory())
			{
				try
				{
					SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");

					var logger = new TestLogger();
					CreateDeployer(logger).AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
					var log = logger.ToString();
					AssertDeploymentIDLogged(log, dbName);

					AssertDatabaseExists(true, dbName);

					using (var connection = Db.NewAdminConnection(dbName))
					{
						var sqlConnection = ((IDbConnectionInternals)connection).ADOConnection;
						var upgradeManager = new SqlUpgradeManager(new UpgradeSqlContext(sqlConnection, sqlConnection.DataSource, sqlConnection.Database));

						UpgradeInfo upgrade = upgradeManager.QueryCurrentVersion();

						AssertEquals(log, new Version(16, 4, 1, 69), upgrade.Version);

						using (var tempFile = TempFile.New())
						{
							upgradeManager.DownloadUpgradePackageFile(upgrade.PK, tempFile.Filename);
							EdpFile.Unpack(tempFile.Filename, unpackDirectory);
							string applicationPath = Path.Combine(unpackDirectory, "Distribution", "Application");
							Assert("ZClientEDI dll should exist", File.Exists(Path.Combine(applicationPath, "ZClientEDI.dll")));
						}

						using (var cmd = connection.Command("select cast(SD_BinaryValue as nvarchar(max)) from dbo.StmData where SD_Name = 'EXPECTED_CLIENT_DLL'"))
						{
							AssertEquals("ZClientEDI", cmd.ExecuteScalar());
						}

						using (var cmd = connection.Command("select cast(SD_BinaryValue as nvarchar(max)) from dbo.StmData where SD_Name = 'ClientDocumentName'"))
						{
							AssertEquals("EDI", cmd.ExecuteScalar());
						}
					}
					AssertSiteExists(log, true, webRootDomain, "ZClientWebEDI", Db.ServerName, dbName);
				}
				finally
				{
					var logger = new TestLogger();
					CreateDeployer(logger).TeardownTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
					var log = logger.ToString();

					AssertDatabaseExists(false, dbName);
					AssertSiteExists(log, false, webRootDomain, "ZClientWebEDI", Db.ServerName, dbName);
				}
			}
		}

		public void TestAutoDeployTestedShelfWithWebClientApplicationWithoutClientCode()
		{
			var shelfName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var webRootDomain = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");
			var taskComments = $@"DML 25-Jul-16 12:06:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigCreateIcon: false
TestRigRegister: false
TestRigLaunchDbUpgrade: false
TestRigWebSites: ZClientWebEDI
TestRigWebServer: localhost
TestRigWebDomain: {webRootDomain}
TestRigOverrideSanityCheck: true
TestRigWebServerInstallPath: {webServerInstallPath}
";

			var exception = AssertExceptionThrown<ArgumentException>(() => new TestedShelfDeploymentOptions(new TaskInfo("testshelf", "owner", taskComments)));
			AssertEquals("Cannot Deploy Website ZClientWebEDI without matching TestRigClientCode", exception.Message);
		}

		public void TestAutoDeployTestedShelfWithWebClientApplicationWithIncorrectClientCode()
		{
			var shelfName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var dbName = "SH0" + shelfName;
			var webRootDomain = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");
			var taskComments = $@"DML 25-Jul-16 12:06:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigCreateIcon: false
TestRigRegister: false
TestRigLaunchDbUpgrade: false
TestRigClientCode: ABC
TestRigWebSites: ZClientWebEDI
TestRigWebServer: localhost
TestRigWebDomain: {webRootDomain}
TestRigOverrideSanityCheck: true
TestRigWebServerInstallPath: {webServerInstallPath}
";

			var exception = AssertExceptionThrown<ArgumentException>(() => new TestedShelfDeploymentOptions(new TaskInfo("testshelf", "owner", taskComments)));
			AssertEquals("TestRigWebSites: Client web site ZClientWebEDI should end with client code ABC", exception.Message);
		}

		public void TestAutoDeployTestedShelfWithWebClientApplicationWithMultipleClients()
		{
			var shelfName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var dbName = "SH0" + shelfName;
			var webRootDomain = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");
			var taskComments = $@"DML 25-Jul-16 12:06:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigCreateIcon: false
TestRigRegister: false
TestRigLaunchDbUpgrade: false
TestRigClientCode: ABC
TestRigWebSites: ZClientWebEDI, ZClientWebABC
TestRigWebServer: localhost
TestRigWebDomain: {webRootDomain}
TestRigOverrideSanityCheck: true
TestRigWebServerInstallPath: {webServerInstallPath}
";

			var exception = AssertExceptionThrown<ArgumentException>(() => new TestedShelfDeploymentOptions(new TaskInfo("testshelf", "owner", taskComments)));
			AssertEquals("TestRigWebSites: Only one client web site is supported but multiple were requested [ZClientWebEDI, ZClientWebABC]", exception.Message);
		}

		public void TestAutoDeployClientSpecificTestedShelfRemovesBiServerDetails()
		{
			var shelfName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var dbName = "SH0" + shelfName;
			var backupPath = CopyTestFileResource("OdysseyWithBiServerDetails.bak");
			var taskComments = $@"DML 25-Jul-16 12:06:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigCreateIcon: false
TestRigRegister: false
TestRigLaunchDbUpgrade: false
TestRigClientCode: EDI";

			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			using (var unpackDirectory = new TempDirectory())
			{
				try
				{
					SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");

					var logger = new TestLogger();
					CreateDeployer(logger).AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
					var log = logger.ToString();
					AssertDeploymentIDLogged(log, dbName);

					AssertDatabaseExists(true, dbName);

					using (var connection = Db.NewAdminConnection(dbName))
					using (var command = connection.Command(@"SELECT SD_Name, CONVERT(NVARCHAR(MAX), SD_BinaryValue) AS Value FROM dbo.StmData WHERE SD_Name IN ('BiServers', 'BiAuditServer', 'BiDataWarehouseServer', 'BiAnalysisServer', 'BiSsrsWebServiceUrl', 'BiPowerBiWebPortalUrl')"))
					{
						CombineAssertions("BiServer details should be removed during restore.", () =>
						{
							var reader = command.ExecuteReader();
							while (reader.Read())
							{
								var name = reader["SD_Name"].ToString();
								var value = reader["Value"];
								Assert($"{name} = '{(value == DBNull.Value ? "" : value.ToString())}'", value == DBNull.Value);
							}
						});
					}
				}
				finally
				{
					var logger = new TestLogger();
					CreateDeployer(logger).TeardownTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));

					AssertDatabaseExists(false, dbName);
				}
			}
		}

		public void TestDeployDatabaseWithCdc()
		{
			var shelfName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var dbName = "SH0" + shelfName;
			var backupPath = CopyTestFileResource("OdysseyWithBiServerDetails.bak");
			var taskComments = $@"DML 25-Jul-16 12:06:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigCreateIcon: false
TestRigRegister: false
TestRigLaunchDbUpgrade: false
TestRigClientCode: EDI
TestRigEnableAudit: true";

			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			using (var unpackDirectory = new TempDirectory())
			{
				try
				{
					SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");

					var logger = new TestLogger();
					CreateDeployer(logger).AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
					var log = logger.ToString();
					AssertDeploymentIDLogged(log, dbName);

					AssertDatabaseExists(true, dbName);
					using (var connection = Db.NewAdminConnection(dbName))
					{
						var biDisableChangeDataCapture = DbRegistry.BiDisableChangeDataCapture.LoadValue(connection);
						AssertEquals("Is BiDisableChangeDataCapture False?", false, biDisableChangeDataCapture);
					}
				}
				finally
				{
					var logger = new TestLogger();
					CreateDeployer(logger).TeardownTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));

					AssertDatabaseExists(false, dbName);
				}
			}
		}

		public void TestDeployDatabaseWithoutCdc()
		{
			var shelfName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var dbName = "SH0" + shelfName;
			var backupPath = CopyTestFileResource("OdysseyWithBiServerDetails.bak");
			var taskComments = $@"DML 25-Jul-16 12:06:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigCreateIcon: false
TestRigRegister: false
TestRigLaunchDbUpgrade: false
TestRigClientCode: EDI";

			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			using (var unpackDirectory = new TempDirectory())
			{
				try
				{
					SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");

					var logger = new TestLogger();
					CreateDeployer(logger).AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
					var log = logger.ToString();
					AssertDeploymentIDLogged(log, dbName);

					AssertDatabaseExists(true, dbName);
					using (var connection = Db.NewAdminConnection(dbName))
					{
						var biDisableChangeDataCapture = DbRegistry.BiDisableChangeDataCapture.LoadValue(connection);
						AssertEquals("Is BiDisableChangeDataCapture True?", true, biDisableChangeDataCapture);
					}
				}
				finally
				{
					var logger = new TestLogger();
					CreateDeployer(logger).TeardownTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));

					AssertDatabaseExists(false, dbName);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAutoDeployTestedShelfAndTearDownWithoutBackup()
		{
			var shelfName = Guid.NewGuid().ToString();
			var dbName = "ED80D30F";
			var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");
			var taskComments = $@"
TestRigSqlServer: {Db.ServerName}
TestRigDatabaseName: {dbName}
TestRigCreateIcon: false
TestRigRegister: false
TestRigLaunchDbUpgrade: false";
			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			{
				try
				{
					SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");

					int emptyDbTableCount;
					using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
					{
						using (var cmd = connection.Command($"CREATE DATABASE [{dbName}]"))
						{
							cmd.ExecuteNonQuery();
						}

						using (((ICurrentDbControl)connection).UseDatabase(dbName))
						using (var cmd = connection.Command(DataUtils.SQL_InitialTablesForEmptyDatabase()))
						{
							cmd.ExecuteNonQuery();
						}

						using (var cmd = connection.Command($"select count(*) from [{dbName}].sys.tables"))
						{
							emptyDbTableCount = cmd.ExecuteNonQuery();
						}
					}

					using (var connection = Db.NewAdminConnection(dbName))
					{
						var sqlConnection = ((IDbConnectionInternals)connection).ADOConnection;
						var upgradeManager = new SqlUpgradeManager(new UpgradeSqlContext(sqlConnection, sqlConnection.DataSource, sqlConnection.Database));

						upgradeManager.UploadUpgradePackage(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\MasterFiles\Business\MasterFiles.Business\System\StmUpgrade\testing.edp"), "CUR", "Old Package", null);
					}

					var logger = new TestLogger();
					CreateDeployer(logger).AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
					var log = logger.ToString();
					AssertDeploymentIDLogged(log, dbName);

					using (var connection = Db.NewAdminConnection(dbName))
					{
						var sqlConnection = ((IDbConnectionInternals)connection).ADOConnection;
						var upgradeManager = new SqlUpgradeManager(new UpgradeSqlContext(sqlConnection, sqlConnection.DataSource, sqlConnection.Database));

						AssertEquals(log, new Version(16, 4, 1, 69), upgradeManager.QueryCurrentVersion().Version);

						using (var cmd = connection.Command($"select count(*) from [{dbName}].sys.tables"))
						{
							AssertEquals(emptyDbTableCount, cmd.ExecuteNonQuery());
						}
					}
				}
				finally
				{
					var logger = new TestLogger();
					CreateDeployer(logger).TeardownTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));

					AssertDatabaseExists(true, dbName);

					using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
					using (var cmd = connection.Command($"DROP DATABASE [{dbName}]"))
					{
						cmd.ExecuteNonQuery();
					}
				}
			}
		}

		public void TestAutoDeployTestedShelfNoOptionsSpecified()
		{
			var logger = new TestLogger();
			AssertExceptionThrown<DeploymentCancelledException>(() => CreateDeployer(logger).AutoDeployTestedShelf("DEBUG", string.Empty, string.Empty, string.Empty, new TaskInfo("bla", "bla", "bla")));
		}

		[RequiresSoftware(RequiredSoftware.IsVM)]
		public void TestAutoDeployTestedShelfWithWeb()
		{
			var shelfName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var dbName = "SH0" + shelfName;
			var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");
			var webRootDomain = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var taskComments = $@"BRE 11-Jan-16 16:33:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigCreateIcon: false
TestRigRegister: false
TestRigLaunchDbUpgrade: false
TestRigWebSites: Forwarding
TestRigWebServer: localhost
TestRigWebDomain: {webRootDomain}
TestRigWebServerInstallPath: {webServerInstallPath}
";

			Process.Start("winrm", "quickconfig -quiet").WaitForExit();

			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			{
				try
				{
					SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");

					var logger = new TestLogger();
					CreateDeployer(logger).AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
					var log = logger.ToString();

					AssertSiteExists(log, true, webRootDomain, "Tracking", Db.ServerName, dbName);
				}
				finally
				{
					var logger = new TestLogger();
					CreateDeployer(logger).TeardownTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
					var log = logger.ToString();

					AssertSiteExists(log, false, webRootDomain, "Tracking", Db.ServerName, dbName);
				}
			}
		}

		[RequiresSoftware(RequiredSoftware.IsVM)]
		public void TestAutoDeployTestedShelfWithWebWithLongShelfName()
		{
			var shelfName = Guid.NewGuid().ToString().Replace("-", "");
			var dbName = "SH0" + shelfName;
			var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");
			var webRootDomain = Guid.NewGuid().ToString().Replace("-", "") + ".testrigtest.sand.wtg.zone";
			var taskComments = $@"BRE 11-Jan-16 16:33:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigCreateIcon: false
TestRigRegister: false
TestRigLaunchDbUpgrade: false
TestRigWebSites: Forwarding
TestRigWebServer: localhost
TestRigWebDomain: {webRootDomain}
TestRigWebServerInstallPath: {webServerInstallPath}
";

			Process.Start("winrm", "quickconfig -quiet").WaitForExit();

			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			{
				try
				{
					SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");

					var logger = new TestLogger();
					CreateDeployer(logger).AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
					var log = logger.ToString();

					AssertSiteExists(log, true, webRootDomain, "Tracking", Db.ServerName, dbName);
				}
				finally
				{
					var logger = new TestLogger();
					CreateDeployer(logger).TeardownTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
					var log = logger.ToString();

					AssertSiteExists(log, false, webRootDomain, "Tracking", Db.ServerName, dbName);
				}
			}
		}

		[RequiresSoftware(RequiredSoftware.IsVM)]
		public void TestAutoDeployTestedShelfWithWebHttpsBinding()
		{
			var shelfName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var dbName = "SH0" + shelfName;
			var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");
			var webRootDomain = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20) + ".testrigtest.sand.wtg.zone";
			var taskComments = $@"BRE 11-Jan-16 16:33:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigCreateIcon: false
TestRigRegister: false
TestRigLaunchDbUpgrade: false
TestRigWebSites: Forwarding
TestRigWebServer: localhost
TestRigWebDomain: {webRootDomain}
TestRigWebServerInstallPath: {webServerInstallPath}
";

			Process.Start("winrm", "quickconfig -quiet").WaitForExit();

			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			{
				try
				{
					SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");

					var logger = new TestLogger();
					CreateDeployer(logger).AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));

					using (var serverManager = new ServerManager())
					{
						var siteItem = serverManager.Sites.SingleOrDefault(s => s.Name == webRootDomain);
						AssertContainsExactElementsInAnyOrder(new[] { $"[http] *:80:{webRootDomain}", $"[https] *:443:{webRootDomain}" }, siteItem.Bindings.Select(b => b.ToString()));
					}
				}
				finally
				{
					var logger = new TestLogger();
					CreateDeployer(logger).TeardownTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
					var log = logger.ToString();

					AssertSiteExists(log, false, webRootDomain, "Tracking", Db.ServerName, dbName);
				}
			}
		}

		[RequiresSoftware(RequiredSoftware.IsVM)]
		public void TestAutoDeployTestedShelfWithWebError()
		{
			var shelfName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var dbName = "SH0" + shelfName;
			var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");
			var webRootDomain = Guid.NewGuid().ToString();
			var badServerInstallPath = Guid.NewGuid().ToString();
			var taskComments = $@"BRE 11-Jan-16 16:33:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigCreateIcon: false
TestRigRegister: false
TestRigLaunchDbUpgrade: false
TestRigWebSites: Forwarding
TestRigWebServer: {System.Environment.MachineName}
TestRigWebDomain: {webRootDomain}
TestRigWebServerInstallPath: {badServerInstallPath}
";

			Process.Start("winrm", "quickconfig -quiet").WaitForExit();

			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			{
				try
				{
					SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");

					var logger = new TestLogger();
					Exception exception = null;
					try
					{
						CreateDeployer(logger).AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
					}
					catch (Exception ex)
					{
						exception = ex;
					}
					AssertNotNull(exception);
					Assert(exception.ToString().Contains(badServerInstallPath));
				}
				finally
				{
					try
					{
						var logger = new TestLogger();
						CreateDeployer(logger).TeardownTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
					}
					catch
					{
					}
					AssertDatabaseExists(false, dbName);
				}
			}
		}

		[RequiresSoftware(RequiredSoftware.IsVM)]
		public void TestAutoDeployTestedShelfWithGlowWebAsPortals()
		{
			var shelfName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var dbName = "SH0" + shelfName;
			var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");
			var webRootDomain = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var taskComments = $@"BRE 11-Jan-16 16:33:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigCreateIcon: false
TestRigRegister: false
TestRigLaunchDbUpgrade: false
TestRigWebSites: GlowWebClient
TestRigWebServer: localhost
TestRigWebDomain: {webRootDomain}
TestRigWebServerInstallPath: {webServerInstallPath}
";

			Process.Start("winrm", "quickconfig -quiet").WaitForExit();

			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			{
				try
				{
					SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");

					var logger = new TestLogger();
					CreateDeployer(logger).AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
					var log = logger.ToString();

					AssertSiteExists(log, true, webRootDomain, "Portals", Db.ServerName, dbName);
				}
				finally
				{
					var logger = new TestLogger();
					CreateDeployer(logger).TeardownTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
					var log = logger.ToString();

					AssertSiteExists(log, false, webRootDomain, "Portals", Db.ServerName, dbName);
				}
			}
		}

		[RequiresSoftware(RequiredSoftware.IsVM)]
		public void TestAutoDeployTestedShelfWithWebTwiceToSameSite()
		{
			var shelfName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var dbName = "SH0" + shelfName;
			var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");
			var webRootDomain = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var taskComments = $@"BRE 11-Jan-16 16:33:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigCreateIcon: false
TestRigRegister: false
TestRigLaunchDbUpgrade: false
TestRigWebSites: Forwarding
TestRigWebServer: localhost
TestRigWebDomain: {webRootDomain}
TestRigWebServerInstallPath: {webServerInstallPath}
TestRigPermitDuplicateWebSites: false
";

			using (Process.Start("winrm", "quickconfig -quiet"))
			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			{
				try
				{
					SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");

					var logger = new TestLogger();
					CreateDeployer(logger).AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
					CreateDeployer(logger).AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
					var log = logger.ToString();

					AssertSiteExists(log, true, webRootDomain, "Tracking", Db.ServerName, dbName);
				}
				finally
				{
					var logger = new TestLogger();
					CreateDeployer(logger).TeardownTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
					var log = logger.ToString();

					AssertSiteExists(log, false, webRootDomain, "Tracking", Db.ServerName, dbName);
				}
			}
		}

		[RequiresSoftware(RequiredSoftware.IsVM)]
		public void TestAutoDeployTestedShelfWithWebTwiceToDifferentSite()
		{
			var shelfName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var dbName = "SH0" + shelfName;
			var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");
			var webRootDomain = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var otherWebRootDomain = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var taskComments = $@"CMA 08-Apr-19 11:48:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigCreateIcon: false
TestRigRegister: false
TestRigLaunchDbUpgrade: false
TestRigWebSites: Forwarding
TestRigWebServer: localhost
TestRigWebDomain: {webRootDomain}
TestRigWebServerInstallPath: {webServerInstallPath}
TestRigPermitDuplicateWebSites: false
";

			var otherSiteTaskComments = $@"CMA 08-Apr-19 11:48:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigCreateIcon: false
TestRigRegister: false
TestRigLaunchDbUpgrade: false
TestRigWebSites: Forwarding
TestRigWebServer: localhost
TestRigWebDomain: {otherWebRootDomain}
TestRigWebServerInstallPath: {webServerInstallPath}
TestRigPermitDuplicateWebSites: false
";

			using (Process.Start("winrm", "quickconfig -quiet"))
			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			{
				try
				{
					SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");

					var logger = new TestLogger();
					CreateDeployer(logger).AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
					var log = logger.ToString();

					AssertSiteExists(log, true, webRootDomain, "Tracking", Db.ServerName, dbName);

					logger = new TestLogger();
					AssertNoExceptionThrown(() => CreateDeployer(logger).AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", otherSiteTaskComments)));
					log = logger.ToString();
					AssertSiteExists(log, false, webRootDomain, "Tracking", Db.ServerName, dbName);
					AssertSiteExists(log, true, otherWebRootDomain, "Tracking", Db.ServerName, dbName);
				}
				finally
				{
					var logger = new TestLogger();
					CreateDeployer(logger).TeardownTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
					CreateDeployer(logger).TeardownTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", otherSiteTaskComments));
					var log = logger.ToString();

					AssertSiteExists(log, false, otherWebRootDomain, "Tracking", Db.ServerName, dbName);
				}
			}
		}

		[RequiresSoftware(RequiredSoftware.IsVM)]
		public void TestAutoDeployTestedShelfWithWebTwiceToDifferentSiteWithOverride()
		{
			var shelfName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var dbName = "SH0" + shelfName;
			var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");
			var webRootDomain = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var otherWebRootDomain = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var taskComments = $@"CMA 08-Apr-19 11:48:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigCreateIcon: false
TestRigRegister: false
TestRigLaunchDbUpgrade: false
TestRigWebSites: Forwarding
TestRigWebServer: localhost
TestRigWebDomain: {webRootDomain}
TestRigWebServerInstallPath: {webServerInstallPath}
TestRigPermitDuplicateWebSites: false
";

			var otherSiteTaskComments = $@"CMA 08-Apr-19 11:48:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigCreateIcon: false
TestRigRegister: false
TestRigLaunchDbUpgrade: false
TestRigWebSites: Forwarding
TestRigWebServer: localhost
TestRigWebDomain: {otherWebRootDomain}
TestRigWebServerInstallPath: {webServerInstallPath}
TestRigPermitDuplicateWebSites: true
";

			using (Process.Start("winrm", "quickconfig -quiet"))
			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			{
				try
				{
					SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69",
						new DateTime(2016, 4, 1, 11, 6, 14), "ALP");

					var logger = new TestLogger();
					CreateDeployer(logger).AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory,
						binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
					var log = logger.ToString();

					AssertSiteExists(log, true, webRootDomain, "Tracking", Db.ServerName, dbName);

					logger = new TestLogger();
					CreateDeployer(logger).AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory,
						binPathDirectory, new TaskInfo(shelfName, "whoever", otherSiteTaskComments));
					log = logger.ToString();

					AssertSiteExists(log, true, otherWebRootDomain, "Tracking", Db.ServerName, dbName);
				}
				finally
				{
					var logger = new TestLogger();
					CreateDeployer(logger).TeardownTestedShelf("DEBUG", string.Empty,
						sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
					var log = logger.ToString();

					AssertSiteExists(log, false, webRootDomain, "Tracking", Db.ServerName, dbName);

					logger = new TestLogger();
					CreateDeployer(logger).TeardownTestedShelf("DEBUG", string.Empty,
						sourcePathDirectory, binPathDirectory,
						new TaskInfo(shelfName, "whoever", otherSiteTaskComments));
					log = logger.ToString();

					AssertSiteExists(log, false, otherWebRootDomain, "Tracking", Db.ServerName, dbName);
				}
			}
		}

		[RequiresSoftware(RequiredSoftware.IsVM)]
		public void TestAutoDeployTwoTestedShelfConcurrently()
		{
			var shelfName1 = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var shelfName2 = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var dbName1 = "SH0" + shelfName1;
			var dbName2 = "SH0" + shelfName2;
			var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");
			var webRootDomain1 = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var webRootDomain2 = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var taskComments1 = $@"LCD 18-Mar-19 16:33:
TestRigCreateIcon: false
TestRigRegister: false
TestRigRestoreFromBackup: {backupPath}
TestRigWebSites: Forwarding,GlowWebClient
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigLaunchDbUpgrade: false
TestRigWebServer: localhost
TestRigWebDomain: {webRootDomain1}
TestRigWebServerInstallPath: {webServerInstallPath}
";
			var taskComments2 = taskComments1.Replace(webRootDomain1, webRootDomain2);

			Process.Start("winrm", "quickconfig -quiet").WaitForExit();
			using (var sourcePathDirectory1 = new TempDirectory())
			using (var sourcePathDirectory2 = new TempDirectory())
			using (var binPathDirectory1 = new TempDirectory())
			using (var binPathDirectory2 = new TempDirectory())
			{
				try
				{
					SetupTestFiles(sourcePathDirectory1.DirectoryName, binPathDirectory1.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");
					SetupTestFiles(sourcePathDirectory2.DirectoryName, binPathDirectory2.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");
					var logger1 = new TestLogger();
					var logger2 = new TestLogger();

					var restoreTask = Task.Run(() =>
					{
						CreateDeployer(logger1).AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory1, binPathDirectory1, new TaskInfo(shelfName1, "whoever", taskComments1));
					});

					var buildPackageTask = Task.Run(() =>
					{
						CreateDeployer(logger2).AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory2, binPathDirectory2, new TaskInfo(shelfName2, "whoever", taskComments2));
					});

					Task.WaitAll(restoreTask, buildPackageTask);

					var log1 = logger1.ToString();
					var log2 = logger2.ToString();

					Thread.Sleep(TimeSpan.FromSeconds(1));
					AssertSiteExists(log1, true, webRootDomain1, "Tracking", Db.ServerName, dbName1);
					AssertSiteExists(log2, true, webRootDomain2, "Tracking", Db.ServerName, dbName2);
				}
				finally
				{
					var logger1 = new TestLogger();
					var logger2 = new TestLogger();

					CreateDeployer(logger1).TeardownTestedShelf("DEBUG", string.Empty, sourcePathDirectory1, binPathDirectory1, new TaskInfo(shelfName1, "whoever", taskComments1));
					CreateDeployer(logger2).TeardownTestedShelf("DEBUG", string.Empty, sourcePathDirectory2, binPathDirectory2, new TaskInfo(shelfName2, "whoever", taskComments2));
					var log1 = logger1.ToString();
					var log2 = logger2.ToString();

					AssertSiteExists(log1, false, webRootDomain1, "Tracking", Db.ServerName, dbName1);
					AssertSiteExists(log2, false, webRootDomain2, "Tracking", Db.ServerName, dbName2);
				}
			}
		}

		public void TestAutoDeployTestedShelfIncludeSystemPackage()
		{
			using (var masterPackageArchivePath = new TempDirectory())
			using (var latestBuildArchivePath = new TempDirectory())
			{
				using (var sourcePathDirectory = new TempDirectory())
				using (var binPathDirectory = new TempDirectory())
				{
					SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.20", new DateTime(2016, 4, 1, 9, 5, 1), ReleaseInfo.Instance.ReleaseRing, buildMasterEdp: true);
					var deployer = new MockAutoDeployLatestBuildDeployer(new Mock<ITaskLogger>().Object, latestBuildArchivePath, masterPackageArchivePath);
					deployer.AutoDeployLatestBuild("RELEASE", ReleaseBuildContent.IBPMasterPackageDeploymentConfiguration, sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName);
				}

				Directory.CreateDirectory(Path.Combine(latestBuildArchivePath, ReleaseInfo.Instance.ReleaseRing + "_16.4.1.21_20160401100101"));

				var shelfName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
				var dbName = "SH0" + shelfName;
				var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");
				var taskComments = $@"BRE 11-Jan-16 16:33:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigCreateIcon: false
TestRigRegister: false
TestRigLaunchDbUpgrade: false
TestRigIncludeSystemPackage: true
TestRigSystemPackageDeploymentPath: {latestBuildArchivePath.DirectoryName}
";

				using (var sourcePathDirectory = new TempDirectory())
				using (var binPathDirectory = new TempDirectory())
				{
					try
					{
						SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), ReleaseInfo.Instance.ReleaseRing);

						var logger = new TestLogger();
						CreateDeployer(logger).AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
						var log = logger.ToString();
						AssertDeploymentIDLogged(log, dbName);

						AssertDatabaseExists(true, dbName);

						using (var connection = Db.NewAdminConnection(dbName))
						{
							var sqlConnection = ((IDbConnectionInternals)connection).ADOConnection;
							var upgradeManager = new SqlUpgradeManager(new UpgradeSqlContext(sqlConnection, sqlConnection.DataSource, sqlConnection.Database));

							AssertEquals(log, new Version(16, 4, 1, 20), upgradeManager.QueryCurrentVersion().Version);
							AssertEquals(log, new Version(16, 4, 1, 69), upgradeManager.QueryRunnablePackages().First().Version);
						}
					}
					finally
					{
						var logger = new TestLogger();
						CreateDeployer(logger).TeardownTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));

						AssertDatabaseExists(false, dbName);
					}
				}
			}
		}

		public void TestAutoDeployTestedShelfRegister()
		{
			var shelfName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var dbName = "SH0" + shelfName;
			var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");
			var taskComments = $@"BRE 11-Jan-16 16:33:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigCreateIcon: false
TestRigLaunchDbUpgrade: false
TestRigRegistrationSqlServer: DBSERVER\INSTANCE1
";

			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			{
				try
				{
					SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");

					mockProductKeyService.Setup(o => o.GetInternalTestKey("WUT", @"DBSERVER\INSTANCE1", dbName)).Returns("WUT123");
					mockProductRegistration.Setup(o => o.Register("WUT123", CancellationToken.None, BuildDeployer.DefaultRegistrationTimeoutMs)).Returns(ProductRegistrationRegisterResult.OK);

					var logger = new TestLogger();
					CreateDeployer(logger).AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
					var log = logger.ToString();
					AssertDeploymentIDLogged(log, dbName);

					AssertDatabaseExists(true, dbName);
					mockProductKeyService.VerifyAll();
					mockProductRegistration.VerifyAll();
				}
				finally
				{
					mockProductRegistration.Setup(o => o.Unregister(CancellationToken.None, BuildDeployer.DefaultRegistrationTimeoutMs)).Returns(ProductRegistrationUnregisterResult.OK);

					var logger = new TestLogger();
					CreateDeployer(logger).TeardownTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));

					AssertDatabaseExists(false, dbName);
					mockProductRegistration.VerifyAll();
				}
			}
		}

		public void TestRestoreLeavesDatabaseTrustworthy()
		{
			var shelfName = "Trustworthy";// Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var dbName = "SH0" + shelfName;
			var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");
			var taskComments = $@"BRE 11-Jan-16 16:33:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigCreateIcon: false
TestRigRegister: false
TestRigLaunchDbUpgrade: false";

			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			{
				var deployer = CreateDeployer(new TestLogger());
				var taskInfo = new TaskInfo(shelfName, "whoever", taskComments);
				try
				{
					SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");

					deployer.AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, taskInfo);

					using (var connection = Db.NewAdminConnection())
					{
						AssertEquals(true, connection.ExecuteScalar($"select Is_Trustworthy_on from sys.databases where name = '{dbName}'"));
					}
				}
				finally
				{
					deployer.TeardownTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, taskInfo);
					AssertDatabaseExists(false, dbName);
				}
			}
		}

		public void TestAutoDeployTestedShelfKillsDbConnectionsWhenRestoring()
		{
			var shelfName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var dbName = "SH0" + shelfName;
			var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");
			var taskComments = $@"BRE 11-Jan-16 16:33:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigCreateIcon: false
TestRigRegister: false
TestRigLaunchDbUpgrade: false";

			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			{
				try
				{
					SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");

					using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
					{
						using (var cmd = connection.Command($"CREATE DATABASE [{dbName}]"))
						{
							cmd.ExecuteNonQuery();
						}

						using (((ICurrentDbControl)connection).UseDatabase(dbName))
						using (var cmd = connection.Command(DataUtils.SQL_InitialTablesForEmptyDatabase()))
						{
							cmd.ExecuteNonQuery();
						}
					}

					using (var existingConnection = Db.NewAdminConnection(dbName))
					{
						existingConnection.EnsureIsOpen();

						var logger = new TestLogger();
						CreateDeployer(logger).AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
						var log = logger.ToString();
						AssertDeploymentIDLogged(log, dbName);

						using (var connection = Db.NewAdminConnection(dbName))
						{
							var sqlConnection = ((IDbConnectionInternals)connection).ADOConnection;
							var upgradeManager = new SqlUpgradeManager(new UpgradeSqlContext(sqlConnection, sqlConnection.DataSource, sqlConnection.Database));

							AssertEquals(log, new Version(16, 4, 1, 69), upgradeManager.QueryCurrentVersion().Version);
						}
					}
				}
				finally
				{
					var logger = new TestLogger();
					CreateDeployer(logger).TeardownTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));

					AssertDatabaseExists(false, dbName);
				}
			}
		}

		public void TestAutoDeployTestedShelfTearDownDropsRelatedDatabases()
		{
			// Arrange
			var shelfName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var dbName = "SH0" + shelfName;
			var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");
			const string expectedTearDownSingleRefDb = "CW-RefDb-TearDown";
			const string expectedUniversalRefDb = "CW-RefDatabase";
			var taskComments = $@"BRE 11-Jan-16 16:33:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigCreateIcon: false
TestRigRegister: false
TestRigLaunchDbUpgrade: false
TestRigSingleRefDatabaseName:{expectedTearDownSingleRefDb}
";

			var expectedRelatedDatabases = new[]
			{
				$"{dbName}_SD001",
				$"{dbName}_SD002",
				$"{dbName}_Audit",
				$"{dbName}_EDW",
				$"{dbName}_UserRepository",
				$"DBUPG_NewTemplateDB_{dbName}",
				$"DBUPG_NewTemplateDB_{dbName}_Whatever",
				$"DBUPG_PreSchemaUpgradeDB_{dbName}",
				$"DBUPG_PreSchemaUpgradeDB_{dbName}_Whatever",
				$"DBUPG_DataCopyDb_{dbName}",
				$"DBUPG_DataCopyDb_{dbName}_Whatever",
				expectedTearDownSingleRefDb,
			};

			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
			{
				var logger = new TestLogger();
				var buildDeployer = CreateDeployer(logger);
				var existUniversalRefDb = connection.DatabaseExists(expectedUniversalRefDb);
				try
				{
					SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");
					buildDeployer.AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
					AssertDeploymentIDLogged(logger.ToString(), dbName);
					AssertDatabaseExists(true, dbName);

					foreach (var db in expectedRelatedDatabases)
					{
						connection.ExecuteNonQuery($"create database [{db}]");
					}
					if (!existUniversalRefDb)
					{
						connection.ExecuteNonQuery($"create database [{expectedUniversalRefDb}]");
					}

					// Act
					buildDeployer.TeardownTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));

					// Assert
					CombineAssertions(() =>
					{
						AssertDatabaseExists(false, dbName);
						foreach (var db in expectedRelatedDatabases)
						{
							AssertDatabaseExists(false, db);
						}
						AssertDatabaseExists(true, expectedUniversalRefDb);
					});
				}
				finally
				{
					connection.ExecuteNonQuery($"DROP DATABASE if EXISTS [{dbName}]");
					foreach (var db in expectedRelatedDatabases)
					{
						connection.ExecuteNonQuery($"DROP DATABASE if EXISTS [{db}]");
					}
					if (!existUniversalRefDb)
					{
						connection.ExecuteNonQuery($"DROP DATABASE if EXISTS [{expectedUniversalRefDb}]");
					}
				}
			}
		}

		public void TestTeardownWhenUnregisterFails()
		{
			var shelfName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var dbName = "SH0" + shelfName;
			var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");
			var taskComments = $@"BRE 11-Jan-16 16:33:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigCreateIcon: false
TestRigLaunchDbUpgrade: false
TestRigRegistrationSqlServer: DBSERVER\INSTANCE1
";

			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			{
				try
				{
					SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");

					mockProductKeyService.Setup(o => o.GetInternalTestKey("WUT", @"DBSERVER\INSTANCE1", dbName)).Returns("WUT123");
					mockProductRegistration.Setup(o => o.Register("WUT123", CancellationToken.None, BuildDeployer.DefaultRegistrationTimeoutMs)).Returns(ProductRegistrationRegisterResult.OK);

					var logger = new TestLogger();
					CreateDeployer(logger).AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
					var log = logger.ToString();
					AssertDeploymentIDLogged(log, dbName);

					AssertDatabaseExists(true, dbName);
					mockProductKeyService.VerifyAll();
					mockProductRegistration.VerifyAll();
				}
				finally
				{
					mockProductRegistration.Setup(o => o.Unregister(CancellationToken.None, BuildDeployer.DefaultRegistrationTimeoutMs)).Throws(new DatabaseUpgradeInProgressException());

					var logger = new TestLogger();
					CreateDeployer(logger).TeardownTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));

					AssertDatabaseExists(false, dbName);
					mockProductRegistration.VerifyAll();
				}
			}
		}

		public void TestNoUnderscoreInDatabaseName()
		{
			var shelfName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var dbName = "SH0_" + shelfName;
			var taskComments = $@"
TestRigSqlServer: {Db.ServerName}
TestRigDatabaseName: {dbName}
TestRigCreateIcon: false
TestRigRegister: false
TestRigLaunchDbUpgrade: false";

			var logger = new TestLogger();
			AssertExceptionThrown<InvalidOperationException>("No underscore in Database Name allowed", () => CreateDeployer(logger).AutoDeployTestedShelf("DEBUG", string.Empty, string.Empty, string.Empty, new TaskInfo(shelfName, "whoever", taskComments)));
		}

		public void TestTearDownWithOpenConnection()
		{
			var shelfName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var dbName = "SH0" + shelfName;
			var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");
			var taskComments = $@"BRE 11-Jan-16 16:33:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigCreateIcon: false
TestRigRegister: false
TestRigLaunchDbUpgrade: false";

			AdminConnection connection = null;
			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			{
				try
				{
					SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");

					var logger = new TestLogger();
					CreateDeployer(logger).AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
					var log = logger.ToString();
					AssertDeploymentIDLogged(log, dbName);

					AssertDatabaseExists(true, dbName);

					connection = Db.NewAdminConnection(dbName);
					var sqlConnection = ((IDbConnectionInternals)connection).ADOConnection;
					var upgradeManager = new SqlUpgradeManager(new UpgradeSqlContext(sqlConnection, sqlConnection.DataSource, sqlConnection.Database));

					AssertEquals(log, new Version(16, 4, 1, 69), upgradeManager.QueryCurrentVersion().Version);
				}
				finally
				{
					var logger = new TestLogger();
					CreateDeployer(logger).TeardownTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
					AssertDatabaseExists(false, dbName);
					connection?.Dispose();
				}
			}
		}

		public void TestTeardownTestedShelfCanDropOrphanedUsersSpecificToCurrentDeployment()
		{
			// Arrange
			var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");

			const string shelfName = "TestTeardownDropsOrphanedUser";
			var dbName = "SH0" + shelfName;
			AssertEquals(
				"Ensure we follow rule of `TestedShelfDeploymentOptions.SafeDatabaseName`",
				new TestedShelfDeploymentOptions(GetTaskInfo()).DatabaseName,
				dbName);

			const string preExistingSharedDbName1 = "CW-RefDb-A";
			const string preExistingSharedDbName2 = "CW-RefDb-B";

			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
			using (AdoTestUtils.DropDbIfExistsDisposable(connection, dbName, Db.DatabaseName))
			using (AdoTestUtils.CreateDbDropExistingDisposable(connection, preExistingSharedDbName1, Db.DatabaseName))
			using (AdoTestUtils.CreateDbDropExistingDisposable(connection, preExistingSharedDbName2, Db.DatabaseName))
			{
				Deploy();

				var enterpriseDbUserLoginName = $"EnterpriseDbUser_{dbName}_User_A.N";

				var allLogins = CreateLoginsAndRights();
				var restrictedWriterLoginName = allLogins[3];

				CreateUsersOnSharedDbs();

				AssertAllLoginsExist(true);
				AssertUsersExistOnSharedDbs(true);

				// Act
				CreateDeployer(new TestLogger(), CreateCargoWiseOneInstanceClass()).TeardownTestedShelf(
					"DEBUG",
					string.Empty,
					sourcePathDirectory,
					binPathDirectory,
					GetTaskInfo());

				// Assert
				CombineAssertions(() =>
				{
					AssertAllLoginsExist(false);
					AssertUsersExistOnSharedDbs(false);
				});

				void Deploy()
				{
					SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");

					CreateDeployer(new TestLogger(), CreateCargoWiseOneInstanceClass()).AutoDeployTestedShelf(
						"DEBUG",
						string.Empty,
						sourcePathDirectory,
						binPathDirectory,
						GetTaskInfo());
				}

				IReadOnlyList<string> CreateLoginsAndRights()
				{
					var loginNames = new List<string>();

					// to create logins on db [{dbName}], we need connection's InitialDatabase not to be initial database (i.e. OdysseyDat)
					using (var connectionToCreateLoginsToOurDeploymentDb = Db.NewAdminConnection(Db.ServerName, dbName))
					{
						IDbLoginRepair repairer = connectionToCreateLoginsToOurDeploymentDb;
						repairer.EnableApplicationDbLogins();
						repairer.EnsureDbLoginsHaveRightsToCurrentDatabase();

						loginNames.AddRange(connectionToCreateLoginsToOurDeploymentDb.Logins.Select(x => x.LoginName));
					}

					CreateLogin(connection, enterpriseDbUserLoginName);
					loginNames.Add(enterpriseDbUserLoginName);

					return loginNames;
				}

				void AssertAllLoginsExist(bool doesExist)
				{
					foreach (var login in allLogins)
					{
						AssertDatabaseLoginExists(connection, login, doesExist);
					}
				}

				void CreateUsersOnSharedDbs()
				{
					CreateDbUserForLogin(connection, preExistingSharedDbName1, enterpriseDbUserLoginName);
					CreateDbUserForLogin(connection, preExistingSharedDbName2, restrictedWriterLoginName);
				}

				void AssertUsersExistOnSharedDbs(bool doesExist)
				{
					AssertDatabaseUserExists(connection, preExistingSharedDbName1, enterpriseDbUserLoginName, doesExist);
					AssertDatabaseUserExists(connection, preExistingSharedDbName2, restrictedWriterLoginName, doesExist);
				}

				ICargoWiseOneInstanceClass CreateCargoWiseOneInstanceClass()
				{
					var mockCw1InstanceClass = new Mock<ICargoWiseOneInstanceClass>();
					mockCw1InstanceClass.Setup(c => c.AddNewInstance(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<System.DirectoryServices.DirectoryEntry>()))
						.Returns(Mock.Of<ICargoWiseOneInstanceEntry>());

					var mockCw1SearchResult = new Mock<ICargoWiseOneInstanceSearchResult>();
					mockCw1SearchResult.Setup(r => r.GetDirectoryEntry()).Returns(Mock.Of<ICargoWiseOneInstanceEntry>());

					mockCw1InstanceClass.Setup(c => c.FindInstance(shelfName, It.Is<DirectorySearchOptions>(o => o.DomainName == "sand.wtg.zone")))
						.Returns(mockCw1SearchResult.Object);

					return mockCw1InstanceClass.Object;
				}
			}

			TaskInfo GetTaskInfo()
			{
				var taskComments = $@"BRE 11-Jan-16 16:33:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigRegister: false
TestRigLaunchDbUpgrade: false";
				return new TaskInfo(shelfName, "whoever", taskComments);
			}

			void CreateLogin(AdminConnection connection, string loginName)
			{
				AdoTestUtils.DropDbLoginIfExists(connection, loginName);
				connection.ExecuteNonQuery($"CREATE LOGIN [{loginName}] WITH PASSWORD = '', CHECK_POLICY = OFF, DEFAULT_LANGUAGE = us_english");
			}

			void CreateDbUserForLogin(AdminConnection connection, string db, string loginName)
			{
				using (((ICurrentDbControl)connection).UseDatabase(db))
				{
					connection.ExecuteNonQuery($"CREATE USER [{loginName}] FOR LOGIN [{loginName}];");
				}
			}
		}

		public void TestTeardownTestedShelfCanDropOrphanedUsersOnSharedDbsButSpecificToOtherNonExistentDbOnly()
		{
			// Arrange
			var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");

			const string shelfName = "TestTeardownDropsOrphanedUsers";
			var dbName = "SH0" + shelfName;
			AssertEquals(
				"Ensure we follow rule of `TestedShelfDeploymentOptions.SafeDatabaseName`",
				new TestedShelfDeploymentOptions(GetTaskInfo()).DatabaseName,
				dbName);

			const string sharedDb = "CW-RefDb-C";
			const string nonExistentMainDbName = "NonExistent";
			const string preExistentMainDbName = "PreExistent";

			var loginFormats = new[]
			{
				"{0}_CargoWiseWriterLogin",
				"{0}_CargoWiseReaderLogin",
				"{0}_RestrictedWriterLogin",
				"{0}_RestrictedReaderLogin",
				"{0}_UnrestrictedWriterLogin",
				"EnterpriseDbUser_{0}_UserA",
				"EnterpriseDbUser_{0}_User_B",
				"EnterpriseDbUser_{0}_User.C",
				"EnterpriseDbUser_{0}_User/D",
			};

			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
			using (AdoTestUtils.DropDbIfExistsDisposable(connection, dbName, Db.DatabaseName))
			using (AdoTestUtils.CreateDbDropExistingDisposable(connection, sharedDb, Db.DatabaseName))
			using (AdoTestUtils.CreateDbDropExistingDisposable(connection, preExistentMainDbName, Db.DatabaseName))
			{
				Deploy();

				var allLogins = CreateOrphanedUsers();

				// Act
				CreateDeployer(new TestLogger(), CreateCargoWiseOneInstanceClass()).TeardownTestedShelf(
					"DEBUG",
					string.Empty,
					sourcePathDirectory,
					binPathDirectory,
					GetTaskInfo());

				// Assert
				CombineAssertions(() =>
				{
					foreach (var pair in allLogins)
					{
						AssertDatabaseUserExists(connection, sharedDb, pair.LoginName, pair.DbName != nonExistentMainDbName);
					}
				});

				void Deploy()
				{
					SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");

					CreateDeployer(new TestLogger(), CreateCargoWiseOneInstanceClass()).AutoDeployTestedShelf(
						"DEBUG",
						string.Empty,
						sourcePathDirectory,
						binPathDirectory,
						GetTaskInfo());
				}

				ICargoWiseOneInstanceClass CreateCargoWiseOneInstanceClass()
				{
					var mockCw1InstanceClass = new Mock<ICargoWiseOneInstanceClass>();
					mockCw1InstanceClass.Setup(c => c.AddNewInstance(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<System.DirectoryServices.DirectoryEntry>()))
						.Returns(Mock.Of<ICargoWiseOneInstanceEntry>());

					var mockCw1SearchResult = new Mock<ICargoWiseOneInstanceSearchResult>();
					mockCw1SearchResult.Setup(r => r.GetDirectoryEntry()).Returns(Mock.Of<ICargoWiseOneInstanceEntry>());

					mockCw1InstanceClass.Setup(c => c.FindInstance(shelfName, It.Is<DirectorySearchOptions>(o => o.DomainName == "sand.wtg.zone")))
						.Returns(mockCw1SearchResult.Object);

					return mockCw1InstanceClass.Object;
				}

				(string DbName, string LoginName)[] CreateOrphanedUsers()
				{
					return CreateOrphanedUsersOnSharedDb(nonExistentMainDbName)
						.Concat(CreateOrphanedUsersOnSharedDb(preExistentMainDbName))
						.ToArray();

					IReadOnlyList<(string DbName, string LoginName)> CreateOrphanedUsersOnSharedDb(string mainDb)
					{
						var pairs = new List<(string DbName, string LoginName)>();
						foreach (var format in loginFormats)
						{
							var login = string.Format(format, mainDb);
							CreateLogin(connection, login);
							CreateDbUserForLogin(connection, sharedDb, login);
							DropLogin(connection, login);
							AssertDatabaseUserExists(connection, sharedDb, login, true);
							pairs.Add((mainDb, login));
						}
						return pairs;
					}
				}
			}

			TaskInfo GetTaskInfo()
			{
				var taskComments = $@"BRE 11-Jan-16 16:33:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigRegister: false
TestRigLaunchDbUpgrade: false";
				return new TaskInfo(shelfName, "whoever", taskComments);
			}

			void CreateLogin(AdminConnection connection, string loginName)
			{
				AdoTestUtils.DropDbLoginIfExists(connection, loginName);
				connection.ExecuteNonQuery($"CREATE LOGIN [{loginName}] WITH PASSWORD = '', CHECK_POLICY = OFF, DEFAULT_LANGUAGE = us_english");
			}

			void DropLogin(AdminConnection connection, string loginName)
			{
				connection.ExecuteNonQuery($"DROP LOGIN [{loginName}]");
			}

			void CreateDbUserForLogin(AdminConnection connection, string db, string loginName)
			{
				using (((ICurrentDbControl)connection).UseDatabase(db))
				{
					connection.ExecuteNonQuery($"CREATE USER [{loginName}] FOR LOGIN [{loginName}];");
				}
			}
		}

		public void TestTeardownTestedShelfCanDropOrphanedUsersOnSharedDbButThatHasAppLockOn()
		{
			// Arrange
			var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");

			const string shelfName = "TestTeardownDropsOrphanedUsers";
			var dbName = "SH0" + shelfName;
			AssertEquals(
				"Ensure we follow rule of `TestedShelfDeploymentOptions.SafeDatabaseName`",
				new TestedShelfDeploymentOptions(GetTaskInfo()).DatabaseName,
				dbName);

			const string sharedDbHasAppLockOn = "CW-RefDb-D";
			const string sharedDb2 = "CW-RefDb-E";

			var logins = new[]
			{
				$"{dbName}_CargoWiseWriterLogin",
				$"{dbName}_CargoWiseReaderLogin",
				$"{dbName}_RestrictedWriterLogin",
				$"{dbName}_RestrictedReaderLogin",
				$"{dbName}_UnrestrictedWriterLogin",
				$"EnterpriseDbUser_{dbName}_UserA",
				$"EnterpriseDbUser_{dbName}_User_B",
				$"EnterpriseDbUser_{dbName}_User.C",
				$"EnterpriseDbUser_{dbName}_User/D",
			};

			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
			using (AdoTestUtils.CreateDbDropExistingDisposable(connection, dbName))
			using (AdoTestUtils.CreateDbDropExistingDisposable(connection, sharedDbHasAppLockOn, dbName))
			using (AdoTestUtils.CreateDbDropExistingDisposable(connection, sharedDb2, dbName))
			{
				AdoTestUtils.DropDbIfExists(connection, dbName);
				Deploy();

				CreateOrphanedUsers();

				connection.RunLocked(
					"CleanUpOrphanedUsers",
					_ =>
					{
						// Act
						CreateDeployer(new TestLogger(), CreateCargoWiseOneInstanceClass()).TeardownTestedShelf(
							"DEBUG",
							string.Empty,
							sourcePathDirectory,
							binPathDirectory,
							GetTaskInfo());
					},
					max_tries: 1,
					dbName: sharedDbHasAppLockOn);

				// Assert
				CombineAssertions(() =>
				{
					foreach (var login in logins)
					{
						AssertDatabaseUserExists(connection, sharedDbHasAppLockOn, login, true);
						AssertDatabaseUserExists(connection, sharedDb2, login, false);
					}
				});

				void Deploy()
				{
					SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");

					CreateDeployer(new TestLogger(), CreateCargoWiseOneInstanceClass()).AutoDeployTestedShelf(
						"DEBUG",
						string.Empty,
						sourcePathDirectory,
						binPathDirectory,
						GetTaskInfo());
				}

				ICargoWiseOneInstanceClass CreateCargoWiseOneInstanceClass()
				{
					var mockCw1InstanceClass = new Mock<ICargoWiseOneInstanceClass>();
					mockCw1InstanceClass.Setup(c => c.AddNewInstance(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<System.DirectoryServices.DirectoryEntry>()))
						.Returns(Mock.Of<ICargoWiseOneInstanceEntry>());

					var mockCw1SearchResult = new Mock<ICargoWiseOneInstanceSearchResult>();
					mockCw1SearchResult.Setup(r => r.GetDirectoryEntry()).Returns(Mock.Of<ICargoWiseOneInstanceEntry>());

					mockCw1InstanceClass.Setup(c => c.FindInstance(shelfName, It.Is<DirectorySearchOptions>(o => o.DomainName == "sand.wtg.zone")))
						.Returns(mockCw1SearchResult.Object);

					return mockCw1InstanceClass.Object;
				}

				void CreateOrphanedUsers()
				{
					CreateOrphanedUsersOnSharedDb(sharedDbHasAppLockOn);
					CreateOrphanedUsersOnSharedDb(sharedDb2);

					void CreateOrphanedUsersOnSharedDb(string sharedDb)
					{
						foreach (var login in logins)
						{
							CreateLogin(connection, login);
							CreateDbUserForLogin(connection, sharedDb, login);
							DropLogin(connection, login);
							AssertDatabaseUserExists(connection, sharedDb, login, true);
						}
					}
				}
			}

			TaskInfo GetTaskInfo()
			{
				var taskComments = $@"BRE 11-Jan-16 16:33:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigRegister: false
TestRigLaunchDbUpgrade: false";
				return new TaskInfo(shelfName, "whoever", taskComments);
			}

			void CreateLogin(AdminConnection connection, string loginName)
			{
				AdoTestUtils.DropDbLoginIfExists(connection, loginName);
				connection.ExecuteNonQuery($"CREATE LOGIN [{loginName}] WITH PASSWORD = '', CHECK_POLICY = OFF, DEFAULT_LANGUAGE = us_english");
			}

			void DropLogin(AdminConnection connection, string loginName)
			{
				connection.ExecuteNonQuery($"DROP LOGIN [{loginName}]");
			}

			void CreateDbUserForLogin(AdminConnection connection, string db, string loginName)
			{
				using (((ICurrentDbControl)connection).UseDatabase(db))
				{
					connection.ExecuteNonQuery($"CREATE USER [{loginName}] FOR LOGIN [{loginName}];");
				}
			}
		}

		public void TestRestoreFromBackupAndNotScheduleUPGWillDisableUPG()
		{
			// Arrange
			var shelfName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var dbName = "SH0" + shelfName;
			var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");
			var taskComments = $@"BRE 11-Jan-16 16:33:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigCreateIcon: false
TestRigRegister: false
TestRigLaunchDbUpgrade: false
TestRigScheduleUPG: false";

			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			{
				try
				{
					SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");
					var taskLogger = new TestLogger(
						line =>
						{
							if (line.Equals("start Disabling UPG service task", StringComparison.OrdinalIgnoreCase))
							{
								var nextRunTime = "'2020-01-01 23:59:59'";
								using (var connection = Db.NewAdminConnection(dbName))
								using (var command = connection.Command(Invariant($@"IF OBJECT_ID('[dbo].[StmScheduleTask]') IS NULL
BEGIN
	CREATE TABLE [dbo].[StmScheduleTask]
	(
		[S5_PK] UNIQUEIDENTIFIER NOT NULL,
		[S5_ScheduleDescription] VARCHAR(80) NOT NULL DEFAULT '',
		[S5_TaskPeriod] VARCHAR(3) NOT NULL DEFAULT '',
		[S5_WeekDaysOnly] BIT NOT NULL DEFAULT 1,
		[S5_TaskPeriodCount] INT NOT NULL DEFAULT 0,
		[S5_DayNumber] TINYINT NOT NULL DEFAULT 0,
		[S5_DayList] VARCHAR(7) NOT NULL DEFAULT 'NNNNNNN',
		[S5_MonthNumber] TINYINT NOT NULL DEFAULT 0,
		[S5_WeekDayOccurrenceNumber] TINYINT NOT NULL DEFAULT 0,
		[S5_EndAfterCount] INT NOT NULL DEFAULT 0,
		[S5_ScheduleActualRunCount] INT NOT NULL DEFAULT 0,
		[S5_AccountingPeriodScheduleFrstRun] INT NOT NULL DEFAULT 0,
		[S5_ScheduleType] VARCHAR(3) NOT NULL DEFAULT '',
		[S5_TypeOfDocument] CHAR(3) NOT NULL DEFAULT '',
		[S5_GS_NKPrintUser] VARCHAR(3) NOT NULL DEFAULT '',
		[S5_NextScheduledPrintRunTimeUtc] DATETIME NULL,
		[S5_IsActive] BIT NOT NULL DEFAULT 1,
		[S5_IsPrivate] BIT NOT NULL DEFAULT 0,
		[S5_OverdueDurationInSeconds] INT NOT NULL DEFAULT 0,
		[S5_ParentTableCode] VARCHAR(3) NOT NULL DEFAULT '',
		[S5_RunTimeInMinutes] INT NOT NULL DEFAULT 0,
		[S5_SystemCreateTimeUtc] SMALLDATETIME NULL,
		[S5_SystemCreateUser] VARCHAR(3) NOT NULL DEFAULT '',
		[S5_SystemLastEditTimeUtc] SMALLDATETIME NULL,
		[S5_SystemLastEditUser] VARCHAR(3) NOT NULL DEFAULT '',
	) ON [PRIMARY];
END
INSERT INTO [dbo].[StmScheduleTask] ([S5_PK], [S5_ScheduleType], [S5_IsActive], [S5_ScheduleDescription], [S5_ParentTableCode], [S5_NextScheduledPrintRunTimeUtc], [S5_SystemCreateTimeUtc], [S5_SystemCreateUser], [S5_SystemLastEditTimeUtc], [S5_SystemLastEditUser])
VALUES (NEWID(), 'UPG', 1, 'System Upgrade Service', 'SH', {nextRunTime}, GETUTCDATE(), 'TU1', GETUTCDATE(), 'TU1');

IF OBJECT_ID('[dbo].[StmServiceTask]') IS NULL
BEGIN
	CREATE TABLE [StmServiceTask]
	(
		[SST_PK] UNIQUEIDENTIFIER NOT NULL,
		[SST_ServiceTaskCode] VARCHAR(3) NOT NULL DEFAULT '',
		[SST_Active] BIT NOT NULL DEFAULT 0,
		[SST_Configuration] XML NOT NULL,
		[SST_NextRunTime] DateTimeOffset(0) NOT NULL DEFAULT '',
		[SST_SystemCreateTimeUtc] SMALLDATETIME NOT NULL,
		[SST_SystemCreateUser] VARCHAR(3) NOT NULL DEFAULT '',
		[SST_SystemLastEditTimeUtc] SMALLDATETIME NOT NULL,
		[SST_SystemLastEditUser] VARCHAR(3) NOT NULL DEFAULT '',
	);
END
INSERT INTO [dbo].[StmServiceTask] ([SST_PK], [SST_ServiceTaskCode], [SST_Active], [SST_Configuration], [SST_NextRunTime], [SST_SystemCreateTimeUtc], [SST_SystemCreateUser], [SST_SystemLastEditTimeUtc], [SST_SystemLastEditUser])
VALUES (NEWID(), 'UPG', 1, '', {nextRunTime}, GETUTCDATE(), 'TU1', GETUTCDATE(), 'TU1');")))
								{
									command.ExecuteNonQuery();
								}
							}
						});
					var buildDeployer = CreateDeployer(taskLogger);

					// Act
					buildDeployer.AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));

					// Assert
					AssertDatabaseExists(true, dbName);
					var recordsStmScheduleTask = GetRecords(() =>
						Db.NewAdminConnection(dbName), "[dbo].[StmScheduleTask]")
						.ToArray();
					var recordsStmServiceTask = GetRecords(() =>
						Db.NewAdminConnection(dbName), "[dbo].[StmServiceTask]")
						.ToArray();

					AssertEquals("StmScheduleTask should have one record", 1, recordsStmScheduleTask.Length);
					AssertEquals("StmServiceTask should have one record", 1, recordsStmServiceTask.Length);

					var upgStmScheduleTask = recordsStmScheduleTask[0];
					AssertEquals("S5_ScheduleType", "UPG", upgStmScheduleTask["S5_ScheduleType"]);
					AssertEquals("S5_IsActive", false, upgStmScheduleTask["S5_IsActive"]);
					AssertEquals("next run time should be unchanged", new DateTime(2020, 1, 1, 23, 59, 59, 0), upgStmScheduleTask["S5_NextScheduledPrintRunTimeUtc"]);

					var upgStmServiceTask = recordsStmServiceTask[0];
					AssertEquals("SST_ServiceTaskCode", "UPG", upgStmServiceTask["SST_ServiceTaskCode"]);
					AssertEquals("SST_Active", false, upgStmServiceTask["SST_Active"]);
					AssertEquals("next run time should be unchanged", new DateTimeOffset(2020, 1, 1, 23, 59, 59, TimeSpan.Zero), upgStmServiceTask["SST_NextRunTime"]);
				}
				finally
				{
					var logger = new TestLogger();
					CreateDeployer(logger).TeardownTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
					AssertDatabaseExists(false, dbName);
				}
			}
		}

		public void TestScheduleUPGShouldEnableUPG()
		{
			// Arrange
			var shelfName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var dbName = "SH0" + shelfName;
			var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");
			var taskComments = $@"BRE 11-Jan-16 16:33:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigRegister: false
TestRigLaunchDbUpgrade: false
TestRigServiceTasks: true
TestRigScheduleUPG: true";

			var mockCw1InstanceClass = new Mock<ICargoWiseOneInstanceClass>();
			mockCw1InstanceClass.Setup(c => c.AddNewInstance(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<System.DirectoryServices.DirectoryEntry>())).Returns(new Mock<ICargoWiseOneInstanceEntry>().Object);

			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			{
				try
				{
					SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");
					var taskLogger = new TestLogger(
						line =>
						{
							if (line.Equals("start Scheduling DbUpgrade", StringComparison.OrdinalIgnoreCase))
							{
								var nextRunTime = "'2020-01-01 23:59:59'";
								using (var connection = Db.NewAdminConnection(dbName))
								using (var command = connection.Command(Invariant($@"IF OBJECT_ID('[dbo].[StmScheduleTask]') IS NULL
BEGIN
	CREATE TABLE [dbo].[StmScheduleTask]
	(
		[S5_PK] UNIQUEIDENTIFIER NOT NULL,
		[S5_ScheduleDescription] VARCHAR(80) NOT NULL DEFAULT '',
		[S5_TaskPeriod] VARCHAR(3) NOT NULL DEFAULT '',
		[S5_WeekDaysOnly] BIT NOT NULL DEFAULT 1,
		[S5_TaskPeriodCount] INT NOT NULL DEFAULT 0,
		[S5_DayNumber] TINYINT NOT NULL DEFAULT 0,
		[S5_DayList] VARCHAR(7) NOT NULL DEFAULT 'NNNNNNN',
		[S5_MonthNumber] TINYINT NOT NULL DEFAULT 0,
		[S5_WeekDayOccurrenceNumber] TINYINT NOT NULL DEFAULT 0,
		[S5_EndAfterCount] INT NOT NULL DEFAULT 0,
		[S5_ScheduleActualRunCount] INT NOT NULL DEFAULT 0,
		[S5_AccountingPeriodScheduleFrstRun] INT NOT NULL DEFAULT 0,
		[S5_ScheduleType] VARCHAR(3) NOT NULL DEFAULT '',
		[S5_TypeOfDocument] CHAR(3) NOT NULL DEFAULT '',
		[S5_GS_NKPrintUser] VARCHAR(3) NOT NULL DEFAULT '',
		[S5_NextScheduledPrintRunTimeUtc] DATETIME NULL,
		[S5_IsActive] BIT NOT NULL DEFAULT 1,
		[S5_IsPrivate] BIT NOT NULL DEFAULT 0,
		[S5_OverdueDurationInSeconds] INT NOT NULL DEFAULT 0,
		[S5_ParentTableCode] VARCHAR(3) NOT NULL DEFAULT '',
		[S5_RunTimeInMinutes] INT NOT NULL DEFAULT 0,
		[S5_SystemCreateTimeUtc] SMALLDATETIME NULL,
		[S5_SystemCreateUser] VARCHAR(3) NOT NULL DEFAULT '',
		[S5_SystemLastEditTimeUtc] SMALLDATETIME NULL,
		[S5_SystemLastEditUser] VARCHAR(3) NOT NULL DEFAULT '',
	)
	ON [PRIMARY];
END
INSERT INTO [dbo].[StmScheduleTask] ([S5_PK], [S5_ScheduleType], [S5_IsActive], [S5_ScheduleDescription], [S5_ParentTableCode], [S5_NextScheduledPrintRunTimeUtc], [S5_SystemCreateTimeUtc], [S5_SystemCreateUser], [S5_SystemLastEditTimeUtc], [S5_SystemLastEditUser])
VALUES (NEWID(), 'UPG', 0, 'System Upgrade Service', 'SH', {nextRunTime}, GETUTCDATE(), 'TU1', GETUTCDATE(), 'TU1');

IF OBJECT_ID('[dbo].[StmServiceTask]') IS NULL
BEGIN
	CREATE TABLE [StmServiceTask]
	(
		[SST_PK] UNIQUEIDENTIFIER NOT NULL,
		[SST_ServiceTaskCode] VARCHAR(3) NOT NULL DEFAULT '',
		[SST_Active] BIT NOT NULL DEFAULT 0,
		[SST_Configuration] XML NOT NULL,
		[SST_NextRunTime] DateTimeOffset(0) NOT NULL DEFAULT '',
		[SST_SystemCreateTimeUtc] SMALLDATETIME NOT NULL,
		[SST_SystemCreateUser] VARCHAR(3) NOT NULL DEFAULT '',
		[SST_SystemLastEditTimeUtc] SMALLDATETIME NOT NULL,
		[SST_SystemLastEditUser] VARCHAR(3) NOT NULL DEFAULT '',
	);
END
INSERT INTO [dbo].[StmServiceTask] ([SST_PK], [SST_ServiceTaskCode], [SST_Active], [SST_Configuration], [SST_NextRunTime], [SST_SystemCreateTimeUtc], [SST_SystemCreateUser], [SST_SystemLastEditTimeUtc], [SST_SystemLastEditUser])
VALUES (NEWID(), 'UPG', 0, '', {nextRunTime}, GETUTCDATE(), 'TU1', GETUTCDATE(), 'TU1');
")))
								{
									command.ExecuteNonQuery();
								}
							}
						});
					var buildDeployer = CreateDeployer(taskLogger, mockCw1InstanceClass.Object);

					// Act
					buildDeployer.AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));

					// Assert
					AssertDatabaseExists(true, dbName);

					var recordsStmScheduleTask = GetRecords(() =>
						Db.NewAdminConnection(dbName), "[dbo].[StmScheduleTask]")
						.ToArray();
					var recordsStmServiceTask = GetRecords(() =>
						Db.NewAdminConnection(dbName), "[dbo].[StmServiceTask]")
						.ToArray();

					AssertEquals("StmScheduleTask should have one record", 1, recordsStmScheduleTask.Length);
					AssertEquals("StmServiceTask should have one record", 1, recordsStmServiceTask.Length);

					var upgStmScheduleTask = recordsStmScheduleTask[0];
					AssertEquals("S5_ScheduleType", "UPG", upgStmScheduleTask["S5_ScheduleType"]);
					AssertEquals("S5_IsActive", true, upgStmScheduleTask["S5_IsActive"]);
					AssertGreaterThan("S5_NextScheduledPrintRunTimeUtc",
						(DateTime)upgStmScheduleTask["S5_NextScheduledPrintRunTimeUtc"],
						new DateTime(2020, 1, 1, 23, 59, 59, 0));

					var upgStmServiceTask = recordsStmServiceTask[0];
					AssertEquals("SST_ServiceTaskCode", "UPG", upgStmServiceTask["SST_ServiceTaskCode"]);
					AssertEquals("SST_Active", true, upgStmServiceTask["SST_Active"]);
					AssertGreaterThan("SST_NextRunTime",
						(DateTimeOffset)upgStmServiceTask["SST_NextRunTime"],
						new DateTimeOffset(2020, 1, 1, 23, 59, 59, TimeSpan.Zero));
				}
				finally
				{
					var logger = new TestLogger();
					CreateDeployer(logger, mockCw1InstanceClass.Object).TeardownTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
					AssertDatabaseExists(false, dbName);
				}
			}
		}

		public void TestNotScheduleUPGAndNotRestoreFromBackupShouldLeaveUPGUntouched()
		{
			// Arrange
			var shelfName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var dbName = "SH0" + shelfName;
			var taskComments = $@"BRE 11-Jan-16 16:33:
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigRegister: false
TestRigLaunchDbUpgrade: false
TestRigServiceTasks: true
TestRigScheduleUPG: false";

			var mockCw1InstanceClass = new Mock<ICargoWiseOneInstanceClass>();
			mockCw1InstanceClass.Setup(c => c.AddNewInstance(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<System.DirectoryServices.DirectoryEntry>())).Returns(new Mock<ICargoWiseOneInstanceEntry>().Object);

			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			{
				try
				{
					SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");
					using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
					{
						using (var cmd = connection.Command($"CREATE DATABASE [{dbName}]"))
						{
							cmd.ExecuteNonQuery();
						}

						using (((ICurrentDbControl)connection).UseDatabase(dbName))
						using (var cmd = connection.Command(DataUtils.SQL_InitialTablesForEmptyDatabase()))
						{
							cmd.ExecuteNonQuery();
						}
					}

					var taskLogger = new TestLogger(
						line =>
						{
							if (line.Equals("start Uploading package", StringComparison.OrdinalIgnoreCase))
							{
								var nextRunTime = "'2020-01-01 23:59:59'";
								using (var connection = Db.NewAdminConnection(dbName))
								using (var command = connection.Command(Invariant($@"IF OBJECT_ID('[dbo].[StmScheduleTask]') IS NULL
BEGIN
	CREATE TABLE [dbo].[StmScheduleTask]
	(
		[S5_PK] UNIQUEIDENTIFIER NOT NULL,
		[S5_ScheduleDescription] VARCHAR(80) NOT NULL DEFAULT '',
		[S5_TaskPeriod] VARCHAR(3) NOT NULL DEFAULT '',
		[S5_WeekDaysOnly] BIT NOT NULL DEFAULT 1,
		[S5_TaskPeriodCount] INT NOT NULL DEFAULT 0,
		[S5_DayNumber] TINYINT NOT NULL DEFAULT 0,
		[S5_DayList] VARCHAR(7) NOT NULL DEFAULT 'NNNNNNN',
		[S5_MonthNumber] TINYINT NOT NULL DEFAULT 0,
		[S5_WeekDayOccurrenceNumber] TINYINT NOT NULL DEFAULT 0,
		[S5_EndAfterCount] INT NOT NULL DEFAULT 0,
		[S5_ScheduleActualRunCount] INT NOT NULL DEFAULT 0,
		[S5_AccountingPeriodScheduleFrstRun] INT NOT NULL DEFAULT 0,
		[S5_ScheduleType] VARCHAR(3) NOT NULL DEFAULT '',
		[S5_TypeOfDocument] CHAR(3) NOT NULL DEFAULT '',
		[S5_GS_NKPrintUser] VARCHAR(3) NOT NULL DEFAULT '',
		[S5_NextScheduledPrintRunTimeUtc] DATETIME NULL,
		[S5_IsActive] BIT NOT NULL DEFAULT 1,
		[S5_IsPrivate] BIT NOT NULL DEFAULT 0,
		[S5_OverdueDurationInSeconds] INT NOT NULL DEFAULT 0,
		[S5_ParentTableCode] VARCHAR(3) NOT NULL DEFAULT '',
		[S5_RunTimeInMinutes] INT NOT NULL DEFAULT 0,
		[S5_SystemCreateTimeUtc] SMALLDATETIME NULL,
		[S5_SystemCreateUser] VARCHAR(3) NOT NULL DEFAULT '',
		[S5_SystemLastEditTimeUtc] SMALLDATETIME NULL,
		[S5_SystemLastEditUser] VARCHAR(3) NOT NULL DEFAULT '',
	)
	ON [PRIMARY];
END
INSERT INTO [dbo].[StmScheduleTask] ([S5_PK], [S5_ScheduleType], [S5_IsActive], [S5_ScheduleDescription], [S5_ParentTableCode], [S5_NextScheduledPrintRunTimeUtc], [S5_SystemCreateTimeUtc], [S5_SystemCreateUser], [S5_SystemLastEditTimeUtc], [S5_SystemLastEditUser])
VALUES (NEWID(), 'UPG', 0, 'System Upgrade Service', 'SH', {nextRunTime}, GETUTCDATE(), 'TU1', GETUTCDATE(), 'TU1');

IF OBJECT_ID('[dbo].[StmServiceTask]') IS NULL
BEGIN
	CREATE TABLE [StmServiceTask]
	(
		[SST_PK] UNIQUEIDENTIFIER NOT NULL,
		[SST_ServiceTaskCode] VARCHAR(3) NOT NULL DEFAULT '',
		[SST_Active] BIT NOT NULL DEFAULT 0,
		[SST_Configuration] XML NOT NULL,
		[SST_NextRunTime] DateTimeOffset(0) NOT NULL DEFAULT '',
		[SST_SystemCreateTimeUtc] SMALLDATETIME NOT NULL,
		[SST_SystemCreateUser] VARCHAR(3) NOT NULL DEFAULT '',
		[SST_SystemLastEditTimeUtc] SMALLDATETIME NOT NULL,
		[SST_SystemLastEditUser] VARCHAR(3) NOT NULL DEFAULT '',
	);
END
INSERT INTO [dbo].[StmServiceTask] ([SST_PK], [SST_ServiceTaskCode], [SST_Active], [SST_Configuration], [SST_NextRunTime], [SST_SystemCreateTimeUtc], [SST_SystemCreateUser], [SST_SystemLastEditTimeUtc], [SST_SystemLastEditUser])
VALUES (NEWID(), 'UPG', 0, '', {nextRunTime}, GETUTCDATE(), 'TU1', GETUTCDATE(), 'TU1');
")))
								{
									command.ExecuteNonQuery();
								}
							}
						});
					var buildDeployer = CreateDeployer(taskLogger, mockCw1InstanceClass.Object);

					// Act
					buildDeployer.AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));

					// Assert
					AssertDatabaseExists(true, dbName);

					var recordsStmScheduleTask = GetRecords(() =>
						Db.NewAdminConnection(dbName), "[dbo].[StmScheduleTask]")
						.ToArray();
					var recordsStmServiceTask = GetRecords(() =>
						Db.NewAdminConnection(dbName), "[dbo].[StmServiceTask]")
						.ToArray();

					AssertEquals("StmScheduleTask should have one record", 1, recordsStmScheduleTask.Length);
					AssertEquals("StmServiceTask should have one record", 1, recordsStmServiceTask.Length);

					var upgStmScheduleTask = recordsStmScheduleTask[0];
					AssertEquals("S5_ScheduleType", "UPG", upgStmScheduleTask["S5_ScheduleType"]);
					AssertEquals("S5_IsActive", false, upgStmScheduleTask["S5_IsActive"]);
					AssertEquals("S5_NextScheduledPrintRunTimeUtc",
						(DateTime)upgStmScheduleTask["S5_NextScheduledPrintRunTimeUtc"],
						new DateTime(2020, 1, 1, 23, 59, 59, 0));

					var upgStmServiceTask = recordsStmServiceTask[0];
					AssertEquals("SST_ServiceTaskCode", "UPG", upgStmServiceTask["SST_ServiceTaskCode"]);
					AssertEquals("SST_Active", false, upgStmServiceTask["SST_Active"]);
					AssertEquals("SST_NextRunTime",
						(DateTimeOffset)upgStmServiceTask["SST_NextRunTime"],
						new DateTimeOffset(2020, 1, 1, 23, 59, 59, TimeSpan.Zero));
				}
				finally
				{
					var logger = new TestLogger();
					CreateDeployer(logger).TeardownTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));

					AssertDatabaseExists(true, dbName);

					using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
					using (var cmd = connection.Command($"DROP DATABASE [{dbName}]"))
					{
						cmd.ExecuteNonQuery();
					}

					AssertDatabaseExists(false, dbName);
				}
			}
		}

		public void TestScheduleUPGShouldNotModifyServiceTaskThatIsNotUPG()
		{
			// Arrange
			var shelfName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var dbName = "SH0" + shelfName;
			var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");
			var taskComments = $@"BRE 11-Jan-16 16:33:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigRegister: false
TestRigLaunchDbUpgrade: false
TestRigServiceTasks: true
TestRigScheduleUPG: true";

			var mockCw1InstanceClass = new Mock<ICargoWiseOneInstanceClass>();
			mockCw1InstanceClass.Setup(c => c.AddNewInstance(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<System.DirectoryServices.DirectoryEntry>())).Returns(new Mock<ICargoWiseOneInstanceEntry>().Object);

			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			{
				try
				{
					SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");
					var taskLogger = new TestLogger(
						line =>
						{
							if (line.Equals("start Scheduling DbUpgrade", StringComparison.OrdinalIgnoreCase))
							{
								var nextRunTime = "'2020-01-01 23:59:59'";
								using (var connection = Db.NewAdminConnection(dbName))
								using (var command = connection.Command(Invariant($@"IF OBJECT_ID('[dbo].[StmScheduleTask]') IS NULL
BEGIN
	CREATE TABLE [dbo].[StmScheduleTask]
	(
		[S5_PK] UNIQUEIDENTIFIER NOT NULL,
		[S5_ScheduleDescription] VARCHAR(80) NOT NULL DEFAULT '',
		[S5_TaskPeriod] VARCHAR(3) NOT NULL DEFAULT '',
		[S5_WeekDaysOnly] BIT NOT NULL DEFAULT 1,
		[S5_TaskPeriodCount] INT NOT NULL DEFAULT 0,
		[S5_DayNumber] TINYINT NOT NULL DEFAULT 0,
		[S5_DayList] VARCHAR(7) NOT NULL DEFAULT 'NNNNNNN',
		[S5_MonthNumber] TINYINT NOT NULL DEFAULT 0,
		[S5_WeekDayOccurrenceNumber] TINYINT NOT NULL DEFAULT 0,
		[S5_EndAfterCount] INT NOT NULL DEFAULT 0,
		[S5_ScheduleActualRunCount] INT NOT NULL DEFAULT 0,
		[S5_AccountingPeriodScheduleFrstRun] INT NOT NULL DEFAULT 0,
		[S5_ScheduleType] VARCHAR(3) NOT NULL DEFAULT '',
		[S5_TypeOfDocument] CHAR(3) NOT NULL DEFAULT '',
		[S5_GS_NKPrintUser] VARCHAR(3) NOT NULL DEFAULT '',
		[S5_NextScheduledPrintRunTimeUtc] DATETIME NULL,
		[S5_IsActive] BIT NOT NULL DEFAULT 1,
		[S5_IsPrivate] BIT NOT NULL DEFAULT 0,
		[S5_OverdueDurationInSeconds] INT NOT NULL DEFAULT 0,
		[S5_ParentTableCode] VARCHAR(3) NOT NULL DEFAULT '',
		[S5_RunTimeInMinutes] INT NOT NULL DEFAULT 0,
		[S5_SystemCreateTimeUtc] SMALLDATETIME NULL,
		[S5_SystemCreateUser] VARCHAR(3) NOT NULL DEFAULT '',
		[S5_SystemLastEditTimeUtc] SMALLDATETIME NULL,
		[S5_SystemLastEditUser] VARCHAR(3) NOT NULL DEFAULT '',
	)
	ON [PRIMARY];
END
INSERT INTO [dbo].[StmScheduleTask] ([S5_PK], [S5_ScheduleType], [S5_IsActive], [S5_ScheduleDescription], [S5_ParentTableCode], [S5_NextScheduledPrintRunTimeUtc], [S5_SystemCreateTimeUtc], [S5_SystemCreateUser], [S5_SystemLastEditTimeUtc], [S5_SystemLastEditUser])
VALUES
	(NEWID(), 'UPG', 0, 'System Upgrade Service', 'SH', {nextRunTime}, GETUTCDATE(), 'TU1', GETUTCDATE(), 'TU1'),
	(NEWID(), 'UAR', 0, 'another service task', 'SH', NULL, GETUTCDATE(), 'TU1', GETUTCDATE(), 'TU1');

IF OBJECT_ID('[dbo].[StmServiceTask]') IS NULL
BEGIN
	CREATE TABLE [StmServiceTask]
	(
		[SST_PK] UNIQUEIDENTIFIER NOT NULL,
		[SST_ServiceTaskCode] VARCHAR(3) NOT NULL DEFAULT '',
		[SST_Active] BIT NOT NULL DEFAULT 0,
		[SST_Configuration] XML NOT NULL,
		[SST_NextRunTime] DateTimeOffset(0) NOT NULL DEFAULT '',
		[SST_SystemCreateTimeUtc] SMALLDATETIME NOT NULL,
		[SST_SystemCreateUser] VARCHAR(3) NOT NULL DEFAULT '',
		[SST_SystemLastEditTimeUtc] SMALLDATETIME NOT NULL,
		[SST_SystemLastEditUser] VARCHAR(3) NOT NULL DEFAULT '',
	);
END
INSERT INTO [dbo].[StmServiceTask] ([SST_PK], [SST_ServiceTaskCode], [SST_Active], [SST_Configuration], [SST_NextRunTime], [SST_SystemCreateTimeUtc], [SST_SystemCreateUser], [SST_SystemLastEditTimeUtc], [SST_SystemLastEditUser])
VALUES
	(NEWID(), 'UPG', 0, '', {nextRunTime}, GETUTCDATE(), 'TU1', GETUTCDATE(), 'TU1'),
	(NEWID(), 'UAR', 0, '', {nextRunTime}, GETUTCDATE(), 'TU1', GETUTCDATE(), 'TU1');
")))
								{
									command.ExecuteNonQuery();
								}
							}
						});
					var buildDeployer = CreateDeployer(taskLogger, mockCw1InstanceClass.Object);

					// Act
					buildDeployer.AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));

					// Assert
					AssertDatabaseExists(true, dbName);

					var recordsStmScheduleTask = GetRecords(() =>
						Db.NewAdminConnection(dbName), "[dbo].[StmScheduleTask]")
						.ToArray();
					var recordsStmServiceTask = GetRecords(() =>
						Db.NewAdminConnection(dbName), "[dbo].[StmServiceTask]")
						.ToArray();

					AssertEquals("StmScheduleTask should have two records", 2, recordsStmScheduleTask.Length);
					AssertEquals("StmServiceTask should have two records", 2, recordsStmServiceTask.Length);

					var uarStmScheduleTask = recordsStmScheduleTask[1];
					AssertEquals("S5_ScheduleType", "UAR", uarStmScheduleTask["S5_ScheduleType"]);
					AssertEquals("S5_IsActive", false, uarStmScheduleTask["S5_IsActive"]);
					AssertEquals("S5_NextScheduledPrintRunTimeUtc", DBNull.Value, uarStmScheduleTask["S5_NextScheduledPrintRunTimeUtc"]);

					var uarStmServiceTask = recordsStmServiceTask[1];
					AssertEquals("SST_ServiceTaskCode", "UAR", uarStmServiceTask["SST_ServiceTaskCode"]);
					AssertEquals("SST_Active", false, uarStmServiceTask["SST_Active"]);
					AssertEquals("SST_NextRunTime",
						new DateTimeOffset(2020, 1, 1, 23, 59, 59, TimeSpan.Zero),
						uarStmServiceTask["SST_NextRunTime"]);
				}
				finally
				{
					var logger = new TestLogger();
					CreateDeployer(logger, mockCw1InstanceClass.Object).TeardownTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
					AssertDatabaseExists(false, dbName);
				}
			}
		}

		public void TestAutoDeployTestedShelfServiceTasksCUR() => AssertAutoDeployTestedShelfServiceTasks(packageStatus: "CUR");
		public void TestAutoDeployTestedShelfServiceTasksRDY() => AssertAutoDeployTestedShelfServiceTasks(packageStatus: "RDY");

		void AssertAutoDeployTestedShelfServiceTasks(string packageStatus)
		{
			var shelfName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var dbName = "SH0" + shelfName;
			var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");
			var taskComments = $@"BRE 11-Jan-16 16:33:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigRegister: false
TestRigLaunchDbUpgrade: false
TestRigServiceTasks: true
TestRigPackageStatus: {packageStatus}
";

			var mockCw1InstanceClass = new Mock<ICargoWiseOneInstanceClass>();
			mockCw1InstanceClass.Setup(c => c.AddNewInstance(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<System.DirectoryServices.DirectoryEntry>())).Returns(new Mock<ICargoWiseOneInstanceEntry>().Object);
			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			{
				try
				{
					SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");

					var logger = new TestLogger();
					CreateDeployer(logger, mockCw1InstanceClass.Object).AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
					var log = logger.ToString();

					AssertDatabaseExists(true, dbName);
					AssertDeploymentIDLogged(log, dbName);

					using (var connection = Db.NewAdminConnection(dbName))
					{
						var sqlConnection = ((IDbConnectionInternals)connection).ADOConnection;
						var upgradeManager = new SqlUpgradeManager(new UpgradeSqlContext(sqlConnection, sqlConnection.DataSource, sqlConnection.Database));

						if (packageStatus == "CUR")
						{
							AssertEquals(log, new Version(16, 4, 1, 69), upgradeManager.QueryCurrentVersion().Version);
							var runnablePackages = upgradeManager.QueryRunnablePackages().OfType<UpgradeInfoExtended>().ToArray();
							AssertEquals("Runnable package count", 0, runnablePackages.Length);
						}
						else
						{
							AssertNull(upgradeManager.QueryCurrentVersion());
							var runnablePackages = upgradeManager.QueryRunnablePackages().OfType<UpgradeInfoExtended>().ToArray();
							AssertEquals("Runnable package count", 1, runnablePackages.Length);
							AssertEquals("Runnable package version", new Version(16, 4, 1, 69), runnablePackages[0].Version);
						}
					}

					mockCw1InstanceClass.Verify(c => c.AddNewInstance(shelfName, Db.ServerName, dbName, It.Is<System.DirectoryServices.DirectoryEntry>(e => e.Path == "LDAP://OU=OrchestratedSH0,OU=ASPAC,OU=Applications,OU=root,DC=sand,DC=wtg,DC=zone")));
				}
				finally
				{
					var mockCw1SearchResult = new Mock<ICargoWiseOneInstanceSearchResult>();
					var mockCw1Entry = new Mock<ICargoWiseOneInstanceEntry>();
					mockCw1InstanceClass.Setup(c => c.FindInstance(shelfName, It.Is<DirectorySearchOptions>(o => o.DomainName == "sand.wtg.zone"))).Returns(mockCw1SearchResult.Object);
					mockCw1SearchResult.Setup(r => r.GetDirectoryEntry()).Returns(mockCw1Entry.Object);

					var logger = new TestLogger();
					CreateDeployer(logger, mockCw1InstanceClass.Object).TeardownTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));

					AssertDatabaseExists(false, dbName);
					mockCw1Entry.Verify(e => e.Delete(), Times.Once());
				}
			}
		}

		public void TestTearDownDatabaseInSingleUserMode()
		{
			var shelfName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var dbName = "SH0" + shelfName;

			var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");
			var taskComments = $@"BRE 11-Jan-16 16:33:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigCreateIcon: false
TestRigRegister: false
TestRigLaunchDbUpgrade: false";

			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			{
				try
				{
					SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");

					var logger = new TestLogger();
					CreateDeployer(logger).AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
					var log = logger.ToString();

					AssertDatabaseExists(true, dbName);
					AssertDeploymentIDLogged(log, dbName);
					AssertDatabaseLoginExists($"{dbName}_%");

					using (var connection = Db.NewAdminConnection(dbName))
					{
						var sqlConnection = ((IDbConnectionInternals)connection).ADOConnection;
						var upgradeManager = new SqlUpgradeManager(new UpgradeSqlContext(sqlConnection, sqlConnection.DataSource, sqlConnection.Database));

						AssertEquals(log, new Version(16, 4, 1, 69), upgradeManager.QueryCurrentVersion().Version);
					}
				}
				finally
				{
					using (var connection = Db.NewAdminConnection(dbName))
					{
						connection.ExecuteNonQuery($"ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE");

						var logger = new TestLogger();
						CreateDeployer(logger).TeardownTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));

						AssertDatabaseExists(false, dbName);
						AssertDatabaseLoginDoesNotExist($"{dbName}_%");
					}
				}
			}
		}

		[RequiresSoftware(RequiredSoftware.IsVM)]
		public void TestTeardownRemovesOrphanedAppPool()
		{
			var shelfName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var dbName = "SH0" + shelfName;
			var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");
			var webRootDomain = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var taskComments = $@"LCD 28-Apr-21 00:02 GMT+10:00:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigCreateIcon: false
TestRigRegister: false
TestRigLaunchDbUpgrade: false
TestRigWebSites: GlowWebClient,Forwarding
TestRigWebServer: localhost
TestRigWebDomain: {webRootDomain}
TestRigWebServerInstallPath: {webServerInstallPath}
";

			Process.Start("winrm", "quickconfig -quiet").WaitForExit();

			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			{
				try
				{
					SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");

					var logger = new TestLogger();
					CreateDeployer(logger).AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
					var log = logger.ToString();

					AssertSiteExists(log, true, webRootDomain, "Portals", Db.ServerName, dbName);

					using (var serverManager = new ServerManager())
					{
						var appPool = serverManager.ApplicationPools.FirstOrDefault(ap => ap.Name.StartsWith(webRootDomain));
						AssertNotNull("AppPool item should exist", appPool);
						var siteItem = serverManager.Sites.SingleOrDefault(s => s.Name == webRootDomain);
						AssertEquals(log, true, siteItem != null);
						foreach (var app in siteItem.Applications.ToArray())
						{
							siteItem.Applications.Remove(app);
						}
						serverManager.Sites.Remove(siteItem);
						serverManager.CommitChanges();
						appPool = serverManager.ApplicationPools.FirstOrDefault(ap => ap.Name.StartsWith(webRootDomain));
						AssertEquals("AppPool item should still exist", true, appPool != null);
					}
				}
				finally
				{
					var logger = new TestLogger();
					CreateDeployer(logger).TeardownTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
					var log = logger.ToString();

					AssertSiteExists(log, false, webRootDomain, "Portals", Db.ServerName, dbName);
					using (var serverManager = new ServerManager())
					{
						var appPool = serverManager.ApplicationPools.FirstOrDefault(ap => ap.Name.StartsWith(webRootDomain));
						AssertEquals(log, true, appPool == null);
						var siteItem = serverManager.Sites.SingleOrDefault(s => s.Name == webRootDomain);
						AssertEquals(log, true, siteItem == null);
					}
				}
			}
		}

		[RequiresSoftware(RequiredSoftware.IsVM)]
		public void TestTeardownStopAppPool()
		{
			var shelfName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var dbName = "SH0" + shelfName;
			var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");
			var webRootDomain = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var taskComments = $@"LCD 28-Apr-21 00:02 GMT+10:00:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigCreateIcon: false
TestRigRegister: false
TestRigLaunchDbUpgrade: false
TestRigWebSites: GlowWebClient,Forwarding
TestRigWebServer: localhost
TestRigWebDomain: {webRootDomain}
TestRigWebServerInstallPath: {webServerInstallPath}
TestRigVerbose: true
";

			Process.Start("winrm", "quickconfig -quiet").WaitForExit();
			List<string> appPoolNames = new List<string>();
			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			{
				try
				{
					SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");

					var logger = new TestLogger();
					CreateDeployer(logger).AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
					var log = logger.ToString();

					using (var serverManager = new ServerManager())
					{
						foreach (var appPool in serverManager.ApplicationPools.Where(ap => ap.Name.StartsWith(webRootDomain)))
						{
							AssertEquals("AppPool should have started", true, appPool.State == ObjectState.Started || appPool.State == ObjectState.Starting);
							appPoolNames.Add(appPool.Name);
							if (appPoolNames.Count == 1)
							{
								appPool.Stop();
								AssertEquals("AppPool should be stopped", true, appPool.State == ObjectState.Stopping || appPool.State == ObjectState.Stopped);
							}
						}
					}

					AssertEquals(log, 2, appPoolNames.Count);
				}
				finally
				{
					var logger = new TestLogger();
					CreateDeployer(logger).TeardownTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
					var log = logger.ToString();

					AssertNotContains("The first App pool should NOT be stopped", $"[Verbose]Stop-WebAppPool -Name {appPoolNames[0]}", log);

					string[] expected2ndAppLogs = {
						$"[Verbose]Stop-WebAppPool -Name {appPoolNames[1]}",
						$"[Verbose]Remove-WebApplication -Name /{appPoolNames[1].Split('_')[1]} -Site {webRootDomain}",
					};
					AssertContainsInOrder("The second AppPool should be stopped first", log, expected2ndAppLogs);

					AssertSiteExists(log, false, webRootDomain, "Portals", Db.ServerName, dbName);
					using (var serverManager = new ServerManager())
					{
						var appPool = serverManager.ApplicationPools.FirstOrDefault(ap => ap.Name.StartsWith(webRootDomain));
						AssertNull(log, appPool);
					}
				}
			}
		}

		[RequiresSoftware(RequiredSoftware.IsVM)]
		public void TestLoggingContainsMatchedSitesAndWebServer()
		{
			var shelfName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var dbName = "SH0" + shelfName;
			var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");
			var webRootDomain = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var taskComments = $@"LCD 28-Apr-21 00:02 GMT+10:00:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigCreateIcon: false
TestRigRegister: false
TestRigLaunchDbUpgrade: false
TestRigWebSites: Services,Glow,GlowWebClient
TestRigWebServer: localhost
TestRigWebDomain: {webRootDomain}
TestRigWebServerInstallPath: {webServerInstallPath}
";

			Process.Start("winrm", "quickconfig -quiet").WaitForExit();

			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			{
				CombineAssertions("Logging for AutoDeployTestedShelf and TeardownTestedShelf", () =>
				{
					try
					{
						SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");

						var logger = new TestLogger();
						CreateDeployer(logger).AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
						var log = logger.ToString();

						AssertSiteExists(log, true, webRootDomain, "Glow", Db.ServerName, dbName);
						AssertContains("Logs should include sites matched", "Glow => GLOW Web Services", log);
						AssertContains("Logs should include sites matched", "GlowWebClient => GLOW Web Portals", log);
						AssertContains("Logs should include sites matched", "Services => General/Shared Web Services", log);
						AssertContains("Logs should include sites matched", "Running install script on localhost", log);
					}
					finally
					{
						var logger = new TestLogger();
						CreateDeployer(logger).TeardownTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
						var log = logger.ToString();

						AssertSiteExists(log, false, webRootDomain, "Glow", Db.ServerName, dbName);
					}
				});
			}
		}

		[RequiresSoftware(RequiredSoftware.IsVM)]
		public void TestWarningLoggedForUnMatchedSites()
		{
			var shelfName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var dbName = "SH0" + shelfName;
			var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");
			var webRootDomain = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var taskComments = $@"LCD 28-Apr-21 00:02 GMT+10:00:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigCreateIcon: false
TestRigRegister: false
TestRigLaunchDbUpgrade: false
TestRigWebSites: Glow,UnknownWebSiteName,Services
TestRigWebServer: localhost
TestRigWebDomain: {webRootDomain}
TestRigWebServerInstallPath: {webServerInstallPath}
";

			Process.Start("winrm", "quickconfig -quiet").WaitForExit();

			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			{
				CombineAssertions("Logging for AutoDeployTestedShelf and TeardownTestedShelf", () =>
				{
					try
					{
						SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");

						var logger = new TestLogger();
						CreateDeployer(logger).AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
						var log = logger.ToString();

						AssertSiteExists(log, true, webRootDomain, "Glow", Db.ServerName, dbName);
						AssertContains("Logs should include sites matched", "Glow => GLOW Web Services", log);
						AssertContains("Logs should include sites matched", "Services => General/Shared Web Services", log);
						AssertContains("Logs should include warning for unmatched site", "[Warning]UnknownWebSiteName not found in valid sites", log);
					}
					finally
					{
						var logger = new TestLogger();
						CreateDeployer(logger).TeardownTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
						var log = logger.ToString();

						AssertSiteExists(log, false, webRootDomain, "Glow", Db.ServerName, dbName);
					}
				});
			}
		}

		[RequiresSoftware(RequiredSoftware.IsVM)]
		public void TestErrorIncludesStackTraces()
		{
			var shelfName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var dbName = "SH0" + shelfName;
			var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");
			var webRootDomain = "Error / in \\ here !@#$%^&";
			var taskComments = $@"LCD 28-Apr-21 00:02 GMT+10:00:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigCreateIcon: false
TestRigRegister: false
TestRigLaunchDbUpgrade: false
TestRigWebSites: Glow,UnknownWebSiteName,Services
TestRigWebServer: localhost
TestRigWebDomain: {webRootDomain}
TestRigWebServerInstallPath: {webServerInstallPath}
";

			Process.Start("winrm", "quickconfig -quiet").WaitForExit();

			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			{
				CombineAssertions("Logging for AutoDeployTestedShelf and TeardownTestedShelf", () =>
				{
					try
					{
						SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");

						var logger = new TestLogger();
						AssertExceptionThrown<AggregateException>(() => CreateDeployer(logger).AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments)));
						var log = logger.ToString();
						AssertContains("Logs should include Stack Trace", "--- End of inner exception stack trace ---", log);
					}
					finally
					{
						var logger = new TestLogger();
						CreateDeployer(logger).TeardownTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
						var log = logger.ToString();
						AssertSiteExists(log, false, webRootDomain, "Glow", Db.ServerName, dbName);
					}
				});
			}
		}

		[RequiresSoftware(RequiredSoftware.IsVM)]
		public void TestVerboseAndDebugShownOnError()
		{
			var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");
			var webRootDomain = "Error / in \\ here !@#$%^&";
			var taskComments = $@"LCD 28-Apr-21 00:02 GMT+10:00:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigCreateIcon: false
TestRigRegister: false
TestRigLaunchDbUpgrade: false
TestRigWebSites: Glow,Services
TestRigWebServer: localhost
TestRigWebDomain: {webRootDomain}
TestRigWebServerInstallPath: {webServerInstallPath}
";
			var shelfName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);

			Process.Start("winrm", "quickconfig -quiet").WaitForExit();

			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			{
				CombineAssertions("Logging for AutoDeployTestedShelf and TeardownTestedShelf", () =>
				{
					try
					{
						SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");

						var logger = new TestLogger();
						AssertExceptionThrown<AggregateException>(() => CreateDeployer(logger).AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments)));
						var log = logger.ToString();

						AssertContains("[Verbose]", log);
						AssertContains("[Debug]", log);
					}
					finally
					{
						var logger = new TestLogger();
						CreateDeployer(logger).TeardownTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
					}
				});
			}
		}

		[RequiresSoftware(RequiredSoftware.IsVM)]
		public void TestVerboseAndDebugShownOnRequest()
		{
			var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");
			var webRootDomain = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var taskComments = $@"LCD 28-Apr-21 00:02 GMT+10:00:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigCreateIcon: false
TestRigRegister: false
TestRigLaunchDbUpgrade: false
TestRigWebSites: Glow,UnknownWebSiteName,Services
TestRigWebServer: localhost
TestRigWebDomain: {webRootDomain}
TestRigWebServerInstallPath: {webServerInstallPath}
TestRigVerbose: true
";
			DeployAndAssertThatVerboseLoggingIsCorrect(taskComments, true);
		}

		[RequiresSoftware(RequiredSoftware.IsVM)]
		public void TestVerboseAndDebugNotShownByDefault()
		{
			var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");
			var webRootDomain = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var taskComments = $@"LCD 28-Apr-21 00:02 GMT+10:00:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigCreateIcon: false
TestRigRegister: false
TestRigLaunchDbUpgrade: false
TestRigWebSites: Glow,UnknownWebSiteName,Services
TestRigWebServer: localhost
TestRigWebDomain: {webRootDomain}
TestRigWebServerInstallPath: {webServerInstallPath}
";
			DeployAndAssertThatVerboseLoggingIsCorrect(taskComments, false);
		}

		void DeployAndAssertThatVerboseLoggingIsCorrect(string taskComments, bool expectVerbose)
		{
			Process.Start("winrm", "quickconfig -quiet").WaitForExit();
			var shelfName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);

			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			{
				CombineAssertions("Logging for AutoDeployTestedShelf and TeardownTestedShelf", () =>
				{
					try
					{
						SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");

						var logger = new TestLogger();
						CreateDeployer(logger).AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
						var log = logger.ToString();
						if (expectVerbose)
						{
							AssertContains("[Verbose]calling CargoWiseOne.WebInfrastructure.RemoteSiteInstaller.Install()", log);
							AssertContains("[Debug]@{Name=CargoWiseOne.WebInfrastructure.dll; FileVersion=", log);
						}
						else
						{
							AssertNotContains("[Verbose]calling CargoWiseOne.WebInfrastructure.RemoteSiteInstaller.Install()", log);
							AssertNotContains("[Debug]@{Name=CargoWiseOne.WebInfrastructure.dll; FileVersion=", log);
						}
					}
					finally
					{
						var logger = new TestLogger();
						CreateDeployer(logger).TeardownTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
					}
				});
			}
		}

		[RequiresSoftware(RequiredSoftware.IsVM)]
		public void TestLoggingInTeardownVerbose()
		{
			var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");
			var webRootDomain = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var taskComments = $@"LCD 28-Apr-21 00:02 GMT+10:00:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigCreateIcon: false
TestRigRegister: false
TestRigLaunchDbUpgrade: false
TestRigWebSites: Glow,UnknownWebSiteName,Services
TestRigWebServer: localhost
TestRigWebDomain: {webRootDomain}
TestRigWebServerInstallPath: {webServerInstallPath}
TestRigVerbose: true
";
			DeployAndAssertThatTeardownLoggingIsCorrect(taskComments, true);
		}

		[RequiresSoftware(RequiredSoftware.IsVM)]
		public void TestLoggingInTeardownNotVerbose()
		{
			var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");
			var webRootDomain = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var taskComments = $@"LCD 28-Apr-21 00:02 GMT+10:00:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigCreateIcon: false
TestRigRegister: false
TestRigLaunchDbUpgrade: false
TestRigWebSites: Glow,UnknownWebSiteName,Services
TestRigWebServer: localhost
TestRigWebDomain: {webRootDomain}
TestRigWebServerInstallPath: {webServerInstallPath}
";
			DeployAndAssertThatTeardownLoggingIsCorrect(taskComments, false);
		}

		void DeployAndAssertThatTeardownLoggingIsCorrect(string taskComments, bool expectVerbose)
		{
			Process.Start("winrm", "quickconfig -quiet").WaitForExit();
			var shelfName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var dbName = "SH0" + shelfName;

			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			{
				CombineAssertions("Logging for TeardownTestedShelf", () =>
				{
					try
					{
						SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");

						var logger = new TestLogger();
						CreateDeployer(logger).AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
					}
					finally
					{
						var logger = new TestLogger();
						CreateDeployer(logger).TeardownTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
						var log = logger.ToString();
						AssertContains("start Dropping Databases", log);
						AssertContains($"Dropping database [{dbName}] from SQL Server [{Db.ServerName}]...", log);
						if (expectVerbose)
						{
							AssertContains("[Verbose]Looking for Website", log);
						}
						else
						{
							AssertNotContains("[Verbose]Looking for Website", log);
						}
						AssertContains("TestedShelfDeploymentOptions", log);
						AssertDeploymentIDLogged(log, dbName);
					}
				});
			}
		}

		[RequiresSoftware(RequiredSoftware.IsVM)]
		public void TestWebUrlsShownInLog()
		{
			var shelfName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var dbName = "SH0" + shelfName;
			var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");
			var webRootDomain = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var taskComments = $@"LCD 28-Apr-21 00:02 GMT+10:00:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigCreateIcon: false
TestRigRegister: false
TestRigLaunchDbUpgrade: false
TestRigWebSites: Glow,Forwarding,Services,GlowWebClient
TestRigWebServer: localhost
TestRigWebDomain: {webRootDomain}
TestRigWebServerInstallPath: {webServerInstallPath}
";

			Process.Start("winrm", "quickconfig -quiet").WaitForExit();

			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			{
				CombineAssertions("Logging for AutoDeployTestedShelf and TeardownTestedShelf", () =>
				{
					try
					{
						SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");

						var logger = new TestLogger();
						CreateDeployer(logger).AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
						var log = logger.ToString();

						AssertSiteExists(log, true, webRootDomain, "Glow", Db.ServerName, dbName);
						AssertContains("Logs should include WebUrl for Glow", $"https://{webRootDomain}/Glow", log);
						AssertContains("Logs should include WebUrl for Forwarding", $"https://{webRootDomain}/Tracking", log);
						AssertContains("Logs should include WebUrl for Services", $"https://{webRootDomain}/Services", log);
						AssertContains("Logs should include WebUrl for GlowWebClient", $"https://{webRootDomain}/Portals", log);
					}
					finally
					{
						var logger = new TestLogger();
						CreateDeployer(logger).TeardownTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
						var log = logger.ToString();

						AssertSiteExists(log, false, webRootDomain, "Glow", Db.ServerName, dbName);
					}
				});
			}
		}

		public void TestAutoDeployTestedShelfWithWinzor()
		{
			const int CargoWiseOneRDPAndWinzor = 10;
			var shelfName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var dbName = "SH0" + shelfName;

			using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
			{
				string sid = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
				using (var command = connection.Command(Invariant($"CREATE LOGIN [EnterpriseDbUser_{dbName}_{sid}] WITH PASSWORD = '', CHECK_POLICY = OFF, DEFAULT_LANGUAGE = us_english, SID = 0x{sid}")))
				{
					command.ExecuteNonQuery();
				}
			}

			var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");
			var taskComments = $@"BRE 11-Jan-16 16:33:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigRegister: false
TestRigLaunchDbUpgrade: false
TestRigDeployWinzor: true
TestRigVersionBrokerWarmupTimeout: 00:00:05";

			var mockAd = MockActiveDirecotry();
			var mockCw1InstanceClass = Mock.Get(mockAd.CW1InstanceClass);
			var mockCw1InstanceEntry = Mock.Get(mockAd.CW1InstanceEntry);
			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			using (var unpackDirectory = new TempDirectory())
			{
				try
				{
					SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");
					Directory.CreateDirectory(Path.Combine(binPathDirectory.DirectoryName, "AppServer"));
					Directory.CreateDirectory(Path.Combine(binPathDirectory.DirectoryName, "SessionBroker"));
					Directory.CreateDirectory(Path.Combine(binPathDirectory.DirectoryName, "winzor"));
					File.WriteAllText(Path.Combine(binPathDirectory.DirectoryName, "AppServer", "Blazor.dll"), "whatever");
					File.WriteAllText(Path.Combine(binPathDirectory.DirectoryName, "SessionBroker", "Blazor.dll"), "whatever");
					File.WriteAllText(Path.Combine(binPathDirectory.DirectoryName, "winzor", "Blazor.dll"), "whatever");

					var logger = new TestLogger();
					CreateDeployer(logger, mockAd.CW1InstanceClass, mockAd.DirectorySearcher).AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
					var log = logger.ToString();

					AssertDatabaseExists(true, dbName);

					using (var connection = Db.NewAdminConnection(dbName))
					{
						var sqlConnection = ((IDbConnectionInternals)connection).ADOConnection;
						var upgradeManager = new SqlUpgradeManager(new UpgradeSqlContext(sqlConnection, sqlConnection.DataSource, sqlConnection.Database));

						UpgradeInfo upgrade = upgradeManager.QueryCurrentVersion();

						AssertEquals(log, new Version(16, 4, 1, 69), upgrade.Version);

						using (var tempFile = TempFile.New())
						{
							upgradeManager.DownloadUpgradePackageFile(upgrade.PK, tempFile.Filename);
							EdpFile.Unpack(tempFile.Filename, unpackDirectory);
							string appServerPath = Path.Combine(unpackDirectory, "Distribution", "Application", "AppServer");
							Assert("Blazor dll should exist", File.Exists(Path.Combine(appServerPath, "Blazor.dll")));
						}

						using (var cmd = connection.Command("select cast(SD_BinaryValue as nvarchar(max)) from dbo.StmData where SD_Name = 'BlazorUrl'"))
						{
							AssertEquals("https://" + shelfName + ".blazor.sand.wtg.zone/backchannel", cmd.ExecuteScalar());
						}
					}

					mockCw1InstanceClass.Verify(c => c.AddNewInstance(shelfName, Db.ServerName, dbName, It.Is<System.DirectoryServices.DirectoryEntry>(e => e.Path == "LDAP://CN=SH0,CN=WiseTech Global,CN=Program Data,DC=sand,DC=wtg,DC=zone")));
					mockCw1InstanceEntry.VerifySet(c => c.BlazorUrlAuthority = shelfName + ".blazor.sand.wtg.zone");
					mockCw1InstanceEntry.VerifySet(c => c.Flags = CargoWiseOneRDPAndWinzor);
				}
				finally
				{
					var mockCw1SearchResult = new Mock<ICargoWiseOneInstanceSearchResult>();
					var mockCw1Entry = new Mock<ICargoWiseOneInstanceEntry>();
					mockCw1InstanceClass.Setup(c => c.FindInstance(shelfName, It.Is<DirectorySearchOptions>(o => o.DomainName == "sand.wtg.zone"))).Returns(mockCw1SearchResult.Object);
					mockCw1SearchResult.Setup(r => r.GetDirectoryEntry()).Returns(mockCw1Entry.Object);

					var logger = new TestLogger();
					CreateDeployer(logger, mockCw1InstanceClass.Object).TeardownTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));

					AssertDatabaseExists(false, dbName);
					mockCw1Entry.Verify(e => e.Delete(), Times.Once());
				}
			}
		}

		public void TestTestRigDeploymentLogsIncludeNotesAndCalculatedValues()
		{
			var shelfName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var dbName = "SH0" + shelfName;
			var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");
			var taskComments = $@"LCD 20-Jul-21 12:34:
https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev/pullrequest/12345

TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigCreateIcon: false
TestRigRegister: false
TestRigLaunchDbUpgrade: false
TestRigClientCode: EDI";

			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			{
				try
				{
					SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");

					var logger = new TestLogger();
					CreateDeployer(logger).AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
					var log = logger.ToString();
					AssertDatabaseExists(true, dbName);
					AssertDeploymentIDLogged(log, dbName);
					var expectedComments = $@"TaskComments:{{
	LCD 20-Jul-21 12:34:
	https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev/pullrequest/12345
	
	TestRigRestoreFromBackup: {backupPath}
	TestRigSqlServer: {Db.ServerName}
	TestRigSqlServerDataFilePath: {Temp.TempPath}
	TestRigSqlServerLogFilePath: {Temp.TempPath}
	TestRigCreateIcon: false
	TestRigRegister: false
	TestRigLaunchDbUpgrade: false
	TestRigClientCode: EDI
}}";
					AssertContains(expectedComments, log);
					//more options will be added, order may not be known, so just a sample
					var expectedOptions = $@"TestedShelfDeploymentOptions:{{
	""RestoreFromBackup"": ""{backupPath.Replace("\\", "\\\\")}"",
	""SqlServer"": ""{ReplaceSlash(Db.ServerName)}"",
	""SqlServerDataFilePath"": ""{ReplaceSlash(Temp.TempPath)}"",
	""SqlServerLogFilePath"": ""{ReplaceSlash(Temp.TempPath)}"",
	""CreateIcon"": false,
	""IconName"": ""{shelfName}"",
	""Register"": false,
	""LaunchDbUpgrade"": false,
	""ClientCode"": ""EDI"",
	""AnyOptionsSet"": true,
	""WebSites"": [],
	""RegistryEntries"": {{}},
	""ServiceTasks"": false,
	""EnableAudit"": false,
	""Verbose"": false,
	""ScheduleUPG"": false,
	""PackageStatus"": ""CUR"",
}}";
					foreach (var line in expectedOptions.Split('\n'))
					{
						AssertContains(line, log);
					}
				}
				finally
				{
					var logger = new TestLogger();
					CreateDeployer(logger).TeardownTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));

					AssertDatabaseExists(false, dbName);
				}
			}
		}

		public void TestTestRigDeploymentLogsIncludeNotesAndCalculatedValuesWithBogusValues()
		{
			var shelfName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var dbName = "SH0" + shelfName;
			var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");
			var taskComments = $@"LCD 31-Feb-21 23:59:
TestRigSqlServer: {Db.ServerName}
TestRigDatabaseName: LetsMakeDeployGoSplat
TestRigCreateIcon: Nope
TestRigRegister: Nah
TestRigBlowUp: Yeah Baby !";

			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			{
				SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");

				var logger = new TestLogger();
				AssertExceptionThrown<SqlException>(() => CreateDeployer(logger).AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments)));
				var log = logger.ToString();
				var expectedComments = $@"TaskComments:{{
	LCD 31-Feb-21 23:59:
	TestRigSqlServer: {Db.ServerName}
	TestRigDatabaseName: LetsMakeDeployGoSplat
	TestRigCreateIcon: Nope
	TestRigRegister: Nah
	TestRigBlowUp: Yeah Baby !
}}";
				AssertContains(expectedComments, log);
				//more options will be added, order may not be known, so just a sample
				var expectedOptions = $@"TestedShelfDeploymentOptions:{{
	""RestoreFromBackup"": null,
	""SqlServer"": ""{ReplaceSlash(Db.ServerName)}"",
	""CreateIcon"": true,
	""IconName"": ""LetsMakeDeployGoSplat"",
	""Register"": false,
	""LaunchDbUpgrade"": true,
	""ClientCode"": null,
	""AnyOptionsSet"": true,
	""WebSites"": [],
	""RegistryEntries"": {{}},
	""ServiceTasks"": false,
	""EnableAudit"": false,
	""Verbose"": false,
	""ScheduleUPG"": false,
	""PackageStatus"": ""CUR"",
}}";
				foreach (var line in expectedOptions.Split('\n'))
				{
					AssertContains(line, log);
				}
			}
		}

		static string ReplaceSlash(string item)
		{
			return item.Replace("\\", "\\\\");
		}

		public void TestAutoDeployTestedShelfWithSingleRefDb()
		{
			var shelfName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var dbName = "SH0" + shelfName;

			using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
			{
				string sid = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
				using (var command = connection.Command(Invariant($"CREATE LOGIN [EnterpriseDbUser_{dbName}_{sid}] WITH PASSWORD = '', CHECK_POLICY = OFF, DEFAULT_LANGUAGE = us_english, SID = 0x{sid}")))
				{
					command.ExecuteNonQuery();
				}
			}

			var sRDbName = @"SH0WI00000001-CW-RefDatabase";
			var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");
			var taskComments = $@"JCI 07-Dec-21 16:33:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigRegister: false
TestRigLaunchDbUpgrade: false
TestRigSingleRefDatabaseName: {sRDbName}";

			var mockCw1InstanceClass = new Mock<ICargoWiseOneInstanceClass>();
			mockCw1InstanceClass.Setup(c => c.AddNewInstance(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<System.DirectoryServices.DirectoryEntry>())).Returns(new Mock<ICargoWiseOneInstanceEntry>().Object);
			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			{
				try
				{
					SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");

					var logger = new TestLogger();
					CreateDeployer(logger, mockCw1InstanceClass.Object).AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
					var log = logger.ToString();

					AssertDatabaseExists(true, dbName);
					using (var connection = Db.NewAdminConnection(dbName))
					{
						AssertEquals(sRDbName, DbRegistry.SingleRefDatabaseName.LoadValue(connection));

						using (var cmd = connection.Command($"CREATE DATABASE [{sRDbName}]"))
						{
							cmd.ExecuteNonQuery();
						}
						AssertDatabaseExists(true, sRDbName);
					}
				}
				finally
				{
					var mockCw1SearchResult = new Mock<ICargoWiseOneInstanceSearchResult>();
					var mockCw1Entry = new Mock<ICargoWiseOneInstanceEntry>();
					mockCw1InstanceClass.Setup(c => c.FindInstance(shelfName, It.Is<DirectorySearchOptions>(o => o.DomainName == "sand.wtg.zone"))).Returns(mockCw1SearchResult.Object);
					mockCw1SearchResult.Setup(r => r.GetDirectoryEntry()).Returns(mockCw1Entry.Object);

					var logger = new TestLogger();
					CreateDeployer(logger, mockCw1InstanceClass.Object).TeardownTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));

					AssertDatabaseExists(false, dbName);
					AssertDatabaseExists(false, sRDbName);
				}
			}
		}

		public void TestAutoDeployTestedShelfWithRewindTransformVersionNumber()
		{
			var rewindTransformVersionNumber = 15;
			var shelfName = Guid.NewGuid().ToString("N").Substring(0, 20);
			var dbName = "SH0" + shelfName;
			var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");

			var taskComments = $@"AYY 27-Feb-25 12:06:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigCreateIcon: false
TestRigRegister: false
TestRigLaunchDbUpgrade: false";

			using var sourcePathDirectory = new TempDirectory();
			using var binPathDirectory = new TempDirectory();
			using var unpackDirectory = new TempDirectory();

			var logger = new TestLogger();
			var buildDeployer = CreateDeployer(logger);

			try
			{
				SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "25.2.1.69",
							   new DateTime(2025, 2, 1, 11, 6, 14), "ALP");

				buildDeployer.AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory,
													new TaskInfo(shelfName, "whoever", taskComments));
				using var connection = Db.NewAdminConnection(dbName);

				var registryValue = GetRegistryValue(connection, DbRegistry.DatabaseMajorTransformationVersion.ItemName)?.ToString();
				if (!int.TryParse(registryValue, out int oldMajorTransformationVersion))
				{
					throw new Exception("Missing or invalid registry value for the config DatabaseMajorTransformationVersion");
				}

				var updatedComments = taskComments + $"\nTestRigRewindTransformVersionNumber: {rewindTransformVersionNumber}";
				var options = new TestedShelfDeploymentOptions(new TaskInfo(shelfName, "whoever", updatedComments));

				buildDeployer.SetMajorTransformationVersion(options);
				AssertRegistryEntryInteger(connection, DbRegistry.DatabaseMajorTransformationVersion.ItemName,
										   oldMajorTransformationVersion - rewindTransformVersionNumber);
			}
			finally
			{
				buildDeployer.TeardownTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory,
												  new TaskInfo(shelfName, "whoever", taskComments));

				AssertDatabaseExists(expectedValue: false, dbName);
			}
		}

		public void TestAutoDeployTestedShelfWithAddStaffRecords()
		{
			CombineAssertions(() =>
			{
				TestCaseForAddStaffRecords(true);
				TestCaseForAddStaffRecords(false);
			});

			void TestCaseForAddStaffRecords(bool deployWinzor)
			{
				var shelfName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
				var dbName = "SH0" + shelfName;
				var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");
				var taskComments = $@"SK7 21-Aug-23 10:03:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigRegister: false
TestRigLaunchDbUpgrade: false
TestRigVersionBrokerWarmupTimeout: 00:00:05";

				var mockAd = MockActiveDirecotry();
				var buildDepolyor = CreateDeployer(new TestLogger(), mockAd.CW1InstanceClass, mockAd.DirectorySearcher);

				using (var sourcePathDirectory = new TempDirectory())
				using (var binPathDirectory = new TempDirectory())
				{
					try
					{
						SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");

						buildDepolyor.AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));

						AssertDatabaseExists(true, dbName);

						using (var connection = Db.NewAdminConnection(dbName))
						{
							PrepareTablesForAddStaffRecords(connection);

							var newTaskComments = $@"SK7 21-Aug-23 10:03:
TestRigSqlServer: {Db.ServerName}
TestRigRegister: false
TestRigLaunchDbUpgrade: false
TestRigDeployWinzor: {deployWinzor}
TestRigAddStaffRecords: Hunter.Yang, David.James
TestRigVersionBrokerWarmupTimeout: 00:00:05";

							buildDepolyor.AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", newTaskComments));

							Assert("StmFeatureTest table has 1 record", (int)connection.Command("SELECT COUNT(*) FROM dbo.StmFeatureTest").ExecuteScalar() == 1);

							AssertEquals("GlbStaff table is populated with user login data", 3, (int)connection.Command("SELECT COUNT(*) FROM dbo.GlbStaff").ExecuteScalar());
							AssertUserPopulated(connection, "Duplicate.Test");
							AssertUserPopulated(connection, "Hunter.Yang");
							AssertUserPopulated(connection, "David.James");
						}
					}
					finally
					{
						var logger = new TestLogger();
						CreateDeployer(logger).TeardownTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
						AssertDatabaseExists(false, dbName);
					}
				}
			}
		}

		public void TestAutoDeployTestedShelfWithoutAddStaffRecordsForWinzor()
		{
			var shelfName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var dbName = "SH0" + shelfName;
			var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");
			var taskComments = $@"SK7 21-Aug-23 10:03:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigRegister: false
TestRigLaunchDbUpgrade: false
TestRigVersionBrokerWarmupTimeout: 00:00:05";

			var mockAd = MockActiveDirecotry();
			var buildDepolyor = CreateDeployer(new TestLogger(), mockAd.CW1InstanceClass, mockAd.DirectorySearcher);

			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			{
				try
				{
					SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");

					buildDepolyor.AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));

					AssertDatabaseExists(true, dbName);

					using (var connection = Db.NewAdminConnection(dbName))
					{
						PrepareTablesForAddStaffRecords(connection);

						var newTaskComments = $@"SK7 21-Aug-23 10:03:
TestRigSqlServer: {Db.ServerName}
TestRigRegister: false
TestRigLaunchDbUpgrade: false
TestRigDeployWinzor: true
TestRigVersionBrokerWarmupTimeout: 00:00:05";

						buildDepolyor.AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", newTaskComments));

						Assert("StmFeatureTest table has 1 record", (int)connection.Command("SELECT COUNT(*) FROM dbo.StmFeatureTest").ExecuteScalar() == 1);

						AssertEquals("GlbStaff table is populated with user login data", 5, (int)connection.Command("SELECT COUNT(*) FROM dbo.GlbStaff").ExecuteScalar());
						AssertUserPopulated(connection, "Andy.Li");
						AssertUserPopulated(connection, "Sam.Khanjar");
						AssertUserPopulated(connection, "David.James");
						AssertUserPopulated(connection, "Duplicate.Test");
						AssertUserPopulated(connection, "Duplicate.Test2");
						AssertRegistryEntry(connection, "OIDCConfig", "<?xml version=\"1.0\" encoding=\"utf-16\"?><OIDCConfig><Version>1</Version><IsOIDCEnabled>Y</IsOIDCEnabled><OIDCServerTypeCode>AZU</OIDCServerTypeCode><AuthorityURL>https://loginsimulator.wisetechglobal.com/</AuthorityURL><ClientIdentifier>dummyIdP</ClientIdentifier><ArrayOfOIDCClaimsMapping><OIDCClaimsMapping><Version>1</Version><ClaimName>user_name</ClaimName><Identifier>GlbStaff.GS_LoginName</Identifier></OIDCClaimsMapping></ArrayOfOIDCClaimsMapping><ArrayOfOIDCScope><OIDCScope><Version>1</Version><ScopeName>dummyIdP</ScopeName></OIDCScope></ArrayOfOIDCScope></OIDCConfig>");
						AssertRegistryEntryBool(connection, "IsOIDCFederatedWithWTG", false);
					}
				}
				finally
				{
					var logger = new TestLogger();
					CreateDeployer(logger).TeardownTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
					AssertDatabaseExists(false, dbName);
				}
			}
		}

		public void TestAutoDeployTestedShelfWithoutAddStaffRecordsForRdp()
		{
			var shelfName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var dbName = "SH0" + shelfName;
			var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");
			var taskComments = $@"SK7 21-Aug-23 10:03:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigRegister: false
TestRigLaunchDbUpgrade: false
TestRigVersionBrokerWarmupTimeout: 00:00:05";

			var mockAd = MockActiveDirecotry();
			var buildDepolyor = CreateDeployer(new TestLogger(), mockAd.CW1InstanceClass, mockAd.DirectorySearcher);

			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			{
				try
				{
					SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");

					buildDepolyor.AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));

					AssertDatabaseExists(true, dbName);

					using (var connection = Db.NewAdminConnection(dbName))
					{
						PrepareTablesForAddStaffRecords(connection);

						var newTaskComments = $@"SK7 21-Aug-23 10:03:
TestRigSqlServer: {Db.ServerName}
TestRigRegister: false
TestRigLaunchDbUpgrade: false
TestRigVersionBrokerWarmupTimeout: 00:00:05";

						buildDepolyor.AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", newTaskComments));

						Assert("StmFeatureTest table has 0 record", (int)connection.Command("SELECT COUNT(*) FROM dbo.StmFeatureTest").ExecuteScalar() == 0);

						AssertEquals("GlbStaff table is populated with user login data", 1, (int)connection.Command("SELECT COUNT(*) FROM dbo.GlbStaff").ExecuteScalar());
						AssertUserPopulated(connection, "Duplicate.Test");
					}
				}
				finally
				{
					var logger = new TestLogger();
					CreateDeployer(logger).TeardownTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
					AssertDatabaseExists(false, dbName);
				}
			}
		}

		(ICargoWiseOneInstanceClass CW1InstanceClass, ICargoWiseOneInstanceEntry CW1InstanceEntry, IDirectorySearcher DirectorySearcher) MockActiveDirecotry()
		{
			var mockCw1InstanceClass = new Mock<ICargoWiseOneInstanceClass>();
			var mockCw1InstanceEntry = new Mock<ICargoWiseOneInstanceEntry>();
			mockCw1InstanceClass.Setup(c => c.AddNewInstance(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<System.DirectoryServices.DirectoryEntry>()))
								.Returns(mockCw1InstanceEntry.Object);

			var mockGroupDirectoryEntry = new Mock<IGroupDirectoryEntry>();
			mockGroupDirectoryEntry.Setup(c => c.GetMembers())
									.Returns(new List<IDirectoryEntry>() {
										MockUser("Andy.Li"),
										MockUser("WTG.Andy.Li"),
										MockUser("CN=WTG.Andy.Li"),
										MockUser("Sam.Khanjar"),
										MockUser("WTG.Sam.Khanjar"),
										MockUser("CN=WTG.David.James"),
										MockUser("CN=WTG.Duplicate.Test"),
										MockUser("WTG.Duplicate.Test2"),
										MockUser("WTG.Duplicate.Test2"),
										MockUser("Duplicate.Test2"),
										MockUser("Duplicate.Test2")
									});

			var mockDirectorySearch = new Mock<IDirectorySearcher>();
			mockDirectorySearch.Setup(e => e.FindGroup("g_WTG_Token_Based_Authentication", It.IsAny<string>())).Returns(mockGroupDirectoryEntry.Object);
			return (mockCw1InstanceClass.Object, mockCw1InstanceEntry.Object, mockDirectorySearch.Object);

			IDirectoryEntry MockUser(string userName)
			{
				var mockDirectoryEntry = new Mock<IDirectoryEntry>();
				mockDirectoryEntry.Setup(e => e.Name).Returns(userName);
				return mockDirectoryEntry.Object;
			}
		}

		void PrepareTablesForAddStaffRecords(AdminConnection connection)
		{
			connection.Command(@"
CREATE TABLE dbo.GlbPerson (
PER_PK UNIQUEIDENTIFIER NOT NULL,
PER_IsActive BIT,
PER_FullName NVARCHAR(256),
PER_HomeAddress1 NVARCHAR(50),
PER_RN_NKCountry VARCHAR(2))

CREATE TABLE dbo.GlbStaff (
GS_PK UNIQUEIDENTIFIER NOT NULL,
GS_LoginName NVARCHAR(104),
GS_FullName NVARCHAR(256),
GS_UserAddress1 NVARCHAR(50),
GS_RN_NKCountryCode VARCHAR(2),
GS_Code NVARCHAR(3),
GS_IsActive BIT,
GS_GB_HomeBranch UNIQUEIDENTIFIER NULL,
GS_GE_HomeDepartment UNIQUEIDENTIFIER NULL,
GS_IsController BIT,
GS_PER UNIQUEIDENTIFIER NULL)

CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__GS_LoginName] ON [dbo].[GlbStaff]
(
[GS_LoginName] ASC
)

CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__GS_Code] ON [dbo].[GlbStaff]
(
[GS_Code] ASC
)

CREATE TABLE dbo.GlbBranch (
GB_PK UNIQUEIDENTIFIER NOT NULL,
GB_IsActive BIT,
GB_Code CHAR(3),
GB_BranchName NVARCHAR(50))

CREATE TABLE dbo.GlbDepartment (
GE_PK UNIQUEIDENTIFIER NOT NULL,
GE_IsActive BIT,
GE_Code CHAR(3),
GE_Desc VARCHAR(35))

CREATE TABLE dbo.GlbGroup (
GG_PK UNIQUEIDENTIFIER NOT NULL,
GG_IsValid BIT,
GG_IsActive BIT,
GG_Code VARCHAR(15),
GG_Desc NVARCHAR(64),
GG_IsSystemDefined BIT,
GG_IsSales BIT,
GG_IsSecurityEnabled BIT
)

CREATE TABLE dbo.GlbGroupLink (
GK_PK UNIQUEIDENTIFIER NOT NULL,
GK_IsValid BIT,
GK_MembershipType VARCHAR(3),
GK_GG UNIQUEIDENTIFIER NOT NULL,
GK_GS UNIQUEIDENTIFIER NOT NULL
)

CREATE TABLE dbo.StmFeatureTest (
SFT_PK UNIQUEIDENTIFIER NOT NULL,
SFT_IsActive BIT,
SFT_FeatureName VARCHAR(255),
SFT_GG_Group UNIQUEIDENTIFIER NOT NULL,
SFT_SystemLastEditTimeUtc SMALLDATETIME,
SFT_SystemCreateUser VARCHAR(3),
SFT_SystemLastEditUser VARCHAR(3),
SFT_SystemCreateTimeUtc SMALLDATETIME
)

CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__SFT_FeatureName] ON [dbo].[StmFeatureTest]
(
[SFT_FeatureName] ASC
)

INSERT INTO dbo.GlbGroup
(GG_PK, GG_IsValid, GG_IsActive, GG_Code, GG_Desc, GG_IsSystemDefined, GG_IsSales, GG_IsSecurityEnabled)
VALUES
(NEWID(), 0, 1, 'ALL', 'ALL STAFF', 1, 0, 1)

INSERT INTO dbo.GlbBranch
(GB_PK, GB_IsActive, GB_Code, GB_BranchName)
VALUES
(NEWID(), 1, 'SYD', 'EDIHQ')

INSERT INTO dbo.GlbDepartment
(GE_PK, GE_IsActive, GE_Code, GE_Desc)
VALUES
(NEWID(), 1, 'TE', 'Test')

DECLARE @gsPER uniqueidentifier = NEWID()
DECLARE @department uniqueidentifier = (SELECT GB_PK FROM dbo.GlbBranch WHERE GB_Code = 'SYD')
DECLARE @branch uniqueidentifier = (SELECT GE_PK FROM dbo.GlbDepartment WHERE GE_Code = 'TE')

INSERT INTO dbo.GlbPerson 
(PER_PK, PER_FullName, PER_IsActive, PER_HomeAddress1, PER_RN_NKCountry)
VALUES
(@gsPER, 'Am I Duplicated?', 1, '1 street road', 'AU');

INSERT INTO dbo.GlbStaff
(GS_PK, GS_LoginName, GS_FullName, GS_UserAddress1, GS_RN_NKCountryCode, GS_Code, GS_IsActive, GS_IsController, GS_GB_HomeBranch, GS_GE_HomeDepartment, GS_PER)
VALUES
(NEWID(), 'Duplicate.Test', 'Am I Duplicated?', '1 street road', 'AU', 'DUP', 1, 0, @branch, @department, @gsPER)
					").ExecuteNonQuery();

			Assert("GlbStaff table exists", connection.Exists("FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'GlbStaff'"));
			Assert("GlbPerson table exists", connection.Exists("FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'GlbPerson'"));
			Assert("GlbGroup table exists", connection.Exists("FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'GlbGroup'"));
			Assert("GlbGroupLink table exists", connection.Exists("FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'GlbGroupLink'"));
			Assert("StmFeatureTest table exists", connection.Exists("FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'StmFeatureTest'"));
			Assert("GlbStaff table has 1 record", (int)connection.Command("SELECT COUNT(*) FROM dbo.GlbStaff").ExecuteScalar() == 1);
			Assert("GlbPerson table has 1 record", (int)connection.Command("SELECT COUNT(*) FROM dbo.GlbPerson").ExecuteScalar() == 1);
			Assert("GlbGroup table has 1 record", (int)connection.Command("SELECT COUNT(*) FROM dbo.GlbGroup").ExecuteScalar() == 1);
		}

		void AssertUserPopulated(AdminConnection cn, string userName)
		{
			Assert($"GlbStaff table is populated with user {userName}", (int)cn.Command($"SELECT COUNT(*) FROM dbo.GlbStaff WHERE GS_LoginName LIKE '{userName}'").ExecuteScalar() == 1);
		}

		public void TestAutoDeployTestedShelfWithDbRestoreRunsResetProcessControllerHosts()
		{
			// Arrange
			var scheduleDescription = Guid.NewGuid().ToString("N");
			var shelfName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var dbName = "SH0" + shelfName;
			var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");
			var taskComments = $@"BRE 11-Jan-16 16:33:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigCreateIcon: false
TestRigRegister: false
TestRigLaunchDbUpgrade: false";

			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			{
				try
				{
					SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");
					var taskLogger = new TestLogger(
						line =>
						{
							if (line.Equals("start Resetting Process Controller host information", StringComparison.OrdinalIgnoreCase))
							{
								using (var connection = Db.NewAdminConnection(dbName))
								using (var command = connection.Command(Invariant($@"IF OBJECT_ID('[dbo].[StmServiceHost]') IS NULL
BEGIN
	CREATE TABLE [dbo].[StmServiceHost]
	(
		[SH_PK] [uniqueidentifier] NOT NULL,
		[SH_IsActive] [bit] NOT NULL,
		[SH_HostName] [nvarchar](255) NOT NULL,
		[SH_SystemCreateTimeUtc] [smalldatetime] NULL,
		[SH_SystemCreateUser] [varchar](3) NOT NULL,
		[SH_SystemLastEditTimeUtc] [smalldatetime] NULL,
		[SH_SystemLastEditUser] [varchar](3) NOT NULL
	)
	ON [PRIMARY];
END
INSERT INTO [dbo].[StmServiceHost] ([SH_PK], [SH_IsActive], [SH_HostName], [SH_SystemCreateTimeUtc], [SH_SystemCreateUser], [SH_SystemLastEditTimeUtc], [SH_SystemLastEditUser]) VALUES
	(NEWID(), 1, 'dat.test.host1', GETUTCDATE(), 'US1', GETUTCDATE(), 'US1'),
	(NEWID(), 1, 'dat.test.host2', GETUTCDATE(), 'US1', GETUTCDATE(), 'US1');")))
								{
									command.ExecuteNonQuery();
								}
							}
						});

					var buildDeployer = CreateDeployer(taskLogger);

					// Act
					buildDeployer.AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
					var log = taskLogger.ToString();

					Assert($"[{log}] should contain [start Resetting Process Controller host information]", log.Contains("start Resetting Process Controller host information"));

					// Assert
					AssertDatabaseExists(true, dbName);
					var recordsStmServiceHost = GetRecords(() =>
						Db.NewAdminConnection(dbName), "[dbo].[StmServiceHost]")
							.ToArray();

					AssertEquals("StmServiceHost should be empty", 0, recordsStmServiceHost.Length);
				}
				finally
				{
					var logger = new TestLogger();
					CreateDeployer(logger).TeardownTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
					AssertDatabaseExists(false, dbName);
				}
			}
		}

		public void TestAutoDeployTestedShelfWithoutDbRestoreDoesNotRunResetProcessControllerHosts()
		{
			// Arrange
			var shelfName = Guid.NewGuid().ToString();
			var dbName = "6BCF78F7";
			var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");
			var taskComments = $@"
TestRigSqlServer: {Db.ServerName}
TestRigDatabaseName: {dbName}
TestRigCreateIcon: false
TestRigRegister: false
TestRigLaunchDbUpgrade: false";

			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			{
				try
				{
					SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");

					using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
					{
						using (var cmd = connection.Command($"CREATE DATABASE [{dbName}]"))
						{
							cmd.ExecuteNonQuery();
						}

						using (((ICurrentDbControl)connection).UseDatabase(dbName))
						using (var cmd = connection.Command(DataUtils.SQL_InitialTablesForEmptyDatabase()))
						{
							cmd.ExecuteNonQuery();
						}
					}

					var taskLogger = new TestLogger(
						line =>
						{
							if (line.Equals("start Uploading package", StringComparison.OrdinalIgnoreCase))
							{
								using (var connection = Db.NewAdminConnection(dbName))
								using (var command = connection.Command(Invariant($@"
									IF OBJECT_ID('[dbo].[StmServiceHost]') IS NULL
										BEGIN
											CREATE TABLE [dbo].[StmServiceHost]
											(
												[SH_PK] [uniqueidentifier] NOT NULL,
												[SH_IsActive] [bit] NOT NULL,
												[SH_HostName] [nvarchar](255) NOT NULL,
												[SH_ProxyAutoDetect] [bit] NOT NULL,
												[SH_ProxyHost] [nvarchar](255) NOT NULL,
												[SH_ProxyPort] [int] NOT NULL,
												[SH_ProxyUserName] [nvarchar](256) NOT NULL,
												[SH_ProxyPassword] [nvarchar](127) NOT NULL
											)
											ON [PRIMARY];
										END

									INSERT INTO [dbo].[StmServiceHost] ([SH_PK], [SH_IsActive], [SH_HostName], [SH_ProxyAutoDetect], [SH_ProxyHost], [SH_ProxyPort], [SH_ProxyUserName], [SH_ProxyPassword]) VALUES
										(NEWID(), 1, 'dat.test.host1', 1, '', 0, '', ''),
										(NEWID(), 1, 'dat.test.host2', 1, '', 0, '', '');")))
								{
									command.ExecuteNonQuery();
								}
							}
						});

					// Act
					CreateDeployer(taskLogger).AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
					var log = taskLogger.ToString();

					// Assert
					AssertDatabaseExists(true, dbName);
					Assert($"[{log}] should NOT contain [start Resetting Process Controller host information]", !log.Contains("start Resetting Process Controller host information"));

					var recordsStmServiceHost = GetRecords(() =>
						Db.NewAdminConnection(dbName), "[dbo].[StmServiceHost]")
							.Where(x => x["SH_HostName"].ToString().Equals("dat.test.host1") || x["SH_HostName"].ToString().Equals("dat.test.host2"))
							.ToArray();

					AssertEquals("StmServiceHost should still contain pre-deployment records", 2, recordsStmServiceHost.Length);
				}
				finally
				{
					var logger = new TestLogger();
					CreateDeployer(logger).TeardownTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));

					AssertDatabaseExists(true, dbName);

					using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
					using (var cmd = connection.Command($"DROP DATABASE [{dbName}]"))
					{
						cmd.ExecuteNonQuery();
					}

					AssertDatabaseExists(false, dbName);
				}
			}
		}

		public void TestAutoDeployTestedShelfWithUpgradeChoosesRandomUpgradeServer()
		{
			// Arrange
			var expectedDbUpgradeServerLogMessages = new List<string>
			{
				"start Launching DbUpgrade on server AU2SP-TUPG-401.sand.wtg.zone",
				"start Launching DbUpgrade on server AU2SP-TUPG-402.sand.wtg.zone"
			};

			var actualDbUpgradeServerLogMessages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			// Act
			for (int i = 0; i < 10; i++)
			{
				actualDbUpgradeServerLogMessages.Add(RunDeployment("start Launching DbUpgrade on server").deploymentLog);
			}

			// Assert
			AssertContainsExactElementsInAnyOrder("Should invoke each upgrade server at least once.", expectedDbUpgradeServerLogMessages, actualDbUpgradeServerLogMessages);
		}

		public void TestAutoDeployTestedShelfWithUpgradeUsesOptionFromTaskComments()
		{
			// Arrange
			var results = new List<string>();

			// Act
			for (int i = 0; i < 5; i++)
			{
				results.Add(RunDeployment("start Launching DbUpgrade on server", "AU2SP-TUPG-401.sand.wtg.zone").deploymentLog);
			}

			// Assert
			CombineAssertions(() =>
			{
				AssertCollectionContains("start Launching DbUpgrade on server AU2SP-TUPG-401.sand.wtg.zone", results);
				AssertCollectionNotContains("start Launching DbUpgrade on server AU2SP-TUPG-402.sand.wtg.zone", results);
			});
		}

		public void TestAutoDeployTestedShelfWithUpgradeComposesCorrectLogFileUrl_Server1() => AssertAutoDeployTestedShelfWithUpgradeComposesCorrectLogFileUrl("AU2SP-TUPG-401.sand.wtg.zone");
		public void TestAutoDeployTestedShelfWithUpgradeComposesCorrectLogFileUrl_Server2() => AssertAutoDeployTestedShelfWithUpgradeComposesCorrectLogFileUrl("AU2SP-TUPG-402.sand.wtg.zone");

		void AssertAutoDeployTestedShelfWithUpgradeComposesCorrectLogFileUrl(string serverName)
		{
			// Arrange
			var expectedServerName = serverName.ToLower().Split('.')[0];

			// Act
			var (deploymentLog, dbName) = RunDeployment("The upgrade process will log to UPG service task log file at", $"{expectedServerName}");

			// Assert
			CombineAssertions(() =>
			{
				AssertContains($"query:'host.hostname:{expectedServerName}", deploymentLog);
				AssertContains($"log.file.path:*{dbName}*", deploymentLog);
			});
		}

		(string deploymentLog, string dbName) RunDeployment(string logEntryToTarget, string withDefaultUpgradeServer = "")
		{
			// Arrange
			var result = string.Empty;
			var shelfName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var dbName = "SH0" + shelfName;
			var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");
			var taskComments = $@"BRE 11-Jan-16 16:33:
	TestRigRestoreFromBackup: {backupPath}
	TestRigSqlServer: {Db.ServerName}
	TestRigSqlServerDataFilePath: {Temp.TempPath}
	TestRigSqlServerLogFilePath: {Temp.TempPath}
	TestRigCreateIcon: false
	TestRigRegister: false
	TestRigLaunchDbUpgrade: true";

			if (!string.IsNullOrWhiteSpace(withDefaultUpgradeServer))
			{
				taskComments += $@"
	TestRigDefaultDbUpgradeServer: {withDefaultUpgradeServer}";
			}

			using var sourcePathDirectory = new TempDirectory();
			using var binPathDirectory = new TempDirectory();

			try
			{
				SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");
				var taskLogger = new TestLogger(
					line =>
					{
						if (line.Contains(logEntryToTarget, StringComparison.OrdinalIgnoreCase))
						{
							result = line;
							throw new DeploymentCancelledException("Force cancel to avoid invoking upgrade server.");
						}
					});

				var buildDeployer = CreateDeployer(taskLogger);

				try
				{
					buildDeployer.AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
				}
				catch (DeploymentCancelledException e) when (e.Message.Equals("Force cancel to avoid invoking upgrade server."))
				{ }
			}
			finally
			{
				var tearDownLogger = new TestLogger();
				CreateDeployer(tearDownLogger).TeardownTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
				AssertDatabaseExists(false, dbName);
			}

			return (result, dbName);
		}

		public void TestAutoDeployTestedShelfTearDownOldEnvBeforeDeployment()
		{
			// Arrange
			var shelfName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var dbName = "SH0" + shelfName;

			var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");
			var taskComments = $@"JH6 11-Jan-16 16:33:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigRegister: true
TestRigLaunchDbUpgrade: false";
			mockProductKeyService
				.Setup(x => x.GetInternalTestKey(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
				.Returns("ABCDEFG");
			mockProductRegistration
				.Setup(x => x.Register(It.IsAny<string>(), It.IsAny<CancellationToken>(), BuildDeployer.DefaultRegistrationTimeoutMs))
				.Returns(ProductRegistrationRegisterResult.OK);
			var mockCw1InstanceClass = new Mock<ICargoWiseOneInstanceClass>();
			mockCw1InstanceClass
				.Setup(c => c.AddNewInstance(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<System.DirectoryServices.DirectoryEntry>()))
				.Returns(new Mock<ICargoWiseOneInstanceEntry>().Object);
			var taskInfo = new TaskInfo(shelfName, "whoever", taskComments);
			var loggerMock = new Mock<ITaskLogger>();
			var buildDeployer = CreateDeployer(loggerMock.Object, mockCw1InstanceClass.Object, Mock.Of<IDirectorySearcher>());

			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			using (new DisposableAction(() => buildDeployer.TeardownTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, taskInfo)))
			{
				SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");

				buildDeployer.AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, taskInfo);
				loggerMock.Reset();

				// Act
				buildDeployer.AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, taskInfo);

				// Assert
				loggerMock.Verify(x => x.RecordTask("AutoDeployTestedShelf starts to clean up old env."), Times.Once);
				loggerMock.Verify(x => x.RecordTask("Unregistering product"), Times.Once);
				loggerMock.Verify(x => x.RecordTask("Delete Icon"), Times.Once);
				loggerMock.Verify(x => x.RecordTask("Dropping Databases"), Times.Once);
				loggerMock.Verify(x => x.RecordTask("Global cleanup"), Times.Once);
			}
		}

		public void TestTestedShelfTearDownBiDatabases()
		{
			var shelfName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var dbName = "SH0" + shelfName;

			var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");
			var taskComments = $@"JCI 07-Dec-21 16:33:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigRegister: false
TestRigLaunchDbUpgrade: false";
			var logger = new TestLogger();

			var mockCw1InstanceClass = new Mock<ICargoWiseOneInstanceClass>();
			mockCw1InstanceClass.Setup(c => c.AddNewInstance(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<System.DirectoryServices.DirectoryEntry>())).Returns(new Mock<ICargoWiseOneInstanceEntry>().Object);
			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			using (var mainDbConnection = Db.NewAdminConnection(Db.SqlMasterDb))
			{
				try
				{
					SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");
					CreateDeployer(logger, mockCw1InstanceClass.Object).AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));

					mainDbConnection.CreateDatabase(dbName + Db.AuditDatabaseSuffix);
					mainDbConnection.CreateDatabase(dbName + Db.EdwDatabaseSuffix);

					using (((ICurrentDbControl)mainDbConnection).UseDatabase(dbName))
					{
						DbRegistry.BiAuditServer.SaveValue(System.Environment.MachineName, mainDbConnection);
						DbRegistry.BiDataWarehouseServer.SaveValue(System.Environment.MachineName, mainDbConnection);

						AssertEquals(DbRegistry.BiAuditServer.LoadValue(mainDbConnection), System.Environment.MachineName);
						AssertEquals(DbRegistry.BiDataWarehouseServer.LoadValue(mainDbConnection), System.Environment.MachineName);
					}

					AssertDatabaseExists(true, dbName + Db.AuditDatabaseSuffix);
					AssertDatabaseExists(true, dbName + Db.EdwDatabaseSuffix);
					AssertDatabaseLoginExists($"{dbName}_%");

					var options = new TestedShelfDeploymentOptions(new TaskInfo(shelfName, "whoever", taskComments));
					BuildDeployer.DropBiDatabasesOnRemoteServer(options, mainDbConnection, logger);
					AssertDatabaseExists(false, dbName + Db.AuditDatabaseSuffix);
					AssertDatabaseExists(false, dbName + Db.EdwDatabaseSuffix);
					AssertDatabaseLoginDoesNotExist($"{dbName}_%");
				}
				finally
				{
					CreateDeployer(logger).TeardownTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));
				}
			}
		}

		#region Cache Related Tests

		public void TestRestoreTestDatabaseUsesLocalCache()
		{
			var shelfName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var dbName = "SH0" + shelfName;
			var localCachePath = CopyTestFileResource("OdysseyNoDescription.bak");

			using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
			{
				string sid = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
				using (var command = connection.Command(Invariant($"CREATE LOGIN [EnterpriseDbUser_{dbName}_{sid}] WITH PASSWORD = '', CHECK_POLICY = OFF, DEFAULT_LANGUAGE = us_english, SID = 0x{sid}")))
				{
					command.ExecuteNonQuery();
				}
			}

			var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");
			var taskComments = $@"BRE 11-Jan-16 16:33:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigRegister: false
TestRigLaunchDbUpgrade: false";

			var logger = new TestLogger();

			var mockCw1InstanceClass = new Mock<ICargoWiseOneInstanceClass>();
			mockCw1InstanceClass.Setup(c => c.AddNewInstance(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<System.DirectoryServices.DirectoryEntry>())).Returns(new Mock<ICargoWiseOneInstanceEntry>().Object);

			var cacheDirectory = Path.Combine(TempForTest.TempPath, "CachePath");
			if (!Directory.Exists(cacheDirectory))
			{
				_ = Directory.CreateDirectory(cacheDirectory);
			}
			var cacheHelperMock = new Mock<CacheHelper>(System.Environment.MachineName, cacheDirectory.Replace(":", "$"), logger) { CallBase = true };

			var buildDeployerMock = new Mock<BuildDeployer>(logger, mockCw1InstanceClass.Object, null, CreateDummyNudgeClient()) { CallBase = true };
			buildDeployerMock.Protected()
				.Setup<CacheHelper>("GetCacheHelper", ItExpr.IsAny<string>())
				.Returns(cacheHelperMock.Object);

			var buildDeployer = buildDeployerMock.Object;

			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			{
				try
				{
					SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");

					buildDeployer.AutoDeployTestedShelf("DEBUG",
						string.Empty,
						sourcePathDirectory,
						binPathDirectory,
						new TaskInfo(shelfName,
							"whoever",
							taskComments));
					cacheHelperMock.Verify(c => c.GetFile(It.IsAny<string>(), It.IsNotIn(default(TimeSpan))));
					cacheHelperMock.Verify(c => c.GetFile(It.IsAny<string>(), It.IsAny<TimeSpan>()), Times.Once);
					buildDeployerMock.Protected()
						.Verify<bool>("VerifyBackup", Times.Once(), ItExpr.IsAny<string>(), ItExpr.IsAny<string>());
				}
				finally
				{
					buildDeployer.TeardownTestedShelf("DEBUG",
						string.Empty,
						sourcePathDirectory,
						binPathDirectory,
						new TaskInfo(shelfName,
							"whoever",
							taskComments));

					if (Directory.Exists(cacheDirectory))
					{
						Directory.Delete(cacheDirectory, recursive: true);
					}
				}
			}
		}

		public void TestRestoreTestDatabaseRunsSuccessfullyWhenLocalCacheFails()
		{
			var shelfName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var dbName = "SH0" + shelfName;

			var backupPath = CopyTestFileResource("OdysseyNoDescription.bak");
			var taskComments = $@"BRE 11-Jan-16 16:33:
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigSqlServerDataFilePath: {Temp.TempPath}
TestRigSqlServerLogFilePath: {Temp.TempPath}
TestRigRegister: false
TestRigLaunchDbUpgrade: false";

			var logger = new TestLogger();

			var mockCw1InstanceClass = new Mock<ICargoWiseOneInstanceClass>();
			mockCw1InstanceClass.Setup(c => c.AddNewInstance(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<System.DirectoryServices.DirectoryEntry>())).Returns(new Mock<ICargoWiseOneInstanceEntry>().Object);

			var cacheHelperMock = new Mock<CacheHelper>(System.Environment.MachineName, "TestDir", logger) { CallBase = true };
			cacheHelperMock
				.Setup(c => c.GetFile(It.IsAny<string>(), It.IsAny<TimeSpan>()))
				.Returns((string)null);

			var buildDeployerMock = new Mock<BuildDeployer>(logger, mockCw1InstanceClass.Object, null, CreateDummyNudgeClient()) { CallBase = true };
			buildDeployerMock.Protected()
				.Setup<CacheHelper>("GetCacheHelper", ItExpr.IsAny<string>())
				.Returns(cacheHelperMock.Object);

			var buildDeployer = buildDeployerMock.Object;

			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			{
				try
				{
					SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");

					buildDeployer.AutoDeployTestedShelf("DEBUG",
						string.Empty,
						sourcePathDirectory,
						binPathDirectory,
						new TaskInfo(shelfName,
							"whoever",
							taskComments));

					var log = logger.ToString();

					AssertDatabaseExists(true, dbName);
					AssertDeploymentIDLogged(log, dbName);
					AssertDatabaseLoginExists($"{dbName}_%");
					AssertDatabaseLoginDoesNotExist($"EnterpriseDbUser_{dbName}_%", "Staff Login should not be created by Auto Deployment.");

					using (var connection = Db.NewAdminConnection(dbName))
					{
						var sqlConnection = ((IDbConnectionInternals)connection).ADOConnection;
						var upgradeManager = new SqlUpgradeManager(new UpgradeSqlContext(sqlConnection, sqlConnection.DataSource, sqlConnection.Database));

						AssertEquals(log, new Version(16, 4, 1, 69), upgradeManager.QueryCurrentVersion().Version);
					}

					mockCw1InstanceClass.Verify(c => c.AddNewInstance(shelfName, Db.ServerName, dbName, It.Is<System.DirectoryServices.DirectoryEntry>(e => e.Path == "LDAP://CN=SH0,CN=WiseTech Global,CN=Program Data,DC=sand,DC=wtg,DC=zone")));

					cacheHelperMock.Verify(c => c.GetFile(It.IsAny<string>(), It.IsAny<TimeSpan>()), Times.Once);
				}
				finally
				{
					var mockCw1SearchResult = new Mock<ICargoWiseOneInstanceSearchResult>();
					var mockCw1Entry = new Mock<ICargoWiseOneInstanceEntry>();
					mockCw1InstanceClass.Setup(c => c.FindInstance(shelfName, It.Is<DirectorySearchOptions>(o => o.DomainName == "sand.wtg.zone"))).Returns(mockCw1SearchResult.Object);
					mockCw1SearchResult.Setup(r => r.GetDirectoryEntry()).Returns(mockCw1Entry.Object);

					CreateDeployer(logger, mockCw1InstanceClass.Object).TeardownTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));

					AssertDatabaseExists(false, dbName);
					AssertDatabaseLoginDoesNotExist($"{dbName}_%");
					AssertDatabaseLoginDoesNotExist($"EnterpriseDbUser_{dbName}_%");
					mockCw1Entry.Verify(e => e.Delete(), Times.Once());
				}
			}
		}

		public void TestVerifyBackupShouldReturnFalseWhenFileIsNotRestorable()
		{
			var serverName = System.Environment.MachineName;
			var localCacheFilePath = CreateDummyLocalCacheFile();
			var mockCachePath = GetMockCachePath();
			var logger = new TestLogger();
			var mockCw1InstanceClass = new Mock<ICargoWiseOneInstanceClass>();
			var buildDeployer = new BuildDeployerForTest(logger, mockCw1InstanceClass.Object, localDatabaseCachePath: mockCachePath);

			var verifyResult = buildDeployer.VerifyBackupExposed(serverName, localCacheFilePath);

			Assert("Verify Backup should return false when it cannot restore the file", !verifyResult);
		}

		public void TestVerifyBackupShouldReturnTrueWhenRestoreSucceed()
		{
			var serverName = System.Environment.MachineName;
			var localCacheFilePath = CopyTestFileResource("OdysseyNoDescription.bak");
			var mockCachePath = GetMockCachePath();
			var logger = new TestLogger();
			var mockCw1InstanceClass = new Mock<ICargoWiseOneInstanceClass>();
			var buildDeployer = new BuildDeployerForTest(logger, mockCw1InstanceClass.Object, localDatabaseCachePath: mockCachePath);

			var verifyResult = buildDeployer.VerifyBackupExposed(serverName, localCacheFilePath);

			Assert("Verify Backup should return true when it successfully restores the file", verifyResult);
		}

		#endregion

		#region AON setup tests

		const string sqlServersSharedPath = @"\\uat-backups.wtg.zone\SQL_Backup";
		const string aonShelfName = "2C09DE0D42D94B7E8485";

		[DeveloperOnlyTest]
		public void TestAutoDeployTestedShelfWithAONSetupWherePrimarySqlServerIsNotProvided()
		{
			var shelfName = aonShelfName;
			var backupDirectoryName = Path.Combine(sqlServersSharedPath, shelfName);

			using (var deleteDirectoryAction = new DisposableAction(() =>
			{
				if (Directory.Exists(backupDirectoryName))
				{
					Directory.Delete(backupDirectoryName, true);
				}
			}))
			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			{
				if (!Directory.Exists(backupDirectoryName))
				{
					Directory.CreateDirectory(backupDirectoryName);
				}

				var backupPath = TestDataHelpers.CopyTestFileResource(backupDirectoryName, "OdysseyNoDescription.bak");
				var taskComments = $@"
TestRigRestoreFromBackup: {backupPath}
TestRigRegistryEntries: BackupfilePath|{backupDirectoryName}
TestRigAONSecondarySqlServer: au2sp-ssql-403b.sand.wtg.zone\INSTANCE1
TestRigSetupAlwaysOn: true
";

				SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");
				var buildDeployer = CreateDeployer();

				// Act/Assert
				var exception = AssertExceptionThrown<DeploymentCancelledException>(() => buildDeployer.AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments)));
				AssertEquals($"Insufficient test rig options specified, skipping auto deployment. You must specify TestRigRestoreFromBackup, TestRigSqlServer/DatabaseName and TestRigSqlServer, TestRigAONSecondarySqlServer and BackupFilePath through TestRigRegistryEntries for Always On enabled build.", exception.Message);
			}
		}

		[DeveloperOnlyTest]
		public void TestAutoDeployTestedShelfWithAONSetupWherePrimarySqlServerIsNotConfiguredForAON()
		{
			var shelfName = aonShelfName;
			var backupDirectoryName = Path.Combine(sqlServersSharedPath, shelfName);

			using (var deleteDirectoryAction = new DisposableAction(() =>
			{
				if (Directory.Exists(backupDirectoryName))
				{
					Directory.Delete(backupDirectoryName, true);
				}
			}))
			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			{
				if (!Directory.Exists(backupDirectoryName))
				{
					Directory.CreateDirectory(backupDirectoryName);
				}

				var backupPath = TestDataHelpers.CopyTestFileResource(backupDirectoryName, "OdysseyNoDescription.bak");

				var taskComments = $@"
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: {Db.ServerName}
TestRigAONSecondarySqlServer: au2sp-ssql-403b.sand.wtg.zone\INSTANCE1
TestRigRegistryEntries: BackupfilePath|{backupDirectoryName}
TestRigSetupAlwaysOn: true
";

				SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");
				var buildDeployer = CreateDeployer();

				// Act/Assert
				var exception = AssertExceptionThrown<DeploymentCancelledException>(() => buildDeployer.AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments)));
				AssertEquals($"Server '{Db.ServerName}', specified as primary server is not configured for Always On.", exception.Message);
			}
		}

		[DeveloperOnlyTest]
		public void TestAutoDeployTestedShelfWithAONSetupWhereSecondarySqlServerIsNotProvided()
		{
			var shelfName = aonShelfName;
			var backupDirectoryName = Path.Combine(sqlServersSharedPath, shelfName);

			using (var deleteDirectoryAction = new DisposableAction(() =>
			{
				if (Directory.Exists(backupDirectoryName))
				{
					Directory.Delete(backupDirectoryName, true);
				}
			}))
			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			{
				if (!Directory.Exists(backupDirectoryName))
				{
					Directory.CreateDirectory(backupDirectoryName);
				}

				var backupPath = TestDataHelpers.CopyTestFileResource(backupDirectoryName, "OdysseyNoDescription.bak");

				var taskComments = $@"
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: au2sp-ssql-403a.sand.wtg.zone\INSTANCE1
TestRigRegistryEntries: BackupfilePath|{backupDirectoryName}
TestRigSetupAlwaysOn: true
";

				SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");
				var buildDeployer = CreateDeployer();

				// Act/Assert
				var exception = AssertExceptionThrown<DeploymentCancelledException>(() => buildDeployer.AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments)));
				AssertEquals($"Insufficient test rig options specified, skipping auto deployment. You must specify TestRigRestoreFromBackup, TestRigSqlServer/DatabaseName and TestRigSqlServer, TestRigAONSecondarySqlServer and BackupFilePath through TestRigRegistryEntries for Always On enabled build.", exception.Message);
			}
		}

		[DeveloperOnlyTest]
		public void TestAutoDeployTestedShelfWithAONSetupWhereSecondarySqlServerIsNotConfiguredForAON()
		{
			var shelfName = aonShelfName;
			var backupDirectoryName = Path.Combine(sqlServersSharedPath, shelfName);

			using (var deleteDirectoryAction = new DisposableAction(() =>
			{
				if (Directory.Exists(backupDirectoryName))
				{
					Directory.Delete(backupDirectoryName, true);
				}
			}))
			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			{
				if (!Directory.Exists(backupDirectoryName))
				{
					Directory.CreateDirectory(backupDirectoryName);
				}

				var backupPath = TestDataHelpers.CopyTestFileResource(backupDirectoryName, "OdysseyNoDescription.bak");

				var taskComments = $@"
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: au2sp-ssql-403a.sand.wtg.zone\INSTANCE1
TestRigAONSecondarySqlServer: {Db.ServerName}
TestRigRegistryEntries: BackupfilePath|{backupDirectoryName}
TestRigSetupAlwaysOn: true
";
				SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");
				var buildDeployer = CreateDeployer();

				// Act/Assert
				var exception = AssertExceptionThrown<DeploymentCancelledException>(() => buildDeployer.AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments)));
				AssertEquals($"Server '{Db.ServerName}', specified as secondary server is not configured for Always On.", exception.Message);
			}
		}

		[DeveloperOnlyTest]
		public void TestAutoDeployTestedShelfWithAONSetupAndNoBackupFolderProvided()
		{
			// Arrange
			var shelfName = aonShelfName;
			var dbName = "SH0" + shelfName;
			var backupDirectoryName = Path.Combine(sqlServersSharedPath, shelfName);

			using (var deleteDirectoryAction = new DisposableAction(() =>
			{
				if (Directory.Exists(backupDirectoryName))
				{
					Directory.Delete(backupDirectoryName, true);
				}
			}))
			using (var sourcePathDirectory = new TempDirectory())
			using (var binPathDirectory = new TempDirectory())
			{
				if (!Directory.Exists(backupDirectoryName))
				{
					Directory.CreateDirectory(backupDirectoryName);
				}

				var backupPath = TestDataHelpers.CopyTestFileResource(backupDirectoryName, "OdysseyNoDescription.bak");

				var taskComments = $@"
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: au2sp-ssql-403a.sand.wtg.zone\INSTANCE1
TestRigAONSecondarySqlServer: au2sp-ssql-403b.sand.wtg.zone\INSTANCE1
TestRigSetupAlwaysOn: true
";
				SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");
				var buildDeployer = CreateDeployer();

				// Act/Assert
				var exception = AssertExceptionThrown<DeploymentCancelledException>(() => buildDeployer.AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments)));
				AssertEquals($"Insufficient test rig options specified, skipping auto deployment. You must specify TestRigRestoreFromBackup, TestRigSqlServer/DatabaseName and TestRigSqlServer, TestRigAONSecondarySqlServer and BackupFilePath through TestRigRegistryEntries for Always On enabled build.", exception.Message);
			}
		}

		[DeveloperOnlyTest]
		public void TestAutoDeployTestedShelfAndTearDownWithAON()
		{
			// Arrange
			var shelfName = aonShelfName;
			var dbName = "SH0" + shelfName;
			var dbNameOriginal = Db.DatabaseName;
			var serverNameOriginal = Db.ServerName;
			var backupDirectoryName = Path.Combine(sqlServersSharedPath, shelfName);

			using (var deleteDirectoryAction = new DisposableAction(() =>
			{
				if (Directory.Exists(backupDirectoryName))
				{
					Directory.Delete(backupDirectoryName, true);
				}
			}))
			{
				if (!Directory.Exists(backupDirectoryName))
				{
					Directory.CreateDirectory(backupDirectoryName);
				}

				Db.Connection.IgnoreCommitTracker = true;

				var backupPath = TestDataHelpers.CopyTestFileResource(backupDirectoryName, "OdysseyNoDescription.bak");

				var taskComments = $@"
TestRigRestoreFromBackup: {backupPath}
TestRigSqlServer: au2sp-ssql-403a.sand.wtg.zone\INSTANCE1
TestRigAONSecondarySqlServer: au2sp-ssql-403b.sand.wtg.zone\INSTANCE1
TestRigRegistryEntries: BackupfilePath|{backupDirectoryName},BackupReferenceDatabases|true
TestRigSetupAlwaysOn: true
TestRigCreateIcon: false
TestRigRegister: false
TestRigLaunchDbUpgrade: false";

				var taskInfo = new TaskInfo(shelfName, "whoever", taskComments);
				var testedShelfDeploymentOptions = new TestedShelfDeploymentOptions(taskInfo);

				var primaryServerName = testedShelfDeploymentOptions.SqlServer;
				var secondaryServerName = testedShelfDeploymentOptions.AONSecondarySqlServer;
				var backupSharedLocation = testedShelfDeploymentOptions.RegistryEntries["BackupFilePath"] as string;
				var backupFileName = Path.Combine(backupSharedLocation, $"{dbName}.bak");
				var logBackupFileName = Path.Combine(backupSharedLocation, $"{dbName}.trn");

				using (new DisposableAction(() =>
				{
					Db.ClearServerDetails();
					Db.InitializeDatabaseDetails(serverNameOriginal, dbNameOriginal);
				}))
				using (var sourcePathDirectory = new TempDirectory())
				using (var binPathDirectory = new TempDirectory())
				{
					Db.ClearServerDetails();
					Db.InitializeDatabaseDetails(primaryServerName, dbName);

					try
					{
						SetupTestFiles(sourcePathDirectory.DirectoryName, binPathDirectory.DirectoryName, "16.4.1.69", new DateTime(2016, 4, 1, 11, 6, 14), "ALP");
						var buildDeployer = CreateDeployer();

						// Act
						buildDeployer.AutoDeployTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, taskInfo);

						// Assert
						using (Db.DisableSchemaVersionCheck())
						using (var adminConnection = Db.NewAdminConnection(primaryServerName, dbName))
						using (var adminConnectionToSecondary = Db.NewAdminConnection(secondaryServerName, Db.SqlMasterDb))
						{
							Assert(
								$"Database '{dbName}' should have FULL recovery model.",
								adminConnection.Exists(
									"FROM sys.databases WHERE name = @dbName AND recovery_model_desc = N'FULL'",
								cmd =>
								{
									cmd.AddParameter("@dbName", SqlDbType.NVarChar, 128, dbName);
								}));

							Assert(
								$"Database '{dbName}' backup file should not exist in '{backupSharedLocation}'.",
								File.Exists(backupFileName));

							Assert($"Database '{dbName}' log backup file should not exist in '{backupSharedLocation}'.",
								File.Exists(logBackupFileName));

							AssertDatabaseExists(adminConnectionToSecondary, true, dbName);

							var sqlDatabaseInAvailabilityGroupFrom = @"
FROM master.sys.availability_groups AS ag
	INNER JOIN master.sys.availability_replicas AS ar
		ON ag.group_id = AR.group_id
	INNER JOIN master.sys.dm_hadr_availability_replica_states AS arstates
		ON ar.replica_id = arstates.replica_id
	INNER JOIN master.sys.dm_hadr_database_replica_cluster_states AS dbcs
		ON arstates.replica_id = dbcs.replica_id
WHERE 1=1
	AND ag.name = @agName
	AND ar.replica_server_name = @replicaServerName
	AND dbcs.is_database_joined = 1
	AND dbcs.database_name = @database
";
							var serverName = adminConnection.ExecuteScalar<string>("SELECT @@SERVERNAME");

							Assert(
								$"Database '{dbName}' should be joined to an availability group '{shelfName}' on server '{adminConnection.ServerName}'.",
								adminConnection.Exists(
									sqlDatabaseInAvailabilityGroupFrom,
									cmd =>
									{
										cmd.AddParameter("@agName", SqlDbType.NVarChar, 128, shelfName);
										cmd.AddParameter("@replicaServerName", SqlDbType.NVarChar, 128, serverName);
										cmd.AddParameter("@database", SqlDbType.NVarChar, 128, dbName);
									}));

							serverName = adminConnectionToSecondary.ExecuteScalar<string>("SELECT @@SERVERNAME");

							Assert(
								$"Database '{dbName}' should be joined to an availability group '{shelfName}' on server '{adminConnectionToSecondary.ServerName}'.",
								adminConnection.Exists(
									sqlDatabaseInAvailabilityGroupFrom,
									cmd =>
									{
										cmd.AddParameter("@agName", SqlDbType.NVarChar, 128, shelfName);
										cmd.AddParameter("@replicaServerName", SqlDbType.NVarChar, 128, serverName);
										cmd.AddParameter("@database", SqlDbType.NVarChar, 128, dbName);
									}));
						}
					}
					finally
					{
						CreateDeployer().TeardownTestedShelf("DEBUG", string.Empty, sourcePathDirectory, binPathDirectory, new TaskInfo(shelfName, "whoever", taskComments));

						var sqlAvailabilityGroupFrom = @"
FROM master.sys.availability_groups
WHERE 1=1
	AND name = @agName
";
						using (Db.DisableSchemaVersionCheck())
						using (var adminConnectionToSecondary = Db.NewAdminConnection(secondaryServerName, Db.SqlMasterDb))
						{
							Assert(
								$"There should be no availability group '{shelfName}' on server '{secondaryServerName}'.",
								!adminConnectionToSecondary.Exists(sqlAvailabilityGroupFrom, cmd => cmd.AddParameter("@agName", SqlDbType.NVarChar, 128, shelfName)));

							AssertDatabaseExists(adminConnectionToSecondary, false, dbName);
						}

						using (Db.DisableSchemaVersionCheck())
						using (var adminConnectionToPrimary = Db.NewAdminConnection(primaryServerName, Db.SqlMasterDb))
						{
							Assert(
								$"There should be no availability group '{shelfName}' on server '{primaryServerName}'.",
								!adminConnectionToPrimary.Exists(sqlAvailabilityGroupFrom, cmd => cmd.AddParameter("@agName", SqlDbType.NVarChar, 128, shelfName)));

							AssertDatabaseExists(adminConnectionToPrimary, false, dbName);

							Assert(
								$"Database '{dbName}' backup file should not exist in '{backupSharedLocation}'.",
								!File.Exists(backupFileName));

							Assert($"Database '{dbName}' log backup file should not exist in '{backupSharedLocation}'.",
								!File.Exists(logBackupFileName));
						}

						DbCommitTracker.Reset(dbName);
					}
				}
			}
		}

		#endregion AON setup tests

		class GetDatabasesToDropTest : TestCase
		{
			static readonly string[] SystemAndUpgradeTempDbTemplates = new[]
			{
				"{0}",
				"{0}_Audit",
				"{0}_EDW",
				"{0}_RefDb_Trf_CA",
				"DBUPG_NewTemplateDB_{0}",
				"DBUPG_NewTemplateDB_{0}_SD0001",
				"DBUPG_DataCopyDB_{0}_UserRepository",
			};

			static readonly string[] DatabasesToNotDropTemplate = new[]
			{
				"{0}Audit",
				"{0}XAudit",
				"{0}ABC",
				"{0}ABC_Audit",
				"DBUPGXPreSchemaUpgradeDbX{0}",
				"CompletelyUnrelatedDbAudit",
				"CompletelyUnrelatedDb_Audit",
			};

			public void TestOnlyReturnsSystemAndUpgradeTempDbsWhenRefDbIsNotPassed()
			{
				CombineAssertions(() =>
				{
					AssertOnlyReturnsSystemAndUpgradeTempDbs("DbToDrop");
					AssertOnlyReturnsSystemAndUpgradeTempDbs("SH0WI00645814");
				});
			}

			public void TestOnlyReturnsSystemAndUpgradeTempDbsWhenRefDbIsPassed()
			{
				CombineAssertions(() =>
				{
					AssertOnlyReturnsSystemAndUpgradeTempDbs("ShelfTestDb", "SingleRefDb");
					AssertOnlyReturnsSystemAndUpgradeTempDbs("OdysseyWTGSYD", "SingleRefDb2ElectricBoogaloo");
				});
			}

			void AssertOnlyReturnsSystemAndUpgradeTempDbs(string mainDbName, string singleRefDb = null)
			{
				// Arrange
				var expectedDropDatabases = SystemAndUpgradeTempDbTemplates
					.Select(db => string.Format(db, mainDbName))
					.ToList();

				if (singleRefDb is not null)
				{
					expectedDropDatabases.Add(singleRefDb);
				}

				var expectedDoNotDropDatabases = DatabasesToNotDropTemplate
					.Select(db => string.Format(db, mainDbName))
					.Append("CW-RefDatabase");

				var tempDatabasesToCreate = expectedDropDatabases
					.Concat(expectedDoNotDropDatabases)
					.Except("CW-RefDatabase");

				using (var connection = Db.NewAdminConnection())
				using (new TemporaryDatabases(tempDatabasesToCreate, mainDbName, connection))
				{
					// Act
					var actualContainedDatabases = BuildDeployer.GetDatabasesToDrop(connection, mainDbName, null, singleRefDb);

					// Assert
					AssertContainsExactElementsInAnyOrder($"GetDatabasesToDrop({mainDbName}, {singleRefDb ?? "null"})", expectedDropDatabases, actualContainedDatabases);
				}
			}

			public void TestTeardownTestedShelfOnlyDropsSystemAndUpgradeTempDbs()
			{
				CombineAssertions(() =>
				{
					AssertTeardownTestedShelfOnlyDropsSystemAndUpgradeTempDbs("IntegrationTestDb", "ReferenceDb");
					AssertTeardownTestedShelfOnlyDropsSystemAndUpgradeTempDbs("MyFavouriteTVShowsOfAllTime", "Top10IceCreamFlavoursRefDb");
				});

				void AssertTeardownTestedShelfOnlyDropsSystemAndUpgradeTempDbs(string mainDbName, string singleRefDbName)
				{
					// Arrange
					var dbsBeforeSetupTest = GetAllDatabaseNames();
					var buildDeployer = CreateDeployer(Mock.Of<ITaskLogger>(), Mock.Of<ICargoWiseOneInstanceClass>());
					var taskInfo = new TaskInfo("CW1ButIDroppedOdysseyForPerformanceReasons", "A50", $@"
						TestRigRestoreFromBackup: randomBackupPathSoConfigShouldRestoreFromBackupIsTrue
						TestRigSqlServer: {Db.ServerName}
						TestRigDatabaseName: {mainDbName}
						TestRigSingleRefDatabaseName: {singleRefDbName}
						TestRigCreateIcon: false
					");

					var expectedDropDatabases = SystemAndUpgradeTempDbTemplates
						.Select(db => string.Format(db, mainDbName))
						.Append(singleRefDbName);

					var expectedDoNotDropDatabases = DatabasesToNotDropTemplate
						.Select(db => string.Format(db, mainDbName));

					var tempDatabasesToCreate = expectedDropDatabases
						.Concat(expectedDoNotDropDatabases);

					using (var connection = Db.NewAdminConnection())
					using (new TemporaryDatabases(tempDatabasesToCreate, mainDbName, connection))
					{
						// Act
						buildDeployer.TeardownTestedShelf(string.Empty, string.Empty, string.Empty, string.Empty, taskInfo);

						// Assert
						var actualDbsAfterTearDown = GetAllDatabaseNames();
						var expectedDbsAfterTearDown = expectedDoNotDropDatabases.Concat(dbsBeforeSetupTest);

						AssertContainsExactElementsInAnyOrder("Expected databases existing after TeardownTestedShelf", expectedDbsAfterTearDown, actualDbsAfterTearDown);
					}
				}
			}
		}

		static List<string> GetAllDatabaseNames()
		{
			var dbNames = new List<string>();

			using var command = Db.Connection.Command("select name from sys.databases");
			using var reader = command.ExecuteReader();

			while (reader.Read())
			{
				dbNames.Add(reader.GetString(0));
			}

			return dbNames;
		}

		void AssertDatabaseExists(bool expectedValue, string databaseName)
		{
			using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
			{
				AssertDatabaseExists(connection, expectedValue, databaseName);
			}
		}

		void AssertDatabaseExists(AdminConnection adminConnection, bool expectedValue, string databaseName)
		{
			using (var command = adminConnection.Command("select count(*) from sys.databases where name = @name"))
			{
				command.AddParameter("name", SqlDbType.VarChar, databaseName);
				AssertEquals(expectedValue, (int)command.ExecuteScalar() > 0);
			}
		}

		void AssertDatabaseLoginDoesNotExist(string loginPattern, string message = null)
		{
			using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
			{
				AssertDatabaseLoginExists(connection, loginPattern, false, message);
			}
		}

		void AssertDatabaseLoginExists(string loginPattern)
		{
			using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
			{
				AssertDatabaseLoginExists(connection, loginPattern, true);
			}
		}

		static void AssertDatabaseLoginExists(DbConnection connection, string loginPattern, bool doesExist, string message = "")
		{
			var outputMessage = string.Format("Logins with pattern '{0}' are {1} existing. {2}'", loginPattern, doesExist ? string.Empty : "not", message);
			AssertEquals(outputMessage, doesExist, GetDatabaseLoginCount() > 0);

			int GetDatabaseLoginCount()
			{
				using (var command = connection.Command("SELECT COUNT(*) FROM sys.sql_logins WHERE name LIKE @loginPattern"))
				{
					command.AddParameter("loginPattern", SqlDbType.VarChar, loginPattern);
					int count = (int)command.ExecuteScalar();
					return count;
				}
			}
		}

		static void AssertDatabaseUserExists(AdminConnection connection, string dbName, string dbUserName, bool expectedResult)
		{
			AssertEquals($"Db user [{dbUserName}] on db [{dbName}] should {(expectedResult ? "exist" : "not exist")}", expectedResult, DoesDatabaseUserExist());

			bool DoesDatabaseUserExist()
			{
				return connection.Exists($"FROM {dbName.QuoteName()}.sys.database_principals WHERE name = @dbUserName", cmd =>
				{
					cmd.AddParameter("dbUserName", SqlDbType.VarChar, dbUserName);
				});
			}
		}

		void AssertSiteExists(string log, bool expectedValue, string site, string application, string serverName, string databaseName)
		{
			using (var serverManager = new ServerManager())
			{
				var siteItem = serverManager.Sites.SingleOrDefault(s => s.Name == site);
				AssertEquals(log, expectedValue, siteItem != null);
				if (expectedValue)
				{
					AssertNotNull(application, siteItem.Applications["/" + application]);
					var configInfo = WebDbConfiguration.GetConfiguration(WebAppPath.For(siteItem, siteItem.Applications["/" + application]));
					AssertEquals(serverName, configInfo.ServerName);
					AssertEquals(databaseName, configInfo.DatabaseName);
				}
			}
		}

		void AssertDeploymentIDLogged(string log, string dbName)
		{
			Assert($"[{log}] should contain [{TaskLoggerExtensions.DeploymentInstanceTag + Db.ServerName + "_" + dbName}]", log.Contains(TaskLoggerExtensions.DeploymentInstanceTag + Db.ServerName + "_" + dbName));
		}

		void SetupTestFiles(string sourcePath, string binPath, string versionNumber, DateTime exeDate, string releaseRing, bool buildMasterEdp = false)
		{
			CompileExe(Path.Combine(binPath, ExeFileNames.CargoWiseOneExeForVersionInfo), versionNumber, exeDate, releaseRing);
			File.WriteAllText(Path.Combine(binPath, BuildConstants.BuildXmlFileName), string.Format(@"<Build xmlns=""http://www.edi.com.au/build.xsd"">
	<Solutions>
		<Solution Filename=""whatever.sln"">
			<Bin>{0}</Bin>
			<Bin>Test.dll</Bin>
			<Bin>ZClientEDI.dll</Bin>
		</Solution>
		<OtherFiles>
			<Filename>EnterpriseWebDeploy.zip</Filename>
		</OtherFiles>
	</Solutions>
</Build>", ExeFileNames.CargoWiseOneExeForVersionInfo));
			CreateGlowWebDeployZip(binPath);
			Directory.CreateDirectory(Path.Combine(binPath, "DocumentXmls"));
			File.WriteAllText(Path.Combine(binPath, "Test.dll"), "Test");
			File.WriteAllText(Path.Combine(binPath, "ZClientEDI.dll"), "ZClientEDI");
			CreateEnterpriseWebDeployZip(binPath);

			if (buildMasterEdp)
			{
				ImportableBuild.Prepare(sourcePath, binPath);
				var builder = new RuntimePackageBuilder(binPath, binPath);
				builder.BuildMaster();
				File.WriteAllText(Path.Combine(binPath, "package-path.txt"), builder.LastPackagePath);
			}
		}

		void CreateEnterpriseWebDeployZip(string binPath)
		{
			var targetFile = Path.Combine(binPath, "EnterpriseWebDeploy.zip");
			if (!File.Exists(targetFile))
			{
				using (var tempDir = new TempDirectory())
				{
					Directory.CreateDirectory(Path.Combine(tempDir, "shared-bin"));
					Directory.CreateDirectory(Path.Combine(tempDir, "Forwarding"));
					File.WriteAllText(Path.Combine(tempDir, "Forwarding", "Test.html"), "Test");
					ZipFile.CreateFromDirectory(tempDir, targetFile);
				}
			}
		}

		void CreateGlowWebDeployZip(string binPath)
		{
			var targetFile = Path.Combine(binPath, "GlowWebDeploy.zip");
			if (!File.Exists(targetFile))
			{
				using (var tempDir = new TempDirectory())
				{
					Directory.CreateDirectory(Path.Combine(tempDir, "Glow"));
					Directory.CreateDirectory(Path.Combine(tempDir, "GlowWebClient"));
					File.WriteAllText(Path.Combine(tempDir, "GlowWebClient", "Test.html"), "Test");
					ZipFile.CreateFromDirectory(tempDir, targetFile);
				}
			}
		}

		string CreateDummyLocalCacheFile()
		{
			const string filenamePrefix = "CW";
			var randomSuffix = Guid.NewGuid().ToString("N").Substring(24);
			var filename = $"{filenamePrefix}OdysseyNoDescription{randomSuffix}.bak";
			var mockNetworkFilePath = Path.Combine(CommonTempDirectory.DirectoryName, filename);

			var dummyContent = $"This is a dummy local file content for the temporary file. {System.Environment.NewLine}";
			dummyContent += $"Location: {mockNetworkFilePath}{System.Environment.NewLine}";
			dummyContent += $"Creation Time (UTC): {DateTime.UtcNow:O}{System.Environment.NewLine}";

			File.WriteAllText(mockNetworkFilePath, dummyContent, Encoding.UTF8);

			return mockNetworkFilePath;
		}

		string GetMockCachePath()
		{
			var cachePath = CommonTempDirectory.DirectoryName;
			var rootName = new DirectoryInfo(cachePath).Root.Name;
			var childPath = cachePath.Substring(rootName.Length);
			rootName = $"{rootName.Substring(0, rootName.IndexOf(":", StringComparison.Ordinal))}$";

			var mockNetworkPath = Path.Combine(rootName, childPath);

			return mockNetworkPath;
		}

		static void CompileExe(string path, string versionNumber, DateTime exeDate, string releaseRing)
		{
			using (var tempPath = new TempDirectory())
			{
				ReleaseInfo.CreateNewFileForTesting(Path.Combine(tempPath.DirectoryName, "Enterprise." + ReleaseInfo.XmlFileName), versionNumber, exeDate, releaseRing);

				File.WriteAllText(Path.Combine(tempPath.DirectoryName, "Main.cs"), $@"
					[assembly: System.Reflection.AssemblyFileVersion(""{versionNumber}"")]

					public static class Program
					{{
						public static void Main(string[] cmd)
						{{
							System.Threading.Thread.Sleep(System.TimeSpan.FromSeconds(10));
						}}
					}}");

				var compiler = new CSharpCodeProvider();
				var options = new CompilerParameters();
				options.ReferencedAssemblies.Add("System.dll");
				options.GenerateExecutable = true;
				options.OutputAssembly = Path.Combine(path);
				options.EmbeddedResources.Add(Path.Combine(tempPath.DirectoryName, "Enterprise." + ReleaseInfo.XmlFileName));

				CompilerResults result = compiler.CompileAssemblyFromFile(options, Path.Combine(tempPath.DirectoryName, "Main.cs"));
				string[] output = new string[result.Output.Count];
				result.Output.CopyTo(output, 0);
				AssertEquals(string.Join("\r\n", output), 0, result.Errors.Count);
			}
		}

		static IEnumerable<Dictionary<string, object>> GetRecords(Func<DbConnection> makeConnection, string tablename)
		{
			var result = new List<Dictionary<string, object>>();
			using (var connection = makeConnection())
			using (var command = connection.Command(Invariant($@"SELECT * FROM {tablename}")))
			using (var reader = command.ExecuteReader())
			{
				Dictionary<string, int> columnLookup = null;
				while (reader.Read())
				{
					columnLookup = columnLookup ?? reader.GetSchemaTable().Rows
						.Cast<DataRow>()
						.ToDictionary(row => (string)row["ColumnName"], row => (int)row["ColumnOrdinal"]);

					var record = columnLookup
						.ToDictionary(column => column.Key, column => reader.GetValue(column.Value));

					result.Add(record);
				}
			}
			return result;
		}

		string CopyTestFileResource(string fileName)
			=> TestDataHelpers.CopyTestFileResource(CommonTempDirectory.DirectoryName, fileName);

		TempDirectory CommonTempDirectory => commonTempDirectoryLazy ?? (commonTempDirectoryLazy = new TempDirectory());
		TempDirectory commonTempDirectoryLazy;

		PortableWebServer portableWebServer;
		string webServerInstallPath;

		protected override void SetUp()
		{
			base.SetUp();

			mockProductKeyService = new Mock<IProductKeyService>(MockBehavior.Strict);
			ProductKeyService.Instance.Value = mockProductKeyService.Object;
			mockProductRegistration = new Mock<IProductRegistration>(MockBehavior.Strict);
			mockProductRegistrationDisposable = ObjectFactory.Substitute(mockProductRegistration.Object);

			portableWebServer = new PortableWebServer();
			webServerInstallPath = portableWebServer.InstallPath;
		}

		protected override void TearDown()
		{
			portableWebServer.Dispose();
			commonTempDirectoryLazy?.Dispose();
			commonTempDirectoryLazy = null;
			base.TearDown();
			mockProductRegistrationDisposable?.Dispose();
		}

		Mock<IProductKeyService> mockProductKeyService;
		Mock<IProductRegistration> mockProductRegistration;
		IDisposable mockProductRegistrationDisposable;

		static BuildDeployer CreateDeployer(ITaskLogger logger = null, ICargoWiseOneInstanceClass cargoWiseOneInstanceClass = null, IDirectorySearcher directorySearcher = null, EdiProdServiceTaskNudgeClient serviceTaskNudgeClient = null)
		{
			return new BuildDeployer(logger ?? new TaskLogger(), cargoWiseOneInstanceClass ?? new CargoWiseOneInstanceClass(), directorySearcher, serviceTaskNudgeClient ?? CreateDummyNudgeClient());
		}

		static EdiProdServiceTaskNudgeClient CreateNudgeClient(Mock<HttpMessageHandler> mockHandler)
		{
			return new EdiProdServiceTaskNudgeClient(new HttpClient(mockHandler.Object) { BaseAddress = new Uri("http://nothing") });
		}

		static EdiProdServiceTaskNudgeClient CreateDummyNudgeClient()
		{
			var mockHandler = new Mock<HttpMessageHandler>();
			mockHandler
				.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
				.Returns(Task.FromResult(new HttpResponseMessage { StatusCode = HttpStatusCode.OK }));
			return CreateNudgeClient(mockHandler);
		}

		class MockAutoDeployLatestBuildDeployer : BuildDeployer
		{
			public MockAutoDeployLatestBuildDeployer(ITaskLogger logger, string latestBuildArchivePath, string packageArchivePath, EdiProdServiceTaskNudgeClient nudgeClient = null)
				: base(logger, new CargoWiseOneInstanceClass(), serviceTaskNudgeClient: nudgeClient ?? CreateDummyNudgeClient())
			{
				IBPLatestBuildArchivePath = latestBuildArchivePath;
				IBPPackageArchivePath = packageArchivePath;
			}

			public override string IBPLatestBuildArchivePath { get; }
			public override string IBPPackageArchivePath { get; }
		}

		class TestLogger : ITaskLogger
		{
			public TestLogger(Action<string> loggerCallback = null)
			{
				this.loggerCallback = loggerCallback;
			}

			public void RecordInfo(string message)
			{
				AppendLine(message);
			}

			public IDisposable RecordTask(string taskInfo)
			{
				AppendLine("start " + taskInfo);
				return new DisposableAction(() => AppendLine("end " + taskInfo));
			}

			void AppendLine(string line)
			{
				loggerCallback?.Invoke(line);
				buffer.AppendLine(line);
			}

			public override string ToString() => buffer.ToString();

			readonly StringBuilder buffer = new StringBuilder();
			readonly Action<string> loggerCallback;
		}

		public class BuildDeployerForTest : BuildDeployer
		{
			public BuildDeployerForTest(ITaskLogger taskLogger,
				ICargoWiseOneInstanceClass cargoWiseOneInstanceClass,
				IDirectorySearcher directorySearcher = null,
				string localDatabaseCachePath = null) : base(taskLogger,
				cargoWiseOneInstanceClass, directorySearcher, CreateDummyNudgeClient())
			{
				LocalDatabaseCachePath = localDatabaseCachePath ?? base.LocalDatabaseCachePath;
			}

			protected override string LocalDatabaseCachePath { get; }

			public bool VerifyBackupExposed(string sqlServer, string backupPath) =>
				VerifyBackup(sqlServer, backupPath);
		}

		class TemporaryDatabases : IDisposable
		{
			readonly IEnumerable<string> DbNames;
			readonly AdminConnection Connection;

			public TemporaryDatabases(IEnumerable<string> dbNames, string mainDbName, AdminConnection connection)
			{
				DbNames = dbNames;
				Connection = connection;

				foreach (var dbName in dbNames)
				{
					Connection.ExecuteNonQuery($"CREATE DATABASE {dbName.QuoteName()}");
				}

				Connection.ExecuteNonQuery($@"
USE {mainDbName.QuoteName()};
CREATE TABLE [StmData] (
   [SD_PK] UNIQUEIDENTIFIER NOT NULL,
   [SD_Name] VARCHAR(300) NOT NULL DEFAULT '',
   [SD_Owner] UNIQUEIDENTIFIER NULL,
   [SD_DepartmentGuid] UNIQUEIDENTIFIER NULL,
   [SD_Type] CHAR(3) NOT NULL DEFAULT '',
   [SD_IsLogged] BIT NOT NULL DEFAULT 0,
   [SD_BinaryValue] VARBINARY(MAX) NULL,
   [SD_GuidValue] UNIQUEIDENTIFIER NULL,
   [SD_IsCancelled] BIT NOT NULL DEFAULT 0,
   [SD_PreserveTestValue] BIT NOT NULL DEFAULT 0,
   [SD_SystemCreateTimeUtc] SMALLDATETIME NULL,
   [SD_SystemCreateUser] VARCHAR(3) NOT NULL DEFAULT '',
   [SD_SystemLastEditTimeUtc] SMALLDATETIME NULL,
   [SD_SystemLastEditUser] VARCHAR(3) NOT NULL DEFAULT '',
);
USE MASTER;
");
			}

			public void Dispose()
			{
				foreach (var dbName in DbNames)
				{
					Connection.ExecuteNonQuery($"DROP DATABASE IF EXISTS {dbName.QuoteName()}");
				}
			}
		}
	}
}

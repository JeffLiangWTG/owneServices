using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using CargoWise.ActiveDirectory;
using CargoWise.Application;
using CargoWise.BuildTools;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.SqlServer;
using CargoWise.Shared;
using CargoWise.Types;
using Dat.Integration;
using Dat.Integration.Deployment;
using Enterprise.Client.EDI.AutoDeploy;
using Enterprise.Client.EDI.ReleaseBuilds;
using Enterprise.DataTools.DbBackupAndRestore.Business;
using Enterprise.Environment;
using Enterprise.Initialisation;
using Enterprise.Integration.Licensing;
using Enterprise.Upgrades;
using Enterprise.ZArchitecture.Core;
using Newtonsoft.Json;
using WTG.DeploymentUtils.FileSystem;
using WTG.StaticAnalysis.Annotation;
using static System.FormattableString;
using static System.StringComparison;
using static Enterprise.Integration.Environment;
using DeploymentAssemblyLoader = Enterprise.Dat.Deployment.AssemblyLoader;

namespace Enterprise.Dat.Implementation
{
	[CodeAlive("DAT Implementation")]
	public partial class BuildDeployer : IBuildDeployer, IBuildDeployer2
	{
		public BuildDeployer(ITaskLogger taskLogger)
			: this(taskLogger, new CargoWiseOneInstanceClass(), new DirectorySearcherWrapper(), new EdiProdServiceTaskNudgeClient())
		{
		}

		public BuildDeployer(ITaskLogger taskLogger, ICargoWiseOneInstanceClass cargoWiseOneInstanceClass, IDirectorySearcher directorySearcher = null, EdiProdServiceTaskNudgeClient serviceTaskNudgeClient = null)
		{
			DeploymentAssemblyLoader.Enable();
			this.logger = new IndentedTaskLogger(taskLogger);
			this.processFactory = new DeploymentProcessFactory();
			this.cargoWiseOneInstanceClass = cargoWiseOneInstanceClass;
			this.directorySearcher = directorySearcher;
			this.serviceTaskNudgeClient = serviceTaskNudgeClient ?? new EdiProdServiceTaskNudgeClient();
		}

		public void AutoDeployLatestBuild(string buildConfiguration, string deploymentConfiguration, string sourcePath, string binPath)
		{
			if (buildConfiguration.Equals("RELEASE", OrdinalIgnoreCase) && !string.IsNullOrEmpty(deploymentConfiguration) && deploymentConfiguration.Equals(ReleaseBuildContent.IBPMasterPackageDeploymentConfiguration, OrdinalIgnoreCase))
			{
				InitializeEnvironment();

				ImportableBuild.Prepare(sourcePath, binPath);

				var packagePath = GetPackagePath(binPath);

				using (logger.RecordTask("Copying package"))
				{
					File.Copy(packagePath, Path.Combine(IBPPackageArchivePath, Path.GetFileName(packagePath)));
				}

				// can remove archive creation after ediProd is upgraded to read from package archive
				var releaseInfo = new ReleaseInfo(Path.Combine(binPath, ReleaseInfo.XmlFileName));
				var targetDirectoryName = releaseInfo.ReleaseRing + "_" + releaseInfo.VersionNumber + "_" + releaseInfo.ExeDate.ToString("yyyyMMddHHmmss", Culture.Invariant);
				var targetDirectory = Path.Combine(IBPLatestBuildArchivePath, targetDirectoryName);
				CopyDirectory(binPath, targetDirectory);
				File.WriteAllText(Path.Combine(targetDirectory, "ArchiveComplete"), "ArchiveComplete");
				logger.RecordInfo("Archived Build: " + targetDirectory);

				foreach (var ringArchive in Directory.EnumerateDirectories(IBPLatestBuildArchivePath, releaseInfo.ReleaseRing + "*"))
				{
					if (!Path.GetFileName(ringArchive).Equals(targetDirectoryName, OrdinalIgnoreCase))
					{
						try
						{
							FileIO.DeleteDirectory(ringArchive);
						}
						catch (Exception ex)
						{
							logger.RecordInfo("Error deleting " + ringArchive + ": " + ex.ToString());
						}
					}
				}

				serviceTaskNudgeClient.NudgeServiceTaskAsync("IBP", logger).GetAwaiter().GetResult();
			}
			else
			{
				throw new InvalidOperationException(string.Format(Culture.Invariant, "Unexpected buildConfiguration '{0}' and deploymentConfiguration '{1}', please check Build.xml settings.", buildConfiguration, deploymentConfiguration));
			}
		}

		static string GetPackagePath(string binPath)
		{
			var packagePathFilePath = Path.Combine(binPath, "package-path.txt");
			return File.ReadAllText(packagePathFilePath);
		}

		static void CopyDirectory(string sourcePath, string targetPath)
		{
			Directory.CreateDirectory(targetPath);
			foreach (var file in Directory.GetFiles(sourcePath))
			{
				File.Copy(file, Path.Combine(targetPath, Path.GetFileName(file)));
			}
			foreach (var directory in Directory.GetDirectories(sourcePath))
			{
				CopyDirectory(directory, Path.Combine(targetPath, Path.GetFileName(directory)));
			}
		}

		public void AutoDeployTestedShelf(string buildConfiguration, string deploymentConfiguration, string sourcePath, string binPath, TaskInfo taskInfo)
		{
			logger.RecordInfo($"TaskComments:{{{System.Environment.NewLine}\t{taskInfo.TaskComments.Replace(System.Environment.NewLine, System.Environment.NewLine + "\t")}{System.Environment.NewLine}}}");
			InitializeEnvironment();
			var options = new TestedShelfDeploymentOptions(taskInfo);

			if (!options.AnyOptionsSet)
			{
				if (options.SetupAlwaysOn)
				{
					throw new DeploymentCancelledException("Insufficient test rig options specified, skipping auto deployment. You must specify TestRigRestoreFromBackup, TestRigSqlServer/DatabaseName and TestRigSqlServer, TestRigAONSecondarySqlServer and BackupFilePath through TestRigRegistryEntries for Always On enabled build.");
				}

				throw new DeploymentCancelledException("Insufficient test rig options specified, skipping auto deployment. You must specify TestRigRestoreFromBackup or a TestRigSqlServer/DatabaseName.");
			}
			else
			{
				if (options.DatabaseName.Contains("_"))
				{
					throw new InvalidOperationException("Database Name cannot include an underscore.");
				}

				using (logger.RecordTask("AutoDeployTestedShelf starts to clean up old env."))
				{
					TeardownTestedShelf(buildConfiguration, deploymentConfiguration, sourcePath, binPath, taskInfo);
				}

				using (InitializeDb(options))
				{
					string packagePath = null;
					string systemPackagePath = null;
					logger.RecordInfo($"TestedShelfDeploymentOptions:" + JsonConvert.SerializeObject(options, Newtonsoft.Json.Formatting.Indented).Replace(System.Environment.NewLine + "  ", System.Environment.NewLine + "\t"));

					if (options.SetupAlwaysOn)
					{
						using (var connection = Db.NewAdminConnection(options.SqlServer, Db.SqlMasterDb))
						{
							if (connection.ExecuteScalar<int>("SELECT SERVERPROPERTY('IsHadrEnabled')") != 1)
							{
								throw new DeploymentCancelledException($"Server '{options.SqlServer}', specified as primary server is not configured for Always On.");
							}
						}

						using (var connection = Db.NewAdminConnection(options.AONSecondarySqlServer, Db.SqlMasterDb))
						{
							if (connection.ExecuteScalar<int>("SELECT SERVERPROPERTY('IsHadrEnabled')") != 1)
							{
								throw new DeploymentCancelledException($"Server '{options.AONSecondarySqlServer}', specified as secondary server is not configured for Always On.");
							}
						}
					}

					var restoreTask = Task.Run(() =>
					{
						if (options.ShouldRestoreFromBackup)
						{
							using (logger.RecordTask("Restoring " + options.RestoreFromBackup))
							{
								RestoreTestDatabase(options);
							}
						}
					});

					if (options.SetupAlwaysOn)
					{
						restoreTask = restoreTask.ContinueWith(task => SetupMainDatabaseForAlwaysOn(options));
					}

					var buildPackageTask = Task.Run(() =>
					{
						// currently package builder uses a common temp path for all packages, so we cannot build more than one at a time in parallel
						if (options.IncludeSystemPackage)
						{
							using (logger.RecordTask("Building system package"))
							{
								systemPackagePath = BuildSystemPackage(options, binPath);
							}
						}

						using (logger.RecordTask("Building package"))
						{
							ImportableBuild.Prepare(sourcePath, binPath);
							var packageBuilder = new RuntimePackageBuilder(binPath, Path.Combine(binPath, "package"));
							packageBuilder.Build(options.ClientCode, true);
							packagePath = packageBuilder.LastPackagePath;
						}
					});

					Task.WaitAll(restoreTask, buildPackageTask);

					using (logger.RecordTask("Creating application login"))
					{
						using var connection = Db.NewAdminConnection(options.SqlServer, options.DatabaseName);

						((IDbLoginRepair)connection).EnsureRestrictedWriterDbLogin();
						((IDbLoginRepair)connection).EnsureRestrictedReaderDbLogin();
					}

					var uploadPackageAndCreateWebsitesTask = Task.Run(() =>
					{
						using (logger.RecordTask("Uploading package"))
						{
							UploadPackage(options, packagePath, systemPackagePath);
						}

						InstallWebApplications(options);

						SetExpectedClientDllRegistryItems(options);

						SetBlazorRegistryItems(options);

						SetTestRigOrigin(options);

						ProcessRegistryEntries(options);

						UpsertWinzorOIDCSettings(options);

						AddStaffRecords(options);
					});

					var createIconTask = Task.Run(() =>
					{
						if (options.CreateIcon)
						{
							using (logger.RecordTask("Create Icon"))
							{
								CreateIcon(options);
							}
						}
					});

					var registerProductTask = Task.Run(() =>
					{
						if (options.Register)
						{
							using (logger.RecordTask("Registering product"))
							{
								RegisterProduct(options.RegistrationEnterpriseCode, options.RegistrationSqlServer, options.DatabaseName);
							}
						}
					});

					var createSRDbTask = Task.Run(() => SetSingleRefDbNameRegistry(options));

					var createMTVTask = Task.Run(() => SetMajorTransformationVersion(options));

					var resetProcessControllerHostsTask = Task.Run(() =>
					{
						if (options.ShouldRestoreFromBackup)
						{
							using (logger.RecordTask("Resetting Process Controller host information"))
							{
								ResetProcessControllerHosts(options);
							}
						}
					});

					Task.WaitAll(uploadPackageAndCreateWebsitesTask, createIconTask, registerProductTask, createSRDbTask, resetProcessControllerHostsTask, createMTVTask);

					LaunchDbUpgrade(options);
					WarmUpVersionBrokers(options);
					logger.LogDeploymentInstanceID($"{options.SqlServer}_{options.DatabaseName}");
					//SubmitWinzorE2EShelvesetToDAT(options, taskInfo);
				}
			}
		}

		// Temporary measure to stop E2E task notes execution tests for Winzor in DAT
		/*
		void SubmitWinzorE2EShelvesetToDAT(TestedShelfDeploymentOptions options, TaskInfo taskInfo)
		{
			if (options.DeployWinzor && options.RunWinzorE2ETests)
			{
				using (logger.RecordTask("Submit Winzor E2E Tests"))
				{
					using var repoLocation = new TempDirectory();
					GitCli.CloneAsync("https://devops.wisetechglobal.com/wtg/InternalTools/_git/WinzorTools", repoLocation).GetAwaiter().GetResult();
					logger.RecordInfo($"Cloned WinzorTools repo");

					var configFiles = new List<string>()
					{
						"CargoWise.Winzor.E2EIntegration.Test/appsettings.E2E.json",
						"CargoWise.Winzor.E2EIntegration.Test/appsettings.E2E.Development.json",
						"CargoWise.Winzor.E2EIntegration.Test/appsettings.E2E.DAT.json"
					};

					using (var sourceControl = (GitSourceControl)SourceControlFactory.Instance.GetSourceControl(repoLocation))
					{
						var deploymentConfig = new Dictionary<string, object>()
						{
							{ "Server:Deployment:ServerBaseUrl", $"https://{options.BlazorUrlAuthority}" },
							{ "Server:Deployment:DbServerName", options.SqlServer },
							{ "Server:Deployment:DatabaseName", options.DatabaseName }
						};

						var keysToDelete = new List<string>()
						{
							"Server:SessionBroker",
							"Server:AppServer"
						};

						sourceControl.UpdateAppsettingProperties(configFiles, deploymentConfig, keysToDelete);
						logger.RecordInfo($"Appsettings file changes for owner {taskInfo.ShelfOwner}");

						sourceControl.UseIdentity("DAT Service", "dat@wisetechglobal.com");
						sourceControl.SubmitChanges("dat", $"E2ETestRunFor{options.BlazorUrlAuthority.Split('.').First().ToUpperInvariant()}/{Guid.NewGuid().ToString("N")}", string.Empty, sourceControl.GetFilesWithPendingChanges(), taskInfo.ShelfOwner, false);
						logger.RecordInfo("E2E Test Suite Shelved");
					}
				}
			}
		}*/

		void AddStaffRecords(TestedShelfDeploymentOptions options)
		{
			var staffRecords = options.AddStaffRecords;
			if (staffRecords.IsNullOrEmpty() && options.DeployWinzor)
			{
				// WCA will have already synced the WTG users to the AD in a special group.
				// For winzor deployment, as a simplistic measure for SAND testing, we simply copy all the users to GlbStaff so they can log in.
				staffRecords = AdUsersTransToStaffRecords();
			}

			if (!staffRecords.IsNullOrEmpty())
			{
				InsertStaffRecords(staffRecords);
			}

			string[] AdUsersTransToStaffRecords()
			{
				var adUsers = directorySearcher.FindGroup("g_WTG_Token_Based_Authentication").GetMembers();
				return adUsers.Select(adUsers =>
				{
					const string CN_PREFIX = "CN=";
					const string WTG_PREFIX = "WTG.";
					var staff = adUsers.Name;
					// In observations it always seems to start with CN=. Using a conditional just in case so we can see what's going on.
					if (staff.StartsWith(CN_PREFIX))
					{
						staff = staff.Substring(CN_PREFIX.Length);
					}
					if (staff.StartsWith(WTG_PREFIX))
					{
						staff = staff.Substring(WTG_PREFIX.Length);
					}
					return staff.Trim();
				}).Where(s => !s.IsNullOrEmpty()).Distinct().ToArray();
			}

			void InsertStaffRecords(string[] staffRecords)
			{
				using (var connection = Db.NewAdminConnection(options.SqlServer, options.DatabaseName))
				{
					if (!connection.Exists("FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'GlbStaff'"))
					{
						return;
					}

					// Get the existing 3-letter-codes from the DB, to avoid duplicates.
					// For our purposes we don't really care what code values we put in.
					// For bonus points we could do something similar to FastStaffCodeCalculator
					// if we cared about the codes matching the names.
					// If we did that, note that logins are prefixed with "WTG.", e.g. "WTG.David.James"
					var usedCodes = SqlQueryToHashSet(connection, "SELECT GS_Code FROM dbo.GlbStaff");
					var usedLoginNames = SqlQueryToHashSet(connection, "SELECT GS_LoginName FROM dbo.GlbStaff");

					var department = connection.ExecuteScalar(@"
					SELECT COALESCE(
						(SELECT TOP 1 GE_PK FROM dbo.GlbDepartment WHERE GE_Code = 'SYD' AND GE_IsActive = 1),
						(SELECT TOP 1 GE_PK FROM dbo.GlbDepartment WHERE GE_IsActive = 1))");

					var branch = connection.ExecuteScalar(@"
					SELECT COALESCE(
						(SELECT TOP 1 GB_PK FROM dbo.GlbBranch WHERE GB_Code = 'TE' AND GB_IsActive = 1),
						(SELECT TOP 1 GB_PK FROM dbo.GlbBranch WHERE GB_IsActive  = 1))");

					// Prepare a table of data to insert
					var personsTable = new DataTable("GlbPerson");
					personsTable.Columns.Add("PER_PK", typeof(Guid));
					personsTable.Columns.Add("PER_IsActive", typeof(bool));
					personsTable.Columns.Add("PER_FullName", typeof(string));
					personsTable.Columns.Add("PER_HomeAddress1", typeof(string));
					personsTable.Columns.Add("PER_RN_NKCountry", typeof(string));

					// Prepare a table of data to insert
					var usersTable = new DataTable("GlbStaff");
					usersTable.Columns.Add("GS_PK", typeof(Guid));
					usersTable.Columns.Add("GS_LoginName", typeof(string));
					usersTable.Columns.Add("GS_FullName", typeof(string));
					usersTable.Columns.Add("GS_UserAddress1", typeof(string));
					usersTable.Columns.Add("GS_RN_NKCountryCode", typeof(string));
					usersTable.Columns.Add("GS_Code", typeof(string));
					usersTable.Columns.Add("GS_IsActive", typeof(bool));
					usersTable.Columns.Add("GS_IsController", typeof(bool));
					usersTable.Columns.Add("GS_GB_HomeBranch", typeof(Guid));
					usersTable.Columns.Add("GS_GE_HomeDepartment", typeof(Guid));
					usersTable.Columns.Add("GS_PER", typeof(Guid));

					// Prepare a table of data insert
					var groupLinkTable = new DataTable("GlbGroupLink");
					groupLinkTable.Columns.Add("GK_PK", typeof(Guid));
					groupLinkTable.Columns.Add("GK_IsValid", typeof(bool));
					groupLinkTable.Columns.Add("GK_MembershipType", typeof(string));
					groupLinkTable.Columns.Add("GK_GG", typeof(Guid));
					groupLinkTable.Columns.Add("GK_GS", typeof(Guid));

					var sftGgGroup = UpsertFeatureTestGroupId(connection);

					foreach (var loginName in staffRecords)
					{
						if (usedLoginNames.Contains(loginName, StringComparer.OrdinalIgnoreCase))
						{
							continue; //Skip as this user is already populated
						}
						usedLoginNames.Add(loginName);

						var fullName = loginName.Replace(".", " ");

						// Pick a random unique code. Testers shouldn't assume codes are stable,
						// so we'll randomise them.
						var newCode = GenerateNewCode(ref usedCodes);
						var gsPER = Guid.NewGuid();
						var gsPK = Guid.NewGuid();

						personsTable.Rows.Add(
							gsPER,              //PER_PK
							true,               //PER_IsActive
							fullName,           //PER_FullName
							"1 street road",    //PER_HomeAddress1
							"AU");              //PER_RN_NKCountry

						usersTable.Rows.Add(
							gsPK,                               //GS_PK
							loginName,                          //GS_LoginName
							fullName,                           //GS_FullName
							"1 street road",                    //GS_UserAddress1
							"AU",                               //GS_RN_NKCountryCode
							newCode,                            //GS_Code
							true,                               //GS_IsActive
							true,                               //GS_IsController
							Guid.Parse(branch.ToString()),      //GS_GB_HomeBranch
							Guid.Parse(department.ToString()),  //GS_GE_HomeDepartment
							gsPER);                             //GS_PER

						groupLinkTable.Rows.Add(
							Guid.NewGuid(),         //GK_PK
							true,                   //GK_IsValid
							"UDF",                  //GK_MembershipType
							sftGgGroup,             //GK_GG
							gsPK);                  //GK_GS
					}

					SaveTableUsingBulkCopy(personsTable, connection);
					SaveTableUsingBulkCopy(usersTable, connection);
					SaveTableUsingBulkCopy(groupLinkTable, connection);
				}
			}

			HashSet<string> SqlQueryToHashSet(AdminConnection cn, string sql)
			{
				using (var reader = cn.Command(sql).ExecuteReader())
				{
					var values = new HashSet<string>();
					while (reader.Read())
					{
						values.Add(reader.GetString(0));
					}
					return values;
				}
			}

			string GenerateNewCode(ref HashSet<string> codes)
			{
				var codeChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ23456789".ToCharArray();
				var random = new Random();

				if (codes.Count >= Math.Pow(codeChars.Length, 3))
				{
					throw new Exception("There are too many staffs to fit in the tight space? " + codes.Count.ToString());
				}

				string code;
				var attempt = 100_000;
				do
				{
					code = new string(new[] {
									codeChars[random.Next(codeChars.Length)],
									codeChars[random.Next(codeChars.Length)],
									codeChars[random.Next(codeChars.Length)],
								});
					attempt--;
					if (attempt < 0)
					{
						throw new Exception("This should not happen, but somehow we have it.");
					}
				} while (codes.Any(c => string.Equals(c, code, OrdinalIgnoreCase)));
				codes.Add(code);
				return code;
			}
		}

		void UpsertWinzorOIDCSettings(TestedShelfDeploymentOptions options)
		{
			// Winzor standalone requires OIDC login.
			if (!options.DeployWinzor)
			{
				return;
			}

			using (logger.RecordTask("Add Winzor Logins"))
			using (var connection = Db.NewAdminConnection(options.SqlServer, options.DatabaseName))
			{
				if (!connection.Exists("FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'GlbStaff'"))
				{
					return;
				}

				UpsertOIDCConfigSettings(connection);
				UpsertIsOIDCFederatedWithWTG(connection);
			}
		}

		string UpsertFeatureTestGroupId(AdminConnection connection)
		{
			var sql = @"
DECLARE @GG_Desc NVARCHAR(64) = 'ALL STAFF';
DECLARE @GG_Code VARCHAR(15) = 'ALL';
DECLARE @FeatureName VARCHAR(255) = 'WINZORMAINFORM';
DECLARE @GlbGroupID UNIQUEIDENTIFIER = NEWID();
INSERT INTO dbo.GlbGroup 
(
    GG_PK,
    GG_Isvalid,
    GG_IsActive,
    GG_Code,
    GG_Desc,
    GG_IsSystemDefined,
    GG_IsSales,
    GG_IsSecurityEnabled
)
SELECT
    @GlbGroupID,
    0,
    1,
    @GG_Code,
    @GG_Desc,
    1,
    0,
    1
WHERE NOT EXISTS 
(
    SELECT 1 FROM dbo.GlbGroup WHERE GG_Code = @GG_Code
);

IF @@ROWCOUNT > 0
BEGIN
    INSERT INTO dbo.StmFeatureTest (SFT_PK, SFT_IsActive, SFT_FeatureName, SFT_GG_Group
        , SFT_SystemCreateTimeUtc, SFT_SystemLastEditTimeUtc, SFT_SystemCreateUser, SFT_SystemLastEditUser)
    VALUES (NEWID(), 1, @FeatureName, @GlbGroupID
        , GETUTCDATE(), GETUTCDATE(), 'E', 'E');
    SELECT @GlbGroupID;
END
ELSE
BEGIN
    SELECT @GlbGroupID = GG_PK FROM dbo.GlbGroup WHERE GG_Code = @GG_Code;

    INSERT INTO dbo.StmFeatureTest (SFT_PK, SFT_IsActive, SFT_FeatureName, SFT_GG_Group
        , SFT_SystemCreateTimeUtc, SFT_SystemLastEditTimeUtc, SFT_SystemCreateUser, SFT_SystemLastEditUser)
    SELECT NEWID(), 1, @FeatureName, @GlbGroupID
        , GETUTCDATE(), GETUTCDATE(), 'E', 'E'
    WHERE NOT EXISTS
    (
        SELECT 1 FROM dbo.StmFeatureTest WHERE SFT_FeatureName = @FeatureName AND SFT_GG_Group = @GlbGroupID
    );
    SELECT @GlbGroupID AS GlbGroupId;
END";
			return connection.ExecuteScalar(sql).ToString();
		}

		void UpsertOIDCConfigSettings(AdminConnection connection)
		{
			// OIDC Config Settings for WiseCloud Group, for SAND Testing Purpose
			var sqlInsert = $@"
INSERT INTO dbo.STMDATA
(
	SD_PK,
	SD_Name,
	SD_Type,
	SD_IsLogged,
	SD_BinaryValue,
	SD_IsCancelled,
	SD_PreserveTestValue,
	SD_SystemCreateTimeUtc,
	SD_SystemCreateUser,
	SD_SystemLastEditTimeUtc,
	SD_SystemLastEditUser
)
SELECT
	NEWID(),
	'OIDCConfig',
	'BIN',
	1,
	CAST(N'<?xml version=""1.0"" encoding=""utf-16""?><OIDCConfig><Version>1</Version><IsOIDCEnabled>Y</IsOIDCEnabled><OIDCServerTypeCode>AZU</OIDCServerTypeCode><AuthorityURL>https://loginsimulator.wisetechglobal.com/</AuthorityURL><ClientIdentifier>dummyIdP</ClientIdentifier><ArrayOfOIDCClaimsMapping><OIDCClaimsMapping><Version>1</Version><ClaimName>user_name</ClaimName><Identifier>GlbStaff.GS_LoginName</Identifier></OIDCClaimsMapping></ArrayOfOIDCClaimsMapping><ArrayOfOIDCScope><OIDCScope><Version>1</Version><ScopeName>dummyIdP</ScopeName></OIDCScope></ArrayOfOIDCScope></OIDCConfig>' AS VARBINARY(MAX)),
	0,
	0,
	'2023-09-03 00:11:00',
	'E',
	'2023-09-03 00:11:00',
	'E'
WHERE NOT EXISTS
(
	SELECT 1 FROM dbo.STMDATA WHERE SD_Name = 'OIDCConfig'
)";

			connection.ExecuteNonQuery(sqlInsert);
		}

		void UpsertIsOIDCFederatedWithWTG(AdminConnection connection)
		{
			// Disable this setting, so that OIDC flow in Glow is not federated with the WTG B2C
			var sqlInsert = $@"
INSERT INTO dbo.STMDATA
(
	SD_PK,
	SD_Name,
	SD_Type,
	SD_IsLogged,
	SD_BinaryValue,
	SD_IsCancelled,
	SD_PreserveTestValue,
	SD_SystemCreateTimeUtc,
	SD_SystemCreateUser,
	SD_SystemLastEditTimeUtc,
	SD_SystemLastEditUser
)
SELECT
	NEWID(),
	'IsOIDCFederatedWithWTG',
	'BIN',
	1,
	CAST(N'False' AS VARBINARY(MAX)),
	0,
	0,
	'2023-09-03 00:11:00',
	'E',
	'2023-09-03 00:11:00',
	'E'
WHERE NOT EXISTS 
(
	SELECT 1 FROM dbo.STMDATA WHERE SD_Name = 'IsOIDCFederatedWithWTG'
)";

			connection.ExecuteNonQuery(sqlInsert);
		}

		void SaveTableUsingBulkCopy(DataTable table, AdminConnection connection)
		{
			if (table == null || table.Rows.Count == 0 || string.IsNullOrWhiteSpace(table.TableName))
			{
				var message = $@"There is no record to insert into the table {nameof(table.TableName)}";
				logger.RecordInfo($"{nameof(table.TableName)} : {message}");
				return;
			}

			using (var bulkCopy = new SqlServerBulkCopy(((IDbConnectionInternals)connection).SqlConnection))
			{
				bulkCopy.DestinationTableName = table.TableName;

				foreach (DataColumn column in table.Columns)
				{
					bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
				}

				try
				{
					bulkCopy.WriteToServer(table);
				}
				catch (InvalidOperationException ex)
				{
					var moreInformativeMessage = "BulkCopy failure: " + table.TableName + " (" + string.Join(", ", table.Columns) + ")";
					logger.RecordInfo($"{moreInformativeMessage} : {ex.Message}");
					throw new InvalidOperationException(moreInformativeMessage, ex);
				}
			}
		}

		string BuildSystemPackage(TestedShelfDeploymentOptions options, string binPath)
		{
			var buildPath = Directory.GetDirectories(options.SystemPackageDeploymentPath)
				.FirstOrDefault(p => Path.GetFileName(p).StartsWith(ReleaseInfo.Instance.ReleaseRing, OrdinalIgnoreCase)
					&& File.Exists(Path.Combine(p, "ArchiveComplete")));
			if (buildPath == null)
			{
				throw new InvalidOperationException("No archived build could be found to generate system package.");
			}
			else
			{
				var packageBuilder = new RuntimePackageBuilder(buildPath, Path.Combine(binPath, "package"));
				packageBuilder.Build(options.ClientCode, true);
				return packageBuilder.LastPackagePath;
			}
		}

		void UploadPackage(TestedShelfDeploymentOptions options, string packagePath, string systemPackagePath)
		{
			using (var connection = Db.NewAdminConnection(options.SqlServer, options.DatabaseName))
			{
				if (options.PackageStatus == "CUR")
				{
					connection.ExecuteNonQuery("update dbo.StmUpgrade set SZ_Status = 'APL' where SZ_Status = 'CUR'");
				}

				var sqlConnection = ((IDbConnectionInternals)connection).ADOConnection;
				var upgradeManager = new SqlUpgradeManager(new UpgradeSqlContext(sqlConnection, sqlConnection.DataSource, sqlConnection.Database));

				if (options.IncludeSystemPackage)
				{
					upgradeManager.UploadUpgradePackage(systemPackagePath, options.PackageStatus, "AutoDeployTestedShelf", null);
					upgradeManager.UploadUpgradePackage(packagePath, "RDY", "AutoDeployTestedShelf", null);
				}
				else
				{
					upgradeManager.UploadUpgradePackage(packagePath, options.PackageStatus, "AutoDeployTestedShelf", null);
				}
			}
		}

		void SetExpectedClientDllRegistryItems(TestedShelfDeploymentOptions options)
		{
			if (!string.IsNullOrEmpty(options.ClientCode))
			{
				using (var connection = Db.NewAdminConnection(options.SqlServer, options.DatabaseName))
				{
					RegistryAccess.ExpectedClientDll.SaveValue("ZClient" + options.ClientCode, connection);
					RegistryAccess.ClientDocumentName.SaveValue(options.ClientCode, connection);
				}
			}
		}

		void WarmUpVersionBrokers(TestedShelfDeploymentOptions options)
		{
			if (options.DeployWinzor)
			{
				using (logger.RecordTask("Warm up VersionBrokers"))
				{
					var url = $"https://{options.BlazorUrlAuthority}/";
					var client = new HttpClient();
					var start = ZDateTime.Now;
					while (true)
					{
						if (options.VersionBrokerWarmupTimeout < ZDateTime.Now - start)
						{
							logger.RecordInfo("Warm up VersionBrokers time out.");
							break;
						}

						try
						{
							var resp = client.GetAsync(url).GetAwaiter().GetResult();
							if (resp.StatusCode == HttpStatusCode.OK || resp.StatusCode == HttpStatusCode.Unauthorized)
							{
								logger.RecordInfo("Warm up VersionBrokers successfully.");
								break;
							}
						}
						catch (Exception ex)
						{
							logger.RecordInfo(ex.Message);
						}
						Thread.Sleep(500);
					}
				}
			}
		}

		void SetBlazorRegistryItems(TestedShelfDeploymentOptions options)
		{
			if (options.DeployWinzor)
			{
				using (var connection = Db.NewAdminConnection(options.SqlServer, options.DatabaseName))
				{
					SetRegistryValueString(connection, "BlazorUrl", options.BlazorUrl);
				}
			}
		}

		void SetDisableCdcRegistryItem(TestedShelfDeploymentOptions options, bool shouldDisableCdc)
		{
			using (var connection = Db.NewAdminConnection(options.SqlServer, options.DatabaseName))
			{
				SetRegistryValueBool(connection, nameof(DbRegistry.BiDisableChangeDataCapture), shouldDisableCdc);
			}
		}

		void SetTestRigOrigin(TestedShelfDeploymentOptions options)
		{
			if (!string.IsNullOrEmpty(options.Origin))
			{
				using (var connection = Db.NewAdminConnection(options.SqlServer, options.DatabaseName))
				{
					SetRegistryValueString(connection, "TEST_RIG_ORIGIN", options.Origin);
				}
			}
		}

		void ProcessRegistryEntries(TestedShelfDeploymentOptions options)
		{
			if (options.RegistryEntries.Count > 0)
			{
				using (var connection = Db.NewAdminConnection(options.SqlServer, options.DatabaseName))
				{
					foreach (var entry in options.RegistryEntries)
					{
						if (entry.Value is bool boolValue)
						{
							SetRegistryValueBool(connection, entry.Key, boolValue);
						}
						else if (entry.Value is string stringValue)
						{
							SetRegistryValueString(connection, entry.Key, stringValue);
						}
						else if (entry.Value is string[] stringArrayValue)
						{
							SetRegistryValueStringArray(connection, entry.Key, stringArrayValue);
						}
						else
						{
							throw new InvalidOperationException($"DataType {entry.Value.GetType().Name} is unsupported");
						}
					}
				}
			}
		}

		internal void SetRegistryValueString(DbConnection connection, string name, string value) => SetRegistryValue(connection, name, value, "STR");
		internal void SetRegistryValueInteger(DbConnection connection, string name, int value) => SetRegistryValue(connection, name, value, "INT");
		internal void SetRegistryValueBool(DbConnection connection, string name, bool value) => SetRegistryValue(connection, name, value ? bool.TrueString : bool.FalseString, "BOL");
		internal void SetRegistryValueStringArray(DbConnection connection, string name, string[] value)
		{
			var stringBuilder = new StringBuilder();
			using (var output = new StringWriter(stringBuilder))
			using (var writer = XmlWriter.Create(output))
			{
				new DataContractSerializer(typeof(string[])).WriteObject(writer, value);
				writer.Flush();
				var result = stringBuilder.ToString();
				SetRegistryValue(connection, name, result, "");
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void SetRegistryValue(DbConnection connection, string name, object value, string dataTypeCode)
		{
			var insertCommand = @"
				DELETE FROM dbo.StmData WHERE SD_NAME = @name;

				INSERT INTO dbo.StmData (SD_PK, SD_Name, SD_BinaryValue, SD_Type, SD_IsCancelled, SD_IsLogged)
				VALUES (newid(), @name, convert(varbinary(max), @value), @dataTypeCode, 0, 0)";

			using var command = connection.Command(insertCommand);
			command.AddParameter("@name", SqlDbType.VarChar, name);
			command.AddParameter("@value", SqlDbType.NVarChar, value);
			command.AddParameter("@dataTypeCode", SqlDbType.VarChar, dataTypeCode);

			command.ExecuteNonQuery();
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		object GetRegistryValue(DbConnection connection, string name)
		{
			var selectCommand = @"select cast(SD_BinaryValue as nvarchar(max)) from dbo.StmData where SD_Name = @name";

			using var command = connection.Command(selectCommand);
			command.AddParameter("@name", SqlDbType.VarChar, name);

			return command.ExecuteScalar();
		}

		[SuppressMessage("Microsoft.Security", "CA2100:ReviewSqlQueriesForSecurityVulnerabilities", Justification = "Database name cannot be parameterized")]
		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		protected void RestoreTestDatabase(TestedShelfDeploymentOptions options)
		{
			var cacheHelper = GetCacheHelper(options.SqlServer);
			cacheHelper.Verify = cachedFilePath => VerifyBackup(options.SqlServer, cachedFilePath);

			var backupPath = cacheHelper.GetFile(options.RestoreFromBackup, LocalCachingTimeout) ??
							 options.RestoreFromBackup;

			var backupInfo = DbFileInfoCollection.GetDbFileInfoCollectionFromBackup(options.SqlServer, backupPath, restoreDbName: options.DatabaseName);
			using (var connection = Db.NewAdminConnection(options.SqlServer, Db.SqlMasterDb))
			{
				DropDatabase(connection, options.DatabaseName, logger);

				using (var command = connection.Command(string.Empty))
				{
					var restoreCmdBuilder = new StringBuilder();
					restoreCmdBuilder.AppendFormat(CultureInfo.InvariantCulture, "RESTORE DATABASE [{0}] FROM DISK = @file WITH REPLACE, KEEP_CDC", options.DatabaseName);
					command.AddParameter("file", SqlDbType.VarChar, backupPath);

					int i = 0;
					foreach (DbFileInfo dbFile in backupInfo)
					{
						restoreCmdBuilder.AppendFormat(CultureInfo.InvariantCulture, ", MOVE @logicalFile{0} TO @physicalFile{0}", i);
						command.AddParameter("logicalFile" + i, SqlDbType.VarChar, dbFile.LogicalName);
						command.AddParameter("physicalFile" + i, SqlDbType.VarChar, Path.Combine(dbFile.FileType == DbFileInfo.FileTypeLog ? options.SqlServerLogFilePath : options.SqlServerDataFilePath, options.DatabaseName + dbFile.FilePathSuffix));
						i++;
					}
					restoreCmdBuilder.AppendLine();
					restoreCmdBuilder.AppendFormat(CultureInfo.InvariantCulture, "ALTER DATABASE [{0}] SET MULTI_USER;", options.DatabaseName);
					restoreCmdBuilder.AppendLine();
					restoreCmdBuilder.AppendFormat(CultureInfo.InvariantCulture, "ALTER DATABASE [{0}] SET TRUSTWORTHY ON;", options.DatabaseName);

					command.CommandText = restoreCmdBuilder.ToString();
					command.CommandTimeout = DbCommand.Timeout.Minutes(20).Value;
					command.ExecuteNonQuery();
				}
				CheckCdcAndBiServerDetails(connection, options);
			}

			cacheHelper.CleanupOldCacheFiles();
		}

		protected void SetupMainDatabaseForAlwaysOn(TestedShelfDeploymentOptions options)
		{
			using (var connection = Db.NewAdminConnection(options.SqlServer, Db.SqlMasterDb))
			{
				connection.ExecuteNonQuery($"ALTER DATABASE {options.DatabaseName.QuoteName()} SET RECOVERY FULL;");

				var backupFilePath = options.RegistryEntries["BackupFilePath"] as string;
				var dbBackupSharedPath = Path.Combine(backupFilePath, $"{options.DatabaseName}.bak");
				var logBackupSharedPath = Path.Combine(backupFilePath, $"{options.DatabaseName}.trn");

				// create database backup
				connection.ExecuteNonQuery(
					$"BACKUP DATABASE {options.DatabaseName.QuoteName()} TO DISK = @backupSharedPath WITH INIT",
					cmd => cmd.AddParameter("@backupSharedPath", SqlDbType.NVarChar, dbBackupSharedPath.Length, dbBackupSharedPath));

				// create log backup
				connection.ExecuteNonQuery(
					$"BACKUP LOG {options.DatabaseName.QuoteName()} TO DISK = @backupSharedPath WITH INIT",
					cmd => cmd.AddParameter("@backupSharedPath", SqlDbType.NVarChar, logBackupSharedPath.Length, logBackupSharedPath));

				// get values to create availability group
				// get dns name
				var dnsName = BuildDeployerHelper.GetSqlServerDns(connection);

				// get hadr endpoint port
				var hadrPort = BuildDeployerHelper.GetSqlServerHadrPort(connection);

				// get server name
				var serverName = BuildDeployerHelper.GetServerName(connection);

				var secondaryServerName = string.Empty;
				string secondaryDnsName = null;
				var secondaryhadrPort = hadrPort;
				using (var secondaryConnection = Db.NewAdminConnection(options.AONSecondarySqlServer, Db.SqlMasterDb))
				{
					// get dns name
					secondaryDnsName = BuildDeployerHelper.GetSqlServerDns(secondaryConnection);

					// get hadr endpoint port
					secondaryhadrPort = BuildDeployerHelper.GetSqlServerHadrPort(secondaryConnection);

					//get server name
					secondaryServerName = BuildDeployerHelper.GetServerName(secondaryConnection);

					// create an availability group
					connection.ExecuteNonQuery(
					$@"
CREATE AVAILABILITY GROUP {options.AONGroupName.QuoteName()}
WITH (
	AUTOMATED_BACKUP_PREFERENCE = SECONDARY
	, DB_FAILOVER = OFF
	, DTC_SUPPORT = NONE
	, REQUIRED_SYNCHRONIZED_SECONDARIES_TO_COMMIT = 0
)
FOR DATABASE {options.DatabaseName.QuoteName()}
REPLICA ON
{serverName.QuoteName('\'')}
WITH (
	ENDPOINT_URL = {$"TCP://{dnsName}:{hadrPort}".QuoteName('\'')}
	, FAILOVER_MODE = MANUAL
	, AVAILABILITY_MODE = SYNCHRONOUS_COMMIT
	, BACKUP_PRIORITY = 50
	, SEEDING_MODE = MANUAL
	, SECONDARY_ROLE(ALLOW_CONNECTIONS = NO)
)
, {secondaryServerName.QuoteName('\'')}
WITH (
	ENDPOINT_URL = {$"TCP://{secondaryDnsName}:{secondaryhadrPort}".QuoteName('\'')}
	, FAILOVER_MODE = MANUAL
	, AVAILABILITY_MODE = SYNCHRONOUS_COMMIT
	, BACKUP_PRIORITY = 50
	, SEEDING_MODE = MANUAL
	, SECONDARY_ROLE(ALLOW_CONNECTIONS = NO)
)");

					// restore databse
					secondaryConnection.ExecuteNonQuery(
						$"RESTORE DATABASE {options.DatabaseName.QuoteName()} FROM DISK = @backupSharedPath WITH NORECOVERY",
						cmd => cmd.AddParameter("@backupSharedPath", SqlDbType.NVarChar, dbBackupSharedPath.Length, dbBackupSharedPath));

					// restore log
					secondaryConnection.ExecuteNonQuery(
						$"RESTORE LOG {options.DatabaseName.QuoteName()} FROM DISK = @backupSharedPath WITH NORECOVERY",
						cmd => cmd.AddParameter("@backupSharedPath", SqlDbType.NVarChar, dbBackupSharedPath.Length, logBackupSharedPath));

					secondaryConnection.ExecuteNonQuery($@"
ALTER AVAILABILITY GROUP {options.AONGroupName.QuoteName()} JOIN;
ALTER DATABASE {options.DatabaseName.QuoteName()} SET HADR AVAILABILITY GROUP =  {options.AONGroupName.QuoteName()}
");
				}
			}
		}

		void CheckCdcAndBiServerDetails(DbConnection connection, TestedShelfDeploymentOptions options)
		{
			if (connection.DatabaseExists(options.DatabaseName))
			{
				using (((ICurrentDbControl)connection).UseDatabase(options.DatabaseName))
				{
					DisableCdcIfNotRequired(options);
					RemoveBiServerDetailsFromRegistry(connection);
				}
			}
		}

		void DisableCdcIfNotRequired(TestedShelfDeploymentOptions options)
		{
			var shouldDisableCdc = !options.EnableAudit && string.IsNullOrEmpty(options.DataWarehouseServer);
			SetDisableCdcRegistryItem(options, shouldDisableCdc);
		}

		void RemoveBiServerDetailsFromRegistry(DbConnection connection)
		{
			var sqlText =
@"DECLARE @sqlText NVARCHAR(MAX);
IF EXISTS (SELECT NULL FROM sys.tables WHERE name = 'StmData')
BEGIN
	SET @sqlText = N'DELETE FROM dbo.StmData WHERE SD_Name IN (''BiServers'', ''BiAuditServer'', ''BiDataWarehouseServer'', ''BiAnalysisServer'', ''BiSsrsWebServiceUrl'', ''BiPowerBiWebPortalUrl'')'
	EXEC(@sqlText)
END";
			connection.ExecuteNonQuery(sqlText);
		}

		void InstallWebApplications(TestedShelfDeploymentOptions options)
		{
			if (options.WebSites?.Length > 0)
			{
				using (logger.RecordTask("Uninstalling web applications"))
				{
					new RemoteUninstallWeb().Invoke(options.WebServer, options.WebServerInstallPath, options.SqlServer, options.DatabaseName, options.WebDomain, !options.PermitDuplicateWebSites, logger, options.Verbose);
				}
				using (logger.RecordTask("Installing web applications"))
				{
					new RemoteInstallWeb().Invoke(options.WebServer, options.WebServerInstallPath, options.SqlServer, options.DatabaseName, options.WebDomain, options.WebSites, !options.PermitDuplicateWebSites, options.ClientCode, logger, options.Verbose);
				}
			}
		}

		void CreateIcon(TestedShelfDeploymentOptions options)
		{
			const int CargoWiseOneRDPAndWinzorFlag = 10; // We should set the Flags to CargoWiseOneRDPAndWinzor with Winzor. Eventually we may want Winzor only but now it will be helpful to have both icons to compare behaviour between Winforms and Winzor
			string ou;
			if (options.ServiceTasks)
			{
				ou = "LDAP://OU=OrchestratedSH0,OU=ASPAC,OU=Applications,OU=root,DC=sand,DC=wtg,DC=zone";
			}
			else
			{
				ou = "LDAP://CN=SH0,CN=WiseTech Global,CN=Program Data,DC=sand,DC=wtg,DC=zone";
			}
			using (var container = new System.DirectoryServices.DirectoryEntry(ou))
			{
				var existingInstance = cargoWiseOneInstanceClass.FindInstance(options.IconName, new DirectorySearchOptions() { DomainName = "sand.wtg.zone" });
				if (existingInstance == null)
				{
					try
					{
						using (var instance = cargoWiseOneInstanceClass.AddNewInstance(options.IconName, options.SqlServer, options.DatabaseName, container))
						{
							if (options.DeployWinzor)
							{
								instance.Flags = CargoWiseOneRDPAndWinzorFlag;
								instance.BlazorUrlAuthority = options.BlazorUrlAuthority;
								instance.CommitChanges();
							}
						}
						logger.RecordInfo("Created instance " + options.IconName);
					}
					catch (DirectoryServicesException ex) when (ex.Message.Contains("already exists", OrdinalIgnoreCase))
					{
						logger.RecordInfo("Instance " + options.IconName + " already exists");
					}
				}
				else
				{
					using (var entry = existingInstance.GetDirectoryEntry())
					{
						entry.ServerName = options.SqlServer;
						entry.DatabaseName = options.DatabaseName;
						if (options.DeployWinzor)
						{
							entry.Flags = CargoWiseOneRDPAndWinzorFlag;
							entry.BlazorUrlAuthority = options.BlazorUrlAuthority;
						}
						entry.CommitChanges();
						logger.RecordInfo("Updated instance " + options.IconName);
					}
				}
			}
		}

		internal void RegisterProduct(string registrationEnterpriseCode, string registrationSqlServer, string databaseName)
		{
			try
			{
				using (Db.DisposableActionForDbConnection())
				{
					var registrationCode = ProductKeyService.Instance.Value.GetInternalTestKey(registrationEnterpriseCode, registrationSqlServer, databaseName);
					logger.RecordInfo("Registration Code: " + registrationCode);
					var result = ObjectFactory.Get<IProductRegistration>().Register(registrationCode, CancellationToken.None, DefaultRegistrationTimeoutMs);
					if (result != ProductRegistrationRegisterResult.OK)
					{
						throw new DeploymentFailedException($"Failed to register product: {result}");
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				logger.RecordInfo("Failed to register product: " + ex.ToString());
				throw;
			}
		}

		void LaunchDbUpgrade(TestedShelfDeploymentOptions options)
		{
			if (options.LaunchDbUpgrade)
			{
				using (logger.RecordTask($"Launching DbUpgrade on server {options.DbUpgradeServer}"))
				{
					var logFileUrl = $@"
						https://eye.wtg.ws/s/wisecloud-support/app/discover#/?
						_g=(filters:!(),time:(from:now-10d,to:now))&
						_a=(
							columns:!(host.hostname,message),
							filters:!(),
							hideChart:!f,
							index:'53ab8045-e2a0-4670-97bf-32dab413cb5c',
							interval:auto,
							query:(language:kuery,query:'host.hostname:{options.DbUpgradeServer.ToLower().Split('.')[0]}%20and%20log.file.path:*{options.DatabaseName}*'),
							sort:!(!('@timestamp',desc)))".Replace("\n", "").Replace("\r", "").Replace("\t", "").Replace(" ", "");

					logger.RecordInfo($"The upgrade process will log to UPG service task log file at [{logFileUrl}]");

					RemoteLaunchDbUpgrade.Invoke(logger, options.DbUpgradeServer, options.SqlServer, options.DatabaseName);
				}
			}

			if (options.ScheduleUPG)
			{
				using var scheduleUPGLog = logger.RecordTask("Scheduling DbUpgrade");
				using var connection = Db.NewAdminConnection(options.SqlServer, options.DatabaseName);
				using var command = connection.Command(Invariant($@"
IF OBJECT_ID('[dbo].[StmScheduleTask]') IS NOT NULL
BEGIN
	UPDATE [dbo].[StmScheduleTask]
	SET [S5_IsActive] = 1, [S5_NextScheduledPrintRunTimeUtc] = GetUtcDate()
	WHERE [S5_ScheduleType] = 'UPG' AND [S5_ParentTableCode] = 'SH'
END
IF OBJECT_ID('[dbo].[StmServiceTask]') IS NOT NULL
BEGIN
	UPDATE [dbo].[StmServiceTask]
	SET [SST_Active] = 1, [SST_NextRunTime] = ToDateTimeOffset(GetUtcDate(), 0)
	WHERE [SST_ServiceTaskCode] = 'UPG'
END"));
				command.ExecuteNonQuery();
			}
			else if (options.ShouldRestoreFromBackup)
			{
				using var disableUPGLog = logger.RecordTask("Disabling UPG service task");
				using var connection = Db.NewAdminConnection(options.SqlServer, options.DatabaseName);
				using var command = connection.Command(Invariant($@"
IF OBJECT_ID('[dbo].[StmScheduleTask]') IS NOT NULL
BEGIN
	UPDATE [dbo].[StmScheduleTask]
	SET [S5_IsActive] = 0
	WHERE [S5_ScheduleType] = 'UPG' AND [S5_ParentTableCode] = 'SH'
END
IF OBJECT_ID('[dbo].[StmServiceTask]') IS NOT NULL
BEGIN
	UPDATE [dbo].[StmServiceTask]
	SET [SST_Active] = 0
	WHERE [SST_ServiceTaskCode] = 'UPG'
END"));
				command.ExecuteNonQuery();
			}
		}

		void SetSingleRefDbNameRegistry(TestedShelfDeploymentOptions options)
		{
			var sRDbName = options.SingleRefDatabaseName;
			if (!string.IsNullOrEmpty(sRDbName))
			{
				using (logger.RecordTask($"Setting Single Reference Database Name: {sRDbName}"))
				{
					using var connection = Db.NewAdminConnection(options.SqlServer, options.DatabaseName);

					SetRegistryValueString(connection, DbRegistry.SingleRefDatabaseName.ItemName, sRDbName);
				}
			}
		}

		public void SetMajorTransformationVersion(TestedShelfDeploymentOptions options)
		{
			if (options.RewindTransformVersionNumber <= 0)
			{
				return;
			}

			using var connection = Db.NewAdminConnection(options.SqlServer, options.DatabaseName);

			var registryValue = GetRegistryValue(connection, DbRegistry.DatabaseMajorTransformationVersion.ItemName)?.ToString();
			if (!int.TryParse(registryValue, out int oldMajorTransformationVersion))
			{
				logger.RecordInfo("Missing or invalid registry value for the config DatabaseMajorTransformationVersion");
				return;
			}

			if (oldMajorTransformationVersion > options.RewindTransformVersionNumber)
			{
				var newMajorTransformationVersion = oldMajorTransformationVersion - options.RewindTransformVersionNumber;

				using (logger.RecordTask($"Rewinding Database Major Transformation Version from {oldMajorTransformationVersion} to {newMajorTransformationVersion}."))
				{
					SetRegistryValueInteger(connection, DbRegistry.DatabaseMajorTransformationVersion.ItemName, newMajorTransformationVersion);
				}
			}
		}

		void ResetProcessControllerHosts(TestedShelfDeploymentOptions options)
		{
			using var connection = Db.NewAdminConnection(options.SqlServer, options.DatabaseName);
			var sqlText =
				@"IF OBJECT_ID('[dbo].[StmServiceHost]') IS NOT NULL
					BEGIN
						DELETE [dbo].[StmServiceHost];
					END";

			connection.ExecuteNonQuery(sqlText);
		}

		[Obsolete("Use the IBuildDeployer2.TeardownTestedShelf overload specifying sourcePath and binPath")]
		public void TeardownTestedShelf(string buildConfiguration, string deploymentConfiguration, TaskInfo taskInfo)
		{
			throw new NotImplementedException();
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public void TeardownTestedShelf(string buildConfiguration, string deploymentConfiguration, string sourcePath, string binPath, TaskInfo taskInfo)
		{
			InitializeEnvironment();
			var options = new TestedShelfDeploymentOptions(taskInfo);
			logger.RecordInfo($"TestedShelfDeploymentOptions:" + JsonConvert.SerializeObject(options, Newtonsoft.Json.Formatting.Indented).Replace(System.Environment.NewLine + "  ", System.Environment.NewLine + "\t"));
			using (InitializeDb(options))
			{
				var tasks = new List<Task>();
				tasks.Add(Task.Run(() =>
				{
					if (options.Register || options.ShouldRestoreFromBackup)
					{
						using (var connection = Db.NewAdminConnection(options.SqlServer, Db.SqlMasterDb))
						{
							var databases = GetDatabasesToDrop(connection, options.DatabaseName, options.AONGroupName, options.SingleRefDatabaseName);

							if (options.Register && databases.Any(db => db.Equals(options.DatabaseName, OrdinalIgnoreCase)))
							{
								using (logger.RecordTask("Unregistering product"))
								{
									try
									{
										ObjectFactory.Get<IProductRegistration>().Unregister(CancellationToken.None, DefaultRegistrationTimeoutMs);
									}
									catch (Exception ex)
									{
										logger.RecordInfo("Unregister failed: " + ex.ToString());
									}
								}
							}
						}
					}
				}));

				tasks.Add(Task.Run(() =>
				{
					if (options.CreateIcon)
					{
						using (logger.RecordTask("Delete Icon"))
						{
							var instance = cargoWiseOneInstanceClass.FindInstance(options.IconName, new DirectorySearchOptions() { DomainName = "sand.wtg.zone" });
							if (instance != null)
							{
								using (var entry = instance.GetDirectoryEntry())
								{
									entry.Delete();
								}
							}
						}
					}
				}));

				tasks.Add(Task.Run(() =>
				{
					if (options.WebSites?.Length > 0)
					{
						const int retriesCount = 3;
						for (var i = 0; i < retriesCount; i++)
						{
							try
							{
								using (logger.RecordTask("Uninstalling web applications"))
								{
									new RemoteUninstallWeb().Invoke(options.WebServer, options.WebServerInstallPath, options.SqlServer, options.DatabaseName, options.WebDomain, !options.PermitDuplicateWebSites, logger, options.Verbose);
									break;
								}
							}
							catch
							{
							}
						}
					}
				}));

				Task.WaitAll(tasks.ToArray());

				var dropDbTask = Task.Run(() =>
				{
					if (options.ShouldRestoreFromBackup)
					{
						using (logger.RecordTask("Dropping Databases"))
						using (var connection = Db.NewAdminConnection(options.SqlServer, Db.SqlMasterDb))
						{
							/*The main database might be in single user mode.
							 * To drop audit/edw db, we need to get server from cw1 main db's registry,
							 * then it turns out that we need to kill other connections so that we are able to establish db connection to main db to read StmData table.*/
							DbConnectionKiller.KillOtherConnections(connection, options.DatabaseName);
							// Drop Bi Databases relies on the server names stored in the registry located at Main Db.
							DropBiDatabasesOnRemoteServer(options, connection, logger);

							var databases = GetDatabasesToDrop(connection, options.DatabaseName, options.AONGroupName, options.SingleRefDatabaseName);

							foreach (var databaseName in databases.Except(options.DatabaseName))
							{
								DropDatabase(connection, databaseName, logger);
							}

							if (databases.Contains(options.DatabaseName))
							{
								DropDatabase(connection, options.DatabaseName, logger);
							}

							DropLogins(connection, options.DatabaseName);
						}
					}

					var timeout = TimeSpan.FromMinutes(10d);
					using (logger.RecordTask("Global cleanup"))
					using (var cts = new CancellationTokenSource(timeout))
					{
						try
						{
							DropOrphanedUsersOnSharedDbs(options.SqlServer, cts.Token);
						}
						catch (OperationCanceledException)
						{
							logger.RecordInfo($"Dropping orphaned users did not finish after {timeout}");
						}
						catch (Exception ex)
						{
							logger.RecordInfo($"Dropping orphaned users failed: {ex}");
						}
					}
				});

				dropDbTask.Wait();

				var cleanUpAfterAlwaysOnTask = Task.Run(() =>
				{
					if (options.SetupAlwaysOn)
					{
						using (var connection = Db.NewAdminConnection(options.SqlServer, Db.SqlMasterDb))
						{
							using (logger.RecordTask("Dropping backup files"))
							{
								var backupsToDrop = BuildDeployerHelper.GetBackupsToDrop(connection, options.DatabaseName, options.SingleRefDatabaseName, options.AONGroupName);

								foreach (var backup in backupsToDrop)
								{
									using (logger.RecordTask($"Dropping backup file '{backup.FileName}' for database [{backup.DatabaseName}]."))
									{
										BuildDeployerHelper.DropBackupUsingDumpDevice(connection, backup.DatabaseName, backup.FileName);
									}
								}

								foreach (var database in backupsToDrop.Select(b => b.DatabaseName).Distinct())
								{
									using (logger.RecordTask($"Dropping backup history for database [{database}]."))
									{
										BuildDeployerHelper.DropBackupHistoryForDatabase(connection, database);
									}
								}
							}

							using (logger.RecordTask($"Dropping availability group from server '{connection.ServerName}'"))
							{
								connection.ExecuteNonQuery($@"
IF EXISTS(SELECT name FROM master.sys.availability_groups WHERE name = @agName)
	DROP AVAILABILITY GROUP {options.AONGroupName.QuoteName()};",
		cmd => cmd.AddParameter("@agName", SqlDbType.NVarChar, 128, options.AONGroupName));
							}

							using (var secondaryConnection = Db.NewAdminConnection(options.AONSecondarySqlServer, Db.SqlMasterDb))
							using (logger.RecordTask($"Dropping availability group from server '{secondaryConnection.ServerName}'"))
							{
								secondaryConnection.ExecuteNonQuery($@"
IF EXISTS(SELECT name FROM master.sys.availability_groups WHERE name = @agName)
	DROP AVAILABILITY GROUP {options.AONGroupName.QuoteName()};",
		cmd => cmd.AddParameter("@agName", SqlDbType.NVarChar, 128, options.AONGroupName));
							}
						}
					}
				});

				cleanUpAfterAlwaysOnTask.Wait();

				RunTaskAndWait("Cleanup Orphaned Web", TimeSpan.FromMinutes(5), () =>
				{
					if (options.WebSites?.Length > 0)
					{
						using (logger.RecordTask($"Cleanup orphaned web on {options.WebServer}."))
						{
							new RemoteCleanupOrphanedWeb().Invoke(options.WebServer, options.WebServerInstallPath, logger, options.Verbose);
						}
					}
				});

				logger.LogDeploymentInstanceID($"{options.SqlServer}_{options.DatabaseName}");
			}
		}

		void RunTaskAndWait(string description, TimeSpan timeout, Action action)
		{
			var cancellationTokenSource = new CancellationTokenSource();
			cancellationTokenSource.CancelAfter(timeout);

			var cleanupOrphanedWebTask = Task.Run(() => action.Invoke(), cancellationTokenSource.Token);

			try
			{
				cleanupOrphanedWebTask.Wait(cancellationTokenSource.Token);
			}
			catch (OperationCanceledException)
			{
				logger.RecordInfo($"{description} has timed out in {timeout}.");
			}
			catch (AggregateException aggregateException)
			{
				logger.RecordInfo($"{description} has encountered with exception {aggregateException}.");
			}
		}

		IDisposable InitializeDb(TestedShelfDeploymentOptions options)
		{
			if (!Db.ServerNameIsInitialized || !Db.DatabaseNameIsInitialized)
			{
				Db.InitializeDatabaseDetails(options.SqlServer, options.DatabaseName);
			}
			return Db.DisableSchemaVersionCheck();
		}

		void InitializeEnvironment()
		{
			var currentProvider = Env.GetCurrentProvider();
			if (currentProvider == null || currentProvider is INullEnvProvider)
			{
				Initialiser.InitialiseWinForms();
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "List of databases must be queried")]
		internal static IEnumerable<string> GetDatabasesToDrop(DbConnection connection, string databaseName, string availabilityGroupName, string singleRefDb)
		{
			var databases = new List<string>();
			var sharedAvailabilityGroupRefDbPrefix = RefDbTableNameResolver.SharedAvailabilityGroupRefDbPrefix;
			var getRelatedDbsSql = @"
SELECT
	name
FROM
	sys.databases
WHERE 1 = 2
	OR name = @name
	OR name LIKE CONCAT(@name, '[_]%')
	OR name LIKE CONCAT('DBUPG[_]NewTemplateDB[_]', @name)
	OR name LIKE CONCAT('DBUPG[_]NewTemplateDB[_]', @name, '[_]%')
	OR name LIKE CONCAT('DBUPG[_]PreSchemaUpgradeDb[_]', @name)
	OR name LIKE CONCAT('DBUPG[_]PreSchemaUpgradeDb[_]', @name, '[_]%')
	OR name LIKE CONCAT('DBUPG[_]DataCopyDb[_]', @name)
	OR name LIKE CONCAT('DBUPG[_]DataCopyDb[_]', @name, '[_]%')
";
			if (!string.IsNullOrEmpty(availabilityGroupName))
			{
				getRelatedDbsSql = $@"
{getRelatedDbsSql}
	OR name	LIKE CONCAT(@sharedAvailabilityGroupRefDbPrefix, '-%')
";
			}

			using var command = connection.Command(getRelatedDbsSql);
			command.AddParameter("name", SqlDbType.VarChar, databaseName);
			if (!string.IsNullOrWhiteSpace(availabilityGroupName))
			{
				command.AddParameter("sharedAvailabilityGroupRefDbPrefix", SqlDbType.VarChar, sharedAvailabilityGroupRefDbPrefix + availabilityGroupName);
			}

			if (!string.IsNullOrEmpty(singleRefDb))
			{
				command.CommandText += @"
OR (1 = 1
	AND name = @singleRefDbName
	AND name <> 'CW-RefDatabase'
)";

				command.AddParameter("singleRefDbName", SqlDbType.VarChar, singleRefDb);
			}

			using var reader = command.ExecuteReader();
			while (reader.Read())
			{
				databases.Add((string)reader["name"]);
			}

			return databases;
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		internal static void DropDatabase(AdminConnection connection, string databaseName, ITaskLogger logger)
		{
			if (AlwaysOn.IsDbPartOfAlwaysOn(connection, databaseName))
			{
				using (logger.RecordTask($"Dropping database [{databaseName}] already joined to AlwaysOn Availability Group."))
				{
					DropDatabasesJoinedToAg(connection, databaseName, logger);
				}
			}
			else
			{
				DropDatabaseCore(connection, databaseName, logger);
			}
		}

		static void DropDatabaseCore(AdminConnection connection, string databaseName, ITaskLogger logger)
		{
			logger.RecordInfo($"Dropping database [{databaseName}] from SQL Server [{connection.ServerName}]...");

			DbConnectionKiller.KillOtherConnections(connection, databaseName);

			var attemptsRemaining = 3;
			do
			{
				try
				{
					connection.ExecuteNonQuery(GetDropDbNotJoinedToAgSafelySql(databaseName));
					return;
				}
				catch (SqlException ex)
				{
					if (!connection.DatabaseExists(databaseName))
					{
						break;
					}

					var cannotDropMessage = Invariant($"Cannot drop the database '{databaseName}'");
					if (--attemptsRemaining == 0 ||
						!ex.Message.Contains(cannotDropMessage, Ordinal))
					{
						throw new DataException(cannotDropMessage, ex);
					}
				}
			} while (attemptsRemaining > 0);
		}

		internal static void DropBiDatabasesOnRemoteServer(TestedShelfDeploymentOptions options, AdminConnection connectionToMainDb, ITaskLogger logger)
		{
			string auditServer = null;
			string datawareHouseServer = null;
			if (connectionToMainDb.DatabaseExists(options.DatabaseName))
			{
				using (((ICurrentDbControl)connectionToMainDb).UseDatabase(options.DatabaseName))
				{
					auditServer = DbRegistry.BiAuditServer.LoadValue(connectionToMainDb);
					datawareHouseServer = DbRegistry.BiDataWarehouseServer.LoadValue(connectionToMainDb);
				}
			}

			var cleanupAudit = !string.IsNullOrEmpty(auditServer);
			var cleanupEdw = !string.IsNullOrEmpty(datawareHouseServer);

			if (cleanupAudit)
			{
				DropDatabasesOnRemoteServer(auditServer, options.DatabaseName, options.DatabaseName + Db.AuditDatabaseSuffix, logger);
			}
			if (cleanupEdw)
			{
				DropDatabasesOnRemoteServer(datawareHouseServer, options.DatabaseName, options.DatabaseName + Db.EdwDatabaseSuffix, logger);
			}
		}

		static void DropDatabasesOnRemoteServer(string serverName, string mainDbName, string dbName, ITaskLogger logger)
		{
			try
			{
				using (var connection = Db.NewAdminConnection(serverName, Db.SqlMasterDb))
				{
					DropDatabase(connection, dbName, logger);
					DropLogins(connection, mainDbName);
				}
			}
			catch (Exception e)
			{
				logger.RecordInfo($"Failed to drop database(s) {string.Join(",", dbName)} on remote server [{serverName}].{System.Environment.NewLine}{e.Message}");
			}
		}

		static string GetDropDbNotJoinedToAgSafelySql(string dbName)
		{
			return Invariant($@"
IF EXISTS (SELECT name FROM sys.databases WHERE name = '{dbName}')
BEGIN
	IF EXISTS(SELECT NULL FROM sys.databases WHERE name = '{dbName}' AND is_auto_update_stats_async_on = 1)
	BEGIN
		ALTER DATABASE [{dbName}] SET AUTO_UPDATE_STATISTICS_ASYNC OFF

		DECLARE @killCmd nvarchar(800) = ''
		SELECT @killCmd = @killCmd + 'KILL STATS JOB ' + STR(job_id) + ' '
			FROM sys.dm_exec_background_job_queue j
			INNER JOIN sys.databases d
			ON j.database_id = d.database_id
			WHERE d.name = '{dbName}'
			AND j.in_progress = 1
		EXEC sp_executesql @killCmd
	END

	ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE

	DROP DATABASE [{dbName}];
END
");
		}

		static void DropDatabasesJoinedToAg(AdminConnection primaryConnection, string databaseName, ITaskLogger logger)
		{
			var lockResult = ZArchitecture.AlwaysOnHelper.AlwaysOnHelper.RunActionWithAlwaysOnLock(primaryConnection, _ =>
			{
				logger.RecordInfo($"Always On app lock acquired.");

				var secondaryOnReplicas = AlwaysOn.GetAlwaysOnSecondaryReplicaNamesList(primaryConnection, databaseName);
				logger.RecordInfo($"Found {secondaryOnReplicas.Count} secondary replica(s): {string.Join(", ", secondaryOnReplicas)}");

				logger.RecordInfo($"Removing database [{databaseName}] from AlwaysOn AG...");
				if (!AlwaysOn.RemoveDatabaseFromAlwaysOnSetup(primaryConnection, databaseName))
				{
					throw new Exception($"Database [{databaseName}] cannot be removed from AlwaysOn AG.");
				}

				foreach (var secondaryReplicaName in secondaryOnReplicas)
				{
					using (var cts = new CancellationTokenSource(TimeSpan.FromMinutes(1d)))
					using (var secondaryConnection = Db.NewAdminConnection(secondaryReplicaName, Db.SqlMasterDb))
					{
						try
						{
							logger.RecordInfo($"Waiting for database on secondary replica [{secondaryReplicaName}] to get ready for drop...");

							while (true)
							{
								cts.Token.ThrowIfCancellationRequested();

								if (IsDatabaseReadyToDrop(secondaryConnection))
								{
									DropDatabaseCore(secondaryConnection, databaseName, logger);
									break;
								}

								Thread.Sleep(TimeSpan.FromSeconds(2d));
							}
						}
						catch (OperationCanceledException)
						{
							logger.RecordInfo($"Checking database RESTORING state expired. Skip dropping database from AlwaysOn replica [{secondaryReplicaName}].");
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							logger.RecordInfo($@"Failed to drop database due to exception:
{ex}

");
						}
					}
				}

				DropDatabaseCore(primaryConnection, databaseName, logger);
			});

			if (lockResult != LockedProcessResult.Completed)
			{
				throw new Exception($"Failed to get app lock for {databaseName} with lock result {lockResult}. Unable to drop database from Always On Availability Group.");
			}

			bool IsDatabaseReadyToDrop(DbConnection secondaryConn)
			{
				var dbDescription = secondaryConn.DatabaseStateDescription(databaseName);
				return string.IsNullOrWhiteSpace(dbDescription) || string.Equals(dbDescription, "RESTORING", OrdinalIgnoreCase);
			}
		}

		internal static void DropLogins(AdminConnection connection, string databaseName)
		{
			DropLoginsByPattern(connection, $"{databaseName}[_]%");
			DropLoginsByPattern(connection, $"EnterpriseDbUser[_]{databaseName}[_]%");
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		static void DropLoginsByPattern(AdminConnection connection, string loginPattern)
		{
			try
			{
				var dropLoginSql = $@"
DECLARE @sqlCmd VARCHAR(MAX) = '';

SELECT @sqlCmd = @sqlCmd +
'
DECLARE @session' + cast(row_number as varchar(3)) + ' INT;
DECLARE session_cursor' + cast(row_number as varchar(3)) + ' CURSOR FOR
	SELECT session_id
	FROM sys.dm_exec_sessions
	WHERE login_name = ' + quotename(name, '''') + ';

OPEN session_cursor' + cast(row_number as varchar(3)) + ';
FETCH NEXT FROM session_cursor' + cast(row_number as varchar(3)) + ' INTO @session' + cast(row_number as varchar(3)) + ';

WHILE @@FETCH_STATUS = 0
BEGIN
	EXEC(''KILL '' + @session' + cast(row_number as varchar(3)) + ');
	FETCH NEXT FROM session_cursor' + cast(row_number as varchar(3)) + ' INTO @session' + cast(row_number as varchar(3)) + ';
END;

CLOSE session_cursor' + cast(row_number as varchar(3)) + ';
DEALLOCATE session_cursor' + cast(row_number as varchar(3)) + ';

DROP LOGIN ' + quotename(name) + '; ' FROM (SELECT ROW_NUMBER() OVER (ORDER BY name) as row_number, name FROM sys.server_principals WHERE name LIKE @loginPattern) tbl;
IF (@sqlCmd != '') EXEC (@sqlCmd);";

				using (var command = connection.Command(dropLoginSql))
				{
					command.AddParameter("loginPattern", SqlDbType.VarChar, loginPattern);
					command.ExecuteNonQuery();
				}
			}
			catch (SqlException sqlException)
			{
				throw new Exception($@"Failed to drop logins from server {connection.ServerName}, with error: {sqlException.Number}, {sqlException.Message}
dropping logins like: '{loginPattern}'", sqlException);
			}
		}

		public void DeployOnDemand(string buildConfiguration, string deploymentConfiguration, string sourcePath, string binPath)
		{
			var configuration = DeploymentConfiguration.FromConfigurationString(deploymentConfiguration, binPath, sourcePath);
			var process = processFactory.Create(configuration);
			process.Deploy(logger);
		}

		public virtual string IBPLatestBuildArchivePath => @"\\cw1datfiles.wtg.zone\IBPLatestBuildArchive";
		public virtual string IBPPackageArchivePath => @"\\cw1datfiles.wtg.zone\CW1Packages";

		[SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods", Justification = "<Pending>")]
		void DropOrphanedUsersOnSharedDbs(string server, CancellationToken token)
		{
			foreach (var dbName in GetAllSharedDbs())
			{
				token.ThrowIfCancellationRequested();

				try
				{
					using (var dbConnection = Db.NewAdminConnection(server, dbName)) // connection might be killed by another teardown job, create a new per db thus.
					{
						dbConnection.RunLocked(
							"CleanUpOrphanedUsers",
							_ => CleanUpOrphanedUsersOnDb(),
							max_tries: 1,
							dbName: dbName);

						Queue<string> GetDroppingOrphanedUserSqls()
						{
							var sb = new StringBuilder();

							var sqls = new Queue<string>();
							dbConnection.ExecuteReader(@"
WITH Users (UserName, UserID)
AS
(
	SELECT
		UserName = dp.name,
		UserId = dp.principal_id
	FROM sys.database_principals AS dp
		LEFT JOIN sys.server_principals AS sp ON dp.sid = sp.sid
	WHERE 1 = 1
		AND DB_ID
		(
			CASE
			WHEN CHARINDEX('_CargoWiseWriterLogin', dp.name) > 1
				THEN LEFT(dp.name, CHARINDEX('_CargoWiseWriterLogin', dp.name) - 1)
			WHEN CHARINDEX('_CargoWiseReaderLogin', dp.name) > 1
				THEN LEFT(dp.name, CHARINDEX('_CargoWiseReaderLogin', dp.name) - 1)
			WHEN CHARINDEX('_RestrictedWriterLogin', dp.name) > 1
				THEN LEFT(dp.name, CHARINDEX('_RestrictedWriterLogin', dp.name) - 1)
			WHEN CHARINDEX('_RestrictedReaderLogin', dp.name) > 1
				THEN LEFT(dp.name, CHARINDEX('_RestrictedReaderLogin', dp.name) - 1)
			WHEN CHARINDEX('_UnrestrictedWriterLogin', dp.name) > 1
				THEN LEFT(dp.name, CHARINDEX('_UnrestrictedWriterLogin', dp.name) - 1)
			WHEN CHARINDEX('EnterpriseDbUser_', dp.name) = 1
				THEN SUBSTRING(dp.name, 18, CHARINDEX('_', dp.name, 18) - 18)
			ELSE 'master' -- if user doesn't belong to CW1, use 'master' as master always exists and then this user won't be included in teardown
			END
		) IS NULL
		AND sp.sid IS NULL
		AND dp.authentication_type_desc = 'INSTANCE'
),
PermissionGrantees (UserName, AggregatedRoles)
AS
(
	SELECT
		UserName        = QUOTENAME(u.UserName),
		AggregatedRoles = STRING_AGG(CONVERT(NVARCHAR(max), IIF(dp.grantee_principal_id IS NOT NULL, CONCAT('[', USER_NAME(dp.grantee_principal_id), ']'), '')), ',')
	FROM Users AS u
		LEFT JOIN sys.database_permissions AS dp ON dp.grantor_principal_id = u.UserID AND dp.type = 'IM'
	GROUP BY u.UserName
)
SELECT
	DroppingSql = CONCAT
	(
		IIF(pg.AggregatedRoles <> '',
			CONCAT('REVOKE IMPERSONATE ON USER::', pg.UserName, ' FROM ', pg.AggregatedRoles, ';'),
			''),
		'DROP USER IF EXISTS ', pg.UserName, ';'
	)
FROM PermissionGrantees AS pg
",
							(IDataRecord record) =>
							{
								sqls.Enqueue((string)record["DroppingSql"]);
							});
							return sqls;
						}

						void CleanUpOrphanedUsersOnDb()
						{
							var allOrphanedUsersDroppingSqls = GetDroppingOrphanedUserSqls();
							var originalCount = allOrphanedUsersDroppingSqls.Count;
							if (originalCount == 0)
							{
								return;
							}

							using (logger.RecordTask($"Dropping orphaned users on [{dbName}]"))
							{
								logger.RecordInfo($"Found [{allOrphanedUsersDroppingSqls.Count}] orphaned users");

								try
								{
									var loggingStopWatch = Stopwatch.StartNew();

									while (allOrphanedUsersDroppingSqls.Count > 0)
									{
										token.ThrowIfCancellationRequested();

										if (loggingStopWatch.Elapsed >= TimeSpan.FromMinutes(1d))
										{
											logger.RecordInfo($"[{allOrphanedUsersDroppingSqls.Count}] users remain");
											loggingStopWatch.Restart();
										}

										var droppingSql = allOrphanedUsersDroppingSqls.Dequeue();

										try
										{
											dbConnection.ExecuteNonQuery(droppingSql);
										}
										catch (SqlException ex)
										{
											logger.RecordInfo($@"Dropping orphaned users failed: {ex.Message}
-------------------- sql --------------------
{droppingSql}");
											return;
										}
									}
								}
								finally
								{
									logger.RecordInfo($"[{originalCount - allOrphanedUsersDroppingSqls.Count}] users have been dropped, [{allOrphanedUsersDroppingSqls.Count}] remain");
								}
							}
						}
					}
				}
				catch (SqlException ex)
				{
					logger.RecordInfo($"Dropping orphaned users from database [{dbName}] failed: {ex}");
				}
			}

			IEnumerable<string> GetAllSharedDbs()
			{
				var dbs = new List<string>();

				using (var connection = Db.NewAdminConnection(server, Db.SqlMasterDb))
				{
					connection.ExecuteReader(
						@"
SELECT
	name
FROM sys.databases
WHERE 1 = 2
	OR name = 'CW-RefDatabase'
	OR name like 'CW-RefDb-%'
",
						(IDataRecord record) =>
						{
							dbs.Add((string)record["name"]);
						});
				}

				return dbs;
			}
		}

		#region Cache Support

		protected virtual CacheHelper GetCacheHelper(string sqlServer) =>
			new CacheHelper(sqlServer, LocalDatabaseCachePath, logger);

		protected virtual bool VerifyBackup(string sqlServer, string backupPath)
		{
			if (!File.Exists(backupPath))
			{
				return false;
			}

			var query = $"RESTORE VERIFYONLY FROM DISK = '{backupPath}'";

			try
			{
				using (var connection = Db.NewAdminConnection(sqlServer, "master"))
				using (var cmd = connection.Command(query))
				{
					_ = cmd.ExecuteNonQuery();
					return true;
				}
			}
			catch (Exception e)
			{
				logger.RecordInfo($"Verify backup failed for {backupPath}: {e.Message}");
				return false;
			}
		}

		protected virtual string LocalDatabaseCachePath => "SQL_Backup_Cache";

		protected virtual TimeSpan LocalCachingTimeout => TimeSpan.FromMinutes(10);

		#endregion

		readonly ITaskLogger logger;
		readonly IDeploymentProcessFactory processFactory;
		readonly ICargoWiseOneInstanceClass cargoWiseOneInstanceClass;
		readonly IDirectorySearcher directorySearcher;
		readonly EdiProdServiceTaskNudgeClient serviceTaskNudgeClient;
		public const int DefaultRegistrationTimeoutMs = 60 * 1000;
	}
}

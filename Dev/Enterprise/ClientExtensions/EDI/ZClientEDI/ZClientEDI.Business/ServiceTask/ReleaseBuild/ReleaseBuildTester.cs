using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using CargoWise.Data;
using CargoWise.IO;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Integration;
using Enterprise.Upgrades;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.ServiceTask
{
	public class ReleaseBuildTester : IReleaseBuildTester
	{
		public ReleaseBuildTester(ReleaseBuild build, ILogger logger, CancellationToken cancellationToken)
		{
			this.build = build;
			this.logger = logger;
			this.cancellationToken = cancellationToken;
		}
		readonly ReleaseBuild build;
		readonly ILogger logger;
		readonly CancellationToken cancellationToken;

		protected virtual int RetryIntervalInSeconds => 120;
		protected virtual int MaxRetryTimes => 10;

		#region Run Deployment

		public bool RunDeployment()
		{
			bool result = false;
			var serverName = EDIDataRegistry.Instance.ReleaseBuildTestDbServerName.Value;
			var dbName = EDIDataRegistry.Instance.ReleaseBuildTestDbName.Value;
			var targetHost = EDIDataRegistry.Instance.ReleaseBuildTestWebAppHost.Value;

			logger?.Information($"Upgrade configuration - server: {serverName} | databse: {dbName} | target host: {targetHost}");
			if (!string.IsNullOrEmpty(serverName) && !string.IsNullOrEmpty(dbName) && !string.IsNullOrEmpty(targetHost))
			{
				var buildVersions = GetVersionsFromBuild();
				var needsUpgrade = !CompareBuildVersionsWithDbVersions(buildVersions, serverName, dbName);

				if (needsUpgrade)
				{
					logger?.Information("Starting upgrade");
					result = UploadBuildToTestDatabase(serverName, dbName);
					if (result)
					{
						result = RunRemoteUpgrade(serverName, dbName, targetHost);
					}
					if (result)
					{
						result = CheckRemoteUpgradeCompletion(buildVersions, serverName, dbName);
					}
				}
				else
				{
					logger?.Information($"Upgrade is not required - database {serverName} {dbName} versions match build {build.VersionNumber} versions");
					result = true;
				}
			}
			else
			{
				logger?.Information("Invalid upgrade configuration");
			}

			return result;
		}

		#region Get Versions

		Versions GetVersionsFromBuild()
		{
			var buildVersions = GetVersionsFromBuildCore();
			logger?.Information($"Checked build {build.VersionNumber} versions: {buildVersions}");
			return buildVersions;
		}

		protected virtual Versions GetVersionsFromBuildCore()
		{
			using (var tempFile = TempFile.New())
			{
				Assembly versionAssembly;
				File.Copy(build.HL_PackagePath, tempFile.Filename, true);
				using (var zip = new ZipFileCore(tempFile.Filename))
				using (var tempExtractDir = new TempDirectory())
				{
					try
					{
						zip.ExtractEntry("Distribution/Application/Enterprise.DbUpgrader.Resource.Version.dll", tempExtractDir);
						versionAssembly = Assembly.Load(File.ReadAllBytes(Path.Combine(tempExtractDir.DirectoryName, "Enterprise.DbUpgrader.Resource.Version.dll")));

						var schemaVersionType = versionAssembly.GetType("Enterprise.DbUpgrader.Resource.Version.SchemaVersion");
						var scriptVersionType = versionAssembly.GetType("Enterprise.DbUpgrader.Resource.Version.ScriptVersion");
						var dataVersionType = versionAssembly.GetType("Enterprise.DbUpgrader.Resource.Version.DataVersion");
						var transformVersionType = versionAssembly.GetType("Enterprise.DbUpgrader.Resource.Version.TransformationVersion");

						BindingFlags flags = BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.Static;

						var schemaVersion = schemaVersionType.GetField("Application", flags).GetValue(null).ToString();
						var scriptVersion = scriptVersionType.GetField("Application", flags).GetValue(null).ToString();
						var dataVersion = dataVersionType.GetField("Application", flags).GetValue(null).ToString();
						var transformVersion = transformVersionType.GetField("ApplicationNumber", flags).GetValue(null).ToString();

						var buildVersions = new Versions()
						{
							SchemaVerion = schemaVersion,
							ScriptVerion = scriptVersion,
							DataVerion = dataVersion,
							TransformationVerion = transformVersion,
						};

						return buildVersions;
					}
					finally
					{
						versionAssembly = null;
					}
				}
			}
		}

		Versions GetVersionsFromDb(string serverName, string dbName)
		{
			var dbVersions = GetVersionsFromDbCore(serverName, dbName);
			logger?.Information($"Checked database {serverName} {dbName} versions: {dbVersions}");
			return dbVersions;
		}

		protected virtual Versions GetVersionsFromDbCore(string serverName, string dbName)
		{
			using (var dbConnection = Db.NewAdminConnection(serverName, dbName))
			{
				var dbMajorSchemaVersion = DbRegistry.DatabaseMajorSchemaVersion.LoadValue(dbConnection);
				var dbMinorSchemaVersion = DbRegistry.DatabaseMinorSchemaVersion.LoadValue(dbConnection);

				var dbMajorScriptVersion = DbRegistry.DatabaseMajorScriptVersion.LoadValue(dbConnection);
				var dbMinorScriptVersion = DbRegistry.DatabaseMinorScriptVersion.LoadValue(dbConnection);

				var dbMajorDataVersion = DbRegistry.DatabaseSystemDataVersionMajor.LoadValue(dbConnection);
				var dbMinorDataVersion = DbRegistry.DatabaseSystemDataVersionMinor.LoadValue(dbConnection);

				var dbMajorTrasformationVersion = DbRegistry.DatabaseMajorTransformationVersion.LoadValue(dbConnection);
				var dbMinorTrasformationVersion = DbRegistry.DatabaseMinorTransformationVersion.LoadValue(dbConnection);

				return new Versions()
				{
					SchemaVerion = $"{dbMajorSchemaVersion}.{dbMinorSchemaVersion}",
					ScriptVerion = $"{dbMajorScriptVersion}.{dbMinorScriptVersion}",
					DataVerion = $"{dbMajorDataVersion}.{dbMinorDataVersion}",
					TransformationVerion = $"{dbMajorTrasformationVersion}.{dbMinorTrasformationVersion}",
				};
			}
		}

		#endregion Get Versions

		#region Upload Build

		bool UploadBuildToTestDatabase(string serverName, string dbName)
		{
			var buildVersion = build.VersionNumber.ToVersion();
			logger?.Information($"Checked build version number {buildVersion}");
			var dbVersion = GetDbCurrentVersionNumber(serverName, dbName);
			logger?.Information($"Checked database current version number {dbVersion}");

			if (dbVersion == null || buildVersion.CompareTo(dbVersion) > 0)
			{
				logger?.Information($"Starting uploading build to {serverName} {dbName}");
				var uploadResult = UploadBuildToTestDatabaseCore(serverName, dbName, buildVersion);
				if (uploadResult)
				{
					logger?.Information($"Build upload completed");
				}
				else
				{
					logger?.Information($"Build upload failed");
				}
				return uploadResult;
			}
			else if (buildVersion.CompareTo(dbVersion) == 0)
			{
				logger?.Information($"Database {serverName} {dbName} has current build uploaded");
				return true;
			}
			else
			{
				logger?.Information($"Database {serverName} {dbName} has more recent build uploaded");
				return false;
			}
		}

		protected virtual bool UploadBuildToTestDatabaseCore(string serverName, string dbName, Version buildVersion)
		{
			try
			{
				using (var dbConnection = Db.NewAdminConnection(serverName, dbName))
				{
					var sqlConnection = ((IDbConnectionInternals)dbConnection).ADOConnection;
					var upgradeManager = new SqlUpgradeManager(new UpgradeSqlContext(sqlConnection, sqlConnection.DataSource, sqlConnection.Database));
					dbConnection.ExecuteNonQuery("update dbo.StmUpgrade set SZ_Status = 'APL' where SZ_Status = 'CUR'");
					using (FileStream buildReader = File.OpenRead(build.HL_PackagePath))
					{
						upgradeManager.UploadUpgradePackage(buildReader, buildVersion, build.HL_ExeVersionDate.ToDateTime(), Enterprise.MasterFiles.Business.StmUpgrade.StmUpgradeStatus.CurrentVersion, "Uploaded by ReleaseBuildTester", null);
						return true;
					}
				}
			}
			catch (Exception ex)
			{
				logger?.Error("Error uploading build: " + ex.ToString());
				return false;
			}
		}

		protected virtual Version GetDbCurrentVersionNumber(string serverName, string dbName)
		{
			using (var dbConnection = Db.NewAdminConnection(serverName, dbName))
			{
				var sqlConnection = ((IDbConnectionInternals)dbConnection).ADOConnection;
				var upgradeManager = new SqlUpgradeManager(new UpgradeSqlContext(sqlConnection, sqlConnection.DataSource, sqlConnection.Database));
				var currentVersion = upgradeManager.QueryCurrentVersion();
				return currentVersion?.Version;
			}
		}

		#endregion Upload Build

		#region Remote Upgrade

		bool RunRemoteUpgrade(string serverName, string dbName, string targetHost)
		{
			bool result;
			ProcessStartInfo processInfo;

			try
			{
				processInfo = GetUpgradeCommandToRemote(serverName, dbName, targetHost);
			}
			catch (Exception ex)
			{
				logger?.Error($"Unable to get remote upgrade command", ex);
				return false;
			}

			var retries = MaxRetryTimes;
			do
			{
				logger?.Information($"Starting remote upgrade command {processInfo.FileName} {processInfo.Arguments}");
				result = RunRemoteUpgradeCore(processInfo);
				if (result)
				{
					logger?.Information($"Remote upgrade command completed");
					break;
				}
				else if (retries > 0)
				{
					cancellationToken.ThrowIfCancellationRequested();
					Thread.Sleep(TimeSpan.FromSeconds(10));
					logger?.Information($"Waiting for another run");
				}
			} while (--retries >= 0);

			if (!result)
			{
				logger?.Information($"Remote upgrade command failed");
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "These are application folder and command names.")]
		protected virtual ProcessStartInfo GetUpgradeCommandToRemote(string serverName, string dbName, string targetHost)
		{
			var binPath = EDIDataRegistry.Instance.ReleaseBuildTestRemoteCommandExecutablePath.Value;
			var paexecDir = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData), "WiseTech Global", "CargoWise One");
			var paexecTarget = Path.Combine(paexecDir, "PAExec.exe");
			if (!File.Exists(paexecTarget))
			{
				Directory.CreateDirectory(paexecDir);
				File.Copy(Path.Combine(binPath, "PAExec.exe"), paexecTarget);
			}
			var command = Path.Combine(System.Environment.GetEnvironmentVariable("ProgramW6432") ?? System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles), "WiseTech Global", "CargoWise", "CargoWise.Start.exe");
			var arguments = serverName + " " + dbName + " -ScheduledDbUpgrader -NoUI";
			var fullCommand = FormattableString.Invariant($@"\\{targetHost} -s -d ""{command}"" {arguments}");

			return new ProcessStartInfo(paexecTarget, fullCommand) { UseShellExecute = false };
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1078:Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.", Justification = "False alarm, not opening a file running a process on the local server")]
		protected virtual bool RunRemoteUpgradeCore(ProcessStartInfo processInfo)
		{
			using (var process = Process.Start(processInfo))
			{
				process.WaitForExit();
				if (process.ExitCode == 0)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		#endregion Remote Upgrade

		#region Check Upgrade Completion

		bool CheckRemoteUpgradeCompletion(Versions buildVersions, string serverName, string dbName)
		{
			var result = false;

			var retries = MaxRetryTimes;
			do
			{
				try
				{
					result = CompareBuildVersionsWithDbVersions(buildVersions, serverName, dbName);

					if (result)
					{
						logger?.Information($"Upgrade completed - database {serverName} {dbName} versions match build {build.VersionNumber} versions");
						break;
					}
					else
					{
						logger?.Information($"Upgrade not complete yet - database {serverName} {dbName} versions don't match build {build.VersionNumber}");
						cancellationToken.ThrowIfCancellationRequested();
						logger?.Information($"Waiting for next check in {RetryIntervalInSeconds} seconds");
						Thread.Sleep(TimeSpan.FromSeconds(RetryIntervalInSeconds));
					}
				}
				catch (Exception ex)
				{
					logger?.Information($"Upgrade not complete yet - database {serverName} {dbName} - error {ex.Message}");
					cancellationToken.ThrowIfCancellationRequested();
					logger?.Information($"Waiting for next check in {RetryIntervalInSeconds} seconds");
					Thread.Sleep(TimeSpan.FromSeconds(RetryIntervalInSeconds));
				}
			} while (--retries >= 0);

			if (!result)
			{
				logger?.Information($"Upgrade not complete - database {serverName} {dbName} versions don't match build {build.VersionNumber}");
			}

			return result;
		}

		bool CompareBuildVersionsWithDbVersions(Versions buildVersions, string serverName, string dbName)
		{
			var dbVersions = GetVersionsFromDb(serverName, dbName);
			return dbVersions != null && dbVersions.IsSameVersion(buildVersions);
		}

		#endregion Check Upgrade Completion

		#endregion Run Deployment

		#region Run Test

		public bool RunTest()
		{
			var result = false;

			var webAppUrl = EDIDataRegistry.Instance.ReleaseBuildTestWebAppUrl.Value;

			if (!string.IsNullOrEmpty(webAppUrl))
			{
				logger?.Information($"Starting web app health check on {webAppUrl}");
				var retries = MaxRetryTimes;
				do
				{
					try
					{
						var response = RunWebAppHealthCheck(webAppUrl);
						if (response.StatusCode == HttpStatusCode.OK)
						{
							var content = response.Content;
							if (content.Contains(EDIConstants.DatabaseConnectionStatus.Ok))
							{
								logger?.Information($"Web app is up and running");
								result = true;
								break;
							}
							else
							{
								logger?.Information($"Web app is up with issue - {content}");
							}
						}
						else
						{
							logger?.Information($"Web app is down with status {response.StatusCode}");
						}

						if (!result && retries > 0)
						{
							cancellationToken.ThrowIfCancellationRequested();
							logger?.Information($"Waiting for next check in {RetryIntervalInSeconds} seconds");
							Thread.Sleep(TimeSpan.FromSeconds(RetryIntervalInSeconds));
						}
					}
					catch (Exception ex)
					{
						logger?.Information($"Unable to run web app health check - {ex.Message}");
						cancellationToken.ThrowIfCancellationRequested();
						logger?.Information($"Waiting for next check in {RetryIntervalInSeconds} seconds");
						Thread.Sleep(TimeSpan.FromSeconds(RetryIntervalInSeconds));
					}
				} while (--retries >= 0);
			}
			else
			{
				logger?.Information($"Web app health check URL is blank");
			}

			if (!result)
			{
				logger?.Information($"Web app health check failed");
			}

			return result;
		}

		protected virtual (HttpStatusCode StatusCode, string Content) RunWebAppHealthCheck(string webAppUrl)
		{
			using (var client = new HttpClient())
			{
				var response = client.GetAsync(webAppUrl).ConfigureAwait(false).GetAwaiter().GetResult();
				var content = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				return (response.StatusCode, content);
			}
		}

		#endregion Run Test

		#region Versions

		protected class Versions
		{
			public string SchemaVerion { get; set; }
			public string ScriptVerion { get; set; }
			public string DataVerion { get; set; }
			public string TransformationVerion { get; set; }

			public bool IsSameVersion(Versions otherVersions)
			{
				return SchemaVerion == otherVersions.SchemaVerion
					&& ScriptVerion == otherVersions.ScriptVerion
					&& DataVerion == otherVersions.DataVerion
					&& TransformationVerion == otherVersions.TransformationVerion;
			}

			public override string ToString()
			{
				return $"Schema Version {SchemaVerion} | Script Version {ScriptVerion} | Data Version {DataVerion} | Transformation Version {TransformationVerion}";
			}
		}

		#endregion Class Version
	}
}

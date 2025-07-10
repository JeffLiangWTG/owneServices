using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Data;
using Dat.Integration.Deployment;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Dat.Implementation
{
	public class TestedShelfDeploymentOptions
	{
		[SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public TestedShelfDeploymentOptions(TaskInfo taskInfo)
		{
			RestoreFromBackup = Parse(nameof(RestoreFromBackup), taskInfo.TaskComments);
			if (!string.IsNullOrEmpty(RestoreFromBackup))
			{
				RestoreFromBackup = RestoreFromBackup.TrimStart('<').TrimEnd('>');
			}

			var configuredDatabaseName = Parse(nameof(DatabaseName), taskInfo.TaskComments);
			DatabaseName = configuredDatabaseName ?? "SH0" + SafeDatabaseName(taskInfo.ShelfName);
			SetupAlwaysOn = ParseBool(nameof(SetupAlwaysOn), taskInfo.TaskComments, false);
			AONSecondarySqlServer = Parse(nameof(AONSecondarySqlServer), taskInfo.TaskComments);
			AONGroupName = Parse(nameof(AONGroupName), taskInfo.TaskComments) ?? taskInfo.ShelfName;

			var configuredSqlServer = Parse(nameof(SqlServer), taskInfo.TaskComments);
			RegistryEntries = GetRegistryEntries(taskInfo.TaskComments);

			AnyOptionsSet = (SetupAlwaysOn && (!string.IsNullOrEmpty(RestoreFromBackup) && !string.IsNullOrEmpty(configuredSqlServer) && !string.IsNullOrEmpty(AONSecondarySqlServer) && RegistryEntries.TryGetValue("BackupFilePath", out var backupFilePath) && !string.IsNullOrEmpty(backupFilePath as string)))
				|| (!SetupAlwaysOn && (!string.IsNullOrEmpty(RestoreFromBackup) || !string.IsNullOrEmpty(configuredSqlServer) || !string.IsNullOrEmpty(configuredDatabaseName)));

			if (AnyOptionsSet)
			{
				if (SetupAlwaysOn)
				{
					SqlServer = configuredSqlServer;
				}
				else
				{
					SqlServer = configuredSqlServer ?? DefaultSqlServer;
				}

				if (!Db.DatabaseNameIsInitialized || !Db.ServerNameIsInitialized)
				{
					Db.InitializeDatabaseDetails(SqlServer, DatabaseName);
				}

				SqlServerDataFilePath = Parse(nameof(SqlServerDataFilePath), taskInfo.TaskComments) ?? DefaultSqlServerDataFilePath;
				SqlServerLogFilePath = Parse(nameof(SqlServerLogFilePath), taskInfo.TaskComments) ?? DefaultSqlServerLogFilePath;
				IconName = Parse(nameof(IconName), taskInfo.TaskComments) ?? InstanceNameFromDbName(DatabaseName);
				CreateIcon = ParseBool(nameof(CreateIcon), taskInfo.TaskComments, defaultValue: true);
				Register = ParseBool(nameof(Register), taskInfo.TaskComments, defaultValue: !string.IsNullOrEmpty(RestoreFromBackup));
				RegistrationEnterpriseCode = Parse(nameof(RegistrationEnterpriseCode), taskInfo.TaskComments) ?? DefaultRegistrationEnterpriseCode;
				RegistrationSqlServer = Parse(nameof(RegistrationSqlServer), taskInfo.TaskComments) ?? DefaultRegistrationSqlServer;

				IncludeSystemPackage = ParseBool(nameof(IncludeSystemPackage), taskInfo.TaskComments);
				SystemPackageDeploymentPath = Parse(nameof(SystemPackageDeploymentPath), taskInfo.TaskComments) ?? DefaultSystemPackageDeploymentPath;

				ClientCode = Parse(nameof(ClientCode), taskInfo.TaskComments);

				Origin = Parse(nameof(Origin), taskInfo.TaskComments);

				WebSites = (Parse(nameof(WebSites), taskInfo.TaskComments) ?? string.Empty).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();

				var requestedWebServer = Parse(nameof(WebServer), taskInfo.TaskComments);
				var requestedWebDomain = Parse(nameof(WebDomain), taskInfo.TaskComments);
				if (requestedWebServer == null && requestedWebDomain == null)
				{
					WebServer = DefaultWebServer;
					WebDomain = InstanceNameFromDbName(DatabaseName).ToLowerInvariant() + "." + DefaultWebRootDomain;
				}
				else if (requestedWebServer == null)
				{
					var matchedConfigurations = WebServerConfigurations.Where(w => requestedWebDomain.EndsWith(w.Domain, StringComparison.OrdinalIgnoreCase));
					if (matchedConfigurations.Any())
					{
						var selectedWebServerName = JenkinsPick(DatabaseName, "selected server name", matchedConfigurations.ToArray()).Name;
						WebDomain = requestedWebDomain;
						WebServer = selectedWebServerName;
					}
					else
					{
						throw new ArgumentException($"Cannot match WebDomain {requestedWebDomain} in known configurations [{string.Join(", ", WebServerConfigurations.Select(w => $"{w.Name}/*.{w.Domain}"))}] Please use known configurations or specify TestRigWebServer");
					}
				}
				else if (requestedWebDomain == null)
				{
					WebServer = requestedWebServer;
					var matchedConfiguration = AllWebServerConfigurations.SingleOrDefault(w => w.Name.Equals(requestedWebServer, StringComparison.OrdinalIgnoreCase));
					if (matchedConfiguration != null)
					{
						WebDomain = InstanceNameFromDbName(DatabaseName).ToLowerInvariant() + "." + matchedConfiguration.Domain;
					}
					else
					{
						WebDomain = InstanceNameFromDbName(DatabaseName).ToLowerInvariant() + "." + requestedWebServer;
					}
				}
				else
				{
					var isWebServerMatched = AllWebServerConfigurations.Any(w => w.Name.Equals(requestedWebServer, StringComparison.OrdinalIgnoreCase));
					var isWebDomainMatched = requestedWebDomain.EndsWith(KnownWebDomain, StringComparison.OrdinalIgnoreCase);
					if (!isWebServerMatched && isWebDomainMatched)
					{
						throw new ArgumentException($"Error with WebServer/WebDomain {requestedWebServer}/{requestedWebDomain}. WebServer does not match the WebDomain found in known configurations [{string.Join(", ", AllWebServerConfigurations.Select(w => $"{w.Name}/*.{w.Domain}"))}]");
					}

					WebServer = requestedWebServer;
					WebDomain = requestedWebDomain;
				}

				WebServerInstallPath = Parse(nameof(WebServerInstallPath), taskInfo.TaskComments) ?? DefaultWebServerInstallPath;

				LaunchDbUpgrade = ParseBool(nameof(LaunchDbUpgrade), taskInfo.TaskComments, defaultValue: true);
				DbUpgradeServer = Parse(nameof(DefaultDbUpgradeServer), taskInfo.TaskComments) ?? DefaultDbUpgradeServer;
				PermitDuplicateWebSites = ParseBool(nameof(PermitDuplicateWebSites), taskInfo.TaskComments);

				EnableAudit = ParseBool(nameof(EnableAudit), taskInfo.TaskComments);
				ServiceTasks = ParseBool(nameof(ServiceTasks), taskInfo.TaskComments, EnableAudit);

				EnableAnalysis = ParseBool(nameof(EnableAnalysis), taskInfo.TaskComments, false);
				AnalysisServer = Parse(nameof(AnalysisServer), taskInfo.TaskComments);

				DataWarehouseServer = Parse(nameof(DataWarehouseServer), taskInfo.TaskComments);

				ScheduleUPG = ParseBool(nameof(ScheduleUPG), taskInfo.TaskComments);
				PackageStatus = Parse(nameof(PackageStatus), taskInfo.TaskComments) ?? PackageStatusDefault;

				Verbose = ParseBool(nameof(Verbose), taskInfo.TaskComments);

				DeployWinzor = ParseBool(nameof(DeployWinzor), taskInfo.TaskComments);
				if (DeployWinzor)
				{
					RunWinzorE2ETests = ParseBool(nameof(RunWinzorE2ETests), taskInfo.TaskComments);
				}
				VersionBrokerWarmupTimeout = ParseTimeSpan(nameof(VersionBrokerWarmupTimeout), taskInfo.TaskComments, TimeSpan.FromMinutes(10));
				BlazorUrl = Parse(nameof(BlazorUrl), taskInfo.TaskComments) ?? DefaultBlazorUrl;

				AddStaffRecords = (Parse(nameof(AddStaffRecords), taskInfo.TaskComments) ?? string.Empty).Split(',')
																										 .Select(s => s.Trim())
																										 .Where(s => !s.IsNullOrEmpty())
																										 .Distinct()
																										 .ToArray();

				SingleRefDatabaseName = Parse(nameof(SingleRefDatabaseName), taskInfo.TaskComments);

				RewindTransformVersionNumber = ParseInt(nameof(RewindTransformVersionNumber), taskInfo.TaskComments);

				CalculateAdditionalSettings();

				VerifyClientWebSettings();
			}
		}

		void VerifyClientWebSettings()
		{
			var clientWeb = WebSites.Where(w => w.StartsWith("ZClientWeb")).ToArray();
			if (clientWeb.Length == 0)
			{
				return;
			}

			if (clientWeb.Length > 1)
			{
				throw new ArgumentException($"TestRigWebSites: Only one client web site is supported but multiple were requested [{string.Join(", ", clientWeb)}]");
			}

			if (string.IsNullOrEmpty(ClientCode))
			{
				throw new ArgumentException($"Cannot Deploy Website {clientWeb[0]} without matching TestRigClientCode");
			}

			var clientCode = clientWeb[0].Replace("ZClientWeb", string.Empty);
			if (!clientCode.Equals(ClientCode, StringComparison.Ordinal))
			{
				throw new ArgumentException($"TestRigWebSites: Client web site {clientWeb[0]} should end with client code {ClientCode}");
			}
		}

		void CalculateAdditionalSettings()
		{
			if (!RegistryEntries.ContainsKey(RegistrySettingGlowEnterpriseServicesRootUri) && WebSites.Contains(WebSiteServices))
			{
				RegistryEntries[RegistrySettingGlowEnterpriseServicesRootUri] = "https://" + WebDomain + "/" + WebSiteServices;
			}

			if (!RegistryEntries.ContainsKey(RegistrySettingGlowPortalsUri) && WebSites.Contains(WebSitePortalsName))
			{
				RegistryEntries[RegistrySettingGlowPortalsUri] = "https://" + WebDomain + "/" + WebSitePortalsPath;
			}

			if (!RegistryEntries.ContainsKey(RegistrySettingGlowServiceUri) && WebSites.Contains(WebSiteGlow))
			{
				RegistryEntries[RegistrySettingGlowServiceUri] = "https://" + WebDomain + "/" + WebSiteGlow;
			}

			if (EnableAudit && !RegistryEntries.Keys.Contains(RegistrySettingBiAuditServer))
			{
				RegistryEntries[RegistrySettingBiAuditServer] = SqlServer;
			}

			if (EnableAnalysis && !RegistryEntries.Keys.Contains(RegistrySettingBiAnalysisServer))
			{
				RegistryEntries[RegistrySettingBiAnalysisServer] = AnalysisServer;
			}
			if (!string.IsNullOrEmpty(DataWarehouseServer))
			{
				RegistryEntries[RegistrySettingBiDataWarehouseServer] = DataWarehouseServer;
			}
		}

		static string Parse(string key, string text)
		{
			var regex = new Regex(@"^\s*TestRig" + key + ":(?<value>.*)", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Multiline);
			var match = regex.Match(text);
			return match.Success ? match.Groups["value"].Value.Trim() : null;
		}

		static int ParseInt(string setting, string comments, int defaultValue = 0)
			=> int.TryParse(Parse(setting, comments), out var result) ? result : defaultValue;

		static bool ParseBool(string setting, string comments, bool defaultValue = false)
		{
			var text = Parse(setting, comments);
			if (bool.TryParse(text, out var result))
			{
				return result;
			}
			return defaultValue;
		}

		static TimeSpan ParseTimeSpan(string setting, string comments, TimeSpan defaultValue)
		{
			var text = Parse(setting, comments);
			if (TimeSpan.TryParse(text, out var result))
			{
				return result;
			}
			return defaultValue;
		}

		static IDictionary<string, object> GetRegistryEntries(string comments)
		{
			var errorMessageBuilder = new StringBuilder();
			var registryEntries = ParseRegistryEntriesDictionary(nameof(RegistryEntries), comments, errorMessageBuilder);

			if (errorMessageBuilder.Length > 0)
			{
				throw new InvalidOperationException(errorMessageBuilder.ToString());
			}

			return registryEntries;
		}

		static IDictionary<string, object> ParseRegistryEntriesDictionary(string setting, string comments, StringBuilder errorMessage)
		{
			const int ConfiguredWithType = 3;
			const int DefaultConfig = 2;
			const string StringType = "STR";
			const string BoolType = "BOOL";
			const string StringArrayType = "STR_ARRAY";

			var registry = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			var configurations = (Parse(setting, comments) ?? string.Empty)
					.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

			foreach (var configuration in configurations)
			{
				var parts = configuration.Split(new[] { '|' }, ConfiguredWithType, StringSplitOptions.None);
				if (parts.Length != ConfiguredWithType && parts.Length != DefaultConfig)
				{
					continue;
				}

				var key = parts[0].Trim();
				var configuredWithType = parts.Length == ConfiguredWithType;
				var type = configuredWithType ? parts[1].Trim() : StringType;
				var stringValue = (configuredWithType ? parts[2] : parts[1]).Trim();

				if (type.Equals(StringType, StringComparison.OrdinalIgnoreCase))
				{
					registry[key] = stringValue;
				}
				else if (type.Equals(BoolType, StringComparison.OrdinalIgnoreCase))
				{
					if (!bool.TryParse(stringValue, out var boolValue))
					{
						errorMessage.AppendLine($"Key '{key}' requires a boolean value, but value '{stringValue}' could not be converted to a boolean value");
					}
					else
					{
						registry[key] = boolValue;
					}
				}
				else if (type.Equals(StringArrayType, StringComparison.OrdinalIgnoreCase))
				{
					var stringArrayValue = stringValue.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
					registry[key] = stringArrayValue;
				}
				else
				{
					errorMessage.AppendLine($"Key '{key}' has unknown datatype '{type}', only {StringType}, {StringArrayType} and {BoolType} are supported");
				}
			}
			return registry;
		}

		static string SafeDatabaseName(string databaseName)
		{
			databaseName = new string(databaseName.Where(c => char.IsLetterOrDigit(c)).ToArray());
			if (databaseName.Length > 32)
			{
				databaseName = databaseName.Substring(0, 32);
			}
			return databaseName;
		}

		static string InstanceNameFromDbName(string databaseName)
		{
			if (databaseName.StartsWith("SH0", StringComparison.OrdinalIgnoreCase))
			{
				databaseName = databaseName.Substring(3);
			}
			return databaseName;
		}

		public string RestoreFromBackup { get; }

		public string ClientCode { get; }

		public bool ShouldRestoreFromBackup => !string.IsNullOrEmpty(RestoreFromBackup);

		public string SqlServer { get; }

		public string AONSecondarySqlServer { get; }

		public string DataWarehouseServer { get; }

		public string SqlServerDataFilePath { get; }

		public string SqlServerLogFilePath { get; }

		public string DatabaseName { get; }

		public string IconName { get; }

		public bool CreateIcon { get; }

		public bool Register { get; }

		public string RegistrationEnterpriseCode { get; }

		public string RegistrationSqlServer { get; }

		public bool IncludeSystemPackage { get; }

		public string SystemPackageDeploymentPath { get; }

		public bool AnyOptionsSet { get; }

		public string[] WebSites { get; }

		public IDictionary<string, object> RegistryEntries { get; }

		public string Origin { get; }

		public string WebServer { get; }

		public string WebDomain { get; }

		public string WebServerInstallPath { get; }

		public bool LaunchDbUpgrade { get; }

		public string DbUpgradeServer { get; }

		public bool ServiceTasks { get; }

		public bool EnableAudit { get; }

		public bool EnableAnalysis { get; }

		public string AnalysisServer { get; }

		public string BlazorUrl { get; }

		public string[] AddStaffRecords { get; }

		public string BlazorUrlAuthority => new Uri(BlazorUrl ?? DefaultBlazorUrl).Authority;

		public bool SetupAlwaysOn { get; }

		public string AONGroupName { get; }

		public string DefaultSqlServer => GetDefaultServerFromDatabaseName(DatabaseName);

		public string DefaultSqlServerDataFilePath => GetDefaultFileLocations(DatabaseFileTypes.Data);

		public string DefaultSqlServerLogFilePath => GetDefaultFileLocations(DatabaseFileTypes.Log);

		const string DefaultRegistrationEnterpriseCode = "WUT";

		public string DefaultRegistrationSqlServer => GetDefaultRegistrationSqlServer();

		const string DefaultSystemPackageDeploymentPath = @"\\cw1datfiles.wtg.zone\IBPLatestBuildArchive";

		public string DefaultWebServer => GetDefaultWebServerFromDatabaseName(DatabaseName);

		public string DefaultWebRootDomain => GetDefaultWebRootDomainFromDatabaseName(DatabaseName);

		[SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Allowed in file path")]
		const string DefaultWebServerInstallPath = @"C:\Program Files\WiseTech Global\CargoWise One Web Server Installer";

		static string DefaultDbUpgradeServer => GetRandomDefaultDbUpgradeServer();

		static string[] DefaultDbUpgradeServerList => new[]
		{
			"AU2SP-TUPG-401.sand.wtg.zone",
			"AU2SP-TUPG-402.sand.wtg.zone",
		};

		public string DefaultBlazorUrl => "https://" + InstanceNameFromDbName(DatabaseName) + ".blazor.sand.wtg.zone/backchannel";

		const string RegistrySettingGlowEnterpriseServicesRootUri = "GlowEnterpriseServicesRootUri";
		const string RegistrySettingGlowPortalsUri = "GlowPortalsUri";
		const string RegistrySettingGlowServiceUri = "GlowServiceUri";
		const string RegistrySettingBiAuditServer = "BiAuditServer";
		const string RegistrySettingBiAnalysisServer = "BiAnalysisServer";
		const string RegistrySettingBiDataWarehouseServer = "BiDataWarehouseServer";

		const string WebSiteGlow = "Glow";
		const string WebSitePortalsName = "GlowWebClient";
		const string WebSitePortalsPath = "Portals";
		const string WebSiteServices = "Services";

		const string PackageStatusDefault = "CUR";

		internal static ServerConfiguration[] DefaultDatabaseServerConfigurations =>
		[
			new ServerConfiguration(name: @"au2sp-tsql-403.sand.wtg.zone\INSTANCE1"),
			new ServerConfiguration(name: @"au2sp-tsql-404.sand.wtg.zone\INSTANCE1"),
			new ServerConfiguration(name: @"au2sp-tsql-405.sand.wtg.zone\INSTANCE1"),
			new ServerConfiguration(name: @"au2sp-tsql-406.sand.wtg.zone\INSTANCE1"),
		];

		const string KnownWebDomain = "testrig.sand.wtg.zone";

		internal static ServerConfiguration[] WebServerConfigurations =>
		[
			new ServerConfiguration(name: "Au2sp-tweb-403a.sand.wtg.zone", domain: KnownWebDomain),
			new ServerConfiguration(name: "Au2sp-tweb-403b.sand.wtg.zone", domain: KnownWebDomain),
			new ServerConfiguration(name: "Au2sp-tweb-404a.sand.wtg.zone", domain: KnownWebDomain),
			new ServerConfiguration(name: "Au2sp-tweb-404b.sand.wtg.zone", domain: KnownWebDomain),
			new ServerConfiguration(name: "Au2sp-tweb-405a.sand.wtg.zone", domain: KnownWebDomain),
			new ServerConfiguration(name: "Au2sp-tweb-405b.sand.wtg.zone", domain: KnownWebDomain),
		];

		internal static ServerConfiguration[] ExperimentalWebServerConfigurations =>
		[
			new ServerConfiguration(name: "Au2sp-sweb-407.sand.wtg.zone", domain: KnownWebDomain),
		];

		internal static ServerConfiguration[] AllWebServerConfigurations => WebServerConfigurations.Concat(ExperimentalWebServerConfigurations).ToArray();

		public bool PermitDuplicateWebSites { get; }

		public bool Verbose { get; }

		public bool ScheduleUPG { get; }

		public string PackageStatus { get; }

		public string SingleRefDatabaseName { get; }

		public int RewindTransformVersionNumber { get; }

		public bool DeployWinzor { get; }

		public bool RunWinzorE2ETests { get; }

		public TimeSpan VersionBrokerWarmupTimeout { get; }

		public static string GetDefaultServerFromDatabaseName(string databaseName) => JenkinsPick(databaseName, "default server name", DefaultDatabaseServerConfigurations).Name;
		static ServerConfiguration GetWebServerFromDatabaseName(string databaseName) => JenkinsPick(databaseName, "default web server name", WebServerConfigurations, includePurposeInEntropy: true);
		public static string GetDefaultWebServerFromDatabaseName(string databaseName) => GetWebServerFromDatabaseName(databaseName).Name;
		public static string GetDefaultWebRootDomainFromDatabaseName(string databaseName) => GetWebServerFromDatabaseName(databaseName).Domain;

		static T JenkinsPick<T>(string key, string purpose, T[] values, bool includePurposeInEntropy = false)
		{
			if (values == null || values.Length == 0)
			{
				throw new ArgumentException("values must be available before a selection can be made");
			}

			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentException($"databaseName must be given before a {purpose} can be determined");
			}

			var entropy = includePurposeInEntropy
				? $"{purpose} {key}"
				: key;
			var unsignedJenkinsHash = (long)JenkinsHash(entropy) - int.MinValue;
			var index = unsignedJenkinsHash * values.Length / uint.MaxValue;
			return values[index];
		}

		static int JenkinsHash(string value)
		{
			unchecked
			{
				int hash, i;
				for (hash = i = 0; i < value.Length; ++i)
				{
					hash += value[i];
					hash += (hash << 10);
					hash ^= (hash >> 6);
				}
				hash += (hash << 3);
				hash ^= (hash >> 11);
				hash += (hash << 15);
				return hash;
			}
		}

		public static string GetDefaultFileLocations(DatabaseFileTypes fileType)
		{
			try
			{
				var result = GetDefaultFileLocationFromMasterDB(fileType);
				if (string.IsNullOrEmpty(result))
				{
					result = GetDefaultFileLocationFromServer(fileType);
				}

				return result;
			}
			catch (SqlException ex)
			{
				var message = $"Error connecting to server '{Db.ServerName}': {ex.Message}";
				throw new InvalidOperationException(message, ex);
			}
		}

		internal static string GetDefaultFileLocationFromMasterDB(DatabaseFileTypes fileType)
		{
			using (var adminConn = Db.NewAdminConnection(Db.ServerName, "master"))
			{
				var sqlText = fileType == DatabaseFileTypes.Data
					? "SELECT value FROM master.sys.extended_properties WHERE name = 'MainDbDataFile'"
					: "SELECT value FROM master.sys.extended_properties WHERE name = 'MainDbLogFile'";
				return adminConn.ExecuteScalar(sqlText)?.ToString();
			}
		}

		internal static string GetDefaultFileLocationFromServer(DatabaseFileTypes fileType)
		{
			using (var adminConn = Db.NewAdminConnection(Db.ServerName, "master"))
			{
				var sqlText = fileType == DatabaseFileTypes.Data
					? "SELECT SERVERPROPERTY ('InstanceDefaultDataPath')"
					: "SELECT SERVERPROPERTY ('InstanceDefaultLogPath')";
				return adminConn.ExecuteScalar(sqlText).ToString();
			}
		}

		public static string GetDefaultRegistrationSqlServer()
		{
			using (var adminConn = Db.NewAdminConnection(Db.ServerName, "master"))
			{
				var sqlText = "SELECT @@SERVERNAME";
				return adminConn.ExecuteScalar(sqlText).ToString();
			}
		}

		static string GetRandomDefaultDbUpgradeServer()
		{
			var randomIndex = new Random().Next(DefaultDbUpgradeServerList.Length);
			return DefaultDbUpgradeServerList[randomIndex];
		}

		public enum DatabaseFileTypes
		{
			Data = 0,
			Log = 1,
		}

		[Immutable]
		internal sealed class ServerConfiguration
		{
			public ServerConfiguration(string name, string domain = null)
			{
				Name = name;
				Domain = domain;
			}
			public string Name { get; }
			public string Domain { get; }
		}
	}
}

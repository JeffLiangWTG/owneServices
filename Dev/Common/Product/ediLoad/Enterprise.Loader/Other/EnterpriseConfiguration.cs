using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Loader.Client;
using Enterprise.Client.Common;
using Enterprise.Upgrades;
using Enterprise.URLHandler;

namespace Enterprise.Loader
{
	public class EnterpriseConfiguration : ClientConfiguration
	{
		#region Fields

		public const string StartupExeFileName = "CargoWise.Start.exe";
		public const string StartupExeConfigFileName = "CargoWise.Start.exe.config";
		public const string ApplicationLoggingConfigFileName = "applog.json";
		public const string LaunchedByLoaderArgument = "-IAmDoingTheWrongThingByRunningEnterpriseWithoutLoader";
		public const string ServerDirectoryForEnterpriseArgumentPrefix = "-SDir:";
		public const string ShowLoginArgument = "-ShowLogin";
		public const string InstallCurrentVersionArgument = "-InstallCurrentVersion";
		public const string SelectCurrentVersionArgument = "-SelectCurrentVersion";
		public const string InstallOnlyArgument = "-InstallOnly";
		public const string RepairArgument = "-Repair";
		public const string CommandArgument = "-Cmd";
		public const string GlowServerArgument = "-GlowServer:";
		public const string IsHttpArgument = "-Http";
		public const string VerboseLoginArgument = "-VerboseLoginFilename";
		public const string UninstallArgument = "-Uninstall";
		public const string StartupPathArgumentPrefix = "-StartupPath:";
		public const string RunFromThisLocationArgument = "-RunFromThisLocation";
		public const string KeepStartRunningArgument = "-KeepStartRunning";
		public const string InstanceArgument = "-Instance:";
		public const string UpdateUsageLogArgument = "-UpdateUsageLog";
		public const string RemoveOldVersionsArgument = "-RemoveOldVersions";
		public const string ForceRemoveOldVersionsInFrontOfInstallationArgument = "-ForceRemoveOldVersionsInFrontOfInstallation";

		string databaseInstanceName = String.Empty;
		string databaseName = String.Empty;
		string databaseServerName = String.Empty;
		string legacyInstanceName;

		string glowServer;

		#endregion

		public EnterpriseConfiguration()
		{
			MaintenanceMessage = string.Empty;
		}

		public string LegacyInstanceName
		{
			get { return LegacyClientSetupHelper.GetInstanceName(legacyInstanceName, TargetDirectoryName); }
			protected set { legacyInstanceName = value; }
		}

		public string ServerName
		{
			get { return databaseServerName + (databaseInstanceName.Length > 0 ? '\\' + databaseInstanceName : string.Empty); }
			set
			{
				var parts = value.Split('\\');
				databaseServerName = parts[0];
				if (parts.Length > 1)
				{
					databaseInstanceName = parts[1];
				}
			}
		}

		public string DatabaseName
		{
			get { return databaseName; }
			set { databaseName = value; }
		}

		public string GlowServer
		{
			get { return glowServer; }
		}

		public bool IsHttp { get; private set; }

		public bool SelectCurrentVersion { get; private set; }

		public bool InstallOnly { get; private set; }

		public bool Repair { get; private set; }

		public bool Uninstall { get; private set; }

		public bool UpdateUsageLog { get; private set; }

		public bool RemoveOldVersions { get; private set; }
		public bool ForceRemoveOldVersionsInFrontOfInstallation { get; private set; }

		public string MaintenanceMessage { get; private set; }

		public bool UsingConfigFile { get; private set; }

		public bool RunFromThisLocation { get; private set; }

		public bool KeepStartRunning { get; private set; }

		public string InstanceName { get; set; }

		public string EdiEntUrl { get; private set; }

		public override string ProgramArguments
		{
			get
			{
				string result;
				var separator = " ";
				if (commandProgramName != null)
				{
					var parameters = new List<string>();
					if (commandProgramArguments.Count > 0)
					{
						parameters.AddRange(commandProgramArguments);
					}
					else
					{
						parameters.Add(ServerName);
						parameters.Add(DatabaseName);
					}
					result = string.Join(separator, parameters.ToArray()).Trim();
				}
				else
				{
					var parameters = new List<string> { ServerName, DatabaseName };

					if (!String.IsNullOrWhiteSpace(GlowServer))
					{
						parameters.Add(GlowServerArgument + GlowServer);
					}

					parameters.Add(LaunchedByLoaderArgument);

					parameters.Add(ShowLoginArgument);

					if (new ConfigFile(this.StartupPath).Exists)
					{
						parameters.Add(ServerDirectoryForEnterpriseArgumentPrefix + '\"' + StartupPath + "\"");
					}

					result = AppendNonExistingParameters(string.Join(separator, parameters.ToArray()).Trim(), OtherProgramArguments);
				}

				return result;
			}
		}

		public override string ProgramFileName
		{
			get
			{
				if (commandProgramName != null)
				{
					return commandProgramName;
				}

				return ExeFileNames.GetMainExeFileName(BaseTargetPath);
			}
		}

		bool commandProgramArgument;
		string commandProgramName;
		List<string> commandProgramArguments;

		protected override void InitializeCore(string[] args)
		{
			ReadStartupPathArgument(args);
			UsingConfigFile = ParseConfigFile();
			base.InitializeCore(args);
			LoadInstallationVersion();
		}

		void ReadStartupPathArgument(string[] args)
		{
			if (args != null)
			{
				foreach (var arg in args)
				{
					if (arg != null && arg.StartsWith(StartupPathArgumentPrefix))
					{
						StartupPathOverride = arg.Substring(StartupPathArgumentPrefix.Length);
					}
				}
			}
		}

		protected override bool ParseCommandLineArgument(string arg, CommandLineArgumentType type)
		{
			if (type == CommandLineArgumentType.ThisLoaderInstance)
			{
				if (arg.StartsWith(EdiUrlPrefix.Value))
				{
					EdiEntUrl = arg;
					return true;
				}
				if (!UsingConfigFile && databaseArgument == "server" && !arg.StartsWith("-"))
				{
					var parts = arg.Split('\\');
					databaseServerName = parts[0];
					if (parts.Length > 1)
					{
						databaseInstanceName = parts[1];
					}
					databaseArgument = "database";
				}
				else if (!UsingConfigFile && databaseArgument == "database" && !arg.StartsWith("-"))
				{
					databaseName = arg;
				}
				else
				{
					databaseArgument = string.Empty;
					if (commandProgramName != null)
					{
						commandProgramArguments.Add(arg);
						return true;
					}
					else if (commandProgramArgument)
					{
						commandProgramName = arg;
						commandProgramArguments = new List<string>();
					}
					else if (arg == CommandArgument)
					{
						commandProgramArgument = true;
					}
					else if (arg == IsHttpArgument)
					{
						IsHttp = true;
					}
					else if (string.Equals(arg, InstallCurrentVersionArgument, StringComparison.OrdinalIgnoreCase))
					{
						return true;
					}
					else if (string.Equals(arg, SelectCurrentVersionArgument, StringComparison.OrdinalIgnoreCase))
					{
						SelectCurrentVersion = true;
						return true;
					}
					else if (string.Equals(arg, InstallOnlyArgument, StringComparison.OrdinalIgnoreCase))
					{
						InstallOnly = true;
						return true;
					}
					else if (string.Equals(arg, RepairArgument, StringComparison.OrdinalIgnoreCase))
					{
						Repair = true;
						return true;
					}
					else if (string.Equals(arg, UninstallArgument, StringComparison.OrdinalIgnoreCase))
					{
						Uninstall = true;
						return true;
					}
					else if (string.Equals(arg, UpdateUsageLogArgument, StringComparison.OrdinalIgnoreCase))
					{
						UpdateUsageLog = true;
						return true;
					}
					else if (string.Equals(arg, RemoveOldVersionsArgument, StringComparison.OrdinalIgnoreCase))
					{
						RemoveOldVersions = true;
						return true;
					}
					else if (string.Equals(arg, ForceRemoveOldVersionsInFrontOfInstallationArgument, StringComparison.OrdinalIgnoreCase))
					{
						ForceRemoveOldVersionsInFrontOfInstallation = true;
						return true;
					}
					else if (string.Equals(arg, RunFromThisLocationArgument, StringComparison.OrdinalIgnoreCase))
					{
						RunFromThisLocation = true;
						return true;
					}
					else if (arg.StartsWith(StartupPathArgumentPrefix, StringComparison.OrdinalIgnoreCase))
					{
						return true;
					}
					else if (arg.StartsWith(KeepStartRunningArgument, StringComparison.OrdinalIgnoreCase))
					{
						KeepStartRunning = true;
						return true;
					}
					else if (arg.StartsWith(InstanceArgument, StringComparison.OrdinalIgnoreCase))
					{
						InstanceName = arg.Substring(InstanceArgument.Length);
						return true;
					}
				}
			}
			return false;
		}
		string databaseArgument = "server";

		bool ParseConfigFile()
		{
			var configFile = new ConfigFile(StartupPath);
			if (configFile.Exists)
			{
				if (!string.IsNullOrWhiteSpace(configFile.AppManagerDirectoryOverride))
				{
					AppManagerDirectoryName = configFile.AppManagerDirectoryOverride;
				}

				if (!string.IsNullOrWhiteSpace(configFile.DbInstance))
				{
					databaseInstanceName = configFile.DbInstance;
				}

				if (!string.IsNullOrWhiteSpace(configFile.DbName))
				{
					databaseName = configFile.DbName;
				}

				if (!string.IsNullOrWhiteSpace(configFile.DbServer))
				{
					databaseServerName = configFile.DbServer;
				}

				if (!string.IsNullOrWhiteSpace(configFile.EnterpriseInstance))
				{
					LegacyInstanceName = configFile.EnterpriseInstance;
				}

				if (!string.IsNullOrWhiteSpace(configFile.MaintenanceMessage))
				{
					MaintenanceMessage = configFile.MaintenanceMessage;
				}

				if (!string.IsNullOrWhiteSpace(configFile.OtherParameters))
				{
					OtherProgramArguments = AppendNonExistingParameters(OtherProgramArguments, configFile.OtherParameters);
				}
				if (!string.IsNullOrWhiteSpace(configFile.GlowServer))
				{
					glowServer = configFile.GlowServer;
				}
			}
			return configFile.Exists;
		}

		static string AppendNonExistingParameters(string mainParams, string paramsToAppend)
		{
			var separator = ' ';
			var nameRegex = new Regex(@"^(.[^:]*):?.*$");
			var mainParamsNames = mainParams.Split(separator).Select(p => nameRegex.Match(p).Groups[1].Value).ToArray();
			var nonExistingParams = paramsToAppend.Split(separator).Where(p => !mainParamsNames.Contains(nameRegex.Match(p).Groups[1].Value));
			return string.Join(" ", Enumerable.Repeat(mainParams, 1).Concat(nonExistingParams)).Trim();
		}

		void LoadInstallationVersion()
		{
			try
			{
				var versionNumber = new CurrentVersionFile(Path.Combine(BaseTargetPath, "CurrentVersion")).RetrieveVersionNumber(ServerName, DatabaseName);
				if (!string.IsNullOrEmpty(versionNumber))
				{
					TargetVersion = new Version(versionNumber);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				// If we can't read the version number, we'll just use the default version
			}
		}

		public virtual UpgradeManager NewUpgradeManager()
		{
			var con = DatabaseConnectionInitializer.DatabaseHelperInstance.Connection;
			return new SqlUpgradeManager(new UpgradeSqlContext(con, con.DataSource, con.Database));
		}
	}
}


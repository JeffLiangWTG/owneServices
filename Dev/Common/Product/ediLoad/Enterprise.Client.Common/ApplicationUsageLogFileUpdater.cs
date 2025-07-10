using System;
using System.Diagnostics;
using CargoWise.Loader.Common;
using Enterprise.Upgrades;

namespace Enterprise.Client.Common
{
	public class ApplicationUsageLogFileUpdater : InstallationItem
	{
		readonly string serverName;
		readonly string databaseName;
		readonly ApplicationUsageLogFile applicationUsageLogFile;

		public ApplicationUsageLogFileUpdater(Installation installation, string serverName, string databaseName) : this(installation, serverName, databaseName, new ())
		{
		}

		public ApplicationUsageLogFileUpdater(Installation installation, string serverName, string databaseName, ApplicationUsageLogFile applicationUsageLogFile) : base(installation)
		{
			this.serverName = serverName;
			this.databaseName = databaseName;
			this.applicationUsageLogFile = applicationUsageLogFile;
		}

		protected override bool NeedsToInstallCore()
		{
			return true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Path names are exempt")]
		protected override InstallationResult InstallExcludingDependencies()
		{
			try
			{
				applicationUsageLogFile.LogUsage(new ApplicationUsageLog(serverName, databaseName));
				return InstallationResult.OK();
			}
			catch (Exception ex)
			{
				LogHelper.WriteEventLog($"Update ApplicationUsageLog failed: {ex}", EventLogEntryType.Warning);
				return InstallationResult.OK(); // ApplicationUsageLog updating should not prevent the startup
			}
		}
	}
}

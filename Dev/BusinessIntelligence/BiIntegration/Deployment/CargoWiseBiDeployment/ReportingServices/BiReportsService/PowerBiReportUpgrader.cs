using System.Globalization;
using CargoWise.Data;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.Integration;
using Enterprise.Registry.Business;

namespace CargoWise.Bi.Deployment.ReportingServices
{
	#region SuppressResourceStringsCheckRegion

	public abstract class PowerBiReportUpgrader
	{
		public PowerBiReportUpgrader(ILogger logger)
		{
			versionBeforeUpgrade = new VersionLabel(DatabaseMajorVersion, DatabaseMinorVersion);
			this.logger = logger;
		}
		readonly ILogger logger;

		public PowerBiReportUpgrader(VersionLabel versionBeforeUpgrade)
		{
			this.versionBeforeUpgrade = versionBeforeUpgrade;
		}

		readonly VersionLabel versionBeforeUpgrade;

		protected abstract int DatabaseMajorVersion { get; }
		protected abstract int DatabaseMinorVersion { get; }
		protected abstract VersionLabel LatestVersion { get; }

		protected abstract void UpdatePowerBiProjectUpgradeVersion();

		public void Upgrade(PowerBiDeployer deployer)
		{
			Log(LogType.Debug, "Deploying Power BI reports");

			deployer.DeployReportFiles();
			UpdatePowerBiProjectUpgradeVersion();

			Log(LogType.Debug, "Power BI reports deployed successfully.");
		}

		public bool CheckPowerBiServerHealthStatus(PowerBiDeployer deployer)
		{
			Log(LogType.Debug, "Checking Power BI Server contents");

			var healthStatus = deployer.CheckPowerBiServerHealthStatus();

			Log(LogType.Debug, "Checking completed");

			return healthStatus;
		}

		protected void Log(LogType logType, string message)
		{
			logger?.Log(logType, message);
		}

		#region Upgrade Required

		public bool UpgradeRequired
		{
			get { return LatestVersion.CompareTo(versionBeforeUpgrade) != 0; }
		}

		public bool CheckRequirements(PowerBiDeployer deployer)
		{
			var result = false;

			string errorMsg;
			if (!Db.Connection.ServerVersionNumber.IsMinimumRequiredVersionOrAbove)
			{
				Log(LogType.Debug, "Database Engine 2016 not installed. Reports deployment skipped.");
			}
			else if (string.IsNullOrEmpty(SystemDataRegistry.Instance.BiPowerBiWebPortalUrl.Value))
			{
				Log(LogType.Debug, "Power BI Reports URL registry is empty. Power BI Reports deployment skipped.");
			}
			else if (!deployer.IsPowerBiConfigured(out errorMsg))
			{
				Log(LogType.Debug, string.Format(CultureInfo.InvariantCulture, "Power BI report server is not configured. Reports deployment skipped.\r\nError Message: {0}", errorMsg));
			}
			else if (!deployer.CheckPowerBiReportList())
			{
				Log(LogType.Debug, "No Power BI reports to deploy.");
			}
			else
			{
				result = true;
			}

			return result;
		}

		#endregion
	}

	#endregion
}

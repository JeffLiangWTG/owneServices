using System;
using CargoWise.Common;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.Environment;
using Enterprise.Integration;

namespace CargoWise.Bi.Deployment.ReportingServices
{
	#region SuppressResourceStringsCheckRegion

	public class AnalyticsReportUpgrader : PowerBiReportUpgrader
	{
		public AnalyticsReportUpgrader(ILogger logger) : base(logger)
		{
		}

		public AnalyticsReportUpgrader(VersionLabel versionBeforeUpgrade) : base(versionBeforeUpgrade)
		{
		}

		protected override int DatabaseMajorVersion => Env.Registry.DatabaseMajorAnalyticsReportProjectVersion;
		protected override int DatabaseMinorVersion => Env.Registry.DatabaseMinorAnalyticsReportProjectVersion;
		protected override VersionLabel LatestVersion => AnalyticsReportProjectVersion.Application;

		protected override void UpdatePowerBiProjectUpgradeVersion()
		{
			try
			{
				Env.Registry.DatabaseMajorAnalyticsReportProjectVersion = LatestVersion.Major;
				Env.Registry.DatabaseMinorAnalyticsReportProjectVersion = LatestVersion.Minor;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				throw new PowerBiException("Failed to update PowerBi project upgrade version.\r\n" + ex.Message, ex);
			}
		}
	}

	#endregion
}

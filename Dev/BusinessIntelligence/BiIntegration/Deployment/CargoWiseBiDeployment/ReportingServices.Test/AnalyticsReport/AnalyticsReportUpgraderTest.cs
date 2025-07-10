using Enterprise.DbUpgrader.Resource.Version;
using NUnit.Framework;

namespace CargoWise.Bi.Deployment.ReportingServices.Testing
{
	class AnalyticsReportUpgraderTest : TestCase
	{
		public void TestUpgradeRequired()
		{
			AssertUpgradeRequired("[Same Version as Latest]", AnalyticsReportProjectVersion.Application.Major, AnalyticsReportProjectVersion.Application.Minor, expected: false);
			AssertUpgradeRequired("[Minor Version higher than Latest]", AnalyticsReportProjectVersion.Application.Major, AnalyticsReportProjectVersion.Application.Minor + 1, expected: true);
			AssertUpgradeRequired("[Minor Version lower than Latest]", AnalyticsReportProjectVersion.Application.Major, AnalyticsReportProjectVersion.Application.Minor - 1, expected: true);
			AssertUpgradeRequired("[Major Version higher than Latest]", AnalyticsReportProjectVersion.Application.Major + 1, AnalyticsReportProjectVersion.Application.Minor, expected: true);
			AssertUpgradeRequired("[Major Version lower than Latest]", AnalyticsReportProjectVersion.Application.Major - 1, AnalyticsReportProjectVersion.Application.Minor, expected: true);
			AssertUpgradeRequired("[Both Major and Minor Version differ from Latest]", AnalyticsReportProjectVersion.Application.Major + 1, AnalyticsReportProjectVersion.Application.Minor - 1, expected: true);
		}

		void AssertUpgradeRequired(string assertMessagePrefix, int majorVersion, int minorVersion, bool expected)
		{
			var powerBiProjectUpgrader = new AnalyticsReportUpgrader(new VersionLabel(majorVersion, minorVersion));
			AssertEquals(assertMessagePrefix + " => UpgradeRequired?", expected, powerBiProjectUpgrader.UpgradeRequired);
		}
	}
}

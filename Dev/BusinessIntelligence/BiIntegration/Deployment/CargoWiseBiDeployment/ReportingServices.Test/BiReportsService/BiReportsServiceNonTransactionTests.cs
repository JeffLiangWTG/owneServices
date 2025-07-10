using System;
using CargoWise.Bi.Registration.PowerBi;
using CargoWise.Data.Testing;
using Enterprise.Environment;
using NUnit.Framework;

namespace CargoWise.Bi.Deployment.ReportingServices.Testing
{
	class BiReportsServiceNonTransactionTests : TestCase
	{
		[UseSnapshotProtection]
		public void TestGetPowerBiReportsListNoReports()
		{
			var service = new BiReportsServiceForTest();
			var linkBuilderFactory = new GlowPowerBiReportLinkBuilder.Factory();

			PowerBiReport[] reports = null;
			using (Env.SetTemporaryUserContext(Guid.Empty, Guid.Empty, Guid.Empty))
			{
				reports = service.GetPowerBiReportsList(linkBuilderFactory, BiReportCategories.GetBiReportCategory(nameof(BiReportCategory.All)));
			}
			AssertEquals("Invalid user checkpoints should return no reports", 0, reports.Length);
		}

		[UseSnapshotProtection]
		public void TestGetPowerBiReportsListAllReports()
		{
			var service = new BiReportsServiceForTest();
			var linkBuilderFactory = new GlowPowerBiReportLinkBuilder.Factory();

			var reports = service.GetPowerBiReportsList(linkBuilderFactory, BiReportCategories.GetBiReportCategory(nameof(BiReportCategory.All)));
			AssertEquals("valid user checkpoints should return all reports", 0, reports.Length);
		}

		[UseSnapshotProtection]
		public void TestGetPowerBiReportsReportTypeSet()
		{
			var service = new BiReportsServiceForTest();
			var linkBuilderFactory = new GlowPowerBiReportLinkBuilder.Factory();

			var reports = service.GetPowerBiReportsList(linkBuilderFactory, BiReportCategories.GetBiReportCategory(nameof(BiReportCategory.All)));
			AssertEquals("valid user checkpoints should return all reports", 0, reports.Length);
			foreach (var report in reports)
			{
				AssertNotNull("ResourceType should not be null", report.ResourceType);
			}
		}
	}
}

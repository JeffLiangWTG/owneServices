using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.DataTransfer.ComplianceReport;
using Enterprise.Accounting.DataTransfer.ComplianceReport.JPKV7M;
using Enterprise.Accounting.DataTransfer.ComplianceReport.SAFT;

namespace Enterprise.Accounting.DataTransfer.Testing.ComplianceReport
{
	public class ComplianceReportAdditionalDataCollectorFactoryTest : TestCaseWithFactory
	{
		public void TestGetComplianceReportAdditionalDataCollector()
		{
			var report = Factory.NewWithValidTestData<AccComplianceReport>();
			Factory.Save();

			var saftModes = new[] {
				ComplianceReportDataCollectionMode.None,
				ComplianceReportDataCollectionMode.TransactionBatch,
				ComplianceReportDataCollectionMode.SAFT,
				ComplianceReportDataCollectionMode.SAFTSelfBilling,
				ComplianceReportDataCollectionMode.SAFT1_10,
				ComplianceReportDataCollectionMode.Document,
				ComplianceReportDataCollectionMode.Esterometro,
			};
			var supplierPK = ZGuid.NewZGuid();
			var supplierCode = new ZString("SUPPLIER");

			foreach (var mode in saftModes)
			{
				var dataCollector = ComplianceReportAdditionalDataCollectorFactory.GetComplianceReportAdditionalDataCollector(report, mode, supplierPK, supplierCode);
				AssertNotNull($"Data Collector for mode {mode}", dataCollector);
				AssertEquals($"Collection mode {mode} gives SAFTAdditionalDataCollector", typeof(SAFTAdditionalDataCollector), dataCollector.GetType());
			}

			var dataCollectorJPK = ComplianceReportAdditionalDataCollectorFactory.GetComplianceReportAdditionalDataCollector(report, ComplianceReportDataCollectionMode.JPKV7M, supplierPK, supplierCode);
			AssertNotNull($"Data Collector for mode {ComplianceReportDataCollectionMode.JPKV7M}", dataCollectorJPK);
			AssertEquals($"Collection mode {ComplianceReportDataCollectionMode.JPKV7M} gives JPKAdditionalDataCollector", typeof(JPKAdditionalDataCollector), dataCollectorJPK.GetType());
		}

		public void TestGetComplianceReportAdditionalDataCollectorRequiresReport()
		{
			AssertExceptionThrown<ArgumentException>("report parameter cannot be null", @"Value cannot be null.
Parameter name: report", () => ComplianceReportAdditionalDataCollectorFactory.GetComplianceReportAdditionalDataCollector(null));
		}
	}
}

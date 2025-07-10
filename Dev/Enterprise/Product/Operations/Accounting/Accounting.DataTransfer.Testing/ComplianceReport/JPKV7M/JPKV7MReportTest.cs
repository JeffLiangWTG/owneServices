using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.DataTransfer.ComplianceReport.JPKV7M;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.Testing.ComplianceReport.JPKV7M
{
	public class JPKV7MReportTest : TestCaseWithFactory
	{
		[TestDate(2022, 2, 22)]
		public void TestExportXmlToEDocs()
		{
			//Arrange
			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			complianceReport.ACR_ReportType = AccComplianceReport.ReportTypes.JPKV7M;
			complianceReport.ACR_Description = "Compliance Report JPK";
			complianceReport.ACR_DateFrom = new ZDate(2022, 1, 1);
			complianceReport.ACR_DateTo = new ZDate(2022, 1, 31);
			complianceReport.ACR_Status = AccComplianceReport.Status.ReportGenerated;
			Factory.Save();
			var logger = new LoggerForTest();
			var report = new JPKV7MReport();

			using (complianceReport.Factory.AddDisposableService())
			{
				// Act
				var newStatus = report.ExportXmlToEDocs(complianceReport, logger);

				// Assert
				AssertEquals("New status returned", AccComplianceReport.Status.ReportOutputGenerated, newStatus);
				AssertEquals("Report status not changed yet", AccComplianceReport.Status.ReportGenerated, complianceReport.ACR_Status);

				var eDocs = ((IDocManagerSupport)complianceReport).DocManagerInfo.AllEDocs;
				AssertEquals("One eDoc added", 1, eDocs.Count);
				AssertEquals("Filename", "2022-02-22T000000_JPK_VAT.xml", eDocs[0].FileName);

				var actualLogs = logger.ToString();
				AssertContains("Export finished for Compliance Report JPK", actualLogs);
			}
		}
	}
}

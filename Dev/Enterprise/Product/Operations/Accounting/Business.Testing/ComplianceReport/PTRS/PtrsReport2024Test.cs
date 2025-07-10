using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.ComplianceReport.PTRS;
using Moq;
using NUnit.Framework;
using static Enterprise.Accounting.Business.AccountingConstants;

namespace Enterprise.Accounting.Business.Testing.ComplianceReport.PTRS
{
	[TestedType(typeof(PtrsReport2024))]
	public class PtrsReport2024Test : PtrsReportTestBase
	{
		public void TestPopulatingPtrsReportData()
		{
			(var complianceReport, var allPaymentsComplianceReport) = PrepareComplianceReports();

			var mockHelper = SetupReport2024HelperMock(Factory, DefaultAllPaymentsData, DefaultPtrsData);

			var allPaymentsReport = Factory.New<PtrsAllPaymentsReport2024>();
			allPaymentsReport.ATR_ACR_ComplianceReport = allPaymentsComplianceReport.PK;
			mockHelper.Verify(m => m.CalculatePtrsAllPaymentsReport2024Data(allPaymentsComplianceReport), Times.Once);

			// ptrsReport data is updated on setting propery
			var ptrsReport = Factory.New<PtrsReport2024>();
			ptrsReport.ATR_ACR_ComplianceReport = complianceReport.PK;
			mockHelper.Verify(m => m.CalculatePtrsReport2024Data(complianceReport, It.IsAny<PtrsAllPaymentsReport2024Data>()), Times.Once);

			AssertPtrsReport2024Data(ptrsReport, DefaultPtrsData);
			Factory.Save();
			AssertEquals("Saved", AccTaxReturn.Status.Saved, ptrsReport.ATR_Status);

			var newFactory = new BusinessObjectFactory();
			mockHelper = SetupReport2024HelperMock(newFactory, AlternativeAllPaymentsData, AlternativePtrsData);

			// ptrsReport data is updated on Load
			ptrsReport = newFactory.Load<PtrsReport2024>(ptrsReport.PK);
			mockHelper.Verify(m => m.CalculatePtrsAllPaymentsReport2024Data(ptrsReport.AllPaymentsComplianceReport), Times.Once);
			mockHelper.Verify(m => m.CalculatePtrsReport2024Data(ptrsReport.ComplianceReport, It.IsAny<PtrsAllPaymentsReport2024Data>()), Times.Once);
			AssertPtrsReport2024Data(ptrsReport, AlternativePtrsData);

			// Make ptrsReport submitted 
			newFactory.Save();
			ptrsReport.SubmitReport();
			AssertEquals("Submitted", AccTaxReturn.Status.Submitted, ptrsReport.ATR_Status);
			newFactory.Save();

			newFactory = new BusinessObjectFactory();
			mockHelper = SetupReport2024HelperMock(newFactory, DefaultAllPaymentsData, DefaultPtrsData);

			// No changes to the stored data after ptrsReport is submitted
			ptrsReport = newFactory.Load<PtrsReport2024>(ptrsReport.PK);
			AssertEquals("Submitted", AccTaxReturn.Status.Submitted, ptrsReport.ATR_Status);
			mockHelper.Verify(m => m.CalculatePtrsAllPaymentsReport2024Data(It.IsAny<AccComplianceReport>()), Times.Never);
			mockHelper.Verify(m => m.CalculatePtrsReport2024Data(It.IsAny<AccComplianceReport>(), It.IsAny<PtrsAllPaymentsReport2024Data>()), Times.Never);
			AssertPtrsReport2024Data(ptrsReport, AlternativePtrsData);
		}

		public void TestPtrsReportCreatesAccTaxReturnColumnsOnPopulatingDataFromComplianceReport()
		{
			(var complianceReport, var allPaymentsComplianceReport) = PrepareComplianceReports();

			SetupReport2024HelperMock(Factory, DefaultAllPaymentsData, DefaultPtrsData);

			var allPaymentsReport = Factory.New<PtrsAllPaymentsReport2024>();
			allPaymentsReport.ATR_ACR_ComplianceReport = allPaymentsComplianceReport.PK;

			var ptrsReport = Factory.New<PtrsReport2024>();
			AssertEquals("No columns", 0, ptrsReport.Columns.Count);

			ptrsReport.ATR_ACR_ComplianceReport = complianceReport.PK;
			AssertEquals("New columns", 12, ptrsReport.Columns.Count);
		}

		#region Implementation

		(AccComplianceReport complianceReport, AccComplianceReport allPaymentsComplianceReport) PrepareComplianceReports()
		{
			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();

			var allPaymentsComplianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			allPaymentsComplianceReport.ACR_GC_Company = complianceReport.ACR_GC_Company;
			allPaymentsComplianceReport.ACR_ReportType = ComplianceReportTypes.PaymentTimesAllPayments2024ReportType;
			allPaymentsComplianceReport.ACR_DateFrom = complianceReport.ACR_DateFrom;
			allPaymentsComplianceReport.ACR_DateTo = complianceReport.ACR_DateTo;

			return (complianceReport, allPaymentsComplianceReport);
		}

		#endregion
	}
}

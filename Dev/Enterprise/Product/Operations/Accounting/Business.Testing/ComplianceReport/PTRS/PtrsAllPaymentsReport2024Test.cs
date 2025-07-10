using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.ComplianceReport.PTRS;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ComplianceReport.PTRS
{
	[TestedType(typeof(PtrsAllPaymentsReport2024))]
	public class PtrsAllPaymentsReport2024Test : PtrsReportTestBase
	{
		public void TestPopulatingAllPaymentsData()
		{
			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			var mockHelper = SetupReport2024HelperMock(Factory, DefaultAllPaymentsData, DefaultPtrsData);

			// ptrsReport data is updated on setting propery
			var ptrsReport = Factory.New<PtrsAllPaymentsReport2024>();
			ptrsReport.ATR_ACR_ComplianceReport = complianceReport.PK;
			mockHelper.Verify(m => m.CalculatePtrsAllPaymentsReport2024Data(complianceReport), Times.Once);

			AssertPtrsAllPaymentsReport2024Data(ptrsReport, DefaultAllPaymentsData);
			Factory.Save();
			AssertEquals("Saved", AccTaxReturn.Status.Saved, ptrsReport.ATR_Status);

			var newFactory = new BusinessObjectFactory();
			mockHelper = SetupReport2024HelperMock(newFactory, AlternativeAllPaymentsData, AlternativePtrsData);

			// ptrsReport data is updated on Load
			ptrsReport = newFactory.Load<PtrsAllPaymentsReport2024>(ptrsReport.PK);
			mockHelper.Verify(m => m.CalculatePtrsAllPaymentsReport2024Data(ptrsReport.ComplianceReport), Times.Once);
			AssertPtrsAllPaymentsReport2024Data(ptrsReport, AlternativeAllPaymentsData);

			// Make ptrsReport submitted 
			newFactory.Save();
			ptrsReport.SubmitReport();
			AssertEquals("Submitted", AccTaxReturn.Status.Submitted, ptrsReport.ATR_Status);
			newFactory.Save();

			newFactory = new BusinessObjectFactory();
			mockHelper = SetupReport2024HelperMock(newFactory, DefaultAllPaymentsData, DefaultPtrsData);

			// No changes to the stored data after ptrsReport is submitted
			ptrsReport = newFactory.Load<PtrsAllPaymentsReport2024>(ptrsReport.PK);
			AssertEquals("Submitted", AccTaxReturn.Status.Submitted, ptrsReport.ATR_Status);
			mockHelper.Verify(m => m.CalculatePtrsAllPaymentsReport2024Data(ptrsReport.ComplianceReport), Times.Never);
			AssertPtrsAllPaymentsReport2024Data(ptrsReport, AlternativeAllPaymentsData);
		}

		public void TestAllPaymentsReportCreatesAccTaxReturnColumnsOnPopulatingDataFromComplianceReport()
		{
			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			SetupReport2024HelperMock(Factory, DefaultAllPaymentsData, DefaultPtrsData);

			var ptrsReport = Factory.New<PtrsAllPaymentsReport2024>();
			AssertEquals("No columns", 0, ptrsReport.Columns.Count);

			ptrsReport.ATR_ACR_ComplianceReport = complianceReport.PK;
			AssertEquals("New columns", 4, ptrsReport.Columns.Count);
		}
	}
}

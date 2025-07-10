using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ComplianceReport.LiquidazioneIVA.Testing
{
	class LIQSubmissionDataColumnsAdapterTest : TestCaseWithFactory
	{
		AccComplianceReport Report => report ?? (report = Factory.New<AccComplianceReport>());

		AccComplianceReport report;

		public void TestLoadAccVatSummaryData()
		{
			var submissionData = FillReportWithValidTestData();

			var newFactory = new BusinessObjectFactory();
			var reportInNewFactory = newFactory.Load<AccComplianceReport>(report.PK);
			var submissionDataInNewFactory = reportInNewFactory.LoadLIQSubmissionData();

			AssertNotEquals(submissionData, submissionDataInNewFactory);
			AssertEquals("Report", submissionData.ComplianceReport.PK, submissionDataInNewFactory.ComplianceReport.PK);

			AssertEquals("PageFromLiquidazione", submissionDataInNewFactory.PageFromLiquidazione, 3);
			AssertEquals("PageToAR", submissionDataInNewFactory.PageToAR, 5);

			AssertSubmissionDataColumn(submissionData.ComputedByCW1, submissionDataInNewFactory.ComputedByCW1);
			AssertSubmissionDataColumn(submissionData.Adjustments, submissionDataInNewFactory.Adjustments);
			AssertSubmissionDataColumn(submissionData.ValuesToSubmit, submissionDataInNewFactory.ValuesToSubmit);

			var valuesToSubmit = submissionDataInNewFactory.ValuesToSubmit;
			AssertEquals("Box3_TotalVatBasePayables", valuesToSubmit.Box3_TotalVatBasePayables, 400M);
			AssertEquals("Box6_VatBalanceReceivablesAndPayables", valuesToSubmit.Box6_VatBalanceReceivablesAndPayables, 3265M);
			AssertEquals("Box8_TotalBalance", valuesToSubmit.Box8_TotalBalance, 3035M);
		}

		public void TestLoadAccVatSummaryDataWithNullValues()
		{
			var submissionData = FillReportWithValidTestData();

			var newFactory = new BusinessObjectFactory();
			var accTaxReturn = newFactory.LoadTop1<AccTaxReturn>(new ZQuery(AccTaxReturnSchema.ATR_ACR_ComplianceReport, Report.PK));
			accTaxReturn.Columns.RemoveAndDelete(accTaxReturn.Columns[4]);
			accTaxReturn.Columns.RemoveAndDelete(accTaxReturn.Columns[1]);
			newFactory.Save();

			var reportInNewFactory = newFactory.Load<AccComplianceReport>(report.PK);
			var submissionDataInNewFactory = reportInNewFactory.LoadLIQSubmissionData();

			AssertNotEquals(submissionData, submissionDataInNewFactory);
			AssertEquals("Report", submissionData.ComplianceReport.PK, submissionDataInNewFactory.ComplianceReport.PK);

			var adjustments = submissionDataInNewFactory.Adjustments;
			AssertEquals("Box1_TotalVatBaseReceivables", adjustments.Box1_TotalVatBaseReceivables, 45M);
			AssertEquals("Box2_TotalVatReceivables", adjustments.Box2_TotalVatReceivables, 0M);
			AssertEquals("Box3_TotalVatBasePayables", adjustments.Box3_TotalVatBasePayables, 200M);
			AssertEquals("Box4_TotalVatPayablesRecoverable", adjustments.Box4_TotalVatPayablesRecoverable, 250M);
			AssertEquals("Box5_TotalVatPayablesNotRecoverable", adjustments.Box5_TotalVatPayablesNotRecoverable, 0M);
			AssertEquals("Box7_BalancePreviousPeriod", adjustments.Box7_BalancePreviousPeriod, 70M);

			var valuesToSubmit = submissionDataInNewFactory.ValuesToSubmit;
			AssertEquals("Box3_TotalVatBasePayables ValuesToSubmit", valuesToSubmit.Box3_TotalVatBasePayables, 400M);
			AssertEquals("Box5_TotalVatPayablesNotRecoverable ValuesToSubmit", valuesToSubmit.Box5_TotalVatPayablesNotRecoverable, 3500M);
			AssertEquals("Box6_VatBalanceReceivablesAndPayables", valuesToSubmit.Box6_VatBalanceReceivablesAndPayables, 3250M);
			AssertEquals("Box8_TotalBalance", valuesToSubmit.Box8_TotalBalance, 3020M);
		}

		public void TestLoadAccTaxReturn()
		{
			var accTaxReturn = Report.LoadAccTaxReturn();
			AssertNull("Tax Return has not been entered", accTaxReturn);

			FillReportWithValidTestData();

			accTaxReturn = Report.LoadAccTaxReturn();
			AssertNotNull("Tax Return has been saved", accTaxReturn);
			AssertEquals("16 columns should be saved", 16, accTaxReturn.Columns.Count);

			AssertEquals("ATR_ACR_ComplianceReport", report.PK, accTaxReturn.ATR_ACR_ComplianceReport);
			AssertEquals("ATR_GovtReturnIdentifier", "", accTaxReturn.ATR_GovtReturnIdentifier);
			AssertEquals("ATR_GovtReceiptInformation", "", accTaxReturn.ATR_GovtReceiptInformation);
			AssertEquals("ATR_Status", "SAV", accTaxReturn.ATR_Status);
			AssertEquals("ATR_Version", 1, accTaxReturn.ATR_Version);

			var columns = accTaxReturn.Columns.Cast<AccTaxReturnColumn>().Where(c => c.ATC_GroupCode == "ADJ").ToArray();
			AssertEquals("B1_TotalVatBaseReceivables", 45M, columns.First(c => c.ATC_ColumnName == "B1_TotalVatBaseReceivables").ATC_Amount);
			AssertEquals("B2_TotalVatReceivables", 15M, columns.First(c => c.ATC_ColumnName == "B2_TotalVatReceivables").ATC_Amount);
			AssertEquals("B3_TotalVatBasePayables", 200M, columns.First(c => c.ATC_ColumnName == "B3_TotalVatBasePayables").ATC_Amount);
			AssertEquals("B4_TotalVatPayablesRecoverable", 250M, columns.First(c => c.ATC_ColumnName == "B4_TotalVatPayablesRecoverable").ATC_Amount);
			AssertEquals("B5_TotalVatPayablesNotRecoverable", 35M, columns.First(c => c.ATC_ColumnName == "B5_TotalVatPayablesNotRecoverable").ATC_Amount);
			AssertEquals("B7_BalancePreviousPeriod", 70M, columns.First(c => c.ATC_ColumnName == "B7_BalancePreviousPeriod").ATC_Amount);
		}

		LIQSubmissionDataColumns FillReportWithValidTestData()
		{
			Report.FillWithValidTestData();
			Report.ACR_ReportType = "LIQ";

			Factory.Save();

			var submissionData = LIQSubmissionDataTestHelper.CreateSubmissionData(Factory, Report);
			submissionData.Adjustments.Box1_TotalVatBaseReceivables = 45;
			submissionData.Adjustments.Box2_TotalVatReceivables = 15;
			submissionData.UpdateValuesToSubmit();

			Factory.Save();

			return submissionData;
		}

		void AssertSubmissionDataColumn(LIQSubmissionData expected, LIQSubmissionData actual)
		{
			AssertEquals(expected.Box1_TotalVatBaseReceivables, actual.Box1_TotalVatBaseReceivables);
			AssertEquals(expected.Box2_TotalVatReceivables, actual.Box2_TotalVatReceivables);
			AssertEquals(expected.Box3_TotalVatBasePayables, actual.Box3_TotalVatBasePayables);
			AssertEquals(expected.Box4_TotalVatPayablesRecoverable, actual.Box4_TotalVatPayablesRecoverable);
			AssertEquals(expected.Box5_TotalVatPayablesNotRecoverable, actual.Box5_TotalVatPayablesNotRecoverable);
			AssertEquals(expected.Box6_VatBalanceReceivablesAndPayables, actual.Box6_VatBalanceReceivablesAndPayables);
			AssertEquals(expected.Box7_BalancePreviousPeriod, actual.Box7_BalancePreviousPeriod);
			AssertEquals(expected.Box8_TotalBalance, actual.Box8_TotalBalance);
		}
	}
}
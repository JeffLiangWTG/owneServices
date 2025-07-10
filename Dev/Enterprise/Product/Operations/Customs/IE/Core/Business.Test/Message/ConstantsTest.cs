using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Business.Testing
{
	class AdditionalReferenceCodesTest : TestCaseWithFactory
	{
		public void TestIsEstimatedDeparture()
		{
			AssertEquals("IsEstimatedDeparture, Empty: false", false, Constants.AdditionalReferenceCodes.IsEstimatedDeparture(string.Empty));
			AssertEquals("IsEstimatedDeparture, Unrecognized input: false", false, Constants.AdditionalReferenceCodes.IsEstimatedDeparture("XXX"));
			AssertEquals("IsEstimatedDeparture, 1D24: false", false, Constants.AdditionalReferenceCodes.IsEstimatedDeparture(Constants.SupportingDocumentCodes._1D24));
			AssertEquals("IsEstimatedDeparture, 1D23: true", true, Constants.AdditionalReferenceCodes.IsEstimatedDeparture(Constants.AdditionalReferenceCodes.EstimatedTimeOfDeparture));
		}
	}

	class SupportingDocumentCodesTest : TestCaseWithFactory
	{
		public void TestIsEstimatedDestination()
		{
			AssertEquals("IsEstimatedDestination, Empty: false", false, Constants.SupportingDocumentCodes.IsEstimatedDestination(string.Empty));
			AssertEquals("IsEstimatedDestination, Unrecognized input: false", false, Constants.SupportingDocumentCodes.IsEstimatedDestination("XXX"));
			AssertEquals("IsEstimatedDestination, 1D24: true", true, Constants.SupportingDocumentCodes.IsEstimatedDestination(Constants.SupportingDocumentCodes._1D24));
			AssertEquals("IsEstimatedDestination, 1D23: false", false, Constants.SupportingDocumentCodes.IsEstimatedDestination(Constants.AdditionalReferenceCodes.EstimatedTimeOfDeparture));
		}
	}

	sealed class CustomsExciseReportTest : TestCaseWithFactory
	{
		static readonly ZDate testDate = new ZDate(2024, 7, 25);
		static readonly ZString expectedDateString = "20240725";
		public void TestUDR()
		{
			AssertEquals("/transactions/payer-unpaids-report", Constants.CustomsExciseReport.UDR);
		}

		public void TestBAL()
		{
			AssertEquals("/transactions/balance", Constants.CustomsExciseReport.BAL);
		}

		public void TestPSR()
		{
			AssertEquals($"/transactions/periods/{expectedDateString}/payer-summary-report", Constants.CustomsExciseReport.PSR(testDate));
		}

		public void TestPCT()
		{
			AssertEquals($"/transactions/periods/{expectedDateString}/payer-combined-taxes-report", Constants.CustomsExciseReport.PCT(testDate));
		}

		public void TestPTT()
		{
			AssertEquals($"/transactions/periods/{expectedDateString}/payer-tax-types-report", Constants.CustomsExciseReport.PTT(testDate));
		}

		public void TestPCI()
		{
			AssertEquals($"/transactions/periods/{expectedDateString}/importer-combined-taxes-report", Constants.CustomsExciseReport.PCI(testDate));
		}

		public void TestDSR()
		{
			AssertEquals($"/transactions/daily/{expectedDateString}/payer-summary-report", Constants.CustomsExciseReport.DSR(testDate));
		}

		public void TestDCT()
		{
			AssertEquals($"/transactions/daily/{expectedDateString}/payer-combined-taxes-report", Constants.CustomsExciseReport.DCT(testDate));
		}

		public void TestDTT()
		{
			AssertEquals($"/transactions/daily/{expectedDateString}/payer-tax-types-report", Constants.CustomsExciseReport.DTT(testDate));
		}
	}
}

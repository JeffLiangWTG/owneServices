using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.ComplianceReport.PTRS;
using Enterprise.Registry.Business;
using static Enterprise.Accounting.Business.AccountingConstants;

namespace Enterprise.Accounting.Business.Testing.ComplianceReport.PTRS
{
	public class PtrsReport2024HelperTest : TestCaseWithFactory
	{
		readonly ZDate ReportingFromDate = new ZDate(2024, 07, 01);
		readonly ZDate ReportingToDate = new ZDate(2024, 12, 31);
		TestObjectCreator Creator => creator ?? (creator = new TestObjectCreator(Factory));
		TestObjectCreator creator;

		public void TestCalculatePtrsReport2024Data()
		{
			var report = CreateComplianceReport(ComplianceReportTypes.PaymentTimesSmallBusinessReportType); //Use this old Report Type for this unit test, which is sufficient

			AddDataToPTSComplianceReport(report);

			var helper = new PtrsReport2024Helper();
			var allReportData = new PtrsAllPaymentsReport2024Data() //hard code The data for this unit test
			{
				OthersFullPaymentAmount = -10044.67m,
				OthersPartialPaymentAmount = -250m,
				SmallBusinessFullPaymentAmount = -88883.73m,
				SmallBusinessPartialPaymentAmount = -20407m
			};
			var reportData = helper.CalculatePtrsReport2024Data(report, allReportData);

			AssertEquals("ReportLines.Count", 24, report.ReportLines.Count);
			AssertEquals("MostCommonPaymentTerm", 30, reportData.MostCommonPaymentTerm);
			AssertEquals("AveragePaymentTime", 48.08m, reportData.AveragePaymentTime);
			AssertEquals("MedianPaymentTime", 40.50m, reportData.MedianPaymentTime);
			AssertEquals("PaymentTimeOf80thPercentile", 88, reportData.PaymentTimeOf80thPercentile);
			AssertEquals("PaymentTimeOf95thPercentile", 93, reportData.PaymentTimeOf95thPercentile);
			AssertEquals("PercentagPaidWithinTerm", 29.17m, reportData.PercentagePaidWithinTerm);
			AssertEquals("PercentagePaidWithin30days", 37.50m, reportData.PercentagePaidWithin30days);
			AssertEquals("PercentagePaidBetween31And60Days", 33.33m, reportData.PercentagePaidBetween31And60Days);
			AssertEquals("PercentagePaidAfter60Days", 29, 17m, reportData.PercentagePaidAfter60Days);
			AssertEquals("SmallBusinessPaymentPercentage", 91.39m, reportData.SmallBusinessPaymentPercentage);
			AssertEquals("PaymentTermMin", 17, reportData.PaymentTermMin);
			AssertEquals("PaymentTermMax", 60, reportData.PaymentTermMax);
		}

		/// <summary>
		/// This function is to test the scenario where payment to the small business is actually 0, in this case, the percentage value should be 0.
		/// According to the requirement , "The percentage calculated using the above formula must be rounded to 2 decimal places,
		/// but do not round down to 0. If the calculated percentage is less than 0.005%, please input 0.01 in this field."
		/// If the small business payment amount is actually, we are not rounding it down to 0, it is 0 in the first place.
		/// </summary>
		public void TestCalculatePtrsReport2024DataZeroSmallBusinessPayment()
		{
			var report = CreateComplianceReport(ComplianceReportTypes.PaymentTimesSmallBusinessReportType); //Use this old Report Type for this unit test, which is sufficient

			//AddDataToPTSComplianceReport(report);

			var helper = new PtrsReport2024Helper();
			var allReportData = new PtrsAllPaymentsReport2024Data() //hard code The data for this unit test
			{
				OthersFullPaymentAmount = 100000m,
				OthersPartialPaymentAmount = 0m,
				SmallBusinessFullPaymentAmount = 0m,
				SmallBusinessPartialPaymentAmount = 0m
			};
			var reportData = helper.CalculatePtrsReport2024Data(report, allReportData);

			AssertEquals("SmallBusinessPaymentPercentage", 0.0m, reportData.SmallBusinessPaymentPercentage);
		}

		/// <summary>
		/// This function is to test the scenario where the smallBusinessPaymentPercentage is below 0.005 based on the formula (non zero),
		/// in this scenario, the ultimate value of SmallBusinessPaymentPercentage is adjust to 0.01
		/// </summary>
		public void TestCalculatePtrsReport2024DataLowSmallBusinessPaymentPercentage()
		{
			var report = CreateComplianceReport(ComplianceReportTypes.PaymentTimesSmallBusinessReportType); //Use this old Report Type for this unit test, which is sufficient
			//AddDataToPTSComplianceReport(report);
			var helper = new PtrsReport2024Helper();
			var allReportData = new PtrsAllPaymentsReport2024Data() //hard code The data for this unit test
			{
				OthersFullPaymentAmount = 99995.01m,
				OthersPartialPaymentAmount = 0m,
				SmallBusinessFullPaymentAmount = 4.99m,
				SmallBusinessPartialPaymentAmount = 0m
			};
			var reportData = helper.CalculatePtrsReport2024Data(report, allReportData);

			AssertEquals("SmallBusinessPaymentPercentage", 0.01m, reportData.SmallBusinessPaymentPercentage);
		}

		public void TestCalculatePtrsAllPaymentsReport2024Data()
		{
			var report = CreateComplianceReport(ComplianceReportTypes.PaymentTimesAllPaymentsReportType); //Use this old Report Type for this unit test, which is sufficient

			AddDataToTCPComplianceReport(report);

			var helper = new PtrsReport2024Helper();
			var reportData = helper.CalculatePtrsAllPaymentsReport2024Data(report);

			AssertEquals("ReportLines.Count", 29, report.ReportLines.Count);

			AssertEquals("SmallBusinessFullPaymentAmount", -88883.73m, reportData.SmallBusinessFullPaymentAmount);
			AssertEquals("SmallBusinessPartialPaymentAmount", -20407m, reportData.SmallBusinessPartialPaymentAmount);
			AssertEquals("OthersFullPaymentAmount", -10044.67m, reportData.OthersFullPaymentAmount);
			AssertEquals("OthersPartialPaymentAmount", -250m, reportData.OthersPartialPaymentAmount);
		}

		public void TestCalculateMedianValue()
		{
			var helper = new PtrsReport2024Helper();

			//This list contains odd number of items,
			//use the very item in the middle as the median 
			var list = Enumerable.Range(1, 21).ToList();
			var actualResult = helper.CalculateMedianValue(list);
			AssertEquals(11m, actualResult);

			//This list contains even number (none zero) of items
			//use the avereage of the middle two values as Median
			list = Enumerable.Range(1, 20).ToList();
			actualResult = helper.CalculateMedianValue(list);
			AssertEquals(10.5m, actualResult);

			//This list is empty (zero items), the expected result will be zero
			list = new List<int>();
			actualResult = helper.CalculateMedianValue(list);
			AssertEquals(0m, actualResult);
		}

		public void TestGetPercentileValueFromList()
		{
			var helper = new PtrsReport2024Helper();

			//This list contains 100 items, the easies scenario where
			//the percentile value matches the numeric value stored in the list
			var list = Enumerable.Range(1, 100).ToList();
			var actualResult = helper.GetPercentileValueFromList(list, 80);
			AssertEquals(80, actualResult);

			actualResult = helper.GetPercentileValueFromList(list, 95);
			AssertEquals(95, actualResult);

			//This list contains smaller even number (none zero) of items
			//use the avereage of the middle two values as Median
			list = Enumerable.Range(1, 20).ToList();
			actualResult = helper.GetPercentileValueFromList(list, 80);
			AssertEquals(16, actualResult);
			actualResult = helper.GetPercentileValueFromList(list, 95);
			AssertEquals(19, actualResult);

			//This list is empty (zero items), the expected result will be zero
			list = new List<int>();
			actualResult = helper.GetPercentileValueFromList(list, 80);
			AssertEquals(0, actualResult);
			actualResult = helper.GetPercentileValueFromList(list, 95);
			AssertEquals(0, actualResult);
		}

		public void TestRoundDecimal()
		{
			var helper = new PtrsReport2024Helper();
			AssertEquals(95.46m, helper.RoundDecimalValue(95.4567m));
			AssertEquals(95.46m, helper.RoundDecimalValue(95.456m));
			AssertEquals(95.50m, helper.RoundDecimalValue(95.50m));
		}

		AccComplianceReport CreateComplianceReport(string reportType)
		{
			var report = Factory.NewWithValidTestData<AccComplianceReport>();
			report.ACR_ReportType = reportType; 
			report.ACR_Periodicity = ComplianceReportConfigurationLookups.ReportPeriodicityCodes.DateRange;
			report.ACR_DateFrom = ReportingFromDate;
			report.ACR_DateTo = ReportingToDate;
			report.ACR_Status = AccComplianceReport.Status.ReportGenerated;
			Creator.CreateConfigurationForComplianceReport(report, "AH", reportType);
			return report;
		}

		void AddDataToPTSComplianceReport(AccComplianceReport report)
		{
			var sequence = 0;

			//create one single dummy invoice to be associated with the records in the table AccComplianceReportTransactionPivot,
			// the test is really about the values in the column ReportSubCode in this Pivot table, no need to create many invoices
			var invoice = Creator.CreateAPInvoice<APInvoice>("invoice1" , Creator.AUD, 1m, 1m, 0m, 0m, 1m, 0m, 0m, Creator.ABIGAS);
			Factory.Save();

			addTransactionPivotRecord(invoice, -638.00m, 63, -2, 60);
			addTransactionPivotRecord(invoice, -520.00m, 60, -29, 30);
			addTransactionPivotRecord(invoice, -0.00m, 52, -31, 30);
			addTransactionPivotRecord(invoice, -0.00m, 94, -65, 30);
			addTransactionPivotRecord(invoice, -900.00m, 44, -40, 30);
			addTransactionPivotRecord(invoice, -700.00m, 26, -8, 17);
			addTransactionPivotRecord(invoice, -2750.00m, 89, -58, 30);
			addTransactionPivotRecord(invoice, -550.00m, 11, 16, 31);
			addTransactionPivotRecord(invoice, -4000.00m, 91, -88, 30);
			addTransactionPivotRecord(invoice, -462.00m, 28, 2, 30);
			addTransactionPivotRecord(invoice, -1962.00m, 115, -89, 30);
			addTransactionPivotRecord(invoice, -5500.00m, 93, -93, 30);
			addTransactionPivotRecord(invoice, -1870.00m, 58, -34, 30);
			addTransactionPivotRecord(invoice, -577.50m, 36, -7, 30);
			addTransactionPivotRecord(invoice, -742.50m, 1, 28, 30);
			addTransactionPivotRecord(invoice, -407.00m, 37, -36, 0);
			addTransactionPivotRecord(invoice, -1625.00m, 88, -59, 30);
			addTransactionPivotRecord(invoice, -2275.01m, 56, -30, 30);
			addTransactionPivotRecord(invoice, -3146.00m, 36, -38, 0);
			addTransactionPivotRecord(invoice, -20.00m, 19, 10, 30);
			addTransactionPivotRecord(invoice, -250.00m, 16, -19, 0);
			addTransactionPivotRecord(invoice, -100.00m, 2, 29, 30);
			addTransactionPivotRecord(invoice, -902.72m, 12, 12, 30);
			addTransactionPivotRecord(invoice, -3300.00m, 27, 4, 30);

			void addTransactionPivotRecord(APInvoice invoice, decimal amount, int paymentTimes, int aheadOfDueDays, int paymentTerms)
			{
				Creator.CreateComplianceReportTransactionPivot(report, invoice, (++sequence), $"{amount:0.00}|{paymentTimes}|{aheadOfDueDays}|{paymentTerms}");
			}
		}

		void AddDataToTCPComplianceReport(AccComplianceReport report)
		{
			var sequence = 0;

			//create one single dummy invoice to be associated with the records in the table AccComplianceReportTransactionPivot,
			// the test is really about the values in the column ReportSubCode in this Pivot table, no need to create many invoices
			var invoice = Creator.CreateAPInvoice<APInvoice>("invoice1", Creator.AUD, 1m, 1m, 0m, 0m, 1m, 0m, 0m, Creator.ABIGAS);
			Factory.Save();

			addTransactionPivotRecord(invoice, -638.00m, 3);
			addTransactionPivotRecord(invoice, -520.00m, 3);
			addTransactionPivotRecord(invoice, -900.00m, 3);
			addTransactionPivotRecord(invoice, -700.00m, 3);
			addTransactionPivotRecord(invoice, -2750.00m, 1);
			addTransactionPivotRecord(invoice, -30000.00m, 3);
			addTransactionPivotRecord(invoice, -275.00m, 3);
			addTransactionPivotRecord(invoice, -4000.00m, 1);
			addTransactionPivotRecord(invoice, -6600.00m, 3);
			addTransactionPivotRecord(invoice, -462.00m, 3);
			addTransactionPivotRecord(invoice, -1424.67m, 1);
			addTransactionPivotRecord(invoice, -20000.00m, 2);
			addTransactionPivotRecord(invoice, -3200.00m, 3);
			addTransactionPivotRecord(invoice, -5500.00m, 3);
			addTransactionPivotRecord(invoice, -1870.00m, 1);
			addTransactionPivotRecord(invoice, -500.00m, 3);
			addTransactionPivotRecord(invoice, -577.50m, 3);
			addTransactionPivotRecord(invoice, -742.50m, 3);
			addTransactionPivotRecord(invoice, -407.00m, 2);
			addTransactionPivotRecord(invoice, -200.00m, 3);
			addTransactionPivotRecord(invoice, -1500.00m, 3);
			addTransactionPivotRecord(invoice, -2100.01m, 3);
			addTransactionPivotRecord(invoice, -3146.00m, 3);
			addTransactionPivotRecord(invoice, -20.00m, 3);
			addTransactionPivotRecord(invoice, -250.00m, 0);
			addTransactionPivotRecord(invoice, -27000.00m, 3);
			addTransactionPivotRecord(invoice, -100.00m, 3);
			addTransactionPivotRecord(invoice, -902.72m, 3);
			addTransactionPivotRecord(invoice, -3300.00m,3);

			void addTransactionPivotRecord(APInvoice invoice, decimal amount, int flag)
			{
				Creator.CreateComplianceReportTransactionPivot(report, invoice, (++sequence), $"{amount:0.00}|{flag}");
			}
		}
	}
}

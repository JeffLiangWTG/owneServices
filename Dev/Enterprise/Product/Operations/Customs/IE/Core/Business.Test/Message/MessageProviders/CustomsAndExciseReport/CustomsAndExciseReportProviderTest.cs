using System;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IE.Business.Testing.Message.MessageProviders.CustomsAndExciseReport
{
	sealed class CustomsAndExciseReportProviderTest : DataProviderTestCase<CustomsAndExciseReportProvider>
	{
		static readonly ZDate testDate = new ZDate(2024, 7, 25);
		static readonly ZString expectedDateString = "20240725";
		static readonly ZString serverApiUrl = "https://www.example.ie/api/";
		static readonly ZString testReportType = "BAL";
		protected override CustomsAndExciseReportProvider GetProvider() => new CustomsAndExciseReportProvider(Factory, testReportType, ZDate.Empty);

		public void TestReportUrl()
		{
			AssertEquals($"{serverApiUrl}/transactions/balance", Provider.ReportUrl);
		}

		public void TestDateRequiredMessageWithNoDateProvided()
		{
			AssertExceptionThrown<ArgumentException>(() => { var provider = new CustomsAndExciseReportProvider(Factory, "PSR", ZDate.Empty); });
		}

		public void TestPSR()
		{
			var provider = new CustomsAndExciseReportProvider(Factory, CustomsAndExciseReportTypeList.Codes.PSR, testDate);
			AssertEquals($"{serverApiUrl}/transactions/periods/{expectedDateString}/payer-summary-report", provider.ReportUrl);
		}

		public void TestPCT()
		{
			var provider = new CustomsAndExciseReportProvider(Factory, CustomsAndExciseReportTypeList.Codes.PCT, testDate);
			AssertEquals($"{serverApiUrl}/transactions/periods/{expectedDateString}/payer-combined-taxes-report", provider.ReportUrl);
		}

		public void TestPTT()
		{
			var provider = new CustomsAndExciseReportProvider(Factory, CustomsAndExciseReportTypeList.Codes.PTT, testDate);
			AssertEquals($"{serverApiUrl}/transactions/periods/{expectedDateString}/payer-tax-types-report", provider.ReportUrl);
		}

		public void TestPCI()
		{
			var provider = new CustomsAndExciseReportProvider(Factory, CustomsAndExciseReportTypeList.Codes.PCI, testDate);
			AssertEquals($"{serverApiUrl}/transactions/periods/{expectedDateString}/importer-combined-taxes-report", provider.ReportUrl);
		}

		public void TestDSR()
		{
			var provider = new CustomsAndExciseReportProvider(Factory, CustomsAndExciseReportTypeList.Codes.DSR, testDate);
			AssertEquals($"{serverApiUrl}/transactions/daily/{expectedDateString}/payer-summary-report", provider.ReportUrl);
		}

		public void TestDCT()
		{
			var provider = new CustomsAndExciseReportProvider(Factory, CustomsAndExciseReportTypeList.Codes.DCT, testDate);
			AssertEquals($"{serverApiUrl}/transactions/daily/{expectedDateString}/payer-combined-taxes-report", provider.ReportUrl);
		}

		public void TestDTT()
		{
			var provider = new CustomsAndExciseReportProvider(Factory, CustomsAndExciseReportTypeList.Codes.DTT, testDate);
			AssertEquals($"{serverApiUrl}/transactions/daily/{expectedDateString}/payer-tax-types-report", provider.ReportUrl);
		}

		public void TestUDR()
		{
			var provider = new CustomsAndExciseReportProvider(Factory, CustomsAndExciseReportTypeList.Codes.UDR, ZDate.Empty);
			AssertEquals($"{serverApiUrl}/transactions/payer-unpaids-report", provider.ReportUrl);
		}

		public void TestBAL()
		{
			var provider = new CustomsAndExciseReportProvider(Factory, CustomsAndExciseReportTypeList.Codes.BAL, ZDate.Empty);
			AssertEquals($"{serverApiUrl}/transactions/balance", provider.ReportUrl);
		}

		protected override void SetUp()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			const string configCode = "IEIERSUBMT";
			helper.CreateRefSysConfigType(configCode, "ROS Production REST URL for C&E transactions", "Revenue Online Services Production REST URL for retrieval of Customs & Excise transactions");
			helper.CreateRefSysConfig(configCode, serverApiUrl, ZDateTime.BrettsBirthday, ZDateTime.Empty);
			Factory.Save();
		}
	}
}

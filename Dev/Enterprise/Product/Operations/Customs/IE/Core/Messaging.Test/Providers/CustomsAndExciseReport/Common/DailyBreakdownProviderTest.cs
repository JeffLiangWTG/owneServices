using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.Testing
{
	public class DailyBreakdownProviderTest : TestCaseWithFactory
	{
		DailyBreakdownProvider provider;

		public void TestDate()
		{
			provider = new DailyBreakdownProvider(new DailyBreakdown { Date = "20220801" });
			AssertEquals(new ZDateTime(2022, 8, 1), provider.Date);
		}

		public void TestDate_XlsxField() => typeof(DailyBreakdownProvider).TestXlsxField(nameof(DailyBreakdownProvider.Date), 1, "Date");

		public void TestTaxTotal()
		{
			provider = new DailyBreakdownProvider(new DailyBreakdown() { TaxTotal = 150.0M });
			AssertEquals(150.0M, provider.TaxTotal);
			AssertEquals("150.00", provider.TaxTotal.ToString("#,0.00"));
		}
		public void TestTaxTotal_XlsxField() => typeof(DailyBreakdownProvider).TestXlsxField(nameof(DailyBreakdownProvider.TaxTotal), 2, "Tax Total");
	}
}

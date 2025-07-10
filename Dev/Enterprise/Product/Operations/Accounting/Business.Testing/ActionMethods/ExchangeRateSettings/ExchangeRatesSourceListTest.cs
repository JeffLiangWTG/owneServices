using Enterprise.Accounting.Integration;

namespace Enterprise.Accounting.Business.Testing
{
	using System.Text;
	using NUnit.Framework;

	internal class ExchangeRatesSourceListTest : TestCase
	{
		public void TestDefaultExchangeRatesSourceList()
		{
			StringBuilder expected = new StringBuilder();
			ExchangeRatesSourceList list = new ExchangeRatesSourceList(null);
			expected.AppendFormat("{0} - {1}", ExchangeRatesSourceList.Codes.CurrencyFile, ExchangeRatesSourceList.Descriptions.CurrencyFile);
			expected.AppendLine();
			AssertEquals("Should be Only CFR", expected.ToString().Trim(), list.ElementsAsString);
			list = new ExchangeRatesSourceList(System.Array.Empty<ExRateSourceType>());
			AssertEquals("Should be Only CFR", expected.ToString().Trim(), list.ElementsAsString);
		}

		public void TestExchangeRatesSourceListForVoyage()
		{
			StringBuilder expected = new StringBuilder();
			expected.AppendFormat("{0} - {1}", ExchangeRatesSourceList.Codes.CurrencyFile, ExchangeRatesSourceList.Descriptions.CurrencyFile);
			expected.AppendLine();
			expected.AppendFormat("{0} - {1}", ExchangeRatesSourceList.Codes.SailingSchedule, ExchangeRatesSourceList.Descriptions.SailingSchedule);
			expected.AppendLine();
			var list = new ExchangeRatesSourceList(new ExRateSourceType[] { ExRateSourceType.Voyage });
			AssertEquals("Should be CFR & SSR", expected.ToString().Trim(), list.ElementsAsString);
		}

		public void TestExchangeRatesSourceListForBillingJobs()
		{
			StringBuilder expected = new StringBuilder();
			expected.AppendFormat("{0} - {1}", ExchangeRatesSourceList.Codes.CurrencyFile, ExchangeRatesSourceList.Descriptions.CurrencyFile);
			expected.AppendLine();
			expected.AppendFormat("{0} - {1}", ExchangeRatesSourceList.Codes.BillingJobExRateRegistry, ExchangeRatesSourceList.Descriptions.BillingJobExRateRegistry);
			expected.AppendLine();
			var list = new ExchangeRatesSourceList(new ExRateSourceType[] { ExRateSourceType.BillingJob });
			AssertEquals("Should be CFR & RFR", expected.ToString().Trim(), list.ElementsAsString);
		}
	}
}

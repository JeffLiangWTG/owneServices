using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities.Testing
{
	sealed class CurrencyToWords_ENTest : TransactionedTestCase
	{
		public void TestCurrencyToWordsForDifferentCurrencies()
		{
			var cnyCurrency = new BusinessObjectFactory().LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "CNY");
			cnyCurrency.RX_UnitName = "yuan"; //As commented in the work item, the RefCurrency is not up to date, so some times we need to manually update it.
			cnyCurrency.Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("two taka only", new CurrencyToWords_EN().ConvertToWords(2, "BDT"));
				AssertEquals("two yuan only", new CurrencyToWords_EN().ConvertToWords(2, "CNY"));
				AssertEquals("two yen only", new CurrencyToWords_EN().ConvertToWords(2, "JPY"));
				AssertEquals("two won only", new CurrencyToWords_EN().ConvertToWords(2, "KRW"));
				AssertEquals("two won only", new CurrencyToWords_EN().ConvertToWords(2, "KPW"));
				AssertEquals("two reais only", new CurrencyToWords_EN().ConvertToWords(2, "BRL"));
				AssertEquals("two kronor only", new CurrencyToWords_EN().ConvertToWords(2, "SEK"));
				AssertEquals("two bolívares only", new CurrencyToWords_EN().ConvertToWords(2, "VEB"));
				AssertEquals("two ariary only", new CurrencyToWords_EN().ConvertToWords(2, "MGA"));
			});
		}

		public void TestReplacement()
		{
			AssertEquals("twelve dollars only", (new CurrencyToWords_EN()).ConvertToWords(12, "USD"));
			AssertEquals("one dollar only", (new CurrencyToWords_EN()).ConvertToWords(1, "USD"));
			AssertEquals("one dollar and 50 cents", (new CurrencyToWords_EN()).ConvertToWords(1.5, "USD"));
			AssertEquals("zero dollars only", (new CurrencyToWords_EN()).ConvertToWords(0, "USD"));
			AssertEquals("thirty two dollars only", (new CurrencyToWords_EN()).ConvertToWords(32, "USD"));
			AssertEquals("twelve dollars and 20 cents", (new CurrencyToWords_EN()).ConvertToWords(12.2, "USD"));
		}
	}
}

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities.Testing
{
	sealed class CurrencyToWords_RU_RUTest : SlavicCurrencyConverterTest
	{
		public void TestReplacement()
		{
			var rsn = new CurrencyToWords_RU_RU();
			CombineAssertions(delegate
			{
				AssertEquals("ноль рублей двенадцать копеек", rsn.ConvertToWords(0.12, "RUB"));
				AssertEquals("один рубль двадцать три копейки", rsn.ConvertToWords(1.23, "RUB"));
				AssertEquals("два рубля тридцать четыре копейки", rsn.ConvertToWords(2.34, "RUB"));
				AssertEquals("три рубля сорок пять копеек", rsn.ConvertToWords(3.45, "RUB"));
				AssertEquals("четыре рубля пятьдесят шесть копеек", rsn.ConvertToWords(4.56, "RUB"));
				AssertEquals("пять рублей шестьдесят семь копеек", rsn.ConvertToWords(5.67, "RUB"));
				AssertEquals("одна тысяча семьсот пять рублей", rsn.ConvertToWords(1705.00, "RUB"));
				AssertEquals("шестьдесят семь XXX", rsn.ConvertToWords(67, "XXX"));
			});
		}

		#region implementation

		protected override SlavicCurrencyConverter Converter
		{
			get { return new CurrencyToWords_RU_RU(); }
		}

		protected override string Language
		{
			get { return Core.Constants.Languages.Russian; }
		}

		#endregion
	}
}

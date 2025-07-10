namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class TariffFormatterTest : Customs.Business.Testing.TariffFormatterTest
	{
		public override void TestFormat()
		{
			AssertEquals("12345678900", Formatter.Format("123. 45 .67.89 00 "));
		}

		public override void TestDisplayFormat()
		{
			AssertEquals("1234.56-7890", Formatter.DisplayFormat("123. 45 .67.89 0 "));
			AssertEquals("12345678900", Formatter.DisplayFormat("123. 45 .67.89 00 "));
		}

		protected override Customs.Business.TariffFormatter GetNewTariffFormatter()
		{
			return new TariffFormatter();
		}
	}
}

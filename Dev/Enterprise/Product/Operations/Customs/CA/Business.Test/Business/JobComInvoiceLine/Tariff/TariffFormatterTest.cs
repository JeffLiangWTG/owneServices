namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class TariffFormatterTest : Customs.Business.Testing.TariffFormatterTest
	{
		public override void TestFormat()
		{
			AssertEquals("1234567890", Formatter.Format("123. 45 .67.890"));
			AssertEquals("99420000", Formatter.Format("9942"));
		}

		protected override Customs.Business.TariffFormatter GetNewTariffFormatter()
		{
			return new TariffFormatter();
		}
	}
}

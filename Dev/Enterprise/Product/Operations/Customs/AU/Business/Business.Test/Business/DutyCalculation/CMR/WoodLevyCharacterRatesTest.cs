using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class WoodLevyCharacterRatesTest : TestCase
	{
		public void TestHasCharacter()
		{
			AssertEquals("Has 23", true, testRates.HasCharacter("23"));
			AssertEquals("Does not have 0", false, testRates.HasCharacter("0"));
		}

		public void TestRatesForEachCharacter()
		{
			AssertEquals(0.58m, testRates.GetRateWithCharacterCode("23"));
			AssertEquals(0.58m, testRates.GetRateWithCharacterCode("17"));
			AssertEquals(0.72m, testRates.GetRateWithCharacterCode("25"));
			AssertEquals(0.72m, testRates.GetRateWithCharacterCode("19"));
			AssertEquals(0.30m, testRates.GetRateWithCharacterCode("22"));
			AssertEquals(0.72m, testRates.GetRateWithCharacterCode("24"));
			AssertEquals(0.72m, testRates.GetRateWithCharacterCode("18"));
			AssertEquals(0.15m, testRates.GetRateWithCharacterCode("27"));
			AssertEquals(0.17m, testRates.GetRateWithCharacterCode("26"));
			AssertEquals(0.37m, testRates.GetRateWithCharacterCode("20"));
			AssertEquals(0.0003m, testRates.GetRateWithCharacterCode("21"));
			AssertEquals(0m, testRates.GetRateWithCharacterCode("0"));
		}

		WoodLevyCharacterRates testRates;
		protected override void SetUp()
		{
			base.SetUp();
			testRates = new WoodLevyCharacterRates();
		}
	}
}

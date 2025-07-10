using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class GSTExemptionCalculatorTest : TestCaseWithFactory
	{
		public void TestHasGSTExemptItem()
		{
			var dummy = new DummyGSTExempt();
			dummy.CharacteristicCodeExposed = ZShort.Parse(CharacteristicCodeList.Codes.NonTaxableImports);
			dummy.RateCodeExposed = "000";
			dummy.RateNumberExposed = "111";
			dummy.PrefSchemeExposed = "GEN";

			var testCalculator = new GSTExemptionCalculator(new IGSTExempt[] { dummy });

			AssertEquals("HasGSTExemptItem", false, testCalculator.IsThisGSTExempt("000", "000", "GEN"));
			AssertEquals("HasGSTExemptItem", false, testCalculator.IsThisGSTExempt("000", "111", ""));
			AssertEquals("HasGSTExemptItem", true, testCalculator.IsThisGSTExempt("000", "111", "GEN"));
		}

		sealed class DummyGSTExempt : IGSTExempt
		{
			public ZShort CharacteristicCodeExposed;
			public ZShort CharacteristicCode => CharacteristicCodeExposed;

			public ZString RateCodeExposed;
			public ZString RateCode => RateCodeExposed;

			public ZString RateNumberExposed;
			public ZString RateNumber => RateNumberExposed;

			public ZString PrefSchemeExposed;
			public ZString PrefScheme => PrefSchemeExposed;
		}
	}
}

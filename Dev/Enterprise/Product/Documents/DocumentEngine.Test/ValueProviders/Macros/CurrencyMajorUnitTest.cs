using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(CurrencyMajorUnit))]
	sealed class CurrencyMajorUnitTest : ValueProviderWithLoadControlFactoryTest<CurrencyMajorUnit>
	{
		public override void TestIsResponsibleForReplacing()
		{
			Assert("Should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.SecondPass));
			Assert("Should not match <CurrencyMajorUnit>", !ValueProviderToTest.IsResponsibleForReplacing("<CurrencyMajorUnit>", Passes.SecondPass));
			Assert("Should not match <CurrencyMajorUnit()>", !ValueProviderToTest.IsResponsibleForReplacing("<CurrencyMajorUnit()>", Passes.SecondPass));
			Assert("Should not match < CurrencyMajorUnit ( ) >", !ValueProviderToTest.IsResponsibleForReplacing("<CurrencyMajorUnit()>", Passes.SecondPass));
			Assert("Should match <CurrencyMajorUnit(USD)>", ValueProviderToTest.IsResponsibleForReplacing("<CurrencyMajorUnit(USD)>", Passes.SecondPass));
			Assert("Should match < CurrencyMajorUnit ( USD ) >", ValueProviderToTest.IsResponsibleForReplacing("<CurrencyMajorUnit( USD)>", Passes.SecondPass));
		}

		public override void TestReplacement()
		{
			AssertEquals("dollar", ValueProviderToTest.GetReplacement("<CurrencyMajorUnit(USD)>", Report));
			AssertEquals("rupee", ValueProviderToTest.GetReplacement("<CurrencyMajorUnit( INR)>", Report));
			AssertEquals("yen", ValueProviderToTest.GetReplacement("<CurrencyMajorUnit(JPY )>", Report));
		}
	}
}

using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(CurrencySubUnitFormat))]
	sealed class CurrencySubUnitFormatTest : ValueProviderWithLoadControlFactoryTest<CurrencySubUnitFormat>
	{
		public override void TestIsResponsibleForReplacing()
		{
			Assert("Should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.SecondPass));
			Assert("Should not match <CurrencySubUnitFormat>", !ValueProviderToTest.IsResponsibleForReplacing("<CurrencySubUnitFormat>", Passes.SecondPass));
			Assert("Should not match <CurrencySubUnitFormat()>", !ValueProviderToTest.IsResponsibleForReplacing("<CurrencySubUnitFormat()>", Passes.SecondPass));
			Assert("Should not match < CurrencySubUnitFormat ( ) >", !ValueProviderToTest.IsResponsibleForReplacing("<CurrencySubUnitFormat()>", Passes.SecondPass));
			Assert("Should match <CurrencySubUnitFormat(USD)>", ValueProviderToTest.IsResponsibleForReplacing("<CurrencySubUnitFormat(USD)>", Passes.SecondPass));
			Assert("Should match < CurrencySubUnitFormat ( USD ) >", ValueProviderToTest.IsResponsibleForReplacing("<CurrencySubUnitFormat( USD)>", Passes.SecondPass));
		}

		public override void TestReplacement()
		{
			AssertEquals("#,##0.00", ValueProviderToTest.GetReplacement("<CurrencySubUnitFormat(USD)>", Report));
			AssertEquals("#,##0.000", ValueProviderToTest.GetReplacement("<CurrencySubUnitFormat( BHD)>", Report));
			AssertEquals("#,##0", ValueProviderToTest.GetReplacement("<CurrencySubUnitFormat(KRW )>", Report));
		}
	}
}

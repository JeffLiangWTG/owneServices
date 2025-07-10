using Enterprise.Barcode.Business;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(SSCCCheckDigit))]
	sealed class SSCCCheckDigitTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<SSCCCheckDigit(34012345123458789)>", Passes.SecondPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<SSCCCheckDigit>", Passes.SecondPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<   bar code  >", Passes.SecondPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<   SSCCCheckDigit somefield   >", Passes.SecondPass));
			Assert("should not match as it's first pass", !ValueProviderToTest.IsResponsibleForReplacing("<   SSCCCheckDigit  \t  (     ' 34012345123458789 ' )   >", Passes.FirstPass));
			Assert("should match ", ValueProviderToTest.IsResponsibleForReplacing("<SSCCCheckDigit('34012345123458789')>", Passes.SecondPass));
			Assert("should match ", ValueProviderToTest.IsResponsibleForReplacing("<   SSCCCheckDigit  \t  (     ' 34012345123458789 ' )   >", Passes.SecondPass));
			Assert("should match ", ValueProviderToTest.IsResponsibleForReplacing(@"<SSCCCheckDigit('0033588600<SubString('<JobNumber >', 3)>')>", Passes.SecondPass));
		}

		public void TestReplacement()
		{
			PrepareRenderer();
			var textBarcode = new TextBarcode("34012345123458789");
			AssertEquals("3", ValueProviderToTest.GetReplacement("<SSCCCheckDigit('34012345123458789')>", Report));

			textBarcode = new TextBarcode(" 34012345123458789 ", true);
			AssertEquals("3", ValueProviderToTest.GetReplacement("<SSCCCheckDigit(' 34012345123458789 ')>", Report));

			textBarcode = new TextBarcode(" 34012345678901234 ", true);
			AssertEquals("0", ValueProviderToTest.GetReplacement("<SSCCCheckDigit(' 34012345678901234 ')>", Report));
		}

		[ExpectException(typeof(FieldNotFoundException))]
		public void TestInvalidInputValueThrowFieldNotFoundException()
		{
			ValueProviderToTest.GetReplacement("<SSCCCheckDigit('340abc45123458789')>", Report);
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new SSCCCheckDigit();
		}
	}
}

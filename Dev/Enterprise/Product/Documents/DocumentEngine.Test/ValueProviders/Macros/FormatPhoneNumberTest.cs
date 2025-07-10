using Enterprise.DocumentEngine.ValueProviders.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(FormatPhoneNumber))]
	sealed class FormatPhoneNumberTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("Should not pass", !ValueProviderToTest.IsResponsibleForReplacing("FormatPhoneNumber", Passes.FirstPass));
			Assert("Should not pass", !ValueProviderToTest.IsResponsibleForReplacing("<FormatPhoneNumber meh>", Passes.FirstPass));
			Assert("Should not pass", !ValueProviderToTest.IsResponsibleForReplacing("<FormatPhoneNumber(\" + 86 156 0113 1981\", \"NatiONAL\", \"blah\")>", Passes.FirstPass));
			Assert("Should not pass", !ValueProviderToTest.IsResponsibleForReplacing("<If(\"<Bill_1>\" == \"<Bill_1>\", \"<FormatPhoneNumber(\"012491281\",\"DEF\")>\",\"false\")>", Passes.FirstPass));
			Assert("Should pass", ValueProviderToTest.IsResponsibleForReplacing("<FormatPhoneNumber(\"912373128\",\"DEF\")>", Passes.FirstPass));
			Assert("Should pass", ValueProviderToTest.IsResponsibleForReplacing("<FORMATphoneNumber(\"012491281\",\"DEF\")>", Passes.FirstPass));
			Assert("Should pass", ValueProviderToTest.IsResponsibleForReplacing("< FormatPhoneNumber(          \"12312512\",  \"ASD\")    >", Passes.FirstPass));
			Assert("Should pass", ValueProviderToTest.IsResponsibleForReplacing("< FormatPhoneNumber (\"\",\"Hello World\")>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			// Arrange
			// Act
			var validReplacementForE164 = ValueProviderToTest.GetReplacement("<FormatPhoneNumber(\"+61 426 829 924\",\"E164\")>", Report);
			var invalidReplacementForE164 = ValueProviderToTest.GetReplacement("<FormatPhoneNumber(\"ABC DEF GHI\",\"E164\")>", Report);
			var validReplacementForInternational = ValueProviderToTest.GetReplacement("<FormatPhoneNumber(\"+61 280012200\",\"international\")>", Report);
			var invalidReplacementForInternational = ValueProviderToTest.GetReplacement("<FormatPhoneNumber(\"2 8001 2200\",\"INTERNATIONAL\")>", Report);
			var validReplacementForNational = ValueProviderToTest.GetReplacement("<FormatPhoneNumber(\"+86 156 0113 1981\",\"NatiONAL\")>", Report);
			var invalidReplacementForNational = ValueProviderToTest.GetReplacement("<FormatPhoneNumber(\"123123\",\"NATIONAL\")>", Report);
			// Assert
			AssertEquals("Incorrect format for E164", "+61426829924", validReplacementForE164);
			AssertEquals("Incorrect format for E164", "ABC DEF GHI", invalidReplacementForE164);
			AssertEquals("Incorrect format for international", "+61 2 8001 2200", validReplacementForInternational);
			AssertEquals("Incorrect format for international", "2 8001 2200", invalidReplacementForInternational);
			AssertEquals("Incorrect format for national", "156 0113 1981", validReplacementForNational);
			AssertEquals("Incorrect format for national", "123123", invalidReplacementForNational);
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new FormatPhoneNumber();
		}
	}
}

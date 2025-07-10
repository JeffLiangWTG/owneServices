using CargoWise.Common;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(LanguageFromCode))]
	sealed class LanguageFromCodeTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<languagefromcode>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<   Language From Code   >", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<   LanguageFromCode somefield   >", Passes.FirstPass));
			Assert("should match <LanguageFromCode(AField)>", ValueProviderToTest.IsResponsibleForReplacing("<LanguageFromCode(AField)>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<   LanguageFromCode(AField, other)   >", Passes.SecondPass));
			Assert("should match ", ValueProviderToTest.IsResponsibleForReplacing("<LanguageFromCode()>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			AssertEquals("English (American)", ValueProviderToTest.GetReplacement("<LanguageFromCode(EN-US)>", Report));
		}

		public void TestReplacementForNoLanguage()
		{
			ErrorReporter.Clear();

			AssertEquals("Empty description returned", "", ValueProviderToTest.GetReplacement("<LanguageFromCode()>", Report));
			AssertEquals("No Error for blank lang code", "", ErrorReporter.LastMessageReported);

			Report.MenuItem.SU_IsSystemDefined = true;
			AssertEquals("Empty description returned", "", ValueProviderToTest.GetReplacement("<LanguageFromCode(XYZ)>", Report));
			Report.ErrorManager.ReportErrors();
			AssertContains("Error for invalid lang code", "The language code [XYZ] has no corresponding description in the macro <LanguageFromCode>.", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
			Report.ErrorManager.ClearErrors();

			Report.MenuItem.SU_IsSystemDefined = false;
			AssertEquals("Empty description returned", "", ValueProviderToTest.GetReplacement("<LanguageFromCode(XYZ)>", Report));
			Report.ErrorManager.ReportErrors();
			AssertEquals("No error reported to CW1 for customized documents", "", ErrorReporter.LastMessageReported);

			AssertEquals("Correct description returned", "English (American)", ValueProviderToTest.GetReplacement("<LanguageFromCode(EN-US)>", Report));
			AssertEquals("No error for valid lang code", "", ErrorReporter.LastMessageReported);
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new LanguageFromCode();
		}
	}
}

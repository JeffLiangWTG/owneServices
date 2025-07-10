using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class ValidationUtilityTest : TestCaseWithFactory
{
	public void TestIsStartWithOneOrMoreDigitsAndEndWithAnUppercaseAlphabeticCharacter()
	{
		CombineAssertions("Assert ValidateApprovedLocationAuthorisationNumber", () =>
		{
			AssertEquals("string.Empty", expected: false, ValidationUtility.IsStartWithOneOrMoreDigitsAndEndWithAnUppercaseAlphabeticCharacter(string.Empty));
			AssertEquals("null", expected: false, ValidationUtility.IsStartWithOneOrMoreDigitsAndEndWithAnUppercaseAlphabeticCharacter(null));
			AssertEquals("0", expected: false, ValidationUtility.IsStartWithOneOrMoreDigitsAndEndWithAnUppercaseAlphabeticCharacter("0"));
			AssertEquals("A", expected: false, ValidationUtility.IsStartWithOneOrMoreDigitsAndEndWithAnUppercaseAlphabeticCharacter("A"));
			AssertEquals("01234", expected: false, ValidationUtility.IsStartWithOneOrMoreDigitsAndEndWithAnUppercaseAlphabeticCharacter("01234"));
			AssertEquals("ABCDE", expected: false, ValidationUtility.IsStartWithOneOrMoreDigitsAndEndWithAnUppercaseAlphabeticCharacter("ABCDE"));
			AssertEquals("0!@#A", expected: false, ValidationUtility.IsStartWithOneOrMoreDigitsAndEndWithAnUppercaseAlphabeticCharacter("0!@#A"));
			AssertEquals("01234ABCDE", expected: false, ValidationUtility.IsStartWithOneOrMoreDigitsAndEndWithAnUppercaseAlphabeticCharacter("01234ABCDE"));
			AssertEquals("01234A", expected: true, ValidationUtility.IsStartWithOneOrMoreDigitsAndEndWithAnUppercaseAlphabeticCharacter("01234A"));
		});
	}
}

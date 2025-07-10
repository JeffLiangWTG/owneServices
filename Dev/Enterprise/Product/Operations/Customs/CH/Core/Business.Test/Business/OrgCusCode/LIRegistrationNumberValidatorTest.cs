using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

public class LIRegistrationNumberValidatorTest : TestCase
{
	public void TestIsValidVATNumber()
	{
		AssertEquals("Valid", true, LIRegistrationNumberValidator.IsValidVATNumber("12345"));
		AssertEquals("Too long", false, LIRegistrationNumberValidator.IsValidVATNumber("123456"));
		AssertEquals("Too short", false, LIRegistrationNumberValidator.IsValidVATNumber("1234"));
		AssertEquals("No digit", false, LIRegistrationNumberValidator.IsValidVATNumber("1x345"));
		AssertEquals("Empty", false, LIRegistrationNumberValidator.IsValidVATNumber(""));
	}
}

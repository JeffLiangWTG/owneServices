using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class TirCarnetNumberValidationHelperTest : TestCase
	{
		public void TestNumberIsValid()
		{
			CombineAssertions(() =>
			{
				var actualResult = TirCarnetNumberValidationHelper.NumberIsValid(null);
				AssertEquals("null", false, actualResult);
				actualResult = TirCarnetNumberValidationHelper.NumberIsValid("");
				AssertEquals("empty", false, actualResult);
				actualResult = TirCarnetNumberValidationHelper.NumberIsValid("JX72581267");
				AssertEquals("with valid check code", true, actualResult);
				actualResult = TirCarnetNumberValidationHelper.NumberIsValid("jx72581267");
				AssertEquals("with lower case chars", true, actualResult);
				actualResult = TirCarnetNumberValidationHelper.NumberIsValid("JX 72581267");
				AssertEquals("with whitespace", false, actualResult);
				actualResult = TirCarnetNumberValidationHelper.NumberIsValid("XX72581267");
				AssertEquals("with invalid check code", false, actualResult);
				actualResult = TirCarnetNumberValidationHelper.NumberIsValid("XX25000000");
				AssertEquals("with unnecessary check code", false, actualResult);
				actualResult = TirCarnetNumberValidationHelper.NumberIsValid("25000000");
				AssertEquals("without check code", true, actualResult);
				actualResult = TirCarnetNumberValidationHelper.NumberIsValid("25000001");
				AssertEquals("with missed check code", false, actualResult);
				actualResult = TirCarnetNumberValidationHelper.NumberIsValid("12345678901234567890123456");
				AssertEquals("Digital conversion error ", false, actualResult);
			});
		}
	}
}

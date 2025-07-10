using NUnit.Framework;

namespace Enterprise.CustomerService.Business.Testing
{
	sealed class IncidentDetailsValidatorTest : TestCase
	{
		public void TestHasWesternCharactersAndIsTooShort()
		{
			var details = "";
			AssertEquals(true, IncidentDetailsValidator.HasWesternCharactersAndIsTooShort(details));

			details = "Test Details Data";
			AssertEquals(true, IncidentDetailsValidator.HasWesternCharactersAndIsTooShort(details));

			details = "Test Details Data Is OK";
			AssertEquals(false, IncidentDetailsValidator.HasWesternCharactersAndIsTooShort(details));

			details =
@"Line
Break
Should
Count
Too";
			AssertEquals(false, IncidentDetailsValidator.HasWesternCharactersAndIsTooShort(details));

			details =
@"LineBreakAfterSpace

  ";
			AssertEquals(true, IncidentDetailsValidator.HasWesternCharactersAndIsTooShort(details));
		}

		public void TestHasNonWesternCharactersAndIsTooShort()
		{
			var details = "";
			AssertEquals(true, IncidentDetailsValidator.HasNonWesternCharactersAndIsTooShort(details));

			details = "問題ある";
			AssertEquals(true, IncidentDetailsValidator.HasNonWesternCharactersAndIsTooShort(details));

			details = "問題がある";
			AssertEquals(false, IncidentDetailsValidator.HasNonWesternCharactersAndIsTooShort(details));

			details =
@"問題
がある";
			AssertEquals(false, IncidentDetailsValidator.HasNonWesternCharactersAndIsTooShort(details));

			details =
@"問題

  ";
			AssertEquals(true, IncidentDetailsValidator.HasNonWesternCharactersAndIsTooShort(details));
		}
	}
}

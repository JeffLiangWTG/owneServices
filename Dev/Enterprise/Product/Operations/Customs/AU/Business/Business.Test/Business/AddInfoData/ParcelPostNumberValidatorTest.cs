namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class ParcelPostNumberValidatorTest : NUnit.Framework.TestCase
	{
		public void TestValidate()
		{
			AssertEquals("Error", "", new ParcelPostNumberValidator().Validate("2NT12345"));
			AssertEquals("Error", "The parcel post card number '4V123' doesn't start with a '1' or '2'", new ParcelPostNumberValidator().Validate("4V123"));
			AssertEquals("Error", "The parcel post card number '1X123' must have a valid state/territory at its second character", new ParcelPostNumberValidator().Validate("1X123"));
			AssertEquals("Error", "The parcel post card number '1N123N' is invalid because it should only contain numbers after the state/territory code", new ParcelPostNumberValidator().Validate("1N123N"));
		}

		public void TestValidateCorrectlyForEachStateAndTerritory()
		{
			string[] validCodes = new string[] { "A", "NT", "T", "W", "S", "Q", "V", "N" };
			foreach (string validCode in validCodes)
			{
				AssertEquals("Error", "", new ParcelPostNumberValidator().Validate("2NT12345".Replace("NT", validCode)));
			}
		}

		public void TestValidationNumber()
		{
			AssertEquals("Error", "", new ParcelPostNumberValidator().Validate("1W136860"));
		}
	}
}

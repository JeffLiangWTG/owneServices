using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class RegistrationNumberValidationHelperTest : TestCaseWithFactory
	{
		public void TestRegistrationNumberLength() => AssertEquals(21, RegistrationNumberValidationHelper.RegistrationNumberLength);
		public void TestMRNLength() => AssertEquals(18, RegistrationNumberValidationHelper.MRNLength);

		public void TestValidateRegistrationNumberLengthAndFormat_Length()
		{
			const string registrationNumberLengthValidationMessage = "The Reference must have 18 characters (MRN) or 21 characters (ATLAS Reg. No.).";
			CombineAssertions(() =>
			{
				var messageError = RegistrationNumberValidationHelper.ValidateRegistrationNumberLengthAndMrnFormat("1".PadRight(17), Factory);
				AssertEquals("Length invalid", registrationNumberLengthValidationMessage, messageError);

				messageError = RegistrationNumberValidationHelper.ValidateRegistrationNumberLengthAndMrnFormat("1".PadRight(18), Factory);
				AssertNotContains("Length 18", registrationNumberLengthValidationMessage, messageError);

				messageError = RegistrationNumberValidationHelper.ValidateRegistrationNumberLengthAndMrnFormat("1".PadRight(21), Factory);
				AssertNotContains("Length 21", registrationNumberLengthValidationMessage, messageError);
			});
		}

		public void TestValidateRegistrationNumberLengthAndFormat_18Characters_MRNValidation()
		{
			const string mrnFormatValidationError = "MRN does not contain a valid country/region code";
			CombineAssertions(() =>
			{
				var messageError = RegistrationNumberValidationHelper.ValidateRegistrationNumberLengthAndMrnFormat("23DE586601055987B7", Factory);
				AssertNullOrEmpty("Length 18: valid MRN", messageError);

				messageError = RegistrationNumberValidationHelper.ValidateRegistrationNumberLengthAndMrnFormat("23XX586601055987B7", Factory);
				AssertContains("Length 18: invalid country code", mrnFormatValidationError, messageError);
			});
		}

		public void TestIsRegistrationNumberAWorkingNumberATA() => CombineAssertions(() =>
		{
			AssertEquals(true, RegistrationNumberValidationHelper.IsRegistrationNumberAWorkingNumber("ATA123456789012345678"));
			AssertEquals(false, RegistrationNumberValidationHelper.IsRegistrationNumberAWorkingNumber("ATC123456789012345678"));
		});

		public void TestIsRegistrationNumberAWorkingNumberMRN() => CombineAssertions(() =>
		{
			AssertEquals(true, RegistrationNumberValidationHelper.IsRegistrationNumberAWorkingNumber("24DE12345A78901234"));
			AssertEquals(false, RegistrationNumberValidationHelper.IsRegistrationNumberAWorkingNumber("24DE12345678901234"));
		});

		public void TestIsRegistrationNumberAWorkingNumberNull()
		{
			AssertEquals(false, RegistrationNumberValidationHelper.IsRegistrationNumberAWorkingNumber(null));
		}
	}
}

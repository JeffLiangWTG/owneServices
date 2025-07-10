using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	sealed class AdditionalInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_Code()
		{
			var additionalInfo = Factory.New<AdditionalInfo>();

			additionalInfo.Validation.ValidateCSI_Code();
			AssertNoMessageErrors("Precondition: nothing entered", additionalInfo.CSI_CodeInfo);

			additionalInfo.CSI_Description = "Some random text";
			additionalInfo.Validation.ValidateCSI_Code();
			AssertHasMessageError(additionalInfo.CSI_CodeInfo, "You have not entered a Code.");

			additionalInfo.CSI_Code = "12345";
			additionalInfo.Validation.ValidateCSI_Code();
			AssertHasMessageError(additionalInfo.CSI_CodeInfo, "The code you have selected is not in the list.");
			AssertNoMessageError(additionalInfo.CSI_CodeInfo, "You have not entered a Code.");
		}
	}
}

using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class AdditionalInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_Code()
		{
			var additionalInfo = Factory.New<AdditionalInfo>();
			additionalInfo.CSI_Description = "A random description without the code entered.";
			AssertNoMessageErrors(additionalInfo.CSI_CodeInfo);

			additionalInfo.CSI_Code = "10600";
			AssertHasMessageError(additionalInfo.CSI_CodeInfo, "You have entered both Code and Text fields when not required. Please enter either Code or Text.");
		}

		public void TestCheckCSI_Description()
		{
			var additionalInfo = Factory.New<AdditionalInfo>();
			additionalInfo.CSI_Code = "10600";
			AssertNoMessageErrors(additionalInfo.CSI_DescriptionInfo);

			additionalInfo.CSI_Description = "A random description even though there is already a code.";
			AssertHasMessageError(additionalInfo.CSI_DescriptionInfo, "You have entered both Code and Text fields when not required. Please enter either Code or Text.");
		}
	}
}

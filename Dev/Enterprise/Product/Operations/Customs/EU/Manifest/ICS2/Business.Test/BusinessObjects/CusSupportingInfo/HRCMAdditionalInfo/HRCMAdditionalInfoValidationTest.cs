using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class HRCMAdditionalInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_Code()
		{
			var additionalInfo = Factory.New<HRCMAdditionalInfo>();
			additionalInfo.CSI_Code = "XXX";
			Assert(additionalInfo.CSI_CodeInfo.HasMessageError("The code you have selected is not in the list."));
		}

		public void TestCheckCSI_SubType()
		{
			var additionalInfo = Factory.New<HRCMAdditionalInfo>();
			additionalInfo.CSI_SubType = "XXX";
			Assert(additionalInfo.CSI_SubTypeInfo.HasMessageError("The code you have selected is not in the list."));
		}
	}
}

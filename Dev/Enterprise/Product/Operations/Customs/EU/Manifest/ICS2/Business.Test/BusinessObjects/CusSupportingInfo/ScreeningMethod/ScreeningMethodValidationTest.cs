using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class ScreeningMethodValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_Code()
		{
			var screeningMethod = Factory.New<ScreeningMethod>();
			screeningMethod.CSI_Code = "XXX";
			Assert(screeningMethod.CSI_CodeInfo.HasMessageError("The code you have selected is not in the list."));
		}
	}
}

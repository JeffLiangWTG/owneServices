using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CH.Business.Testing;

internal class AdditionalCodeDataValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCY_Code()
	{
		var codeData = Factory.New<AdditionalCodeData>();
		CombineAssertions(() =>
		{
			codeData.CY_Code = "";
			AssertNoNotifications("When empty", codeData.CY_CodeInfo);
			codeData.CY_Code = "@";
			AssertNoNotifications("When invalid code", codeData.CY_CodeInfo);
		});
	}
}

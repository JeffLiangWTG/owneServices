using CargoWise.Types;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.BR.Business.Testing
{
	class CustomsOfficeValidationTest : CusCodeDataValidationTest
	{
		public void TestCY_Code()
		{
			var office = Factory.New<CustomsOffice>();
			office.CY_Code = ZString.Empty;
			AssertNoMessageErrors("No message error on CY_Code", office.CY_CodeInfo);
			office.CY_Code = "XXXX";
			AssertNoMessageErrors("No message error on CY_Code", office.CY_CodeInfo);
		}
	}
}

using CargoWise.Types;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.BR.Business.Testing
{
	class CustomsEnclosureValidationTest : CusCodeDataValidationTest
	{
		public void TestCY_Code()
		{
			var enclosure = Factory.New<CustomsEnclosure>();
			enclosure.CY_Code = ZString.Empty;
			AssertNoMessageErrors("No message error on CY_Code", enclosure.CY_CodeInfo);
			enclosure.CY_Code = "XXXX";
			AssertNoMessageErrors("No message error on CY_Code", enclosure.CY_CodeInfo);
		}
	}
}

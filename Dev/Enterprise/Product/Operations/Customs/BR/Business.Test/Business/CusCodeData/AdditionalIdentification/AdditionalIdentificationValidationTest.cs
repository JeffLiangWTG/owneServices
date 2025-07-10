using CargoWise.Types;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class AdditionalIdentificationValidationTest : CusCodeDataValidationTest
	{
		public void TestCY_Code()
		{
			var ddditionalIdentification = Factory.New<AdditionalIdentification>();
			ddditionalIdentification.CY_Code = ZString.Empty;
			AssertNoMessageErrors("No message error on CY_Code", ddditionalIdentification.CY_CodeInfo);
			ddditionalIdentification.CY_Code = "XXX";
			AssertNoMessageErrors("No message error on CY_Code", ddditionalIdentification.CY_CodeInfo);
		}
	}
}

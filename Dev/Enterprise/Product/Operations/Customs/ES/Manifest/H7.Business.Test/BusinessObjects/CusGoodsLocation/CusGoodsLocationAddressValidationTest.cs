using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	public class CusGoodsLocationAddressValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCusGoodsLocationAddressValidationTest()
		{
			var address = Factory.NewWithValidTestData<CusGoodsLocationAddress>();
			AssertType<CusGoodsLocationAddressValidation>(address.Validation);
		}

		public void TestC0065RuleNotApplied()
		{
			var location = Factory.NewWithValidTestData<CusGoodsLocation>();
			var address = location.Address;
			address.AuthorisationNumber = "";
			location.CGL_Qualifier = "Y";
			AssertNoMessageErrors(address.E2_GovRegNumInfo);
		}
	}
}

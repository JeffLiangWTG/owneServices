using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	sealed class CusGoodsLocationAddressValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCusGoodsLocationAddressValidationTest()
		{
			var address = Factory.NewWithValidTestData<CusGoodsLocationAddress>();
			AssertType<CusGoodsLocationAddressValidation>(address.Validation);
		}
	}
}

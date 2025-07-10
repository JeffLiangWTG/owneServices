using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(CusGoodsLocationAddress))]
	sealed class CusGoodsLocationAddressTest : EnterpriseBusinessObjectTestCase
	{
		public void TestValidation()
		{
			var locationAddress = Factory.New<CusGoodsLocationAddress>();
			AssertType<CusGoodsLocationAddressValidation>(locationAddress.Validation);
		}
	}
}

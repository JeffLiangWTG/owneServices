using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	[TestedType(typeof(CusGoodsLocationAddress))]
	sealed class CusGoodsLocationAddressTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCusGoodsLocationAddressTest()
		{
			var address = Factory.NewWithValidTestData<CusGoodsLocationAddress>();
			AssertType<CusGoodsLocationAddress>(address);
		}
	}
}

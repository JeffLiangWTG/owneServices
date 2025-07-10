using CargoWise.ComponentModel;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	[TestedType(typeof(CusGoodsLocationAddress))]
	sealed class CusGoodsLocationAddressTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCusGoodsLocationAddressTest()
		{
			var address = Factory.NewWithValidTestData<CusGoodsLocationAddress>();
			AssertType<CusGoodsLocationAddress>(address);
		}

		public void TestAuthorisationNumber()
		{
			var cusGoodsLocationAddress = Factory.NewWithValidTestData<CusGoodsLocationAddress>();
			var descriptor = cusGoodsLocationAddress.AuthorisationNumberInfo.PropertyDescriptor;
			var listAttribute = descriptor.Attributes[typeof(ListAttribute)] as ListAttribute;
			AssertEquals("Lookups.AuthorisationNumberList", listAttribute.ListDataSourceMember);
		}

		public void TestLookups()
		{
			var lookup = Factory.NewWithValidTestData<CusGoodsLocationAddress>().Lookups;
			AssertType<CusGoodsLocationAddressLookups>(lookup);
		}
	}
}

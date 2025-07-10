using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(CusGoodsLocationAddress))]
	sealed class CusGoodsLocationAddressTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLookups()
		{
			AssertType<CusGoodsLocationAddressLookups>(locationAddress.Lookups);
		}

		public void TestValidation()
		{
			AssertType<CusGoodsLocationAddressValidation>(locationAddress.Validation);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<TemporaryStorageHeader>();
			var location = header.GoodsLocation;
			locationAddress = (CusGoodsLocationAddress)location.Address;
		}
		CusGoodsLocationAddress locationAddress;
	}
}

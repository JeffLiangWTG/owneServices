using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	class JPAFRHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCarriers()
		{
			var header = Factory.New<JPAFRHeader>();
			SeaShippingProviderCollection collection = header.Lookups.Carriers;
			AssertNotNull(collection);
		}

		public void TestMessageStatusList()
		{
			var header = Factory.New<JPAFRHeader>();
			AssertEquals(Factory.GetCachedValue<MessageStatusList>(), header.Lookups.MessageStatusList);
		}

		public void TestVessels()
		{
			var header = Factory.New<JPAFRHeader>();
			RefVesselCollection collection = header.Lookups.Vessels;
			AssertNotNull(collection);
		}
	}
}

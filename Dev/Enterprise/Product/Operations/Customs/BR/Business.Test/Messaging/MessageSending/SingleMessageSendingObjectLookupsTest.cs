using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.Testing
{
	class SingleMessageSendingObjectLookupsTest : TestCaseWithFactory
	{
		public void TestLookups()
		{
			AssertType(typeof(GlbStaffCollection), lookups.BrokerList);
		}

		SingleMessageSendingObjectLookups lookups;

		protected override void SetUp()
		{
			goodsCatalog = Factory.NewWithValidTestData<CusGoodsCatalog>();
			sendingObjectParent = new GoodsCatalogMessageSendingObject(goodsCatalog);
			lookups = new SingleMessageSendingObjectLookups(sendingObjectParent);
		}

		CusGoodsCatalog goodsCatalog;
		GoodsCatalogMessageSendingObject sendingObjectParent;
	}
}


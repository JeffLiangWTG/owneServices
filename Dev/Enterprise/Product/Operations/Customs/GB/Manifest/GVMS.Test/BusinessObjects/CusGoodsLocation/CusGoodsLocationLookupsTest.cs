using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.GVMS.Testing
{
	class CusGoodsLocationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestInspectionTypeLookup()
		{
			GVMSMessageTestHelper.SetupInspectionLocationsRefCusCodeList(Factory);
			var location = Factory.New<CusGoodsLocation>();
			AssertEquals("1 - CUSTOMS", location.Lookups.TypeList.GetWithDescription("1"));
			AssertEquals("2 - DEFRA", location.Lookups.TypeList.GetWithDescription("2"));
		}

		public void TestInspectionLocationLookup()
		{
			GVMSMessageTestHelper.SetupInspectionLocationsRefCusCodeList(Factory);
			var location = Factory.New<CusGoodsLocation>();
			AssertEquals("L0029A - Sevington", location.Lookups.InspectionLocationList.GetWithDescription("L0029A"));
			AssertEquals("L0030A - Stop 24", location.Lookups.InspectionLocationList.GetWithDescription("L0030A"));
			AssertEquals("L0031A - Dover Western Docks", location.Lookups.InspectionLocationList.GetWithDescription("L0031A"));
		}
	}
}

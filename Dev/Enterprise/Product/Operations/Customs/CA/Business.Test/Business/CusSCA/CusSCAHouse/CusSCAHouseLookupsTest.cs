using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusSCAHouseLookupsTest : TestCaseWithFactory
	{
		public void TestInTransitCodeList()
		{
			var helper = new CusSCATestHelper();
			AssertEquals("InTransitCodeList", typeof(InTransitCodeList), helper.House.Lookups.InTransitCodeList.GetType());
		}

		public void TestState_List_For_Country()
		{
			var house = Factory.New<CusSCAHouse>();
			house.CA_RN_NKConsigneeCountryCode = "CA";
			house.CA_RN_NKConsignorCountryCode = "CA";
			house.CA_RN_NKDeliveryCountryCode = "CA";
			house.CA_RN_NKNotifyCountryCode = "CA";
			Assert(house.Lookups.ConsigneeState_List_For_Country.ContainsCode("BC"));
			Assert(house.Lookups.ConsignorState_List_For_Country.ContainsCode("BC"));
			Assert(house.Lookups.DeliveryState_List_For_Country.ContainsCode("BC"));
			Assert(house.Lookups.NotifyState_List_For_Country.ContainsCode("NS"));

			house.CA_RN_NKConsigneeCountryCode = "US";
			house.CA_RN_NKConsignorCountryCode = "US";
			house.CA_RN_NKDeliveryCountryCode = "US";
			house.CA_RN_NKNotifyCountryCode = "US";
			Assert(house.Lookups.ConsigneeState_List_For_Country.ContainsCode("TX"));
			Assert(house.Lookups.ConsignorState_List_For_Country.ContainsCode("TX"));
			Assert(house.Lookups.DeliveryState_List_For_Country.ContainsCode("TX"));
			Assert(house.Lookups.NotifyState_List_For_Country.ContainsCode("TX"));
		}
	}
}

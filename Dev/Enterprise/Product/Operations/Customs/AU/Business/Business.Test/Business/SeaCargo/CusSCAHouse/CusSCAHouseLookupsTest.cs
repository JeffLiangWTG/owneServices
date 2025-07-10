using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSCAHouseLookupsTest : BusinessObjectLookupsTestCase
	{
		[TestDate(2005, 10, 12)]
		public void TestMethodsOfPayment()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAHouse house = oceanBill.HouseBills.AddNew();
			var lookups = house.Lookups;
			AssertNotNull(lookups.MethodsOfPayment);
			Assert("MethodsOfPayment should be cached in same factory", ReferenceEquals(Factory.GetCachedValue<CMRMethodsOfPayment>(), lookups.MethodsOfPayment));
		}

		public void TestStatusList()
		{
			var house = Factory.New<CusSCAHouse>();
			AssertNotNull(house.Lookups.StatusList);
			Assert("StatusList should be cached", ReferenceEquals(Factory.GetCachedValue<CMRAllStatuses>(), house.Lookups.StatusList));
		}

		public void TestShipmentStatusList()
		{
			var house = Factory.New<CusSCAHouse>();
			AssertNotNull(house.Lookups.ShipmentStatusList);
			Assert("ShipmentStatusList should be cached", ReferenceEquals(Factory.GetCachedValue<CMRShipmentStatuses>(), house.Lookups.ShipmentStatusList));
		}

		public void TestNotifyPartyCountryCodes()
		{
			var house = Factory.New<CusSCAHouse>();
			AssertNotNull(house.Lookups.NotifyPartyCountryCodes);
			AssertEquals(typeof(RefCountryCollection), house.Lookups.NotifyPartyCountryCodes.GetType());
		}

		public void TestConsigneeLists()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAHouse houseBill = oceanBill.HouseBills.AddNew();
			var lookups = houseBill.Lookups;
			AssertEquals("Consignee List", typeof(ConsigneeCollection), lookups.Consignee_List.GetType());
			houseBill.CA_IsMasterHouse = true;
			AssertEquals("Consignee Forwarder List", typeof(ForwarderCollection), lookups.Consignee_List.GetType());
		}

		public void TestConsignorLists()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAHouse houseBill = oceanBill.HouseBills.AddNew();
			var lookups = houseBill.Lookups;
			AssertEquals("Consignor List", typeof(ConsignorCollection), lookups.Consignor_List.GetType());
			houseBill.CA_IsMasterHouse = true;
			AssertEquals("Consignor Forwarder List", typeof(ForwarderCollection), lookups.Consignor_List.GetType());
		}
	}
}

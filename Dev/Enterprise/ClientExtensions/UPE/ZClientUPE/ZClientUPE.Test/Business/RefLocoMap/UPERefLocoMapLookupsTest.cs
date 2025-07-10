using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Client.UPE.Business.RefLocoMap.Testing
{
	internal class UPERefLocoMapLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestRY_SystemUsage_List_AU()
		{
			AssertListDetails(Core.Constants.CountryGuids.Australia, typeof(UPEAirSeaMailSystemUsageList), "AIR, MAI, SEA, UPS");
		}

		public void TestRY_SystemUsage_List_UK()
		{
			AssertListDetails(Core.Constants.CountryGuids.Australia, typeof(UPEAirSeaMailSystemUsageList), "AIR, MAI, SEA, UPS");
		}

		public void TestRY_SystemUsage_List_US()
		{
			AssertListDetails(Core.Constants.CountryGuids.UnitedStates, typeof(UPEUSLocoMapSystemUsageList), "AIR, ALL, SCD, SCK, SEA, UPS");
		}

		public void TestRY_SystemUsage_List_ER()
		{
			AssertListDetails(Core.Constants.CountryGuids.Eritrea, typeof(UPEOtherLocoMapSystemUsageList), "UPS");
		}

		public void TestRY_SystemUsage_List_IS()
		{
			AssertListDetails(Core.Constants.CountryGuids.Iceland, typeof(UPEISLocoMapSystemUsageList), "COC, UPS");
		}

		public void TestRY_SystemUsage_List_SG()
		{
			AssertListDetails(Core.Constants.CountryGuids.Singapore, typeof(UPESGLocoMapSystemUsageList), "CUS, UPS");
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			locoMap = Factory.New<UPERefLocoMap>();
			base.SetUp();
		}
		UPERefLocoMap locoMap;

		void AssertListDetails(ZGuid country, Type type, ZString expectedList)
		{
			locoMap.RY_RN = country;
			var list = locoMap.Lookups.RY_SystemUsage_List;
			AssertType(type, list);
			AssertEquals(expectedList, list.CodesAsString);
		}
	}
}

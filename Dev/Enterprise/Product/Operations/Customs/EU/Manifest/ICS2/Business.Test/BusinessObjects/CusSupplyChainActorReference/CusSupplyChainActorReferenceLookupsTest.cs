using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	class CusSupplyChainActorReferenceLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCusSupplyChainActorReferenceCodes()
		{
			var today = ZDateTime.Now;
			var yesterday = today.AddDays(-1);
			var tomorrow = today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("IC2SA", "IC2SA");

			helper.CreateCusCodeList("EUN", "IC2SA", "CS", "Consolidator.", yesterday, tomorrow);
			helper.CreateCusCodeList("EUN", "IC2SA", "FW", "Freight Forwarder", yesterday, tomorrow);

			Factory.Save();

			var supplyChainActorReference = Factory.NewWithValidTestData<CusSupplyChainActorReference>();
			var list = supplyChainActorReference.Lookups.CodeList;
			AssertEquals(2, list.Count);
			Assert(list.ContainsCode("CS"));
			Assert(list.ContainsCode("FW"));
		}

		public void TestCusSupplyChainActorReferenceCodesAreSorted()
		{
			var today = ZDateTime.Now;
			var yesterday = today.AddDays(-1);
			var tomorrow = today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("IC2SA", "IC2SA");

			helper.CreateCusCodeList("EUN", "IC2SA", "WH", "Warehouse Keeper", yesterday, tomorrow);
			helper.CreateCusCodeList("EUN", "IC2SA", "FW", "Freight Forwarder", yesterday, tomorrow);
			helper.CreateCusCodeList("EUN", "IC2SA", "MF", "Manufacturer", yesterday, tomorrow);
			helper.CreateCusCodeList("EUN", "IC2SA", "CS", "Freight Forwarder", yesterday, tomorrow);

			Factory.Save();

			var supplyChainActorReference = Factory.NewWithValidTestData<CusSupplyChainActorReference>();
			var list = supplyChainActorReference.Lookups.CodeList;

			AssertContainsExactElementsInExactOrder("Should be sorted by code in alphabetical order", new[] { "CS", "FW", "MF", "WH" }, list.GetAllCodes());
		}

		[TestDate()]
		public void TestCusSupplyChainActorReferenceCodes_CodeListCache()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("IC2SA", "IC2SA");
			helper.CreateCusCodeList("EUN", "IC2SA", "CS", "Consolidator.", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(2));

			Factory.Save();

			var supplyChainActorReference = Factory.NewWithValidTestData<CusSupplyChainActorReference>();
			var list = supplyChainActorReference.Lookups.CodeList;

			AssertEquals("predition : list contain only 1 element", 1, list.Count);
			AssertSame("Should get the cached list.", list, supplyChainActorReference.Lookups.CodeList);

			helper.CreateCusCodeList("EUN", "IC2SA", "FW", "Freight Forwarder", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(2));
			Factory.Save();

			var listOnSameDay = supplyChainActorReference.Lookups.CodeList;
			AssertEquals("on the same day, load data from the code list cache", 1, listOnSameDay.Count);

			TestDateAttribute.Date = ZDateTime.Now.AddDays(1).ToDateTime();
			var listOnDifferentDay = supplyChainActorReference.Lookups.CodeList;
			AssertEquals("On a different day, the code list cache is refreshed", 2, listOnDifferentDay.Count);
		}
	}
}

using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	class OtherRelevantLawLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestOtherRelevantLawsAndOrdinancesCodeList()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill = header.Bills.AddNew();
			var law = bill.OtherRelevantLaws.AddNew();
			AssertEquals(Factory.GetCachedValue<OtherRelevantLawsAndOrdinancesCodeList>(), law.Lookups.OtherRelevantLawsAndOrdinancesCodeList);
		}
	}
}

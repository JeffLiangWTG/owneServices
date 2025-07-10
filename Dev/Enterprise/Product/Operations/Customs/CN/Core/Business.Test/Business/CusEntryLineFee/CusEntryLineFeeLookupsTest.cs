using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CusEntryLineFeeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestEntryLineFee()
		{
			var parent = Factory.New<CusEntryLineFee>();
			AssertEquals(parent.Lookups.EntryLineFee, parent);
		}
	}
}

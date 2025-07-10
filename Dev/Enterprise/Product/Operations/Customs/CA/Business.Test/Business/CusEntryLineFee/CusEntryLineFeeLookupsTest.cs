using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusEntryLineFeeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestEntryLineFee()
		{
			CusEntryLineFee parent = Factory.New<CusEntryLineFee>();
			AssertEquals(parent.Lookups.EntryLineFee, parent);
		}
	}
}

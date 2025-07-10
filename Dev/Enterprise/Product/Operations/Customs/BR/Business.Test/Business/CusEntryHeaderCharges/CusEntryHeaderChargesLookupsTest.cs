using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class CusEntryHeaderChargesLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestEntryHeaderCharges()
		{
			CusEntryHeaderCharges parent = Factory.New<CusEntryHeaderCharges>();
			AssertEquals(parent.Lookups.EntryHeaderCharges, parent);
		}
	}
}

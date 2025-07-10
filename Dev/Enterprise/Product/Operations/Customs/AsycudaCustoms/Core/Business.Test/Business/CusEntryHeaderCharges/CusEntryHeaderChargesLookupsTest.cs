using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	class CusEntryHeaderChargesLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestEntryHeaderCharges()
		{
			var parent = Factory.New<CusEntryHeaderCharges>();
			AssertEquals(parent.Lookups.EntryHeaderCharges, parent);
		}
	}
}

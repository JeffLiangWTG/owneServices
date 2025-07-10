namespace Enterprise.Customs.BR.Business.Testing
{
	public class BillLookupsTest : Customs.Business.Testing.CusDecHouseBillLookupsTest
	{
		public void TestHouseBill()
		{
			Bill parent = Factory.New<Bill>();
			AssertEquals(parent.Lookups.HouseBill, parent);
		}
	}
}

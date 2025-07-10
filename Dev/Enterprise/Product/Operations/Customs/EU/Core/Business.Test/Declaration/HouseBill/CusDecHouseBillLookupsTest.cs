namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public class CusDecHouseBillLookupsTest : Customs.Business.Testing.CusDecHouseBillLookupsTest
	{
		public void TestHouseBill()
		{
			Bill parent = Factory.New<Bill>();
			AssertEquals(parent.Lookups.HouseBill, parent);
		}
	}
}

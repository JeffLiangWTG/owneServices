namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	class BillLookupsTest : Customs.Business.Testing.CusDecHouseBillLookupsTest
	{
		public void TestHouseBill()
		{
			var parent = Factory.New<Bill>();
			AssertEquals(parent.Lookups.HouseBill, parent);
		}
	}
}

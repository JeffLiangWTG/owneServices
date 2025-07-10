namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class CusDecHouseBillLookupsTest : Customs.Business.Testing.CusDecHouseBillLookupsTest
	{
		public void TestParent()
		{
			var bill = Factory.New<Bill>();
			var lookup = new CusDecHouseBillLookupsForTest(bill);
			AssertSame(bill, lookup.ParentExposed);
		}

		public void TestHouseBillSplitDeclarationIndicatorCodeList()
		{
			var bill = Factory.New<Bill>();
			var lookups = bill.Lookups;

			AssertEquals("Expected Codes", "N, Y", lookups.HouseBillSplitDeclarationIndicatorCodeList.CodesAsString);
		}

		public void TestHouseBillSplitDeclarationReasonCodeList()
		{
			var bill = Factory.New<Bill>();
			var lookups = bill.Lookups;

			AssertEquals("Expected Codes", "A, B, C, D, E, Z", lookups.HouseBillSplitDeclarationReasonCodeList.CodesAsString);
		}
	}

	sealed class CusDecHouseBillLookupsForTest : CusDecHouseBillLookups
	{
		public CusDecHouseBillLookupsForTest(Bill houseBill) : base(houseBill)
		{
		}

		public Bill ParentExposed => Parent;
	}
}

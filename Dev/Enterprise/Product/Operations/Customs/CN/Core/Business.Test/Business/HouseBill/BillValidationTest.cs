namespace Enterprise.Customs.CN.Business.Testing
{
	class BillValidationTest : Customs.Business.Testing.CusDecHouseBillValidationTest
	{
		public void TestHouseBill()
		{
			var parent = Factory.New<Bill>();
			AssertEquals(parent.Validation.Bill, parent);
		}
	}
}

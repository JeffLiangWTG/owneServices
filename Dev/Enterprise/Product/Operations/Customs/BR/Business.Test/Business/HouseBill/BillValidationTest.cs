namespace Enterprise.Customs.BR.Business.Testing
{
	public class BillValidationTest : Customs.Business.Testing.CusDecHouseBillValidationTest
	{
		public void TestHouseBill()
		{
			Bill parent = Factory.New<Bill>();
			AssertEquals(parent.Validation.Bill, parent);
		}
	}
}

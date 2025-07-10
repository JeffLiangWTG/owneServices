namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public class CusDecHouseBillValidationTest : Customs.Business.Testing.CusDecHouseBillValidationTest
	{
		public void TestHouseBill()
		{
			Bill parent = Factory.New<Bill>();
			AssertEquals(parent.Validation.Bill, parent);
		}
	}
}

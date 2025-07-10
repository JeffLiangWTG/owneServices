namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public class InvoiceChargeValidationTest : Customs.Business.Testing.InvoiceChargeValidationTest
	{
		public void TestInvoiceCharge()
		{
			InvoiceCharge parent = Factory.New<InvoiceCharge>();
			AssertEquals(parent.Validation.InvoiceCharge, parent);
		}
	}
}

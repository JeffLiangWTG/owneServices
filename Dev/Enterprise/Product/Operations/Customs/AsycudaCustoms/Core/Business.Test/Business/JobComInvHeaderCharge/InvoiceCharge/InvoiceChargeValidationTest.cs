namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	public class InvoiceChargeValidationTest : Customs.Business.Testing.InvoiceChargeValidationTest
	{
		public void TestInvoiceCharge()
		{
			var parent = Factory.New<InvoiceCharge>();
			AssertEquals(parent.Validation.InvoiceCharge, parent);
		}
	}
}

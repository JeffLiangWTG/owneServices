namespace Enterprise.Customs.CN.Business.Testing
{
	class InvoiceChargeValidationTest : Customs.Business.Testing.InvoiceChargeValidationTest
	{
		public void TestInvoiceCharge()
		{
			var parent = Factory.New<InvoiceCharge>();
			AssertEquals(parent.Validation.InvoiceCharge, parent);
		}
	}
}

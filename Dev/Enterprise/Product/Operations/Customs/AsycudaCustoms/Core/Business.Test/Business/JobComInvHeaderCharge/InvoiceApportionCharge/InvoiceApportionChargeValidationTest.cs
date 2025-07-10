using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	class InvoiceApportionChargeValidationTest : TestCaseWithFactory
	{
		public void TestInvoiceCharge()
		{
			var parent = Factory.New<InvoiceApportionCharge>();
			AssertEquals(parent.Validation.InvoiceApportionCharge, parent);
		}
	}
}

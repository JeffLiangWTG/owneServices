using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public class InvoiceApportionChargeValidationTest : TestCaseWithFactory
	{
		public void TestInvoiceCharge()
		{
			InvoiceApportionCharge parent = Factory.New<InvoiceApportionCharge>();
			AssertEquals(parent.Validation.InvoiceApportionCharge, parent);
		}
	}
}

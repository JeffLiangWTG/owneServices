using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	public class InvoiceChargeValidationTest : TestCaseWithFactory
	{
		public void TestParent()
		{
			var parent = Factory.New<InvoiceCharge>();
			AssertEquals(parent.Validation.Parent, parent);
		}
	}
}

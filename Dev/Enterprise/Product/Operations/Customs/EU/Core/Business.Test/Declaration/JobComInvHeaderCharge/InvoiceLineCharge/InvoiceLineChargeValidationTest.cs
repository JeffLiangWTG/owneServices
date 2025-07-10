using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public class InvoiceLineChargeValidationTest : TestCaseWithFactory
	{
		public void TestParent()
		{
			var parent = Factory.New<InvoiceLineCharge>();
			AssertEquals(parent.Validation.Parent, parent);
		}
	}
}

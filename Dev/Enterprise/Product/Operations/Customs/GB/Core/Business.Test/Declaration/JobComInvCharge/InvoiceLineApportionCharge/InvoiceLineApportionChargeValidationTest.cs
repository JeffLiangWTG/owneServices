using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	public class InvoiceLineApportionChargeValidationTest : TestCaseWithFactory
	{
		public void TestParent()
		{
			var parent = Factory.New<InvoiceLineApportionCharge>();
			AssertEquals(parent.Validation.Parent, parent);
		}
	}
}

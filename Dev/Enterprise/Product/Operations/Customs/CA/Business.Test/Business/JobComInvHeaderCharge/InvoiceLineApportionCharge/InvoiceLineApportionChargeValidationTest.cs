using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class InvoiceLineApportionChargeValidationTest : TestCaseWithFactory
	{
		public void TestMessageValidation()
		{
			InvoiceLineApportionCharge parent = Factory.New<InvoiceLineApportionCharge>();
			AssertEquals(typeof(ExternalMessageValidation), parent.Validation.MessageValidation.GetType());
		}

		public void TestIsCIFComponentUsed()
		{
			Assert(new InvoiceLineApportionChargeValidationHelper(Factory.New<InvoiceLineApportionCharge>()).IsCIFComponentUsedExposed);
		}
	}
}

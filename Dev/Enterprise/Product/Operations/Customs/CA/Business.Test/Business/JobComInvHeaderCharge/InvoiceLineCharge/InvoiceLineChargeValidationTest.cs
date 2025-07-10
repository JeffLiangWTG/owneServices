using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class InvoiceLineChargeValidationTest : TestCaseWithFactory
	{
		public void TestMessageValidation()
		{
			InvoiceLineCharge parent = Factory.New<InvoiceLineCharge>();
			AssertEquals(typeof(ExternalMessageValidation), parent.Validation.MessageValidation.GetType());
		}

		public void TestIsCIFComponentUsed()
		{
			Assert(new InvoiceLineChargeValidationHelper(Factory.New<InvoiceLineCharge>()).IsCIFComponentUsedExposed);
		}
	}
}

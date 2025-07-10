using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class JobComInvoiceGroupHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestInvoiceGroupHeader()
		{
			JobComInvoiceGroupHeader groupHeader = Factory.New<JobComInvoiceGroupHeader>();
			AssertEquals(groupHeader.Validation.InvoiceGroupHeader, groupHeader);
		}

		public void TestMessageValidation()
		{
			JobComInvoiceGroupHeader parent = Factory.New<JobComInvoiceGroupHeader>();
			AssertEquals(typeof(ExternalMessageValidation), parent.Validation.MessageValidation.GetType());
		}
	}
}

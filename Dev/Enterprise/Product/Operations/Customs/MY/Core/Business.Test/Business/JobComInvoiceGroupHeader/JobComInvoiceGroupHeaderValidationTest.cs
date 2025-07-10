using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.MY.Business.Testing
{
	class JobComInvoiceGroupHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestInvoiceGroupHeader()
		{
			JobComInvoiceGroupHeader groupHeader = Factory.New<JobComInvoiceGroupHeader>();
			AssertEquals(groupHeader.Validation.InvoiceGroupHeader, groupHeader);
		}
	}
}

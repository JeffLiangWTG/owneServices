using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CN.Business.Testing
{
	class JobComInvoiceGroupHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestInvoiceGroupHeader()
		{
			var groupHeader = Factory.New<JobComInvoiceGroupHeader>();
			AssertEquals(groupHeader.Validation.InvoiceGroupHeader, groupHeader);
		}
	}
}

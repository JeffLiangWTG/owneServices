using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public class JobComInvoiceGroupHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestInvoiceGroupHeader()
		{
			JobComInvoiceGroupHeader groupHeader = Factory.New<JobComInvoiceGroupHeader>();
			AssertEquals(groupHeader.Validation.InvoiceGroupHeader, groupHeader);
		}
	}
}

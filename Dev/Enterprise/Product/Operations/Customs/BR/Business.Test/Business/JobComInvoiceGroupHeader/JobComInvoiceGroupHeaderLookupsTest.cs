namespace Enterprise.Customs.BR.Business.Testing
{
	public class JobComInvoiceGroupHeaderLookupsTest : Customs.Business.Testing.JobComInvoiceHeaderLookupsTest
	{
		public void TestInvoice()
		{
			JobComInvoiceGroupHeader groupHeader = Factory.New<JobComInvoiceGroupHeader>();
			AssertEquals(groupHeader.Lookups.Invoice, groupHeader);
		}
	}
}

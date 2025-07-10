namespace Enterprise.Customs.EU.Business.Declaration.Testing
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

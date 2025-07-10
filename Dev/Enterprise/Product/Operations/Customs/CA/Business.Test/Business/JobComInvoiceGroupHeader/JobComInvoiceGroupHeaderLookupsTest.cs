namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class JobComInvoiceGroupHeaderLookupsTest : Customs.Business.Testing.JobComInvoiceHeaderLookupsTest
	{
		public void TestInvoice()
		{
			JobComInvoiceGroupHeader groupHeader = Factory.New<JobComInvoiceGroupHeader>();
			AssertEquals(groupHeader.Lookups.Invoice, groupHeader);
		}
	}
}

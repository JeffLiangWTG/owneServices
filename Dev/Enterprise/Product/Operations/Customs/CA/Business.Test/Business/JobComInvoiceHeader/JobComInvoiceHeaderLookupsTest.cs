namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class JobComInvoiceHeaderLookupsTest : Customs.Business.Testing.JobComInvoiceHeaderLookupsTest
	{
		public void TestInvoice()
		{
			JobComInvoiceHeader parent = Factory.New<JobComInvoiceHeader>();
			AssertEquals(parent.Lookups.Invoice, parent);
		}
	}
}

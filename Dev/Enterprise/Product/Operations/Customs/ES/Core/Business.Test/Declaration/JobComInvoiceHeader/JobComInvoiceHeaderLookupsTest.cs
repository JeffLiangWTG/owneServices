namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	public class JobComInvoiceHeaderLookupsTest : Customs.Business.Testing.JobComInvoiceHeaderLookupsTest
	{
		public void TestInvoice()
		{
			JobComInvoiceHeader parent = Factory.New<JobComInvoiceHeader>();
			AssertEquals(parent.Lookups.Invoice, parent);
		}
	}
}

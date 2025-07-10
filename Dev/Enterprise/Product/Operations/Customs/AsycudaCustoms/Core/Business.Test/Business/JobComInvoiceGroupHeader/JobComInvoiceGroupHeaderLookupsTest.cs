namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	public class JobComInvoiceGroupHeaderLookupsTest : Customs.Business.Testing.JobComInvoiceHeaderLookupsTest
	{
		public void TestInvoice()
		{
			var groupHeader = Factory.New<JobComInvoiceGroupHeader>();
			AssertEquals(groupHeader.Lookups.Invoice, groupHeader);
		}
	}
}

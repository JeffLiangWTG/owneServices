using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class JobComInvoiceHeaderFetchStrategyTest : EU.Business.Declaration.Testing.JobComInvoiceHeaderFetchStrategyTest
{
	public void TestFetchForLoad()
	{
		var invoice = Factory.New<JobComInvoiceHeader>();
		var strategy = new JobComInvoiceHeaderFetchStrategy(invoice);

		strategy.FetchForLoad();
		AssertEquals("Fetch hints on JobDocAddressSchema", 1, Factory.ActiveFetchHintsForTable(JobDocAddressSchema.Constants.TableName));
	}
}

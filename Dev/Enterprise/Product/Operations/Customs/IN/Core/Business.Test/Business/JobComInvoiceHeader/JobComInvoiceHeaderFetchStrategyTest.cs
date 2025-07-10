using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IN.Business.Testing;

sealed class JobComInvoiceHeaderFetchStrategyTest : TestCaseWithFactory
{
	public void TestFetchForValidate()
	{
		var invoice = Factory.New<JobComInvoiceHeader>();
		var strategy = new JobComInvoiceHeaderFetchStrategy(invoice);

		strategy.FetchForValidate();
		AssertEquals("Fetch hints on JobDocAddressSchema", 1, Factory.ActiveFetchHintsForTable(JobDocAddressSchema.Constants.TableName));
	}
}

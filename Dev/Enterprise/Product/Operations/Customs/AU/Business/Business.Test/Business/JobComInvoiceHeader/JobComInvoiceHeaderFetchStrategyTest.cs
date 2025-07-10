using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing;

[TestedType(typeof(JobComInvoiceHeaderFetchStrategy))]
class JobComInvoiceHeaderFetchStrategyTest : BusinessObjectFetchStrategyTestCase
{
	public void TestFetchForLoad()
	{
		var invoiceHeader = Factory.New<JobComInvoiceHeader>();
		var strategy = new JobComInvoiceHeaderFetchStrategy(invoiceHeader);
		strategy.FetchForLoad();
		AssertEquals("hint for JobComInvHeaderCharge", "JobComInvHeaderCharge", string.Join(",", Factory.GetAllFetchHintedTableNames()));
	}

	#region Implementation

	protected override IBusinessObjectCollection CreateCollectionToTest(BusinessObjectFactory factory) => new CommercialInvoiceCollection(factory);

	#endregion
}


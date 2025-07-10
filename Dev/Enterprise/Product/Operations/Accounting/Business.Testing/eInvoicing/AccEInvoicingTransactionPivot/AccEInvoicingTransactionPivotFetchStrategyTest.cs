using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.EInvoicing;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class AccEInvoicingTransactionPivotFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForLoad()
		{
			var pivot = Factory.NewWithValidTestData<AccEInvoicingTransactionPivot>();
			pivot.AIP_AIB = new CargoWise.Types.ZGuid(Guid.NewGuid());

			var fetchStrategy = new AccEInvoicingTransactionPivotFetchStrategy(pivot);

			fetchStrategy.FetchForLoad();
			AssertEquals($"Factory should have Fetch Hint for Batch table", 1, Factory.ActiveFetchHintsForTable(AccEInvoicingBatch.Schema.TableName));
		}
	}
}

using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	class JobComInvoiceLineFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForLoad()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var strategy = invoiceLine.FetchStrategy;

			strategy.FetchForLoad();
			AssertEquals("Should have fetch hints on CusHouseContPackInvoiceLinePivot.", 1, Factory.ActiveFetchHintsForTable(CusHouseContPackInvoiceLinePivotSchema.Constants.TableName));
		}
	}
}

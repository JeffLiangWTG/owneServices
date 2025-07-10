using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.FetchStrategies;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public class JobComInvoiceLineFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForLoadChildEditableObjects()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var strategy = new JobComInvoiceLineFetchStrategy(invoiceLine);

			strategy.FetchForLoadChildEditableObjects();
			AssertEquals("Should have fetch hints on CusAuthorizationUsageSchema.", 1, Factory.ActiveFetchHintsForTable(CusAuthorizationUsageSchema.Constants.TableName));
		}
	}
}

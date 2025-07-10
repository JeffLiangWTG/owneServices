using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Business.Testing
{
	public class JobComInvoiceLineFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForLoadChildEditableObjects()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var strategy = new JobComInvoiceLineFetchStrategy(invoiceLine);

			strategy.FetchForLoadChildEditableObjects();
			Assert("Should have fetch hints on CusReference (for the fiscal references).", Factory.ActiveFetchHintsForTable(CusReferenceSchema.Constants.TableName) > 0);
		}
	}
}

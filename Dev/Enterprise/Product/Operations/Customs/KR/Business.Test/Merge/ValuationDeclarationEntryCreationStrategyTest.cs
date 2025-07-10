using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class ValuationDeclarationEntryCreationStrategyTest : TestCaseWithFactory
	{
		public void TestGetKeyForHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			var strategy = new ValuationDeclarationEntryCreationStrategy(declaration);
			var keys = strategy.GetKeyForHeader(invoiceLine).Keys;

			AssertEquals(1, keys.Count);
		}
	}
}

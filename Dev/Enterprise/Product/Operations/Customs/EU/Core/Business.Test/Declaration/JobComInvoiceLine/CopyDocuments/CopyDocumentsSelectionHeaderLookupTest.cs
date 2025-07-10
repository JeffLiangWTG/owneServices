using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	sealed class CopyDocumentsSelectionHeaderLookupTest : TestCaseWithFactory
	{
		public void TestInvoices()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "INV1";
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "INV2";

			var invoiceLine = invoice1.InvoiceLines.AddNew();
			var header = new CopyDocumentsSelectionHeader(invoiceLine);

			AssertEquals("ALL, INV1, INV2", header.Lookups.Invoices.CodesAsString);
		}
	}
}

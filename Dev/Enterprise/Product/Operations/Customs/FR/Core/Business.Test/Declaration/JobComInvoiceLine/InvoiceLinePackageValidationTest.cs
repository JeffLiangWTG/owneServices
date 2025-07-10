using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	class InvoiceLinePackageValidationTest : TestCaseWithFactory
	{
		public void TestCheckIsLinked()
		{
			string message = "Packages line is not selected.";
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var package1 = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();

			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			var packing1 = invoiceLine1.PackagesForInvoiceLinesForBindingOnly[0];
			packing1.IsLinked = false;
			AssertHasWarning(packing1.IsLinkedInfo, message);

			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			var packing2 = invoiceLine2.PackagesForInvoiceLinesForBindingOnly[0];
			packing2.IsLinked = true;
			AssertNoWarning(packing2.IsLinkedInfo, message);
		}
	}
}

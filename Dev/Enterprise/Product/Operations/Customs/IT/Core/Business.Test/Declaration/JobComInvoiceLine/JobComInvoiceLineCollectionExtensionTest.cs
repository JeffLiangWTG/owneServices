using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class JobComInvoiceLineCollectionExtensionTest : TestCaseWithFactory
{
	public void TestGetDistinctInvoiceHeaders()
	{
		var declaration = Factory.New<JobDeclaration>();

		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLineWithHeader1 = invoiceHeader.InvoiceLines.AddNew();
		var invoiceLineWithHeader2 = invoiceHeader.InvoiceLines.AddNew();

		var invoiceLineWithNoHeader = Factory.New<JobComInvoiceLine>();

		var invoiceLineList = new List<JobComInvoiceLine>();
		invoiceLineList.Add(invoiceLineWithHeader1);
		invoiceLineList.Add(invoiceLineWithHeader2);
		invoiceLineList.Add(invoiceLineWithNoHeader);

		var invoiceHeaders = JobComInvoiceLineCollectionExtension.GetDistinctInvoiceHeaders(invoiceLineList).ToArray();

		AssertEquals("Only one Distinct invoiceHeader is expected", 1, invoiceHeaders.Length);
		AssertNotNull("No null InvoiceHeader in the list expected", invoiceHeaders.FirstOrDefault());
	}
}

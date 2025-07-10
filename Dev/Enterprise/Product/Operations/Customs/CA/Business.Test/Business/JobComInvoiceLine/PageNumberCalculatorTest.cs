using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.Testing
{
	class PageNumberCalculatorTest : TestCaseWithFactory
	{
		public void TestCalculatePageNumber_WhenCurrentPageNumberIsZero()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader1 = declaration.Invoices.AddNew();
			var invoice1Line1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			var invoice1Line2 = invoiceHeader1.JobComInvoiceLines.AddNew();

			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoice2Line1 = invoiceHeader2.JobComInvoiceLines.AddNew();
			var invoice2Line2 = invoiceHeader2.JobComInvoiceLines.AddNew();

			foreach (JobComInvoiceLine invoiceLine in declaration.InvoiceLines)
			{
				using (invoiceLine.SuspendRecalculatePageNumbers())
				{
					invoiceLine.CA_PageNumber = 0;
				}
			}

			PageNumberCalculator.RecalculatePageNumber(declaration, 0);

			AssertContainsExactElementsInExactOrder(new ZInt[] { 1, 2, 3, 4 }, new[] { invoice1Line1.CA_PageNumber, invoice1Line2.CA_PageNumber, invoice2Line1.CA_PageNumber, invoice2Line2.CA_PageNumber });
		}

		public void TestCalculatePageNumber_WithLuxuryTaxInvoiceLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.Invoices.AddNew();

			var line1 = declaration.FilteredInvoiceLines.AddNew();
			line1.CA_ApplyLuxuryTax = true;
			var luxLine1 = line1.LuxuryTaxInvoiceLine;
			AssertEquals(line1.CA_PageNumber, luxLine1.CA_PageNumber);

			var line2 = declaration.FilteredInvoiceLines.AddNew();
			line2.CA_PageNumber = 1;
			var line3 = declaration.FilteredInvoiceLines.AddNew();
			line3.CA_PageNumber = 2;
			var line4 = declaration.FilteredInvoiceLines.AddNew();
			line4.CA_PageNumber = 3;
			line1.CA_PageNumber = 3;
			line3.CA_ApplyLuxuryTax = true;
			var luxLine3 = line3.LuxuryTaxInvoiceLine;

			AssertEquals(3, line1.CA_PageNumber);
			AssertEquals(line1.CA_PageNumber, luxLine1.CA_PageNumber);
			AssertEquals(1, line2.CA_PageNumber);
			AssertEquals(2, line3.CA_PageNumber);
			AssertEquals(line3.CA_PageNumber, luxLine3.CA_PageNumber);
			AssertEquals(3, line4.CA_PageNumber);
		}
	}
}

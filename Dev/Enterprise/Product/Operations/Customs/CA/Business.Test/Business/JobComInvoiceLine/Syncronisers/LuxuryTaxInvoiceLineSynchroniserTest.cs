using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class LuxuryTaxInvoiceLineSynchroniserTest : TestCaseWithFactory
	{
		public void TestSynchroniser()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.Invoices.AddNew();

			var line1 = declaration.FilteredInvoiceLines.AddNew();
			line1.JI_InvoiceQuantity = 111.11;
			line1.JI_InvoiceUQ = "KGM";
			line1.CA_ApplyLuxuryTax = true;
			var luxLine1 = line1.LuxuryTaxInvoiceLine;
			AssertEquals(line1.CA_PageNumber, luxLine1.CA_PageNumber);
			AssertEquals(line1.JI_InvoiceQuantity, luxLine1.JI_InvoiceQuantity);
			AssertEquals(line1.JI_InvoiceUQ, luxLine1.JI_InvoiceUQ);
			line1.JI_InvoiceQuantity = 222.22;
			line1.JI_InvoiceUQ = "EA";
			AssertEquals(line1.JI_InvoiceQuantity, luxLine1.JI_InvoiceQuantity);
			AssertEquals(line1.JI_InvoiceUQ, luxLine1.JI_InvoiceUQ);

			var line2 = declaration.FilteredInvoiceLines.AddNew();
			line2.CA_ApplyLuxuryTax = true;
			var luxLine2 = line2.LuxuryTaxInvoiceLine;
			AssertEquals(line2.CA_PageNumber, luxLine2.CA_PageNumber);
			AssertEquals(line2.JI_InvoiceQuantity, luxLine2.JI_InvoiceQuantity);
			AssertEquals(line2.JI_InvoiceUQ, luxLine2.JI_InvoiceUQ);

			line2.CA_PageNumber = 2;
			line2.JI_InvoiceQuantity = 333.33;
			line2.JI_InvoiceUQ = "KGM";
			AssertEquals(line2.CA_PageNumber, luxLine2.CA_PageNumber);
			AssertEquals(line2.JI_InvoiceQuantity, luxLine2.JI_InvoiceQuantity);
			AssertEquals(line2.JI_InvoiceUQ, luxLine2.JI_InvoiceUQ);
			AssertEquals(line1.CA_PageNumber, luxLine1.CA_PageNumber);
			AssertEquals(line1.JI_InvoiceQuantity, luxLine1.JI_InvoiceQuantity);
			AssertEquals(line1.JI_InvoiceUQ, luxLine1.JI_InvoiceUQ);
		}
	}
}

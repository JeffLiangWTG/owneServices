using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class IDrawbackEntryLineExtensionMethodTest : TestCaseWithFactory
	{
		public void TestGetInvoiceLines()
		{
			var importDeclaration = Factory.New<JobDeclaration>();
			importDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			importDeclaration.JE_MergeBy = "TRF";

			var invoice = importDeclaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 3000m;
			invoice.JZ_RX_NKInvoice_Currency = "AUD";
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_PartNo = "PART3";
			invoiceLine1.JI_Tariff = "1704.90.00 44";
			invoiceLine1.JI_LinePrice = 1200m;
			invoiceLine1.JI_CountryOfOrigin = "CA";
			invoiceLine1.JI_InvoiceUQ = Enterprise.Core.Constants.Area.SquareCentimetre;
			invoiceLine1.JI_InvoiceQuantity = 3000;
			invoiceLine1.JI_CustomsQuantity = 6m;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_PartNo = "PART4";
			invoiceLine2.JI_Tariff = "1704.90.00 44";
			invoiceLine2.JI_LinePrice = 1000m;
			invoiceLine2.JI_InvoiceUQ = Enterprise.Core.Constants.Area.SquareCentimetre;
			invoiceLine2.JI_InvoiceQuantity = 10000;
			invoiceLine2.JI_CustomsQuantity = 20m;
			invoiceLine2.JI_CountryOfOrigin = "CA";

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_PartNo = "PART3";
			invoiceLine3.JI_Tariff = "1704.90.00 44";
			invoiceLine3.JI_LinePrice = 800m;
			invoiceLine3.JI_CountryOfOrigin = "CA";
			invoiceLine3.JI_InvoiceUQ = Enterprise.Core.Constants.Area.SquareCentimetre;
			invoiceLine3.JI_InvoiceQuantity = 2000;
			invoiceLine3.JI_CustomsQuantity = 4m;

			var merger = new LineMerger(importDeclaration);
			merger.DoMerge();

			AssertEquals(1, importDeclaration.CustomsEntryHeaders.Count);
			AssertEquals(1, importDeclaration.CustomsEntryHeaders[0].AllEntryLines.Count);

			var entryLine = (IDrawbackEntryLine)importDeclaration.CustomsEntryHeaders[0].AllEntryLines[0];
			AssertEquals(3, entryLine.GetInvoiceLines().Length);
		}
	}
}

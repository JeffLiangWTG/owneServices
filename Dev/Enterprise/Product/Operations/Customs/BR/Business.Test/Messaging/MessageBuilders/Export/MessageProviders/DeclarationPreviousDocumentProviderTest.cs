using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BR.Business.Export.Testing
{
	class DeclarationPreviousDocumentProviderTest : TestCaseWithFactory
	{
		public void TestDeclarationPreviousDocumentProvider()
		{
			var oInvoiceLine = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew();
			var previousDoc = oInvoiceLine.PreviousDocuments.AddNew();
			previousDoc.CSI_ReferenceNumber = "00000000";
			previousDoc.CSI_ReferenceNumber2 = "Dossie001";
			previousDoc.CSI_Code = ExportPreviousDocumentList.Codes.DI;
			previousDoc.CSI_Quantity = 10m;
			previousDoc.CSI_LineNo = 10;

			var dpreviousDoc = new DeclarationPreviousDocumentProvider(previousDoc);

			AssertEquals("ID should be", "00000000", dpreviousDoc.ID);
			AssertEquals("TypeOfEntry should be", ExportPreviousDocumentList.Codes.DI, dpreviousDoc.TypeOfEntry);
			AssertEquals("CustomsQuantity should be", 10m, dpreviousDoc.CustomsQuantity);
			AssertEquals("LineNo should be", 10, dpreviousDoc.LineNo);
			AssertEquals("ID should be", "Dossie001", dpreviousDoc.DigitalServiceDossiers);
		}
	}
}

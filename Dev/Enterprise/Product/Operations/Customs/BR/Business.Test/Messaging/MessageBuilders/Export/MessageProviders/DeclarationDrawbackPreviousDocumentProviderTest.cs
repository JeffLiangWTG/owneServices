using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BR.Business.Export.Testing
{
	class DeclarationDrawbackPreviousDocumentProviderTest : TestCaseWithFactory
	{
		public void TestDeclarationDrawbackPreviousDocumentProvider()
		{
			var oInvoiceLine = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew();
			var drawback = oInvoiceLine.SuspensionDrawbackCollection.AddNew();
			drawback.CSI_ReferenceNumber2 = "1111111";
			drawback.CSI_Tariff = "88888888";
			drawback.CSI_ReferenceNumber = "0000000000000";
			drawback.CSI_Value = 25m;
			drawback.CSI_LineNo = 1;
			drawback.CSI_Quantity = 10m;

			var drawbackEntryDocument = drawback.SuspensionDrawbackImportEntryDocumentCollection.AddNew();
			drawbackEntryDocument.CSI_ReferenceNumber = "00000";
			drawbackEntryDocument.CSI_SubType = "DI";
			drawbackEntryDocument.CSI_Value = 25m;
			drawbackEntryDocument.CSI_Quantity = 10m;
			drawbackEntryDocument.CSI_LineNo = 1;

			var drawbackEntryDoc = new DeclarationDrawbackPreviousDocumentProvider(drawbackEntryDocument);

			AssertEquals("ID should be", "00000", drawbackEntryDoc.ID);
			AssertEquals("CategoryCode should be", "DI", drawbackEntryDoc.CategoryCode);
			AssertEquals("QuantityQuantity should be", 10m, drawbackEntryDoc.QuantityQuantity);
			AssertEquals("AmountAmount should be", 25m, drawbackEntryDoc.AmountAmount);
			AssertEquals("ItemID should be", 1, drawbackEntryDoc.ItemID);
		}
	}
}


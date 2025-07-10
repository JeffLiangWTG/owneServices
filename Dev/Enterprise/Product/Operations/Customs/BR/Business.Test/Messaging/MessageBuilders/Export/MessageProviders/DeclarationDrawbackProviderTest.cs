using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.BR.Business.Export.Testing
{
	class DeclarationDrawbackProviderTest : TestCaseWithFactory
	{
		public void TesDeclarationDrawbackProvider()
		{
			var oInvoiceLine = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew();
			var drawback = oInvoiceLine.SuspensionDrawbackCollection.AddNew();
			drawback.CSI_ReferenceNumber2 = "1111111";

			var drawbackInvoice = drawback.SuspensionDrawbackInvoiceCollection.AddNew();
			drawbackInvoice.CSI_ReferenceNumber = "INV01";
			drawbackInvoice.CSI_DateOfIssue = new ZDateTime(2023, 1, 1);
			drawbackInvoice.CSI_Value = 25m;
			drawbackInvoice.CSI_Quantity = 10m;

			var dDrawbackInvoice = new DeclarationDrawbackInvoiceProvider(drawbackInvoice);

			AssertEquals("ID should be", "INV01", dDrawbackInvoice.ID);
			AssertEquals("IssueDate should be", new ZDateTime(2023, 1, 1), dDrawbackInvoice.IssueDate);
			AssertEquals("TradingCurrencyValue should be", "0000000000000", dDrawbackInvoice.TradingCurrencyValue);
			AssertEquals("Quantity should be", ZDecimal.Zero, dDrawbackInvoice.Quantity);
		}
	}
}

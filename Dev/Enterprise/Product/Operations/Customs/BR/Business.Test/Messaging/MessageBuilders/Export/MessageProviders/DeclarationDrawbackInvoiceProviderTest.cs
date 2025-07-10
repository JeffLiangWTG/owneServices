using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.BR.Business.Export.Testing
{
	class DeclarationDrawbackInvoiceProviderTest : TestCaseWithFactory
	{
		public void TesDeclarationDrawbackInvoiceProvider()
		{
			var oInvoiceLine = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew();
			var drawback = oInvoiceLine.SuspensionDrawbackCollection.AddNew();
			drawback.CSI_ReferenceNumber2 = "1111111";
			drawback.CSI_Tariff = "88888888";
			drawback.CSI_ReferenceNumber = "0000000000000";
			drawback.CSI_Value = 25m;
			drawback.CSI_LineNo = 1;
			drawback.CSI_Quantity = 10m;
			drawback.CSI_SubType = TypeSuspensionDrawbackList.Codes.Common;

			var drawbackInvoice = drawback.SuspensionDrawbackInvoiceCollection.AddNew();
			drawbackInvoice.CSI_ReferenceNumber = "INV01";
			drawbackInvoice.CSI_DateOfIssue = ZDateTime.Today;
			drawbackInvoice.CSI_Value = 25m;
			drawbackInvoice.CSI_Quantity = 10m;

			var dDrawback = new DeclarationDrawbackProvider(drawback);

			AssertEquals("ID should be", "1111111", dDrawback.ID);
			AssertEquals("DrawbackHsClassification should be", "88888888", dDrawback.DrawbackHsClassification);
			AssertEquals("DrawbackRecipientId should be", "0000000000000", dDrawback.DrawbackRecipientId);
			AssertEquals("ValueWithoutExchangeCoverAmount should be", ZDecimal.Zero, dDrawback.ValueWithoutExchangeCoverAmount);
			AssertEquals("ValueWithExchangeCoverAmount should be", 25m, dDrawback.ValueWithExchangeCoverAmount);
			AssertEquals("ItemID should be", 1, dDrawback.ItemID);
			AssertEquals("Quantity should be", 10m, dDrawback.Quantity);
			AssertEquals("Invoices count should be", 1, dDrawback.Invoices.Count());
			AssertEquals("CategoryCode should be", TypeSuspensionDrawbackList.Codes.Common, dDrawback.CategoryCode);
		}
	}
}


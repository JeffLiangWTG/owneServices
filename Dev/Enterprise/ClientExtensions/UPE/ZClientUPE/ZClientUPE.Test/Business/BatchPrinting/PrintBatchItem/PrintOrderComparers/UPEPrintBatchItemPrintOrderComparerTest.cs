using NUnit.Framework;

namespace Enterprise.Client.UPE.Business
{
	public class UPEPrintBatchItemPrintOrderComparerTest : TestCase
	{
		public void TestNew()
		{
			UPEPrintBatchItemPrintOrderComparer taxInvoice = UPEPrintBatchItemPrintOrderComparer.New(UPEPrintBatchTypes.Codes.TaxInvoice);
			AssertEquals("Should return the correct comparer for TaxInvoice", typeof(UPETaxInvoicePrintBatchItemComparer), taxInvoice.GetType());
			UPEPrintBatchItemPrintOrderComparer shipmentHeldLetter = UPEPrintBatchItemPrintOrderComparer.New(UPEPrintBatchTypes.Codes.ShipmentHeldLetter);
			AssertEquals("Should return no comparer for ShipmentHeldLetter", null, shipmentHeldLetter);
		}
	}
}

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;

namespace Enterprise.Client.UPE.Business.Testing
{
	sealed class UPETaxInvoiceAndCommercialInvoiceAutoDeliveryTest : UPETaxInvoiceAutoDeliveryTest
	{
		protected override UPEDocumentAutoDelivery NewDocumentAutoDelivery()
		{
			return new UPETaxInvoiceAndCommercialInvoiceAutoDelivery(Callout);
		}

		protected override ZString ExpectedDeliveryFailureEmailBody
		{
			get
			{
				return @"
Delivery instructions incomplete for Tax + Commercial Invoice; Generated 11-Nov-05 00:00:00

HAWB              : HAWB
Bill To Name      : Bill-to Party
Bill To Account # : BillToAccountNum

Error: DeliveryAddress: Please enter a Fax Number.
";
			}
		}

		protected override DocumentCommand ExpectedDocumentCommand
		{
			get
			{
				return DocumentLoader.LoadUPSTaxInvoice();
			}
		}

		protected override DocumentCommand ExpectedDocumentCommandForFailureEmail => DocumentLoader.LoadUPSTaxInvoiceAndCommercialInvoice();

		public void TestPrintCommercialInvoiceDocument()
		{
			UPEPrintBatchItem.Loader loader = new UPEPrintBatchItem.Loader(Factory);
			DocumentCommand taxInvoice = new UPEDocumentMenuItemLoader(Factory).LoadUPSTaxInvoice();
			DocumentCommand taxInvoiceAndCommercialInvoice = new UPEDocumentMenuItemLoader(Factory).LoadUPSTaxInvoiceAndCommercialInvoice();
			OrgDocument.OD_DeliverBy = Core.Constants.ContactNotifyModes.Print;
			Factory.Save();
			Delivery.Deliver();
			UPEPrintBatchItem printItem = loader.LoadPrintBatchItem(ExpectedPrintBatchType, DocumentSupportable.Identifier, taxInvoiceAndCommercialInvoice.PK);
			AssertNull("Only Tax Invoice should be printed", printItem);
			printItem = loader.LoadPrintBatchItem(ExpectedPrintBatchType, DocumentSupportable.Identifier, taxInvoice.PK);
			AssertEquals("Document should be queued with the correct document", taxInvoice.PK, printItem.T6_SU);
			AssertEquals("Document should be queued with the correct print batch type", ExpectedPrintBatchType, printItem.PrintBatch.T7_BatchType);
			StmPrintJob[] printJobs = (StmPrintJob[])Factory.Load(typeof(StmPrintJob), new ZQuery());
			AssertEquals("Document queued for batch print, no documents should be physically printed", 0, printJobs.Length);
		}

		public void TestEmailFaxCommercialInvoiceDocument()
		{
			OrgDocument.OD_DeliverBy = Core.Constants.ContactNotifyModes.Email;
			DeliveryContact.OC_Email = "kot.matroskin@edi.com.au";
			Factory.Save();
			Delivery.Deliver();
			StmPrintJob[] printJobs = (StmPrintJob[])Factory.Load(typeof(StmPrintJob), new ZQuery());
			AssertEquals("Document shouldnt be emailed automatically", 0, printJobs.Length);
		}
	}
}

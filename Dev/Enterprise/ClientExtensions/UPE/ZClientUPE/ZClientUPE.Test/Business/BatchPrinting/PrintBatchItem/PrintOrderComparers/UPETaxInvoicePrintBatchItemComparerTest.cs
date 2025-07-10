using CargoWise.EntityFramework;
using Enterprise.Client.UPE.Business.Testing;

namespace Enterprise.Client.UPE.Business
{
	sealed class UPETaxInvoicePrintBatchItemComparerTest : TestCaseWithClientSpecificDocuments
	{
		public void TestSortOrder()
		{
			Callout callout1 = Factory.NewWithValidTestData<Callout>();
			callout1.BillToAccountNumber = "AccountID1";
			callout1.InvoiceNumber = "InvoiceNumber1";
			QueueTaxInvoiceForBatchPrintAndSave(callout1);
			Callout callout2 = Factory.NewWithValidTestData<Callout>();
			callout2.BillToAccountNumber = "AccountID2";
			callout2.InvoiceNumber = "InvoiceNumber1";
			QueueTaxInvoiceForBatchPrintAndSave(callout2);
			Callout callout3 = Factory.NewWithValidTestData<Callout>();
			callout3.BillToAccountNumber = "AccountID2";
			callout3.InvoiceNumber = "InvoiceNumber2";
			QueueTaxInvoiceForBatchPrintAndSave(callout3);
			BusinessObjectFactory loadingFactory = new BusinessObjectFactory();
			TestUPEPrintBatch printBatch = (TestUPEPrintBatch)new TestUPEPrintBatch.Loader(loadingFactory).CreateOrLoadLatestBatch(UPEPrintBatchTypes.Codes.TaxInvoice);
			AssertEquals(callout1.PK, printBatch.PrintItems[0].Parent.PK);
			AssertEquals(callout2.PK, printBatch.PrintItems[1].Parent.PK);
			AssertEquals(callout3.PK, printBatch.PrintItems[2].Parent.PK);
		}

		void QueueTaxInvoiceForBatchPrintAndSave(Callout callout)
		{
			UPEPrintBatch currentPrintBatch = new UPEPrintBatch.Loader(Factory).CreateOrLoadLatestBatch(UPEPrintBatchTypes.Codes.TaxInvoice);
			currentPrintBatch.QueueForBatchPrintAndSave(callout, new UPEDocumentMenuItemLoader(Factory).LoadUPSTaxInvoice().PK);
		}
	}
}

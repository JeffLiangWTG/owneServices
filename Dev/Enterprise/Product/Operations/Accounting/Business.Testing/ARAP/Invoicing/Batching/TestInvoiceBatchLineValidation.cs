using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class TestInvoiceBatchLineValidation : TestCaseWithFactory
	{
		public void TestInvoiceLineNotAlreadyPartOfBatch()
		{
			InvoiceBatchHeader thisInvoiceBatch = Factory.NewWithValidTestData(typeof(InvoiceBatchHeader)) as InvoiceBatchHeader;
			InvoiceBatchHeader anotherInvoiceBatch = Factory.NewWithValidTestData(typeof(InvoiceBatchHeader)) as InvoiceBatchHeader;
			anotherInvoiceBatch.AH_TransactionNum = "777";

			ARInvoice aRInvoiceGonnaGetBatched = Factory.NewWithValidTestData<ARInvoice>();
			aRInvoiceGonnaGetBatched.AH_TransactionNum = "666";

			ARInvoice aRInvoiceForThisBatch = Factory.NewWithValidTestData<ARInvoice>();

			thisInvoiceBatch.Line.Add(aRInvoiceGonnaGetBatched);
			thisInvoiceBatch.Line.Add(aRInvoiceForThisBatch);

			AssertCollectionContains("Should contain GonnaGetBatched", aRInvoiceGonnaGetBatched, thisInvoiceBatch.Line);
			AssertCollectionContains("Should contain NotBatched", aRInvoiceForThisBatch, thisInvoiceBatch.Line);

			aRInvoiceGonnaGetBatched.AH_AH_InvoiceStatement = anotherInvoiceBatch.PK;

			thisInvoiceBatch.RunPreSaveValidation();
			AssertHasRowError(aRInvoiceGonnaGetBatched, "This invoice is already part of batch 777.\r\nRedo search for invoices to batch or exclude this invoice from the batch.");

			thisInvoiceBatch.Line.Remove(aRInvoiceGonnaGetBatched);
			thisInvoiceBatch.RunPreSaveValidation();
			AssertNoRowError(thisInvoiceBatch, "Invoices 666, 999 and Whatever are already part of batch 777.\r\nInvoice te he he is already part of batch Im a complete batch.\r\nRedo search for invoices to batch or exclude these invoices from the batch.");
		}

		[ExpectNoExceptions("No System.NullReferenceException should occur")]
		public void TestCheckNotAlreadyInBatchWhenBatchingHandlesInvalidAH_AH_InvoiceStatementValue()
		{
			InvoiceBatchHeader invoiceBatch = Factory.NewWithValidTestData(typeof(InvoiceBatchHeader)) as InvoiceBatchHeader;
			invoiceBatch.AH_TransactionNum = "A380";
			ARInvoice aRInvoiceForThisBatch = Factory.NewWithValidTestData<ARInvoice>();
			invoiceBatch.Line.Add(aRInvoiceForThisBatch);

			//Empty Guid
			aRInvoiceForThisBatch.AH_AH_InvoiceStatement = new ZGuid();
			invoiceBatch.RunPreSaveValidation();

			//Invalid Guid
			aRInvoiceForThisBatch.AH_AH_InvoiceStatement = new ZGuid("0000ffff-ffff-ffff-ffff-ffffffffffff");
			invoiceBatch.RunPreSaveValidation();

			//Null Batch
			aRInvoiceForThisBatch.AH_AH_InvoiceStatement = new ZGuid("935af866-e695-47ea-acf7-487df1576704");
			invoiceBatch.RunPreSaveValidation();
		}
	}
}

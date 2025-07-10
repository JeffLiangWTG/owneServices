using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(InvoiceBatchLineCollection))]
	public class InvoiceBatchLineCollectionTest : InvoicingBaseCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new InvoiceBatchLineCollection(Factory);
		}

		[ExpectException(typeof(NotSupportedException))]
		public new void TestAddNew()
		{
			((InvoiceBatchLineCollection)GetCollectionToTest()).AddNew();
		}

		public new void TestAllowNew()
		{
			AssertEquals("Allow New by default", false, TestCollection.AllowNew);
		}

		public void TestFullyPaidFilter()
		{
			ARInvoice fullyPaidInvoice = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			ARInvoice partiallyPaidInvoice = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			ARInvoice unPaidInvoice = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			ARInvoice zeroAmountInvoice = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;

			fullyPaidInvoice.AH_InvoiceAmount = 120m;
			fullyPaidInvoice.AH_OSTotal = 120m;
			fullyPaidInvoice.AH_FullyPaidDate = ZDateTime.Now;
			fullyPaidInvoice.AH_OutstandingAmount = 0m;

			partiallyPaidInvoice.AH_InvoiceAmount = 120m;
			partiallyPaidInvoice.AH_OSTotal = 120m;
			partiallyPaidInvoice.AH_OutstandingAmount = 50m;

			unPaidInvoice.AH_InvoiceAmount = 120m;
			unPaidInvoice.AH_OSTotal = 120m;
			unPaidInvoice.AH_OutstandingAmount = 120m;

			zeroAmountInvoice.AH_InvoiceAmount = 0m;
			zeroAmountInvoice.AH_OSTotal = 0m;
			zeroAmountInvoice.AH_FullyPaidDate = ZDateTime.Now;
			zeroAmountInvoice.AH_OutstandingAmount = 0m;

			InvoiceBatchLineCollection resultCollection = new InvoiceBatchLineCollection(Factory);
			resultCollection.Load();

			AssertEquals(2, resultCollection.Count);
			Assert(resultCollection.Contains(unPaidInvoice.PK));
			Assert(resultCollection.Contains(zeroAmountInvoice.PK));
		}

		public void TestPaymentStatusFilter()
		{
			ARInvoice zeroAmountInvoice = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			zeroAmountInvoice.AH_InvoiceAmount = 0.1m;
			zeroAmountInvoice.AH_OSTotal = 0m;
			zeroAmountInvoice.AH_OutstandingAmount = 0.1m;

			var resultCollection = new InvoiceBatchLineCollection(Factory);
			resultCollection.Load();

			AssertEquals(1, resultCollection.Count);
			Assert(resultCollection.Contains(zeroAmountInvoice.PK));

			zeroAmountInvoice.AH_FullyPaidDate = ZDateTime.Now;
			zeroAmountInvoice.AH_OutstandingAmount = 0m;
			resultCollection.Load();

			AssertEquals(0, resultCollection.Count);
		}

		public void TestTransactionTypeFilter()
		{
			ARInvoice aRInvoice = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			ARCreditNote aRCreditNote = Factory.NewWithValidTestData(typeof(ARCreditNote)) as ARCreditNote;
			ARPayment aRPayment = Factory.NewWithValidTestData(typeof(ARPayment)) as ARPayment;
			ARJournal aRJournal = Factory.NewWithValidTestData(typeof(ARJournal)) as ARJournal;

			InvoiceBatchLineCollection result = GetCollectionToTest() as InvoiceBatchLineCollection;
			result.Load();

			AssertEquals(2, result.Count);
			Assert(result.Contains(aRInvoice.PK));
			Assert(result.Contains(aRCreditNote.PK));
			Assert(!result.Contains(aRPayment.PK));
			Assert(!result.Contains(aRJournal.PK));
		}

		public void TestUnPaidFilterWithOtherTaxes()
		{
			ARInvoice unPaidInvoice = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			unPaidInvoice.AH_InvoiceAmount = 120m;
			unPaidInvoice.AH_GSTAmount = 10M;
			unPaidInvoice.AH_LocalTaxAmountOtherTaxes = 10M;

			InvoiceBatchLineCollection resultCollection = new InvoiceBatchLineCollection(Factory);
			resultCollection.Load();

			AssertEquals(140m, unPaidInvoice.AH_LocalTotal);
			AssertEquals(140m, unPaidInvoice.AH_OutstandingAmount);

			AssertEquals(1, resultCollection.Count);
			Assert(resultCollection.Contains(unPaidInvoice.PK));
		}

		public void TestLedgerFilter()
		{
			ARInvoice aRInvoice = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			ARCreditNote aRCreditNote = Factory.NewWithValidTestData(typeof(ARCreditNote)) as ARCreditNote;
			APInvoice aPInvoice = Factory.NewWithValidTestData(typeof(APInvoice)) as APInvoice;
			APCreditNote aPCreditNote = Factory.NewWithValidTestData(typeof(APCreditNote)) as APCreditNote;

			InvoiceBatchLineCollection result = GetCollectionToTest() as InvoiceBatchLineCollection;
			result.Load();

			AssertEquals(2, result.Count);
			Assert(result.Contains(aRInvoice.PK));
			Assert(result.Contains(aRCreditNote.PK));
			Assert(!result.Contains(aPInvoice.PK));
			Assert(!result.Contains(aPCreditNote.PK));
		}

		public void TestLoadingExistingTransaction()
		{
			InvoiceBatchHeader invoiceBatch = Factory.NewWithValidTestData(typeof(InvoiceBatchHeader)) as InvoiceBatchHeader;

			ARInvoice aRInvoice = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			aRInvoice.AH_AH_InvoiceStatement = invoiceBatch.PK;
			ARCreditNote aRCreditNote = Factory.NewWithValidTestData(typeof(ARCreditNote)) as ARCreditNote;
			aRCreditNote.AH_AH_InvoiceStatement = invoiceBatch.PK;

			ARInvoice aRInvoiceNotBatched = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			ARCreditNote aRCreditNoteNotBatched = Factory.NewWithValidTestData(typeof(ARCreditNote)) as ARCreditNote;

			Factory.Save();

			InvoiceBatchLineCollection result = new InvoiceBatchLineCollection(Factory, invoiceBatch);
			result.Load();

			AssertEquals(2, result.Count);

			Assert(result.Contains(aRInvoice.PK));
			Assert(result.Contains(aRCreditNote.PK));
			Assert(!result.Contains(aRInvoiceNotBatched.PK));
			Assert(!result.Contains(aRCreditNoteNotBatched.PK));
		}

		public void TestSelectedCount()
		{
			InvoiceBatchHeader batchHeader = Factory.NewWithValidTestData(typeof(InvoiceBatchHeader)) as InvoiceBatchHeader;

			ARInvoice aRInvoice1 = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			ARInvoice aRInvoice2 = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			ARInvoice aRInvoice3 = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;

			InvoiceBatchLineCollection collection = new InvoiceBatchLineCollection(Factory, batchHeader);

			collection.Add(aRInvoice1);
			collection.Add(aRInvoice2);
			collection.Add(aRInvoice3);

			AssertEquals(3, collection.SelectedCount);

			aRInvoice1.IncludeInTheBatch = false;

			AssertEquals(2, collection.SelectedCount);
		}

		public void TesSetTheIncludeBatchFlagOnAddedAndRemoved()
		{
			var aRInvoice = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			var aRCreditNote = Factory.NewWithValidTestData(typeof(ARCreditNote)) as ARCreditNote;

			Assert(!aRInvoice.IncludeInTheBatch);
			Assert(!aRCreditNote.IncludeInTheBatch);

			var collection = new InvoiceBatchLineCollection(Factory);

			collection.Add(aRInvoice);
			collection.Add(aRCreditNote);

			Assert(aRInvoice.IncludeInTheBatch);
			Assert(aRCreditNote.IncludeInTheBatch);

			collection.RemoveAll();

			Assert(!aRInvoice.IncludeInTheBatch);
			Assert(!aRCreditNote.IncludeInTheBatch);
		}

		public void TestAllEntriesSetToReadOnly()
		{
			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.IncludeInTheBatch = true;
			InvoiceBatchHeader invoiceBatchHeader = Factory.New<InvoiceBatchHeader>();
			invoiceBatchHeader.Line.Add(invoice);
			invoice.AH_AH_InvoiceStatement = invoiceBatchHeader.PK;
			Factory.Save();

			InvoiceBatchHeader invoiceBatchHeaderInNewFactory = new BusinessObjectFactory().Load<InvoiceBatchHeader>(invoiceBatchHeader.PK);
			AssertEquals(true, invoiceBatchHeaderInNewFactory.Line[0].ReadOnly);
		}
	}
}

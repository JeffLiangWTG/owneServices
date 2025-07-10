using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Accounting.Business.Riba.AccCollectionOrder;

namespace Enterprise.Accounting.Business.Riba.Testing
{
	[TestedType(typeof(AccCollectionBatchPoster))]
	public class AccCollectionBatchPosterTest : NonPersistentBusinessObjectTestCase
	{
		[TestDate(2014, 11, 11)]
		public void TestCreateBatchOrdersAndFillByTransactions()
		{
			var invoices = CreateTestInvoices();
			Invoice1.AH_DueDate = new ZDateTime(2014, 10, 9);
			Invoice2.AH_DueDate = new ZDateTime(2014, 10, 11, 11, 20, 10);
			Invoice3.AH_DueDate = new ZDateTime(2014, 10, 10);
			Invoice4.AH_DueDate = new ZDateTime(2014, 10, 10);
			Invoice5.AH_DueDate = new ZDateTime(2014, 12, 12);
			Invoice6.AH_DueDate = new ZDateTime(2014, 12, 12);

			var poster = new AccCollectionBatchPoster(Factory);
			poster.CreateBatchOrdersAndFillByTransactions(invoices, AccountingConstants.CreateColletionOrdersBatchOption.GroupByDebtorAndDueDate);
			AssertNotNull(poster.Batch);
			AssertEquals(3, poster.Batch.CollectionOrders.Count);
			AssertEquals("AUD", poster.Batch.ACB_RX_NKCurrency);
			var order1 = poster.Batch.CollectionOrders.First(order => order.ACO_OH_Debtor == Creator.ABIGAS.PK);
			AssertEquals("AUD", order1.ACO_RX_NKCurrency);
			AssertEquals(new ZDate(2014, 11, 11), order1.ACO_CollectionDate);
			Assert(order1.CollectionOrderLines.Any(line => line.AOL_AH == Invoice1.PK));
			Assert(order1.CollectionOrderLines.Any(line => line.AOL_AH == Invoice2.PK));
			AssertCollectionOrderLinesCountDebtorAndCollectionDate(order1, 2, Creator.ABIGAS.PK, new ZDate(2014, 11, 11), DebtorValidation.DebtorIsRequired);

			var order2 = poster.Batch.CollectionOrders.First(order => order.ACO_OH_Debtor == Creator.AALSHI.PK && order.ACO_CollectionDate == new ZDate(2014, 11, 11));
			AssertEquals("AUD", order2.ACO_RX_NKCurrency);
			Assert(order2.CollectionOrderLines.Any(line => line.AOL_AH == Invoice3.PK));
			Assert(order2.CollectionOrderLines.Any(line => line.AOL_AH == Invoice4.PK));
			AssertCollectionOrderLinesCountDebtorAndCollectionDate(order2, 2, Creator.AALSHI.PK, new ZDate(2014, 11, 11), DebtorValidation.DebtorIsRequired);

			var order3 = poster.Batch.CollectionOrders.First(order => order.ACO_OH_Debtor == Creator.AALSHI.PK && order.ACO_CollectionDate == new ZDate(2014, 12, 12));
			AssertEquals("AUD", order3.ACO_RX_NKCurrency);
			Assert(order3.CollectionOrderLines.Any(line => line.AOL_AH == Invoice5.PK));
			Assert(order3.CollectionOrderLines.Any(line => line.AOL_AH == Invoice6.PK));
			AssertCollectionOrderLinesCountDebtorAndCollectionDate(order3, 2, Creator.AALSHI.PK, new ZDate(2014, 12, 12), DebtorValidation.DebtorIsRequired);

			poster.Batch.IsCancelled = true;

			AccCollectionBatchPoster poster2 = new AccCollectionBatchPoster(Factory);
			poster2.CreateBatchOrdersAndFillByTransactions(invoices, AccountingConstants.CreateColletionOrdersBatchOption.GroupByDebtor);
			AssertNotNull(poster2.Batch);
			AssertEquals(2, poster2.Batch.CollectionOrders.Count);
			AssertCollectionOrderLinesCountDebtorAndCollectionDate(poster2.Batch.CollectionOrders[0], 2, Creator.ABIGAS.PK, new ZDate(2014, 10, 11), DebtorValidation.DebtorIsRequired);
			AssertCollectionOrderLinesCountDebtorAndCollectionDate(poster2.Batch.CollectionOrders[1], 4, Creator.AALSHI.PK, new ZDate(2014, 12, 12), DebtorValidation.DebtorIsRequired);
		}

		[TestDate(2022, 07, 15)]
		public void TestCreateBatchOrdersAndFillByTransactions_GroupAllSelected()
		{
			var today = new ZDate(2022, 07, 15);
			var invoices = CreateTestInvoices();
			var collectionBatchPoster = new AccCollectionBatchPoster(Factory);
			collectionBatchPoster.CreateBatchOrdersAndFillByTransactions(invoices, AccountingConstants.CreateColletionOrdersBatchOption.GroupAllSelected);
			AssertNotNull(collectionBatchPoster.Batch);

			var collectionOrders = collectionBatchPoster.Batch.CollectionOrders;
			AssertEquals(1, collectionBatchPoster.Batch.CollectionOrders.Count);
			AssertCollectionOrderLinesCountDebtorAndCollectionDate(collectionOrders[0], 6, ZGuid.Empty, today, DebtorValidation.DebtorShouldBeEmpty);
		}

		void AssertCollectionOrderLinesCountDebtorAndCollectionDate(AccCollectionOrder collectionOrder, int expectedCollectionOrderLinesCount, ZGuid expectedDebtor, ZDate expectedCollectionDate, DebtorValidation expectedDebtorValidationType)
		{
			CombineAssertions(() =>
			{
				AssertEquals(expectedCollectionOrderLinesCount, collectionOrder.CollectionOrderLines.Count);
				AssertEquals(expectedDebtor, collectionOrder.ACO_OH_Debtor);
				AssertEquals(expectedCollectionDate, collectionOrder.ACO_CollectionDate);
				AssertEquals(expectedDebtorValidationType, collectionOrder.DebtorValidationType);
			});
		}

		[TestDate(2014, 11, 11)]
		public void TestCreateBatchUseForeignCurrencyOnlyIfAllInForeign()
		{
			var invoices = new AccTransactionHeaderCollection(Factory);
			var invoice1 = Creator.CreateInvoice(typeof(ARInvoice), Creator.USD, 1, Creator.ABIGAS);
			invoice1.AH_DueDate = new ZDateTime(2014, 10, 10);
			var invoice2 = Creator.CreateInvoice(typeof(ARInvoice), Creator.USD, 1, Creator.ABIGAS);
			invoice2.AH_DueDate = new ZDateTime(2014, 10, 10);
			var invoice3 = Creator.CreateInvoice(typeof(ARInvoice), Creator.USD, 1, Creator.AALSHI);
			invoice1.AH_DueDate = new ZDateTime(2014, 10, 10);
			var invoice4 = Creator.CreateInvoice(typeof(ARInvoice), Creator.USD, 1, Creator.AALSHI);
			invoice1.AH_DueDate = new ZDateTime(2014, 10, 10);
			invoices.AddRange(new InvoicingBase[] { invoice1, invoice2, invoice3, invoice4 });
			AccCollectionBatchPoster poster = new AccCollectionBatchPoster(Factory);
			poster.CreateBatchOrdersAndFillByTransactions(invoices, AccountingConstants.CreateColletionOrdersBatchOption.GroupByDebtorAndDueDate);
			AssertNotNull(poster.Batch);
			AssertEquals(2, poster.Batch.CollectionOrders.Count);
			AssertEquals("USD", poster.Batch.ACB_RX_NKCurrency);
		}

		AccTransactionHeaderCollection CreateTestInvoices()
		{
			var invoices = new AccTransactionHeaderCollection(Factory);
			Invoice1 = Creator.CreateInvoice(typeof(ARInvoice), Creator.AUD, 1, Creator.ABIGAS);
			Invoice2 = Creator.CreateInvoice(typeof(ARInvoice), Creator.USD, 1, Creator.ABIGAS);
			Invoice3 = Creator.CreateInvoice(typeof(ARInvoice), Creator.USD, 1, Creator.AALSHI);
			Invoice4 = Creator.CreateInvoice(typeof(ARInvoice), Creator.AUD, 1, Creator.AALSHI);
			Invoice5 = Creator.CreateInvoice(typeof(ARInvoice), Creator.USD, 1, Creator.AALSHI);
			Invoice6 = Creator.CreateInvoice(typeof(ARInvoice), Creator.USD, 1, Creator.AALSHI);

			invoices.AddRange(new InvoicingBase[] { Invoice1, Invoice2, Invoice3, Invoice4, Invoice5, Invoice6 });
			return invoices;
		}

		InvoicingBase Invoice1;
		InvoicingBase Invoice2;
		InvoicingBase Invoice3;
		InvoicingBase Invoice4;
		InvoicingBase Invoice5;
		InvoicingBase Invoice6;

		TestObjectCreator Creator => creator ?? (creator = new TestObjectCreator(Factory));
		TestObjectCreator creator;
	}
}

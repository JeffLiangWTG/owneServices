using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Riba;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(AccCollectionBatchFilterBusinessObject))]
	public class AccCollectionBatchFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestBatchCollectionOrders()
		{
			var fTestFilterBizO = (AccCollectionBatchFilterBusinessObject)GetNewFilterStripBusinessObject();
			AssertNotNull(fTestFilterBizO.BatchCollectionOrders);
			AssertEquals(0, fTestFilterBizO.BatchCollectionOrders.Count);
		}

		public void TestBatchNumberFilter()
		{
			ModuleTextFilter filter = (ModuleTextFilter)FilterBO["Collection Batch #"];
			filter.Property = batch1.ACB_BatchNumber;
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			collection = new AccCollectionBatchCollection(Factory, FilterBO.Filter);
			AssertEquals(1, collection.Count);
			Assert(collection.Contains(batch1));

			filter.Property = batch2.ACB_BatchNumber;
			collection = new AccCollectionBatchCollection(Factory, FilterBO.Filter);
			AssertEquals(1, collection.Count);
			Assert(collection.Contains(batch2));
		}

		public void TestBatchTypeFilter()
		{
			ModuleTextFilter filter = (ModuleTextFilter)FilterBO["Collection Batch Type"];
			filter.Property = batch1.ACB_Type;
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			collection = new AccCollectionBatchCollection(Factory, FilterBO.Filter);
			AssertEquals(1, collection.Count);
			Assert(collection.Contains(batch1));

			filter.Property = batch2.ACB_Type;
			collection = new AccCollectionBatchCollection(Factory, FilterBO.Filter);
			AssertEquals(1, collection.Count);
			Assert(collection.Contains(batch2));
		}

		[TestDate(2011, 11, 11)]
		public void TestCollectionDateFilter()
		{
			ModuleDateFilter filter = (ModuleDateFilter)FilterBO["Collection Date"];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2014, 10, 10);
			filter.Property2 = new ZDateTime(2014, 10, 10);
			filter.IsActive = true;
			collection = new AccCollectionBatchCollection(Factory, FilterBO.Filter);
			AssertEquals(2, collection.Count);
			Assert(collection.Contains(batch1));
			Assert(collection.Contains(batch2));

			filter.Property1 = new ZDateTime(2014, 12, 12);
			filter.Property2 = new ZDateTime(2014, 12, 12);
			collection = new AccCollectionBatchCollection(Factory, FilterBO.Filter);
			AssertEquals(1, collection.Count);
			Assert(collection.Contains(batch3));
		}

		public void TestOrganizationFilter()
		{
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterBO["Debtor"];
			filter.Property = creator.ABIGAS.PK;
			filter.IsActive = true;
			collection = new AccCollectionBatchCollection(Factory, FilterBO.Filter);
			AssertEquals(1, collection.Count);
			Assert(collection.Contains(batch1));

			filter.Property = creator.AALSHI.PK;
			filter.IsActive = true;
			collection = new AccCollectionBatchCollection(Factory, FilterBO.Filter);
			AssertEquals(2, collection.Count);
			Assert(collection.Contains(batch2));
			Assert(collection.Contains(batch3));
		}

		public void TestBankAccountFilter()
		{
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterBO["Bank Account"];
			filter.Property = creator.AUDBankAccount.PK;
			filter.IsActive = true;
			collection = new AccCollectionBatchCollection(Factory, FilterBO.Filter);
			AssertEquals(2, collection.Count);
			Assert(collection.Contains(batch1));
			Assert(collection.Contains(batch2));

			filter.Property = creator.USDBankAccount.PK;
			filter.IsActive = true;
			collection = new AccCollectionBatchCollection(Factory, FilterBO.Filter);
			AssertEquals(1, collection.Count);
			Assert(collection.Contains(batch3));
		}

		TestObjectCreator creator;
		AccCollectionBatchFilterBusinessObject FilterBO;
		AccCollectionBatchCollection collection;
		AccCollectionBatch batch1;
		AccCollectionBatch batch2;
		AccCollectionBatch batch3;

		protected override void SetUp()
		{
			base.SetUp();
			creator = new TestObjectCreator(Factory);
			var invoice1 = creator.CreateInvoiceWithLine(typeof(ARInvoice), "", creator.AUD, 1, 10, 0, 10, 0);
			invoice1.AH_OH = creator.ABIGAS.PK;
			invoice1.AH_DueDate = new ZDateTime(2014, 10, 10);

			var invoice2 = creator.CreateInvoiceWithLine(typeof(ARInvoice), "", creator.USD, 1, 10, 0, 10, 0);
			invoice2.AH_OH = creator.ABIGAS.PK;
			invoice2.AH_DueDate = new ZDateTime(2014, 10, 10);

			var invoice3 = creator.CreateInvoiceWithLine(typeof(ARInvoice), "", creator.USD, 1, 10, 0, 10, 0);
			invoice3.AH_OH = creator.AALSHI.PK;
			invoice3.AH_DueDate = new ZDateTime(2014, 10, 10);

			var invoice4 = creator.CreateInvoiceWithLine(typeof(ARInvoice), "", creator.AUD, 1, 10, 0, 10, 0);
			invoice4.AH_OH = creator.AALSHI.PK;
			invoice4.AH_DueDate = new ZDateTime(2014, 10, 10);

			var invoice5 = creator.CreateInvoiceWithLine(typeof(ARInvoice), "", creator.USD, 1, 10, 0, 10, 0);
			invoice5.AH_OH = creator.AALSHI.PK;
			invoice5.AH_DueDate = new ZDateTime(2014, 12, 12);

			var invoice6 = creator.CreateInvoiceWithLine(typeof(ARInvoice), "", creator.USD, 1, 10, 0, 10, 0);
			invoice6.AH_OH = creator.AALSHI.PK;
			invoice6.AH_DueDate = new ZDateTime(2014, 12, 12);

			Factory.Save();

			AccCollectionBatchPoster poster1 = new AccCollectionBatchPoster(Factory);
			AccTransactionHeaderCollection collection1 = new AccTransactionHeaderCollection(Factory);
			collection1.AddRange(new InvoicingBase[] { invoice1, invoice2 });
			poster1.CreateBatchOrdersAndFillByTransactions(collection1, AccountingConstants.CreateColletionOrdersBatchOption.GroupByDebtorAndDueDate);
			batch1 = poster1.Batch;
			batch1.ACB_Type = new ZString("ST1");
			batch1.ACB_AB = creator.AUDBankAccount.PK;
			AssertEquals(1, poster1.Batch.CollectionOrders.Count);

			AccCollectionBatchPoster poster2 = new AccCollectionBatchPoster(Factory);
			AccTransactionHeaderCollection collection2 = new AccTransactionHeaderCollection(Factory);
			collection2.AddRange(new InvoicingBase[] { invoice3, invoice4 });
			poster2.CreateBatchOrdersAndFillByTransactions(collection2, AccountingConstants.CreateColletionOrdersBatchOption.GroupByDebtorAndDueDate);
			batch2 = poster2.Batch;
			batch2.ACB_Type = new ZString("ST2");
			batch2.ACB_AB = creator.AUDBankAccount.PK;
			AssertEquals(1, poster2.Batch.CollectionOrders.Count);

			AccCollectionBatchPoster poster3 = new AccCollectionBatchPoster(Factory);
			AccTransactionHeaderCollection collection3 = new AccTransactionHeaderCollection(Factory);
			collection3.AddRange(new InvoicingBase[] { invoice5, invoice6 });
			poster3.CreateBatchOrdersAndFillByTransactions(collection3, AccountingConstants.CreateColletionOrdersBatchOption.GroupByDebtorAndDueDate);
			batch3 = poster3.Batch;
			batch3.ACB_AB = creator.USDBankAccount.PK;
			AssertEquals(1, poster3.Batch.CollectionOrders.Count);

			FilterBO = (AccCollectionBatchFilterBusinessObject)GetNewFilterStripBusinessObject();
			Factory.Save();
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new AccCollectionBatchFilterBusinessObject();
		}
	}
}

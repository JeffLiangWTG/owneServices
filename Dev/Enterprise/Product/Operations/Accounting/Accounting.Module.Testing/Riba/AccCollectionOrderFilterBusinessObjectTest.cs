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
	[TestedType(typeof(AccCollectionOrderFilterBusinessObject))]
	public class AccCollectionOrderFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestDefaultFilter()
		{
			var collection = new AccCollectionOrderCollection(Factory, FilterBO.Filter);
			AssertEquals(3, collection.Count);
			AssertCollectionContains(order1, collection);
			AssertCollectionContains(order2, collection);
			AssertCollectionContains(order3, collection);
			AssertCollectionNotContains(order4, collection);
		}

		public void TestBatchNumberFilter()
		{
			var filter = (ModuleTextFilter)FilterBO["Collection Batch #"];
			filter.Property = "ab";
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			var collection = new AccCollectionOrderCollection(Factory, FilterBO.Filter);
			AssertCollectionContains(order1, collection);

			filter.Property = "e";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			collection = new AccCollectionOrderCollection(Factory, FilterBO.Filter);
			AssertCollectionContains(order2, collection);
		}

		public void TestBatchTypeFilter()
		{
			ModuleTextFilter filter = (ModuleTextFilter)FilterBO["Collection Batch Type"];
			filter.Property = batch1.ACB_Type;
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			var collection = new AccCollectionOrderCollection(Factory, FilterBO.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(order1, collection);

			filter.Property = batch2.ACB_Type;
			collection = new AccCollectionOrderCollection(Factory, FilterBO.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(order2, collection);
		}

		public void TestOrderNumberFilter()
		{
			var filter = (ModuleTextFilter)FilterBO["Order #"];
			filter.Property = "12";
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			var collection = new AccCollectionOrderCollection(Factory, FilterBO.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(order1, collection);

			filter.Property = "5";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			collection = new AccCollectionOrderCollection(Factory, FilterBO.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(order2, collection);
		}

		[TestDate(2011, 11, 11)]
		public void TestCollectionDateFilter()
		{
			var filter = (ModuleDateFilter)FilterBO["Collection Date"];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = date1;
			filter.Property2 = date1;
			filter.IsActive = true;
			var collection = new AccCollectionOrderCollection(Factory, FilterBO.Filter);
			AssertEquals(2, collection.Count);
			AssertCollectionContains(order1, collection);
			AssertCollectionContains(order2, collection);

			filter.Property1 = date2;
			filter.Property2 = date2;
			collection = new AccCollectionOrderCollection(Factory, FilterBO.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(order3, collection);
		}

		public void TestDepositedDateFilter()
		{
			var filter = (ModuleDateFilter)FilterBO["Deposited Date"];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = date1;
			filter.Property2 = date1;
			filter.IsActive = true;
			var collection = new AccCollectionOrderCollection(Factory, FilterBO.Filter);
			AssertEquals(2, collection.Count);
			AssertCollectionContains(order1, collection);
			AssertCollectionContains(order2, collection);

			filter.Property1 = date2;
			filter.Property2 = date2;
			collection = new AccCollectionOrderCollection(Factory, FilterBO.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(order3, collection);
		}

		public void TestDebtorFilter()
		{
			var filter = (ModuleGuidFilter)FilterBO["Debtor"];
			filter.Property = creator.ABIGAS.PK;
			filter.IsActive = true;
			var collection = new AccCollectionOrderCollection(Factory, FilterBO.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(order1, collection);

			filter.Property = creator.AALSHI.PK;
			filter.IsActive = true;
			collection = new AccCollectionOrderCollection(Factory, FilterBO.Filter);
			AssertEquals(2, collection.Count);
			AssertCollectionContains(order2, collection);
			AssertCollectionContains(order3, collection);

			filter.Property = creator.NonCurrentCompany.GC_OH_OrgProxy;
			filter.IsActive = true;
			collection = new AccCollectionOrderCollection(Factory, FilterBO.Filter);
			AssertEquals(0, collection.Count);
		}

		public void TestBankAccountFilter()
		{
			var filter = (ModuleGuidFilter)FilterBO["Bank Account"];
			filter.Property = creator.AUDBankAccount.PK;
			filter.IsActive = true;
			var collection = new AccCollectionOrderCollection(Factory, FilterBO.Filter);
			AssertEquals(2, collection.Count);
			AssertCollectionContains(order1, collection);
			AssertCollectionContains(order2, collection);

			filter.Property = creator.USDBankAccount.PK;
			filter.IsActive = true;
			collection = new AccCollectionOrderCollection(Factory, FilterBO.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(order3, collection);

			var nonCurrentCompanyBankAccount = creator.GBPBankAccount;
			nonCurrentCompanyBankAccount.AB_GC = creator.NonCurrentCompany.PK;
			nonCurrentCompanyBankAccount.AB_GB = creator.NonCurrentBranch.PK;
			filter.Property = nonCurrentCompanyBankAccount.PK;
			filter.IsActive = true;
			collection = new AccCollectionOrderCollection(Factory, FilterBO.Filter);
			AssertEquals(0, collection.Count);
		}

		TestObjectCreator creator;
		readonly ZDateTime date1 = new ZDateTime(2014, 10, 10);
		readonly ZDateTime date2 = new ZDateTime(2014, 12, 12);

		AccCollectionOrderFilterBusinessObject FilterBO;
		AccCollectionOrder order1;
		AccCollectionOrder order2;
		AccCollectionOrder order3;
		AccCollectionOrder order4;
		AccCollectionBatch batch1;
		AccCollectionBatch batch2;
		AccCollectionBatch batch3;
		AccCollectionBatch batch4;

		protected override void SetUp()
		{
			base.SetUp();
			creator = new TestObjectCreator(Factory);

			var invoice1 = creator.CreateInvoiceWithLine(typeof(ARInvoice), "", creator.AUD, 1, 10, 0, 10, 0);
			invoice1.AH_OH = creator.ABIGAS.PK;
			invoice1.AH_DueDate = date1;

			var invoice2 = creator.CreateInvoiceWithLine(typeof(ARInvoice), "", creator.USD, 1, 10, 0, 10, 0);
			invoice2.AH_OH = creator.ABIGAS.PK;
			invoice2.AH_DueDate = date1;

			var invoice3 = creator.CreateInvoiceWithLine(typeof(ARInvoice), "", creator.USD, 1, 10, 0, 10, 0);
			invoice3.AH_OH = creator.AALSHI.PK;
			invoice3.AH_DueDate = date1;

			var invoice4 = creator.CreateInvoiceWithLine(typeof(ARInvoice), "", creator.AUD, 1, 10, 0, 10, 0);
			invoice4.AH_OH = creator.AALSHI.PK;
			invoice4.AH_DueDate = date1;

			var invoice5 = creator.CreateInvoiceWithLine(typeof(ARInvoice), "", creator.USD, 1, 10, 0, 10, 0);
			invoice5.AH_OH = creator.AALSHI.PK;
			invoice5.AH_DueDate = date2;

			var invoice6 = creator.CreateInvoiceWithLine(typeof(ARInvoice), "", creator.USD, 1, 10, 0, 10, 0);
			invoice6.AH_OH = creator.AALSHI.PK;
			invoice6.AH_DueDate = date2;

			var nonCurrComp = creator.NonCurrentCompany;
			var nonCurrBranch = nonCurrComp.FirstActiveBranch;
			var invoice7 = creator.CreateInvoiceWithLine(typeof(ARInvoice), "", creator.GBP, 1, 10, 0, 10, 0);
			invoice7.AH_OH = nonCurrComp.GC_OH_OrgProxy;
			invoice7.AH_DueDate = date1;
			invoice7.AH_GC = nonCurrComp.PK;
			invoice7.AH_GB = nonCurrBranch.PK;
			invoice7.Lines[0].AL_GC = nonCurrComp.PK;
			invoice7.Lines[0].AL_GB = nonCurrBranch.PK;

			Factory.Save();

			var poster1 = new AccCollectionBatchPoster(Factory);
			var collection1 = new AccTransactionHeaderCollection(Factory);
			collection1.AddRange(new InvoicingBase[] { invoice1, invoice2 });
			poster1.CreateBatchOrdersAndFillByTransactions(collection1, AccountingConstants.CreateColletionOrdersBatchOption.GroupByDebtorAndDueDate);
			batch1 = poster1.Batch;
			batch1.ACB_Type = "ST1";
			batch1.ACB_BatchNumber = "abc";
			batch1.ACB_AB = creator.AUDBankAccount.PK;
			AssertEquals(1, poster1.Batch.CollectionOrders.Count);
			order1 = poster1.Batch.CollectionOrders[0];
			AssertNotNull(order1);
			order1.ACO_OrderNumber = "123";
			order1.ACO_DepositedDate = date1.Date;

			var poster2 = new AccCollectionBatchPoster(Factory);
			var collection2 = new AccTransactionHeaderCollection(Factory);
			collection2.AddRange(new InvoicingBase[] { invoice3, invoice4 });
			poster2.CreateBatchOrdersAndFillByTransactions(collection2, AccountingConstants.CreateColletionOrdersBatchOption.GroupByDebtorAndDueDate);
			batch2 = poster2.Batch;
			batch2.ACB_Type = "ST2";
			batch2.ACB_BatchNumber = "def";
			batch2.ACB_AB = creator.AUDBankAccount.PK;
			AssertEquals(1, poster2.Batch.CollectionOrders.Count);
			order2 = poster2.Batch.CollectionOrders[0];
			AssertNotNull(order2);
			order2.ACO_OrderNumber = "456";
			order2.ACO_DepositedDate = date1.Date;

			var poster3 = new AccCollectionBatchPoster(Factory);
			var collection3 = new AccTransactionHeaderCollection(Factory);
			collection3.AddRange(new InvoicingBase[] { invoice5, invoice6 });
			poster3.CreateBatchOrdersAndFillByTransactions(collection3, AccountingConstants.CreateColletionOrdersBatchOption.GroupByDebtorAndDueDate);
			batch3 = poster3.Batch;
			batch3.ACB_BatchNumber = "ghi";
			batch3.ACB_AB = creator.USDBankAccount.PK;
			AssertEquals(1, poster3.Batch.CollectionOrders.Count);
			order3 = poster3.Batch.CollectionOrders[0];
			AssertNotNull(order3);
			order3.ACO_OrderNumber = "789";
			order3.ACO_DepositedDate = date2.Date;

			var poster4 = new AccCollectionBatchPoster(Factory);
			var collection4 = new AccTransactionHeaderCollection(Factory);
			collection4.AddRange(new InvoicingBase[] { invoice7 });
			poster4.CreateBatchOrdersAndFillByTransactions(collection4, AccountingConstants.CreateColletionOrdersBatchOption.GroupByDebtorAndDueDate);
			batch4 = poster4.Batch;
			batch4.ACB_Type = batch2.ACB_Type;
			batch4.ACB_BatchNumber = "jkl";
			batch4.ACB_AB = creator.GBPBankAccount.PK;
			batch4.ACB_GC = nonCurrComp.PK;
			AssertEquals(1, poster4.Batch.CollectionOrders.Count);
			order4 = poster4.Batch.CollectionOrders[0];
			AssertNotNull(order4);
			order4.ACO_OrderNumber = "000";
			order4.ACO_DepositedDate = date1.Date;

			FilterBO = (AccCollectionOrderFilterBusinessObject)GetNewFilterStripBusinessObject();
			Factory.Save();
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new AccCollectionOrderFilterBusinessObject();
		}
	}
}

using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.CashBook.DepositBatch;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(DepositBatchFilterBusinessObject))]
	public class DepositBatchFilterBusinessObjectTest : AccountingFilterStripBusinessObjectTestCase
	{
		public void TestBatchNumberFilter()
		{
			ARReceipt testReceipt1 = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			ARReceipt testReceipt2 = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 300M, TestObjectCreator.AUDBankAccount2.PK);
			Factory.Save();

			DepositBatchParent testDepositBatchParent = new DepositBatchParent(Factory);
			AssertEquals(2, testDepositBatchParent.DepositBatchLines.Count);
			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO["Batch Number"];

			filter.Property = "1000";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			AssertEquals(1, FilterCollection.Count);

			filter.Property = "00001001";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			AssertEquals(1, FilterCollection.Count);

			filter.Property = "2222";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			AssertEquals(0, FilterCollection.Count);
		}

		public void TestDepositDateFilter()
		{
			ARReceipt testReceipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			ARReceipt testReceipt2 = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 300M, TestObjectCreator.AUDBankAccount2.PK);

			Factory.Save();

			DepositBatchParent testDepositBatchParent = new DepositBatchParent(Factory);
			AssertEquals(2, testDepositBatchParent.DepositBatchLines.Count);

			testDepositBatchParent.DepositBatchLines[0].AH_InvoiceDate = ZDateTime.Today.AddDays(-2);
			testDepositBatchParent.DepositBatchLines[1].AH_InvoiceDate = ZDateTime.Today.AddDays(-1);

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterBO["Deposit Date"];

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Today.AddDays(-3);
			filter.Property2 = ZDateTime.Today.AddDays(-2);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			AssertEquals(1, FilterCollection.Count);

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Today.AddDays(-1);
			filter.Property2 = ZDateTime.Today;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			AssertEquals(1, FilterCollection.Count);

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Today.AddDays(-2);
			filter.Property2 = ZDateTime.Today.AddDays(-1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			AssertEquals(2, FilterCollection.Count);

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Today.AddDays(-5);
			filter.Property2 = ZDateTime.Today.AddDays(-4);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			AssertEquals(0, FilterCollection.Count);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new DepositBatchFilterBusinessObject();
		}

		DepositBatchFilterBusinessObject FilterBO;
		DepositBatchModuleCollection FilterCollection;

		protected override void SetUp()
		{
			base.SetUp();
			FilterCollection = new DepositBatchModuleCollection(Factory);
			FilterBO = (DepositBatchFilterBusinessObject)GetNewFilterStripBusinessObject();
		}
	}
}

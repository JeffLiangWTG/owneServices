using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.DepositBatch.Testing
{
	[TestedType(typeof(DepositBatchCollection))]
	public class DepositBatchCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAllowNew()
		{
			DepositBatchCollection testCollection = new DepositBatchCollection(Factory);
			AssertEquals(false, testCollection.AllowNew);
		}

		public void TestCreateDepositBatches()
		{
			ARReceipt testReceiptAUD1 = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			ARReceipt testReceiptAUD2 = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 200M, TestObjectCreator.AUDBankAccount.PK);

			ARReceipt testReceiptUSD1 = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.USDBankAccount.PK);
			ARReceipt testReceiptUSD2 = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 200M, TestObjectCreator.USDBankAccount.PK);
			Factory.Save();

			DepositBatchParent testParent = new DepositBatchParent(Factory);
			DepositBatchCollection testCollection = new DepositBatchCollection(Factory);

			testCollection.CreateDepositBatches(testParent);
			AssertEquals(2, testCollection.Count);
			AssertEquals(TestObjectCreator.AUDBankAccount.AB_Code, testCollection[0].BankCode);
			AssertEquals(TestObjectCreator.USDBankAccount.AB_Code, testCollection[1].BankCode);

			testParent.FilterByLocalCurrency = true;
			testCollection.CreateDepositBatches(testParent);
			AssertEquals(1, testCollection.Count);
			AssertEquals(TestObjectCreator.AUDBankAccount.AB_Code, testCollection[0].BankCode);

			testParent.FilterByForeignCurrency = true;
			testCollection.CreateDepositBatches(testParent);
			AssertEquals(1, testCollection.Count);
			AssertEquals(TestObjectCreator.USDBankAccount.AB_Code, testCollection[0].BankCode);
		}

		public void TestCreateDepositBatches_InvoiceDateAndPostDate()
		{
			var testReceiptAUD1 = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();

			var postDate = ZDateTime.Now.AddDays(-1);
			var invoiceDate = ZDateTime.Now.AddDays(-2);
			var testParent = new DepositBatchParent(Factory);
			testParent.DepositPostDate = postDate;
			testParent.DepositDate = invoiceDate;
			var testCollection = new DepositBatchCollection(Factory);

			testCollection.CreateDepositBatches(testParent);

			AssertEquals(1, testCollection.Count);
			AssertEquals(invoiceDate, testCollection[0].AH_InvoiceDate);
			AssertEquals(postDate, testCollection[0].AH_PostDate);
		}

		public void TestFilterByCompany()
		{
			ARReceipt testReceiptAUD = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);

			GlbBranchCollection testBranchCol = new GlbBranchCollection(Factory);
			testBranchCol.Load();

			ZGuid anotherBranchPK = ZGuid.Empty;
			foreach (GlbBranch branch in testBranchCol)
			{
				if (branch.GB_GC == GlbCompany.CurrentCompany.PK && branch.PK != GlbBranch.CurrentBranch.PK)
				{
					anotherBranchPK = branch.PK;
					break;
				}
			}

			ARReceipt testReceiptAnotherBranch = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 200M, TestObjectCreator.USDBankAccount.PK);
			testReceiptAnotherBranch.AH_GB = anotherBranchPK;
			Factory.Save();

			DepositBatchParent testParent = new DepositBatchParent(Factory);
			testParent.FilterByBranch = true;
			DepositBatchCollection testCollection = new DepositBatchCollection(Factory);
			testCollection.CreateDepositBatches(testParent);
			AssertEquals(1, testCollection.Count);

			testParent.FilterByCompany = true;
			DepositBatchCollection testCollection2 = new DepositBatchCollection(Factory);
			testCollection2.CreateDepositBatches(testParent);
			AssertEquals(2, testCollection2.Count);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new DepositBatchCollection(Factory);
		}

		TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}
				return fTestObjectCreator;
			}
		}
		TestObjectCreator fTestObjectCreator;

		#endregion
	}
}

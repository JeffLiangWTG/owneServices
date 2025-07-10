using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.DepositBatch.Testing
{
	[TestedType(typeof(DepositBatchTransactionLineCollection))]
	public class DepositBatchTransactionLineCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestOnAdded()
		{
			ARReceipt testReceipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();

			DepositBatch testDepositBatch = Factory.New<DepositBatch>();
			testDepositBatch.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			testDepositBatch.LoadTransactions(ZGuid.Empty);
			Factory.Save();

			DepositBatchTransactionLineCollection testCollection = new DepositBatchTransactionLineCollection(Factory, TestObjectCreator.AUDBankAccount.PK, GlbBranch.CurrentBranch.PK, testDepositBatch);
			testCollection.Load();
			AssertEquals(1, testCollection.Count);

			AssertEquals(testDepositBatch.PK, testCollection[0].Parent.PK);
		}

		public void TestCreateRelationshipFilter()
		{
			CreateTestCollectionWithTwoReceiptTransactions();
			AssertEquals(2, TestCollectionTwoReceiptTransactions.Count);
		}

		public void TestOriginalReceiptPKFilter()
		{
			ARReceipt testReceipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			ARReceipt testReceipt2 = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 200M, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();

			DepositBatch testDepositBatch = Factory.New<DepositBatch>();
			testDepositBatch.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			testDepositBatch.LoadTransactions(ZGuid.Empty);
			Factory.Save();

			TestCollectionTwoReceiptTransactions = new DepositBatchTransactionLineCollection(Factory, TestObjectCreator.AUDBankAccount.PK, GlbBranch.CurrentBranch.PK, testDepositBatch, testReceipt.PK);
			TestCollectionTwoReceiptTransactions.Load();
			AssertEquals(1, TestCollectionTwoReceiptTransactions.Count);
			Assert("Collection should contain only TestReceipt", TestCollectionTwoReceiptTransactions.Contains(testReceipt));
		}

		public void TestCreateRelationshipFilterWithOtherTransactionTypes()
		{
			ARReceipt testCancelledReceipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 1M, TestObjectCreator.AUDBankAccount.PK);
			testCancelledReceipt.AH_IsCancelled = true;
			((IMatching)testCancelledReceipt).CurrentMatchGroup.AddNew().AP_AH = testCancelledReceipt.PK;
			TestObjectCreator.SetupMatchLinkMatchDate(testCancelledReceipt);

			ARReceipt testReceiptAUDBank2 = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 2M, TestObjectCreator.AUDBankAccount2.PK);

			GlbBranch testBranch = Factory.NewWithValidTestData(typeof(GlbBranch)) as GlbBranch;
			testBranch.GB_Code = "TST";
			testBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			ARReceipt testReceiptDiffBranch = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 3M, TestObjectCreator.AUDBankAccount2.PK);
			testReceiptDiffBranch.AH_GB = testBranch.PK;

			ARReceipt testReceiptOtherTransactionType = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.DirectPayment, ZArchitecture.Core.LedgerTypes.CashBook, 4M, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();

			CreateTestCollectionWithTwoReceiptTransactions();

			AssertEquals(2, TestCollectionTwoReceiptTransactions.Count);
			AssertEquals(100M, TestCollectionTwoReceiptTransactions[0].AH_OSTotalAmount);
			AssertEquals(200M, TestCollectionTwoReceiptTransactions[1].AH_OSTotalAmount);
		}

		public void TestIsNewBatchFetch()
		{
			ARReceipt testReceipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 1M, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();

			DepositBatch testDepositBatch = Factory.New<DepositBatch>();
			testDepositBatch.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			testDepositBatch.LoadTransactions(ZGuid.Empty);
			Factory.Save();

			testReceipt.AH_IsCancelled = true;
			((IMatching)testReceipt).CurrentMatchGroup.AddNew().AP_AH = testReceipt.PK;
			TestObjectCreator.SetupMatchLinkMatchDate(testReceipt);
			Factory.Save();

			DepositBatchTransactionLineCollection testCollection = new DepositBatchTransactionLineCollection(Factory, TestObjectCreator.AUDBankAccount.PK, GlbBranch.CurrentBranch.PK, testDepositBatch);
			testCollection.Load();

			AssertEquals(1, testCollection.Count);
		}

		public void TestAllowNew()
		{
			DepositBatchTransactionLineCollection testCollection = new DepositBatchTransactionLineCollection(Factory, TestObjectCreator.AUDBankAccount.PK, GlbBranch.CurrentBranch.PK, null);
			AssertEquals(false, testCollection.AllowNew);
		}

		#region Implementation

		void CreateTestCollectionWithTwoReceiptTransactions()
		{
			ARReceipt testReceipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			ARReceipt testReceipt2 = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 200M, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();

			DepositBatch testDepositBatch = Factory.New<DepositBatch>();
			testDepositBatch.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			testDepositBatch.LoadTransactions(ZGuid.Empty);
			Factory.Save();

			TestCollectionTwoReceiptTransactions = new DepositBatchTransactionLineCollection(Factory, TestObjectCreator.AUDBankAccount.PK, GlbBranch.CurrentBranch.PK, testDepositBatch);
			TestCollectionTwoReceiptTransactions.Load();
		}
		DepositBatchTransactionLineCollection TestCollectionTwoReceiptTransactions;

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			DepositBatch depositBach = Factory.New<DepositBatch>();
			return new DepositBatchTransactionLineCollection(Factory, ZGuid.Empty, ZGuid.Empty, depositBach);
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

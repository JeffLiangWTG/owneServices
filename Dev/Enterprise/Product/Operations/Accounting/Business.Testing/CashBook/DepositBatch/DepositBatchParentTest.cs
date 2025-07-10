using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.DepositBatch.Testing
{
	[TestedType(typeof(DepositBatchParent))]
	public class DepositBatchParentTest : NonPersistentBusinessObjectTestCase
	{
		#region ITransaction Members

		public void TestITransactionLedger()
		{
			DepositBatchParent testDepositBatchParent = new DepositBatchParent(Factory);
			AssertEquals(LedgerTypes.CashBook, ((ITransaction)testDepositBatchParent).Ledger);
		}

		public void TestITransactionCurrencyCode()
		{
			ARReceipt testReceipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();

			DepositBatchParent testDepositBatchParent = new DepositBatchParent(Factory);
			Factory.Save();

			ZGuid firstBatchPK = testDepositBatchParent.DepositBatchLines[0].PK;
			DepositBatch testDepositBatch = Factory.Load<DepositBatch>(firstBatchPK);

			DepositBatchParent testSavedDepositBatchParent = new DepositBatchParent(Factory, testDepositBatch);
			AssertEquals(testDepositBatch.CurrencyCode, ((ITransaction)testSavedDepositBatchParent).CurrencyCode);
		}

		public void TestITransactionOverseasTotalAmount()
		{
			ARReceipt testReceipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();

			DepositBatchParent testDepositBatchParent = new DepositBatchParent(Factory);
			Factory.Save();

			ZGuid firstBatchPK = testDepositBatchParent.DepositBatchLines[0].PK;
			DepositBatch testDepositBatch = Factory.Load<DepositBatch>(firstBatchPK);

			DepositBatchParent testSavedDepositBatchParent = new DepositBatchParent(Factory, testDepositBatch);
			AssertEquals(testDepositBatch.AH_OSTotal, ((ITransaction)testDepositBatchParent).OverseasTotalAmount);
		}

		public void TestUserAllowedToBackPost()
		{
			DepositBatchParent dBParent = new DepositBatchParent(Factory);

			bool orginalSecurityValue = Env.Security.CashBookPostToPreviousOrFutureOpenPeriod.IsAllowed;
			try
			{
				Env.Security.CashBookPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
				AssertEquals("UserAllowedToBackPost", true, dBParent.UserAllowedToBackPost);

				Env.Security.CashBookPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
				AssertEquals("UserAllowedToBackPost", false, dBParent.UserAllowedToBackPost);
			}
			finally
			{
				Env.Security.CashBookPostToPreviousOrFutureOpenPeriod.IsAllowed = orginalSecurityValue;
			}
		}

		#region ReversalStatusCode

		public void TestReversalStatusCode_ShouldBeEmpty()
		{
			var testDepositBatchParent = new DepositBatchParent(Factory);
			AssertEquals(nameof(testDepositBatchParent.ReversalStatusCode), ZString.Empty, testDepositBatchParent.ReversalStatusCode);
		}

		public void TestReversalStatusCode_ReadOnly_ShouldBeTrue()
		{
			AssertEquals(nameof(ITransaction.ReversalStatusCode_ReadOnly), true, (new DepositBatchParent(Factory) as ITransaction).ReversalStatusCode_ReadOnly);
		}

		public void TestReversalStatusCodeList_ShouldBeNull()
		{
			AssertNull(nameof(ITransaction.ReversalStatusCodeList), (new DepositBatchParent(Factory) as ITransaction).ReversalStatusCodeList);
		}

		#endregion ReversalStatusCode

		#endregion

		public void TestCodeAndDescriptionProperties()
		{
			var testReceipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();

			var testDepositBatchParent = new DepositBatchParent(Factory);
			AssertEquals("Load batch lines", 1, testDepositBatchParent.DepositBatchLines.Count);
			Factory.Save();

			AssertNotNullOrEmpty(testReceipt.AH_TransactionNum);

			AssertEquals(testReceipt.AH_TransactionNum, CodePropertyAttribute.CodeFromBusinessObject(testDepositBatchParent));
			AssertEquals("Deposit Batch", DescriptionPropertyAttribute.DescriptionFromBusinessObject(testDepositBatchParent));
			AssertEquals("Recent Item Link caption", $"{testReceipt.AH_TransactionNum} - Deposit Batch", testDepositBatchParent.HumanReadableShortcutName);
		}

		public void TestReversingValidation()
		{
			ARReceipt testReceipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();

			DepositBatchParent testDepositBatchParent = new DepositBatchParent(Factory);
			AssertEquals(false, testDepositBatchParent.IsClearedInCashbook);
			Factory.Save();

			DepositBatch batch = Factory.Load<DepositBatch>(testDepositBatchParent.DepositBatchLines[0].PK);

			testDepositBatchParent = new DepositBatchParent(Factory, batch);

			DepositBatchReversing rev = (DepositBatchReversing)new ReversingFactory().NewReversing(testDepositBatchParent);
			rev.Reverse();

			AssertNotNull(testDepositBatchParent.DepositBatchLines[0]);
			Assert(testDepositBatchParent.DepositBatchLines[0].IsReverseTransaction);
			Assert(testDepositBatchParent.DepositBatchLines[0].Validation is TransactionReversalValidation);
			Assert(testDepositBatchParent.DepositBatchLines[0].Transactions[0].Validation is TransactionReversalValidation);
		}

		public void TestIsClearedInCashbook()
		{
			ARReceipt testReceipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();

			DepositBatchParent testDepositBatchParent = new DepositBatchParent(Factory);
			AssertEquals(false, testDepositBatchParent.IsClearedInCashbook);
			Factory.Save();

			ZGuid firstBatchPK = testDepositBatchParent.DepositBatchLines[0].PK;
			DepositBatch testDepositBatch = Factory.Load<DepositBatch>(firstBatchPK);
			testDepositBatch.AH_DateClearedInCashbook = ZDateTime.Now;

			DepositBatchParent testSavedDepositBatchParent = new DepositBatchParent(Factory, testDepositBatch);
			AssertEquals(true, testSavedDepositBatchParent.IsClearedInCashbook);
		}

		[TestDate(1999, 7, 2)]
		public void TestIsPeriodSubLedgerClosed()
		{
			PeriodManager testManager = TestObjectCreator.CreateTestPeriods(new ZDateTime(1999, 6, 1));
			Factory.Save();
			ARReceipt testReceipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();

			DepositBatchParent testDepositBatchParent = new DepositBatchParent(Factory);
			Factory.Save();

			ZGuid firstBatchPK = testDepositBatchParent.DepositBatchLines[0].PK;
			DepositBatch testDepositBatch = Factory.Load<DepositBatch>(firstBatchPK);
			testDepositBatch.AH_PostDate = new ZDateTime(1999, 6, 1);
			DepositBatchParent testSavedDepositBatchParent = new DepositBatchParent(Factory, testDepositBatch);

			AssertEquals(false, testSavedDepositBatchParent.IsPeriodSubLedgerClosed);

			testManager.CloseSubLedgerPeriod();
			AssertEquals(true, testSavedDepositBatchParent.IsPeriodSubLedgerClosed);
		}

		public void TestReadOnlyFields()
		{
			DepositBatchParent testDepositBatchParent = new DepositBatchParent(Factory);

			AssertEquals(false, testDepositBatchParent.DepositDateInfo.ReadOnly);
			AssertEquals(true, testDepositBatchParent.BatchNumberInfo.ReadOnly);

			bool originalRegistryValue = AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.Value;
			bool originalSecurityValue = Env.Security.CashBookPostToPreviousOrFutureOpenPeriod.IsAllowed;
			try
			{
				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				Env.Security.CashBookPostToPreviousOrFutureOpenPeriod.IsAllowed = true;

				AssertEquals(false, testDepositBatchParent.DepositDateInfo.ReadOnly);
				AssertEquals(false, testDepositBatchParent.DepositPostDateInfo.ReadOnly);
				AssertEquals(true, testDepositBatchParent.BatchNumberInfo.ReadOnly);

				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				Env.Security.CashBookPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
				AssertEquals(false, testDepositBatchParent.DepositPostDateInfo.ReadOnly);

				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				Env.Security.CashBookPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
				AssertEquals(false, testDepositBatchParent.DepositPostDateInfo.ReadOnly);

				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				Env.Security.CashBookPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
				AssertEquals(false, testDepositBatchParent.DepositPostDateInfo.ReadOnly);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalRegistryValue);
				Env.Security.CashBookPostToPreviousOrFutureOpenPeriod.IsAllowed = originalSecurityValue;
			}

			DepositBatch depositBatch = Factory.New<DepositBatch>();
			DepositBatchParent depositBatchParent = new DepositBatchParent(Factory, depositBatch);

			AssertEquals(true, depositBatchParent.DepositDateInfo.ReadOnly);
			AssertEquals(true, depositBatchParent.DepositPostDateInfo.ReadOnly);
		}

		public void TestDefaultFilter()
		{
			DepositBatchParent testDepositBatchParent = new DepositBatchParent(Factory);
			AssertEquals(true, testDepositBatchParent.FilterByBranch);
			AssertEquals(true, testDepositBatchParent.FilterByAllCurrencies);
		}

		public void TestDefaultBranchFilterPK()
		{
			DepositBatchParent testDepositBatchParent = new DepositBatchParent(Factory);
			AssertEquals(GlbBranch.CurrentBranch.PK, testDepositBatchParent.BranchFilterPK);
		}

		public void TestIsInDatabase()
		{
			ARReceipt testReceipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();

			DepositBatchParent testDepositBatchParent = new DepositBatchParent(Factory);
			AssertEquals(1, testDepositBatchParent.DepositBatchLines.Count);
			AssertEquals(false, testDepositBatchParent.IsInDatabase);
			Factory.Save();

			ZGuid firstBatchPK = testDepositBatchParent.DepositBatchLines[0].PK;
			DepositBatch testDepositBatch = Factory.Load<DepositBatch>(firstBatchPK);

			DepositBatchParent testSavedDepositBatchParent = new DepositBatchParent(Factory, testDepositBatch);
			AssertEquals(true, testSavedDepositBatchParent.IsInDatabase);
		}

		public void TestHasDirectCreditReceipt()
		{
			ARReceipt testReceipt = TestObjectCreator.CreateARReceipt(ReceiptTypes.DirectCredit, TransactionTypes.Receipt, LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();

			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.ReceiptBatch);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, testReceipt.AH_ReceiptBatchNo);

			DepositBatch testBatch = Factory.LoadTop1<DepositBatch>(filter);
			DepositBatchParent testDepositBatchParent = new DepositBatchParent(Factory, testBatch);
			AssertEquals(1, testDepositBatchParent.DepositBatchLines.Count);
			AssertEquals(true, testDepositBatchParent.HasDirectCreditReceipt);

			testReceipt.AH_ReceiptType = ReceiptTypes.Cash;
			Factory.Save();

			testBatch = Factory.LoadTop1<DepositBatch>(filter);
			testDepositBatchParent = new DepositBatchParent(Factory, testBatch);
			AssertEquals(1, testDepositBatchParent.DepositBatchLines.Count);
			AssertEquals(false, testDepositBatchParent.HasDirectCreditReceipt);

			testReceipt.AH_ReceiptType = ReceiptTypes.eNettDirectCredit;
			Factory.Save();

			testBatch = Factory.LoadTop1<DepositBatch>(filter);
			testDepositBatchParent = new DepositBatchParent(Factory, testBatch);
			AssertEquals(1, testDepositBatchParent.DepositBatchLines.Count);
			AssertEquals(false, testDepositBatchParent.HasDirectCreditReceipt);
		}

		public void TestHasCancelledReceipt()
		{
			ARReceipt testReceipt = TestObjectCreator.CreateARReceipt(ReceiptTypes.Cash, TransactionTypes.Receipt, LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();

			DepositBatchParent testDepositBatchParent = new DepositBatchParent(Factory);
			DepositBatch testBatch = testDepositBatchParent.DepositBatchLines[0];
			AssertEquals(1, testDepositBatchParent.DepositBatchLines.Count);
			AssertEquals(false, testDepositBatchParent.HasCancelledReceipt);
			Factory.Save();

			testReceipt.AH_IsCancelled = true;
			((IMatching)testReceipt).CurrentMatchGroup.AddNew().AP_AH = testReceipt.PK;
			TestObjectCreator.SetupMatchLinkMatchDate(testReceipt);
			Factory.Save();

			testDepositBatchParent = new DepositBatchParent(Factory, testBatch);
			AssertEquals(1, testDepositBatchParent.DepositBatchLines.Count);
			AssertEquals(true, testDepositBatchParent.HasCancelledReceipt);
		}

		[TestDate(2000, 1, 10)]
		public void TestDepositDate()
		{
			DepositBatchParent testDepositBatchParent = new DepositBatchParent(Factory);
			AssertEquals(new ZDateTime(2000, 1, 10), testDepositBatchParent.DepositDate);

			ARReceipt testReceipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();
			DepositBatchParent testDepositBatchParent2 = new DepositBatchParent(Factory);
			AssertEquals(1, testDepositBatchParent2.DepositBatchLines.Count);
			DepositBatch testBatch = testDepositBatchParent2.DepositBatchLines[0];
			Factory.Save();

			testBatch.AH_InvoiceDate = new ZDateTime(2000, 1, 3);
			DepositBatchParent loadedBatchParent = new DepositBatchParent(Factory, testBatch);
			AssertEquals(new ZDateTime(2000, 1, 3), loadedBatchParent.DepositDate);
			Factory.Save();

			ARReceipt testReceipt3 = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();
			DepositBatchParent testDepositBatchParent3 = new DepositBatchParent(Factory);
			AssertEquals(1, testDepositBatchParent3.DepositBatchLines.Count);
			DepositBatch testBatch3 = testDepositBatchParent3.DepositBatchLines[0];
			testDepositBatchParent3.DepositDate = new ZDateTime(2000, 1, 20);
			Factory.Save();
			AssertEquals(testBatch3.AH_InvoiceDate, testDepositBatchParent3.DepositDate);
		}

		[TestDate(2000, 1, 10)]
		public void TestDepositPostDate()
		{
			DepositBatchParent testDepositBatchParent = new DepositBatchParent(Factory);
			AssertEquals(new ZDateTime(2000, 1, 10), testDepositBatchParent.DepositPostDate);

			ARReceipt testReceipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();

			DepositBatchParent testDepositBatchParent2 = new DepositBatchParent(Factory);
			AssertEquals(1, testDepositBatchParent2.DepositBatchLines.Count);
			DepositBatch testBatch = testDepositBatchParent2.DepositBatchLines[0];
			testBatch.AH_PostDate = new ZDateTime(2000, 1, 3);
			Factory.Save();

			DepositBatchParent loadedBatchParent = new DepositBatchParent(Factory, testBatch);
			AssertEquals(new ZDateTime(2000, 1, 3), loadedBatchParent.DepositPostDate);
			Factory.Save();

			ARReceipt testReceipt3 = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();
			DepositBatchParent testDepositBatchParent3 = new DepositBatchParent(Factory);
			AssertEquals(1, testDepositBatchParent3.DepositBatchLines.Count);
			DepositBatch testBatch3 = testDepositBatchParent3.DepositBatchLines[0];
			testDepositBatchParent3.DepositPostDate = new ZDateTime(2000, 1, 20);
			Factory.Save();
			AssertEquals(testBatch3.AH_PostDate, testDepositBatchParent3.DepositPostDate);
		}

		public void TestBranchFilterPKReadOnly()
		{
			DepositBatchParent testDepositBatchParent = new DepositBatchParent(Factory);
			testDepositBatchParent.FilterByCompany = true;
			AssertEquals(true, testDepositBatchParent.BranchFilterPKInfo.ReadOnly);

			testDepositBatchParent.FilterByBranch = true;
			AssertEquals(false, testDepositBatchParent.BranchFilterPKInfo.ReadOnly);
		}

		public void TestTotalNoOfTransactionsSelected()
		{
			ARReceipt testReceipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			ARReceipt testReceipt2 = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 300M, TestObjectCreator.AUDBankAccount2.PK);
			Factory.Save();

			DepositBatchParent testDepositBatchParent = new DepositBatchParent(Factory);
			AssertEquals(2, testDepositBatchParent.DepositBatchLines.Count);
			AssertEquals(2, testDepositBatchParent.TotalNoOfTransactionsSelected);

			testDepositBatchParent.DepositBatchLines[0].IsSelected = false;
			AssertEquals(1, testDepositBatchParent.TotalNoOfTransactionsSelected);

			testDepositBatchParent.SelectAllTransactions(false);
			AssertEquals("Should unselected all transactoins", 0, testDepositBatchParent.TotalNoOfTransactionsSelected);

			testDepositBatchParent.SelectAllTransactions(true);
			AssertEquals("Should selected all transactions", 2, testDepositBatchParent.TotalNoOfTransactionsSelected);
		}

		public void TestSelectAllTransactionsForBankAccount()
		{
			ARReceipt testReceipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);

			ARReceipt testReceipt2 = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 300M, TestObjectCreator.AUDBankAccount2.PK);
			ARReceipt testReceipt3 = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 200M, TestObjectCreator.AUDBankAccount2.PK);

			Factory.Save();

			DepositBatchParent testDepositBatchParent = new DepositBatchParent(Factory);

			testDepositBatchParent.SelectAllTransactionsForBankAccount(TestObjectCreator.AUDBankAccount.AB_Code, false);
			AssertEquals(2, testDepositBatchParent.TotalNoOfTransactionsSelected);

			testDepositBatchParent.SelectAllTransactionsForBankAccount(TestObjectCreator.AUDBankAccount.AB_Code, true);
			AssertEquals(3, testDepositBatchParent.TotalNoOfTransactionsSelected);
		}

		public void TestReadOnlyAfterSaving()
		{
			ARReceipt testReceipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			ARReceipt testReceipt2 = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 200M, TestObjectCreator.USDBankAccount.PK);
			Factory.Save();
			DepositBatchParent testDepositBatchParent = new DepositBatchParent(Factory);

			AssertReadOnlyness(testDepositBatchParent, false);
			AssertEquals(2, testDepositBatchParent.DepositBatchLines.Count);
			testDepositBatchParent.DepositBatchLines[1].IsSelected = false;
			Factory.Save();

			AssertReadOnlyness(testDepositBatchParent, true);

			AssertAtLeastOneSelected(testDepositBatchParent);

			testDepositBatchParent.SelectAllTransactionsForBankAccount(testDepositBatchParent.DepositBatchLines[0].BankCode, false);
			testDepositBatchParent.SelectAllTransactions(false);
			// Should not have any effect on ReadOnly BatchParent
			AssertAtLeastOneSelected(testDepositBatchParent);
		}

		void AssertReadOnlyness(DepositBatchParent testDepositBatchParent, bool isReadOnly)
		{
			AssertEquals("IsExistingBatch", isReadOnly, testDepositBatchParent.IsExistingBatch);
			AssertEquals("FilterByBranch.ReadOnly", isReadOnly, testDepositBatchParent.FilterByBranchInfo.ReadOnly);
			AssertEquals("FilterByCompany.ReadOnly", isReadOnly, testDepositBatchParent.FilterByCompanyInfo.ReadOnly);
			AssertEquals("FilterByForeignCurrency.ReadOnly", isReadOnly, testDepositBatchParent.FilterByForeignCurrencyInfo.ReadOnly);
			AssertEquals("FilterByLocalCurrency.ReadOnly", isReadOnly, testDepositBatchParent.FilterByLocalCurrencyInfo.ReadOnly);
			AssertEquals("BranchFilterPK.ReadOnly", isReadOnly, testDepositBatchParent.BranchFilterPKInfo.ReadOnly);
		}

		void AssertAtLeastOneSelected(DepositBatchParent testDepositBatchParent)
		{
			Assert("At least one should be selected", (from DepositBatch batch in testDepositBatchParent.DepositBatchLines where batch.IsSelected select batch).First() != null);
		}

		public void TestSavedData()
		{
			ARReceipt testReceipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			ARReceipt testReceipt2 = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cheque, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 500M, TestObjectCreator.AUDBankAccount.PK);
			ARReceipt testReceipt3 = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 300M, TestObjectCreator.AUDBankAccount2.PK);
			ARReceipt testReceipt4 = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cheque, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 200M, TestObjectCreator.AUDBankAccount2.PK);
			Factory.Save();

			DepositBatchParent testDepositBatchParent = new DepositBatchParent(Factory);
			AssertEquals(2, testDepositBatchParent.DepositBatchLines.Count);
			Factory.Save();

			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.ReceiptBatch);
			filter.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_AB, SQLComparisonOperator.Equal, TestObjectCreator.AUDBankAccount.PK);
			DepositBatch testFirstDepositBatch = Factory.LoadTop1<DepositBatch>(filter);

			AssertNotNull(testFirstDepositBatch);
			AssertEquals(ZArchitecture.Core.LedgerTypes.CashBook, testFirstDepositBatch.AH_Ledger);
			AssertEquals(ZArchitecture.Core.TransactionTypes.ReceiptBatch, testFirstDepositBatch.AH_TransactionType);
			AssertEquals((ZByte)1, testFirstDepositBatch.AH_TransactionCount);
			AssertEquals(ZString.Empty, testFirstDepositBatch.AH_TransactionReference);
			AssertEquals(ZString.Empty, testFirstDepositBatch.AH_Desc);
			AssertEquals(ZDateTime.Today.ToShortDateString(), testFirstDepositBatch.AH_InvoiceDate.ToShortDateString());
			AssertEquals(ZString.Empty, testFirstDepositBatch.AH_TransactionCategory);
			AssertEquals(ZDateTime.Today.ToShortDateString(), testFirstDepositBatch.AH_DueDate.ToShortDateString());
			AssertEquals(600M, testFirstDepositBatch.AH_InvoiceAmount);
			AssertEquals(0M, testFirstDepositBatch.AH_GSTAmount);
			AssertEquals(0M, testFirstDepositBatch.AH_WithholdingTax);
			AssertEquals(600M, testFirstDepositBatch.AH_OSTotal);
			AssertEquals(TestObjectCreator.AUDBankAccount.AB_RX_NKAccountCurrency, testFirstDepositBatch.AH_RX_NKTransactionCurrency);
			AssertEquals(1M, testFirstDepositBatch.AH_ExchangeRate);
			AssertEquals(0, testFirstDepositBatch.AH_AgePeriod);
			AssertEquals(0, testFirstDepositBatch.AH_PostPeriod);
			AssertEquals(ZDateTime.Today.ToShortDateString(), testFirstDepositBatch.AH_PostDate.ToShortDateString());
			AssertEquals(false, testFirstDepositBatch.AH_IsDisbursementCalc);
			AssertEquals(ZString.Empty, testFirstDepositBatch.AH_ChequeOrReference);
			AssertEquals(ZString.Empty, testFirstDepositBatch.AH_ReceiptType);
			AssertEquals(false, testFirstDepositBatch.AH_CashBasisGSTIndicator);
			AssertEquals(false, testFirstDepositBatch.AH_CashBasisGSTRealisedToGL);
			AssertEquals("Deposit Batch", testFirstDepositBatch.AH_ChequeDrawer);
			AssertEquals(ZString.Empty, testFirstDepositBatch.AH_DrawerBranch);
			AssertEquals(false, testFirstDepositBatch.AH_InvoiceApproved);
			AssertEquals(ZString.Empty, testFirstDepositBatch.AH_ConsolidatedInvoiceRef);
			AssertEquals(ZDateTime.Empty, testFirstDepositBatch.AH_FullyPaidDate);
			AssertEquals(false, testFirstDepositBatch.AH_InvoicePrinted);
			AssertEquals(false, testFirstDepositBatch.AH_IsCancelled);
			AssertEquals(ZDateTime.Empty, testFirstDepositBatch.AH_DateClearedInCashbook);
			AssertEquals(false, testFirstDepositBatch.AH_NotAllocated);
			AssertEquals(0M, testFirstDepositBatch.AH_OutstandingAmount);
			AssertEquals(false, testFirstDepositBatch.AH_PostedToEFT);
			AssertEquals("N", testFirstDepositBatch.AH_PostToGL);
			AssertEquals(testFirstDepositBatch.AH_TransactionNum, testFirstDepositBatch.AH_ReceiptBatchNo);
			AssertEquals(GlbBranch.CurrentBranch.PK, testFirstDepositBatch.AH_GB);
			AssertEquals(ZGuid.Empty, testFirstDepositBatch.AH_AG);
			AssertEquals(ZGuid.Empty, testFirstDepositBatch.AH_TransactionBelongsToGroup);
			AssertEquals(ZGuid.Empty, testFirstDepositBatch.AH_AH_InvoiceStatement);
		}

		[ExpectNoExceptions]
		public void TestReversingReasonExceedsMaxLength()
		{
			DepositBatchParent testDepositBatchParent = new DepositBatchParent(Factory, Factory.New<DepositBatch>());

			string reverseReason = new string('d', AccTransactionHeaderSchema.AH_Desc.MaxLength + 1);
			testDepositBatchParent.ReversingReason = reverseReason;
		}

		[TestDate(2010, 06, 15)]
		public void TestValidateDepositPostDate()
		{
			bool originalRegistryValue = AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.Value;
			bool originalSecurityValue = Env.Security.CashBookPostToPreviousOrFutureOpenPeriod.IsAllowed;

			try
			{
				PeriodManager testManager = TestObjectCreator.CreateTestPeriods(new ZDateTime(2010, 6, 1));
				Factory.Save();

				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				Env.Security.CashBookPostToPreviousOrFutureOpenPeriod.IsAllowed = true;

				AccBankAccount bankAccount = TestObjectCreator.AUDBankAccount;

				ARReceipt receipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, bankAccount.PK);
				receipt.AH_PostDate = ZDate.Today.AddDays(-2);

				ARReceipt receipt2 = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, bankAccount.PK);
				receipt2.AH_PostDate = ZDate.Today.AddDays(-4);

				Factory.Save();

				DepositBatchParent depositBatchParent = new DepositBatchParent(Factory);

				AssertEquals("depositBatchParent.DepositBatchLines.Count", 1, depositBatchParent.DepositBatchLines.Count);
				AssertEquals("depositBatchParent.DepositBatchLines[0].Transactions.Count", 2, depositBatchParent.DepositBatchLines[0].Transactions.Count);

				//date in the past, amongst receipts
				depositBatchParent.DepositPostDate = ZDate.Today.AddDays(-3);
				depositBatchParent.ValidateDepositPostDate();
				AssertHasError(depositBatchParent.DepositPostDateInfo, "Deposit Post Date must be equal to or later than each transaction's Post Date.");

				//date in the past, after all receipts
				depositBatchParent.DepositPostDate = ZDate.Today.AddDays(-1);
				depositBatchParent.ValidateDepositPostDate();
				AssertNoError(depositBatchParent.DepositPostDateInfo, "Deposit Post Date must be equal to or later than each transaction's Post Date.");

				//date today, receipts today
				receipt.AH_PostDate = ZDate.Today;
				receipt2.AH_PostDate = ZDate.Today;
				using (ServiceContainerSuspenderHelper.FunctionalitySuspender<ServiceContainerSuspenderHelper.PostDateCriticalValidationSuspenderService>.GetSuspender(Factory))
				{
					Factory.Save();
				}

				ARReceipt receipt3 = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, bankAccount.PK);
				ARReceipt receipt4 = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, bankAccount.PK);
				ARReceipt receipt5 = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 300M, bankAccount.PK);
				receipt5.AH_PostDate = ZDate.Today.AddDays(1);

				Factory.Save();

				DepositBatchParent depositBatchParent2 = new DepositBatchParent(Factory);

				AssertEquals("depositBatchParent.DepositBatchLines.Count", 1, depositBatchParent2.DepositBatchLines.Count);
				AssertEquals("depositBatchParent.DepositBatchLines[0].Transactions.Count", 3, depositBatchParent2.DepositBatchLines[0].Transactions.Count);

				foreach (DepositBatchTransactionLine transaction in depositBatchParent2.DepositBatchLines[0].Transactions)
				{
					if (transaction.PK == receipt5.PK)
					{
						transaction.IsSelected = false;
					}
				}

				int count = 0;
				foreach (DepositBatchTransactionLine transaction in depositBatchParent2.DepositBatchLines[0].Transactions)
				{
					if (transaction.IsSelected)
					{
						count++;
					}
				}
				AssertEquals("Selected Transactions", 2, count);

				depositBatchParent2.DepositPostDate = ZDate.Today;
				depositBatchParent2.ValidateDepositPostDate();
				AssertNoError(depositBatchParent2.DepositPostDateInfo, "Deposit Post Date must be equal to or later than each transaction's Post Date.");
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalRegistryValue);
				Env.Security.CashBookPostToPreviousOrFutureOpenPeriod.IsAllowed = originalSecurityValue;
			}
		}

		public void TestCheckDepositPostDateNotInFuture()
		{
			new AccountingPeriodTestHelper(Factory).SetupPeriods();

			AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			DepositBatchParent testDepositBatchParent = new DepositBatchParent(Factory);
			testDepositBatchParent.DepositPostDate = ZDateTime.Now.AddDays(1);
			AssertHasError("DepositPostDate", testDepositBatchParent.DepositPostDateInfo, AccountingConstants.FuturePostingErrorMessages.RegistryIsNotEnabled);

			testDepositBatchParent.DepositPostDate = ZDateTime.Now;
			AssertNoErrors("DepositPostDate", testDepositBatchParent.DepositPostDateInfo);

			AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			TestObjectCreator.ResetSecurityCore();
			Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = false;

			testDepositBatchParent.DepositPostDate = ZDateTime.Now.AddDays(2);
			AssertHasError("DepositPostDate", testDepositBatchParent.DepositPostDateInfo, AccountingConstants.FuturePostingErrorMessages.UserHasNoSecurity);

			Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = true;
			testDepositBatchParent.DepositPostDate = ZDateTime.Now.AddDays(3);
			AssertNoErrors("DepositPostDate", testDepositBatchParent.DepositPostDateInfo);
		}

		public void TestTransactionDate()
		{
			ARReceipt testReceipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cheque, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 500M, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();

			DepositBatchParent testDepositBatchParent = new DepositBatchParent(Factory);
			DepositBatch testBatch = testDepositBatchParent.DepositBatchLines[0];
			AssertEquals(1, testDepositBatchParent.DepositBatchLines.Count);
			Factory.Save();

			testDepositBatchParent = new DepositBatchParent(Factory, testBatch);
			AssertEquals(1, testDepositBatchParent.DepositBatchLines.Count);
			Assert("IsExistingBatch", testDepositBatchParent.IsExistingBatch);

			ZDateTime transactionDate = ZDateTime.Now.AddDays(10);
			((ITransaction)testDepositBatchParent).TransactionDate = transactionDate;
			AssertEquals("Transaction Date", transactionDate, ((ITransaction)testDepositBatchParent).TransactionDate);
		}

		public void TestSetCancellationFlag()
		{
			var testReceipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();

			var testDepositBatchParent = new DepositBatchParent(Factory);
			AssertEquals(1, testDepositBatchParent.DepositBatchLines.Count);
			AssertEquals(false, testDepositBatchParent.IsInDatabase);
			Factory.Save();

			ZGuid firstBatchPK = testDepositBatchParent.DepositBatchLines[0].PK;
			var testDepositBatch = Factory.Load<DepositBatch>(firstBatchPK);
			var testSavedDepositBatchParent = new DepositBatchParent(Factory, testDepositBatch);

			testSavedDepositBatchParent.SetCancellationFlag(true);
			AssertEquals("TestDepositBatch.AH_IsCancelled", true, testDepositBatch.AH_IsCancelled);

			testSavedDepositBatchParent.SetCancellationFlag(false);
			AssertEquals("TestDepositBatch.AH_IsCancelled", false, testDepositBatch.AH_IsCancelled);
		}

		#region Implementations

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

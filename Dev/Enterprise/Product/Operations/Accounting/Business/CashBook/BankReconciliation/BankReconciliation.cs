using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.AccStatement;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.ExcelTemplates;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Statement = Enterprise.Accounting.Business.Base.AccStatement.Statement;

namespace Enterprise.Accounting.Business.CashBook
{
	public partial class BankReconciliation : NonPersistentBusinessObject, IObsoleteValidation
	{
		static string RECEIPT_BATCH
			=> Res.GetString("1805b5a3-212e-49cd-affb-48244e0d13fc", "Receipt Batch");

		static string OPENING_RECEIPT
			=> Res.GetString("52e001ed-5c09-4c32-bc1e-0939cb606e17", "Opening Receipt");

		static string PAYMENT
			=> Res.GetString("b79f2953-9e37-419a-9e23-f5120a53379e", "Payment");

		static string DIRECT_PAYMENT
			=> Res.GetString("06fc6ab3-f9b2-4864-b068-19f1fdb8e966", "Direct Payment");

		static string OPENING_PAYMENT
			=> Res.GetString("346074ca-5243-4699-bf02-3170665c9256", "Opening Payment");

		static string BANK_TRANSFER
			=> Res.GetString("b7100035-6a64-4120-b17c-a9d081aaeeb5", "Bank Transfer");

		static string DDR_BATCH
			=> Res.GetString("2332a432-aa29-4d26-87ef-4389794fb37e", "Direct Debit Batch");

		static string CashAccountErrorMessage
			=> Res.GetString("a0437ea8-b331-4299-9131-dc1a3e73f9c4", "The selected Bank Account is a Cash Account. Please select a suitable Bank Account for Bank Reconciliation");

		const string DEBIT = "DEBIT";
		const string CREDIT = "CREDIT";

		const string ALL = "ALL";

		const string POST_DATE = "PSD";
		const string DATE_IN_STATEMENT = "DSS";

		abstract class Schema
		{
			public const string UnclearedCashbookAmount = "UnclearedCashbookAmount";
			public const string UnclearedStatementAmount = "UnclearedStatementAmount";
			public const string AmendedBankStatementBalance = "AmendedBankStatementBalance";
			public const string BankCurrencyDecimals = "BankCurrencyDecimals";
		}

		public BankReconciliation(BusinessObjectFactory factory)
			: base(factory)
		{
			GetTransactionsFactory();
		}

		#region Custom Events

		public event EventHandler TransactionsPopulated;
		public event EventHandler BankAndDatesSet;
		public event EventHandler BankOrDatesGoingToChange;

		public class BankReconEventArgs : EventArgs
		{
			public BankReconEventArgs(string fieldName)
			{
				this.FieldName = fieldName;
			}

			public readonly string FieldName;
			public bool Result;
		}

		void RaiseTransactionsPopulated()
		{
			if (TransactionsPopulated != null)
			{
				TransactionsPopulated(this, EventArgs.Empty);
			}
		}

		void RaiseBankAndDatesSet()
		{
			if (BankAndDatesSet != null)
			{
				BankAndDatesSet(this, EventArgs.Empty);
			}
		}

		bool ContinueWithChangingBankOrDates(ZPropertyInfo info)
		{
			bool result = true;
			if (info.Value.IsValid && !info.Value.IsEmpty && !info.HasErrors() &&
				MergedTransactions.Count > 0 && MergedTransactions.HasChanges &&
				BankOrDatesGoingToChange != null && !PreventReloadMultiple_ReloadRecords_Call)
			{
				string fieldName = (info == BankAccountPKInfo) ? new ZString(Res.GetString("b9d5eca4-1ece-46a9-8387-c3264900d2a4", "Bank")) : info.HumanReadableName;
				BankReconEventArgs args = new BankReconEventArgs(fieldName);
				BankOrDatesGoingToChange(this, args);
				result = args.Result;
			}
			return result;
		}

		#endregion

		#region Collections

		#region Transactions
		// cashbook only
		BankReconTransCollection fTransactions;
		BankReconTransCollection Transactions
		{
			get
			{
				if (fTransactions == null)
				{
					fTransactions = new BankReconTransCollection(TransactionsFactory, bankAccountPK);

					if (AreKeyFieldsValid)
					{
						fTransactions.LoadWithMoreFiltering(TransactionFilter);

						BusinessObject[] depositBatches = fTransactions.Find(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.ReceiptBatch));
						foreach (BankReconTransaction transactionToFilter in depositBatches)
						{
							if (transactionToFilter.AH_IsCancelled ||
								AmountFilter != 0 && transactionToFilter.Debit != AmountFilter && transactionToFilter.Credit != AmountFilter)
							{
								fTransactions.Remove(transactionToFilter);
							}
						}
					}
				}
				return fTransactions;
			}
		}

		void ResetTransactions()
		{
			fTransactions?.RemoveAll();
			fTransactions = null;
			ResetCombinedTransactions();
		}

		#endregion

		#region CombinedTransactions
		// cashbook only
		BankReconTransCollection CombinedTransactions_innerValue;
		public BankReconTransCollection CombinedTransactions
		{
			get
			{
				if (CombinedTransactions_innerValue == null)
				{
					CombinedTransactions_innerValue = new BankReconTransCollection(TransactionsFactory, ZGuid.Empty);
					CombinedTransactions_innerValue.AddRange(Transactions);
					CombinedTransactions_innerValue.AddRange(AdditionalTransactions.BankReconTransactions);
					RegisterEditableChildObject(CombinedTransactions_innerValue);
				}
				return CombinedTransactions_innerValue;
			}
		}

		void ResetCombinedTransactions()
		{
			if (CombinedTransactions_innerValue != null)
			{
				UnRegisterEditableChildObject(CombinedTransactions_innerValue);
				CombinedTransactions_innerValue?.RemoveAll();
				CombinedTransactions_innerValue = null;
			}
			fMergedTransactions?.RemoveAll();
			fMergedTransactions = null;
		}

		#endregion

		#region Statements

		StatementCollection fStatements;

		public StatementCollection GetStatements()
		{
			if (fStatements == null && BankAccount != null)
			{
				fStatements = new StatementCollection(BankAccount, TransactionsFactory);
				if (AreKeyFieldsValid)
				{
					fStatements.LoadWithMoreFiltering(StatementFilter);
				}
			}
			return fStatements;
		}

		void ResetStatements()
		{
			if (fStatements != null)
			{
				fStatements?.LoadWithMoreFiltering(new ZQuery { IsNoResultQuery = true }); // can't use RemoveAll as it remove relationship in each element to not belong to this collection anymore
				fStatements = null;
			}
			fMergedTransactions?.RemoveAll();
			fMergedTransactions = null;
		}

		#endregion

		#region MergedTransactions

		//bank statements and cashbook
		MergedTransactionCollection fMergedTransactions;
		public MergedTransactionCollection MergedTransactions
		{
			get
			{
				if (fMergedTransactions == null)
				{
					fMergedTransactions = new MergedTransactionCollection(TransactionsFactory, this);
					fMergedTransactions.AddRange(CombinedTransactions);
					if (GetStatements() != null)
					{
						fMergedTransactions.AddRange(GetStatements());
					}

					if (AreKeyFieldsValid && !SuspendCashbookRecalculation)
					{
						CashBookBalanceFromDB = GetCashBookAmount(bankAccountPK, ReconcileDate);
					}
				}

				return fMergedTransactions;
			}
		}

#if DEBUG
		internal void ResetMergedTransactions_ForTestOnly()
		{
			// Required to force load of MergedTransactions after ResetCurrentSessionSnapshot(), as the snapshot triggers the lazy loading.
			fTransactions = null;
			CombinedTransactions_innerValue = null;
			fAdditionalTransactions = null;
			fStatements = null;
			fMergedTransactions = null;
		}
#endif

		public IReadOnlyCollection<BankReconTransactionSnapshot> MergedTransactionsSnapshot()
			=> new List<BankReconTransactionSnapshot>(MergedTransactions.Cast<IBankReconMergedTransaction>().Select(t => new BankReconTransactionSnapshot(t)));

		#endregion

		#region Checked and Unchecked Tracking

		readonly HashSet<ZGuid> transactionIdsClearedInCurrentSession = new HashSet<ZGuid>();
		public IReadOnlyCollection<ZGuid> TransactionIdsClearedInCurrentSession => transactionIdsClearedInCurrentSession;

		readonly HashSet<ZGuid> transactionIdsUnclearedInCurrentSession = new HashSet<ZGuid>();
		public IReadOnlyCollection<ZGuid> TransactionIdsUnclearedInCurrentSession => transactionIdsUnclearedInCurrentSession;

		bool TransactionIdUsedInCurrentSession(ZGuid transactionId)
			=> transactionIdsClearedInCurrentSession.Contains(transactionId)
			|| transactionIdsUnclearedInCurrentSession.Contains(transactionId);

		public void SetTransactionIdCleared(ZGuid transactionId)
		{
			var alreadyUsed = TransactionIdUsedInCurrentSession(transactionId);
			if (!alreadyUsed)   // So repeatedly checking & unchecking the same transaction behaves correctly.
			{
				transactionIdsClearedInCurrentSession.Add(transactionId);
			}
			transactionIdsUnclearedInCurrentSession.Remove(transactionId);
		}

		public void SetTransactionIdUncleared(ZGuid transactionId)
		{
			var alreadyUsed = TransactionIdUsedInCurrentSession(transactionId);
			transactionIdsClearedInCurrentSession.Remove(transactionId);
			if (!alreadyUsed)   // So repeatedly checking & unchecking the same transaction behaves correctly.
			{
				transactionIdsUnclearedInCurrentSession.Add(transactionId);
			}
		}

		public IReadOnlyCollection<IBankReconMergedTransaction> TransactionsClearedInCurrentSession()
			=> MergedTransactions.Cast<IBankReconMergedTransaction>()
				.Where(t => TransactionIdsClearedInCurrentSession.Contains(t.Identifier))
				.OrderBy(t => t.TransactionDate)
				.ToArray();

		public IReadOnlyCollection<IBankReconMergedTransaction> TransactionsUnclearedInCurrentSession()
			=> MergedTransactions.Cast<IBankReconMergedTransaction>()
				.Where(t => TransactionIdsUnclearedInCurrentSession.Contains(t.Identifier))
				.OrderBy(t => t.TransactionDate)
				.ToArray();

		#endregion

		#region Transaction Change Tracking

		readonly Dictionary<ZGuid, BankReconTransactionSnapshot> transactionsAddedInThisSession = new Dictionary<ZGuid, BankReconTransactionSnapshot>();
		public IReadOnlyDictionary<ZGuid, BankReconTransactionSnapshot> TransactionsAddedInThisSession => transactionsAddedInThisSession;

		readonly Dictionary<ZGuid, BankReconTransactionSnapshot> transactionsRemovedInThisSession = new Dictionary<ZGuid, BankReconTransactionSnapshot>();
		public IReadOnlyDictionary<ZGuid, BankReconTransactionSnapshot> TransactionsRemovedInThisSession => transactionsRemovedInThisSession;

		public IReadOnlyCollection<BankReconTransactionSnapshot> TransactionsAddedInCurrentSession()
			=> TransactionsAddedInThisSession.Values.OrderBy(t => t.TransactionDate).ToArray();

		public IReadOnlyCollection<BankReconTransactionSnapshot> TransactionsRemovedInCurrentSession()
			=> TransactionsRemovedInThisSession.Values.OrderBy(t => t.TransactionDate).ToArray();

		public void TrackTransactionChanges(IReadOnlyCollection<BankReconTransactionSnapshot> beforeTransactions, IReadOnlyCollection<BankReconTransactionSnapshot> afterTransactions)
		{
			var beforeAsDictionary = beforeTransactions.ToDictionary(x => x.PK, x => x);
			var afterAsDictionary = afterTransactions.ToDictionary(x => x.PK, x => x);

			var newTransactionIds = afterAsDictionary.Keys.Except(beforeAsDictionary.Keys);
			foreach (var id in newTransactionIds)
			{
				transactionsAddedInThisSession[id] = afterAsDictionary[id];
			}

			var removedTransactionIds = beforeAsDictionary.Keys.Except(afterAsDictionary.Keys);
			foreach (var id in removedTransactionIds)
			{
				transactionsRemovedInThisSession[id] = beforeAsDictionary[id];
			}

			var continuingTransactionIds = beforeAsDictionary.Keys.Intersect(afterAsDictionary.Keys);
			foreach (var id in continuingTransactionIds)
			{
				if (!beforeAsDictionary[id].EqualsWithAmount(afterAsDictionary[id])
					&& !transactionsAddedInThisSession.ContainsKey(id))
				{
					// Transaction previously saved, and now modified. Represented as a removal and addition.
					transactionsRemovedInThisSession[id] = beforeAsDictionary[id];
					transactionsAddedInThisSession[id] = afterAsDictionary[id];
				}
			}
		}

		#endregion

		#endregion

		#region Properties

		#region BankAccount

		public BankStatement BankAccount
		{
			get { return BankAccount_innerValue ?? (BankAccount_innerValue = TransactionsFactory.Load<BankStatement>(bankAccountPK)); }
		}
		BankStatement BankAccount_innerValue;

		#region BankCurrencyDecimals

		public ZInt BankCurrencyDecimals
		{
			get { return (BankAccount != null && BankAccount.AccountCurrency != null) ? BankAccount.AccountCurrency.Decimals : 2; }
		}

		public ZPropertyInfo BankCurrencyDecimalsInfo
		{
			get { return GetZPropertyInfo(nameof(BankCurrencyDecimals)); }
		}

		public int BankCurrencyDecimalsAsInt => BankCurrencyDecimals;

		#endregion

		#endregion

		#region BankAccountPK

		[List("BankAccountList")]
		public ZGuid BankAccountPK
		{
			get { return bankAccountPK; }
			set
			{
				if (bankAccountPK != value)
				{
					if (ContinueWithChangingBankOrDates(BankAccountPKInfo))
					{
						SetNonPersistentPropertyValue(BankAccountPKInfo, ref bankAccountPK, value);
						BankAccount_innerValue = null;

						PreventReloadMultiple_ReloadRecords_Call = true;
						try
						{
							if (BankAccount != null)
							{
								ReconcileDate = BankAccount.AB_LastReconcileDate;
								StatementDate = BankAccount.AB_LastStatementDate;
							}
							else
							{
								ReconcileDate = ZDateTime.Empty;
								StatementDate = ZDateTime.Empty;
							}
						}
						finally
						{
							PreventReloadMultiple_ReloadRecords_Call = false;
						}

						if (!IsValidationSuspended)
						{
							ValidateBankAccount();
						}
						ResetFactoryAndReloadRecords();
					}
				}
			}
		}

		public ZPropertyInfo BankAccountPKInfo
		{
			get { return GetZPropertyInfo(nameof(BankAccountPK)); }
		}
		ZGuid bankAccountPK;

		#endregion

		#region ReconcileDate

		public ZDateTime ReconcileDate
		{
			get { return ReconcileDate_innerValue; }
			set
			{
				if (ReconcileDate_innerValue != value)
				{
					if (ContinueWithChangingBankOrDates(ReconcileDateInfo))
					{
						SetNonPersistentPropertyValue(ReconcileDateInfo, ref ReconcileDate_innerValue, value);
						if (!IsValidationSuspended)
						{
							ValidateReconcileDate();
						}
						ResetFactoryAndReloadRecords();
					}
				}
			}
		}

		public ZPropertyInfo ReconcileDateInfo
		{
			get { return GetZPropertyInfo(nameof(ReconcileDate)); }
		}

		ZDateTime ReconcileDate_innerValue;

		#endregion

		#region StatementDate

		public ZDateTime StatementDate
		{
			get { return StatementDate_innerValue; }
			set
			{
				if (StatementDate_innerValue != value)
				{
					if (ContinueWithChangingBankOrDates(StatementDateInfo))
					{
						SetNonPersistentPropertyValue(StatementDateInfo, ref StatementDate_innerValue, value);
						if (!IsValidationSuspended)
						{
							ValidateStatementDate();
						}
						IsChangingStatementDate = true;
						try
						{
							ResetFactoryAndReloadRecords();
						}
						finally
						{
							IsChangingStatementDate = false;
						}
					}
				}
			}
		}

		public ZPropertyInfo StatementDateInfo
		{
			get { return GetZPropertyInfo(nameof(StatementDate)); }
		}

		ZDateTime StatementDate_innerValue;
		bool IsChangingStatementDate;

		#endregion

		#region CashBookBalance

		[DecimalPlaces(nameof(BankCurrencyDecimalsAsInt))]
		public ZDecimal CashBookBalance
		{
			get
			{
				ZDecimal additionalTransactionsCashBookBalance = 0M;

				foreach (BankReconTransaction transaction in AdditionalTransactions.BankReconTransactions)
				{
					additionalTransactionsCashBookBalance += transaction.Debit;
					additionalTransactionsCashBookBalance -= transaction.Credit;
				}

				return CashBookBalanceFromDB + additionalTransactionsCashBookBalance;
			}
		}

		public ZPropertyInfo CashBookBalanceInfo
		{
			get { return GetZPropertyInfo(nameof(CashBookBalance)); }
		}

		ZDecimal CashBookBalanceFromDB;

		#endregion

		#region Difference

		public ZDecimal Difference
		{
			get { return CashBookBalance - ClosingBalance; }
		}

		public ZPropertyInfo DifferenceInfo
		{
			get { return GetZPropertyInfo(nameof(Difference)); }
		}

		#endregion

		#region Closing Balance

		[DecimalPlaces(nameof(BankCurrencyDecimalsAsInt))]
		public ZDecimal ClosingBalance
		{
			get
			{
				if (BankAccount != null)
				{
					closingBalance = BankAccount.AB_StatementBalance;
				}
				return closingBalance;
			}
			set
			{
				if (BankAccount != null)
				{
					if (BankAccount.AB_StatementBalance != value)
					{
						BankAccount.AB_StatementBalance = value;
						HasChanges = true;
						ClosingBalanceInfo.RefreshBinding(closingBalance);
					}
				}
				else
				{
					SetNonPersistentPropertyValue(ClosingBalanceInfo, ref closingBalance, value);
				}
			}
		}

		public ZPropertyInfo ClosingBalanceInfo
		{
			get { return GetZPropertyInfo(nameof(ClosingBalance)); }
		}

		ZDecimal closingBalance;

		[DecimalPlaces(nameof(BankCurrencyDecimalsAsInt))]
		public ZDecimal ClosingBalanceReadOnly
		{
			get { return ClosingBalance; }
		}

		public ZPropertyInfo ClosingBalanceReadOnlyInfo
		{
			get { return GetZPropertyInfo(nameof(ClosingBalanceReadOnly)); }
		}

		#endregion

		#region AmendedBankStatementBalance

		[DecimalPlaces(nameof(BankCurrencyDecimalsAsInt))]
		public ZDecimal AmendedBankStatementBalance
		{
			get
			{
				return ClosingBalance + UnclearedStatementAmount + UnclearedCashbookAmount;
			}
		}

		public ZPropertyInfo AmendedBankStatementBalanceInfo
		{
			get { return GetZPropertyInfo(Schema.AmendedBankStatementBalance); }
		}

		#endregion

		#region UnclearedStatementAmount

		[DecimalPlaces(nameof(BankCurrencyDecimalsAsInt))]
		public ZDecimal UnclearedStatementAmount
		{
			get
			{
				ZDecimal result = 0m;
				if (AreKeyFieldsValid)
				{
					result = UnclearedStatementAmountFromDB;
					foreach (Statement statementItem in GetStatements())
					{
						if (statementItem.IsInDatabase)
						{
							if (statementItem.IsCleared && !new ZBool(statementItem.IsClearedInfo.OriginalValue))
							{
								result += statementItem.Debit;
								result -= statementItem.Credit;
							}
							else if (!statementItem.IsCleared && new ZBool(statementItem.IsClearedInfo.OriginalValue))
							{
								result -= statementItem.Debit;
								result += statementItem.Credit;
							}
						}
						else if (!statementItem.IsCleared)
						{
							result -= statementItem.Debit;
							result += statementItem.Credit;
						}
					}
				}
				return result;
			}
		}

		public ZPropertyInfo UnclearedStatementAmountInfo
		{
			get { return GetZPropertyInfo(Schema.UnclearedStatementAmount); }
		}

		#endregion

		#region UnclearedAmount

		[DecimalPlaces(nameof(BankCurrencyDecimalsAsInt))]
		public ZDecimal UnclearedCashbookAmount
		{
			get
			{
				ZDecimal result = 0m;

				if (AreKeyFieldsValid)
				{
					result = this.UnclearedCashbookAmountFromDB;
					foreach (BankReconTransaction transaction in CombinedTransactions)
					{
						if (transaction.IsInDatabase)
						{
							if (!transaction.AH_DateClearedInCashbook.IsEmpty && transaction.AH_DateClearedInCashbookInfo.OriginalValue.IsEmpty)
							{
								result -= transaction.Debit;
								result += transaction.Credit;
							}
							else if (transaction.AH_DateClearedInCashbook.IsEmpty && !transaction.AH_DateClearedInCashbookInfo.OriginalValue.IsEmpty)
							{
								result += transaction.Debit;
								result -= transaction.Credit;
							}
						}
						else if (!transaction.IsCleared)
						{
							result += transaction.Debit;
							result -= transaction.Credit;
						}
					}
				}
				return result;
			}
		}

		public ZPropertyInfo UnclearedCashbookAmountInfo
		{
			get { return GetZPropertyInfo(Schema.UnclearedCashbookAmount); }
		}

		#endregion

		#region CashbookTotal

		[DecimalPlaces(nameof(BankCurrencyDecimalsAsInt))]
		public ZDecimal CashbookTotal
		{
			get
			{
				ZDecimal debitTotal = 0m;
				ZDecimal creditTotal = 0m;

				foreach (BankReconTransaction transaction in CombinedTransactions)
				{
					if (transaction.IsCleared)
					{
						debitTotal += transaction.Debit;
						creditTotal += transaction.Credit;
					}
				}

				return debitTotal - creditTotal;
			}
		}

		public ZPropertyInfo CashbookTotalInfo
		{
			get { return GetZPropertyInfo(nameof(CashbookTotal)); }
		}

		#endregion

		#region Statement Total

		[DecimalPlaces(nameof(BankCurrencyDecimalsAsInt))]
		public ZDecimal StatementTotal
		{
			get
			{
				ZDecimal total = 0.0m;

				if (GetStatements() != null)
				{
					foreach (Statement statementToAdd in GetStatements())
					{
						if (statementToAdd.AS_IsCleared)
						{
							total += statementToAdd.AS_Amount * (statementToAdd.AS_DebitCredit == Statement.DEBIT ? -1.0m : 1.0m);
						}
					}
				}

				return total;
			}
		}

		public ZPropertyInfo StatementTotalInfo
		{
			get { return GetZPropertyInfo(nameof(StatementTotal)); }
		}

		#endregion

		#region Total Differences

		[DecimalPlaces(nameof(BankCurrencyDecimalsAsInt))]
		public ZDecimal TotalDifference
		{
			get { return CashbookTotal - StatementTotal; }
		}

		public ZPropertyInfo TotalDifferenceInfo
		{
			get { return GetZPropertyInfo(nameof(TotalDifference)); }
		}

		#endregion

		#region CurrentDebitTotal

		[DecimalPlaces(nameof(BankCurrencyDecimalsAsInt))]
		public ZDecimal CurrentDebitTotal
		{
			get { return MergedTransactions.CalculateDebitTotal(); }
		}

		public ZPropertyInfo CurrentDebitTotalInfo
		{
			get { return GetZPropertyInfo(nameof(CurrentDebitTotal)); }
		}

		#endregion

		#region CurrentCreditTotal

		[DecimalPlaces(nameof(BankCurrencyDecimalsAsInt))]
		public ZDecimal CurrentCreditTotal
		{
			get { return MergedTransactions.CalculateCreditTotal(); }
		}

		public ZPropertyInfo CurrentCreditTotalInfo
		{
			get { return GetZPropertyInfo(nameof(CurrentCreditTotal)); }
		}

		#endregion

		#region ReconError

		[DecimalPlaces(nameof(BankCurrencyDecimalsAsInt))]
		public ZDecimal ReconError
		{
			get { return AmendedBankStatementBalance - CashBookBalance; }
		}

		public ZPropertyInfo ReconErrorInfo
		{
			get { return GetZPropertyInfo(nameof(ReconError)); }
		}

		#endregion

		#region Current Session and OpeningSnapshot

		public BankReconciliationSnapshot OpeningSnapshot { get; set; }

		void ResetCurrentSession()
		{
			OpeningSnapshot = new BankReconciliationSnapshot(this);
			transactionIdsClearedInCurrentSession.Clear();
			transactionIdsUnclearedInCurrentSession.Clear();
			transactionsAddedInThisSession.Clear();
			transactionsRemovedInThisSession.Clear();
		}

		#endregion

		#region Validation

		public void ValidateReconcileDate()
		{
			ReconcileDateInfo.ClearAllNotifications();
			if (!ReconcileDate.IsValid)
			{
				ReconcileDateInfo.AddError(Res.GetString("58c1a918-0d02-44cc-a1a6-57b5b3e64f2a", "Please enter a valid date."));
			}
			else if (ReconcileDate.Date < LastReconcileDate.Date)
			{
				ReconcileDateInfo.AddError(Res.GetString("4f884c9d-364a-4569-bb4a-c631630c0dbe", "Reconcile date cannot be earlier than the last reconciliation date, which is {0}.", LastReconcileDate.ToShortDateString()));
			}
			else if (ReconcileDate.Date > ZDateTime.Today)
			{
				ReconcileDateInfo.AddError(Res.GetString("ec1fe448-7e9e-48b4-9af8-3504fbfc6aab", "Reconcile date cannot be later than today."));
			}
		}

		public void ValidateStatementDate()
		{
			StatementDateInfo.ClearAllNotifications();
			if (!StatementDate.IsValid)
			{
				StatementDateInfo.AddError(Res.GetString("58c1a918-0d02-44cc-a1a6-57b5b3e64f2a", "Please enter a valid date."));
			}
			else if (StatementDate.Date < LastStatementDate.Date)
			{
				StatementDateInfo.AddError(Res.GetString("d24dba05-23c7-4101-8d20-b25b082c535d", "Statement date cannot be earlier than the last statement date, which is {0}.", LastStatementDate.ToShortDateString()));
			}
			else if (StatementDate.Date > ZDateTime.Today)
			{
				StatementDateInfo.AddError(Res.GetString("c9ae9cd8-c4d4-4603-9e48-33bdd417e239", "Statement date cannot be later than today."));
			}
		}

		public void ValidateFromDateFilter()
		{
			FromDateFilterInfo.ClearAllNotifications();
			if (!fromDateFilter.IsValid && !fromDateFilter.IsEmpty)
			{
				FromDateFilterInfo.AddError(Res.GetString("a2e6a357-0ae0-4432-8e50-c02a64abb2c4", "Please enter valid Date From."));
			}
		}

		public void ValidateToDateFilter()
		{
			ToDateFilterInfo.ClearAllNotifications();
			if (!toDateFilter.IsValid && !toDateFilter.IsEmpty)
			{
				ToDateFilterInfo.AddError(Res.GetString("f9249001-e3be-4d5b-b9c3-b36df6f51bf0", "Please enter valid Date To."));
			}
		}

		public void ValidateTotalDifference()
		{
			TotalDifferenceInfo.ClearAllNotifications();
			if (TotalDifference != 0.0m)
			{
				TotalDifferenceInfo.AddWarning(Res.GetString("e1209448-c75d-40e5-963b-ebff4926711e", "Cashbook & Statement totals do not match."));
			}
		}

		public void ValidateBankAccount()
		{
			BankAccountPKInfo.ClearAllNotifications();
			if (BankAccount == null || BankAccount.AB_GC != GlbCompany.CurrentCompany.PK)
			{
				BankAccountPKInfo.AddError(Res.GetString("bd94017e-4190-47ba-92d7-83579cded298", "This bank account is not valid for the current company."));
			}
			else if (BankAccount.IsCashAccount) // this case doesn't happen in the ui, selection list already prevents cash account selection. This check is to prevent wrong doing by code.
			{
				BankAccountPKInfo.AddError(CashAccountErrorMessage);
			}
		}

		#endregion

		#endregion

		#region Filter

		public void ClearFilter()
		{
			TypeFilter = "";
			MethodFilter = "";
			TextSearchFilter = "";
			AmountFilter = 0.0m;
			FromDateFilter = ZDateTime.Empty;
			ToDateFilter = ZDateTime.Empty;
			DateFilterType = "";
		}

		ZQuery TransactionFilter
		{
			get
			{
				ZQuery finalFilter = new ZQuery(AccTransactionHeaderSchema.AH_PostDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, ReconcileDate);

				ZSqlParameterCollection parameters = new ZSqlParameterCollection();
				parameters.Add("@Bank", BankAccountPK.ToGuid(), AccTransactionHeaderSchema.AH_AB);
				parameters.Add("@Company", GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranchSchema.GB_GC);
				parameters.Add("@PostDate", ReconcileDate.Date.AddDays(1).ToDateTime(), AccTransactionHeaderSchema.AH_PostDate);
				ZDBOnlyQuery excludeLaterBatchedDDRQuery = new ZDBOnlyQuery(typeof(BankReconTransaction));
				excludeLaterBatchedDDRQuery.AddFilterAndZSQLParameterCollection(AccTransactionHeaderSchema.PK.Name + " NOT IN (SELECT PaymentHeaders." +
						AccTransactionHeaderSchema.PK.Name + " " + LateBatchedDDRAmountSQL_FROM_WHERE + ")", parameters);
				finalFilter.AddToFilter(excludeLaterBatchedDDRQuery);

				ApplyTypeFilter(finalFilter);
				ApplyMethodFilter(finalFilter);

				if (!ClearedFilter)
				{
					finalFilter.AddToFilter(AccTransactionHeaderSchema.AH_DateClearedInCashbook, ZDateTime.Empty);
				}

				ZQuery textFilter = GetTextFilterForTransaction();

				ZQuery extraDateFilter = GetDateFiltersForTransaction();

				ZQuery amountSearchFilter = GetLocalAmountFilter();

				finalFilter.AddToFilter(textFilter);
				finalFilter.AddToFilter(extraDateFilter);
				finalFilter.AddToFilter(amountSearchFilter);

				return finalFilter;
			}
		}

		void ApplyTypeFilter(ZQuery finalFilter)
		{
			if (TypeFilter != "" && TypeFilter != ALL)
			{
				if (TypeFilter == TransactionTypes.DDRBatch)
				{
					ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TypeFilter);

					ZQuery nonRolledUpPaymentFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Payment);
					nonRolledUpPaymentFilter.AddToFilter(AccTransactionHeaderSchema.AH_ReceiptType, ReceiptTypes.DirectDebit);
					nonRolledUpPaymentFilter.AddToFilter(AccTransactionHeaderSchema.AH_ReceiptBatchNo, SQLComparisonOperator.NotEqual, "");

					filter.AddToFilter(nonRolledUpPaymentFilter, JoinCondition.Or);

					finalFilter.AddToFilter(filter);
				}
				else
				{
					finalFilter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TypeFilter);
				}
			}
		}

		void ApplyMethodFilter(ZQuery finalFilter)
		{
			if (MethodFilter != "")
			{
				ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_ReceiptType, MethodFilter);
				if (MethodFilter == ReceiptTypes.DirectCredit)
				{
					ZQuery rCBFilter = new ZQuery();

					ZSqlParameterCollection parameters = new ZSqlParameterCollection();
					parameters.Add("@ReceiptType", MethodFilter, AccTransactionHeaderSchema.AH_ReceiptType);
					parameters.Add("@CurrentCompany", GlbCompany.CurrentCompany.PK, AccTransactionHeaderSchema.AH_GC);
					rCBFilter.AddFilterAndZSQLParameterCollection(AccTransactionHeaderSchema.PK.Name + @" IN (SELECT AH_PK from dbo.AccTransactionHeader
																	WHERE AH_ReceiptBatchNo IN
																	(SELECT AH_ReceiptBatchNo 
																	FROM dbo.AccTransactionheader 
																	WHERE AH_ReceiptBatchNo <> '' AND
																	AH_ReceiptType = @ReceiptType AND AH_GC = @CurrentCompany)
																	AND AH_TransactionType = 'RCB')", parameters);

					filter.AddToFilter(rCBFilter, JoinCondition.Or);
				}
				finalFilter.AddToFilter(filter);
			}
		}

		ZQuery GetAmountFilter(SchemaColumn column)
		{
			ZQuery filter = null;
			if (AmountFilter != 0)
			{
				filter = new ZQuery(column, this.AmountFilter);
				filter.AddToFilter(JoinCondition.Or, column, SQLComparisonOperator.Equal, -AmountFilter);
				filter.AddToFilter(JoinCondition.Or, column, SQLComparisonOperator.Equal, 0.0m);
			}

			return filter;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter related")]
		ZQuery GetLocalAmountFilter()
		{
			ZQuery filter = new ZQuery();
			if (AmountFilter != 0)
			{
				ZString filterStr = string.Format(CultureInfo.InvariantCulture, "{0} = @Amount OR {0} = -(@Amount) OR {0} = 0.0",
								AccTransactionHeader.AH_LocalTotalSQLFormula);
				ZSqlParameterCollection parameters = new ZSqlParameterCollection();
				parameters.Add(ZSqlParameter.New("@Amount", AmountFilter, AccTransactionHeaderSchema.AH_InvoiceAmount));

				filter.AddFilterAndZSQLParameterCollection(filterStr, parameters);

				filter.DefaultJoinCondition = JoinCondition.Or;
				filter.AddToFilter(GetAmountFilter(AccTransactionHeaderSchema.AH_OSTotal));
			}

			return filter;
		}

		ZQuery GetTextFilterForTransaction()
		{
			ZQuery textFilter = new ZQuery();

			if (TextSearchFilter != "")
			{
				if (TextSearchFilter.Length <= AccTransactionHeaderSchema.AH_ChequeOrReference.MaxLength)
				{
					textFilter.AddToFilter(AccTransactionHeaderSchema.AH_ChequeOrReference, SQLComparisonOperator.Contains, this.TextSearchFilter);
				}
				if (TextSearchFilter.Length <= AccTransactionHeaderSchema.AH_ReceiptBatchNo.MaxLength)
				{
					textFilter.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_ReceiptBatchNo, SQLComparisonOperator.Contains, this.TextSearchFilter);
				}
			}

			return textFilter;
		}

		ZQuery GetDateFilters(SchemaColumn column)
		{
			ZQuery dateFilter = new ZQuery();
			if (FromDateFilter.IsValidSqlDateTime)
			{
				dateFilter.AddToFilter(column, SQLComparisonOperator.GreaterThanOrEqualTo, FromDateFilter);
			}

			if (ToDateFilter.IsValidSqlDateTime)
			{
				dateFilter.AddToFilter(column, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, ToDateFilter);
			}

			return dateFilter;
		}

		ZQuery GetDateFiltersForTransaction()
		{
			if (DateFilterType == DATE_IN_STATEMENT)
			{
				return GetDateFilters(AccTransactionHeaderSchema.AH_DateClearedInCashbook);
			}
			else
			{
				return GetDateFilters(AccTransactionHeaderSchema.AH_PostDate);
			}
		}

		ZQuery GetDateFiltersForStatements()
		{
			return GetDateFilters(AccStatementSchema.AS_StatementDate);
		}

		ZQuery StatementFilter
		{
			get
			{
				ZQuery finalFilter = new ZQuery(AccStatementSchema.AS_StatementDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, StatementDate);

				ZQuery typeFilter = BankAccount != null ? BankAccount.ApplyTypeFilter() : null;
				if (typeFilter != null)
				{
					finalFilter.AddToFilter(typeFilter);
				}

				if (MethodFilter != "")
				{
					finalFilter.AddToFilter(AccStatementSchema.AS_Type, MethodFilter);
				}

				if (TextSearchFilter != "")
				{
					finalFilter.AddToFilter(AccStatementSchema.AS_ChequeOrReference, SQLComparisonOperator.Contains, TextSearchFilter);
				}

				if (!ClearedFilter)
				{
					finalFilter.AddToFilter(AccStatementSchema.AS_IsCleared, false);
				}

				ZQuery extraDateFilter = GetDateFiltersForStatements();
				ZQuery amountSearchFilter = GetAmountFilter(AccStatementSchema.AS_Amount);

				finalFilter.AddToFilter(extraDateFilter);
				finalFilter.AddToFilter(amountSearchFilter);
				return finalFilter;
			}
		}

		#region List

		CodeDescriptionPairList fType_List;
		public CodeDescriptionPairList Type_List
		{
			get
			{
				if (fType_List == null)
				{
					fType_List = new CodeDescriptionPairList();
					fType_List.AddPair(TransactionTypes.Payment, PAYMENT);
					fType_List.AddPair(TransactionTypes.DirectPayment, DIRECT_PAYMENT);
					fType_List.AddPair(TransactionTypes.OpeningPayment, OPENING_PAYMENT);
					fType_List.AddPair(TransactionTypes.ReceiptBatch, RECEIPT_BATCH);
					fType_List.AddPair(TransactionTypes.OpeningReceipt, OPENING_RECEIPT);
					fType_List.AddPair(TransactionTypes.Transfer, BANK_TRANSFER);
					fType_List.AddPair(TransactionTypes.DDRBatch, DDR_BATCH);
					fType_List.AddPair("STM", Res.GetString("14cdf679-dd79-4872-94ba-a5121549e51f", "Bank Statement"));
					fType_List.AddPair(ALL, Res.GetString("63C90138-7788-494D-BBA9-15DEF8D24A5C", "ALL"));
				}
				return fType_List;
			}
		}

		CodeDescriptionPairList fMethod_List;
		public CodeDescriptionPairList Method_List
		{
			get
			{
				if (fMethod_List == null)
				{
					fMethod_List = new CodeDescriptionPairList();
					fMethod_List.AddPair(ReceiptTypes.Cheque, Res.GetString("698f4bca-1a7a-4455-95a9-45ea5f606522", "Check"));
					fMethod_List.AddPair(ReceiptTypes.Cash, Res.GetString("748ad01b-4069-455d-9d77-5d27509f9744", "Cash"));
					fMethod_List.AddPair(ReceiptTypes.DirectDebit, Res.GetString("05ba9888-10c4-42c5-b860-468c99ca9cd3", "Direct Debit"));
					fMethod_List.AddPair(ReceiptTypes.DirectCredit, Res.GetString("14361c3b-41a2-4761-a6de-38f87795a9be", "Direct Receipt"));
					fMethod_List.AddPair(ReceiptTypes.CreditCard, Res.GetString("23d83fac-5cd7-4e47-8d25-99de3ca87aac", "Credit Card"));
					fMethod_List.AddPair(TransactionTypes.ReceiptBatch, Res.GetString("1805b5a3-212e-49cd-affb-48244e0d13fc", "Receipt Batch"));
					fMethod_List.AddPair(TransactionTypes.Transfer, Res.GetString("6ffaa8d1-e070-42c6-8c21-e63df289eb0f", "Transfer"));
					fMethod_List.AddPair(ReceiptTypes.EFT, Res.GetString("df5789c7-8218-4d2c-9751-19627496ffab", "EFT"));
					fMethod_List.AddPair(ReceiptTypes.ScheduledEFT, Res.GetString("8879FE1C-A43B-4C7B-A4E3-10045A547DE9", "Scheduled EFT"));
					fMethod_List.AddPair(ReceiptTypes.CollectionRequest, Res.GetString("239CF643-83E5-4348-836A-84D8D55032C9", "Collection Request"));
					fMethod_List.AddPair(ReceiptTypes.eNettDirectDebit, Res.GetString("f1ac5de1-18fc-48f9-8fab-6eafe27f56af", "ComPay Direct Debit"));
					fMethod_List.AddRange(new CodeDescriptionPairList(OLookUpEditType.BankChargeTypes));
				}
				return fMethod_List;
			}
		}

		CodeDescriptionPairList fDebitCredit_List;
		public CodeDescriptionPairList DebitCredit_List
		{
			get
			{
				if (fDebitCredit_List == null)
				{
					fDebitCredit_List = new CodeDescriptionPairList();
					fDebitCredit_List.AddPair(DEBIT, DEBIT);
					fDebitCredit_List.AddPair(CREDIT, CREDIT);
					fDebitCredit_List.AddPair(ALL, ALL);
				}
				return fDebitCredit_List;
			}
		}

		#endregion

		#region Type

		[MaxLength(3)]
		[List("Type_List")]
		public ZString TypeFilter
		{
			get { return fTypeFilter; }
			set
			{
				CheckMaximumLength(TypeFilterInfo, value);
				SetNonPersistentPropertyValue(TypeFilterInfo, ref fTypeFilter, value);
				if (BankAccount != null)
				{
					BankAccount.TypeFilter = BankAccount.GetStatementTypeFromTransactionType(value);
				}
			}
		}

		public ZPropertyInfo TypeFilterInfo
		{
			get { return GetZPropertyInfo(nameof(TypeFilter)); }
		}

		ZString fTypeFilter;

		#endregion

		#region Method

		[MaxLength(3)]
		[List("Method_List")]
		public ZString MethodFilter
		{
			get { return methodFilter; }
			set
			{
				CheckMaximumLength(MethodFilterInfo, value);
				SetNonPersistentPropertyValue(MethodFilterInfo, ref methodFilter, value);
			}
		}

		public ZPropertyInfo MethodFilterInfo
		{
			get { return GetZPropertyInfo(nameof(MethodFilter)); }
		}
		ZString methodFilter;

		#endregion

		#region TextSearch

		[MaxLength(AccTransactionHeader.Schema.AH_ChequeOrReferenceMaxLength)]
		public ZString TextSearchFilter
		{
			get { return textSearchFilter; }
			set
			{
				SetNonPersistentPropertyValue(TextSearchFilterInfo, ref textSearchFilter, value);
			}
		}

		public ZPropertyInfo TextSearchFilterInfo
		{
			get { return GetZPropertyInfo(nameof(TextSearchFilter)); }
		}
		ZString textSearchFilter;

		#endregion

		#region Amount

		[DecimalPlaces(nameof(BankCurrencyDecimalsAsInt))]
		public ZDecimal AmountFilter
		{
			get { return amountFilter; }
			set { SetNonPersistentPropertyValue(AmountFilterInfo, ref amountFilter, value); }
		}

		public ZPropertyInfo AmountFilterInfo
		{
			get { return GetZPropertyInfo(nameof(AmountFilter)); }
		}
		ZDecimal amountFilter;

		#endregion

		#region Date Filter Type

		ZString dateFilterType;

		[MaxLength(3)]
		[List("DateFilterList")]
		public ZString DateFilterType
		{
			get { return dateFilterType; }
			set
			{
				CheckMaximumLength(DateFilterTypeInfo, value);
				SetNonPersistentPropertyValue(DateFilterTypeInfo, ref dateFilterType, value);
				if (value == DATE_IN_STATEMENT)
				{
					ClearedFilter = ZBool.True;
					ClearedFilterInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo DateFilterTypeInfo
		{
			get { return GetZPropertyInfo(nameof(DateFilterType)); }
		}

		CodeDescriptionPairList dateFilterList;
		public CodeDescriptionPairList DateFilterList
		{
			get
			{
				if (dateFilterList == null)
				{
					dateFilterList = new CodeDescriptionPairList();
					dateFilterList.AddPair(POST_DATE, Res.GetString("d4b2dfef-720f-4e4b-8f30-559f2288676f", "Post Date"));
					dateFilterList.AddPair(DATE_IN_STATEMENT, Res.GetString("fec66d90-0817-4f0d-bb17-e5df0c07dce6", "Date Shown In Statement"));
				}
				return dateFilterList;
			}
		}

		#endregion

		#region FromDate

		public ZDateTime FromDateFilter
		{
			get { return fromDateFilter; }
			set { SetNonPersistentPropertyValue(FromDateFilterInfo, ref fromDateFilter, value); }
		}

		public ZPropertyInfo FromDateFilterInfo
		{
			get { return GetZPropertyInfo(nameof(FromDateFilter)); }
		}
		ZDateTime fromDateFilter;

		#endregion

		#region ToDate

		public ZDateTime ToDateFilter
		{
			get { return toDateFilter; }
			set { SetNonPersistentPropertyValue(ToDateFilterInfo, ref toDateFilter, value); }
		}

		public ZPropertyInfo ToDateFilterInfo
		{
			get { return GetZPropertyInfo(nameof(ToDateFilter)); }
		}
		ZDateTime toDateFilter;

		#endregion

		#region Cleared

		public ZBool ClearedFilter
		{
			get { return clearedFilter; }
			set { SetNonPersistentPropertyValue(ClearedFilterInfo, ref clearedFilter, value); }
		}

		public ZPropertyInfo ClearedFilterInfo
		{
			get { return GetZPropertyInfo(nameof(ClearedFilter)); }
		}

		ZBool clearedFilter;

		#endregion

		#region Validation

		public void ValidateTypeFilter()
		{
			TypeFilterInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(TypeFilterInfo, Type_List);
		}

		public void ValidateMethodFilter()
		{
			MethodFilterInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(MethodFilterInfo, Method_List);
		}

		#endregion

		public string ApplyFilter()
		{
			string msg = "";

			if (CanReloadTransactions)
			{
				ReloadRecords();
			}
			else
			{
				msg = Res.GetString("0f30bc4d-6cbd-4bf8-91e7-f7ecf9453804", "Please save your changes before applying filter.");
			}

			return msg;
		}

		#endregion

		#region BankList

		AccBankAccountCollection fBankAccountList;
		public AccBankAccountCollection BankAccountList
		{
			get
			{
				if (fBankAccountList == null)
				{
					var cashAccountFilter = new ZQuery(AccBankAccountSchema.AB_AccountType, SQLComparisonOperator.NotEqual, AccountTypeCodeDescriptionPairList.Codes.CSH);
					fBankAccountList = new AccBankAccountCollection(Factory, GlbCompany.CurrentCompany, cashAccountFilter);
					fBankAccountList.SetOverrideNotificationWhenAdditionalFilterNotMet(CashAccountErrorMessage);
				}
				return fBankAccountList;
			}
		}

		#endregion

		#region AdditionalTransactions

		public DirectTransactionsBusinessObject AdditionalTransactions
		{
			get
			{
				if (fAdditionalTransactions == null)
				{
					fAdditionalTransactions = new DirectTransactionsBusinessObject(TransactionsFactory, BankAccount != null ? BankAccount.PK : ZGuid.Empty, StatementDate);
					RegisterEditableChildObject(fAdditionalTransactions);
				}
				return fAdditionalTransactions;
			}
		}
		DirectTransactionsBusinessObject fAdditionalTransactions;

		void ResetAdditionalTransactions()
		{
			if (fAdditionalTransactions != null)
			{
				UnRegisterEditableChildObject(fAdditionalTransactions);
				fAdditionalTransactions = null;
			}
			ResetCombinedTransactions();
		}

		#endregion

		#region Implementation

		ZDateTime LastReconcileDate
		{
			get { return BankAccount != null ? BankAccount.AB_LastReconcileDate : ZDateTime.Empty; }
		}

		ZDateTime LastStatementDate
		{
			get { return BankAccount != null ? BankAccount.AB_LastStatementDate : ZDateTime.Empty; }
		}

		#region Utility Methods

		public bool CanReloadTransactions
		{
			get { return !(Transactions.HasChanges || (GetStatements() != null && GetStatements().HasChanges)); }
		}

		public bool AnyChangesAffectingHistoryDocument
			=> HasChanges || GetStatements()?.HasChanges == true || MergedTransactions.HasChanges
			|| transactionsAddedInThisSession.Count > 0 || transactionsRemovedInThisSession.Count > 0
			|| transactionIdsClearedInCurrentSession.Count > 0 || transactionIdsUnclearedInCurrentSession.Count > 0;

		void ResetFactoryAndReloadRecords()
		{
			if (PreventReloadMultiple_ReloadRecords_Call)
			{
				return;
			}

			ResetTransactionsFactory();
			fUnclearedCashbookAmountFromDB = null;
			fUnclearedStatementAmountFromDB = null;
			OnElementReset();
			RaiseBankAndDatesSet();
			RaiseTransactionsPopulated();
			ResetCurrentSession();
		}

		public void ReloadRecords()
		{
			SuspendCashbookRecalculation = true;
			try
			{
				if (!HasErrors)
				{
					ResetTransactions();
					ResetStatements();
					fUnclearedStatementAmountFromDB = null;
					fUnclearedCashbookAmountFromDB = null;
					OnElementReset();
				}
				RaiseBankAndDatesSet();
			}
			finally
			{
				SuspendCashbookRecalculation = false;
			}
		}

		public void ReloadAdditionalTransactions()
		{
			ResetCombinedTransactions();
			fUnclearedCashbookAmountFromDB = null;
			fUnclearedStatementAmountFromDB = null;
			OnElementReset();
			RaiseBankAndDatesSet();
		}

		public string ValidateBeforeLoad()
		{
			var errorMessageBuilder = new ZStringBuilder();

			if (!IsChangingStatementDate && bankAccountPK.IsValid && !ReconcileDate.IsEmpty && !ReconcileDateInfo.HasErrors())
			{
				ZDecimal unbatchedReceiptTotal = GetUndepositedReceiptAmount(bankAccountPK, ReconcileDate);
				if (unbatchedReceiptTotal != 0)
				{
					AddTwoNewLines(errorMessageBuilder);
					errorMessageBuilder.Append(" ")
						.Append(Res.GetString("28a1d285-8dcc-4dba-8017-97123cadc6e2",
							"- There are some Un-deposited Receipts.{0}   You are advised to create deposit batch for the un-batched receipts before proceeding.",
							System.Environment.NewLine));
				}

				ZDecimal latebatchedReceiptTotal = GetLateDepositedReceiptAmount(bankAccountPK, ReconcileDate);
				if (latebatchedReceiptTotal != 0)
				{
					AddTwoNewLines(errorMessageBuilder);
					errorMessageBuilder.Append(" ")
						.Append(Res.GetString("ba18dc4d-939e-44bd-860e-cc5b60805709",
							"- There are some Receipts deposited later than current statement date."));
				}

				ZDecimal unbatchedDDRTotal = GetUnbatchedDDRAmount(bankAccountPK, ReconcileDate);
				if (unbatchedDDRTotal != 0)
				{
					AddTwoNewLines(errorMessageBuilder);
					errorMessageBuilder.Append(" ")
						.Append(Res.GetString("b982a8cf-0dc2-4545-8bf1-ae302e8ebe87", "- There are some un-batched DDR Payments.{0}   You are advised to create direct debit batch for them since un-batched DDR payments do not appear on the Bank Reconciliation.", System.Environment.NewLine));
				}

				ZDecimal latebatchedDDRTotal = GetLateBatchedDDRAmount(bankAccountPK, ReconcileDate);
				if (latebatchedDDRTotal != 0)
				{
					AddTwoNewLines(errorMessageBuilder);
					errorMessageBuilder.Append(" ")
						.Append(Res.GetString("31b0cf4a-89d3-429d-a4b2-294343cf89dc", "- There are some DDR Payments batched later than current statement date."));
				}
			}

			if (!errorMessageBuilder.IsEmpty)
			{
				errorMessageBuilder.Prepend(Res.GetString("aefdb860-65df-4d4c-b956-fca0a3c1cfd6", "Bank Reconciliation has found following issues."));
			}
			return errorMessageBuilder.ToString();
		}

		#region SQLClauses

		string UndepositedReceiptAmountSQL
		{
			get
			{
				return @"SELECT ISNULL(SUM(
							CASE
							WHEN AH_TransactionType = 'REC' THEN
							(
								CASE
								WHEN AB_RX_NKAccountCurrency = GC_RX_NKLocalCurrency THEN
								-(AH_InvoiceAmount + AH_GSTAmount)
								ELSE
								-AH_OSTotal
								End
							)
							ELSE
								AH_OSTotal
							END), 0) AS UndepositedReceiptAmount
						FROM 
							dbo.AccTransactionHeader
							INNER JOIN dbo.GlbCompany ON GC_PK = AH_GC
							INNER JOIN dbo.AccBankAccount ON AB_PK = AH_AB
						WHERE	
							AH_TransactionType IN ('REC','DRC')
							AND AH_AB = @Bank
							AND AH_GC = @Company
							AND AH_DateClearedInCashbook IS NULL 
							AND AH_IsCancelled <> 1 
							AND (AH_ReceiptBatchNo = '' OR AH_ReceiptBatchNo IS NULL)
							AND AH_PostDate < @PostDate
						OPTION (RECOMPILE)";
			}
		}

		string LateDepositedReceiptAmountSQL
		{
			get
			{
				return @"SELECT ISNULL(SUM(
							CASE 
								WHEN ReceiptHeaders.AH_TransactionType = 'REC' THEN
								(
									CASE
									WHEN AB_RX_NKAccountCurrency = GC_RX_NKLocalCurrency THEN
									-(ReceiptHeaders.AH_InvoiceAmount + ReceiptHeaders.AH_GSTAmount)
									ELSE
									-ReceiptHeaders.AH_OSTotal
									End
								)
								ELSE
									ReceiptHeaders.AH_OSTotal
								END), 0) AS LateDepositedReceiptAmount
						FROM 
							dbo.AccTransactionHeader AS ReceiptHeaders
							INNER JOIN dbo.GlbCompany ON GC_PK = ReceiptHeaders.AH_GC
							INNER JOIN dbo.AccBankAccount ON AB_PK = ReceiptHeaders.AH_AB
							INNER JOIN dbo.AccTransactionHeader AS BatchHeader
								ON ReceiptHeaders.AH_ReceiptBatchNo =  BatchHeader.AH_TransactionNum 
									AND ReceiptHeaders.AH_AB = BatchHeader.AH_AB 
									AND ReceiptHeaders.AH_GC = BatchHeader.AH_GC 
						WHERE 
							ReceiptHeaders.AH_TransactionType IN ('REC','DRC')
							AND BatchHeader.AH_TransactionType = 'RCB' 
							AND ReceiptHeaders.AH_AB = @Bank
							AND BatchHeader.AH_GC = @Company
							AND ReceiptHeaders.AH_DateClearedInCashbook IS NULL 
							AND ReceiptHeaders.AH_IsCancelled = 0
							AND BatchHeader.AH_DateClearedInCashbook IS NULL 
							AND BatchHeader.AH_IsCancelled = 0
							AND ReceiptHeaders.AH_PostDate < @PostDate
							AND BatchHeader.AH_PostDate >= @PostDate
						OPTION (RECOMPILE)";
			}
		}

		string UnbatchedDDRAmountSQL
		{
			get
			{
				return @"SELECT ISNULL(SUM(
							CASE 
								WHEN AH_TransactionType = 'PAY' THEN 
									-AH_OSTotal 
								ELSE 
									AH_OSTotal 
							END), 0) AS UnbatchedDDRAmount
						FROM 
							dbo.AccTransactionHeader
						WHERE 
							AH_TransactionType IN ('PAY','DPY') 
							AND AH_AB = @Bank 
							AND AH_GC = @Company
							AND AH_DateClearedInCashbook IS NULL 
							AND AH_IsCancelled <> 1 
							AND (AH_ReceiptBatchNo = '' OR AH_ReceiptBatchNo IS NULL)
							AND AH_ReceiptType = 'DDR'
							AND AH_PostDate < @PostDate
						OPTION (RECOMPILE)";
			}
		}

		string LateBatchedDDRAmountSQL
		{
			get
			{
				return @"SELECT ISNULL(SUM(
								CASE 
									WHEN PaymentHeaders.AH_TransactionType = 'PAY' THEN 
										-PaymentHeaders.AH_OSTotal 
									ELSE 
										PaymentHeaders.AH_OSTotal 
								END), 0) AS LateBatchedDDRAmount
						" + LateBatchedDDRAmountSQL_FROM_WHERE
						+ " OPTION (RECOMPILE)";
			}
		}

		string LateBatchedDDRAmountSQL_FROM_WHERE
		{
			get
			{
				return @"FROM 
							dbo.AccTransactionHeader AS PaymentHeaders 
							INNER JOIN dbo.AccTransactionHeader AS BatchHeader ON 
								PaymentHeaders.AH_ReceiptBatchNo =  BatchHeader.AH_TransactionNum 
								AND PaymentHeaders.AH_AB = BatchHeader.AH_AB 
								AND PaymentHeaders.AH_GC = BatchHeader.AH_GC
						WHERE 
							BatchHeader.AH_TransactionType = 'DDB'
							AND PaymentHeaders.AH_TransactionType IN ('PAY', 'DPY')
							AND BatchHeader.AH_GC = @Company 
							AND PaymentHeaders.AH_AB = @Bank
							AND PaymentHeaders.AH_DateClearedInCashbook IS NULL 
							AND PaymentHeaders.AH_IsCancelled = 0
							AND BatchHeader.AH_DateClearedInCashbook IS NULL 
							AND BatchHeader.AH_IsCancelled = 0
							AND PaymentHeaders.AH_PostDate < @PostDate 
							AND BatchHeader.AH_PostDate >= @PostDate";
			}
		}

		#endregion

		void AddTwoNewLines(ZStringBuilder baseErrorMessage)
		{
			baseErrorMessage.Append(System.Environment.NewLine);
			baseErrorMessage.Append(System.Environment.NewLine);
		}

		protected ZDecimal GetUndepositedReceiptAmount(ZGuid aH_AB, ZDateTime aH_PostDate)
		{
			var sqlParameters = new ZSqlParameter[]
			{
				ZSqlParameter.New("@Bank", aH_AB.ToGuid(), AccTransactionHeaderSchema.AH_AB),
				ZSqlParameter.New("@PostDate", aH_PostDate.AddDays(1).ToDateTime().Date, AccTransactionHeaderSchema.AH_PostDate),
				ZSqlParameter.New("@Company", GlbCompany.CurrentCompany.PK.ToGuid(), AccTransactionHeaderSchema.AH_GC)
			};

			var collection = new DynamicBusinessObjectCollection(Factory);
			collection.Load(UndepositedReceiptAmountSQL, sqlParameters);
			if (collection.Count > 0)
			{
				return (ZDecimal)collection[0]["UndepositedReceiptAmount"];
			}
			return 0m;
		}

		ZDecimal GetLateDepositedReceiptAmount(ZGuid aH_AB, ZDateTime aH_PostDate)
		{
			var sqlParameters = new ZSqlParameter[]
			{
				ZSqlParameter.New("@Bank", aH_AB.ToGuid(), AccTransactionHeaderSchema.AH_AB),
				ZSqlParameter.New("@PostDate", aH_PostDate.AddDays(1).ToDateTime().Date, AccTransactionHeaderSchema.AH_PostDate),
				ZSqlParameter.New("@Company", GlbCompany.CurrentCompany.PK.ToGuid(), AccTransactionHeaderSchema.AH_GC)
			};

			var collection = new DynamicBusinessObjectCollection(Factory);
			collection.Load(LateDepositedReceiptAmountSQL, sqlParameters);
			if (collection.Count > 0)
			{
				return (ZDecimal)collection[0]["LateDepositedReceiptAmount"];
			}
			return 0m;
		}

		ZDecimal GetUnbatchedDDRAmount(ZGuid aH_AB, ZDateTime aH_PostDate)
		{
			var sqlParameters = new ZSqlParameter[]
			{
				ZSqlParameter.New("@Bank", aH_AB.ToGuid(), AccTransactionHeaderSchema.AH_AB),
				ZSqlParameter.New("@PostDate", aH_PostDate.AddDays(1).ToDateTime().Date, AccTransactionHeaderSchema.AH_PostDate),
				ZSqlParameter.New("@Company", GlbCompany.CurrentCompany.PK.ToGuid(), AccTransactionHeaderSchema.AH_GC)
			};

			var collection = new DynamicBusinessObjectCollection(Factory);
			collection.Load(UnbatchedDDRAmountSQL, sqlParameters);
			if (collection.Count > 0)
			{
				return (ZDecimal)collection[0]["UnbatchedDDRAmount"];
			}
			return 0m;
		}

		ZDecimal GetLateBatchedDDRAmount(ZGuid aH_AB, ZDateTime aH_PostDate)
		{
			var sqlParameters = new ZSqlParameter[]
			{
				ZSqlParameter.New("@Bank", aH_AB.ToGuid(), AccTransactionHeaderSchema.AH_AB),
				ZSqlParameter.New("@PostDate", aH_PostDate.AddDays(1).ToDateTime().Date, AccTransactionHeaderSchema.AH_PostDate),
				ZSqlParameter.New("@Company", GlbCompany.CurrentCompany.PK.ToGuid(), AccTransactionHeaderSchema.AH_GC)
			};

			var collection = new DynamicBusinessObjectCollection(Factory);
			collection.Load(LateBatchedDDRAmountSQL, sqlParameters);
			if (collection.Count > 0)
			{
				return (ZDecimal)collection[0]["LateBatchedDDRAmount"];
			}
			return 0m;
		}

		protected ZDecimal GetCashBookAmount(ZGuid bank, ZDateTime postDate)
		{
			if (!bank.IsValid || postDate.IsEmpty)
			{
				return 0m;
			}

			string sQLText = @"	SELECT CashbookBalance 
								FROM BankInformationWithDate(@AH_AB, @AH_PostDate)
								OPTION (RECOMPILE)";

			var sqlParameters = new ZSqlParameter[]
			{
				ZSqlParameter.New("@AH_AB", bank.ToGuid(), AccTransactionHeaderSchema.AH_AB),
				ZSqlParameter.New("@AH_PostDate", postDate.AddDays(1).ToDateTime().Date, AccTransactionHeaderSchema.AH_PostDate)
			};

			var collection = new DynamicBusinessObjectCollection(Factory);
			collection.Load(sQLText, sqlParameters);
			if (collection.Count > 0)
			{
				return (ZDecimal)collection[0]["CashbookBalance"];
			}
			return 0m;
		}

		ZDecimal? fUnclearedStatementAmountFromDB;
		ZDecimal UnclearedStatementAmountFromDB
		{
			get
			{
				if (fUnclearedStatementAmountFromDB == null)
				{
					fUnclearedStatementAmountFromDB = 0;
					if (AreKeyFieldsValid && !PreventReloadMultiple_ReloadRecords_Call)
					{
						SetUnclearedAmountsFromDB(BankAccountPK, ReconcileDate, StatementDate);
					}
				}
				return fUnclearedStatementAmountFromDB.Value;
			}
		}

		ZDecimal? fUnclearedCashbookAmountFromDB;
		ZDecimal UnclearedCashbookAmountFromDB
		{
			get
			{
				if (fUnclearedCashbookAmountFromDB == null)
				{
					fUnclearedCashbookAmountFromDB = 0;
					if (AreKeyFieldsValid && !PreventReloadMultiple_ReloadRecords_Call)
					{
						SetUnclearedAmountsFromDB(BankAccountPK, ReconcileDate, StatementDate);
					}
				}
				return fUnclearedCashbookAmountFromDB.Value;
			}
		}

		void SetUnclearedAmountsFromDB(ZGuid bank, ZDateTime postDate, ZDateTime statementDate)
		{
			if (!bank.IsValid || postDate.IsEmpty)
			{
				this.fUnclearedCashbookAmountFromDB = 0m;
				this.fUnclearedStatementAmountFromDB = 0m;
				return;
			}

			var sql = @"
SELECT
	CashbookUnclearedAmount  = SUM(DepTotal) + SUM(PayTotal) + SUM(TransferTotal) + SUM(ReceiptTotal) + SUM(UnbatchedDDRTotal),
	StatementUnclearedAmount = SUM(DepStatementAmount) + SUM(PayStatementAmount) + SUM(TransferStatementAmount)
FROM
	dbo.BankReconciliationSummaryWithDate (@Bank, @Company, @PostDate, @StatementDate)
";
			var sqlParameters = new ZSqlParameter[]
			{
				ZSqlParameter.New("@Bank", bank, AccTransactionHeaderSchema.AH_AB),
				ZSqlParameter.New("@PostDate", postDate.Date.AddDays(1), AccTransactionHeaderSchema.AH_PostDate),
				ZSqlParameter.New("@StatementDate", statementDate.Date.AddDays(1), AccStatementSchema.AS_StatementDate),
				ZSqlParameter.New("@Company", GlbCompany.CurrentCompany.PK.ToGuid(), AccTransactionHeaderSchema.AH_GC),
			};

			var collection = new DynamicBusinessObjectCollection(Factory);
			collection.Load(sql, sqlParameters);
			if (collection.Count > 0)
			{
				var bizO = collection[0];
				this.fUnclearedCashbookAmountFromDB = (ZDecimal)bizO["CashbookUnclearedAmount"];
				this.fUnclearedStatementAmountFromDB = (ZDecimal)bizO["StatementUnclearedAmount"];
			}
		}

		bool SuspendCashbookRecalculation;
		bool PreventReloadMultiple_ReloadRecords_Call;

		#endregion

		public bool AreKeyFieldsValid
		{
			get
			{
				return BankAccountPK.IsValid && BankAccount != null &&
					ReconcileDate.IsValid && !ReconcileDateInfo.HasErrors() &&
					StatementDate.IsValid && !StatementDateInfo.HasErrors();
			}
		}

		#region TransactionsFactory

		BusinessObjectFactory TransactionsFactory
		{
			get { return GetTransactionsFactory(); }
		}

		BusinessObjectFactory GetTransactionsFactory()
		{
			if (TransactionsFactory_innerValue == null)
			{
				TransactionsFactory_innerValue = new BusinessObjectFactory();
				Factory.ChildFactories.Add(TransactionsFactory_innerValue);
			}
			return TransactionsFactory_innerValue;
		}

		BusinessObjectFactory TransactionsFactory_innerValue;

		void ResetTransactionsFactory()
		{
			ResetTransactions();
			ResetStatements();
			ResetAdditionalTransactions();
			BankAccount_innerValue = null;
			if (TransactionsFactory_innerValue != null)
			{
				Factory.ChildFactories.Remove(TransactionsFactory_innerValue);
				GCWrapper.ReclaimMemory(ref TransactionsFactory_innerValue);
			}
			GetTransactionsFactory();
		}

		#endregion

		#region History (eDocs)

		/// <summary>
		/// Attaches a bank reconciliation history document to the eDocs of bank account.
		/// Must be called before Factory.Save() or ZForm Save action.
		/// </summary>
		public void AttachHistoryEDocForCurrentSession()
		{
			if (!AnyChangesAffectingHistoryDocument)
			{
				return;
			}
			var docManager = BankAccount.DocManagerInfo();
			AddEDocsFactoryToMainFactory(docManager);   // To ensure the eDoc is saved in the same transaction

			var filename = GetUniqueFilenameForHistoryEDoc(docManager);
			var docTemplate = LoadBankReconciliationHistoryDocumentTemplate();
			var wrappedBizo = DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.BankReconciliation, this);
			using (var docPack = new DocumentPack())
			{
				var report = new Report(docPack, docTemplate, wrappedBizo, filename.Replace(".xlsx", ""), null, DocumentEngineCore.DocumentSupport.DocumentDirection.ANY, false);
				RunReportAndAttachEDoc(report, docManager, filename);
			}
		}

		public void RunReportAndAttachEDoc(Report report, DocManagerInfo docManager, string filename)
		{
			using (var outputStream = new MemoryStream())
			{
				report.Save(outputStream);
				if (outputStream.Length > 0)
				{
					docManager.AddFileOrDocument(outputStream.ToArray(), filename, Core.Constants.RefDocTypes.BankReconciliation);
				}
			}
		}

		string GetUniqueFilenameForHistoryEDoc(DocManagerInfo docManager)
		{
			var baseFileName = FormattableString.Invariant($"Bank Reconciliation {Env.Time.CurrentLocalDateTime:yyyyMMdd_HHmmss}");
			var candidateFilename = baseFileName + ".xlsx";
			int counter = 1;
			while (docManager.Files.Cast<IDeliveryEmailAttachment>().Any(f => string.Equals(f.FileName, candidateFilename, StringComparison.OrdinalIgnoreCase)))
			{
				candidateFilename = FormattableString.Invariant($"{baseFileName}_{counter}.xlsx");
				counter++;
			}
			return candidateFilename;
		}

		ExcelTemplate LoadBankReconciliationHistoryDocumentTemplate()
			=> new ExcelTemplateReadFromStmTemplateTable(Factory.Load<StmTemplate>(Guid.Parse("088955bc-1a67-4435-baae-21fc318c14b6")));

		void AddEDocsFactoryToMainFactory(DocManagerInfo docManager)
		{
			docManager.UseBusinessEntityFactoryAsInternal = true;

			var docFactory = (BusinessObjectFactory)docManager.MasterFactory;
			if (docFactory.ChildFactories.Contains(Factory))
			{
				docFactory.ChildFactories.Remove(Factory);
			}
			if (docFactory.ChildFactories.Contains(TransactionsFactory))
			{
				docFactory.ChildFactories.Remove(TransactionsFactory);
			}

			if (!Factory.ChildFactories.Contains(docFactory))
			{
				Factory.ChildFactories.Add(docFactory);
				Factory.Saved += RemoveEDocsFactoryAfterFactorySaved;
			}
		}

		void RemoveEDocsFactoryAfterFactorySaved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			Factory.Saved -= RemoveEDocsFactoryAfterFactorySaved;
			var docManager = BankAccount.DocManagerInfo();
			var docFactory = (BusinessObjectFactory)docManager.MasterFactory;
			Factory.ChildFactories.Remove(docFactory);
		}

		#endregion

		#endregion

		#region Override

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateBankAccount();
			ValidateReconcileDate();
			ValidateStatementDate();
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();

			if (BankAccount != null)
			{
				if (!ReconcileDate.IsEmpty)
				{
					BankAccount.AB_LastReconcileDate = ReconcileDate;
				}
				if (!StatementDate.IsEmpty)
				{
					BankAccount.AB_LastStatementDate = StatementDate;
				}
			}
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			if (saveSucceeded)
			{
				ResetAdditionalTransactions();
				ResetTransactions();
				ReloadAdditionalTransactions();
				ResetCurrentSession();
			}
		}

#endregion
	}
}

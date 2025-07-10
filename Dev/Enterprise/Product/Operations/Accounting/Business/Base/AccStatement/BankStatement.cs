using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Auto;
using Enterprise.Accounting.Business.CashBook;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Base.AccStatement
{
	[Serializable]
	public partial class BankStatementException : Exception
	{
		public BankStatementException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected BankStatementException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}

	public partial class BankStatement : AccBankAccount
	{
		public event StatementEventHandler StatementTypeChanged;
		public event StatementEventHandler StatementDirectTransactionCreated;

		public BankStatement(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Overriden Readonly Properties

		protected bool AB_LastStatementDate_ReadOnly
		{
			get { return true; }
		}

		protected bool BalanceAmount_ReadOnly
		{
			get { return true; }
		}

		protected bool PageBalanceAmountDate_ReadOnly
		{
			get { return true; }
		}

		#endregion

		#region DirectTransactions

		public DirectTransactionsBusinessObject DirectTransactions
		{
			get
			{
				if (fDirectTransactions == null)
				{
					fDirectTransactions = new DirectTransactionsBusinessObject(Factory, PK, AB_LastStatementDate);
					RegisterEditableChildObject(fDirectTransactions);
				}
				return fDirectTransactions;
			}
		}
		DirectTransactionsBusinessObject fDirectTransactions;

		#endregion

		#region Statements

		[ChildEditable(true)]
		StatementCollection Statements
		{
			get
			{
				if (fStatements == null)
				{
					fStatements = new StatementCollection(this, Factory);
					RegisterEditableChildObject(fStatements);
				}

				return fStatements;
			}
		}

		public Statement AddNewStatement()
		{
			return Statements.AddNew();
		}

#if DEBUG

		public StatementCollection GetStatements_ForTestOnly()
		{
			return Statements;
		}

#endif

		#endregion

		#region UnreconciledStatements

		public UnreconciledStatementCollectionView UnreconciledStatements
		{
			get
			{
				if (fUnreconciledStatements == null)
				{
					fUnreconciledStatements = new UnreconciledStatementCollectionView(Statements);
				}

				return fUnreconciledStatements;
			}
		}

		#endregion

		#region ReconciledStatements

		public ReconciledStatementCollectionView ReconciledStatements
		{
			get
			{
				if (fReconciledStatements == null)
				{
					fReconciledStatements = new ReconciledStatementCollectionView(Statements);
				}

				return fReconciledStatements;
			}
		}

		#endregion

		#region Statement Balance

		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		public ZDecimal StatementBalance
		{
			get { return fStatementBalance; }
			set { SetNonPersistentPropertyValue(StatementBalanceInfo, ref fStatementBalance, value); }
		}

		public ZPropertyInfo StatementBalanceInfo
		{
			get { return GetZPropertyInfo(nameof(StatementBalance)); }
		}

		#endregion

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region Opening Statement Balance
		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		public ZDecimal OpeningStatementBalance
		{
			get { return fOpeningStatementBalance; }

			set
			{
				SetNonPersistentPropertyValue(OpeningStatementBalanceInfo, ref fOpeningStatementBalance, value);
				CalculateBalanceAmount();
			}
		}

		public ZPropertyInfo OpeningStatementBalanceInfo
		{
			get { return GetZPropertyInfo(nameof(OpeningStatementBalance)); }
		}

		#endregion

		#region ShouldCreateDirectTransactionForStatement

		public bool ShouldCreateDirectTransactionForStatement(Statement statement)
		{
			bool result = false;
			if (StatementTypeChanged != null)
			{
				StatementEventArgs args = new StatementEventArgs(statement);
				StatementTypeChanged(this, args);
				result = args.Result;
			}
			return result;
		}

		#endregion

		#region ShowStatementDirectTransaction

		public void ShowStatementDirectTransaction(Statement statement)
		{
			if (StatementDirectTransactionCreated != null)
			{
				StatementEventArgs args = new StatementEventArgs(statement);
				StatementDirectTransactionCreated(this, args);
			}
		}

		#endregion

		#region DebitTotal

		public ZString DebitTotal
		{
			get { return Res.GetString("BankStatemen|TransactionsWithTotal", "{0} ({1} Transactions)", fDebitTotal.ToString(2), fDebitTotalCount); }
		}

		public ZPropertyInfo DebitTotalInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(DebitTotal));
			}
		}

		#endregion

		#region CreditTotal

		public ZString CreditTotal
		{
			get { return Res.GetString("BankStatemen|TransactionsWithTotal", "{0} ({1} Transactions)", fCreditTotal.ToString(2), fCreditTotalCount); }
		}

		public ZPropertyInfo CreditTotalInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(CreditTotal));
			}
		}

		#endregion

		#region BalanceAmount

		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		public ZDecimal BalanceAmount
		{
			get { return fBalanceAmount; }
			set
			{
				SetNonPersistentPropertyValue(BalanceAmountInfo, ref fBalanceAmount, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateBalanceAmount();
				}
			}
		}

		public ZPropertyInfo BalanceAmountInfo
		{
			get { return GetZPropertyInfo(nameof(BalanceAmount)); }
		}

		#endregion

		#region PageDebit Total

		public ZString PageDebitTotal
		{
			get { return Res.GetString("BankStatemen|TransactionsWithTotal", "{0} ({1} Transactions)", fPageDebitTotal.ToString(2), fPageDebitTotalCount); }
		}

		public ZPropertyInfo PageDebitTotalInfo
		{
			get { return GetZPropertyInfo(nameof(PageDebitTotal)); }
		}

		#endregion

		#region PageCredit Total

		public ZString PageCreditTotal
		{
			get { return Res.GetString("BankStatemen|TransactionsWithTotal", "{0} ({1} Transactions)", fPageCreditTotal.ToString(2), fPageCreditTotalCount); }
		}

		public ZPropertyInfo PageCreditTotalInfo
		{
			get { return GetZPropertyInfo(nameof(PageCreditTotal)); }
		}

		#endregion

		#region PageBalance Amount

		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		public ZDecimal PageBalanceAmount
		{
			get { return fPageBalanceAmount; }

			set
			{
				fPageBalanceAmount = value;
				CalculatePageBalanceAmount();
			}
		}

		public ZPropertyInfo PageBalanceAmountInfo
		{
			get { return GetZPropertyInfo(nameof(PageBalanceAmount)); }
		}

		#endregion

		#region Max Page

		public ZShort MaxPage
		{
			get { return fMaxPage; }
		}

		public ZPropertyInfo MaxPageInfo
		{
			get { return GetZPropertyInfo(nameof(MaxPage)); }
		}

		#endregion

		#region Filters

		#region Filter List

		#region AB_Type_List

		public CodeDescriptionPairList AB_Type_List
		{
			get
			{
				if (fAB_Type_List == null)
				{
					fAB_Type_List = new CodeDescriptionPairList();
					fAB_Type_List.AddPair(ReceiptTypes.Cheque, Res.GetString("5511aa26-8843-461b-a25f-4f7e1b20d77f", "Check"));
					fAB_Type_List.AddPair(ReceiptTypes.CreditCard, Res.GetString("9dc9a0ec-603d-47d0-b398-62a3a1c2ffa5", "Credit Card"));
					fAB_Type_List.AddPair(ReceiptTypes.DirectDebit, Res.GetString("2e19a5f5-4821-48d2-ab1d-54d13cf8431b", "Direct Debit"));
					fAB_Type_List.AddPair(ReceiptTypes.EFT, Res.GetString("1e00632e-99a0-4ca8-b7d8-66b3e42bf7cd", "Electronic Funds Transfer"));
					fAB_Type_List.AddPair(ReceiptTypes.ScheduledEFT, ResString.GetMultilingualString("54869BF2-4A1D-405A-9520-A48B2D40D5C1", "Scheduled EFT"));
					fAB_Type_List.AddPair(ReceiptTypes.CollectionRequest, ResString.GetMultilingualString("4FD2D963-C755-42D1-9A6F-9E61900EA7C5", "Collection Request"));
					fAB_Type_List.AddPair(TransactionTypes.ReceiptBatch, Res.GetString("4c8c085b-49b1-45d6-888c-6bf0f200b722", "Receipt Batch"));
					fAB_Type_List.AddPair(TransactionTypes.Transfer, Res.GetString("f58d44bb-562b-4e37-bc75-81908bc0ae62", "Transfer"));
					fAB_Type_List.AddRange(new CodeDescriptionPairList(OLookUpEditType.BankChargeTypes));
				}
				return fAB_Type_List;
			}
		}

		#endregion

		#region AB_DebitCredit_List

		public CodeDescriptionPairList AB_DebitCredit_List
		{
			get
			{
				if (fAB_DebitCredit_List == null)
				{
					fAB_DebitCredit_List = new CodeDescriptionPairList(OLookUpEditType.DebitCredit);
				}
				return fAB_DebitCredit_List;
			}
		}

		#endregion

		#endregion

		#region Statement Date Filter

		public ZDateTime StatementDateFilter
		{
			get { return fStatementDateFilter; }
			set
			{
				SetNonPersistentPropertyValue(StatementDateFilterInfo, ref fStatementDateFilter, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateStatementDateFilter();
				}
			}
		}

		public ZPropertyInfo StatementDateFilterInfo
		{
			get { return GetZPropertyInfo(nameof(StatementDateFilter)); }
		}

		#endregion

		#region Amount Filter

		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		public ZDecimal AmountFilter
		{
			get { return fAmountFilter; }
			set { SetNonPersistentPropertyValue(AmountFilterInfo, ref fAmountFilter, value); }
		}

		public ZPropertyInfo AmountFilterInfo
		{
			get { return GetZPropertyInfo(nameof(AmountFilter)); }
		}

		#endregion

		#region Debit & Credit Filter

		[MaxLength(2)]
		[List("AB_DebitCredit_List")]
		public ZString DebitCreditFilter
		{
			get { return fDebitCreditFilter; }
			set
			{
				if (fDebitCreditFilter != value)
				{
					CheckMaximumLength(DebitCreditFilterInfo, value);
					SetNonPersistentPropertyValue(DebitCreditFilterInfo, ref fDebitCreditFilter, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateDebitCreditFilter();
					}
				}
			}
		}

		public ZPropertyInfo DebitCreditFilterInfo
		{
			get { return GetZPropertyInfo(nameof(DebitCreditFilter)); }
		}

		#endregion

		#region Type Filter

		[MaxLength(3)]
		[List("AB_Type_List")]
		public ZString TypeFilter
		{
			get { return fTypeFilter; }
			set
			{
				if (fTypeFilter != value)
				{
					CheckMaximumLength(TypeFilterInfo, value);
					SetNonPersistentPropertyValue(TypeFilterInfo, ref fTypeFilter, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateTypeFilter();
					}
				}
			}
		}

		public ZPropertyInfo TypeFilterInfo
		{
			get { return GetZPropertyInfo(nameof(TypeFilter)); }
		}

		#endregion

		#region Current Page Filter

		public ZShort CurrentPageFilter
		{
			get { return fCurrentPageFilter; }
			set { SetNonPersistentPropertyValue(CurrentPageFilterInfo, ref fCurrentPageFilter, value); }
		}

		public ZPropertyInfo CurrentPageFilterInfo
		{
			get { return GetZPropertyInfo(nameof(CurrentPageFilter)); }
		}

		#endregion

		#region Cheque & Reference Filter

		[MaxLength(AutoAccStatement.Schema.AS_ChequeOrReferenceMaxLength)]
		public ZString ChequeReferenceFilter
		{
			get { return fChequeReferenceFilter; }
			set
			{
				if (fChequeReferenceFilter != value)
				{
					CheckMaximumLength(ChequeReferenceFilterInfo, value);
					SetNonPersistentPropertyValue(ChequeReferenceFilterInfo, ref fChequeReferenceFilter, value);
				}
			}
		}

		public ZPropertyInfo ChequeReferenceFilterInfo
		{
			get { return GetZPropertyInfo(nameof(ChequeReferenceFilter)); }
		}

		#endregion

		public void ApplyFilter()
		{
			ApplyFilter(false);
		}

		public void ApplyFilter(bool suspendValidation)
		{
			if (!suspendValidation)
			{
				ValidateFilter(); // throws exception to the GUI layer.
			}

			ZQuery filter = new ZQuery();

			if (!StatementDateFilter.IsEmpty)
			{
				filter.AddToFilter(AccStatementSchema.AS_StatementDate, SQLComparisonOperator.Equal, StatementDateFilter);
			}

			if (!AmountFilter.IsEmpty && AmountFilter != 0.0m)
			{
				filter.AddToFilter(AccStatementSchema.AS_Amount, SQLComparisonOperator.Equal, AmountFilter);
			}

			if (!DebitCreditFilter.IsEmpty)
			{
				filter.AddToFilter(AccStatementSchema.AS_DebitCredit, SQLComparisonOperator.Equal, DebitCreditFilter);
			}

			ZQuery typeFilter = ApplyTypeFilter();
			if (typeFilter != null)
			{
				filter.AddToFilter(typeFilter);
			}

			if (!CurrentPageFilter.IsEmpty)
			{
				filter.AddToFilter(AccStatementSchema.AS_PageNumber, SQLComparisonOperator.Equal, CurrentPageFilter);
			}

			if (!ChequeReferenceFilter.IsEmpty)
			{
				filter.AddToFilter(AccStatementSchema.AS_ChequeOrReference, SQLComparisonOperator.Contains, ChequeReferenceFilter);
			}

			ZQuery totalQuery = new ZQuery(filter);

			if (totalQuery != null)
			{
				var tmp = new StatementCollection(this, Factory);
				ReconciledStatements.SwapCollectionToFilter(tmp);
				UnreconciledStatements.SwapCollectionToFilter(tmp);

				Statements.LoadWithMoreFiltering(totalQuery);
				ReconciledStatements.SwapCollectionToFilter(Statements);
				UnreconciledStatements.SwapCollectionToFilter(Statements);
			}

			CalculateTotals();
		}

		public ZQuery ApplyTypeFilter()
		{
			ZQuery result = null;
			if (!TypeFilter.IsEmpty && TypeFilter != "ALL" && TypeFilter != "STM")
			{
				result = new ZQuery(AccStatementSchema.AS_Type, SQLComparisonOperator.Equal, TypeFilter);
			}
			return result;
		}

		public void ClearFilter()
		{
			StatementDateFilter = ZDateTime.Empty;
			AmountFilter = 0.0m;
			DebitCreditFilter = ZString.Empty;
			TypeFilter = ZString.Empty;
			CurrentPageFilter = 0;
			ChequeReferenceFilter = ZString.Empty;

			ApplyFilter();
		}

		#endregion

		#region Validation

		public new BankStatementValidation Validation
		{
			get { return (BankStatementValidation)base.Validation; }
		}

		protected override AccBankAccountValidation GetNewValidation()
		{
			return new BankStatementValidation(this);
		}

		void ValidateFilter()
		{
			if (Statements.HasChanges)
			{
				throw new BankStatementException("Please save statements before filtering.");
			}

			RunPreSaveValidation();

			if (HasErrors)
			{
				throw new BankStatementException("There are errors. Please fix them before filtering.");
			}
		}

		#endregion

		#region Calculate

		#region AB_LastStatementDate

		public override ZDateTime AB_LastStatementDate
		{
			get { return base.AB_LastStatementDate; }
			set
			{
				if (!Statements.HasChanges)
				{
					base.AB_LastStatementDate = value;
					StatementDateFilter = value;
				}
			}
		}

		#endregion

		#region AB_StatementBalance
		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		public override ZDecimal AB_StatementBalance
		{
			get
			{
				return base.AB_StatementBalance;
			}
			set
			{
				base.AB_StatementBalance = value;
				CalculateBalanceAmount();
			}
		}

		#endregion

		public void CalculateTotals()
		{
			CalculatePageDebitTotal();
			CalculatePageCreditTotal();
			CalculateDebitTotal();
			CalculateCreditTotal();
			CalculateBalanceAmount();
			CalculatePageBalanceAmount();
		}

		public void CalculateTotalsAfterStatementDeleted(ZString statementType, ZDecimal deletedAmountValue)
		{
			if (statementType == Statement.CREDIT)
			{
				CalculatePageCreditTotal();
				CalculateCreditTotal(0m, -deletedAmountValue);
			}
			else
			{
				CalculatePageDebitTotal();
				CalculateDebitTotal(0m, -deletedAmountValue);
			}
			CalculateBalanceAmount();
			CalculatePageBalanceAmount();
		}

		public void CalculateDebitTotal()
		{
			CalculateDebitTotal(0.0m, 0.0m);
		}

		public void CalculateDebitTotal(ZDecimal originalValue, ZDecimal value)
		{
			if (value != 0.0m)
			{
				fDebitTotal += value - originalValue;

				if (value < 0)
				{
					fDebitTotalCount--;
				}
				else if (originalValue == 0m)
				{
					fDebitTotalCount++;
				}
			}

			DebitTotalInfo.RefreshBinding();
		}

		public void CalculateCreditTotal()
		{
			CalculateCreditTotal(0.0m, 0.0m);
		}

		public void CalculateCreditTotal(ZDecimal originalValue, ZDecimal value)
		{
			if (value != 0.0m)
			{
				fCreditTotal += value - originalValue;
				if (value < 0)
				{
					fCreditTotalCount--;
				}
				else if (originalValue == 0m)
				{
					fCreditTotalCount++;
				}
			}

			CreditTotalInfo.RefreshBinding();
		}

		public void CalculatePageDebitTotal()
		{
			fPageDebitTotal = 0.0m;
			fPageDebitTotalCount = 0;

			foreach (Statement statement in Statements)
			{
				if (statement.AS_DebitCredit == Core.Constants.DebitCredit.Debit)
				{
					fPageDebitTotal += statement.AS_Amount;
					fPageDebitTotalCount++;
				}
			}

			PageDebitTotalInfo.RefreshBinding();
		}

		public void CalculatePageCreditTotal()
		{
			fPageCreditTotal = 0.0m;
			fPageCreditTotalCount = 0;

			foreach (Statement statement in Statements)
			{
				if (statement.AS_DebitCredit == Core.Constants.DebitCredit.Credit)
				{
					fPageCreditTotal += statement.AS_Amount;
					fPageCreditTotalCount++;
				}
			}

			PageCreditTotalInfo.RefreshBinding();
		}

		internal void CalculateBalanceAmount()
		{
			BalanceAmount = OpeningStatementBalance + fCreditTotal - fDebitTotal - AB_StatementBalance;
			BalanceAmountInfo.RefreshBinding();
		}

		public void CalculatePageBalanceAmount()
		{
			fPageBalanceAmount = OpeningStatementBalance + fPageCreditTotal - fPageDebitTotal - AB_StatementBalance;
			PageBalanceAmountInfo.RefreshBinding();
		}

		#endregion

		#region Overrides

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			SetSequenceNo();
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			if (saveSucceeded)
			{
				fDirectTransactions = null;
			}
		}

		#endregion

		#region Utiltiy Methods

		public string GetStatementTypeFromTransactionType(string transType)
		{
			switch (transType)
			{
				case Enterprise.ZArchitecture.Core.TransactionTypes.Payment:
					return "CHQ";
				case Enterprise.ZArchitecture.Core.TransactionTypes.ReceiptBatch:
					return "RCB";
				case Enterprise.ZArchitecture.Core.TransactionTypes.Transfer:
					return "TRF";
				default:
					return transType;
			}
		}

		public bool IsPageExistForThisCollection(int pageNum)
		{
			return IsPageExistForThisCollection(Statements, pageNum);
		}

		public bool IsPageExistForThisCollection(StatementCollection statementsToSearch, int pageNum)
		{
			bool pageExist = false;

			if (pageNum == 0)
			{
				pageExist = true;
			}
			else
			{
				foreach (Statement statement in statementsToSearch)
				{
					if (statement.AS_PageNumber == pageNum)
					{
						pageExist = true;
						break;
					}
				}
			}

			return pageExist;
		}

		#endregion

		#region Implementation

		ZDecimal fPageDebitTotal;
		ZDecimal fPageDebitTotalCount;
		ZDecimal fPageCreditTotal;
		ZDecimal fPageCreditTotalCount;
		ZDecimal fPageBalanceAmount;
		ZShort fMaxPage;
		StatementCollection fStatements;
		UnreconciledStatementCollectionView fUnreconciledStatements;
		ReconciledStatementCollectionView fReconciledStatements;
		ZDecimal fOpeningStatementBalance;
		ZDecimal fStatementBalance;
		ZDecimal fDebitTotal;
		ZInt fDebitTotalCount;
		ZDecimal fCreditTotal;
		ZInt fCreditTotalCount;
		ZDecimal fBalanceAmount;
		CodeDescriptionPairList fAB_Type_List;
		CodeDescriptionPairList fAB_DebitCredit_List;
		ZDateTime fStatementDateFilter;
		ZDecimal fAmountFilter;
		ZString fDebitCreditFilter;
		ZString fTypeFilter;
		ZShort fCurrentPageFilter;
		ZString fChequeReferenceFilter;

		/// <summary>
		/// Returns the next Sequence no. for the Statements
		/// </summary>
		/// <param name="date"></param>
		/// <returns></returns>
		int GetMaxSequenceNo(ZDateTime date)
		{
			int maxSequence = 0;

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			ZQuery filter = new ZQuery(AccStatementSchema.AS_StatementDate, SQLComparisonOperator.Equal, date.Date);
			filter.OrderBy = AccStatementSchema.Constants.AS_Sequence + " DESC ";

			Statement readOnlyStatement = newFactory.LoadTop1(typeof(Statement), filter) as Statement;
			if (readOnlyStatement != null && !readOnlyStatement.AS_Sequence.IsEmpty)
			{
				maxSequence = readOnlyStatement.AS_Sequence + 1;
			}

			return maxSequence;
		}

		void SetSequenceNo()
		{
			if (!AB_LastStatementDate.IsEmpty)
			{
				int maxSequenceNo = GetMaxSequenceNo(AB_LastStatementDate);
				foreach (Statement statement in Statements)
				{
					if (!statement.IsInDatabase)
					{
						statement.AS_Sequence = maxSequenceNo++;
					}
				}
			}
		}

		public void SetMaxPageNo(ZShort pageNo)
		{
			fMaxPage = fMaxPage > pageNo ? fMaxPage : pageNo;
		}

		#region Optimization

		public void SetTotalValuesForTheDate()
		{
			fMaxPage = (ZShort)0;
			fDebitTotal = 0.0m;
			fDebitTotalCount = 0;
			fCreditTotal = 0.0m;
			fCreditTotalCount = 0;

			if (!AB_LastStatementDate.IsEmpty)
			{
				var totalStatementsFilter = new StatementCollection(this, new ZQuery(AccStatementSchema.AS_StatementDate, SQLComparisonOperator.Equal, AB_LastStatementDate)).CompleteFilter;
				foreach (Statement statement in Factory.Load<Statement>(totalStatementsFilter))
			{
					fMaxPage = fMaxPage > statement.AS_PageNumber ? fMaxPage : statement.AS_PageNumber;
					if (statement.AS_DebitCredit == Core.Constants.DebitCredit.Credit)
				{
						fCreditTotal += statement.AS_Amount;
					fCreditTotalCount++;
				}
					if (statement.AS_DebitCredit == Core.Constants.DebitCredit.Debit)
				{
						fDebitTotal += statement.AS_Amount;
					fDebitTotalCount++;
				}
			}
			}

			CalculateBalanceAmount();

			MaxPageInfo.RefreshBinding();
			CreditTotalInfo.RefreshBinding();
			DebitTotalInfo.RefreshBinding();
			BalanceAmountInfo.RefreshBinding();
		}

		#endregion
		#endregion
	}
}

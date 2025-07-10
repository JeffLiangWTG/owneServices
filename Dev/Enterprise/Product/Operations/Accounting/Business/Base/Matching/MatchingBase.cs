using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.CashAdvance;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.Overpayment;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.Security;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Base.Matching
{
	public enum OverlapResult
	{
		NoneFound,
		CurrentContainsFound,
		CurrentDoesNotContainFound
	}

	[UniversalDataContext(DataContextType.AccountingMatching)]
	public abstract partial class MatchingBase : NonPersistentBusinessObject, IObsoleteValidation
	{
		protected readonly ReceiptPaymentBase ReceiptPaymentDetail;

		#region Constructor

		protected MatchingBase(BusinessObjectFactory factory, bool isLoadedFromGUI = true)
			: base(factory)
		{
			this.IsLoadedFromGUI = isLoadedFromGUI;
			InitializeTaxFrameworkDependenciesIfApplicable();
		}

		protected MatchingBase(BusinessObjectFactory factory, ReceiptPaymentBase receiptPaymentDetail, bool isLoadFromGUI = true)
			: this(factory, (TransactionHeader)receiptPaymentDetail, isLoadFromGUI)
		{
			this.ReceiptPaymentDetail = receiptPaymentDetail;
			IMatching receiptPaymentMatching = receiptPaymentDetail;
			if (receiptPaymentMatching != null)
			{
				MatchDate = receiptPaymentDetail.CurrentMatchingDate.IsValid
					? receiptPaymentDetail.CurrentMatchingDate :
						receiptPaymentMatching.PostDate;
			}

			// Only add SavingEventHandler if ReceiptPaymentDetail is a payment. 
			// In this case Factory.Save will be called in the BaseReceiptPaymentForm instead of in MatchingBase
			if (!IsNotMatchingPayment)
			{
				factory.Saving += new BusinessObjectFactory.SavingEventHandler(SetMatchGroupNumberAndMatchDateOnSaving);
			}
		}

		protected MatchingBase(BusinessObjectFactory factory, Journal journal, bool isLoadFromGUI = true)
			: this(factory, (TransactionHeader)journal, isLoadFromGUI)
		{
		}

		MatchingBase(BusinessObjectFactory factory, TransactionHeader transaction, bool isLoadFromGUI = true)
			: base(factory)
		{
			this.IsLoadedFromGUI = isLoadFromGUI;
			IMatching matchingTransaction = transaction as IMatching;
			if (matchingTransaction != null)
			{
				matchingTransaction.OSPartialPaymentAmount = matchingTransaction.OSOutstandingAmount;
				matchingTransaction.OSPartialPaymentAmountInfo.ValueChanged += new EventHandler(OSPartialPaymentAmountInfo_ValueChanged);
				MatchedTransactions.AddTransactionThatMustBeMatched(matchingTransaction);
				MatchDate = matchingTransaction.PostDate;
			}
			BalanceInfo.RefreshBinding();

			// The following should be done for both Receipt and Payment Matching
			PrimaryOrganisationForGUINotification = transaction.AH_OH;
			PrimaryOrganization = transaction.AH_OH;

			IsInitializedFromPayment = true;
			InitializeTaxFrameworkDependenciesIfApplicable();
		}

		void InitializeTaxFrameworkDependenciesIfApplicable()
		{
			if (GlbCompany.CurrentCompany.IsEnabledForTaxFrameworkConfiguration(Factory))
			{
				taxRealisationEnabler_constructorInitalizedOnly = TaxFrameworkObjectFactory.GetTaxRealisationEnabler(Factory);
			}

			if (GlbCompany.CurrentCompany.IsAPWithholdTaxEnabled())
			{
				withholdingJournalCreationManager_constructorInitializedOnly = TaxFrameworkObjectFactory.GetAPJournalBasedWHTAmountCalculator(Factory);
			}
		}

		bool IsLoadedFromGUI;

		ITaxRealisationEnabler TaxRealisationEnabler => taxRealisationEnabler_constructorInitalizedOnly;
		ITaxRealisationEnabler taxRealisationEnabler_constructorInitalizedOnly;

		IWithholdingJournalCreationManager WithholdingJournalCreationManager => withholdingJournalCreationManager_constructorInitializedOnly;
		IWithholdingJournalCreationManager withholdingJournalCreationManager_constructorInitializedOnly;

#if DEBUG
		void SetIsLoadedFromGUIForTest(bool value)
		{
			IsLoadedFromGUI = value;
		}

		internal bool GetIsLoadedFromGUIForTest()
		{
			return IsLoadedFromGUI;
		}

		public void SubstituteTaxRealisationEnabler_ForTestOnly(ITaxRealisationEnabler replacement)
		{
			TaxFrameworkObjectFactory.SubstituteTaxRealisationEnabler_ForTestOnly(Factory, replacement);
			taxRealisationEnabler_constructorInitalizedOnly = replacement;
		}
		public ITaxRealisationEnabler TaxRealisationEnabler_ExposedForTestOnly => TaxRealisationEnabler;
		public void SubstituteWithholdingJournalCreationManager_ForTestOnly(IWithholdingJournalCreationManager replacement)
		{
			TaxFrameworkObjectFactory.SubstituteAPJournalBasedWHTAmountCalculator_ForTestOnly(Factory, replacement);
			withholdingJournalCreationManager_constructorInitializedOnly = replacement;
		}
		public IWithholdingJournalCreationManager WithholdingJournalCreationManager_ExposedForTestOnly => WithholdingJournalCreationManager;
#endif

		protected bool IsInitializedFromPayment { get; set; }

		#endregion

		public ZBool IsPrimaryOrgValidAndNonEmpty
		{
			get { return PrimaryOrganization.IsValid; }
		}

		public ZBool IsReceiptPaymentDetailChequeNumberInvalid
		{
			get { return ReceiptPaymentDetail != null && ReceiptPaymentDetail.AH_ChequeOrReferenceInfo.HasErrors(); }
		}

		public ZBool IsOverPaymentAllowedInThisMatchingSession
		{
			get { return IsOverPaymentAllowedInThisMatchingSessionCore; }
		}

		protected virtual ZBool IsOverPaymentAllowedInThisMatchingSessionCore
		{
			get { return true; }
		}

		public virtual SecurityCheckpoint CheckpointForNewMiscTransaction(ZString transactionType) =>
			  LedgerType == LedgerTypes.AccountsPayable    && transactionType == TransactionTypes.Overpayment        ? Env.Security.PayablesNewMatchTransactionsOverpaymentType
			: LedgerType == LedgerTypes.AccountsPayable    && transactionType == TransactionTypes.Discount           ? null
			: LedgerType == LedgerTypes.AccountsPayable    && transactionType == TransactionTypes.Journal            ? null
			: LedgerType == LedgerTypes.AccountsPayable    && transactionType == TransactionTypes.ExchangeDifference ? null
			: LedgerType == LedgerTypes.AccountsReceivable && transactionType == TransactionTypes.Overpayment        ? Env.Security.ReceivablesNewMatchTransactionsOverpaymentType
			: LedgerType == LedgerTypes.AccountsReceivable && transactionType == TransactionTypes.Discount           ? null
			: LedgerType == LedgerTypes.AccountsReceivable && transactionType == TransactionTypes.Journal            ? null
			: LedgerType == LedgerTypes.AccountsReceivable && transactionType == TransactionTypes.ExchangeDifference ? null
			: throw new ArgumentException(FormattableString.Invariant($"Combination of Ledger '{LedgerType}' and Transaction Type '{transactionType}' is not valid for Matching."), nameof(transactionType));

		/// <summary>
		/// <p>Flag indicates that there were a problem during saving data to database after objects in factory were already modified.</p>
		/// <p>Since we cannot restore state of those objects, we set this flag to indicate that this instance should not be used.</p>
		/// </summary>
		public ZBool Corrupted { get; private set; }

		public void ExcludeTransactionsFromUnmatchedList(IMatchingCollection excludedTransactions)
		{
			UnmatchingExcludedTransactions.AddRange(excludedTransactions);
			ReloadUnmatchedTransactions();
		}

		/// <summary>
		/// Add an IMatching to the transactions to be matched.  
		/// </summary>
		public void AddIMatching(IMatching transaction)
		{
			MatchedTransactions.Add(transaction);
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			if (!saveSucceeded)
			{
				Corrupted = true;
			}
			base.OnFactorySaved(saveSucceeded);
		}

		#region Filtering

		public abstract MatchingFilterBusinessObject MatchingFilterBizO { get; }
		protected MatchingFilterBusinessObject matchingFilterBizO;

		public virtual CashAdvanceFilterBusinessObjectForMatchingBase CashAdvanceFilter => null;

		#endregion

		#region Database Loading

		#region ReloadSettlementOrgTransactions

		public void ReloadSettlementOrgTransactions()
		{
			if (MatchingFilterBizO.IsDBReloadRequired)
			{
				ReloadUnmatchedTransactions();
				ReloadCashAdvanceRequests();
			}
		}

		void ReloadUnmatchedTransactions()
		{
			UnmatchedTransactions.RemoveAll();
			LoadTransactionsMatchingTheFilterCore();
			MoveFilterMatchingTransactionsToOutstanding();
		}

		void ReloadCashAdvanceRequests()
		{
			if (CanCashAdvanceRequestBeMatched)
			{
				UnmatchedCashAdvanceRequests.RemoveAll();
				LoadCashAdvanceRequests();
			}
		}

		#endregion

		#region GetTransactionOverlapResultAndMoveIfRequired

		public OverlapResult GetTransactionOverlapResultAndMoveIfRequired()
		{
			OverlapResult resultToReturn = OverlapResult.CurrentDoesNotContainFound;
			if (MatchingLoadedBizOs.Length == 0)
			// there are no results
			{
				resultToReturn = OverlapResult.NoneFound;
				//UnmatchedTransactions.RemoveAll();
			}
			else if (UnmatchedTransactions.ContainsBusinessObjects(MatchingLoadedBizOs))
			// the results are contained within current outstanding transactions
			{
				resultToReturn = OverlapResult.CurrentContainsFound;
			}
			else // the results partially overlap with current outstanding transactions
			{
				// This should only be the case if user clicks find immediately after matching
				UnmatchedTransactions.RemoveAll();
				MoveFilterMatchingTransactionsToOutstanding();
			}
			return resultToReturn;
		}

		#endregion

		#region LoadTransactionsMatchingTheFilter

		public void LoadTransactionsMatchingTheFilter()
		{
			LoadTransactionsMatchingTheFilterCore();
		}

		#endregion

		#region LoadTransactionsMatchingTheFilterCore

		public event EventHandler<UserMessageEventArgs> OnLoadFilterWithTooManyParameters;

		void RaiseOnLoadFilterWithTooManyParameters(object sender, string failureReason)
		{
			OnLoadFilterWithTooManyParameters?.Invoke(sender, new UserMessageEventArgs(failureReason));
		}

		void LoadTransactionsMatchingTheFilterCore(ZQuery additionalFilter = null)
		{
			var filter = MatchingFilterBizO.FilterForDBReload;
			var transactionPKs = new List<ZGuid>();

			MatchedTransactions
				.OfType<IMatching>()
				.OfType<BusinessObject>()
				.ForEach(bizO => transactionPKs.Add(bizO.PK));

			UnmatchingExcludedTransactions
				.OfType<IMatching>()
				.OfType<BusinessObject>()
				.ForEach(bizO => transactionPKs.Add(bizO.PK));

			if (transactionPKs.Any())
			{
				filter.AllowTableValuedParameters = true;
				filter.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, transactionPKs);
			}

			if (additionalFilter != null)
			{
				filter.AddToFilter(additionalFilter);
			}

			if (!filter.IsNoResultQuery)
			{
				filter.MaximumRows++;
			}

			FoundMoreThanMaxResultRows = false;

			filter.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);

			if (filter.Params.Length >= ZSQLInFilter.MAXIMUM_SUPPORTS_COUNT_FOR_PARAMETER)
			{
				var message = Res.GetString("50f6568f-8d25-4f55-be69-f8aa351aa939", "The transaction search has too many parameters ({0}), the search supports a maximum of {1} parameters. Reduce the number of parameters and try again.",
					filter.Params.Length, ZSQLInFilter.MAXIMUM_SUPPORTS_COUNT_FOR_PARAMETER);
				RaiseOnLoadFilterWithTooManyParameters(this, message);
				return;
			}

			LoadedTransactions.Load(filter);
			if (LoadedTransactions.Count == filter.MaximumRows)
			{
				FoundMoreThanMaxResultRows = true;
				LoadedTransactions.Remove(LoadedTransactions[LoadedTransactions.Count - 1]);
			}

			fMatchingLoadedBizOs = PostFilterTransactions(LoadedTransactions.ToArray());

			if (LoadedTransactions.Any())
			{
				Factory.RegisterTransactionsForLoadingWHTAmounts(true, LoadedTransactions.GetPKs().ToArray());
			}
		}

		protected virtual BusinessObject[] PostFilterTransactions(BusinessObject[] arrayToFilter)
		{
			return arrayToFilter;
		}

		#endregion

		#region LoadedTransactions

		TransactionHeaderCollection LoadedTransactions
		{
			get
			{
				if (fLoadedTransasctions == null)
				{
					fLoadedTransasctions = new TransactionHeaderCollection(Factory);
				}

				return fLoadedTransasctions;
			}
		}

		TransactionHeaderCollection fLoadedTransasctions;

		public ZBool FoundMoreThanMaxResultRows
		{ get; private set; }

		#endregion

		#region MatchingLoadedBizOs

		public BusinessObject[] MatchingLoadedBizOs
		{
			get
			{
				if (fMatchingLoadedBizOs == null)
				{
					fMatchingLoadedBizOs = Array.Empty<BusinessObject>();
				}

				return fMatchingLoadedBizOs;
			}
		}

		BusinessObject[] fMatchingLoadedBizOs;

		#endregion

		#region CashAdvanceFilterCore

		public bool CanCashAdvanceRequestBeMatched => CashAdvanceFilter != null &&
														CanCashAdvanceRequestBeMatchedCore;

		protected virtual bool CanCashAdvanceRequestBeMatchedCore => false;

		public void LoadCashAdvanceRequests()
		{
			if (CanCashAdvanceRequestBeMatched)
			{
				var filter = CashAdvanceFilter.Filter;

				// exclude cash advance requests in the MatchCashAdvanceRequests Collection
				var cahPKs = new List<ZGuid>();
				foreach (var cah in MatchedCashAdvanceRequests.Values)
				{
					cahPKs.Add(cah.PK);
				}
				if (cahPKs.Any())
				{
					filter.AddToFilter(JoinCondition.And, AccCashAdvanceRequestHeaderSchema.PK, SQLComparisonOperator.NotEqual, cahPKs);
				}

				UnmatchedCashAdvanceRequests.Load(filter);
			}
		}

		public void ClearCashAdvanceRequests()
		{
			UnmatchedCashAdvanceRequests.RemoveAll();
		}

		protected virtual bool IsCashAdvanceFunctionalityEnabled => false;

		#endregion

		#endregion

		#region Miscellaneous Matching Transactions

		#region AddMiscellaneousTransaction

		public void AddMiscellaneousTransaction(TransactionHeader header)
		{
			MatchedTransactions.Add(header);
			if (header is Discount)
			{
				DiscountBizO = (Discount)header;
				fDiscountTmp = null;
			}
			else if (header is Overpayment)
			{
				OverpaymentBizO = (Overpayment)header;
				fOverpaymentTmp = null;
			}
			else if (header is ExchangeDifference)
			{
				ExchangeDifferenceBizO = (ExchangeDifference)header;
				fExchangeDiffTmp = null;
			}
			else if (header is Journal)
			{
				BankFeeBizO = (Journal)header;
				BankFeeTmp = null;
			}
		}

		#endregion

		#region GetMiscellaneousTransaction

		// If there is no Transaction of the type stored, returns a new one
		// Otherwise returns the stored one
		public TransactionHeader GetMiscellaneousTransaction(ZString transactionType, ZDecimal amounmt)
		{
			OrgHeader primaryOrgBizO = Factory.Load<OrgHeader>(PrimaryOrganization);
			TransactionHeader miscTransToReturn = null;
			if (primaryOrgBizO != null)
			{
				switch (transactionType)
				{
					case ZArchitecture.Core.TransactionTypes.Overpayment:
						if (OverpaymentTmp != null && !OverpaymentTmp.IsDeleted)
						{
							miscTransToReturn = OverpaymentTmp;
						}
						else
						{
							miscTransToReturn = MiscTransCreator.CreateOverpayment(amounmt, 1M, primaryOrgBizO);
							PrepareNewMiscTransaction(miscTransToReturn);
							fOverpaymentTmp = (Overpayment)miscTransToReturn;
						}
						break;

					case ZArchitecture.Core.TransactionTypes.ExchangeDifference:
						if (ExchangeDiffTmp != null && !ExchangeDiffTmp.IsDeleted)
						{
							miscTransToReturn = ExchangeDiffTmp;
						}
						else
						{
							miscTransToReturn = MiscTransCreator.CreateExchangeDifference(amounmt, primaryOrgBizO);
							PrepareNewMiscTransaction(miscTransToReturn);
							fExchangeDiffTmp = (ExchangeDifference)miscTransToReturn;
						}
						break;

					case ZArchitecture.Core.TransactionTypes.Discount:
						if (DiscountTmp != null && !DiscountTmp.IsDeleted)
						{
							miscTransToReturn = DiscountTmp;
						}
						else
						{
							miscTransToReturn = MiscTransCreator.CreateDiscount(amounmt, primaryOrgBizO);
							PrepareNewMiscTransaction(miscTransToReturn);
							fDiscountTmp = (Discount)miscTransToReturn;
						}
						break;

					case ZArchitecture.Core.TransactionTypes.Journal:
						if (BankFeeTmp != null && !BankFeeTmp.IsDeleted)
						{
							miscTransToReturn = BankFeeTmp;
						}
						else
						{
							miscTransToReturn = MiscTransCreator.CreateBankFee(primaryOrgBizO);
							PrepareNewMiscTransaction(miscTransToReturn);
							BankFeeTmp = (Journal)miscTransToReturn;
						}
						break;

					default:
						break;
				}
			}

			if (miscTransToReturn != null)
			{
				miscTransToReturn.AH_PostDate = MiscellaneousTransactionPostTime;
			}

			return miscTransToReturn;
		}

		public ZDateTime MiscellaneousTransactionPostTime
		{
			get
			{
				return MatchDate <= ZDateTime.Today ? MatchDate : ZDateTime.Today;
			}
		}

		public TransactionHeader GetMiscellaneousTransaction(ZString transactionType)
		{
			switch (transactionType)
			{
				case ZArchitecture.Core.TransactionTypes.Overpayment:
				case ZArchitecture.Core.TransactionTypes.ExchangeDifference:
				case ZArchitecture.Core.TransactionTypes.Discount:
					return GetMiscellaneousTransaction(transactionType, -Balance);

				case ZArchitecture.Core.TransactionTypes.Journal:
					return GetMiscellaneousTransaction(transactionType, 0m);

				default:
					break;
			}
			return null;
		}

		#endregion

		#region ContainMiscellaneousTransaction

		public ZBool ContainsMiscellaneousTransaction(TransactionHeader header)
		{
			return (ExchangeDifferenceBizO == header || DiscountBizO == header || OverpaymentBizO == header || BankFeeBizO == header);
		}

		#endregion

		#region DeleteMiscTransaction

		public void DeleteMiscTransaction(TransactionHeader header)
		{
			if (DiscountBizO != null && header == DiscountBizO)
			{
				DeleteDiscount();
			}
			else if (ExchangeDifferenceBizO != null && header == ExchangeDifferenceBizO)
			{
				DeleteExchangeDiff();
			}
			else if (OverpaymentBizO != null && header == OverpaymentBizO)
			{
				DeleteOverpayment();
			}
			else if (BankFeeBizO != null && header == BankFeeBizO)
			{
				DeleteBankFeeJournal();
			}
		}

		#endregion

		#region DeleteCachedMiscTransactions

		public void DeleteCachedMiscTransactions()
		{
			DeleteDiscount();
			DeleteExchangeDiff();
			DeleteOverpayment();
			DeleteBankFeeJournal();
			if (fOverpaymentTmp != null)
			{
				fOverpaymentTmp.DeleteFromDB();
			}
			if (fExchangeDiffTmp != null)
			{
				fExchangeDiffTmp.DeleteFromDB();
			}
			if (fDiscountTmp != null)
			{
				fDiscountTmp.DeleteFromDB();
			}
			if (BankFeeTmp != null)
			{
				BankFeeTmp.DeleteFromDB();
			}

			fOverpaymentTmp = null;
			fExchangeDiffTmp = null;
			fDiscountTmp = null;
			BankFeeTmp = null;
		}

		#endregion

		void PrepareNewMiscTransaction(TransactionHeader miscTransaction)
		{
			// hook the LocalPartialPaidAmount so that balance recalculates
			miscTransaction.AH_OutstandingAmountInfo.ValueChanged += new EventHandler(OSPartialPaymentAmountInfo_ValueChanged);
			// pass the MatchingBase object into the MiscellaneousTransaction
			if (miscTransaction is IMiscellaneousTransaction)
			{
				((IMiscellaneousTransaction)miscTransaction).MatchingBizO = this;
			}
		}

		#region Deleting Miscellaneous Transactions

		protected void DeleteDiscount()
		{
			if (DiscountBizO != null)
			{
				if (MatchedTransactions.Contains(DiscountBizO))
				{
					MatchedTransactions.Remove(DiscountBizO);
				}
				DiscountBizO.DeleteFromDB();
				DiscountBizO = null;
			}
		}

		void DeleteOverpayment()
		{
			if (OverpaymentBizO != null)
			{
				if (MatchedTransactions.Contains(OverpaymentBizO))
				{
					MatchedTransactions.Remove(OverpaymentBizO);
				}
				OverpaymentBizO.DeleteFromDB();
				OverpaymentBizO = null;
			}
		}

		protected void DeleteExchangeDiff()
		{
			if (ExchangeDifferenceBizO != null)
			{
				if (MatchedTransactions.Contains(ExchangeDifferenceBizO))
				{
					MatchedTransactions.Remove(ExchangeDifferenceBizO);
				}
				ExchangeDifferenceBizO.DeleteFromDB();
				ExchangeDifferenceBizO = null;
			}
		}

		protected void DeleteBankFeeJournal()
		{
			if (BankFeeBizO != null)
			{
				if (MatchedTransactions.Contains(BankFeeBizO))
				{
					MatchedTransactions.Remove(BankFeeBizO);
				}
				BankFeeBizO.DeleteFromDB();
				BankFeeBizO = null;
			}
		}

		void ClearCachedMiscTransactions()
		{
			OverpaymentBizO = null;
			DiscountBizO = null;
			ExchangeDifferenceBizO = null;
			BankFeeBizO = null;
			fOverpaymentTmp = null;
			fDiscountTmp = null;
			fExchangeDiffTmp = null;
			BankFeeTmp = null;
		}

		#endregion

		#region Discount, Overpayment, ExchangeDiff Properties

		#region Public Getters

		// Current DSC, OVP, EXX are ones that have been saved
		public Discount DiscountCurrent
		{
			get { return fDiscountBizO; }
		}

		public Overpayment OverpaymentCurrent
		{
			get { return fOverpaymentBizO; }
		}

		public ExchangeDifference ExchangeDiffCurrent
		{
			get { return fExchangeDifferenceBizO; }
		}

		public Journal BankFeeCurrent
		{
			get { return fBankFeeBizO; }
		}

		// Temp DSC, OVP, EXX are ones that have not been saved
		// They are deleted when the Current DSC, OVP, EXX are set
		public Discount DiscountTmp
		{
			get { return fDiscountTmp; }
		}

		public Overpayment OverpaymentTmp
		{
			get { return fOverpaymentTmp; }
		}

		public ExchangeDifference ExchangeDiffTmp
		{
			get { return fExchangeDiffTmp; }
		}

		public Journal BankFeeTmp { get; private set; }

		#endregion

		#region Protected Properties

		#region Current DSC, OVP, EXX

		#region DiscountBizO

		protected Discount DiscountBizO
		{
			get { return fDiscountBizO; }
			set
			{
				fDiscountBizO = value;
				DiscountAmountInfo.RefreshBinding();
				if (DiscountBizObjChanged != null) // Note: set in constructor of MatchingForm
				{
					DiscountBizObjChanged(this, EventArgs.Empty);
				}
			}
		}

		Discount fDiscountBizO;

		#endregion

		#region ExchangeDifferenceBizO

		protected ExchangeDifference ExchangeDifferenceBizO
		{
			get { return fExchangeDifferenceBizO; }
			set
			{
				fExchangeDifferenceBizO = value;
				ExchangeDifferenceAmountInfo.RefreshBinding();
				if (ExchangeDiffBizObjChanged != null) // Note: set in constructor of MatchingForm
				{
					ExchangeDiffBizObjChanged(this, EventArgs.Empty);
				}
			}
		}

		ExchangeDifference fExchangeDifferenceBizO;

		#endregion

		#region OverpaymentBizO

		protected Overpayment OverpaymentBizO
		{
			get { return fOverpaymentBizO; }
			set
			{
				fOverpaymentBizO = value;
				OSOverpaymentAmountInfo.RefreshBinding();
				LocalOverpaymentAmountInfo.RefreshBinding();
				ExchangeRateAmountInfo.RefreshBinding();
				if (OverpaymentBizObjChanged != null) // Note: set in constructor of MatchingForm
				{
					OverpaymentBizObjChanged(this, EventArgs.Empty);
				}
			}
		}
		Overpayment fOverpaymentBizO;

		#endregion

		#region BankFeeBizO

		protected Journal BankFeeBizO
		{
			get { return fBankFeeBizO; }
			set
			{
				fBankFeeBizO = value;
				BankFeeAmountInfo.RefreshBinding();
				if (BankFeeBizObjChanged != null) // Note: set in constructor of MatchingForm
				{
					BankFeeBizObjChanged(this, EventArgs.Empty);
				}
			}
		}

		Journal fBankFeeBizO;

		#endregion

		#endregion

		#region Temp DSC, OVP, EXX

		Discount fDiscountTmp;

		Overpayment fOverpaymentTmp;

		ExchangeDifference fExchangeDiffTmp;

		#endregion

		#endregion

		#region Properties Bound To GUI Controls

		#region ExchangeRateAmount

		[DecimalPlaces(nameof(OSExchangeRateDecimals))]
		public ZDecimal ExchangeRateAmount
		{
			get
			{
				ZDecimal amountToReturn = 1M;
				if (OverpaymentBizO != null)
				{
					amountToReturn = OverpaymentBizO.AH_ExchangeRate;
				}

				return amountToReturn;
			}
		}

		public ZPropertyInfo ExchangeRateAmountInfo
		{
			get { return GetZPropertyInfo(nameof(ExchangeRateAmount)); }
		}

		#endregion

		#region ForeignCurrency

		[MaxLength(3)]
		[List("Currencies")]
		public ZString ForeignCurrency
		{
			get
			{
				ZString currencyToReturn = LocalCurrency;
				if (OverpaymentBizO != null)
				{
					currencyToReturn = OverpaymentBizO.AH_RX_NKTransactionCurrency;
				}
				return currencyToReturn;
			}
		}

		public ZPropertyInfo ForeignCurrencyInfo
		{
			get { return GetZPropertyInfo(nameof(ForeignCurrency)); }
		}

		#endregion

		#region Currency

		[List("Currencies")]
		public ZString LocalCurrency
		{
			get { return GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency; }
		}

		public ZPropertyInfo LocalCurrencyInfo
		{
			get { return GetZPropertyInfo(nameof(LocalCurrency)); }
		}

		#endregion

		#region OSOverpaymentAmount

		[DecimalPlaces(nameof(OSDecimals))]
		public ZDecimal OSOverpaymentAmount
		{
			get
			{
				ZDecimal amountToReturn = 0M;
				if (OverpaymentBizO != null)
				{
					amountToReturn = OverpaymentBizO.AH_OSTotal;
				}

				return amountToReturn;
			}
		}

		public ZPropertyInfo OSOverpaymentAmountInfo
		{
			get { return GetZPropertyInfo(nameof(OSOverpaymentAmount)); }
		}

		#endregion

		#region LocalOverpaymentAmount
		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal LocalOverpaymentAmount
		{
			get
			{
				ZDecimal amountToReturn = 0M;
				if (OverpaymentBizO != null)
				{
					amountToReturn = OverpaymentBizO.AH_InvoiceAmount;
				}

				return amountToReturn;
			}
		}

		public ZPropertyInfo LocalOverpaymentAmountInfo
		{
			get { return GetZPropertyInfo(nameof(LocalOverpaymentAmount)); }
		}

		#endregion

		#region DiscountAmount

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal DiscountAmount
		{
			get
			{
				ZDecimal amountToReturn = 0M;
				if (DiscountBizO != null)
				{
					amountToReturn = DiscountBizO.AH_InvoiceAmount;
				}

				return amountToReturn;
			}
		}

		public ZPropertyInfo DiscountAmountInfo
		{
			get { return GetZPropertyInfo(nameof(DiscountAmount)); }
		}

		#endregion

		#region ExchangeDifferenceAmount

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal ExchangeDifferenceAmount
		{
			get
			{
				if (ExchangeDifferenceBizO != null)
				{
					return ExchangeDifferenceBizO.AH_InvoiceAmount;
				}

				return 0M;
			}
		}

		public ZPropertyInfo ExchangeDifferenceAmountInfo
		{
			get { return GetZPropertyInfo(nameof(ExchangeDifferenceAmount)); }
		}

		#endregion

		#region BankFeeAmount

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal BankFeeAmount
		{
			get
			{
				ZDecimal amountToReturn = 0m;
				if (BankFeeBizO != null)
				{
					amountToReturn = BankFeeBizO.AH_InvoiceAmount;
				}

				return amountToReturn;
			}
		}

		public ZPropertyInfo BankFeeAmountInfo
		{
			get { return GetZPropertyInfo(nameof(BankFeeAmount)); }
		}

		#endregion

		#region Currencies

		public RefCurrencyCollection Currencies
		{
			get
			{
				if (fCurrencies == null)
				{
					fCurrencies = new RefCurrencyCollection(Factory);
				}

				return fCurrencies;
			}
		}

		RefCurrencyCollection fCurrencies;

		#endregion

		#endregion

		#region MiscTransCreator

		protected virtual MiscellaneousTransactionCreator MiscTransCreator
		{
			get
			{
				if (fMiscTransCreator == null)
				{
					fMiscTransCreator = GetNewMiscellaneousTransactionCreator(Factory);
				}

				return fMiscTransCreator;
			}
		}

		protected abstract MiscellaneousTransactionCreator GetNewMiscellaneousTransactionCreator(BusinessObjectFactory factory);
		protected MiscellaneousTransactionCreator fMiscTransCreator;

		#endregion

		#endregion

		#region DecimalPlaces

		public int LocalDecimals => GlbCompany.CurrentCompany.GetLocalDecimals();
		public int LocalExchangeRateDecimals => GlbCompany.CurrentCompany.ExchangeRateDecimalPlaces;
		public int OSDecimals => OverpaymentBizO?.OSCurrencyDecimals ?? LocalDecimals;
		public int OSExchangeRateDecimals => OverpaymentBizO?.ExchangeRateDecimalPlaces ?? LocalExchangeRateDecimals;

		#endregion

		#endregion

		#region GUI Stuff + Inter-Grid Movement

		public void MakeChequeNumEditableOnReceiptPaymentDetail()
		{
			((IMatching)ReceiptPaymentDetail).ChequeOrReference_ReadOnly = false;
		}

		#region MoveFilterMatchingTransactionsToOutstanding

		void MoveFilterMatchingTransactionsToOutstanding()
		{
			UnmatchedTransactions.AddRange(MatchingLoadedBizOs);
		}

		#endregion

		#region MoveFilterMatchingTransactionsToSelected

		public void MoveFilterMatchingTransactionsToSelected()
		{
			// don't want to call event handler for each transaction added
			MatchedTransactions.CountChanged -= new CollectionCountChangedEventHandler(fTransactions_CountChanged);
			try
			{
				MoveFromUnmatchToMatch(MatchingLoadedBizOs);
			}
			finally
			{
				MatchedTransactions.CountChanged += new CollectionCountChangedEventHandler(fTransactions_CountChanged);
			}

			fTransactions_CountChanged(MatchedTransactions, new CollectionCountChangedEventArgs(true, null));

			RaiseTransactionSelectedChanged();
		}

		#endregion

		#region MoveAllFromUnmatchToMatch

		public void MoveAllFromUnmatchToMatch()
		{
			MoveFromUnmatchToMatch(UnmatchedTransactions.ToArray());
		}

		public void MoveAllCashAdvanceFromUnmatchToMatch()
		{
			MoveCashAdvanceFromUnmatchToMatch(UnmatchedCashAdvanceRequests.Select(c => c).ToArray());
		}

		#endregion

		#region MoveAllFromMatchToUnmatch

		public void MoveAllFromMatchToUnmatch()
		{
			DeleteAllBalancingJournals();
			MoveFromMatchToUnmatch(MatchedTransactions.ToArray());
		}

		public void DeleteAllBalancingJournals()
		{
			foreach (var balancingARJournal in BalancingARJournals)
			{
				balancingARJournal.RelatedJournal?.Delete();
				if (balancingARJournal.IsInDatabase)
				{
					BalancingARJournals.RemoveFromRelationship(balancingARJournal);
				}
			}
			BalancingARJournals.DeleteAll();
			foreach (var balancingAPJournal in BalancingAPJournals)
			{
				balancingAPJournal.RelatedJournal?.Delete();
				if (balancingAPJournal.IsInDatabase)
				{
					BalancingAPJournals.RemoveFromRelationship(balancingAPJournal);
				}
			}
			BalancingAPJournals.DeleteAll();
		}

		#endregion

		#region MoveFromUnmatchToMatch

		public void MoveFromUnmatchToMatch(BusinessObject[] selectedBizOs)
		{
			Dictionary<BusinessObject, ZDecimal> selected = new Dictionary<BusinessObject, ZDecimal>();
			foreach (BusinessObject bizO in selectedBizOs)
			{
				if (CheckIfTransactionIsPaidViaWebService(bizO))
				{
					bizO.AddRowError(Res.GetString("b0d29efb-6f43-4eb9-bcf4-a90c8d7c2e6e", "This transaction cannot be matched as it is paid through Invoice Payment Web Service."));
				}

				selected.Add(bizO, ZDecimal.Zero);
			}

			MoveFromUnmatchToMatch(selected);
		}

		public void MoveCashAdvanceFromUnmatchToMatch(CashAdvanceRequestHeader[] selectedCAHs)
		{
			if (selectedCAHs.Any())
			{
				var matchedCAHs = new Dictionary<Journal, CashAdvanceRequestHeader>();
				CashAdvanceMatchingHandler.CreateCashAdvancePaidOrReceivedMatchingJournals(MatchDate
																							, selectedCAHs
																							, AddJournalsToMatchTransactionsCollection
																							, (pcah, err) => throw new CannotGenerateCashAdvanceJournalException(pcah, err));
				
				MoveFromUnmatchToMatch(matchedCAHs.Keys.ToArray());

				void AddJournalsToMatchTransactionsCollection(CashAdvanceRequestHeader requestHeader, Journal journal)
				{
					if (!MatchedCashAdvanceRequests.ContainsKey(journal))
					{
						UnmatchedCashAdvanceRequests.Remove(requestHeader);
						matchedCAHs.Add(journal, requestHeader);
						MatchedCashAdvanceRequests.Add(journal, requestHeader);
						journal.ReadOnly = true;
						AddToBalancingJournals(journal);
					}
				}
			}
		}

		public bool CheckIfTransactionIsPaidViaWebService(BusinessObject bizO)
		{
			if (AccountingConfigurationRegistry.Instance.EnableInvoicePaymentWebService.Value)
			{
				var transaction = bizO as InvoicingBase;
				if (transaction != null)
				{
					return transaction.IsInvoicePaidByPaymentWebservice();
				}
			}

			return false;
		}

		public void MoveFromUnmatchToMatch(Dictionary<BusinessObject, ZDecimal> selectedToMatch)
		{
			using (MatchedTransactions.SuspendBalanceCalculation())
			{
				AddTransactionsToLineTotalPaidAmountPostedCalculator(selectedToMatch);

				ZBool shouldUseRemoveRange = selectedToMatch.Count > UnmatchedTransactions.Count * 0.005;

				if (shouldUseRemoveRange)
				{
					UnmatchedTransactions.RemoveRange(selectedToMatch.Keys);
				}

				foreach (BusinessObject trans in selectedToMatch.Keys)
				{
					if (!shouldUseRemoveRange)
					{
						UnmatchedTransactions.Remove(trans);
					}

					IMatching matchTrans = (IMatching)trans;
					ZDecimal matchAmount = selectedToMatch[trans];
					matchTrans.OSPartialPaymentAmount = matchAmount.IsEmpty ? matchTrans.OSOutstandingAmount : matchAmount;
					matchTrans.OSPartialPaymentAmountInfo.ValueChanged -= new EventHandler(OSPartialPaymentAmountInfo_ValueChanged);
					matchTrans.OSPartialPaymentAmountInfo.ValueChanged += new EventHandler(OSPartialPaymentAmountInfo_ValueChanged);
				}

				using (SuspendTransactions_CountChanged())
				{
					MatchedTransactions.AddRange(selectedToMatch.Keys);
				}

				foreach (BusinessObject trans in selectedToMatch.Keys)
				{
					if (WithholdingJournalCreationManager != null)
					{
						if (WithholdingJournalCreationManager.ShouldAPWithholdingJournalsBeCreated(Factory))
						{
							var withholdingJournals = WithholdingJournalCreationManager.CreateAPWithholdingJournalsIfApplicable(trans as IMatching, Factory, MatchDate.Date);
							foreach (var journal in withholdingJournals)
							{
								journal.ReadOnly = true;
								AddToBalancingJournals(journal);
							}

							using (SuspendTransactions_CountChanged())
							{
								MatchedTransactions.AddRange(withholdingJournals);
							}
						}
					}

					TransactionHeader header = trans as TransactionHeader;
					if (header != null)
					{
						MatchingValidation validation = header.Validation as MatchingValidation;
						if (validation != null)
						{
							validation.ValidateOSPartialPaymentAmount();
							validation.ValidateOSOutstandingAmount();
						}
					}
				}
			}

			RaiseTransactionSelectedChanged();
			MatchedTransactions.ValidatePaymentApproval();
			fTransactions_CountChanged(MatchedTransactions, new CollectionCountChangedEventArgs(false, null));
			ValidateMatchDate();
		}

		void AddTransactionsToLineTotalPaidAmountPostedCalculator(Dictionary<BusinessObject, ZDecimal> selectedToMatch)
		{
			InvoicingBase.LineTotalPaidAmountPostedCalculator lineTotalPaidAmountPostedCalculator = InvoicingBase.LineTotalPaidAmountPostedCalculator.GetOrCreateNewInstance(Factory);

			List<ZGuid> pks = new List<ZGuid>();
			foreach (BusinessObject trans in selectedToMatch.Keys)
			{
				pks.Add(trans.PK);
			}

			lineTotalPaidAmountPostedCalculator.AddNewElements(pks);
		}

		#endregion

		#region MoveFromMatchToUnmatch

		// This will reset the OSPaidAmount member on IMatching to the OSOutstandingAmount
		// Also screens the selected elements for current OVP, DSC and EXX
		public void MoveFromMatchToUnmatch(BusinessObject[] selectedBizOs)
		{
			ZBool containsCurrentDSC = false;
			ZBool containsCurrentOVP = false;
			ZBool containsCurrentEXX = false;
			ZBool containsCurrentJNL = false;

			using (MatchedTransactions.SuspendBalanceCalculation())
			{
				List<BusinessObject> transactionsToAdd = new List<BusinessObject>();

				foreach (BusinessObject bizO in selectedBizOs)
				{
					if (DiscountBizO != null && bizO == DiscountBizO)
					{
						containsCurrentDSC = true;
					}
					else if (OverpaymentBizO != null && bizO == OverpaymentBizO)
					{
						containsCurrentOVP = true;
					}
					else if (ExchangeDifferenceBizO != null && bizO == ExchangeDifferenceBizO)
					{
						containsCurrentEXX = true;
					}
					else if (BankFeeBizO != null && bizO == BankFeeBizO)
					{
						containsCurrentJNL = true;
					}
					else
					{
						IMatching matchTrans = (IMatching)bizO;

						ISupportMatchingOfMyLines matchTransAsISupportMatchingOfMyLines = bizO as ISupportMatchingOfMyLines;
						if (matchTransAsISupportMatchingOfMyLines != null)
						{
							matchTransAsISupportMatchingOfMyLines.ResetAmounts();
						}

						TransactionHeader header = bizO as TransactionHeader;
						if (header != null)
						{
							MatchingValidation validation = header.Validation as MatchingValidation;
							if (validation != null)
							{
								validation.ValidateOSOutstandingAmount();
							}
						}

						if (!MatchedTransactions.MustTransactionBeMatched(bizO))
						{
							transactionsToAdd.Add(bizO);
						}
					}
				}

				bool shouldContinue = true;
				HashSet<APJournal> journalsToDelete = null;
				if (WithholdingJournalCreationManager != null)
				{
					(shouldContinue, journalsToDelete) = DeleteWithholdingJournalsIfApplicable(transactionsToAdd);
				}

				if (shouldContinue)
				{
					// don't want to call event handler for each transaction added
					MatchedTransactions.CountChanged -= new CollectionCountChangedEventHandler(fTransactions_CountChanged);
					try
					{
						MatchedTransactions.RemoveRange(transactionsToAdd);
						if (journalsToDelete != null && journalsToDelete.Count > 0)
						{
							journalsToDelete.ForEach(x => BalancingAPJournals.Delete(x));
							MatchedTransactions.RemoveRange(journalsToDelete);
							transactionsToAdd.RemoveAll(x => journalsToDelete.Contains(x));
						}

						using (SuspendTransactions_CountChanged())
						{
							UnmatchedTransactions.AddRange(transactionsToAdd);
						}
					}
					finally
					{
						MatchedTransactions.CountChanged += new CollectionCountChangedEventHandler(fTransactions_CountChanged);
					}
				}
			}

			fTransactions_CountChanged(MatchedTransactions, new CollectionCountChangedEventArgs(false, null));

			if (containsCurrentDSC)
			{
				DeleteDiscount();
			}
			if (containsCurrentOVP)
			{
				DeleteOverpayment();
			}
			if (containsCurrentEXX)
			{
				DeleteExchangeDiff();
			}
			if (containsCurrentJNL)
			{
				DeleteBankFeeJournal();
			}

			RaiseTransactionSelectedChanged();
			ValidateMatchDate();
		}

		(bool shouldContinue, HashSet<APJournal> journalsToDelete) DeleteWithholdingJournalsIfApplicable(List<BusinessObject> transactionsToAdd)
		{
			var journalsToDelete = new HashSet<APJournal>();
			List<IMatching> applicableTransactions = new List<IMatching>();
			transactionsToAdd.OfType<IMatching>().ForEach(x => applicableTransactions.Add(x));
			if (AskUserForConfirmation != null && applicableTransactions.Any() && WithholdingJournalCreationManager.CheckIfAnyTransactionIsLinkedToWithholdingJournal(applicableTransactions))
			{
				var message = Res.GetString("84820196-B32C-40BB-B9AB-174EB40F08E8", "This operation will delete Withholding AP Journal(s). Do you want to proceed?");
				var eventArg = new UserQueryEventArgs(message);
				AskUserForConfirmation(this, eventArg);
				if (!eventArg.Response)
				{
					return (false, journalsToDelete);
				}

				foreach (IMatching transaction in applicableTransactions)
				{
					journalsToDelete.UnionWith(WithholdingJournalCreationManager.GetWithholdingJournalsToDelete(transaction));
				}
			}

			return (true, journalsToDelete);
		}
		#endregion

		public bool IsNewBalancingJournal(Journal journal) => journal != null && !journal.IsInDatabase && !journal.IsPaymentBasisWithholdingJournal;

		public bool IsNewCashAdvanceMatchingJournal(Journal journal) => journal != null && !journal.IsInDatabase && journal.IsCashAdvanceJournal;

		protected virtual ZQuery GetFilter()
		{
			return new ZQuery();
		}

		#endregion

		#region Matching

		public bool MatchAndClearTransactions()
		{
			return MatchAndClearTransactionsCore();
		}

		public bool MatchAndClearTransactionsWithSaveErrorHandling()
		{
			bool result = false;
			try
			{
				result = MatchAndClearTransactionsCore();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				result = false;
				ZExceptionReporting.HandleSaveException(ex);
			}
			return result;
		}

		protected virtual bool MatchAndClearTransactionsCore()
		{
			bool result = Match();
			if (result)
			{
				if (!IsMatchingPaymentOrReceipt && !DoNotSaveFactoryOnMatching)
				{
					OnSuccessfulMatching();
				}
			}
			return result;
		}

		public void RemoveAllInMatchedTransactions()
		{
			MatchedTransactions.RemoveAllFromAllCollections();
		}

		public ZString MatchingErrorsForGUINotificationOnly
		{
			get
			{
				return matchingErrorsForGUINotificationOnly;
			}
			set
			{
				matchingErrorsForGUINotificationOnly = value;
				MatchingErrorsForGUINotificationOnlyInfo.RefreshBinding();
			}
		}
		ZString matchingErrorsForGUINotificationOnly;

		public ZPropertyInfo MatchingErrorsForGUINotificationOnlyInfo
		{
			get { return GetZPropertyInfo(nameof(MatchingErrorsForGUINotificationOnly), Res.GetString("f18942d4-90e8-4a7e-9f82-31f329c6979a", "Matching Errors")); }
		}

		/// <summary>
		/// Match the transactions in the collection
		/// </summary>
		protected virtual ZBool Match()
		{
			ZBool matchResult = false;

			PrematchValidation();

			if (!MatchingErrorsForGUINotificationOnlyInfo.HasErrors() && TaxRealisationEnabler != null)
			{
				var errorMessage = TaxRealisationEnabler.RealiseTaxIfApplicable(MatchedTransactions, WithholdingJournalCreationManager, MatchDate.Date);
				if (!string.IsNullOrEmpty(errorMessage))
				{
					MatchingErrorsForGUINotificationOnlyInfo.AddError(errorMessage);
				}
			}

			if (!MatchingErrorsForGUINotificationOnlyInfo.HasErrors())
			{
				GenerateDynamicTransactions();

				if (MatchingErrorsForGUINotificationOnlyInfo.HasErrors())
				{
					matchResult = false;
				}
				else
				{
					FullyPayTransactions();
					GenerateMatchLinkRows();
					GenerateTransLinePayRecords();
					LinkCashAdvancePaidOrReceivedJournalToCashAdvance();

					var suspenders = FunctionalitySuspender.SuspendCollection(MatchLinks.Cast<AccTransactionMatchLink>(),
											x => x.MatchDateIsNotEmptyValidationSuspender.GetSuspender());
					try
					{
						MatchLinks.RunPreSaveValidation();
					}
					finally
					{
						FunctionalitySuspender.ResumeCollection(suspenders);
					}

					matchResult = !MatchLinks.HasErrors();
				}
				if (matchResult)
				{
					var receiptAndPayments = MatchLinks.Select(x => x.TransactionHeader).Where(y => y.AH_TransactionType == TransactionTypes.Payment ||
																									y.AH_TransactionType == TransactionTypes.Receipt);
					foreach (var receiptAndPayment in receiptAndPayments)
					{
						if (!receiptAndPayment.IsInDatabase)
						{
							var loadedReceiptAndPayment = Factory.Load<ReceiptPaymentBase>(receiptAndPayment.PK);
							loadedReceiptAndPayment.TransactionNumberSet += new EventHandler(SetEXXDescription_TransactionNumberSet);
						}
					}

					if (IsNotMatchingPayment)
					{
						if (!DoNotSaveFactoryOnMatching)
						{
							Factory.Saving += new BusinessObjectFactory.SavingEventHandler(SetMatchGroupNumberAndMatchDateOnSaving);
							try
							{
								using (MatchedTransactions.SuspendHeaderAmountsRecalculation())
								using (DynamicTransactions.SuspendHeaderAmountsRecalculation())
								using (UnmatchedTransactions.SuspendHeaderAmountsRecalculation())
								using (ServiceContainerSuspenderHelper.GetApplyRevenueRecognitionDateSuspender(Factory))
								{
									Factory.Save();
									LogDeveloperExceptionWhenAP_AmountIsZero();
								}
							}
							finally
							{
								Factory.Saving -= new BusinessObjectFactory.SavingEventHandler(SetMatchGroupNumberAndMatchDateOnSaving);
							}
						}
						else if (Factory.IsInTransaction)
						{
							SetMatchGroupNumberAndMatchDateOnSaving(Factory);
						}
						else if (Factory.HasContext(BusinessContext.UniversalTransactionBatchImport))
						{
							Factory.Saving += new BusinessObjectFactory.SavingEventHandler(SetMatchGroupNumberAndMatchDateOnSaving);
						}
					}
				}
				else
				{
					MatchLinks.RemoveAndDeleteAll();
				}
			}

			if (!matchResult)
			{
				RecordErrorForReportingWithCriticalValidation();
			}
			return matchResult;
		}

		void PrematchValidation()
		{
			MatchingErrorsForGUINotificationOnlyInfo.ClearAllNotifications();

			var orgBizO = Factory.Load<OrgHeader>(PrimaryOrganization);

			if (MatchedTransactions.Balance != 0m)
			{
				MatchingErrorsForGUINotificationOnlyInfo.AddError(Res.GetString("1B791C8B-0BE5-4fe4-A9D9-55E03A40A8AE", "Balance of transaction match result is not zero."));
			}
			if (MatchedTransactions.ContainsFullyPaidTransaction)
			{
				MatchingErrorsForGUINotificationOnlyInfo.AddError(Res.GetString("E6604B99-23B7-448c-BD80-3562FA6D1E48", "Transaction for match is already fully paid. It might have been matched in other forms."));
			}
			if (!MatchedTransactions.ContainsTransactionFromSpecifiedOrg(orgBizO))
			{
				MatchingErrorsForGUINotificationOnlyInfo.AddError(Res.GetString("F2AA2BFD-CA65-4659-BD59-DCAAB372F1BB", "None of matched transactions link with Primary Organization."));
			}
		}

		public bool DoNotSaveFactoryOnMatching { get; set; }

		// Now clear Match Transaction grid and the miscellaneous amounts
		void OnSuccessfulMatching()
		{
			if (MatchingSuccessful != null)
			{
				MatchingSuccessful(this, EventArgs.Empty); // tells form to set MatchButton to readonly
			}
			using (SuspendTransactions_CountChanged())
			{
				MatchedTransactions.RemoveAll();
			}

			var aRJournals = BalancingARJournals.ToArray();
			foreach (var journal in aRJournals)
			{
				BalancingARJournals.RemoveFromRelationship(journal);
			}

			var aPJournals = BalancingAPJournals.ToArray();
			foreach (var journal in aPJournals)
			{
				BalancingAPJournals.RemoveFromRelationship(journal);
			}

			MatchLinks.RemoveAll();
			ClearCachedMiscTransactions();
			WithholdingJournalCreationManager?.ResetCache();
		}

		#region GenerateDynamicTransactions

		/// <summary>
		/// Dynamically generates required transactions
		/// </summary>
		protected virtual void GenerateDynamicTransactions()
		{
			if (!PrimaryOrganization.IsEmpty && PrimaryOrganization.IsValid)
			{
				if (DynamicTransactionCreatorClearingJournal.IsJournalShouldBeCreated)
				{
					fDynamicTransactions = new DynamicTransactionCreatorClearingJournal(MatchedTransactions).CreateTransactions();
				}
				else
				{
					OrganizationBalanceSheet balanceSheet = new OrganizationBalanceSheet(PrimaryOrganization, LedgerTypeCore, Factory);
					balanceSheet.AddIMatchingCollection(MatchedTransactions);
					fDynamicTransactions = balanceSheet.CreateDynamicTransactions();
				}
				if (fDynamicTransactions != null)
				{
					foreach (IMatching transaction in fDynamicTransactions)
					{
						if (!transaction.IsInDatabaseIncludingChildren)
						{
							transaction.PostDate = MatchDate;
						}
					}
					ValidateDynamicTransactions();
				}
			}
		}

		#endregion

		#region FullyPayTransactions

		protected virtual void FullyPayTransactions()
		{
			MatchedTransactions.Pay(MatchDate);
			DynamicTransactions.Pay(MatchDate);
		}

		#endregion

		#region GenerateMatchLinkRows

		protected virtual void GenerateMatchLinkRows()
		{
			MatchLinks.AddRange(MatchedTransactions.GenerateMatchLinkRows());
			MatchLinks.AddRange(DynamicTransactions.GenerateMatchLinkRows());
		}

		#endregion

		#region GenerateTransLinePayRecords

		void GenerateTransLinePayRecords()
		{
			var transactionPKs = new List<ZGuid>(from t in MatchedTransactions where t is ISupportMatchingOfMyLines select t.PK);
			InvoicingBase.LineTotalPaidAmountPostedCalculator.GetOrCreateNewInstance(Factory).AddNewElements(transactionPKs);

			foreach (IMatching transaction in MatchedTransactions)
			{
				var transactionAsISupportMatchingOfMyLines = transaction as ISupportMatchingOfMyLines;

				if (transactionAsISupportMatchingOfMyLines != null &&
					(!transactionAsISupportMatchingOfMyLines.LineTotalPaidAmount.IsEmpty || !transactionAsISupportMatchingOfMyLines.LineTotalPaidAmountPosted.IsEmpty))
				{
					// GenerateTransLinePayRecords will trigger the loading of lines on the transaction.
					// Only load the lines if you're paying at line level, otherwise you'll cause issues with high memory usage
					// and poor performance when you have tens of thousand of lines in memory on the matching screen
					TransactionMatchLink matchLink = findMatchLink(((BusinessObject)transaction).PK);
					transactionAsISupportMatchingOfMyLines.GenerateTransLinePayRecords(matchLink.PK);
				}
			}
		}

		TransactionMatchLink findMatchLink(ZGuid transactionPK)
		{
			return (TransactionMatchLink)MatchLinks.Find(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, transactionPK))[0];
		}

		#endregion

		#region LinkCashAdvancePaidOrReceivedJournalToCashAdvance

		void LinkCashAdvancePaidOrReceivedJournalToCashAdvance()
		{
			if (CanCashAdvanceRequestBeMatched)
			{
				foreach (var kvp in MatchedCashAdvanceRequests)
				{
					var journal = kvp.Key;
					journal.AH_CAH_CashAdvanceRequestHeader = kvp.Value.PK;
				}
			}
		}

		#endregion

		#region Properties for Matching

		#region Balance

		/// <summary>
		///  The balance of all the transactions to be matched
		/// </summary>
		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal Balance
		{
			get { return BalanceCore; }
		}

		protected virtual ZDecimal BalanceCore
		{
			get { return MatchedTransactions.Balance; }
		}

		public ZPropertyInfo BalanceInfo
		{
			get { return GetZPropertyInfo(nameof(Balance)); }
		}

		#endregion

		#region PrimaryOrganisationForGUINotification

		[List("MatchingFilterBizO.PrimaryOrgHeaders")]
		public ZGuid PrimaryOrganisationForGUINotification
		{
			get { return fPrimaryOrganisationForGUINotification; }
			set
			{
				fPrimaryOrganisationForGUINotification = value;
				PrimaryOrganisationForGUINotificationInfo.RefreshBinding();
			}
		}
		ZGuid fPrimaryOrganisationForGUINotification;

		public ZPropertyInfo PrimaryOrganisationForGUINotificationInfo
		{
			get { return GetZPropertyInfo(nameof(PrimaryOrganisationForGUINotification)); }
		}

		protected bool PrimaryOrganisationForGUINotification_ReadOnly
		{
			get { return IsInitializedFromPayment; }
		}

		#endregion

		#region PrimaryOrganization

		public ZGuid PrimaryOrganization
		{
			get { return primaryOrganization; }
			set
			{
				SetNonPersistentPropertyValue(PrimaryOrganizationInfo, ref primaryOrganization, value);
				if (!IsValidationSuspended)
				{
					ValidatePrimaryOrganization();
					ValidateMatchDate();
				}
				if (!PrimaryOrganizationInfo.HasErrors() || PrimaryOrganization.IsEmpty)
				{
					MatchingFilterBizO.PrimaryOrganization = value;

					UnmatchedTransactions.RemoveAll();
					MatchedTransactions.RemoveAll();
					ClearCashAdvanceRequests();
					BalancingARJournals.ForEach(x => x.RelatedJournal?.Delete());
					BalancingAPJournals.ForEach(x => x.RelatedJournal?.Delete());
					BalancingARJournals.DeleteAll();
					BalancingAPJournals.DeleteAll();
					DeleteCachedMiscTransactions();
					WithholdingJournalCreationManager?.DeleteAllWithholdingJournals();

					if (IsLoadedFromGUI)
					{
						LoadTransactionsMatchingTheFilterCore(GetFilter());
						LoadCashAdvanceRequests();
					}

					MoveFilterMatchingTransactionsToOutstanding();
				}
			}
		}

		ZGuid primaryOrganization;

		public ZPropertyInfo PrimaryOrganizationInfo
		{
			get { return GetZPropertyInfo(nameof(PrimaryOrganization)); }
		}

		#endregion

		#region Receipt/Payment Posting Flag

		public ZBool AllowAlteringOfPayment
		{
			get { return AllowAlteringOfPaymentCore && !IsReceiptPaymentDetailInDatabase; }
		}

		ZBool IsReceiptPaymentDetailInDatabase => ReceiptPaymentDetail != null && ReceiptPaymentDetail.IsInDatabase;

		protected virtual ZBool AllowAlteringOfPaymentCore
		{
			get { return IsMatchingPaymentOrReceipt; }
		}

		public ZBool IsNotMatchingPayment
		{
			get { return IsNotMatchingPaymentCore; }
		}

		protected virtual ZBool IsNotMatchingPaymentCore
		{
			get { return ReceiptPaymentDetail == null || ReceiptPaymentDetail is Receipt; }
		}

		public ZBool IsMatchingPaymentOrReceipt
		{
			get { return IsMatchingPaymentOrReceiptCore; }
		}

		protected virtual ZBool IsMatchingPaymentOrReceiptCore
		{
			get { return ReceiptPaymentDetail != null; }
		}

		#endregion

		#region MatchDate

		public ZDateTime MatchDate
		{
			get
			{
				return fMatchDate;
			}
			set
			{
				if (fMatchDate != value.Date)
				{
					fMatchDate = value.Date;
					if (!IsValidationSuspended)
					{
						ValidateMatchDate();
					}
					if (OverpaymentBizO != null)
					{
						OverpaymentBizO.AH_PostDate = MiscellaneousTransactionPostTime;
					}
					if (DiscountBizO != null)
					{
						DiscountBizO.AH_PostDate = MiscellaneousTransactionPostTime;
					}
					if (ExchangeDifferenceBizO != null)
					{
						ExchangeDifferenceBizO.AH_PostDate = MiscellaneousTransactionPostTime;
					}
					if (BankFeeBizO != null)
					{
						BankFeeBizO.AH_PostDate = MiscellaneousTransactionPostTime;
					}
				}
				MatchDateInfo.RefreshBinding();
			}
		}
		ZDateTime fMatchDate = ZDateTime.Today;

		public ZPropertyInfo MatchDateInfo
		{
			get { return GetZPropertyInfo(nameof(MatchDate)); }
		}

		protected bool MatchDate_ReadOnly
		{
			get { return false; }
		}

		#endregion

		#region LedgerType

		public ZString LedgerType
		{
			get { return LedgerTypeCore; }
		}

		protected abstract ZString LedgerTypeCore { get; }

		#endregion

		#endregion

		#region Balancing Journals

		public void AddToBalancingJournals(Journal journal)
		{
			if (journal.AH_Ledger == LedgerTypes.AccountsPayable)
			{
				BalancingAPJournals.Add((APJournal)journal);
			}
			else if (journal.AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				BalancingARJournals.Add((ARJournal)journal);
			}
		}

		public APJournalCollection BalancingAPJournals
		{
			get
			{
				if (fBalancingAPJournals == null)
				{
					fBalancingAPJournals = new APJournalCollection(Factory, this, new AdhocCollectionRelationship(typeof(APJournal)));
				}
				return fBalancingAPJournals;
			}
		}
		APJournalCollection fBalancingAPJournals;

		public ARJournalCollection BalancingARJournals
		{
			get
			{
				if (fBalancingARJournals == null)
				{
					fBalancingARJournals = new ARJournalCollection(Factory, this, new AdhocCollectionRelationship(typeof(ARJournal)));
				}
				return fBalancingARJournals;
			}
		}
		ARJournalCollection fBalancingARJournals;

		public void CreateAndAddJournalsForMatching(Dictionary<BusinessObject, ZDecimal> transactionsToMatch)
		{
			foreach (Journal journal in BalancingAPJournals.Where(x => IsBalancingJournal(x)))
			{
				transactionsToMatch.Add(CopyJournalWithOppositeAmount(journal), ZDecimal.Zero);
			}
			foreach (Journal journal in BalancingARJournals.Where(x => IsBalancingJournal(x)))
			{
				transactionsToMatch.Add(CopyJournalWithOppositeAmount(journal), ZDecimal.Zero);
			}
		}

		static bool IsBalancingJournal(Journal journal) => !journal.IsPaymentBasisWithholdingJournal;

		public Journal CopyJournalWithOppositeAmount(Journal journalToCopy)
		{
			return CopyJournalWithOppositeAmount(journalToCopy, true);
		}

		public Journal CopyJournalWithOppositeAmount(Journal journalToCopy, bool linkRelatedJournals)
		{
			Journal journal;
			if (journalToCopy.AH_Ledger == LedgerTypes.AccountsPayable)
			{
				journal = Factory.New<APJournal>();
			}
			else
			{
				journal = Factory.New<ARJournal>();
			}
			journal.IsAutoGenerated = journalToCopy.IsAutoGenerated;
			journal.AH_TransactionCategory = journalToCopy.AH_TransactionCategory;
			journal.AH_OH = journalToCopy.AH_OH;
			journal.AH_InvoiceDate = journalToCopy.AH_InvoiceDate;
			journal.AH_PostDate = journalToCopy.AH_PostDate;
			journal.AH_DueDate = journalToCopy.AH_DueDate;
			journal.AH_Desc = journalToCopy.AH_Desc;
			journal.AH_ChequeOrReference = journalToCopy.AH_ChequeOrReference;
			journal.AH_RX_NKTransactionCurrency = journalToCopy.AH_RX_NKTransactionCurrency;
			journal.DebitCreditSign = journalToCopy.GetOppositeDebitCreditSign(journalToCopy.DebitCreditSign);

			if (journalToCopy.IsRecalculateExchangeRate)
			{
				journal.AH_OSExTaxAmount = journalToCopy.AH_OSExTaxAmount;
				journal.AH_LocalExTaxAmount = journalToCopy.AH_LocalExTaxAmount;
			}
			else
			{
				journal.AH_ExchangeRate = journalToCopy.AH_ExchangeRate;
				journal.AH_OSExTaxAmount = journalToCopy.AH_OSExTaxAmount;
			}

			journal.AH_AG = journalToCopy.AH_AG;
			journal.AH_GB = journalToCopy.AH_GB;
			journal.AH_GE = journalToCopy.AH_GE;

			SubAccountHelper.CopySubAccounts(journal, journalToCopy);

			journal.RelatedInvoice = journalToCopy.RelatedInvoice;
			if (linkRelatedJournals)
			{
				journal.RelatedJournal = journalToCopy;
				journalToCopy.RelatedJournal = journal;
			}

			return journal;
		}

		/// <summary>
		/// Does a search for the copied journal created by CopyJournalWithOppositeAmount. Since there is no unique key stored
		/// when copying, this could return multiple records in rare circumstances.
		/// </summary>
		/// <param name="journal"></param>
		/// <returns></returns>
		public static IEnumerable<Journal> FindJournalsWithOppositeAmount(Journal journal)
		{
			ZQuery query = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.Journal);
			query.AddToFilter(AccTransactionHeaderSchema.AH_GC, journal.AH_GC);
			query.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, journal.PK);
			query.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, journal.AH_Ledger);
			query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCategory, journal.AH_TransactionCategory);
			query.AddToFilter(AccTransactionHeaderSchema.AH_OH, journal.AH_OH);
			query.AddToFilter(AccTransactionHeaderSchema.AH_InvoiceDate, journal.AH_InvoiceDate);
			query.AddToFilter(AccTransactionHeaderSchema.AH_PostDate, journal.AH_PostDate);
			query.AddToFilter(AccTransactionHeaderSchema.AH_DueDate, journal.AH_DueDate);
			query.AddToFilter(AccTransactionHeaderSchema.AH_Desc, journal.AH_Desc);
			query.AddToFilter(AccTransactionHeaderSchema.AH_ChequeOrReference, journal.AH_ChequeOrReference);
			query.AddToFilter(AccTransactionHeaderSchema.AH_RX_NKTransactionCurrency, journal.AH_RX_NKTransactionCurrency);
			query.AddToFilter(AccTransactionHeaderSchema.AH_ExchangeRate, journal.AH_ExchangeRate);
			query.AddToFilter(AccTransactionHeaderSchema.AH_OSTotal, -journal.AH_OSTotal);
			query.AddToFilter(AccTransactionHeaderSchema.AH_AG, journal.AH_AG);
			return journal.Factory.Load(journal.GetType(), query).Cast<Journal>();
		}

		public void RemoveAndDeleteBalancingJournalsFromMatchingTransactions(List<Journal> journals)
		{
			if (journals.Count > 0)
			{
				MoveFromMatchToUnmatch(journals.ToArray());
				foreach (Journal journal in journals)
				{
					if (!journal.IsInDatabase)
					{
						if (IsNewCashAdvanceMatchingJournal(journal))
						{
							if (MatchedCashAdvanceRequests.ContainsKey(journal))
							{
								var matchedCAH = MatchedCashAdvanceRequests[journal];
								if (matchedCAH != null)
								{
									UnmatchedCashAdvanceRequests.Add(matchedCAH);
									MatchedCashAdvanceRequests.Remove(journal);
								}
							}
							else
							{
								throw new InvalidOperationException(FormattableString.Invariant($"An {journal.AH_Ledger} {journal.AH_TransactionCategory} journal with following description exists. But the corresponding advance payment request does not exist.\r\n Description: {journal.AH_Desc}"));
							}
						}

						if (journal.RelatedJournal != null && !journal.RelatedJournal.IsDeleted)
						{
							journal.RelatedJournal.Delete();
						}
						UnmatchedTransactions.RemoveAndDelete(journal);
					}
				}
			}
		}

		public List<Journal> GetLinkedToInvoiceBalancingJournals(InvoicingBase invoice)
		{
			List<Journal> journals = new List<Journal>();
			foreach (IMatching element in MatchedTransactions)
			{
				Journal journal = element as Journal;
				if (journal != null && journal.RelatedInvoice == invoice)
				{
					journals.Add(journal);
				}
			}
			return journals;
		}

		#endregion

		#region List Properties for Matching

		#region UnmatchedTransactions

		public IMatchingCollection UnmatchedTransactions
		{
			get
			{
				if (fUnmatchedTransactions == null)
				{
					fUnmatchedTransactions = new IMatchingCollection(Factory, MatchingCollectionTypes.UnmatchedTransactions);
				}

				return fUnmatchedTransactions;
			}
		}
		IMatchingCollection fUnmatchedTransactions;

		#endregion

		#region MatchTransactions

		public IMatchingCollection MatchedTransactions
		{
			get
			{
				if (fMatchedTransactions == null)
				{
					fMatchedTransactions = new IMatchingCollection(Factory);
					fMatchedTransactions.IsSettingChequeNumReadOnlyOnAdd = true;
					fMatchedTransactions.CountChanged += new CollectionCountChangedEventHandler(fTransactions_CountChanged);
					RegisterEditableChildObject(fMatchedTransactions);
				}

				return fMatchedTransactions;
			}
		}

		IMatchingCollection fMatchedTransactions;

		#endregion

		#region Show Related Disbursement Transactions

		public virtual bool ShouldShowRelatedDisbursementTransactions => false;

		#endregion

		#region UnmatchingExcludedTransactions

		public IMatchingCollection UnmatchingExcludedTransactions
		{
			get
			{
				if (fUnmatchingExcludedTransactions == null)
				{
					fUnmatchingExcludedTransactions = new IMatchingCollection(Factory, MatchingCollectionTypes.UnmatchedTransactions);
				}

				return fUnmatchingExcludedTransactions;
			}
		}

		IMatchingCollection fUnmatchingExcludedTransactions;

		#endregion

		#region DynamicTransactions

		public IMatchingCollection DynamicTransactions
		{
			get
			{
				if (fDynamicTransactions == null)
				{
					fDynamicTransactions = new IMatchingCollection(Factory);
				}

				return fDynamicTransactions;
			}
		}

		IMatchingCollection fDynamicTransactions;

		#endregion

		#region MatchLinks

		public TransactionMatchLinkGroup MatchLinks
		{
			get
			{
				if (fMatchLinks == null)
				{
					fMatchLinks = new TransactionMatchLinkGroup(Factory);
				}

				return fMatchLinks;
			}
		}

		TransactionMatchLinkGroup fMatchLinks;

		#endregion

		#region UnmatchedCashAdvanceRequests

		public CashAdvanceRequestHeaderCollection UnmatchedCashAdvanceRequests
		{
			get
			{
				if (unmatchedCashAdvanceRequests == null)
				{
					unmatchedCashAdvanceRequests = new CashAdvanceRequestHeaderCollection(Factory);
					UnmatchedCashAdvanceRequests.SetReadOnlyIncludingChildren(true);
				}
				return unmatchedCashAdvanceRequests;
			}
		}
		CashAdvanceRequestHeaderCollection unmatchedCashAdvanceRequests;

		#endregion

		#region MatchedCashAdvanceRequests

		public Dictionary<Journal, CashAdvanceRequestHeader> MatchedCashAdvanceRequests
		{
			get
			{
				if (matchedCashAdvanceRequests == null)
				{
					matchedCashAdvanceRequests = new Dictionary<Journal, CashAdvanceRequestHeader>();
				}
				return matchedCashAdvanceRequests;
			}
		}
		Dictionary<Journal, CashAdvanceRequestHeader> matchedCashAdvanceRequests;

		#endregion

		#endregion

		#endregion

		#region Receipts And Payments

		public Receipt ReceiptDetail
		{
			get { return ReceiptPaymentDetail as Receipt; }
		}

		public Payment PaymentDetail
		{
			get { return ReceiptPaymentDetail as Payment; }
		}

		public bool IsAllPaidInTheSameCurrency(ZString currencyNK, string transactionTypeToExclude = null)
		{
			foreach (IMatching matchable in MatchedTransactions)
			{
				if ((string.IsNullOrEmpty(transactionTypeToExclude) || matchable.TransactionType != transactionTypeToExclude)
					&& !matchable.IsAllPaidInTheSameCurrency(currencyNK))
				{
					return false;
				}
			}
			return true;
		}

		public ZDecimal CalculateOSBalanceExcludingPaymentReceiptInSpecificCurrencyOnly(ZString currencyNK, ZGuid paymentOrReceiptPK)
		{
			ZDecimal result = 0;
			foreach (IMatching matchable in MatchedTransactions)
			{
				if (matchable.Identifier != paymentOrReceiptPK)
				{
					result += matchable.CalculatePaidOutstandingAmountInSpecificCurrencyOnly(currencyNK);
				}
			}
			return result;
		}

		public ZDecimal CalculateLocalBalanceExcludingReceipt()
		{
			ZDecimal localBalance = 0m;
			foreach (IMatching matchable in MatchedTransactions)
			{
				if (matchable.Identifier != ReceiptDetail.PK)
				{
					localBalance += matchable.LocalPartialPaymentAmount;
				}
			}
			return localBalance;
		}

		public virtual ZDecimal CalculateOSBalanceExcludingReceipt()
		{
			ZDecimal result = ReceiptDetail.AH_OSExTaxAmount;
			ZDecimal localBalance = CalculateLocalBalanceExcludingReceipt();
			if (localBalance != 0m && ReceiptDetail.TransactionCurrency != null)
			{
				ZDecimal exchangeRate = ReceiptDetail.ExchangeRate.Rate;
				ZString foreignCurrencyNK = ReceiptDetail.TransactionCurrency.RX_Code;
				result = Env.CurrentCompany.ExchangeRate.LocalToForeign(localBalance, exchangeRate, foreignCurrencyNK);
			}

			return result;
		}

		public ZBool TransactionsHaveSameCurrency
		{
			get
			{
				ZBool result = true;
				foreach (IMatching matchable in MatchedTransactions)
				{
					if (matchable.Identifier != PaymentDetailPK && matchable.CurrencyCode != PaymentCurrency.RX_Code)
					{
						result = false;
						break;
					}
				}
				return result;
			}
		}

		protected virtual ZGuid PaymentDetailPK
		{
			get { return PaymentDetail != null ? PaymentDetail.PK : ZGuid.Empty; }
		}

		protected virtual RefCurrency PaymentCurrency
		{
			get { return PaymentDetail != null ? PaymentDetail.TransactionCurrency : null; }
		}

		#endregion

		#region Create Misc Transactions From PaymentApprovalDetails

		public void CreateMiscTransactionsFromPaymentApprovalDetails(PaymentApprovalBase paymentApprovalDetail)
		{
			if (paymentApprovalDetail.AV_ExchangeDifference != 0 || paymentApprovalDetail.AV_Discount != 0)
			{
				using (MatchedTransactions.SuspendBalanceCalculation())
				{
					if (paymentApprovalDetail.AV_ExchangeDifference != 0 && ExchangeDifferenceBizO == null)
					{
						ExchangeDifferenceBizO = (ExchangeDifference)GetMiscellaneousTransaction(TransactionTypes.ExchangeDifference);
						ExchangeDifferenceBizO.BindableInvoiceAmount = paymentApprovalDetail.AV_ExchangeDifference;
						ExchangeDifferenceBizO.AH_OutstandingAmount = paymentApprovalDetail.AV_ExchangeDifference;
						ExchangeDifferenceBizO.BindableOSAmount = paymentApprovalDetail.AV_ExchangeDifference;
						ExchangeDifferenceBizO.IsOSPartialPaymentAmountReadOnly = true;
						ExchangeDifferenceBizO.AH_MatchStatus = paymentApprovalDetail.AV_ExxMatchStatus;
						ExchangeDifferenceBizO.AH_MatchStatusReasonCode = paymentApprovalDetail.AV_ExxMatchStatusReasonCode;
						MatchedTransactions.Add(ExchangeDifferenceBizO);
					}

					if (paymentApprovalDetail.AV_Discount != 0 && DiscountBizO == null)
					{
						DiscountBizO = (Discount)GetMiscellaneousTransaction(TransactionTypes.Discount);
						DiscountBizO.BindableInvoiceAmount = paymentApprovalDetail.AV_Discount;
						DiscountBizO.AH_OutstandingAmount = paymentApprovalDetail.AV_Discount;
						DiscountBizO.BindableOSAmount = paymentApprovalDetail.AV_Discount;
						DiscountBizO.IsOSPartialPaymentAmountReadOnly = true;
						DiscountBizO.AH_MatchStatus = paymentApprovalDetail.AV_DscMatchStatus;
						DiscountBizO.AH_MatchStatusReasonCode = paymentApprovalDetail.AV_DscMatchStatusReasonCode;
						MatchedTransactions.Add(DiscountBizO);
					}
				}
			}
		}

		#endregion

		#region Currency Summary

		public CurrencySummary CurrencySummary
		{
			get
			{
				if (fCurrencySummary == null)
				{
					fCurrencySummary = new CurrencySummary(this.MatchedTransactions);
				}
				return fCurrencySummary;
			}
		}
		CurrencySummary fCurrencySummary;

		#endregion

		public SettlementOrganisation SettlementOrganisaion
		{
			get
			{
				if (fSettlementOrganisaion == null)
				{
					fSettlementOrganisaion = new SettlementOrganisation(this);
				}
				return fSettlementOrganisaion;
			}
		}
		SettlementOrganisation fSettlementOrganisaion;

		#region Validation

		public void ValidatePrimaryOrganization()
		{
			PrimaryOrganizationInfo.ClearAllNotifications();
			PrimaryOrganisationForGUINotificationInfo.ClearAllNotifications();
			if (PrimaryOrganization.IsEmpty)
			{
				string errorMessage = Res.GetString("6fc6f67e-6480-4f0c-a0ab-740e4dc5a0c8", "Primary account cannot be empty");
				PrimaryOrganizationInfo.AddError(errorMessage);
				PrimaryOrganisationForGUINotificationInfo.AddError(errorMessage);
			}
			if (!PrimaryOrganization.IsValid)
			{
				string errorMessage = Res.GetString("efd94206-492c-4735-ab0e-8c81eb81313a", "You must select a valid Creditor or Debtor account");
				PrimaryOrganizationInfo.AddError(errorMessage);
				PrimaryOrganisationForGUINotificationInfo.AddError(errorMessage);
			}
		}

		void ValidateRows(BusinessObject[] rows)
		{
			string errorMessageForCancelledTransaction = Res.GetString("fc6aecc2-44e8-4825-9994-9b140c09c310", "You cannot choose a canceled transaction for matching");
			string openQueryClaimErrorMessage = Res.GetString("fe793883-037c-4e52-ac15-88c79c8ce83e", "This transaction cannot be matched as it is linked to an open claim");
			string openQueryClaimWarningMessage = Res.GetString("7EB9B312-3E3E-4ef0-AE92-C24B9A37EBD6", "This transaction is linked to an open claim with Allow-To-Match hold option.");

			foreach (BusinessObject row in rows)
			{
				var rowAsTransactionHeader = row as AccTransactionHeader;
				if (rowAsTransactionHeader != null && rowAsTransactionHeader.AH_IsCancelled)
				{
					row.AddRowError(errorMessageForCancelledTransaction);
				}
				if (LedgerType == LedgerTypes.AccountsPayable && row is TransactionHeader && ((TransactionHeader)row).OpenQueryClaim != null)
				{
					var claim = ((TransactionHeader)row).OpenQueryClaim;
					if (!claim.IsAllowMatch)
					{
						row.AddRowError(openQueryClaimErrorMessage);
					}
					else if (claim.AY_HoldOption == HoldOptionType.Codes.ALM)
					{
						row.AddRowWarning(openQueryClaimWarningMessage);
					}
				}
			}
		}

		public FunctionalitySuspender ValidateBalanceSuspender
		{
			get { return validateBalanceSuspender ?? (validateBalanceSuspender = new FunctionalitySuspender()); }
		}
		FunctionalitySuspender validateBalanceSuspender;

		public virtual void ValidateBalance()
		{
			BalanceInfo.ClearAllNotifications();

			ValidateZeroBalance();

			if (DynamicTransactionCreatorClearingJournal.IsJournalShouldBeCreated && AccountingConfigurationRegistry.Instance.ClearingJournalClearingAccount.Value == Guid.Empty)
			{
				BalanceInfo.AddError(Res.GetString("3A03C7EB-EF19-4D87-A304-E2A484C806B8",
					"{0} registry item can't be empty while {1} registry item is set to create Clearing Journals.",
					((IRegistryItemInternals)AccountingConfigurationRegistry.Instance.ClearingJournalClearingAccount).Location,
					((IRegistryItemInternals)AccountingConfigurationRegistry.Instance.ClearingJournalConfiguration).Location));
			}
		}

		protected virtual void ValidateZeroBalance()
		{
			if (Balance != 0 && !ValidateBalanceSuspender.IsSuspended)
			{
				BalanceInfo.AddError(Res.GetString("c332c17b-1913-4b2f-bd45-624c564ee4ca", "The balance must equal 0"));
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			MatchedTransactions.MarkAsNeedingValidationIncludingChildren();//make sure we validate transaction lines

			base.RunPreSaveValidationCore();
			ValidateBalance();
			ValidateRows(MatchedTransactions.ToArray());
			ValidatePrimaryOrganization();
			ValidateMatchDate();
			ValidateDynamicTransactions();
		}

		bool AllowFutureMatchDate
		{
			get
			{
				bool applicableTransactionInMatching =
					MatchedTransactions.Any(b => IsApplicableTransactionTypeForFuturePosting((IMatching)b));

				return
					applicableTransactionInMatching &&
					AccountingUtils.IsAllowFuturePostingRegistryEnabled &&
					AccountingUtils.DoesUserHaveFuturePostingSecurity;
			}
		}

		public static bool IsApplicableTransactionTypeForFuturePosting(IMatching txn)
		{
			switch (txn.Ledger)
			{
				case LedgerTypes.AccountsPayable:
				case LedgerTypes.AccountsReceivable:
					if (txn.TransactionType == TransactionTypes.Payment || txn.TransactionType == TransactionTypes.Receipt || txn.TransactionType == TransactionTypes.ReceiptBatch || txn.TransactionType == "UNA")
					{
						return true;
					}
					else
					{
						return false;
					}
				case LedgerTypes.CashBook:
					if (txn.TransactionType == TransactionTypes.DirectPayment || txn.TransactionType == TransactionTypes.DirectReceipt || txn.TransactionType == TransactionTypes.DDRBatch)
					{
						return true;
					}
					else
					{
						return false;
					}
				default:
					return false;
			}
		}

		public void ValidateMatchDate()
		{
			MatchDateInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(MatchDateInfo);
			if (!MatchDateInfo.HasErrors() && !MatchDate.IsValid)
			{
				MatchDateInfo.AddError(Res.GetString("e13e690d-7da5-42c3-8db4-d9f209bc4cb8", "Invalid value."));
			}
			if (!MatchDateInfo.HasErrors())
			{
				AccountingPeriodCalculator periodCalculator = new AccountingPeriodCalculator(Factory);
				AccPeriodManagement period = periodCalculator.GetPeriodManagementFromDate(MatchDate);
				if (period == null)
				{
					MatchDateInfo.AddError(Res.GetString("1ee408e6-6731-4ac2-a514-c56e794454a4", "This date does not fall into a valid accounting period’s date range.\r\nPlease go to Manage > General Ledger > Period Management > Set Up Next Accounting Year, to ensure there is an accounting period for the date you wish to post to."));
				}
				if (!MatchDateInfo.HasErrors() && (period.AM_IsGeneralLedgerClosed || period.AM_IsSubLedgerClosed))
				{
					MatchDateInfo.AddError(Res.GetString("ced9930d-b05b-4374-b9fd-214a3ae81127", "The match date cannot fall within a closed period."));
				}
				if (!MatchDateInfo.HasErrors() && MatchDate >= ZDateTime.Today.AddDays(1))
				{
					if (!AccountingUtils.IsAllowFuturePostingRegistryEnabled)
					{
						MatchDateInfo.AddError(AccountingConstants.FuturePostingErrorMessages.RegistryIsNotEnabled);
					}
					else if (!AccountingUtils.DoesUserHaveFuturePostingSecurity)
					{
						MatchDateInfo.AddError(AccountingConstants.FuturePostingErrorMessages.UserHasNoSecurity);
					}
				}
				if (!MatchDateInfo.HasErrors() && MatchDate < ZDateTime.Today && !AccountingUtils.IsUserCanChangeMatchDateToBackDate(LedgerTypeCore))
				{
					MatchDateInfo.AddError(Res.GetString("c8d805ff-5442-443d-98cf-10ce65be8ea7", "The match date cannot be in the past."));
				}
				if (!MatchDateInfo.HasErrors() && MatchedTransactions.Count > 0)
				{
					ZDateTime maxDate = MatchedTransactions[0].PostDate;
					foreach (IMatching matching in MatchedTransactions)
					{
						if (matching.PostDate > maxDate)
						{
							maxDate = matching.PostDate;
						}
					}
					if (MatchDate.Date < maxDate.Date)
					{
						MatchDateInfo.AddError(Res.GetString("82494184-d09b-49b0-86db-8e7ced446214", "Match date must be equal or greater than the max post date of the transactions matched ({0}).", maxDate.ToShortDateString()));
					}
					if (MatchedTransactions.OfType<IMatching>().Any(x => x.MatchDate > MatchDate))
					{
						MatchDateInfo.AddError(Res.GetString("826BBBBF-2571-4634-8392-5BB296105E32", @"You are attempting to use a match date which is earlier than a previous match date recorded against one of the transactions contained in this matching.
Please use a match date equal to, or later than ({0}).", MatchedTransactions.OfType<IMatching>().Max(x => x.MatchDate)));
					}
				}
			}
		}

		void ValidateDynamicTransactions()
		{
			MatchingErrorsForGUINotificationOnlyInfo.ClearAllNotifications();
			if (null != fDynamicTransactions)
			{
				foreach (IMatching transaction in fDynamicTransactions)
				{
					var branch = Factory.Load<GlbBranch>(transaction.BranchGuid);
					var department = Factory.Load<GlbDepartment>(transaction.DepartmentGuid);
					var errorMsg = GlbBranchCombinationValidation.CheckBranchDepartmentCombination(branch, department);

					if (!string.IsNullOrWhiteSpace(errorMsg))
					{
						var humanReadableName = transaction.HumanReadableName;

						if (transaction.TransactionType == TransactionTypes.Journal && transaction.TransactionCategory == Constants.TransactionCategory.Codes.Clearing)
						{
							humanReadableName = ResString.GetMultilingualString("e1a68c6e-482c-4fab-a7a3-34024f6e1dd0", "Clearing Journal");
						}

						MatchingErrorsForGUINotificationOnlyInfo.AddError(Res.GetString("eaea0ca0-ceaa-4516-8232-91970a85bd10", "Cannot create the {0} - {1}", humanReadableName, errorMsg));
					}
				}
			}
		}

		public void RefreshExistingPaymentApprovalItems(IMatching trans)
		{
			var header = trans as TransactionHeader;
			if (header != null && header.IsInDatabase)
			{
				header.ExistingPaymentApprovalItems.Reload(true);
			}
		}

		#endregion

		#region Event Handlers and Events

		public event EventHandler MatchingSuccessful;
		public event EventHandler TransactionsSelectedChanged;
		public event EventHandler DiscountBizObjChanged;
		public event EventHandler OverpaymentBizObjChanged;
		public event EventHandler ExchangeDiffBizObjChanged;
		public event EventHandler BankFeeBizObjChanged;

		protected virtual void RaiseTransactionSelectedChanged()
		{
			if (TransactionsSelectedChanged != null)
			{
				TransactionsSelectedChanged(this, EventArgs.Empty);
			}
		}

		public void OSPartialPaymentAmountInfo_ValueChanged(object sender, EventArgs e)
		{
			if (AskUserForConfirmation == null)
			{
				UpdateAndValidateBalance();
				return;
			}

			InvoicingBase transaction = sender as InvoicingBase;
			if (transaction != null && (transaction.AH_TransactionType == TransactionTypes.Invoice ||
				transaction.AH_TransactionType == TransactionTypes.CreditNote || transaction.AH_TransactionType == TransactionTypes.AdjustmentNote) &&
				Math.Abs(((IMatching)transaction).OSPartialPaymentAmount) > Math.Abs(((IMatching)transaction).OSOutstandingAmount))
			{
				Journal linkedJournal = GetJournalLinkedToInvoice(transaction);
				if (linkedJournal != null)
				{
					var messageForAmendingBalancingJournal = Res.GetString("912C7A6A-A316-491a-9B9F-ED81A9EB1FDF", "You have entered an amount that is greater than the outstanding balance of the transaction. Would you like to amend amount of linked balancing journal for the difference?");
					var eventArgs = new UserQueryEventArgs(messageForAmendingBalancingJournal);
					AskUserForConfirmation(this, eventArgs);
					if (eventArgs.Response)
					{
						linkedJournal.SetAH_OSExTaxAmountFromOverpayedInvoice(transaction);
						((IMatching)linkedJournal).OSPartialPaymentAmount = ((IMatching)linkedJournal).OSOutstandingAmount;
						((IMatching)transaction).OSPartialPaymentAmount = ((IMatching)transaction).OSOutstandingAmount;
					}
				}
				else
				{
					var messageForCreatingBalancingJournal = Res.GetString("9e711a51-8b10-4948-8604-62f65dde8c38", "You have entered an amount that is greater than the outstanding balance of the transaction. Would you like to create a balancing journal for the difference?");
					var eventArgs = new UserQueryEventArgs(messageForCreatingBalancingJournal);
					AskUserForConfirmation(this, eventArgs);
					if (eventArgs.Response)
					{
						Journal journal;
						if (transaction.AH_Ledger == LedgerTypes.AccountsPayable)
						{
							journal = Factory.New<APJournal>();
						}
						else
						{
							journal = Factory.New<ARJournal>();
						}

						journal.AH_TransactionCategory = Constants.TransactionCategory.Codes.TransactionAlreadyPaid;
						journal.AH_OH = transaction.AH_OH;

						journal.AH_Desc = string.Format(AccountingConfigurationRegistry.Instance.GetTransactionDescriptionFromCode(AccountingConstants.VoucherItemRegistryCode.BJGDM, Res.GetString("F8ACD0BB-D402-4b68-A7FE-B33052FBC9BD", "Transaction Already Paid: Trans. Num.")) + " {0}", transaction.InvoiceNumber);
						journal.AH_NumberOfSupportingDocuments = AccountingConfigurationRegistry.Instance.GetVoucherNoOfAttchmentsFromCode(AccountingConstants.VoucherItemRegistryCode.BJGDM, 0);

						if (!transaction.AH_ChequeOrReference.IsEmpty)
						{
							journal.AH_Desc += Res.GetString("5140C9C6-9007-4ca6-86AD-6F29AFC18736", ", Payment Reference: {0}", transaction.AH_ChequeOrReference);
						}

						journal.AH_RX_NKTransactionCurrency = transaction.AH_RX_NKTransactionCurrency;
						journal.AH_ExchangeRate = transaction.AH_ExchangeRate;
						journal.SetAH_OSExTaxAmountFromOverpayedInvoice(transaction);
						journal.RelatedInvoice = transaction;
						journal.AH_ChequeOrReference = transaction.AH_TransactionNum;

						#region SuppressResourceStringsCheckRegion
						CriticalValidationInfoCollectorService.GetOrCreateService(Factory)
							.AddInfoWhenAllowed(journal.PK, CriticalValidationInfoCollectorServiceKeyType.TransactionHeaderWithGLAccountThatShouldNotBeNull,
							() =>
							{
								var stringBuilder = new ZStringBuilder();
								stringBuilder.AppendLine("Transaction PK: " + transaction.PK.ToString());
								stringBuilder.AppendLine("OS Partial Payment Amount: " + ((IMatching)transaction).OSPartialPaymentAmount);
								stringBuilder.AppendLine("OS Outstanding Amount: " + ((IMatching)transaction).OSOutstandingAmount);
								stringBuilder.AppendLine("Journal PK: " + journal.PK.ToString());
								stringBuilder.AppendLine("Journal GL Account: " + journal.AH_AG);
								stringBuilder.AppendLine("Journal Transaction Category: " + journal.AH_TransactionCategory);
								stringBuilder.AppendLine("Registry for APMatchingSessionControlAccount: " + AccountingConfigurationRegistry.Instance.APMatchingSessionControlAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
								stringBuilder.AppendLine("Registry for ARMatchingSessionControlAccount: " + AccountingConfigurationRegistry.Instance.ARMatchingSessionControlAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
								stringBuilder.AppendLine("Is Validation Suspended on TransactionHeader: " + transaction.IsValidationSuspended.ToYesNoString());
								stringBuilder.AppendLine("Journal Errors: " + journal.Notifications.GetErrors().ToMessageListString());

								return stringBuilder.ToString();
							},
							CriticalValidationInfoCollectorService.CollectionFrequency.CollectAlways_ForEveryUserAndAction_THIS_CAN_IMPACT_PERFORMANCE);
						#endregion

						((IMatching)transaction).OSPartialPaymentAmount = ((IMatching)transaction).OSOutstandingAmount;

						AddToBalancingJournals(CopyJournalWithOppositeAmount(journal));
						MoveFromUnmatchToMatch(new BusinessObject[] { journal });
					}
				}
			}

			UpdateAndValidateBalance();
		}

		Journal GetJournalLinkedToInvoice(InvoicingBase invoice)
		{
			Journal result = null;
			foreach (IMatching element in MatchedTransactions)
			{
				Journal journal = element as Journal;
				if (journal != null && journal.RelatedInvoice != null && journal.RelatedInvoice == invoice)
				{
					result = journal;
					break;
				}
			}
			return result;
		}

		public EventHandler<UserQueryEventArgs> AskUserForConfirmation;

		public void UpdateAndValidateBalance()
		{
			BalanceInfo.RefreshBinding();
			ValidateBalance();
		}

		public void UpdatePaymentOSPartialPaidAmount()
		{
			UpdatePaymentOSPartialPaidAmountCore();
		}

		protected virtual void UpdatePaymentOSPartialPaidAmountCore()
		{
			if (PaymentDetail != null)
			{
				((IMatching)PaymentDetail).OSPartialPaymentAmount = PaymentDetail.AH_OSExTaxAmount;
			}
			if (ReceiptDetail != null)
			{
				((IMatching)ReceiptDetail).OSPartialPaymentAmount = -1 * ReceiptDetail.AH_OSExTaxAmount;
			}
		}

		public ZString MatchGroupNumber
		{
			get { return fMatchGroupNumber; }
		}

		ZString fMatchGroupNumber;

		protected void fTransactions_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (!IsValidationWhileMovingToMatchSuspended)
			{
				BalanceInfo.RefreshBinding();
				ValidateBalance();
				ValidateRows(MatchedTransactions.ToArray());
			}
		}

		IDisposable SuspendTransactions_CountChanged()
		{
			validationWhileMovingToMatchSuspendedCount++;
			return new DisposableAction(() => validationWhileMovingToMatchSuspendedCount--);
		}

		int validationWhileMovingToMatchSuspendedCount;

		bool IsValidationWhileMovingToMatchSuspended
		{
			get { return validationWhileMovingToMatchSuspendedCount > 0; }
		}

		void SetMatchGroupNumberAndMatchDateOnSaving(BusinessObjectFactory factory)
		{
			fMatchGroupNumber = TransactionMatchLink.MatchGroupNumberFountain.GetNextFormatted(Factory);

			MatchLinks.SetMatchGroupNumberAndMatchDate(MatchGroupNumber, MatchDate);

			SetDescriptionOnMatchLinksOnSaving();

			Factory.Saving -= new BusinessObjectFactory.SavingEventHandler(SetMatchGroupNumberAndMatchDateOnSaving);
		}

#if DEBUG
		internal
#endif
		void SetDescriptionOnMatchLinksOnSaving()
		{
			foreach (TransactionMatchLink matchLink in MatchLinks)
			{
				if (matchLink.TransactionHeader == null)
				{
					throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "matchLink.TransactionHeader object cannot be null in SetMatchGroupNumberAndMatchDateOnSaving() method.\r\nMatchLink PK:{0}, MatchLink Deleted: {1}, MatchLink Name: {2}", matchLink.PK, matchLink.IsDeleted, matchLink.HumanReadableName));
				}

				if (matchLink.TransactionHeader.AH_TransactionCreatedByMatching)
				{
					var trimmedAH_Desc = matchLink.TransactionHeader.AH_Desc.Left(AccTransactionHeader.Schema.AH_DescMaxLength - MatchGroupNumber.Length - 1);

					switch (matchLink.TransactionHeader.AH_TransactionType)
					{
						case TransactionTypes.Discount:
						case TransactionTypes.Overpayment:
						case TransactionTypes.ExchangeDifference:
						case TransactionTypes.Journal when matchLink.TransactionHeader.AH_TransactionCategory == Constants.TransactionCategory.Codes.Standard:
							matchLink.TransactionHeader.AH_Desc = trimmedAH_Desc + " " + MatchGroupNumber;
							break;

						case TransactionTypes.Journal when matchLink.TransactionHeader.AH_TransactionCategory == Constants.TransactionCategory.Codes.Clearing:
							matchLink.TransactionHeader.AH_Desc = MatchGroupNumber + " " + trimmedAH_Desc;
							break;
					}
				}
			}

			SetEXXDescriptionCore(true);
		}

		void SetEXXDescription_TransactionNumberSet(object sender, EventArgs e)
		{
			SetEXXDescriptionCore(false);
		}

		void SetEXXDescriptionCore(bool isPayRecInDatabase)
		{
			var transactions = MatchLinks.Cast<TransactionMatchLink>().Select(x => x.TransactionHeader);
			var exchangeDiff = transactions.FirstOrDefault(y => y.AH_TransactionCreatedByMatching && y.AH_TransactionType == TransactionTypes.ExchangeDifference);
			if (exchangeDiff != null)
			{
				var receiptAndPayments = transactions.Where(x => x.AH_TransactionType == TransactionTypes.Receipt || x.AH_TransactionType == TransactionTypes.Payment);
				if (isPayRecInDatabase)
				{
					if (receiptAndPayments.Any(x => !x.IsInDatabase))
					{
						return;
					}
				}
				else
				{
					receiptAndPayments = Factory.Load<ReceiptPaymentBase>(new ZQuery(AccTransactionHeaderSchema.PK, receiptAndPayments.Select(x => x.PK)));
				}

				var receiptAndPaymentNumber = string.Join(", ", receiptAndPayments
																.OrderBy(x => x.AH_TransactionNum)
																.Select(x => x.AH_TransactionType + ":" + x.AH_TransactionNum));

				if (!string.IsNullOrEmpty(receiptAndPaymentNumber))
				{
					exchangeDiff.AH_Desc = new ZString(exchangeDiff.AH_Desc + " [" + receiptAndPaymentNumber + "]").Left(AccTransactionHeader.Schema.AH_DescMaxLength);
				}
			}
		}

		public ZGuid PaymentPKOnlyForErrorReport { get; set; }

		void RecordErrorForReportingWithCriticalValidation()
		{
			var stringBuilder = new ZStringBuilder();

			var miscTransactionWithOutstandingAmount = MatchedTransactions.Cast<TransactionHeader>()
				.FirstOrDefault(x => (x.AH_TransactionType == TransactionTypes.ExchangeDifference || x.AH_TransactionType == TransactionTypes.Discount || x.AH_TransactionType == TransactionTypes.Overpayment) && x.AH_OutstandingAmount != 0);

			if (miscTransactionWithOutstandingAmount != null)
			{
				CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(miscTransactionWithOutstandingAmount.PK,
				CriticalValidationInfoCollectorServiceKeyType.NonZeroOutstandingAmountOnMiscTransaction, () =>
				{
					if (MatchedTransactions.Balance != 0)
					{
						stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, (NoResString)"Balance is not zero, Balance is {0}", MatchedTransactions.Balance));
						AppendPaymentApprovalBaseMatchDetails();
						stringBuilder.AppendLine(GetMatchingDetails("MatchingBase.Match()"));
					}
					if (MatchedTransactions.ContainsFullyPaidTransaction)
					{
						stringBuilder.AppendLine((NoResString)"Transaction already paid");
						AppendPaymentApprovalBaseMatchDetails();
						stringBuilder.AppendLine(GetMatchingDetails("MatchingBase.Match()"));
					}
					if (MatchingErrorsForGUINotificationOnlyInfo.HasErrors())
					{
						stringBuilder.AppendLine((NoResString)"There are matching errors:");
						foreach (var error in MatchingErrorsForGUINotificationOnlyInfo.GetErrors())
						{
							stringBuilder.AppendLine(error.Message);
						}
						stringBuilder.AppendLine();
					}

					var orgBizo = Factory.Load<OrgHeader>(PrimaryOrganization);
					var isContain = MatchedTransactions.ContainsTransactionFromSpecifiedOrg(orgBizo);
					stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, (NoResString)"MatchedTransactions contains transaction from primary organization: {0}", isContain ? (NoResString)"true" : (NoResString)"false"));

					return stringBuilder.ToString();
				});

				void AppendPaymentApprovalBaseMatchDetails()
				{
					if (PaymentPKOnlyForErrorReport.IsValid)
					{
						var collectorService = CriticalValidationInfoCollectorService.GetService(Factory);
						var details = collectorService?.GetInfo(PaymentPKOnlyForErrorReport, CriticalValidationInfoCollectorServiceKeyType.PaymentApprovalBaseMatchDetails);

						if (details != null)
						{
							stringBuilder.AppendLine(details);
						}
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		protected ZString GetMatchingDetails(ZString infoFrom)
		{
			ZStringBuilder stringBuilder = new ZStringBuilder();
			stringBuilder.AppendLine(infoFrom);
			var currency = Env.CurrentCompany.LocalCurrency;
			stringBuilder.AppendLine($"Current company's currency Code: {currency.Code}, Decimals: {currency.Decimals}");
			foreach (var item in MatchedTransactions)
			{
				if (item is IMatching header)
				{
					stringBuilder.AppendLine(header.GetIMatchingInfo());
				}
				else
				{
					stringBuilder.AppendLine($"MatchedTransactions contains item which is not of type IMatching. It is of type: {item.GetType().ToString()}");
				}
			}

			return stringBuilder.ToString();
		}

		#endregion

		#region Logging

		void LogDeveloperExceptionWhenAP_AmountIsZero()
		{
			if (MatchLinks.Any(x => ((TransactionMatchLink)x).AP_Amount == 0))
			{
				var matchTransaction = MatchLinks.Where(x => x.AP_Amount == 0);
				var errorMsg = new StringBuilder((NoResString)"Transactions: ");
				foreach (TransactionMatchLink tx in MatchLinks)
				{
					errorMsg.AppendLine(FormattableString.Invariant($@"Type = {tx.TransactionHeader.AH_TransactionType}, Ledger = {tx.TransactionHeader.AH_Ledger}, AP_Amount = {tx.AP_Amount}, Outstanding amount = {tx.TransactionHeader.AH_OutstandingAmount}, Bizo = {tx.MatchingTransaction.GetType()}"));
				}
				ErrorReporter.ReportOnce("MatchLink transaction AP_Amount is set to 0", errorMsg.ToString());
			}
		}

		#endregion
	}
}

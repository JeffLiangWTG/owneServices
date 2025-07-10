using System;
using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DepositBatch;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using ReceiptTypes = Enterprise.ZArchitecture.Core.ReceiptTypes;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment
{
	public partial class ARReceiptBatchPoster : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ARReceiptBatchPoster(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Events

		public delegate void DefaultBankSelectionEventHandler(DepositBatch depositSlipToPrint);
		public event DefaultBankSelectionEventHandler OnPrintDepositSlip;

		void PrintDepositSlip(DepositBatch depositSlipToPrint)
		{
			if (OnPrintDepositSlip != null)
			{
				OnPrintDepositSlip(depositSlipToPrint);
			}
		}

		public delegate ZBool OnEnquireUserHandler();
		public event OnEnquireUserHandler OnAskBeforeUpdateDescription;

		ZBool AskBeforeUpdateDescription()
		{
			if (OnAskBeforeUpdateDescription != null)
			{
				return OnAskBeforeUpdateDescription();
			}
			else
			{
				return ZBool.True;
			}
		}

		public event OnEnquireUserHandler OnAskBeforeUpdateReceiptType;

		ZBool AskBeforeUpdateReceiptType()
		{
			if (OnAskBeforeUpdateReceiptType != null)
			{
				return OnAskBeforeUpdateReceiptType();
			}
			else
			{
				return ZBool.True;
			}
		}

		#endregion

		#region Public Members

		public void RemoveReceiptFromBatch(ARReceipt receipt)
		{
			if (receipt != null && ReceiptBatch.Contains(receipt))
			{
				ReceiptBatch.RemoveAndDelete(receipt);
				ReceiptBatch.RefreshBinding();
			}
		}

		#endregion

		#region GUIBindable Members

		#region PostDate

		public ZDateTime PostDate
		{
			get
			{
				return fPostDate;
			}
			set
			{
				fPostDate = value;
				if (!IsValidationSuspended)
				{
					ValidatePostDate();
				}
				UpdatePostDateDateForAllReceipts(value);
				PostDateInfo.RefreshBinding();
			}
		}
		ZDateTime fPostDate;

		public ZPropertyInfo PostDateInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(PostDate));
			}
		}

		protected bool PostDate_ReadOnly
		{
			get { return false; }
		}

		bool AllowBackPosting
		{
			get { return AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.Value && Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed; }
		}

		bool AllowFuturePosting
		{
			get { return AccountingUtils.IsAllowFuturePostingRegistryEnabled && AccountingUtils.DoesUserHaveFuturePostingSecurity; }
		}

		void UpdatePostDateDateForAllReceipts(ZDateTime value)
		{
			foreach (ARReceipt receipt in ReceiptBatch)
			{
				receipt.AH_PostDate = value;
			}
		}

		#endregion

		#region InvoiceDate

		public ZDateTime InvoiceDate
		{
			get
			{
				return fInvoiceDate;
			}
			set
			{
				fInvoiceDate = value;
				if (!IsValidationSuspended)
				{
					ValidateInvoiceDate();
				}
				UpdateInvoiceDateDateForAllReceipts(value);
				InvoiceDateInfo.RefreshBinding();
			}
		}
		ZDateTime fInvoiceDate;

		public ZPropertyInfo InvoiceDateInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(InvoiceDate));
			}
		}

		void UpdateInvoiceDateDateForAllReceipts(ZDateTime value)
		{
			foreach (ARReceipt receipt in ReceiptBatch)
			{
				receipt.AH_InvoiceDate = value;
			}
		}

		#endregion

		#region Description

		[MaxLength(AccTransactionHeader.Schema.AH_DescMaxLength)]
		public ZString Description
		{
			get
			{
				return fDescription;
			}
			set
			{
				CheckMaximumLength(DescriptionInfo, value);
				if (IsSettingDefaultValues || (!IsSettingDefaultValues && AskUserIfNeededBeforeDescriptionUpdate()))
				{
					fDescription = value;
					if (!IsValidationSuspended)
					{
						ValidateDescription();
					}
					UpdateDescriptionForAllPayments(value);
					DescriptionInfo.RefreshBinding();
				}
			}
		}
		ZString fDescription;

		public ZPropertyInfo DescriptionInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(Description));
			}
		}

		ZBool AskUserIfNeededBeforeDescriptionUpdate()
		{
			if (ReceiptBatch.Count > 1)
			{
				ZString currentValue = ZString.Empty;
				foreach (ARReceipt receipt in ReceiptBatch)
				{
					if (currentValue == ZString.Empty)
					{
						currentValue = receipt.AH_Desc;
					}
					else if (currentValue != receipt.AH_Desc)
					{
						return AskBeforeUpdateDescription();
					}
				}
			}
			return ZBool.True;
		}

		void UpdateDescriptionForAllPayments(ZString value)
		{
			foreach (ARReceipt receipt in ReceiptBatch)
			{
				receipt.AH_Desc = value;
			}
		}

		#endregion

		#region ReceiptType

		[List("ReceiptMethods")]
		[MaxLength(AccTransactionHeader.Schema.AH_ReceiptTypeMaxLength)]
		public ZString ReceiptType
		{
			get
			{
				return fReceiptType;
			}
			set
			{
				if (fReceiptType != value)
				{
					CheckMaximumLength(ReceiptTypeInfo, value);

					if (IsSettingDefaultValues || (!IsSettingDefaultValues && AskUserIfNeededBeforeReceiptTypeUpdate()))
					{
						fReceiptType = value;

						UpdateReceiptTypeForAllPayments(value);

						if (!IsValidationSuspended)
						{
							ValidateReceiptType();
						}
						ReceiptTypeInfo.RefreshBinding();
						UpdateAH_ChequeOrReferenceForAllReceipts();
					}
				}
			}
		}
		ZString fReceiptType;

		ZBool AskUserIfNeededBeforeReceiptTypeUpdate()
		{
			if (ReceiptBatch.Count > 1)
			{
				ZString currentValue = ZString.Empty;
				foreach (ARReceipt receipt in ReceiptBatch)
				{
					if (currentValue == ZString.Empty)
					{
						currentValue = receipt.AH_ReceiptType;
					}
					else if (currentValue != receipt.AH_ReceiptType)
					{
						return AskBeforeUpdateReceiptType();
					}
				}
			}
			return ZBool.True;
		}

		public ZPropertyInfo ReceiptTypeInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(ReceiptType));
			}
		}

		void UpdateReceiptTypeForAllPayments(ZString value)
		{
			foreach (ARReceipt receipt in ReceiptBatch)
			{
				receipt.AH_ReceiptType = value;
			}
		}

		void UpdateAH_ChequeOrReferenceForAllReceipts()
		{
			var defaultReferenceNumber = AccountingConfigurationRegistry.Instance.GetReferenceNumberFromType(ReceiptType);
			var value = ZString.Empty;

			if (defaultReferenceNumber.IsEmpty)
			{
				if (ReceiptType == ZArchitecture.Core.ReceiptTypes.Cash)
				{
					value = ZArchitecture.Core.ReceiptTypes.Cash;
				}
				else if (ReceiptType == ZArchitecture.Core.ReceiptTypes.Cheque)
				{
					value = ZString.Empty;
				}
				else if (ReceiptType == ZArchitecture.Core.ReceiptTypes.CreditCard)
				{
					value = ZString.Empty;
				}
			}
			else
			{
				value = defaultReferenceNumber;
			}

			UpdateAH_ChequeOrReferenceForAllReceipts(value);
		}

		void UpdateAH_ChequeOrReferenceForAllReceipts(ZString value)
		{
			foreach (ARReceipt receipt in ReceiptBatch)
			{
				receipt.AH_ChequeOrReference = value;
			}
		}

		#endregion

		#region BankAccountPK

		[List("BankAccounts")]
		public ZGuid BankAccountPK
		{
			get
			{
				return fBankAccountPK;
			}
			set
			{
				fBankAccountPK = value;

				if (BankAccount != null)
				{
					ExchangeRate.Currency = BankAccount.AB_RX_NKAccountCurrency;
					if (BankAccount.AB_AccountType == AccountTypeCodeDescriptionPairList.Codes.CSH)
					{
						ReceiptType = ReceiptTypes.Cash;
					}
					else if (ReceiptType == ZArchitecture.Core.ReceiptTypes.Cheque)
					{
						UpdateAH_ChequeOrReferenceForAllReceipts(ZString.Empty);
					}
				}
				UpdateBankAccountPKForAllReceipts(value);
				if (!IsValidationSuspended)
				{
					ValidateBankAccountPK();
				}
				BankAccountPKInfo.RefreshBinding();
			}
		}
		ZGuid fBankAccountPK;

		public ZPropertyInfo BankAccountPKInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(BankAccountPK));
			}
		}

		void UpdateBankAccountPKForAllReceipts(ZGuid value)
		{
			foreach (ARReceipt receipt in ReceiptBatch)
			{
				receipt.AH_AB = value;
			}
		}

		#endregion

		#region BankAccount

		AccBankAccount BankAccount
		{
			get
			{
				if (BankAccountPK.IsValid)
				{
					return Factory.Load<AccBankAccount>(BankAccountPK);
				}
				else
				{
					return null;
				}
			}
		}

		#endregion

		#region ExchangeRate

		public ZExchangeRate ExchangeRate
		{
			get
			{
				if (fExchangeRate == null)
				{
					fExchangeRate = new ZAccExchangeRate(this, ExchangeRateType.Sell, SellExRateInfo, (ZPropertyInfoString)RX_NKInfo, null);
					fExchangeRate.IsCurrencyRequired = true;
					fExchangeRate.IsRateRequired = true;
				}
				return fExchangeRate;
			}
		}
		ZAccExchangeRate fExchangeRate;

		public ZDecimal SellExRate
		{
			get
			{
				return fSellExRate;
			}
			set
			{
				//if (IsSettingDefaultValues || (!IsSettingDefaultValues && AskUserIfNeededBeforePayExRateUpdate()))
				//{
				UpdateExchangeRateForAllReceipts(value);
				fSellExRate = value;
				//}
				if (!IsValidationSuspended)
				{
					ValidateExchangeRate();
				}
				SellExRateInfo.RefreshBinding();
			}
		}
		ZDecimal fSellExRate;

		//ZBool AskUserIfNeededBeforePayExRateUpdate()
		//{
		//    if (ReceiptBatch.Count > 1)
		//    {
		//        ZDecimal CurrentValue = 0;
		//        foreach (ARReceipt Receipt in ReceiptBatch)
		//        {
		//            if (CurrentValue == 0)
		//            {
		//                CurrentValue = Receipt.ExchangeRate.Rate;
		//            }
		//            else if (CurrentValue != Receipt.ExchangeRate.Rate)
		//            {
		//                return Globals.Message.Show("Are you sure you want to update the Exchange Rate for all receipts?", "Exchange Rate", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
		//            }
		//        }
		//    }
		//    return ZBool.True;
		//}

		void UpdateExchangeRateForAllReceipts(ZDecimal value)
		{
			foreach (ARReceipt receipt in ReceiptBatch)
			{
				receipt.ExchangeRate.Rate = value;
			}
		}

		public ZPropertyInfo SellExRateInfo
		{
			get
			{
				ZPropertyInfo info = GetZPropertyInfo(nameof(SellExRate));
				return info;
			}
		}

		protected bool SellExRate_ReadOnly
		{
			get { return ExchangeRate.IsRateReadOnly; }
		}

		[MaxLength(3)]
		[ReadOnly(true)]
		[List("Currencies")]
		public ZString RX_NK
		{
			get
			{
				return fRX_NK;
			}
			set
			{
				CheckMaximumLength(RX_NKInfo, value);
				fRX_NK = value;
				RX_NKInfo.RefreshBinding();
				UpdateAH_RXForAllReceipts(value);
				ForeignCurrencyTotalInfo.RefreshBinding();
				LocalCurrencyTotalInfo.RefreshBinding();
			}
		}
		ZString fRX_NK;

		void UpdateAH_RXForAllReceipts(ZString value)
		{
			foreach (ARReceipt receipt in ReceiptBatch)
			{
				receipt.AH_RX_NKTransactionCurrency = value;
			}
		}

		public ZPropertyInfo RX_NKInfo
		{
			get { return GetZPropertyInfo(nameof(RX_NK)); }
		}

		#endregion

		#region MatchAfterPosting

		public ZBool MatchAfterPosting
		{
			get
			{
				return fMatchAfterPosting;
			}
			set
			{
				fMatchAfterPosting = value;
				MatchAfterPostingInfo.RefreshBinding();
			}
		}
		ZBool fMatchAfterPosting;

		public ZPropertyInfo MatchAfterPostingInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(MatchAfterPosting));
			}
		}

		#endregion

		#region CreateDepositSlip

		public ZBool CreateDepositSlip
		{
			get
			{
				return fCreateDepositSlip;
			}
			set
			{
				fCreateDepositSlip = value;
				CreateDepositSlipInfo.RefreshBinding();
				ReceiptBatch.SetIncludeInDepositBatchForAllReceipts(value);
				ReceiptBatch.RefreshBinding();
				if (!IsValidationSuspended)
				{
					ValidateCreateDepositSlip();
				}
			}
		}
		ZBool fCreateDepositSlip;

		public ZPropertyInfo CreateDepositSlipInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(CreateDepositSlip));
			}
		}

		#endregion

		#region ForeignCurrencyTotal

		public ZDecimal ForeignCurrencyTotal
		{
			get
			{
				return ReceiptBatch.ForeignTotalAmount;
			}
		}

		public ZPropertyInfo ForeignCurrencyTotalInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(ForeignCurrencyTotal));
			}
		}

		#endregion

		#region LocalCurrencyTotal

		public ZDecimal LocalCurrencyTotal
		{
			get
			{
				return ReceiptBatch.LocalTotalAmount;
			}
		}

		public ZPropertyInfo LocalCurrencyTotalInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(LocalCurrencyTotal));
			}
		}

		#endregion

		#region Calc_LocalRX_NK

		[MaxLength(3)]
		[List("Currencies")]
		public ZString Calc_LocalRX_NK
		{
			get
			{
				return GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			}
		}

		public ZPropertyInfo Calc_LocalRX_NKInfo
		{
			get { return GetZPropertyInfo(nameof(Calc_LocalRX_NK)); }
		}
		#endregion

		#region Calc_RXDecimals

		public ZInt Calc_RXDecimals
		{
			get
			{
				RefCurrency currency = RefCurrency.LoadFromCurrencyCode(Factory, RX_NK);
				return currency != null ? currency.Decimals : 2;
			}
		}

		public ZPropertyInfo Calc_RXDecimalsInfo
		{
			get { return GetZPropertyInfo(nameof(Calc_RXDecimals)); }
		}

		#endregion

		#region Calc_LocalRXDecimals

		public ZInt Calc_LocalRXDecimals
		{
			get
			{
				RefCurrency currency = RefCurrency.LoadFromCurrencyCode(Factory, Calc_LocalRX_NK);
				return currency != null ? currency.Decimals : 2;
			}
		}

		public ZPropertyInfo Calc_LocalRXDecimalsInfo
		{
			get { return GetZPropertyInfo(nameof(Calc_LocalRXDecimals)); }
		}

		#endregion

		#region ReceiptBatch

		public ARReceiptCollection ReceiptBatch
		{
			get
			{
				if (fReceiptBatch == null)
				{
					fReceiptBatch = new ARReceiptCollection(Factory, this);
					fReceiptBatch.SetReceiptHeaderDataIsReadOnly();
					fReceiptBatch.OnAmountOnChildChanged += new ARReceiptCollection.OnAmountOnChildChangedHandler(fReceiptBatch_OnAmountOnChildChanged);
					fReceiptBatch.OnReceiptTypeOnChildChanged += new ARReceiptCollection.OnReceiptTypeOnChildChangedHandler(fReceiptBatch_OnReceiptTypeOnChildChanged);
					RegisterEditableChildObject(ReceiptBatch);
				}
				return fReceiptBatch;
			}
		}
		ARReceiptCollection fReceiptBatch;

		#endregion

		#endregion

		#region Implementation

		protected override void SetDefaultValues()
		{
			using (SuspendSettingHasChanges())
			{
				base.SetDefaultValues();
				IsSettingDefaultValues = ZBool.True;

				InvoiceDate = ZDateTime.Now;
				PostDate = ZDateTime.Now;
				ReceiptType = AccountingConfigurationRegistry.Instance.DefaultReceiptType.Value;
				BankAccountPK = ZGuid.Empty;
				Description = Res.GetString("cc25d401-1888-4d4e-8f31-129951ffad95", "AR RECEIPT");

				IsSettingDefaultValues = ZBool.False;
			}
		}
		ZBool IsSettingDefaultValues;

		ZBool NeedToCreateDepositBatch
		{
			get { return CreateDepositSlip && !IsDepositBatchCreated && !FilterOnReceiptsSelectedForBatch.IsEmpty; }
		}
		ZBool IsDepositBatchCreated;

		ZBool NeedToPrintDepositBatch
		{
			get { return RelatedDepositBatch != null && RelatedDepositBatch.IsInDatabase && !IsDepositBatchPrinted; }
		}
		ZBool IsDepositBatchPrinted;

		void CreateDepositBatch()
		{
			RelatedDepositBatch = Factory.New<DepositBatch>();
			RelatedDepositBatch.AH_GB = GlbBranch.CurrentBranch.PK;
			RelatedDepositBatch.AH_AB = BankAccountPK;
			RelatedDepositBatch.AH_PostDate = PostDate;
			RelatedDepositBatch.AH_InvoiceDate = PostDate;
			RelatedDepositBatch.Transactions.LoadWithMoreFiltering(FilterOnReceiptsSelectedForBatch);
			IsDepositBatchCreated = ZBool.True;
		}
		DepositBatch RelatedDepositBatch;

		ZQuery FilterOnReceiptsSelectedForBatch
		{
			get
			{
				if (fFilterOnReceiptsSelectedForBatch == null)
				{
					fFilterOnReceiptsSelectedForBatch = new ZQuery();
					fFilterOnReceiptsSelectedForBatch.DefaultJoinCondition = JoinCondition.Or;
					foreach (ARReceipt receipt in ReceiptBatch)
					{
						if (receipt.IncludeInDepositBatch)
						{
							fFilterOnReceiptsSelectedForBatch.AddToFilter(AccTransactionHeaderSchema.PK, receipt.PK);
						}
					}
				}
				return fFilterOnReceiptsSelectedForBatch;
			}
		}
		ZQuery fFilterOnReceiptsSelectedForBatch;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		public void RemoveNewlyCreatedReceiptsFromUnmatchedTransactions()
		{
			foreach (ARReceipt receipt in ReceiptBatch)
			{
				ZQuery orgHeadersFilter = new ZQuery(OrgRelatedPartySchema.PR_OH_RelatedParty, receipt.AH_OH);
				orgHeadersFilter.AddToFilter(OrgRelatedPartySchema.PR_PartyType, RelatedPartyTypeList.Codes.ARSettlementGroup);
				orgHeadersFilter.AddToFilter(OrgRelatedPartySchema.PR_GC, GlbCompany.CurrentCompany.PK);
				orgHeadersFilter.AddToFilter(OrgRelatedPartySchema.PR_OH_Parent, SQLComparisonOperator.NotEqual, receipt.AH_OH);

				OrgRelatedParty[] relatedHeaders = Factory.Load<OrgRelatedParty>(orgHeadersFilter);
				ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_OH, receipt.AH_OH);
				if (relatedHeaders != null)
				{
					filter.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_OH, Array.ConvertAll(relatedHeaders, orgRelatedParty => orgRelatedParty.PR_OH_Parent));
				}
				filter.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, receipt.PK);

				BusinessObject[] relatedReceipts = ReceiptBatch.Find(filter);
				foreach (BusinessObject relatedReceipt in relatedReceipts)
				{
					receipt.AddTransactionThatExcludedFromUnmatchedList((ARReceipt)relatedReceipt);
				}
			}
			MatchingObjectsPrepared = ZBool.True;

#if DEBUG
			MatchingObjectsPreparedForTest = ZBool.True;
#endif
		}
		ZBool MatchingObjectsPrepared;

		ZBool AtLeastOneReceiptCanBeIncludedInDepositSlipOrBatchIsEmpty
		{
			get
			{
				if (ReceiptBatch.Count == 0)
				{
					return ZBool.True;
				}
				else
				{
					foreach (ARReceipt receipt in ReceiptBatch)
					{
						if (receipt.AH_ReceiptType == ReceiptTypes.Cheque || receipt.AH_ReceiptType == ReceiptTypes.Cash || receipt.AH_ReceiptType == ReceiptTypes.CreditCard)
						{
							return ZBool.True;
						}
					}
					return ZBool.False;
				}
			}
		}

		void fReceiptBatch_OnReceiptTypeOnChildChanged()
		{
			ReceiptBatch.RefreshBinding();
			CreateDepositSlipInfo.RefreshBinding();
			ValidateCreateDepositSlip();
		}

		void fReceiptBatch_OnAmountOnChildChanged()
		{
			ForeignCurrencyTotalInfo.RefreshBinding();
			LocalCurrencyTotalInfo.RefreshBinding();
		}

		#endregion

		#region LookUps

		#region ReceiptMethods

		public CodeDescriptionPairList ReceiptMethods
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.ReceiptMethod); }
		}

		#endregion

		#region Bank Accounts

		public AccBankAccountCollection BankAccounts
		{
			get
			{
				if (fBankAccounts == null)
				{
					ZQuery branchFilter = new ZQuery(AccBankAccountSchema.AB_GB, GlbBranch.CurrentBranch.PK);
					branchFilter.AddToFilter(JoinCondition.Or, AccBankAccountSchema.AB_GB, SQLComparisonOperator.Equal, null);

					ZQuery bankFilter = new ZQuery(AccBankAccountSchema.AB_GC, GlbCompany.CurrentCompany.PK);
					bankFilter.AddToFilter(AccBankAccountSchema.AB_IsActive, true);
					bankFilter.AddToFilter(branchFilter, JoinCondition.And);

					fBankAccounts = new AccBankAccountCollection(Factory, bankFilter);
				}
				return fBankAccounts;
			}
		}

		AccBankAccountCollection fBankAccounts;

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

		#region Validation

		PeriodValidationProvider fPeriodValidation;
		PeriodValidationProvider PeriodValidation
		{
			get
			{
				if (fPeriodValidation == null)
				{
					fPeriodValidation = new PeriodValidationProvider(Factory);
				}

				return fPeriodValidation;
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateReceiptType();
			ValidatePostDate();
			ValidateInvoiceDate();
			ValidateBankAccountPK();
			ValidateExchangeRate();
			ValidateCreateDepositSlip();
		}

		#region ValidateReceiptType

		void ValidateReceiptType()
		{
			ReceiptTypeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ReceiptTypeInfo, Res.GetString("71583539-330b-4154-b759-7499f18c0072", "Receipt Type"));
			ListValidation.ErrorIfInvalidCode(ReceiptTypeInfo, ReceiptMethods);

			if (ReceiptType != ReceiptTypes.Cash && BankAccount != null && BankAccount.AB_AccountType == AccountTypeCodeDescriptionPairList.Codes.CSH)
			{
				ReceiptTypeInfo.AddError(TransactionHeaderValidation.GetCashAccountTypeErrorMessage(ReceiptTypeInfo.HumanReadableName));
			}

			if (!ReceiptTypeInfo.HasErrors())
			{
				CheckReceiptTypeSecurity();
			}
		}

		void CheckReceiptTypeSecurity()
		{
			bool isAllowed = true;
			switch (ReceiptType)
			{
				case ReceiptTypes.Cheque:
					isAllowed = Env.Security.NewReceivablesReceiptCheque.IsAllowed;
					break;

				case ReceiptTypes.Cash:
					isAllowed = Env.Security.NewReceivablesReceiptCash.IsAllowed;
					break;

				case ReceiptTypes.CreditCard:
					isAllowed = Env.Security.NewReceivablesReceiptCreditCard.IsAllowed;
					break;

				case ReceiptTypes.DirectCredit:
					isAllowed = Env.Security.NewReceivablesReceiptDirectCredit.IsAllowed;
					break;
			}

			if (!isAllowed)
			{
				ReceiptTypeInfo.AddError(Res.GetString("052a205e-2cb5-488b-882f-61e0839caad4", "You do not have appropriate security rights to select this receipt type."));
			}
		}

		#endregion

		#region ValidatePostDate

		void ValidatePostDate()
		{
			PostDateInfo.ClearAllNotifications();
			//Common validation
			MandatoryValidation.CheckEntered(PostDateInfo);
			PeriodValidation.CheckDateFallsIntoValidPeriod(PostDateInfo);

			//Overriden in children
			CheckAH_PostDateNotInFuture();
			CheckAH_PostDateNotInPast();
		}

		void CheckAH_PostDateNotInFuture()
		{
			if (!PostDateInfo.HasErrors())
			{
				if (PostDate.Date > ZDateTime.Today)
				{
					if (!AccountingUtils.IsAllowFuturePostingRegistryEnabled)
					{
						PostDateInfo.AddError(AccountingConstants.FuturePostingErrorMessages.RegistryIsNotEnabled);
					}
					else if (!AccountingUtils.DoesUserHaveFuturePostingSecurity)
					{
						PostDateInfo.AddError(AccountingConstants.FuturePostingErrorMessages.UserHasNoSecurity);
					}
				}
			}
		}

		void CheckAH_PostDateNotInPast()
		{
			if (!PostDateInfo.HasErrors())
			{
				if (PostDate.Date < ZDateTime.Today)
				{
					if (!AllowBackPosting)
					{
						PostDateInfo.AddError(PreviousPostDateError);
					}
					else
					{
						PostDateInfo.AddWarning(PreviousPostDateWarning);
						PostDateInfo.AddWarning(ReceiptBatchPostDateWarning);
					}
				}
			}
		}

		public static ZString ReceiptBatchPostDateWarning
		{
			get
			{
				return Res.GetString("ff7d4364-1c2b-4c82-8d44-86a51d559d58", "The receipt batch post date will be same as this post date.");
			}
		}

		public ZString PreviousPostDateWarning
		{
			get
			{
				return Res.GetString("85d37b28-4bd5-48b4-a9e4-71e6e50ec98c", "You are posting to a previous date. If this transaction is posted, there may be implications in the following subsystems \r\n - Financial Reports\r\n - Sub-Ledger Reports\r\n - Bank Reconciliation\r\n - Reversing");
			}
		}

		public static ZString PreviousPostDateError
		{
			get
			{
				return Res.GetString("98c79877-ad95-4fb7-811d-5027fca51e72", "The post date cannot be in the past");
			}
		}

		#endregion

		#region ValidateInvoiceDate

		void ValidateInvoiceDate()
		{
			InvoiceDateInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(InvoiceDateInfo);
		}

		#endregion

		#region ValidateBankAccountPK

		void ValidateBankAccountPK()
		{
			BankAccountPKInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(BankAccountPKInfo, Res.GetString("536c7643-542a-4808-bee2-8164151f2d12", "Bank"));

			if (!BankAccountPK.IsValid || BankAccount == null)
			{
				BankAccountPKInfo.AddError(Res.GetString("4c4fab18-49e4-4b5f-ba53-4104f858f0be", "Please enter a valid bank account."));
			}
			else if (BankAccount.AB_RX_NKAccountCurrency != RX_NK)
			{
				BankAccountPKInfo.AddError(Res.GetString("5c4bc80c-2ad4-47b8-a40e-84be404bfe16", "Bank account currency does not match the invoice currency."));
			}
		}

		#endregion

		#region ValidateDescription

		void ValidateDescription()
		{
			DescriptionInfo.ClearAllNotifications();
			if (Description.IsEmpty)
			{
				DescriptionInfo.AddError(Res.GetString("97c92150-25d0-4a68-9746-1fd3f741e67a", "You must enter a description"));
			}
		}

		#endregion

		#region ValidateExchangeRate

		void ValidateExchangeRate()
		{
			SellExRateInfo.ClearAllNotifications();
			MandatoryValidation.CheckNotNegative(SellExRateInfo);
			MandatoryValidation.CheckNotZero(SellExRateInfo);
		}

		#endregion

		#region ValidateCreateDepositSlip

		void ValidateCreateDepositSlip()
		{
			CreateDepositSlipInfo.ClearAllNotifications();
			if (!AtLeastOneReceiptCanBeIncludedInDepositSlipOrBatchIsEmpty && CreateDepositSlip)
			{
				CreateDepositSlipInfo.AddError(Res.GetString("390b7896-75ac-4516-99d5-cc70f63ea4d7", "Deposit Slip can not be created as the Receipt Batch does not contain any receipts of types 'CHQ', 'CSH' or 'CCD'"));
			}
		}

		#endregion

		#endregion

		#region Saving

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			if (NeedToCreateDepositBatch)
			{
				CreateDepositBatch();
			}
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			if (saveSucceeded && NeedToPrintDepositBatch)
			{
				PrintDepositSlip(RelatedDepositBatch);
				IsDepositBatchPrinted = ZBool.True;
			}
			if (MatchAfterPosting && !MatchingObjectsPrepared)
			{
				RemoveNewlyCreatedReceiptsFromUnmatchedTransactions();
			}
		}

		#endregion

		#if DEBUG

		ZBool MatchingObjectsPreparedForTest;

		#endif
	}
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.CashBook.ExchangeDifference;
using Enterprise.Accounting.Business.CashBook.Transfer.Strategy;
using Enterprise.Accounting.Business.DataExportBatch;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.AccTransactionHeader;

namespace Enterprise.Accounting.Business.CashBook.Transfer
{
	[ProvideMetaDataProperty("PropertyReadonlyness", MetaDataTypes.ReadOnly)]
	public partial class BankTransfer : NonPersistentBusinessObject, ICashBook, IDataExportBatchSource, IIdentified, IHandleDeleteError, ITemplateCopyable
	{
		public BankTransfer(BusinessObjectFactory factory, BankTransferFromRow existingTransferToBeLoaded)
			: base(factory)
		{
			if (existingTransferToBeLoaded != null)
			{
				transactionBelongsToGroupGuid = existingTransferToBeLoaded.AH_TransactionBelongsToGroup;
				LoadExistingRecords(existingTransferToBeLoaded);
			}
			else
			{
				transactionBelongsToGroupGuid = ZGuid.NewZGuid();
				InitNewBankTransfer();
			}

			SetBankTransferStrategy();
			RegisterEditableChildObject(TransferRowTo);
			RegisterEditableChildObject(TransferRowFrom);
			RegisterEditableChildObject(FinanceCharge);

			((IBusinessObjectState)this).ClearHasChangesIncludingChildren();

			BankTransferFromPKInfo.AdditionalValidation += CheckBankTransferFromPK;
			FinanceChargeOSAmountInfo.AdditionalValidation += CheckBankCharge;
		}

		void InitNewBankTransfer()
		{
			TransferRowFrom = Factory.New<BankTransferFromRow>();
			TransferRowTo = Factory.New<BankTransferToRow>();
			ExchangeDiff = Factory.New<CashbookExchangeDiff>();
			FinanceCharge = Factory.New<BankTransferCharge>();

			using (SuspendSettingHasChanges())
			using (TransferRowFrom.SuspendSettingHasChanges())
			using (TransferRowTo.SuspendSettingHasChanges())
			using (ExchangeDiff.SuspendSettingHasChanges())
			using (FinanceCharge.SuspendSettingHasChanges())
			{
				TransferRowFrom.BankTransferParent = this;
				TransferRowFrom.AH_TransactionBelongsToGroup = transactionBelongsToGroupGuid;

				TransferRowTo.BankTransferParent = this;
				TransferRowTo.AH_TransactionBelongsToGroup = transactionBelongsToGroupGuid;

				FillExchangeDiff();

				FinanceCharge.BankTransferParent = this;
				FinanceCharge.AH_Desc = AccountingConfigurationRegistry.Instance.GetTransactionDescriptionFromCode(
					AccountingConstants.VoucherItemRegistryCode.FinanceCharge,
					Res.GetString("5468F9B7-597D-4ade-B528-AB36334A4137", "Cash Book Bank Transfer Finance Charge"));
			}

			EnableFinanceCharge = false;
		}

		void LoadExistingRecords(BankTransferFromRow existingTransferToBeLoaded)
		{
			LoadTransferRowFrom(existingTransferToBeLoaded);
			LoadTransferRowTo();
			LoadOrCreateExchangeDiff();
			LoadOrCreateFinanceCharge();

			SetReadOnlyIncludingChildren(true);
			TransferRowFrom.SetReadOnlyIncludingChildren(true);
			ExchangeDiff.SetReadOnlyIncludingChildren(true);
			TransferRowTo.SetReadOnlyIncludingChildren(true);
			FinanceCharge.SetReadOnlyIncludingChildren(true);

			RefreshBinding();
		}

		void CheckBankTransferFromPK()
		{
			if (bankTransferFundingInfoCalculator != null)
			{
				var bankAccountCurrency = Factory.Load<AccBankAccount>(BankTransferFromPK)?.AB_RX_NKAccountCurrency ?? ZString.Empty;
				if (bankTransferFundingInfoCalculator.FundingCurrency != bankAccountCurrency)
				{
					var errorMsg = $"Funding currency is {bankTransferFundingInfoCalculator.FundingCurrency}. You must select a bank account with the same currency.";
					BankTransferFromPKInfo.AddError(errorMsg);
				}
			}
		}

		void CheckBankCharge()
		{
			if (bankTransferFundingInfoCalculator == null || bankTransferFundingInfoCalculator.FundingCurrency == Env.CurrentCompany.LocalCurrency.Code)
			{
				return;
			}

			var exchangeRate = Env.CurrentCompany.ExchangeRate.TodaysRate(bankTransferFundingInfoCalculator.FundingCurrency, ExchangeRateType.Buy);
			if (exchangeRate == 0m && FinanceChargeOSAmount == 0m)
			{
				var errorMsg = Res.GetString("ab859ba6-d456-4ded-ba68-24f8d4c7d18e",
						"Finance Charge is calculated from Total Provider Fees in Funding Currency using the Funding Currency's BUY rate. To post this Bank Transfer, please calculate the Bank Charge using the Funding Currency's exchange rate as entered.");
				FinanceChargeOSAmountInfo.AddError(errorMsg);
			}
			else
			{
				var warningMsg = Res.GetString("3a7dbf10-b65d-4fce-8ab6-8572fe4b3b8a",
						"Finance Charge is calculated from Total Provider Fees in Funding Currency using the BUY exchange rate. If you override the Sell Exchange Rate, please also re-calculate and update Finance Charge Amount.");
				FinanceChargeOSAmountInfo.AddWarning(warningMsg);
			}
		}

		protected override void OnFactorySaving()
		{
			if (!IsPosted)
			{
				var newTransactionNum = AccountingNumberFountainWrapperFactory.Instance.TransferNo.Generate(TransferRowFrom);

				ConfigureTransferRowFrom(newTransactionNum);
				ConfigureTransferRowTo(newTransactionNum);
				ConfigureExchangeDiff();
				ConfigureFinanceCharge();

#if DEBUG
				if (Globals.IsTest && ShouldForceImbalancedTransferRowTo_ForTestOnly)
				{
					TransferRowTo.AH_InvoiceAmount += 100m;
				}
#endif
			}

			base.OnFactorySaving();
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			if (saveSucceeded)
			{
				SetReadOnlyIncludingChildren(true);
				RefreshBinding();
			}
		}

		#region IIdentified Members

		ZGuid IIdentified.Identifier
		{
			get { return ((IIdentified)TransferRowFrom).Identifier; }
		}

		#endregion

		#region ReadOnly

		protected virtual bool GetPropertyReadonlyness(PropertyDescriptor property)
		{
			bool result = false;
			if (UseEditableFieldsForReadOnly && property.HasSetter())
			{
				result = !WritableProperties.Contains(property.Name);
			}
			return result || CargoWise.ComponentModel.MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		List<string> WritableProperties
		{
			get
			{
				if (writableProperties == null)
				{
					writableProperties = new List<string>();
				}
				return writableProperties;
			}
		}

		public void AddWritableProperties(string[] list)
		{
			foreach (string line in list)
			{
				WritableProperties.Add(line);
			}
			UseEditableFieldsForReadOnly = true;
			RefreshBinding();
		}

		List<string> writableProperties;

		bool UseEditableFieldsForReadOnly;

		#endregion

		#region Validation

		public BankTransferValidation Validation
		{
			get { return fIsReverseTransaction ? new BankTransferReversalValidation(this) : new BankTransferValidation(this); }
		}

		#endregion

		#region LookUps

		public RefCurrencyCollection CurrencyLookUp
		{
			get { return TransferRowTo.Lookups.TransactionCurrencies; }
		}

		public ZExchangeRate BuyZExchangeRate
		{
			get
			{
				if (buyZExchangeRate == null)
				{
					buyZExchangeRate = new ZAccExchangeRate(this, TransferRowTo.RateType, BuyExchangeRateInfo, (ZPropertyInfoString)BuyCurrencyInfo, TransferRowTo.AH_GCInfo);
					buyZExchangeRate.IsCurrencyRequired = true;
					buyZExchangeRate.IsRateRequired = true;
				}

				return buyZExchangeRate;
			}
		}
		ZExchangeRate buyZExchangeRate;

		public ZExchangeRate SellZExchangeRate
		{
			get
			{
				if (sellZExchangeRate == null)
				{
					sellZExchangeRate = new ZAccExchangeRate(this, TransferRowFrom.RateType, SellExchangeRateInfo, (ZPropertyInfoString)SellCurrencyInfo, TransferRowFrom.AH_GCInfo);
					sellZExchangeRate.IsCurrencyRequired = true;
					sellZExchangeRate.IsRateRequired = true;
				}

				return sellZExchangeRate;
			}
		}
		ZExchangeRate sellZExchangeRate;

		public ZExchangeRate FinanceChargeZExchangeRate
		{
			get
			{
				FinanceCharge.ExchangeRate.Currency_ReadOnly = true;
				FinanceCharge.ExchangeRate.AdditionalRateReadOnlyCondition = !EnableFinanceCharge;
				FinanceCharge.ExchangeRate.RateInfo.RefreshBinding();
				return FinanceCharge.ExchangeRate;
			}
		}

		#endregion

		#region Collections

		public AccTaxRateCollection TaxRates
		{
			get
			{
				if (fTaxRates == null)
				{
					ZQuery taxRatesFilter = new ZQuery(AccTaxRateSchema.AT_IsActive, true);
					fTaxRates = new VATAccTaxRateCollection(Factory, taxRatesFilter);
				}
				return fTaxRates;
			}
		}
		AccTaxRateCollection fTaxRates;

		public AccBankAccountCollection BankAccounts
		{
			get
			{
				if (fBankAccounts == null)
				{
					ZQuery bankFilter = new ZQuery(AccBankAccountSchema.AB_GC, GlbCompany.CurrentCompany.PK);
					bankFilter.AddToFilter(AccBankAccountSchema.AB_IsActive, true);
					fBankAccounts = new AccBankAccountCollection(Factory, bankFilter);
				}
				return fBankAccounts;
			}
		}
		AccBankAccountCollection fBankAccounts;

		#endregion

		#region Properties

		#region EnableFinanceCharge

		public ZBool EnableFinanceCharge
		{
			get { return FinanceCharge.EnableFinanceCharge; }
			set
			{
				FinanceCharge.EnableFinanceCharge = value;
				FinanceCharge.AH_TransactionBelongsToGroup = value ? transactionBelongsToGroupGuid : ZGuid.Empty;
				EnableFinanceChargeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo EnableFinanceChargeInfo
		{
			get { return GetZPropertyInfo(nameof(EnableFinanceCharge)); }
		}

		#endregion

		#region AH_PostDate

		public ZDateTime AH_PostDate
		{
			get { return PostDate; }
			set
			{
				PostDate = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateAH_PostDate();
				}

				AH_PostDateInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo AH_PostDateInfo
		{
			get { return GetZPropertyInfo(nameof(AH_PostDate)); }
		}

		protected bool AH_PostDate_ReadOnly
		{
			get { return !UserAllowedToBackPost; }
		}

		#endregion

		#region PostDate

		public ZDateTime PostDate
		{
			get { return TransferRowFrom.AH_PostDate; }
			set
			{
				TransferRowFrom.AH_PostDate = value;
				TransferRowTo.AH_PostDate = value;
			}
		}

		public ZPropertyInfo PostDateInfo
		{
			get { return TransferRowFrom == null ? null : GetWrappedZPropertyInfo(nameof(PostDate), x => TransferRowFrom.AH_PostDateInfo); }
		}

		protected bool PostDate_ReadOnly
		{
			get { return !Env.Security.CashBookPostToPreviousOrFutureOpenPeriod.IsAllowed; }
		}

		#endregion

		public ZGuid TransactionBelongsToGroup
		{
			get { return TransferRowFrom != null ? TransferRowFrom.AH_TransactionBelongsToGroup : ZGuid.Empty; }
		}

		public ZBool IsPosted
		{
			get { return TransferRowFrom?.IsInDatabase ?? false; }
		}

		public bool UserAllowedToBackPost
		{
			get { return AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.Value && Env.Security.CashBookPostToPreviousOrFutureOpenPeriod.IsAllowed; }
		}

		internal bool BuySellAmountsAndRatesReadOnly
		{
			get { return !BanksSpecified && !this.HasContext(BusinessContext.PopulateBankTransferWithEPaymentData); }
		}

		bool BanksSpecified
		{
			get
			{
				return TransferRowFrom.AH_AB.IsValid && TransferRowFrom.BankAccount != null && TransferRowTo.AH_AB.IsValid && TransferRowTo.BankAccount != null;
			}
		}

		#region Description

		[MaxLength(BankTransferRow.Schema.AH_DescMaxLength)]
		public ZString Description
		{
			get { return TransferRowFrom.AH_Desc; }
			set
			{
				TransferRowFrom.AH_Desc = value;
				TransferRowTo.AH_Desc = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateDescription();
				}

				DescriptionInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(Description)); }
		}

		#endregion

		#region Reference

		[MaxLength(BankTransferRow.Schema.AH_ChequeOrReferenceMaxLength)]
		public ZString Reference
		{
			get { return TransferRowFrom.AH_ChequeOrReference; }
			set
			{
				TransferRowFrom.AH_ChequeOrReference = value;
				TransferRowTo.AH_ChequeOrReference = value;
				FinanceCharge.AH_ChequeOrReference = value;
				ReferenceInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateReference();
				}
			}
		}

		public ZPropertyInfo ReferenceInfo
		{
			get { return TransferRowFrom == null ? null : GetZPropertyInfo(nameof(Reference)); }
		}

		#endregion

		#region TransactionNumber

		[ReadOnly(true)]
		public ZString TransactionNumber
		{
			get { return TransferRowFrom.AH_TransactionNum; }
			set
			{
				TransferRowFrom.AH_TransactionNum = value;
				TransferRowTo.AH_TransactionNum = value;
			}
		}

		public ZPropertyInfo TransactionNumberInfo
		{
			get { return TransferRowFrom == null ? null : GetWrappedZPropertyInfo(nameof(TransactionNumber), x => TransferRowFrom.AH_TransactionNumInfo); }
		}

		#endregion

		#region AH_NumberOfSupportingDocuments

		public ZByte AH_NumberOfSupportingDocuments
		{
			get { return TransferRowFrom.AH_NumberOfSupportingDocuments; }
			set
			{
				if (TransferRowFrom.AH_NumberOfSupportingDocuments != value)
				{
					TransferRowTo.AH_NumberOfSupportingDocuments = value;
					TransferRowFrom.AH_NumberOfSupportingDocuments = value;

					if (!IsValidationSuspended)
					{
						ValidateAH_NumberOfSupportingDocuments();
					}
					AH_NumberOfSupportingDocumentsInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo AH_NumberOfSupportingDocumentsInfo
		{
			get { return GetZPropertyInfo(nameof(AH_NumberOfSupportingDocuments)); }
		}
		public void ValidateAH_NumberOfSupportingDocuments()
		{
			MandatoryValidation.CheckNotNegative(AH_NumberOfSupportingDocumentsInfo, Res.GetString("1b6dffa2-7da9-47f9-91eb-45c79e707eef", "Number of attachments"));
		}

		#endregion

		#region BankTransferFromPK

		[List("BankAccounts")]
		public ZGuid BankTransferFromPK
		{
			get { return TransferRowFrom.AH_AB; }
			set
			{
				TransferRowFrom.AH_AB = value;
				if (EnableFinanceCharge && !this.HasContext(BusinessContext.PopulateBankTransferWithEPaymentData))
				{
					FinanceChargeBankPK = value;
				}
				if (TransferRowFrom.BankAccount != null)
				{
					TransferRowFrom.ExchangeRate.Currency = TransferRowFrom.BankAccount.AB_RX_NKAccountCurrency;
				}
				else
				{
					TransferRowFrom.ExchangeRate.Currency = ZString.Empty;
				}
				OnBankUpdated();

				bankTransferFundingInfoCalculator?.CalculateBankTransferWithFundingCurrency();

				TransferRowTo.Validation.ValidateAH_AB();
				RefreshBinding();
			}
		}

		public ZPropertyInfo BankTransferFromPKInfo
		{
			get { return TransferRowFrom == null ? null : GetWrappedZPropertyInfo(nameof(BankTransferFromPK), x => TransferRowFrom.AH_ABInfo); }
		}

		#endregion

		#region BankTransferToPK

		[List("BankAccounts")]
		public ZGuid BankTransferToPK
		{
			get { return TransferRowTo.AH_AB; }
			set
			{
				TransferRowTo.AH_AB = value;
				if (TransferRowTo.BankAccount != null)
				{
					TransferRowTo.ExchangeRate.Currency = TransferRowTo.BankAccount.AB_RX_NKAccountCurrency;
				}
				else
				{
					TransferRowTo.ExchangeRate.Currency = ZString.Empty;
				}
				OnBankUpdated();

				TransferRowFrom.Validation.ValidateAH_AB();
				RefreshBinding();
			}
		}

		public ZPropertyInfo BankTransferToPKInfo
		{
			get { return TransferRowTo == null ? null : GetWrappedZPropertyInfo(nameof(BankTransferToPK), x => TransferRowTo.AH_ABInfo); }
		}

		public bool BankTransferToPK_ReadOnly { get; set; }

		#endregion

		#region SellAmount

		[ReadOnlyMember(nameof(SellAmountIsReadOnly))]
		public ZDecimal SellAmount
		{
			get { return BankTransferStrategy.GetSellAmount(); }
			set
			{
				BankTransferStrategy.SetSellAmount(value);
				RefreshBinding();
			}
		}

		bool SellAmountIsReadOnly
		{
			get { return BuySellAmountsAndRatesReadOnly; }
		}

		public ZPropertyInfo SellAmountInfo
		{
			get { return TransferRowFrom == null ? null : GetWrappedZPropertyInfo(nameof(SellAmount), x => TransferRowFrom.AH_OSExTaxAmountInfo); }
		}

		public ZInt SellAmountDecimals
		{
			get { return TransferRowFrom.AH_Calc_RXDecimals; }
		}

		public ZPropertyInfo SellAmountDecimalsInfo
		{
			get { return TransferRowFrom == null ? null : GetWrappedZPropertyInfo(nameof(SellAmountDecimals), x => TransferRowFrom.AH_Calc_RXDecimalsInfo); }
		}

		#endregion

		#region BuyAmount

		[ReadOnlyMember(nameof(BuyAmountIsReadOnly))]
		public ZDecimal BuyAmount
		{
			get { return BankTransferStrategy.GetBuyAmount(); }
			set
			{
				BankTransferStrategy.SetBuyAmount(value);
				RefreshBinding();
			}
		}

		void SetBankTransferStrategy()
		{
			(BankTransferStrategy as IDisposable)?.Dispose();

			if (TransferRowFrom.IsReversal && ShouldCalculateExchangeVariance)
			{
				BankTransferStrategy = new BankTransferIncludeExchangeVarianceInRowFromStrategy(this);
			}
			else if (ShouldCalculateExchangeVariance)
			{
				BankTransferStrategy = new BankTransferIncludeExchangeVarianceInRowToStrategy(this);
			}
			else
			{
				BankTransferStrategy = new BankTransferExcludeExchangeVarianceStrategy(this);
			}
		}

		IBankTransferStrategy BankTransferStrategy;

		bool BuyAmountIsReadOnly
		{
			get { return BuySellAmountsAndRatesReadOnly; }
		}

		public ZPropertyInfo BuyAmountInfo
		{
			get { return TransferRowTo == null ? null : GetWrappedZPropertyInfo(nameof(BuyAmount), x => TransferRowTo.AH_OSExTaxAmountInfo); }
		}

		public void OnBankUpdated()
		{
			if (!this.HasContext(BusinessContext.PopulateBankTransferWithEPaymentData))
			{
				SellAmount = 0;
				BuyAmount = 0;
			}

			if (AreBothBuyAndSellCurrencyLocal)
			{
				shouldCalculateExchangeVariance = false;
				SetBankTransferStrategy();
			}
		}

		public ZInt BuyAmountDecimals
		{
			get { return TransferRowTo.AH_Calc_RXDecimals; }
		}

		public ZPropertyInfo BuyAmountDecimalsInfo
		{
			get { return TransferRowTo == null ? null : GetWrappedZPropertyInfo(nameof(BuyAmountDecimals), x => TransferRowTo.AH_Calc_RXDecimalsInfo); }
		}

		#endregion

		#region BuyCurrency

		[List("CurrencyLookUp")]
		public ZString BuyCurrency
		{
			get { return TransferRowTo.BankAccount != null ? TransferRowTo.BankAccount.AB_RX_NKAccountCurrency : ZString.Empty; }
		}

		public ZPropertyInfo BuyCurrencyInfo
		{
			get { return GetZPropertyInfo(nameof(BuyCurrency)); }
		}

		#endregion

		#region  BuyExchangeRate

		public ZDecimal BuyExchangeRate
		{
			get { return BankTransferStrategy.GetBuyExchangeRate(); }
			set
			{
				BankTransferStrategy.SetBuyExchangeRate(value);
				RefreshBinding();
			}
		}

		public ZPropertyInfo BuyExchangeRateInfo
		{
			get { return TransferRowTo == null ? null : GetWrappedZPropertyInfo(nameof(BuyExchangeRate), x => TransferRowTo.AH_ExchangeRateInfo); }
		}

		#endregion

		#region LocalBuyAmount

		public ZDecimal LocalBuyAmount
		{
			get => BankTransferStrategy.GetLocalBuyAmount();
			set
			{
				BankTransferStrategy.SetLocalBuyAmount(value);
				RefreshBinding();
			}
		}

		public ZPropertyInfo LocalBuyAmountInfo => GetWrappedZPropertyInfo(nameof(LocalBuyAmount), x => TransferRowTo.AH_LocalExTaxAmountInfo);

		public bool LocalBuyAmount_ReadOnly
		{
			get
			{
				return BuySellAmountsAndRatesReadOnly || BuyCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			}
		}

		[ReadOnly(true)]
		[List("CurrencyLookUp")]
		public ZString LocalCurrency
		{
			get { return TransferRowTo.AH_Calc_LocalRXCode; }
		}

		public ZPropertyInfo LocalCurrencyInfo
		{
			get { return TransferRowTo == null ? null : GetWrappedZPropertyInfo(nameof(LocalCurrency), x => TransferRowTo.AH_Calc_LocalRXCodeInfo); }
		}

		public ZInt LocalAmountDecimals
		{
			get { return TransferRowTo.AH_Calc_LocalRXDecimals; }
		}

		public ZPropertyInfo LocalAmountDecimalsInfo
		{
			get { return TransferRowTo == null ? null : GetWrappedZPropertyInfo(nameof(LocalAmountDecimals), x => TransferRowTo.AH_Calc_LocalRXDecimalsInfo); }
		}

		#endregion

		#region LocalSellAmount

		public ZDecimal LocalSellAmount
		{
			get => BankTransferStrategy.GetLocalSellAmount();
			set
			{
				BankTransferStrategy.SetLocalSellAmount(value);
				TransferRowFrom.AH_LocalExTaxAmountInfo.RefreshBinding();
				RefreshBinding();
			}
		}

		public ZPropertyInfo LocalSellAmountInfo => GetWrappedZPropertyInfo(nameof(LocalSellAmount), x => TransferRowFrom.AH_LocalExTaxAmountInfo);

		public bool LocalSellAmount_ReadOnly
		{
			get
			{
				var isLocalCurrency = SellCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				return BuySellAmountsAndRatesReadOnly || isLocalCurrency;
			}
		}

		#endregion

		#region SellExchangeRate

		public ZDecimal SellExchangeRate
		{
			get { return BankTransferStrategy.GetSellExchangeRate(); }
			set
			{
				BankTransferStrategy.SetSellExchangeRate(value);
				bankTransferFundingInfoCalculator?.CalculateBankTransferWithFundingCurrency();
				RefreshBinding();
			}
		}

		public ZPropertyInfo SellExchangeRateInfo
		{
			get { return TransferRowFrom == null ? null : GetWrappedZPropertyInfo(nameof(SellExchangeRate), x => TransferRowFrom.AH_ExchangeRateInfo); }
		}

		#endregion

		#region SellCurrency

		[List("CurrencyLookUp")]
		public ZString SellCurrency
		{
			get
			{
				var result = ZString.Empty;
				if (!this.HasContext(BusinessContext.PopulateBankTransferWithEPaymentData))
				{
					result = TransferRowFrom.BankAccount != null ? TransferRowFrom.BankAccount.AB_RX_NKAccountCurrency : ZString.Empty;
				}
				else
				{
					if (TransferRowFrom.BankAccount != null)
					{
						result = TransferRowFrom.BankAccount.AB_RX_NKAccountCurrency;
					}
					else if (TransferRowTo.BankAccount != null)
					{
						result = TransferRowTo.BankAccount.AB_RX_NKAccountCurrency;
					}
				}
				return result;
			}
		}

		public ZPropertyInfo SellCurrencyInfo
		{
			get { return GetZPropertyInfo(nameof(SellCurrency)); }
		}

		#endregion

		#region FinanceCharge

		public ZString FinanceChargeTransactionNum
		{
			get { return FinanceCharge.AH_TransactionNum; }
			set { FinanceCharge.AH_TransactionNum = value; }
		}

		public ZPropertyInfo FinanceChargeTransactionNumInfo
		{
			get { return FinanceCharge == null ? null : GetWrappedZPropertyInfo(nameof(FinanceChargeTransactionNum), x => FinanceCharge.AH_TransactionNumInfo); }
		}

		public ZString FinanceChargeDescription
		{
			get { return FinanceCharge.AH_Desc; }
			set { FinanceCharge.AH_Desc = value; }
		}

		public ZPropertyInfo FinanceChargeDescriptionInfo
		{
			get { return FinanceCharge == null ? null : GetWrappedZPropertyInfo(nameof(FinanceChargeDescription), x => FinanceCharge.AH_DescInfo); }
		}

		protected bool FinanceChargeDescription_ReadOnly
		{
			get { return !EnableFinanceCharge; }
		}

		[List("BankAccounts")]
		public ZGuid FinanceChargeBankPK
		{
			get { return FinanceCharge.AH_AB; }
			set
			{
				FinanceCharge.AH_AB = value;
				if (FinanceCharge.BankAccount != null)
				{
					FinanceChargeLine.AL_RX_NKTransactionCurrency = FinanceCharge.BankAccount.AB_RX_NKAccountCurrency;
				}
				else
				{
					FinanceChargeLine.AL_RX_NKTransactionCurrency = ZString.Empty;
				}
				RefreshBinding();
			}
		}

		public ZPropertyInfo FinanceChargeBankPKInfo
		{
			get { return FinanceCharge == null ? null : GetWrappedZPropertyInfo(nameof(FinanceChargeBankPK), x => FinanceCharge.AH_ABInfo); }
		}

		protected bool FinanceChargeBankPK_ReadOnly
		{
			get { return !EnableFinanceCharge; }
		}

		public ZDecimal FinanceChargeOSAmount
		{
			get { return FinanceChargeLine.AL_OSExTaxAmount; }
			set
			{
				FinanceChargeLine.AL_OSExTaxAmount = value;
				RefreshBinding();
			}
		}

		public ZPropertyInfo FinanceChargeOSAmountInfo
		{
			get { return FinanceChargeLine == null ? null : GetWrappedZPropertyInfo(nameof(FinanceChargeOSAmount), x => FinanceChargeLine.AL_OSExTaxAmountInfo); }
		}

		protected bool FinanceChargeOSAmount_ReadOnly
		{
			get { return !EnableFinanceCharge; }
		}

		public ZInt FinanceChargeOSAmountDecimals
		{
			get { return FinanceCharge.AH_Calc_RXDecimals; }
		}

		public ZPropertyInfo FinanceChargeOSAmountDecimalsInfo
		{
			get { return FinanceCharge == null ? null : GetWrappedZPropertyInfo(nameof(FinanceChargeOSAmountDecimals), x => FinanceCharge.AH_Calc_RXDecimalsInfo); }
		}

		[List("CurrencyLookUp")]
		public ZString FinanceChargeOSAmountCurrency
		{
			get { return FinanceCharge.BankAccount != null ? FinanceCharge.BankAccount.AB_RX_NKAccountCurrency : ZString.Empty; }
		}

		public ZPropertyInfo FinanceChargeOSAmountCurrencyInfo
		{
			get { return GetZPropertyInfo(nameof(FinanceChargeOSAmountCurrency)); }
		}

		public ZDecimal FinanceChargeExchangeRate
		{
			get { return FinanceCharge.AH_ExchangeRate; }
			set { FinanceCharge.AH_ExchangeRate = value; }
		}

		public ZPropertyInfo FinanceChargeExchangeRateInfo
		{
			get { return FinanceCharge == null ? null : GetWrappedZPropertyInfo(nameof(FinanceChargeExchangeRate), x => FinanceCharge.AH_ExchangeRateInfo); }
		}

		protected bool FinanceChargeExchangeRate_ReadOnly
		{
			get
			{
				return !EnableFinanceCharge ||
					((FinanceCharge?.BankAccount == null) || FinanceCharge.BankAccount.AB_RX_NKAccountCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			}
		}

		public ZDecimal FinanceChargeLocalAmount
		{
			get { return FinanceChargeLine.AL_LocalExTaxAmount; }
			set
			{
				FinanceChargeLine.AL_LocalExTaxAmount = value;

				ZString currencyNK = FinanceCharge.BankAccount != null ? FinanceCharge.BankAccount.AB_RX_NKAccountCurrency : ZString.Empty;

				if (currencyNK != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
				{
					FinanceCharge.AH_ExchangeRate = Env.CurrentCompany.ExchangeRate.GetRate(FinanceChargeLine.AL_LocalExTaxAmount, FinanceChargeOSAmount);
				}
				RefreshBinding();
			}
		}

		public ZPropertyInfo FinanceChargeLocalAmountInfo
		{
			get { return FinanceChargeLine == null ? null : GetWrappedZPropertyInfo(nameof(FinanceChargeLocalAmount), x => FinanceChargeLine.AL_LocalExTaxAmountInfo); }
		}

		protected bool FinanceChargeLocalAmount_ReadOnly
		{
			get
			{
				return !EnableFinanceCharge ||
					((FinanceCharge?.BankAccount == null) || FinanceCharge.BankAccount.AB_RX_NKAccountCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			}
		}

		public ZInt FinanceChargeLocalAmountDecimals
		{
			get { return FinanceCharge.AH_Calc_LocalRXDecimals; }
		}

		public ZPropertyInfo FinanceChargeLocalAmountDecimalsInfo
		{
			get { return FinanceCharge == null ? null : GetWrappedZPropertyInfo(nameof(FinanceChargeLocalAmountDecimals), x => FinanceCharge.AH_Calc_LocalRXDecimalsInfo); }
		}

		[List("CurrencyLookUp")]
		public ZString FinanceChargeLocalAmountCurrency
		{
			get { return FinanceCharge.AH_Calc_LocalRXCode; }
		}

		public ZPropertyInfo FinanceChargeLocalAmountCurrencyInfo
		{
			get { return FinanceCharge == null ? null : GetWrappedZPropertyInfo(nameof(FinanceChargeLocalAmountCurrency), x => FinanceCharge.AH_Calc_LocalRXCodeInfo); }
		}

		[List("TaxRates")]
		public ZGuid FinanceChargeTaxID
		{
			get { return FinanceChargeLine.AL_AT; }
			set
			{
				FinanceChargeLine.AL_AT = value;
				RefreshBinding();
			}
		}

		public ZPropertyInfo FinanceChargeTaxIDInfo
		{
			get { return FinanceChargeLine == null ? null : GetWrappedZPropertyInfo(nameof(FinanceChargeTaxID), x => FinanceChargeLine.AL_ATInfo); }
		}

		protected bool FinanceChargeTaxID_ReadOnly
		{
			get
			{
				return !EnableFinanceCharge;
			}
		}

		public ZDate FinanceChargeTaxDate
		{
			get => FinanceChargeLine.AL_TaxDate;
			set
			{
				FinanceChargeLine.AL_TaxDate = value;
				RefreshBinding();
			}
		}

		public ZPropertyInfo FinanceChargeTaxDateInfo => GetWrappedZPropertyInfo(nameof(FinanceChargeTaxDate), x => FinanceChargeLine?.AL_TaxDateInfo);

		protected bool FinanceChargeTaxDate_ReadOnly => !EnableFinanceCharge;

		public ZDecimal FinanceChargeOSTaxAmount
		{
			get { return FinanceChargeLine.AL_OSTaxAmount; }
			set
			{
				FinanceChargeLine.AL_OSTaxAmount = value;
				RefreshBinding();
			}
		}

		public ZPropertyInfo FinanceChargeOSTaxAmountInfo
		{
			get { return FinanceChargeLine == null ? null : GetWrappedZPropertyInfo(nameof(FinanceChargeOSTaxAmount), x => FinanceChargeLine.AL_OSTaxAmountInfo); }
		}

		protected bool FinanceChargeOSTaxAmount_ReadOnly
		{
			get
			{
				return !EnableFinanceCharge ||
					(FinanceChargeLine == null || FinanceChargeLine.AL_TaxRateCalc == 0);
			}
		}

		[ReadOnly(true)]
		public ZDecimal FinanceChargeLocalTaxAmount
		{
			get { return FinanceCharge.AH_LocalTaxAmount; }
		}

		public ZPropertyInfo FinanceChargeLocalTaxAmountInfo
		{
			get { return FinanceCharge == null ? null : GetWrappedZPropertyInfo(nameof(FinanceChargeLocalTaxAmount), x => FinanceCharge.AH_LocalTaxAmountInfo); }
		}

		public ZDecimal FinanceChargeOSTotal
		{
			get { return FinanceCharge.AH_OSTotalAmount; }
		}

		public ZPropertyInfo FinanceChargeOSTotalInfo
		{
			get { return FinanceCharge == null ? null : GetWrappedZPropertyInfo(nameof(FinanceChargeOSTotal), x => FinanceCharge.AH_OSTotalAmountInfo); }
		}

		public ZDecimal FinanceChargeTotalLocalAmount
		{
			get { return FinanceCharge.AH_LocalTotalAmount; }
		}

		public ZPropertyInfo FinanceChargeTotalLocalAmountInfo
		{
			get { return FinanceCharge == null ? null : GetWrappedZPropertyInfo(nameof(FinanceChargeTotalLocalAmount), x => FinanceCharge.AH_LocalTotalAmountInfo); }
		}

		public ZString FinanceChargeGovtChargeCode
		{
			get => FinanceChargeLine?.AL_GovtChargeCode ?? ZString.Empty;
			set
			{
				if (FinanceChargeLine != null)
				{
					FinanceChargeLine.AL_GovtChargeCode = value;
					FinanceChargeGovtChargeCodeInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo FinanceChargeGovtChargeCodeInfo => GetWrappedZPropertyInfo(nameof(FinanceChargeGovtChargeCode), x => FinanceChargeLine?.AL_GovtChargeCodeInfo);

		protected bool FinanceChargeGovtChargeCode_ReadOnly => !EnableFinanceCharge;

		DirectPaymentLine FinanceChargeLine
		{
			get { return FinanceCharge == null ? null : (DirectPaymentLine)FinanceCharge.Lines[0]; }
		}

		#endregion

		#region ExchangeVariance

		[ReadOnlyMember(nameof(ShouldCalculateExchangeVarianceIsReadOnly))]
		[ResourceStringData("BankTransferForm|bb0d6590-de87-4f81-a299-6e6494d495fe",
			Caption = "Calculate Exchange Variance",
			FullDescription = @"When this checkbox is ticked, the Sell Local and Buy Local amounts will calculate based on Sell Amount and Exchange Rate and Buy Amount and Exchange Rate.
The difference between the Sell and Buy Local amounts will populate to the Exchange Gain/Loss field and will post as an EXX transaction when the Bank Transfer is posted.
When this checkbox is not ticked, Sell Local and Buy Local amounts will be equal.")]
		public ZBool ShouldCalculateExchangeVariance
		{
			get => shouldCalculateExchangeVariance;
			set
			{
				if (ShouldCalculateExchangeVariance == value)
				{
					return;
				}

				if (value || (OnCancelingCalculateExchangeVariance?.Invoke() ?? true))
				{
					SetNonPersistentPropertyValue(ShouldCalculateExchangeVarianceInfo, ref shouldCalculateExchangeVariance, value);
					SetBankTransferStrategy();
				}

				RefreshBinding();
			}
		}

		ZBool shouldCalculateExchangeVariance;

		public event Func<bool> OnCancelingCalculateExchangeVariance;

		public ZPropertyInfo ShouldCalculateExchangeVarianceInfo
		{
			get { return GetZPropertyInfo(nameof(ShouldCalculateExchangeVariance)); }
		}

		internal bool ShouldCalculateExchangeVarianceIsReadOnly
		{
			get => AreBothBuyAndSellCurrencyLocal || bankTransferFundingInfoCalculator != null;
		}

		bool AreBothBuyAndSellCurrencyLocal => TransferRowFrom.BankAccount != null && BuyCurrency == SellCurrency && SellCurrency == LocalCurrency;

		public bool ShouldPostExchangeDiff => ShouldCalculateExchangeVariance && ExRateGainLoss != 0m;

		[ReadOnly(true)]
		[ResourceStringData("BankTransferForm|ED1F1AE5-9C1D-4F97-8D53-2A88E2F50EAA",
					Caption = "Exchange Gain/Loss",
					FullDescription = "When the Calculate Exchange Variance checkbox is ticked, the difference between the Sell Local and Buy Local amounts will populate this field, and will post as an EXX transaction when the Bank Transfer is posted.")]
		public ZDecimal ExRateGainLoss
		{
			get
			{
				return ExchangeDiff.IsInDatabase ? ExchangeDiff.AH_InvoiceAmount : (ZDecimal)(LocalBuyAmount - LocalSellAmount);
			}
		}

		#endregion

		#endregion

		#region ICashBook Interface Members

		public ZString Ledger
		{
			get { return LedgerTypes.CashBook; }
		}

		public ZPropertyInfo LedgerInfo
		{
			get { return GetZPropertyInfo(nameof(Ledger)); }
		}

		public ZString TransactionType
		{
			// No particular Transaction Type for the Top Level BizO
			get { return ZString.Empty; }
		}

		public ZPropertyInfo TransactionTypeInfo
		{
			get { return GetZPropertyInfo(nameof(TransactionType)); }
		}

		public ZDateTime TransactionDate
		{
			get { return TransferRowFrom.AH_InvoiceDate; }
			set
			{
				TransferRowFrom.AH_InvoiceDate = value;
				TransferRowTo.AH_InvoiceDate = value;
				FinanceCharge.AH_InvoiceDate = value;
				FinanceCharge.AH_DueDate = value;
			}
		}

		public ZPropertyInfo TransactionDateInfo
		{
			get { return TransferRowFrom == null ? null : GetWrappedZPropertyInfo(nameof(TransactionDate), x => TransferRowFrom.AH_InvoiceDateInfo); }
		}

		public ZString CurrencyCode
		{
			get { return TransferRowFrom.AH_RX_NKTransactionCurrency; }
		}

		public ZPropertyInfo CurrencyCodeInfo
		{
			get { return TransferRowFrom == null ? null : GetWrappedZPropertyInfo(nameof(TransactionDate), x => TransferRowFrom.AH_RX_NKTransactionCurrencyInfo); }
		}

		public ZDecimal OverseasTotalAmount
		{
			get { return 0m; }
		}

		public ZPropertyInfo OverseasTotalAmountInfo
		{
			get { return GetZPropertyInfo(nameof(OverseasTotalAmount)); }
		}

		public bool IsReversing
		{
			get { return fIsReversing; }
		}
		bool fIsReversing;

		public bool IsReversed
		{
			get { return TransferRowFrom.AH_IsCancelled; }
		}

		public static BankTransfer PrepareBankTransferFromPayments(IEnumerable<Payment> paymentsWithTheSameBankAccount, ZString? paymentBatchReference = null)
		{
			var factory = new BusinessObjectFactory();
			var transfer = new BankTransfer(factory, null);

			transfer.SetContext(BusinessContext.PopulateBankTransferWithEPaymentData);
			var firstPayment = paymentsWithTheSameBankAccount.First();
			transfer.BankTransferToPK = firstPayment.AH_AB;
			transfer.SellAmount = transfer.BuyAmount = transfer.LocalBuyAmount = transfer.LocalSellAmount = paymentsWithTheSameBankAccount.Sum(x => x.DealTotalCost);
			transfer.TransactionDate = ZDateTime.Today;
			transfer.AH_PostDate = ZDateTime.Today;
			var referenceText = ZString.Empty;
			if (paymentBatchReference.HasValue && !paymentBatchReference.Value.IsEmpty)
			{
				referenceText = Res.GetString("536F1B46-1A3C-475B-A648-4F936CF99F3D", "{0} PAYMENT BATCH {1}", firstPayment.DealProvider, paymentBatchReference.Value);
				var paymentBatch = GetPaymentBatch(factory, paymentBatchReference.Value);
				transfer.bankTransferFundingInfoCalculator = new BankTransferFundingInfoCalculator(transfer);
				transfer.bankTransferFundingInfoCalculator.ConfigureBankTransferFromPaymentBatch(paymentBatch, paymentsWithTheSameBankAccount);
			}
			else
			{
				referenceText = paymentsWithTheSameBankAccount.Count() == 1 ?
					Res.GetString("901C27A9-57F3-4AA8-A93E-73D48274F7F3", "{0} PAYMENT {1}", firstPayment.DealProvider, firstPayment.AH_TransactionNum) :
					Res.GetString("810CEA45-9C82-4AC1-AAC4-2F530993046F", "{0} MULTIPLE PAYMENTS", firstPayment.DealProvider);
				transfer.bankTransferFundingInfoCalculator = new BankTransferFundingInfoCalculator(transfer);
				transfer.bankTransferFundingInfoCalculator.ConfigureBankTransferFromSinglePayments(paymentsWithTheSameBankAccount);
			}
			transfer.Reference = referenceText.Truncate(AccTransactionHeaderSchema.AH_ChequeOrReference.MaxLength);

			transfer.EnableFinanceCharge = GetOSFinanceCharge(paymentsWithTheSameBankAccount) > 0;
			if (transfer.EnableFinanceCharge)
			{
				transfer.FinanceChargeBankPK = firstPayment.AH_AB;
				if (transfer.bankTransferFundingInfoCalculator == null || transfer.bankTransferFundingInfoCalculator.FundingCurrency.IsEmpty)
				{
					transfer.FinanceChargeOSAmount = GetOSFinanceCharge(paymentsWithTheSameBankAccount);
				}
				transfer.FinanceChargeTaxDate = ZDate.Today;
				var filter = new ZQuery(AccTaxRateSchema.AT_Code, "EXEMPT");
				filter.AddToFilter(AccTaxRateSchema.AT_RN_NKCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				var tax = factory.LoadTop1<AccTaxRate>(filter);
				transfer.FinanceChargeTaxID = tax?.PK ?? ZGuid.Empty;
			}
			return transfer;
		}

		static APPaymentBatchPoster GetPaymentBatch(BusinessObjectFactory factory, ZString paymentBatchReference)
		{
			var query = new ZQuery();
			query.AddToFilter(AccPaymentBatchSchema.APB_BatchNumber, paymentBatchReference);
			query.AddToFilter(AccPaymentBatchSchema.APB_GC, Env.CurrentCompanyPK);
			var paymentBatch = factory.LoadTop1<APPaymentBatchPoster>(query);
			return paymentBatch;
		}

		static decimal GetOSFinanceCharge(IEnumerable<Payment> paymentsWithTheSameBankAccount) => paymentsWithTheSameBankAccount.Sum(x => x.DealTotalFees);

		public ZBool AH_NumberOfSupportingDocumentsVisible_ReadOnly
		{
			get { return GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.China; }
		}

		public void GenerateReverseTransaction(bool mustTransform)
		{
			fIsReversing = true;

			fReverseTransaction = new BankTransfer(Factory, null);
			fReverseTransaction.IsReverseTransaction = true;
			fReverseTransaction.OriginalTransaction = this;

			fReverseTransaction.TransferRowFrom.BankTransferParent = fReverseTransaction;
			fReverseTransaction.TransferRowTo.BankTransferParent = fReverseTransaction;
			fReverseTransaction.FinanceCharge.BankTransferParent = fReverseTransaction;
			fReverseTransaction.TransferRowFrom.AH_TransactionCount = AccTransactionHeader.TransactionCountConstants.BankTransferFromRowWhenReversing;
			fReverseTransaction.TransferRowTo.AH_TransactionCount = AccTransactionHeader.TransactionCountConstants.BankTransferToRowWhenReversing;

			fReverseTransaction.ShouldCalculateExchangeVariance = ShouldCalculateExchangeVariance;
			fReverseTransaction.BankTransferFromPK = BankTransferToPK;
			fReverseTransaction.BankTransferToPK = BankTransferFromPK;

			BankTransferStrategy.UpdateReverseTransaction(fReverseTransaction);

			fReverseTransaction.Reference = Reference;

			fReverseTransaction.EnableFinanceCharge = EnableFinanceCharge;
			fReverseTransaction.FinanceCharge.AH_TransactionCount = BankTransferCharge.TransactionCountWhenReversing;
			fReverseTransaction.FinanceChargeBankPK = FinanceChargeBankPK;
			fReverseTransaction.FinanceChargeExchangeRate = FinanceChargeExchangeRate;
			fReverseTransaction.FinanceChargeOSAmount = -FinanceChargeOSAmount;
			fReverseTransaction.FinanceChargeTaxID = FinanceChargeTaxID;
			fReverseTransaction.FinanceChargeOSTaxAmount = -FinanceChargeOSTaxAmount;
			fReverseTransaction.FinanceChargeGovtChargeCode = FinanceChargeGovtChargeCode;

			fReverseTransaction.ExchangeDiff.OriginalTransaction = ExchangeDiff;
			fReverseTransaction.ExchangeDiff.AH_TransactionCount = TransactionCountConstants.BankTransferExchangeDiffWhenReversing;
		}

		public IReversing ReverseTransaction
		{
			get { return fReverseTransaction; }
		}
		BankTransfer fReverseTransaction;

		public bool IsReverseTransaction
		{
			get { return fIsReverseTransaction; }
			set
			{
				fIsReverseTransaction = value;
				TransferRowFrom.IsReverseTransaction = value;
				TransferRowTo.IsReverseTransaction = value;
				ExchangeDiff.IsReverseTransaction = value;
				FinanceCharge.IsReverseTransaction = value;
			}
		}
		bool fIsReverseTransaction;

		public BankTransfer OriginalTransaction
		{
			get { return fOriginalTransaction; }
			set { fOriginalTransaction = value; }
		}
		BankTransfer fOriginalTransaction;

		public void SetCancellationFlag(bool cancel)
		{
			TransferRowFrom.AH_IsCancelled = cancel;
			TransferRowTo.AH_IsCancelled = cancel;
			ExchangeDiff.AH_IsCancelled = cancel;
			if (FinanceCharge != null)
			{
				FinanceCharge.AH_IsCancelled = cancel;
			}
		}

		public void SetTransactionBelongsToGroupField(ZGuid groupingGuidValue)
		{
		}

		public void SetDescription(ZString descriptionToSet)
		{
			Description = descriptionToSet;
			FinanceCharge.AH_Desc = string.Format(CultureInfo.InvariantCulture, AccountingConfigurationRegistry.Instance.GetTransactionDescriptionFromCode(AccountingConstants.VoucherItemRegistryCode.ReversalRelated,
																				Res.GetString("9F95CDE5-D0EC-4be7-B53E-244A71381CB9", "Reversal related to")) + " {0}", OriginalTransaction.FinanceCharge.AH_TransactionNum);
		}

		public void SetNumberOfSupportingDocuments(ZByte numberOfSupportingDocumentsToSet)
		{
			AH_NumberOfSupportingDocuments = numberOfSupportingDocumentsToSet;
		}

		public void ApplyWorkflowTemplatesOnReverseTransaction()
		{
		}

		public ZString ReversingReason
		{
			get { return fReversingReason; }
			set
			{
				Description = new ZString(Description + " " + value).Left(DescriptionInfo.MaxLength);
				FinanceCharge.AH_Desc = new ZString(FinanceCharge.AH_Desc + " " + value).Left(DescriptionInfo.MaxLength);
				fReversingReason = value;
			}
		}
		ZString fReversingReason;

		public ZString ReversingCode
		{
			get { return fReversingCode; }
			set { fReversingCode = value; }
		}
		ZString fReversingCode;

		public bool IsClearedInCashbook
		{
			get { return false; }
		}

		public ZGuid Organization
		{
			get
			{
				return ZGuid.Empty;
			}
			set
			{
				// Do not do anything for Bank Transfer does not involve Organization
			}
		}

		public ZPropertyInfo OrganizationInfo
		{
			get { return GetZPropertyInfo(nameof(Organization)); }
		}

		public ZString OriginalTransactionNumber
		{
			get { return OriginalTransaction != null ? OriginalTransaction.TransactionNumber : ZString.Empty; }
		}

		public ZPropertyInfo OriginalTransactionNumberInfo
		{
			get { return GetZPropertyInfo(nameof(OriginalTransactionNumber)); }
		}

		public bool OriginalTransactionNumber_ReadOnly { get { return true; } }

		public ZString OriginalTransactionType
		{
			get { return OriginalTransaction != null ? OriginalTransaction.TransactionType : ZString.Empty; }
		}

		public ZPropertyInfo OriginalTransactionTypeInfo
		{
			get { return GetZPropertyInfo(nameof(OriginalTransactionType)); }
		}

		public bool OriginalTransactionType_ReadOnly { get { return true; } }

		[BusinessObjectTestExclude]
		public ZString SupportingDocumentNumber
		{
			get { return ZString.Empty; }
			set { }
		}

		public ZPropertyInfo SupportingDocumentNumberInfo
		{
			get { return GetZPropertyInfo(nameof(SupportingDocumentNumber)); }
		}

		public OrgHeaderCollection Headers
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		public string[] MultipleReversingErrors
		{
			get { return MultipleReversingErrors_innerValue; }
			set { MultipleReversingErrors_innerValue = value; }
		}
		string[] MultipleReversingErrors_innerValue = Array.Empty<string>();

		public ZDateTime ITransactionPostDate
		{
			get { return PostDate; }
			set
			{
				PostDate = value;
			}
		}

		public ZPropertyInfo ITransactionPostDateInfo
		{
			get { return PostDateInfo; }
		}

		public ZDateTime UnmatchDate
		{
			get { return unmatchDate; }
			set { SetNonPersistentPropertyValue(UnmatchDateInfo, ref unmatchDate, value); }
		}
		ZDateTime unmatchDate;

		public bool UnmatchDate_ReadOnly
		{
			get { return true; }
		}

		public ZPropertyInfo UnmatchDateInfo
		{
			get { return GetZPropertyInfo(nameof(UnmatchDate)); }
		}

		#region ReversalStatusCode

		[BusinessObjectTestExclude]
		public ZString ReversalStatusCode
		{
			get { return ZString.Empty; }
			set { }
		}

		public ZPropertyInfo ReversalStatusCodeInfo => GetZPropertyInfo(nameof(ReversalStatusCode));

		bool ITransaction.ReversalStatusCode_ReadOnly => true;

		ReadOnlyCodeDescriptionPairList ITransaction.ReversalStatusCodeList => null;

		#endregion

		#endregion

		#region IDataExportBatchSource Members

		ZBool IDataExportBatchSource.IsDataExportBatchSupported
		{
			get
			{
				var source = TransferRowFrom as IDataExportBatchSource;
				return source != null && source.IsDataExportBatchSupported;
			}
		}

		public DataExportBatchDependentCollection DataExportBatchCollection
		{
			get
			{
				if (!relatedBatchCollectionIsLoaded && relatedBatchCollection == null)
				{
					var source = TransferRowFrom as IDataExportBatchSource;
					if (source != null)
					{
						relatedBatchCollection = new DataExportBatchDependentCollection(source);
						relatedBatchCollection.Load();
					}
					relatedBatchCollectionIsLoaded = true;
				}
				return relatedBatchCollection;
			}
		}
		bool relatedBatchCollectionIsLoaded;
		DataExportBatchDependentCollection relatedBatchCollection;

		#endregion

		#region IHandleDeleteError Members

		bool IHandleDeleteError.RollbackAfterDeleteError
		{
			get { return TransferRowFrom.IsInDatabase && (TransferRowFrom.IsDeleted || !(TransferRowFrom.IsCancelled && TransferRowFrom.IsCancelledHasChanged)); }
		}

		bool IHandleDeleteError.RebindAfterDeleteError
		{
			get { return false; }
		}

		bool IHandleDeleteError.DisableFormOnDeleteConcurrencyError
		{
			get { return !((IHandleDeleteError)this).RollbackAfterDeleteError; }
		}

		#endregion

		#region ITemplateCopyable Members

		public IBusiness TemplateCopy()
		{
			var copyOfCurrent = new BankTransfer(Factory, null);
			copyOfCurrent.Description = this.Description;
			copyOfCurrent.BankTransferFromPK = this.BankTransferFromPK;
			copyOfCurrent.BankTransferToPK = this.BankTransferToPK;
			copyOfCurrent.SellAmount = this.SellAmount; //Copy the selling amount. The buying amount will be automatically calculated based on the current exchange rate
			copyOfCurrent.FinanceChargeDescription = this.FinanceChargeDescription;

			return copyOfCurrent;
		}

		#endregion

		#region Implementation

		void ConfigureTransferRowFrom(string transactionNum)
		{
			TransferRowFrom.AH_TransactionNum = transactionNum;
			TransferRowFrom.AH_FullyPaidDate = AH_PostDate;
		}

		void ConfigureTransferRowTo(string transactionNum)
		{
			TransferRowTo.AH_TransactionNum = transactionNum;
			TransferRowTo.AH_FullyPaidDate = AH_PostDate;
		}

		void ConfigureFinanceCharge()
		{
			if (FinanceCharge.EnableFinanceCharge)
			{
				FinanceCharge.AH_NumberOfSupportingDocuments = AccountingConfigurationRegistry.Instance.GetVoucherNoOfAttchmentsFromCode(AccountingConstants.VoucherItemRegistryCode.FinanceCharge, 0);
				FinanceCharge.AH_PostDate = AH_PostDate;
			}
		}

		void ConfigureExchangeDiff()
		{
			if (ShouldPostExchangeDiff)
			{
				var exchangeDiffCreator = new BankTransferCashbookExchangeDiffCreator();
				exchangeDiffCreator.Fill(this, ExchangeDiff);

				using (ExchangeDiff.ResumeValidationTemporarily())
				{
					ExchangeDiff.Validation.ValidateAll();

					if (ExchangeDiff.HasErrors)
					{
						throw new ZCannotSaveException(ExchangeDiff.GetErrors().ToMessageListString(), Res.GetString("46EABA58-1561-4DAE-A1FE-18EA153B6F6C", "Error occurred while creating Exchange Gain/Loss"));
					}
				}
			}
		}

		void LoadTransferRowFrom(BankTransferFromRow transferRow)
		{
			TransferRowFrom = transferRow;
			TransferRowFrom.BankTransferParent = this;
			TransferRowFrom.TransferType = TransferType.TransferFrom;
		}

		void LoadTransferRowTo()
		{
			var rowToTransactionCount = TransferRowFrom.IsReversal ? TransactionCountConstants.BankTransferToRowWhenReversing : TransactionCountConstants.BankTransferToRow;
			var rowToFilter = AccountingUtils.GetTransactionFilter(transactionBelongsToGroupGuid, TransferRowFrom.AH_GC, rowToTransactionCount);
			TransferRowTo = Factory.LoadTop1<BankTransferToRow>(rowToFilter);
			TransferRowTo.BankTransferParent = this;
		}

		void LoadOrCreateExchangeDiff()
		{
			var exxDiffTransactionCount = TransferRowFrom.IsReversal ? TransactionCountConstants.BankTransferExchangeDiffWhenReversing : TransactionCountConstants.BankTransferExchangeDiff;
			var exxDiffFilter = AccountingUtils.GetTransactionFilter(transactionBelongsToGroupGuid, TransferRowFrom.AH_GC, exxDiffTransactionCount);
			ExchangeDiff = Factory.LoadTop1<CashbookExchangeDiff>(exxDiffFilter);

			if (ExchangeDiff == null)
			{
				ExchangeDiff = Factory.New<CashbookExchangeDiff>();
				FillExchangeDiff();
			}
			else if (ExchangeDiff.IsInDatabase)
			{
				ShouldCalculateExchangeVariance = true;
			}

			ExchangeDiff.BankTransferParent = this;
		}

		void LoadOrCreateFinanceCharge()
		{
			var financeChargeTransactionCount = TransferRowFrom.IsReversal ? BankTransferCharge.TransactionCountWhenReversing : BankTransferCharge.TransactionCount;
			var dPYFilter = AccountingUtils.GetTransactionFilter(transactionBelongsToGroupGuid, TransferRowFrom.AH_GC, financeChargeTransactionCount);
			FinanceCharge = Factory.LoadTop1<BankTransferCharge>(dPYFilter);

			if (FinanceCharge == null)
			{
				FinanceCharge = Factory.New<BankTransferCharge>();
			}
			else
			{
				EnableFinanceCharge = true;
			}

			FinanceCharge.BankTransferParent = this;
		}

		void FillExchangeDiff()
		{
			ExchangeDiff.SuspendValidation();
			ExchangeDiff.BankTransferParent = this;
			ExchangeDiff.AH_TransactionBelongsToGroup = transactionBelongsToGroupGuid;
			ExchangeDiff.AH_TransactionCount = AccTransactionHeader.TransactionCountConstants.BankTransferExchangeDiff;
		}

		readonly ZGuid transactionBelongsToGroupGuid;
		public BankTransferFromRow TransferRowFrom { get; set; }
		public BankTransferToRow TransferRowTo { get; set; }
		internal CashbookExchangeDiff ExchangeDiff;
		internal BankTransferCharge FinanceCharge;
		public BankTransferFundingInfoCalculator bankTransferFundingInfoCalculator;

		#endregion
	}
}

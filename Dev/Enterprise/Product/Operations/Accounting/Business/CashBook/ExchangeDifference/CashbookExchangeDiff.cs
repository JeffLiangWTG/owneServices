using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.Transfer;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.CashBook.ExchangeDifference
{
	public class CashbookExchangeDiff : TransactionHeader, ICashBook, IDocManagerSupport, IEDocsParsingSupport
	{
		public CashbookExchangeDiff(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			AH_InvoiceDate = ZDateTime.Today;
			AH_TransactionCategory = Core.Constants.TransactionCategory.Codes.UnrealizedExchangeGainLoss;
		}

		#endregion

		#region Overrides

		protected override TransactionHeaderValidation GetNewValidationCore()
		{
			return new CashbookExchangeDiffValidation(this);
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("1b299dc8-f50c-40cc-ad22-3bf114e46ce3", "Cashbook Exchange Difference"); }
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			if (ShouldSetAH_AG)
			{
				AH_InvoiceAmount = ForeignCurrencyGainLoss;
				AH_OSTotal = 0m;
				AH_AG = ExchangeGainLossAccount;
			}
			base.OnFactorySavingBeforeTransactionCore();
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (saveSucceeded)
			{
				RefreshBinding();
			}
		}

		protected override void GenerateReverseTransactionCore(bool mustTransform)
		{
			base.GenerateReverseTransactionCore(mustTransform);
			using (fReverseTransaction.GetValidationSuspender())
			using (fReverseTransaction.AmountsCalculationsSuspender.GetSuspender())
			{
				fReverseTransaction.AH_InvoiceAmount = -(AH_InvoiceAmount);
				fReverseTransaction.AH_OutstandingAmount = 0m;
			}
		}

		public override bool AH_RX_NKTransactionCurrency_ReadOnly
		{
			get { return true; }
			set { base.AH_RX_NKTransactionCurrency_ReadOnly = value; }
		}

		protected override bool AH_TransactionNum_ReadOnly
		{
			get { return true; }
		}

		protected override bool InvertSigns
		{
			get { return false; }
		}

		protected override ZString Ledger
		{
			get { return LedgerTypes.CashBook; }
		}

		protected override AccountingNumberFountainWrapper NumberFountainForTransactionNumber
		{
			get { return AccountingNumberFountainWrapperFactory.Instance.ExchangeDifferenceNo; }
		}

		protected override ZString TransactionType
		{
			get { return TransactionTypes.ExchangeDifference; }
		}

		public override ExchangeRateType RateType
		{
			get { return ExchangeRateType.Buy; }
		}

		public override bool IsSavedByFactory => base.IsSavedByFactory && (BankTransferParent?.ShouldPostExchangeDiff ?? true);

		#endregion

		#region Amount Calculation

		protected CashbookExchangeDiffCalculation AmountCalculation
		{
			get
			{
				if (fAmountCalculation == null)
				{
					fAmountCalculation = new CashbookExchangeDiffCalculation(BankAccount);
				}

				fAmountCalculation.BankAccount = BankAccount;
				fAmountCalculation.PostDate = AH_PostDate;

				return fAmountCalculation;
			}
		}

		CashbookExchangeDiffCalculation fAmountCalculation;

		#endregion

		#region Properties

		#region AH_AB

		[ReadOnlyMember(nameof(AH_AB_ReadOnly))]
		[List("BankAccounts")]
		public override ZGuid AH_AB
		{
			get { return base.AH_AB; }
			set
			{
				if (base.AH_AB != value)
				{
					base.AH_AB = value;
					if (BankAccount != null)
					{
						AH_RX_NKTransactionCurrency = BankAccount.AB_RX_NKAccountCurrency;
						AH_ExchangeRate = GetNewExchangeRate();
					}
					else
					{
						AH_ExchangeRate = 0m;
					}
				}
			}
		}

		protected virtual bool AH_AB_ReadOnly => false;

		#endregion

		#region NewExchangeRate

		internal ZDecimal GetNewExchangeRate()
		{
			var rateValidAtDate = AH_PostDate;
			var newExchangeRate = ZDecimal.Zero;
			if (rateValidAtDate.IsValid)
			{
				var rateType = AccountingConfigurationRegistry.Instance.BankCurrencyAdjustmentExchangeRateType.Value;
				if (rateType == Constants.ExchangeRateTypes.Code.PeriodEndRate)
				{
					newExchangeRate = AccountingUtils.GetPeriodEndRateExchangeRate(Factory, ExchangeRate.Currency, PostPeriod);
					if (newExchangeRate.IsEmpty)
					{
						newExchangeRate = ExchangeRateCalculator.GetRate(ExchangeRate.Currency, ExchangeRateType.Buy, rateValidAtDate.ToDateTime());
					}
				}
				else
				{
					newExchangeRate = ExchangeRateCalculator.GetRate(ExchangeRate.Currency, ZArchitecture.Environment.ExchangeRate.GetExchangeRateType(rateType), rateValidAtDate.ToDateTime());
				}
			}
			return newExchangeRate;
		}

		#endregion

		#region BankCurrencyBalance

		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		public ZDecimal BankCurrencyBalance
		{
			get { return AmountCalculation.BankCurrencyBalance; }
		}

		public ZPropertyInfo BankCurrencyBalanceInfo
		{
			get { return GetZPropertyInfo(nameof(BankCurrencyBalance)); }
		}

		#endregion

		#region CurrentExchangeRate

		[DecimalPlaces(nameof(ExchangeRateDecimalPlaces))]
		public ZDecimal CurrentExchangeRate
		{
			get { return AmountCalculation.CurrentExchangeRate; }
		}

		public ZPropertyInfo CurrentExchangeRateInfo
		{
			get { return GetZPropertyInfo(nameof(CurrentExchangeRate)); }
		}

		#endregion

		#region LocalAmountBeforeAdjustment

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public ZDecimal LocalAmountBeforeAdjustment
		{
			get { return AmountCalculation.LocalAmountBeforeAdjustment; }
		}

		public ZPropertyInfo LocalAmountBeforeAdjustmentInfo
		{
			get { return GetZPropertyInfo(nameof(LocalAmountBeforeAdjustment)); }
		}

		#endregion

		#region LocalAmountAfterAdjustment

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public ZDecimal LocalAmountAfterAdjustment
		{
			get { return AmountCalculation.GetLocalAmountAfterAdjustment(AH_ExchangeRate); }
		}

		public ZPropertyInfo LocalAmountAfterAdjustmentInfo
		{
			get { return GetZPropertyInfo(nameof(LocalAmountAfterAdjustment)); }
		}

		#endregion

		#region ForeignCurrencyGainLoss

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public ZDecimal ForeignCurrencyGainLoss => IsInDatabase ? AH_InvoiceAmount : AmountCalculation.GetForeignCurrencyGainLoss(AH_ExchangeRate);

		public ZPropertyInfo ForeignCurrencyGainLossInfo
		{
			get { return GetZPropertyInfo(nameof(ForeignCurrencyGainLoss)); }
		}

		#endregion

		public BankTransfer BankTransferParent { get; set; }

		[List("Lookups.GLHeaders")]
		public ZGuid ExchangeGainLossAccount => ForeignCurrencyGainLoss >= 0 ? AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeGainAccount.Value : AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeLossAccount.Value;

		public ZString ExchangeGainLossAccountDesc => Factory.Load<AccGLHeader>(ExchangeGainLossAccount)?.AG_DescriptionMultilingual;

		#region Debit/Credit

		public override ZDecimal Debit
		{
			get { return DebitForDirectReceiptPayment; }
		}

		public override ZDecimal Credit
		{
			get { return CreditForDirectReceiptPayment; }
		}

		protected override ZDecimal LocalDebitCore
		{
			get { return AH_InvoiceAmount >= 0m ? AH_InvoiceAmount : (ZDecimal)0m; }
		}

		protected override ZDecimal LocalCreditCore
		{
			get { return AH_InvoiceAmount < 0m ? -AH_InvoiceAmount : 0m; }
		}

		#endregion

		public bool IsRealizedExchangeGainLoss => AH_TransactionCategory == Constants.TransactionCategory.Codes.RealizedExchangeGainLoss;

		internal bool ShouldSetAH_AG => !IsInDatabase && !AH_IsCancelled && AH_TransactionCategory != Constants.TransactionCategory.Codes.RealizedExchangeGainLoss;

		#endregion

		#region Lookups

		#region BankAccounts

		public AccBankAccountCollection BankAccounts
		{
			get
			{
				if (fBankAccounts == null)
				{
					ZQuery filter = new ZQuery(AccBankAccountSchema.AB_RX_NKAccountCurrency, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
					filter.AddToFilter(JoinCondition.And, AccBankAccountSchema.AB_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK);
					filter.AddToFilter(AccBankAccountSchema.AB_IsActive, true);
					fBankAccounts = new AccBankAccountCollection(Factory, filter);
					fBankAccounts.SetOverrideNotificationWhenAdditionalFilterNotMet(Res.GetString("a1e8acc7-9808-49d2-8e92-76b03d6e58bc", "This bank account cannot be chosen because it is either inactive or a local currency bank account of which currency adjustment is not applicable. Please choose another bank account."));
				}
				return fBankAccounts;
			}
		}

		AccBankAccountCollection fBankAccounts;

		#endregion

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new AccountingDocManagerInfo(this, Constants.DocManagerCodes.CurrencyAdjustment)); }
		}
		AccountingDocManagerInfo docManagerInfo;

		#endregion

		#region IEDocsParsingSupport Members

		string IEDocsParsingSupport.UtilityData => throw new NotImplementedException();

		bool IEDocsParsingSupport.DenySendForParsing(Guid docPK, string docType, string fileName)
		{
			return true;
		}

		#endregion
	}
}

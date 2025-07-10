using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.EPayment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business
{
	public class EPaymentQuoteSummary : NonPersistentBusinessObject
	{
		public EPaymentQuoteSummary(IEnumerable<EPaymentQuote> quotesToSummarise, ZString providerCode, ZString paymentCurrencyCode, ZString status, ZString errorMessage) : base(quotesToSummarise.FirstOrDefault()?.Factory)
		{
			if ((quotesToSummarise?.Count() ?? 0) == 0 || quotesToSummarise.Any(x => x == null || x.QU_ProviderCode != providerCode || x.QU_RX_NKToCurrency != paymentCurrencyCode || x.QU_Status != status || x.QU_ErrorDescription != errorMessage))
			{
				throw new ArgumentException("quote found that doesn't belong in this summary");
			}
			QuotesToSummarise = quotesToSummarise;
			ProviderCode = providerCode;
			PaymentCurrencyCode = paymentCurrencyCode;
			FundingCurrencyCode = quotesToSummarise.First().QU_RX_NKFromCurrency;
			Status = status;
			// using First because list is grouped by status
			StatusDescription = quotesToSummarise.First().StatusDescription;
			ErrorMessage = errorMessage;

			MostRecentUpdateTime = quotesToSummarise.Max(x => x.QU_LastResponseReceivedUtc);
			TotalPaymentAmount = quotesToSummarise.Sum((x) => x.QU_ToAmount);
			TotalFundingAmount = quotesToSummarise.Sum((x) => x.QU_FromAmount);
			TotalFeeAmount = quotesToSummarise.Sum((x) => x.QU_FeeAmount);
			AverageExRate = TotalPaymentAmount == 0 ? 0 : TotalFundingAmount / TotalPaymentAmount;
			AverageInverseExRate = TotalFundingAmount == 0 ? 0 : TotalPaymentAmount / TotalFundingAmount;
		}

		public IEnumerable<EPaymentQuote> QuotesToSummarise { get; private set; }

		public ZString ProviderCode { get; private set; }
		public ZString PaymentCurrencyCode { get; private set; }
		public ZString FundingCurrencyCode { get; private set; }
		public ZString Status { get; private set; }
		public ZString StatusDescription { get; private set; }
		public ZString ErrorMessage { get; private set; }

		public ZDateTime MostRecentUpdateTime { get; private set; }
		public ZDateTime MostRecentUpdateLocalTime => MostRecentUpdateTime.ToLocalBranchTime();

		[DecimalPlaces(nameof(PaymentCurrencyDecimalPlaces))]
		public ZDecimal TotalPaymentAmount { get; private set; }

		[DecimalPlaces(nameof(FundingCurrencyDecimalPlaces))]
		public ZDecimal TotalFundingAmount { get; private set; }

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		ZDecimal totalFeeAmount;

		[DecimalPlaces(nameof(FundingCurrencyDecimalPlaces))]
		public ZDecimal TotalFeeAmount
		{
			get { return totalFeeAmount; }
			set
			{
				SetNonPersistentPropertyValue(TotalFeeAmountInfo, ref totalFeeAmount, value);
				if (IsValidationSuspended)
				{
				}
				else
				{
					Validation.ValidateTotalFeeAmount();
				}
			}
		}

		public ZPropertyInfo TotalFeeAmountInfo => GetZPropertyInfo(nameof(TotalFeeAmount));

		[DecimalPlaces(nameof(ExchangeRateDecimalPlaces))]
		public ZDecimal AverageExRate { get; private set; }

		[DecimalPlaces(nameof(ExchangeRateDecimalPlaces))]
		public ZDecimal AverageInverseExRate { get; private set; }

		public RefCurrency Currency
		{
			get
			{
				if (currency?.Code != PaymentCurrencyCode)
				{
					currency = RefCurrency.LoadFromCurrencyCode(Factory, PaymentCurrencyCode);
				}
				return currency;
			}
		}
		RefCurrency currency;

		public RefCurrency FundingCurrency
		{
			get
			{
				if (fundingCurrency?.Code != FundingCurrencyCode)
				{
					fundingCurrency = RefCurrency.LoadFromCurrencyCode(Factory, FundingCurrencyCode);
				}
				return fundingCurrency;
			}
		}
		RefCurrency fundingCurrency;

		public int PaymentCurrencyDecimalPlaces => Currency.Decimals;
		public int FundingCurrencyDecimalPlaces => FundingCurrency.Decimals;
		public int ExchangeRateDecimalPlaces => GlbCompany.CurrentCompany.ExchangeRateDecimalPlaces;

		#region Validation
		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}
		public EPaymentQuoteSummaryValidation Validation
		{
			get
			{
				return GetNewValidation();
			}
		}
		protected virtual EPaymentQuoteSummaryValidation GetNewValidation()
		{
			return new EPaymentQuoteSummaryValidation(this);
		}
		#endregion
	}
}

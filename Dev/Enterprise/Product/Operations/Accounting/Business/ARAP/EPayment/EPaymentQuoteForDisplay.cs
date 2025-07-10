using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.EPayment
{
	public class EPaymentQuoteForDisplay : NonPersistentBusinessObject
	{
		public EPaymentQuoteForDisplay(EPaymentQuote quote) : base(quote.Factory)
		{
			RealQuote = quote;
		}

		protected override void RunPreSaveValidationCore()
		{
			RealQuote.RunPreSaveValidation();
		}

		public EPaymentQuote RealQuote;

		public ZString ProviderCode => RealQuote?.QU_ProviderCode ?? ZString.Empty;
		public ZPropertyInfo ProviderCodeInfo => GetZPropertyInfo(nameof(ProviderCode));

		public ZString Status => RealQuote?.QU_Status ?? ZString.Empty;
		public ZPropertyInfo StatusInfo => GetZPropertyInfo(nameof(Status));

		public ZString StatusDescription => RealQuote?.StatusDescription ?? ZString.Empty;

		public ZDateTime LastResponseReceivedUtc => RealQuote?.QU_LastResponseReceivedUtc ?? ZDateTime.Empty;
		public ZPropertyInfo LastResponseReceivedUtcInfo => GetZPropertyInfo(nameof(LastResponseReceivedUtc));

		public ZDateTime LastResponseReceivedLocalTime => RealQuote?.LastResponseReceivedLocalTime ?? ZDateTime.Empty;
		public ZPropertyInfo LastResponseReceivedLocalTimeInfo => GetZPropertyInfo(nameof(LastResponseReceivedLocalTime));

		[List(nameof(Currencies))]
		public ZString FromCurrency => RealQuote?.QU_RX_NKFromCurrency ?? ZString.Empty;
		public ZPropertyInfo FromCurrencyInfo => RealQuote == null
												? GetZPropertyInfo(nameof(FromCurrency))
												: GetWrappedZPropertyInfo(nameof(FromCurrency), x => RealQuote.QU_RX_NKFromCurrencyInfo);

		[List(nameof(Currencies))]
		public ZString ToCurrency => RealQuote?.QU_RX_NKToCurrency ?? ZString.Empty;
		public ZPropertyInfo ToCurrencyInfo => GetZPropertyInfo(nameof(ToCurrency));

		[DecimalPlaces(nameof(ToRXDecimals))]
		public ZDecimal ToAmount => RealQuote?.QU_ToAmount ?? ZDecimal.Zero;
		public ZPropertyInfo ToAmountInfo => GetZPropertyInfo(nameof(ToAmount));

		[DecimalPlaces(nameof(FromRXDecimals))]
		public ZDecimal FromAmount => RealQuote?.QU_FromAmount ?? ZDecimal.Zero;
		public ZPropertyInfo FromAmountInfo => GetZPropertyInfo(nameof(FromAmount));

		[DecimalPlaces(nameof(FeeRXDecimals))]
		public ZDecimal FeeAmount => RealQuote?.QU_FeeAmount ?? ZDecimal.Zero;
		public ZPropertyInfo FeeAmountInfo => RealQuote?.QU_FeeAmountInfo ?? GetZPropertyInfo(nameof(FeeAmount));

		public ZString InternalReference => RealQuote?.QU_InternalReference ?? ZString.Empty;
		public ZPropertyInfo InternalReferenceInfo => GetZPropertyInfo(nameof(InternalReference));

		public ZString ErrorDescription => RealQuote?.QU_ErrorDescription ?? ZString.Empty;
		public ZPropertyInfo ErrorDescriptionInfo => GetZPropertyInfo(nameof(ErrorDescription));

		[DecimalPlaces(nameof(ExchangeRateDecimals))]
		public ZDecimal ExchangeRate => RealQuote?.QU_ExchangeRate ?? ZDecimal.Zero;
		public ZPropertyInfo ExchangeRateInfo => GetZPropertyInfo(nameof(ExchangeRate));

		[DecimalPlaces(nameof(ExchangeRateDecimals))]
		public ZDecimal ExchangeRateInverted => RealQuote?.QU_ExchangeRateInverted ?? ZDecimal.Zero;
		public ZPropertyInfo ExchangeRateInvertedInfo => GetZPropertyInfo(nameof(ExchangeRateInverted));

		public ZString ProviderReference => RealQuote?.QU_ProviderReference ?? ZString.Empty;
		public ZPropertyInfo ProviderReferenceInfo => GetZPropertyInfo(nameof(ProviderReference));

		int ToRXDecimals => RealQuote?.ToRXDecimals ?? 0;
		int FromRXDecimals => RealQuote?.FromRXDecimals ?? 0;
		int ExchangeRateDecimals => RealQuote?.ExchangeRateDecimals ?? 0;
		int FeeRXDecimals => RealQuote?.FeeRXDecimals ?? 0;
		public RefCurrencyCollection Currencies => new RefCurrencyCollection(Factory);
	}
}

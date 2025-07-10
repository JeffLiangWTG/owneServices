
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.ElectronicPayment.Universal
{
	internal static class AccEPaymentDealMessageConstants
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "translation not needed")]
		internal static class XUEFieldNames
		{
			internal const string CompanyCode = "CompanyCode";
			internal const string BranchCode = "BranchCode";
			internal const string DealId = "DealId";
			internal const string StatusCode = "StatusCode";
			internal const string FromAmount = "FromAmount";
			internal const string FromCurrency = "FromCurrency";
			internal const string ToAmount = "ToAmount";
			internal const string ToCurrency = "ToCurrency";
			internal const string ExchaneRate = "ExchangeRate";
			internal const string FeeAmount = "FeeAmount";
			internal const string FeeCurrency = "FeeCurrency";
			internal const string IsAwaitingFunds = "IsAwaitingFunds";
			internal const string IsSettled = "IsSettled";
			internal const string PaymentDate = "PaymentDate";
			internal const string Status = "Status";
			internal const string QuoteIdUsedForPayment = "QuoteIdUsedForPayment";
			internal const string QuoteProviderRef = "ProviderRef";
			internal const string ExchaneRateInverted = "ExchangeRateInverted";
			internal const string ExpiredQuoteIDs = "ExpiredQuoteIDs";
			internal const string ResponseTextFromProvider = "ResponseTextFromProvider";
			internal const string ErrorOriginatesAt = "ErrorOriginatesAt";
			internal const string ErrorMessage = "ErrorMessage";
			internal const string StreamContent = "StreamContent";
			internal const string ErrorType = "ErrorType";

			internal static ZString[] GetRequiredFieldsForIAKEvent(string eventSubType)
			{
				var fields = new List<ZString> { CompanyCode };
				if (eventSubType == MessageSubTypes.CreateADeal)
				{
					fields.Add(DealId);
					fields.Add(Status);
					fields.Add(QuoteIdUsedForPayment);
					fields.Add(FromAmount);
					fields.Add(FromCurrency);
					fields.Add(ToAmount);
					fields.Add(ToCurrency);
					fields.Add(FeeAmount);
					fields.Add(FeeCurrency);
					fields.Add(ExchaneRate);
					fields.Add(ExchaneRateInverted);
				}
				else if (eventSubType == Events.StatusUpdatedCode)
				{
					fields.Add(StatusCode);
				}
				return fields.ToArray();
			}

			internal static ZString[] GetRequiredFieldsForIRJEvent(string errorType)
			{
				var fields = new List<ZString> { CompanyCode };
				if (errorType == IRJErrorTypes.Generic)
				{
					fields.Add(ErrorOriginatesAt);
					fields.Add(ErrorMessage);
				}
				else if (errorType == IRJErrorTypes.QuoteOutsideTolerance)
				{
					fields.Add(QuoteProviderRef);
					fields.Add(FromAmount);
					fields.Add(FromCurrency);
					fields.Add(ToAmount);
					fields.Add(ToCurrency);
					fields.Add(FeeAmount);
					fields.Add(FeeCurrency);
					fields.Add(ExchaneRate);
					fields.Add(ExchaneRateInverted);
				}
				return fields.ToArray();
			}
		}

		internal static class MessageSubTypes
		{
			internal const string CreateADeal = "PAY";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "translation not needed")]
		internal static class GEPErrorSource
		{
			internal const string Provider = "Electronic Payment Service Provider";
			internal const string XT = "XT";
			internal const string UnhandledXTException = "UnhandledXTException";
			internal const string Multiple = "Multiple";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "translation not needed")]
		internal static class IRJErrorTypes
		{
			internal const string Generic = "Generic";
			internal const string QuoteOutsideTolerance = "Unapproved Quote";
		}
	}
}

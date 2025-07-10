using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business
{
	public static class TransactionMatchLinkOSAmountProvider
	{
		public static ZDecimal GetMatchLinkOSAmount(TransactionMatchLink matchLink)
		{
			return IsFeatureEnabled(matchLink) ?
				matchLink.AP_OSAmount : CalculateMatchLinkOSAmount(matchLink);
		}

		public static void FillReversingMatchLinkAmounts(TransactionHeader parent, TransactionMatchLink originalLink)
		{
			var reversingMatchLink = ((IMatching)parent).CurrentMatchGroup[0];
			reversingMatchLink.AP_Amount = originalLink.AP_Amount;

			if (TransactionHeaderOSOutstandingAmountProvider.IsFeatureEnabled(parent) && IsFeatureEnabled(originalLink))
			{
				reversingMatchLink.AP_OSAmount = originalLink.AP_OSAmount;
			}
		}

		public static ZDecimal GetOSPaidAmountForPaymentDataAdapter(AccTransactionMatchLink matchLink, decimal totalLocalPaidAmount, decimal totalOSPaidAmount)
		{
			return IsFeatureEnabled(matchLink)
				? matchLink.AP_OSAmount
				: new ZDecimal(matchLink.AP_Amount / totalLocalPaidAmount * totalOSPaidAmount);
		}

		public static bool IsFeatureEnabled(AccTransactionMatchLink matchLink)
		{
			return matchLink.TransactionHeader != null
				&& TransactionHeaderOSOutstandingAmountProvider.IsFeatureEnabled(matchLink.TransactionHeader);
		}

		#region Implementation

		static ZDecimal CalculateMatchLinkOSAmount(TransactionMatchLink matchLink)
		{
			var header = matchLink.MatchingTransaction;
			var highPrecisionExchangeRate = Env.CurrentCompany.ExchangeRate.GetRate(header.AH_LocalTotal, header.AH_OSTotal, AccTransactionHeaderSchema.AH_ExchangeRate.Scale);
			return Env.CurrentCompany.ExchangeRate.LocalToForeign(matchLink.AP_Amount, highPrecisionExchangeRate, header.AH_RX_NKTransactionCurrency);
		}

#if DEBUG
		public static ZDecimal CalculateMatchLinkOSAmount_ForTestOnly(TransactionMatchLink matchLink) => CalculateMatchLinkOSAmount(matchLink);
#endif

		#endregion
	}
}

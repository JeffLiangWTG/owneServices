using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business
{
	public static class TransactionHeaderOSOutstandingAmountProvider
	{
		public static ZDecimal GetAndRefreshOSOutstandingAmount(TransactionHeader header)
		{
			if (IsFeatureEnabled(header))
			{
				if (!header.OSOutstandingAmountValueChangeMonitor.IsValueUpToDate)
				{
					UpdateOSOutstandingAmountCore(header);
				}

				return header.AH_OSOutstandingAmount;
			}

			return CalculateOSOutstandingAmount(header);
		}

		public static void UpdateOSOutstandingAmount(TransactionHeader header)
		{
			if (IsFeatureEnabled(header) && !header.OSOutstandingAmountValueChangeMonitor.IsValueUpToDate)
			{
				UpdateOSOutstandingAmountCore(header);
			}
		}

		public static void ForceToSetOutstandingAmounts(TransactionHeader header, ZDecimal localAmount, ZDecimal osAmount, bool isIncremental)
		{
			if (IsFeatureEnabled(header))
			{
				header.AH_OSOutstandingAmount = isIncremental ? (ZDecimal)(header.AH_OSOutstandingAmount + osAmount) : osAmount;
				header.OSOutstandingAmountValueChangeMonitor.Fixed();
			}

			header.AH_OutstandingAmount = isIncremental ? (ZDecimal)(header.AH_OutstandingAmount + localAmount) : localAmount;
		}

		public static ZDecimal GetOSMatchedAmount(TransactionHeader header)
		{
			var result = 0m;

			if (IsFeatureEnabled(header))
			{
				result = header.AH_OSTotal - header.AH_OSOutstandingAmount;
			}
			else
			{
				result = GetHighPrecisionMatchedAmount(header.AH_LocalTotal, header.AH_OSTotal, header.AH_OutstandingAmount, header.AH_RX_NKTransactionCurrency);
			}

			return result;
		}

		public static ZDecimal GetHighPrecisionOSOutstandingAmount(ZDecimal localTotal, ZDecimal oSTotal, ZDecimal localOutstandingAmount, ZString currency)
			=> oSTotal - GetHighPrecisionMatchedAmount(localTotal, oSTotal, localOutstandingAmount, currency);

		public static ZDecimal GetHighPrecisionOSAmount(ZDecimal localTotal, ZDecimal oSTotal, ZDecimal localAmount, ZString currency)
		{
			var highPrecisionExchangeRate = Env.CurrentCompany.ExchangeRate.GetRate(localTotal, oSTotal, AccTransactionHeaderSchema.AH_ExchangeRate.Scale);
			return Env.CurrentCompany.ExchangeRate.LocalToForeign(localAmount, highPrecisionExchangeRate, currency);
		}

		public static ZDecimal GetLowPrecisionOSOutstandingAmount(ZDecimal localOutstandingAmount, ZDecimal exRate, ZString currency)
			=> Env.CurrentCompany.ExchangeRate.LocalToForeign(localOutstandingAmount, exRate, currency);

		public static bool IsFeatureEnabled(AccTransactionHeader header) => header.AH_IsOSOutstandingAmountApplicable && AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.Value;

		#region Implementation

		static ZDecimal CalculateOSOutstandingAmount(TransactionHeader header)
		{
			ZDecimal result;

			// All properties that will impact OSOutstandingAmount in this method,
			// should be added into TransactionHeaderOSOutstandingAmountValueChangeMonitor and binded with ValueChanged event.
			if (header.TransactionCurrency == null || header.AH_OutstandingAmount == 0)
			{
				result = 0m;
			}
			else if (header.InvoiceUnpaid)
			{
				result = header.AH_OSTotal;
			}
			else
			{
				result = GetHighPrecisionOSOutstandingAmount(header.AH_LocalTotal, header.AH_OSTotal, header.AH_OutstandingAmount, header.AH_RX_NKTransactionCurrency);
			}

			return result;
		}

		static void UpdateOSOutstandingAmountCore(TransactionHeader header)
		{
			header.AH_OSOutstandingAmount = CalculateOSOutstandingAmount(header);
			header.OSOutstandingAmountValueChangeMonitor.Reset();
		}

		static ZDecimal GetHighPrecisionMatchedAmount(ZDecimal localTotal, ZDecimal oSTotal, ZDecimal localOutstandingAmount, ZString currency)
			=> GetHighPrecisionOSAmount(localTotal, oSTotal, localTotal - localOutstandingAmount, currency);

#if DEBUG
		public static ZDecimal CalculateOSOutstandingAmount_ForTestOnly(TransactionHeader header) => CalculateOSOutstandingAmount(header);
#endif

		#endregion
	}
}

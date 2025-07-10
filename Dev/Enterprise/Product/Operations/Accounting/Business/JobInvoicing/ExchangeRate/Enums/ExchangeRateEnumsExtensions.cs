using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public static class ExchangeRateEnumsExtensions
	{
		public static string ToCode(this ExchangeRateOrgTypeEnum value)
		{
			var fi = value.GetType().GetField(value.ToString());

			var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

			return (attributes?.Length > 0) ? attributes[0].Description : value.ToString();
		}

		public static bool IsDebtor(this ExchangeRateOrgTypeEnum value)
		{
			return (value != ExchangeRateOrgTypeEnum.Creditor) && (value != ExchangeRateOrgTypeEnum.None);
		}

		public static ExchangeRateValidLedgerEnum ToLedger(this ExchangeRateOrgTypeEnum value)
		{
			if (value == ExchangeRateOrgTypeEnum.None)
			{
				return ExchangeRateValidLedgerEnum.None;
			}

			return value == ExchangeRateOrgTypeEnum.Creditor ? ExchangeRateValidLedgerEnum.AP : ExchangeRateValidLedgerEnum.AR;
		}

		public static string ToCode(this ExchangeRateValidLedgerEnum value)
		{
			switch (value)
			{
				case ExchangeRateValidLedgerEnum.None:
					return string.Empty;
				case ExchangeRateValidLedgerEnum.AP:
					return LedgerTypes.AccountsPayable;
				case ExchangeRateValidLedgerEnum.AR:
					return LedgerTypes.AccountsReceivable;
				case ExchangeRateValidLedgerEnum.UA:
					return LedgerTypes.UnapprovedPayableTransactions;
				default:
					throw new InvalidOperationException(FormattableString.Invariant($"Unsupported value {value}"));
			}
		}

		public static ExchangeRateOrgTypeEnum GetOrgTypeFromCode(string orgTypeCode)
		{
			return Enum.GetValues(typeof(ExchangeRateOrgTypeEnum))
						.Cast<ExchangeRateOrgTypeEnum?>()
						.FirstOrDefault(ot => ot.Value.ToCode() == orgTypeCode) ?? ExchangeRateOrgTypeEnum.None;
		}

		public static ExchangeRateValidLedgerEnum GetExRateLedger(this InvoicingBase invoice) => GetLedgerFromCode(invoice.AH_Ledger);

		public static ExchangeRateValidLedgerEnum GetLedgerFromCode(string ledgerCode)
		{
			if (string.IsNullOrEmpty(ledgerCode))
			{
				return ExchangeRateValidLedgerEnum.None;
			}

			switch (ledgerCode)
			{
				case LedgerTypes.AccountsReceivable:
					return ExchangeRateValidLedgerEnum.AR;

				case LedgerTypes.AccountsPayable:
				case LedgerTypes.IncompleteTransactions:
				case LedgerTypes.UnapprovedPayableTransactions:
				case LedgerTypes.TransactionsPendingAllocation:
					return ExchangeRateValidLedgerEnum.AP;

				default: throw new InvalidOperationException(FormattableString.Invariant($"Unsupported ledger {ledgerCode}"));
			}
		}

		public static string ToCode(this InvoiceCurrencyType value)
		{
			switch (value)
			{
				case InvoiceCurrencyType.NotApplicable:
					return string.Empty;
				case InvoiceCurrencyType.Foreign:
					return Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign;
				case InvoiceCurrencyType.Local:
					return Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Local;
				default:
					throw new InvalidOperationException(FormattableString.Invariant($"Unsupported value {value}"));
			}
		}

		public static InvoiceCurrencyType GetInvoiceCurrencyTypeFromCode(string invoiceCurrencyType)
		{
			switch (invoiceCurrencyType)
			{
				case Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign:
					return InvoiceCurrencyType.Foreign;

				case Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Local:
					return InvoiceCurrencyType.Local;

				default:
					return InvoiceCurrencyType.NotApplicable;
			}
		}

		public static IEnumerable<ExchangeRateOrgTypeEnum> GetMatchingOrgTypes(this ExchangeRateOrgTypeEnum orgType)
		{
			yield return orgType;

			if (orgType != ExchangeRateOrgTypeEnum.None)
			{
				yield return ExchangeRateOrgTypeEnum.None;
			}
		}
	}
}

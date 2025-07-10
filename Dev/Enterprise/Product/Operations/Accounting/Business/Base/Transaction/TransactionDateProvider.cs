using System;
using System.Linq;
using CargoWise.Types;
using WTG.ProductionRules.Core;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	internal static class TransactionDateProvider
	{
		public enum DateKind
		{
			Default,
			PostDate,
			InvoiceDate,
			DocumentReceivedFallbackToInvoiceDate,
			EarliestTaxDate,
			LatestTaxDate,
		}

		public static ZDate GetTransactionDate(this TransactionHeader transaction, DateKind date)
		{
			Argument.NotNull(transaction, nameof(transaction));

			switch (date)
			{
				case DateKind.Default:
				case DateKind.PostDate:
					return transaction.AH_PostDate.Date;
				case DateKind.InvoiceDate:
					return transaction.AH_InvoiceDate.Date;
				case DateKind.DocumentReceivedFallbackToInvoiceDate:
					return transaction.AH_DocumentReceivedDate.IsEmpty
						? transaction.AH_InvoiceDate.Date
						: transaction.AH_DocumentReceivedDate.Date;
				case DateKind.EarliestTaxDate:
					return transaction is TransactionHeaderWithLines headerWithLinesEarliest
						? (headerWithLinesEarliest.Lines.Cast<TransactionLine>().Where(x => x.AL_TaxDate.IsValid).OrderBy(x => x.AL_TaxDate).FirstOrDefault()?.AL_TaxDate ?? ZDate.Empty)
						: ZDate.Empty;
				case DateKind.LatestTaxDate:
					return transaction is TransactionHeaderWithLines headerWithLinesLatest
						? (headerWithLinesLatest.Lines.Cast<TransactionLine>().Where(x => x.AL_TaxDate.IsValid).OrderByDescending(x => x.AL_TaxDate).FirstOrDefault()?.AL_TaxDate ?? ZDate.Empty)
						: ZDate.Empty;
				default:
					throw new NotImplementedException(date.ToString() + " DateKind is not supported.");
			}
		}

		public static DateKind ConvertReportingDate(ZString reportingDate)
		{
			switch (reportingDate)
			{
				case "": return DateKind.Default;
				case ReportingDateCodes.PostDate: return DateKind.PostDate;
				case ReportingDateCodes.InvoiceDate: return DateKind.InvoiceDate;
				case ReportingDateCodes.DocumentReceivedDate: return DateKind.DocumentReceivedFallbackToInvoiceDate;
				case ReportingDateCodes.EarliestTaxDate: return DateKind.EarliestTaxDate;
				case ReportingDateCodes.LatestTaxDate: return DateKind.LatestTaxDate;
				default: throw new NotImplementedException("Invalid Reporting Date code: " + reportingDate);
			}
		}
	}
}

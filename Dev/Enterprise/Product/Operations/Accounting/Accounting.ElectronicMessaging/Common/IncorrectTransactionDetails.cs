using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	[WTG.StaticAnalysis.Annotation.Immutable]
	public struct IncorrectTransactionDetails
	{
		public static readonly IncorrectTransactionDetails Empty;

		public IncorrectTransactionDetails(ZGuid transactionPK, ZString transactionNumber, IEnumerable<ZString> errors, ZString ledgerType, ZString transactionType, ZString parentTableCode)
		{
			TransactionPK = transactionPK;
			TransactionNumber = transactionNumber;
			fErrors = (errors ?? Enumerable.Empty<ZString>()).ToImmutableArray();
			LedgerType = ledgerType;
			TransactionType = transactionType;
			ParentTableCode = parentTableCode;
		}

		public static IncorrectTransactionDetails FromBizo(AccTransactionHeader transaction, IEnumerable<ZString> errors)
		{
			Argument.NotNull(transaction, nameof(transaction));
			return new IncorrectTransactionDetails(
				transaction.PK,
				transaction.AH_TransactionNum,
				errors,
				transaction.AH_Ledger,
				transaction.AH_TransactionType,
				AccTransactionHeaderSchema.Constants.Prefix);
		}

		public static IncorrectTransactionDetails FromBizo(AccComplianceDocumentHeader complianceDocumentHeader, IEnumerable<ZString> errors)
		{
			Argument.NotNull(complianceDocumentHeader, nameof(complianceDocumentHeader));
			return new IncorrectTransactionDetails(
				complianceDocumentHeader.PK,
				complianceDocumentHeader.ADH_DocumentNumber,
				errors,
				complianceDocumentHeader.ADH_Ledger,
				complianceDocumentHeader.ADH_TransactionType,
				AccComplianceDocumentHeaderSchema.Constants.Prefix);
		}

		public static IncorrectTransactionDetails FromBizo(AccEInvoicingBatch transactionBatch, IEnumerable<ZString> errors)
		{
			Argument.NotNull(transactionBatch, nameof(transactionBatch));
			return new IncorrectTransactionDetails(
				transactionBatch.PK,
				transactionBatch.AIB_BatchNumber.ToString(),
				errors,
				ZString.Empty,
				ZString.Empty,
				AccEInvoicingBatchSchema.Constants.Prefix);
		}

		public readonly ZGuid TransactionPK;
		public readonly ZString TransactionNumber;
		readonly ImmutableArray<ZString> fErrors;
		public IEnumerable<ZString> Errors => fErrors;
		public readonly ZString LedgerType;
		public readonly ZString TransactionType;
		public readonly ZString ParentTableCode;

		public bool IsEmpty => this == Empty;

		public string GetAllErrorsAsString(string separator = "")
		{
			var result = string.Empty;
			if (Errors.Any())
			{
				result = string.Join(separator, Errors.ToArray());
			}
			return result;
		}

		public override bool Equals(object obj)
		{
			var result = false;
			if (obj is IncorrectTransactionDetails)
			{
				var castedObj = (IncorrectTransactionDetails)obj;
				result = TransactionPK == castedObj.TransactionPK && TransactionNumber == castedObj.TransactionNumber && fErrors == castedObj.fErrors &&
								LedgerType == castedObj.LedgerType && TransactionType == castedObj.TransactionType && ParentTableCode == castedObj.ParentTableCode;
			}
			return result;
		}

		public override int GetHashCode()
		{
			return TransactionPK.GetHashCode() ^ TransactionNumber.GetHashCode() ^ fErrors.GetHashCode() ^ LedgerType.GetHashCode() ^ TransactionType.GetHashCode() ^ ParentTableCode.GetHashCode();
		}

		public static bool operator ==(IncorrectTransactionDetails detail1, IncorrectTransactionDetails detail2)
		{
			return detail1.Equals(detail2);
		}

		public static bool operator !=(IncorrectTransactionDetails detail1, IncorrectTransactionDetails detail2)
		{
			return !detail1.Equals(detail2);
		}

		#region SuppressResourceStringsCheckRegion
		// Reason = No need to localise as email will be in English

		public string BizoName
			=> ParentTableCode == AccTransactionHeaderSchema.Constants.Prefix        ? Res.GetString("1D6AFBE6-C77C-40F6-822B-1C3546D42749", "Transaction")
			 : ParentTableCode == AccComplianceDocumentHeaderSchema.Constants.Prefix ? Res.GetString("FD7F5CB6-AEFD-4346-8334-0A3DC4EBE296", "Compliance Document")
			 : ParentTableCode == AccEInvoicingBatchSchema.Constants.Prefix          ? Res.GetString("DBEE7E0E-2B8C-474D-B9E8-8E7D2C6ED2E4", "Electronic Invoicing Batch")
			 : Res.GetString("7EF49A91-CBB9-4E9F-8E07-6BD83CBD8BC6", "Unknown");

		public string UniqueIdentifier
			=> ParentTableCode == AccTransactionHeaderSchema.Constants.Prefix        ? (LedgerType + " " + TransactionType + " " + TransactionNumber)
			 : ParentTableCode == AccComplianceDocumentHeaderSchema.Constants.Prefix ? (string)TransactionNumber
			 : ParentTableCode == AccEInvoicingBatchSchema.Constants.Prefix          ? (string)TransactionNumber
			 : Res.GetString("E2D77E4C-D50E-42C3-BA05-4973300E8705", "Unknown");

		public string ModuleName
			=> ParentTableCode == AccTransactionHeaderSchema.Constants.Prefix && LedgerType == LedgerTypes.AccountsReceivable ? Res.GetString("951563F7-7CA2-48AD-B303-AA5E1E2297C9", "Receivables Transactions")
			 : ParentTableCode == AccTransactionHeaderSchema.Constants.Prefix && LedgerType == LedgerTypes.AccountsPayable    ? Res.GetString("194C0780-D890-461E-BB41-6ABF8B5AB639", "Payables Transactions")

			 : ParentTableCode == AccComplianceDocumentHeaderSchema.Constants.Prefix && LedgerType == LedgerTypes.AccountsReceivable ? Res.GetString("82A905AD-E92A-42B7-B24F-4BCE354FF7F8", "Receivables Compliance Documents")
			 : ParentTableCode == AccComplianceDocumentHeaderSchema.Constants.Prefix && LedgerType == LedgerTypes.AccountsPayable    ? Res.GetString("82F41FE9-C089-4B63-8C36-A2CB5B92064C", "Payables Compliance Documents")

			 : string.Empty;

		#endregion
	}
}

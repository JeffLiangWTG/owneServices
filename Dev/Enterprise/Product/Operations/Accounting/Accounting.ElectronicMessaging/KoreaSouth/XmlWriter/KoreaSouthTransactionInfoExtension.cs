using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth
{
	static class KoreaSouthTransactionInfoExtension
	{
		public static ZBool IsCreditNote(this TransactionInfo transactionInfo)
			=> transactionInfo.TransactionType == TransactionType.CRD;

		public static ZBool IsAmendment(this TransactionInfo transactionInfo)
			=> transactionInfo.OriginalReference != null &&
			   transactionInfo.OriginalReference.OriginalTransactionNumber.HasValue &&
			   !transactionInfo.OriginalReference.OriginalTransactionNumber.Value.IsEmpty;

		public static ZBool IsTaxApplicable(this TransactionInfo transactionInfo)
			=> (transactionInfo.PostingJournalCollection ?? new List<PostingJournal>()).Any(x => x.VATTaxID != null);
	}
}

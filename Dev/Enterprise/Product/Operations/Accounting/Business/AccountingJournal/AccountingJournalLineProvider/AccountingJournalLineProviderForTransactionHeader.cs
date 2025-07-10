using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business
{
	public class AccountingJournalLineProviderForTransactionHeader : AccountingJournalLineProvider
	{
		public AccountingJournalLineProviderForTransactionHeader(ReadOnlyBusinessObjectFactory factory, TransactionHeader transaction, IEnumerable<ZString> validLedgerTypes, IEnumerable<ZString> validTransactionTypes)
			: base(factory, validLedgerTypes, validTransactionTypes)
		{
			this.transaction = transaction;
		}
		readonly TransactionHeader transaction;

		public override IEnumerable<IAccountingJournalLine> GetAccountingJournalLines()
		{
			var result = new List<AccountingJournalLine>();

			if (validLedgerTypes.Contains(transaction.AH_Ledger) && validTransactionTypes.Contains(transaction.AH_TransactionType))
			{
				var journalLineCreator = GetLineCreator(transaction);
				if (journalLineCreator != null)
				{
					result.AddRange(journalLineCreator.CreateAccountJournalLines());
				}
			}
			else
			{
				throw new InvalidAccountingJournalOperationException(GetErrorMessageForMissingTransactionType(transaction.AH_Ledger, transaction.AH_TransactionType));
			}

			return result;
		}

		AccountingJournalLineCreator GetLineCreator(TransactionHeader header)
		{
			AccountingJournalLineCreator creator = null;

			#region Cash Book
			if (header.AH_Ledger == LedgerTypes.CashBook && header.AH_TransactionType == TransactionTypes.Transfer)
			{
				creator = new CBTRFAccountingJournalLineCreator(header, Factory);
			}
			else if (header.AH_Ledger == LedgerTypes.CashBook && header.AH_TransactionType == TransactionTypes.ExchangeDifference)
			{
				creator = new CBEXXAccountingJournalLineCreator(header, Factory);
			}
			#endregion

			#region AR and AP
			else if ((header.AH_Ledger == LedgerTypes.AccountsReceivable || header.AH_Ledger == LedgerTypes.AccountsPayable)
					&& (header.AH_TransactionType == TransactionTypes.ExchangeDifference || header.AH_TransactionType == TransactionTypes.Overpayment || header.AH_TransactionType == TransactionTypes.Discount))
			{
				creator = new EXXOVPDSCAccountingJournalLineCreator(header, Factory);
			}

			else if ((header.AH_Ledger == LedgerTypes.AccountsReceivable || header.AH_Ledger == LedgerTypes.AccountsPayable)
					&& (header.AH_TransactionType == TransactionTypes.Payment || header.AH_TransactionType == TransactionTypes.Receipt))
			{
				creator = new PAYRECAccountingJournalLineCreator(header, Factory);
			}

			else if ((header.AH_Ledger == LedgerTypes.AccountsReceivable || header.AH_Ledger == LedgerTypes.AccountsPayable)
					&& (header.AH_TransactionType == TransactionTypes.Contra))
			{
				creator = new CTRAccountingJournalLineCreator(header, Factory);
			}
			else if ((header.AH_Ledger == LedgerTypes.AccountsReceivable || header.AH_Ledger == LedgerTypes.AccountsPayable)
					&& (header.AH_TransactionType == TransactionTypes.Journal))
			{
				creator = new JNLAccountingJournalLineCreator(header, Factory);
			}
			else if ((header.AH_Ledger == LedgerTypes.AccountsReceivable || header.AH_Ledger == LedgerTypes.AccountsPayable)
					&& (header.AH_TransactionType == TransactionTypes.Transfer))
			{
				creator = new TRFAccountingJournalLineCreator(header, Factory);
			}
			#endregion

			else
			{
				throw new InvalidAccountingJournalOperationException(GetErrorMessageForMissingTransactionType(header.AH_Ledger, header.AH_TransactionType));
			}

			return creator;
		}
	}
}

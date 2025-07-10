using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business
{
	public interface IAccountingJournalLineForGeneralLedgerDataCreator
	{
		IEnumerable<IAccountingJournalLine> CreateAccountingJournalLines(TransactionHeader transaction, TransactionLine transactionLine, IEnumerable<ZString> validLedgerTypes, IEnumerable<ZString> validTransactionTypes);
	}
}

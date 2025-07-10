using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business
{
	public class AccountingJournalLineProviderForGeneralLedgerData : AccountingJournalLineProvider
	{
		public AccountingJournalLineProviderForGeneralLedgerData(ReadOnlyBusinessObjectFactory factory, TransactionHeader transaction, IEnumerable<ZString> validLedgerTypes, IEnumerable<ZString> validTransactionTypes)
			: base(factory, validLedgerTypes, validTransactionTypes)
		{
			Argument.NotNull(transaction, nameof(transaction));
			this.transaction = transaction;
		}

		public AccountingJournalLineProviderForGeneralLedgerData(TransactionLine transactionLine, IEnumerable<ZString> validLedgerTypes, IEnumerable<ZString> validTransactionTypes)
			: base(transactionLine?.Factory as ReadOnlyBusinessObjectFactory, validLedgerTypes, validTransactionTypes)
		{
			Argument.NotNull(transactionLine, nameof(transactionLine));
			this.transactionLine = transactionLine;
		}

		readonly TransactionHeader transaction;
		readonly TransactionLine transactionLine;

		public override IEnumerable<IAccountingJournalLine> GetAccountingJournalLines()
		{
			return ObjectFactory.Get<IAccountingJournalLineForGeneralLedgerDataCreator>().CreateAccountingJournalLines(transaction, transactionLine, validLedgerTypes, validTransactionTypes);
		}
	}
}

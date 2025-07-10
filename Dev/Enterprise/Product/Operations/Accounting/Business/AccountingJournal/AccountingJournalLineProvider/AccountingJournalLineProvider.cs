using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business
{
	public abstract class AccountingJournalLineProvider
	{
		protected AccountingJournalLineProvider(ReadOnlyBusinessObjectFactory factory, IEnumerable<ZString> validLedgerTypes, IEnumerable<ZString> validTransactionTypes)
		{
			this.Factory = factory;
			this.validLedgerTypes = validLedgerTypes;
			this.validTransactionTypes = validTransactionTypes;
		}
		protected readonly ReadOnlyBusinessObjectFactory Factory;
		protected readonly IEnumerable<ZString> validLedgerTypes;
		protected readonly IEnumerable<ZString> validTransactionTypes;

		public abstract IEnumerable<IAccountingJournalLine> GetAccountingJournalLines();

		protected string GetErrorMessageForMissingTransactionType(ZString ledgerType, ZString transactionType)
		{
			return Res.GetString("e63ac689-affc-4c62-85b5-829b909c03e9", "Printing of Accounting Journal is not supported for Ledger: {0} with Transaction Type: {1}", ledgerType, transactionType);
		}
	}
}

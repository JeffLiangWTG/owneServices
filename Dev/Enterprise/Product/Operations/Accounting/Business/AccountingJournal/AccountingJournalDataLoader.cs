using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business
{
	public class AccountingJournalDataLoader
	{
		public AccountingJournalDataLoader(ZGuid? reportingBookPK = null, DataTable generalLedgerTransactionData = null)
		{
			Factory = new ReadOnlyBusinessObjectFactory();
			if (reportingBookPK != null)
			{
				ReportingBook = Factory.Load<AccReportingBook>(reportingBookPK.Value);
				GeneralLedgerTransactionData = generalLedgerTransactionData;
			}
		}

		public IEnumerable<AccountingJournal> LoadTransactionsWithHeader(IEnumerable<ZGuid> transactionPKs)
		{
			var transactions = Factory.Load<TransactionHeader>(new ZQuery(AccTransactionHeaderSchema.PK, transactionPKs));
			var journals = CreateAccountingJournal(transactions);
			return journals;
		}

		public IEnumerable<AccountingJournal> LoadTransactionsWithoutHeader(IEnumerable<ZGuid> transactionLinePKs)
		{
			var lines = Factory.Load<TransactionLine>(new ZQuery(AccTransactionLinesSchema.PK, transactionLinePKs));
			var journals = CreateAccountingJournal(lines, LedgerTypes.JobCosting);
			return journals;
		}

#if DEBUG
		protected virtual
#endif
		List<AccountingJournal> CreateAccountingJournal(IEnumerable<BusinessObject> bizOs, string ledgerType = "")
		{
			var journals = new List<AccountingJournal>();
			if (bizOs != null)
			{
				foreach (BusinessObject bizO in bizOs)
				{
					var transaction = bizO as TransactionHeader;
					var aH_Ledger = (string.IsNullOrEmpty(ledgerType) && transaction != null) ? transaction.AH_Ledger : new ZString(ledgerType);
					var journal = GetJournal(bizO, aH_Ledger);
					journals.Add(journal);
				}
			}
			return journals;
		}

		AccountingJournal GetJournal(BusinessObject parent, string ledgerType)
		{
			AccountingJournal journal = null;
			var header = parent as TransactionHeader;
			switch (ledgerType)
			{
				case LedgerTypes.AccountsReceivable:
				case LedgerTypes.AccountsPayable:
					if (header != null)
					{
						journal = new ARAPAccountingJournal(header, Factory, ReportingBook, GeneralLedgerTransactionData);
					}
					break;

				case LedgerTypes.CashBook:
					if (header != null)
					{
						journal = new CashBookAccountingJournal(header, Factory, ReportingBook, GeneralLedgerTransactionData);
					}
					break;

				case LedgerTypes.JobCosting:

					var jrj = parent as JobRevenueJournal;
					journal = jrj != null ? new JRJJNLAccountingJournal(jrj, Factory, ReportingBook, GeneralLedgerTransactionData) : null;

					if (journal == null)
					{
						var line = parent as TransactionLine;
						journal = line != null ? new WIPACRAccountingJournal(line, Factory, ReportingBook, GeneralLedgerTransactionData) : null;
					}

					if (journal == null)
					{
						var jcj = parent as JCJournalHeader;
						journal = jcj != null ? new JRJJNLAccountingJournal(jcj, Factory, ReportingBook, GeneralLedgerTransactionData) : null;
					}

					if (journal == null)
					{
						throw new InvalidAccountingJournalOperationException(Res.GetString("df723e18-c7ea-4c45-8816-814adcfe366f", "Invalid Transaction Header Object : Accounting Journal object cannot be initialized for 'JC' Ledger, "));
					}
					break;

				case LedgerTypes.General:
					journal = new GLAccountingJournal(parent as GLJournal, Factory, ReportingBook, GeneralLedgerTransactionData);
					break;

				default:
					throw new InvalidAccountingJournalOperationException(Res.GetString("8e40ec84-5828-40ee-9678-b86838917ad3", "Accounting Journal object cannot be initialized for Ledger type: {0}", ledgerType));
			}

			return journal;
		}

#if DEBUG
		protected
#endif
		ReadOnlyBusinessObjectFactory Factory;

		readonly AccReportingBook ReportingBook;

		readonly DataTable GeneralLedgerTransactionData;
	}
}

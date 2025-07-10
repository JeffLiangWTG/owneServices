using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business
{
	public class JRJJNLAccountingJournal : AccountingJournal
	{
		public JRJJNLAccountingJournal(JobRevenueJournal transaction, ReadOnlyBusinessObjectFactory factory, AccReportingBook reportingBook = null, DataTable generalLedgerTransactionData = null)
			: base(transaction, factory, reportingBook, generalLedgerTransactionData)
		{
			PopulateJournalLines();
		}

		public JRJJNLAccountingJournal(JCJournalHeader transaction, ReadOnlyBusinessObjectFactory factory, AccReportingBook reportingBook = null, DataTable generalLedgerTransactionData = null)
		   : base(transaction, factory, reportingBook, generalLedgerTransactionData)
		{
			PopulateJournalLines();
		}

		public override Dictionary<ZString, ZString> ApplicableOptionalFields
		{
			get
			{
				applicableOptionalFields_innervalue = new Dictionary<ZString, ZString>();
				if (transaction.AH_Ledger == LedgerTypes.JobCosting && (transaction.AH_TransactionType == TransactionTypes.JobRevenueJournal || transaction.AH_TransactionType == TransactionTypes.Journal))
				{
					applicableOptionalFields_innervalue.Add(StatusText, GetJournalStatus());
				}
				return applicableOptionalFields_innervalue;
			}
		}
		Dictionary<ZString, ZString> applicableOptionalFields_innervalue;

		protected override IEnumerable<ZString> ValidLedgerTypes
		{
			get
			{
				return new ZString[] { LedgerTypes.JobCosting };
			}
		}

		protected override IEnumerable<ZString> ValidTransactionTypes
		{
			get
			{
				return new ZString[] { TransactionTypes.Journal, TransactionTypes.JobRevenueJournal };
			}
		}
	}
}

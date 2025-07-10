using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business
{
	public class GLAccountingJournal : AccountingJournal
	{
		public GLAccountingJournal(GLJournal transaction, ReadOnlyBusinessObjectFactory factory, AccReportingBook reportingBook = null, DataTable generalLedgerTransactionData = null)
			: base(transaction, factory, reportingBook, generalLedgerTransactionData)
		{
			PopulateJournalLines();
		}

		public override Dictionary<ZString, ZString> ApplicableOptionalFields
		{
			get
			{
				if (applicableOptionalFields_innervalue == null)
				{
					applicableOptionalFields_innervalue = new Dictionary<ZString, ZString>();
					applicableOptionalFields_innervalue.Add(PostToPeriodText, transaction.PostPeriod.ToString());

					if (transaction.AH_Ledger == LedgerTypes.General)
					{
						if (transaction.AH_TransactionType == TransactionTypes.GLAutoJournal)
						{
							applicableOptionalFields_innervalue.Add(EndPeriodText, transaction.AgePeriod.ToString());
						}
						else if (transaction.AH_TransactionType == TransactionTypes.GLReversingJournal)
						{
							applicableOptionalFields_innervalue.Add(ReversePeriodText, transaction.AgePeriod.ToString());
						}
						else
						{
							applicableOptionalFields_innervalue.Add(Filler, Filler);
						}
					}

					applicableOptionalFields_innervalue.Add(PresentationText, transaction.AH_TransactionCategory);
				}

				return applicableOptionalFields_innervalue;
			}
		}
		Dictionary<ZString, ZString> applicableOptionalFields_innervalue;

		public override ZString LastPostedRequest_RequesterFullName
		{
			get
			{
				return Journal.LastPostedRequest_RequesterFullName;
			}
		}

		public override ZDateTime LastPostedRequest_RequestedTime
		{
			get
			{
				return Journal.LastPostedRequest_RequestedTime;
			}
		}

		public override ZString LastPostedRequest_ApproverFullName
		{
			get
			{
				return Journal.LastPostedRequest_ApproverFullName;
			}
		}

		public override ZDateTime LastPostedRequest_ApprovedTime
		{
			get
			{
				return Journal.LastPostedRequest_ApprovedTime;
			}
		}

		public override ZString OriginalRequest_RequesterFullName
		{
			get
			{
				return Journal.OriginalRequest_RequesterFullName;
			}
		}

		public override ZString OriginalPostedRequest_ApproverFullName
		{
			get
			{
				return Journal.OriginalPostedRequest_ApproverFullName;
			}
		}

		public override bool GroupByLinesWhilePrinting
		{
			get
			{
				return false;
			}
		}

		protected override IEnumerable<ZString> ValidLedgerTypes
		{
			get
			{
				return new ZString[] { LedgerTypes.General };
			}
		}

		protected override IEnumerable<ZString> ValidTransactionTypes
		{
			get
			{
				return new ZString[]
				{
					TransactionTypes.GLStandardJournal,
					TransactionTypes.GLReversingJournal,
					TransactionTypes.GLAutoJournal,
					TransactionTypes.GLNoteJournal
				};
			}
		}

		public override ZInt LocalCurrencyDecimals => Journal.IsNoteJournal ? new ZInt(AccountingConstants.CurrencyDefaultValues.NoteJournalCurrencyDecimal) : base.LocalCurrencyDecimals;

		GLJournal Journal
		{
			get { return (GLJournal)transaction; }
		}
	}
}

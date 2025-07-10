using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business
{
	public class WIPACRAccountingJournal : AccountingJournal
	{
		public WIPACRAccountingJournal(TransactionLine transactionLine, ReadOnlyBusinessObjectFactory factory, AccReportingBook reportingBook = null, DataTable generalLedgerTransactionData = null)
			: base(factory, reportingBook, generalLedgerTransactionData)
		{
			Argument.NotNull(transactionLine, "transactionLine");
			this.transactionLine = transactionLine;
			PopulateJournalLines();
		}

		public override ZString Ledger
		{
			get
			{
				return LedgerTypes.JobCosting;
			}
		}

		public override ZString TransactionType
		{
			get
			{
				return transactionLine.AL_LineType;
			}
		}

		public override ZString TransactionNumber
		{
			get
			{
				return Job != null ? Job.JH_JobLocalReference : ZString.Empty;
			}
		}

		public override ZString TransactionDescription
		{
			get { return Job != null ? Job.JH_JobNum : ZString.Empty; }
		}

		public override ZString Currency
		{
			get
			{
				return transactionLine.TransactionCurrency.RX_Code;
			}
		}

		public override ZDateTime CreatedDate => transactionLine.AL_Calc_CreatedDate;

		public override ZString CreatedBy => transactionLine.AL_SystemCreateUser;

		public override ZString TransactionReference => ZString.Empty;

		public override ZString ConsolidatedInvoiceRef => ZString.Empty;

		public override ZString ComplianceSubType => ZString.Empty;

		public override JobHeader Job
		{
			get
			{
				return transactionLine.Job;
			}
		}

		protected override AccountingJournalLineProvider JournalLineProvider
		{
			get
			{
				if (ReportingBook != null)
				{
					journalLineProvider = new AccountingJournalLineProviderForReportingBook(transactionLine, ValidLedgerTypes, ValidTransactionTypes, reportingBook, GeneralLedgerTransactionData);
				}
				else if (AccountingConfigurationRegistry.Instance.GenerateAndStoreJournalEntriesForPostedAccountingTransactions.Value)
				{
					journalLineProvider = new AccountingJournalLineProviderForGeneralLedgerData(transactionLine, ValidLedgerTypes, ValidTransactionTypes);
				}
				else if (transactionLine != null && journalLineProvider == null)
				{
					journalLineProvider = new AccountingJournalLineProviderForTransactionLine(transactionLine, ValidLedgerTypes, ValidTransactionTypes);
				}
				return journalLineProvider;
			}
		}

		public override Dictionary<ZString, ZString> ApplicableOptionalFields
		{
			get
			{
				if (applicableOptionalFields_innervalue == null)
				{
					applicableOptionalFields_innervalue = new Dictionary<ZString, ZString>();
					StmALog reversingEvent = null;

					if (transactionLine != null)
					{
						reversingEvent = transactionLine.GetLogs().Find(new Func<StmALog, bool>(delegate(StmALog x) { return x.SL_SE_NKEvent == Events.TransactionReversed.Code && !x.SL_IsEstimate && x.SL_EventTime.IsValid; })).FirstOrDefault();

						if (reversingEvent != null
							&& (transactionLine.AL_LineType == TransactionLineTypes.Accrual || transactionLine.AL_LineType == TransactionLineTypes.WIP))
						{
							applicableOptionalFields_innervalue.Add(DateReversedText, reversingEvent.SL_EventTime.ToShortDateString());
							applicableOptionalFields_innervalue.Add(ReversedByText, reversingEvent.User != null ? reversingEvent.User.GS_FullName : (ZString)"N/A");
						}
					}
				}

				return applicableOptionalFields_innervalue;
			}
		}
		Dictionary<ZString, ZString> applicableOptionalFields_innervalue;

		readonly TransactionLine transactionLine;

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
				return new ZString[]
				{
					TransactionLineTypes.Accrual,
					TransactionLineTypes.WIP
				};
			}
		}

		public override bool GroupByLinesWhilePrinting
		{
			get
			{
				return false;
			}
		}
	}
}

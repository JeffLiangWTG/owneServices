using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business
{
	public interface IAccountingJournal
	{
		ZString Ledger { get; }
		ZString TransactionType { get; }
		ZString TransactionNumber { get; }
		ZString TransactionDescription { get; }
		ZString TransactionReference { get; }
		ZString ConsolidatedInvoiceRef { get; }
		ZString ComplianceSubType { get; }
		List<GLJournal> RelatedGLJournals { get; }
		ZString Currency { get; }
		ZInt LocalCurrencyDecimals { get; }
		JobHeader Job { get; }
		ZString JournalName { get; }
		ZDateTime CreatedDate { get; }
		ZString CreatedBy { get; }
		ZString LastPostedRequest_RequesterFullName { get; }
		ZDateTime LastPostedRequest_RequestedTime { get; }
		ZString LastPostedRequest_ApproverFullName { get; }
		ZDateTime LastPostedRequest_ApprovedTime { get; }
		ZString OriginalRequest_RequesterFullName { get; }
		ZString OriginalPostedRequest_ApproverFullName { get; }
		bool GroupByLinesWhilePrinting { get; }
		DocumentSupporter DocumentSupporter { get; }
		BusinessObjectFactory Factory { get; }
		IEnumerable<IAccountingJournalLine> Lines { get; }
		IEnumerable<IAccountingJournalTaxDetail> TaxDetails { get; }
		Dictionary<ZString, ZString> ApplicableOptionalFields { get; }
		ZString PostToPeriod { get; }
		ZString JournalEntriesNumber { get; }
		ZBool IsMissingPeriodForReportingBook { get; }
		DataTable GeneralLedgerTransactionData { get; }

		AccReportingBook ReportingBook { get; }
		ZString ReportingBookCode { get; }
		ZString ReportingBookDescription { get; }

		ZString ChartCode { get; }
		ZString ChartDescription { get; }

		bool DisplayParentAccount { get; }
	}
}

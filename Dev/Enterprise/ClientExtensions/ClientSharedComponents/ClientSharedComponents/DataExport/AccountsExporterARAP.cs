using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;

namespace Enterprise.ClientSharedComponents
{
	public abstract class AccountsExporterARAP : AccountsExporter
	{
		protected AccountsExporterARAP(BusinessObjectFactory factory, NotificationBuffer notifications)
			: base(factory, notifications)
		{
		}

		/// <summary>
		/// Accumulate the data export counters of this.Converter (AR by default) and a given AP Exporter.Converter.
		/// </summary>
		/// <param name="exporter"></param>
		public void AccumulateCounters(AccountsExporterARAP apExporter)
		{
			((AccountsConverterARAP)Converter).AccumulateCounters((AccountsConverterARAP)(apExporter.Converter));
		}

		public bool AllTransactionsExported
		{
			get
			{
				return (IsExportSuccessful(NumberOfLedgerSpecificInvoicesInBatch(CurrentLedgerType), NumberOfInvoicesProcessed_FlatFile) &&
				IsExportSuccessful(NumberOfLedgerSpecificCreditNotesInBatch(CurrentLedgerType), NumberOfCreditNotesProcessed_FlatFile) &&
				IsExportSuccessful(NumberOfLedgerSpecificAdjustmentNotesInBatch(CurrentLedgerType), NumberOfAdjustmentNotesProcessed_FlatFile) &&
				CurrentLedgerType == ZArchitecture.Core.LedgerTypes.AccountsReceivable ?
					IsExportSuccessful(NumberOfWipPostingsInBatch, NumberOfWipPostingProcessed_FlatFile) &&
					IsExportSuccessful(NumberOfWipReversalsInBatch, NumberOfWipReversingProcessed_FlatFile) :
					IsExportSuccessful(NumberOfAccrualPostingsInBatch, NumberOfAccrualPostingProcessed_FlatFile) &&
					IsExportSuccessful(NumberOfAccrualReversalsInBatch, NumberOfAccrualReversingProcessed_FlatFile));
			}
		}

		protected abstract string CurrentLedgerType { get; }
	}
}

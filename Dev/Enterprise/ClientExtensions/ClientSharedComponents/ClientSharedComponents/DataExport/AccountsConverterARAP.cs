using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;

namespace Enterprise.ClientSharedComponents
{
	/// <summary>
	/// Ledger specific Accounting Data Conversion Class to a fixed or delimited text file.
	/// </summary>
	/// <remarks>When data exporting all AR & AP transactions into separate files, inherit from here.</remarks>
	public abstract class AccountsConverterARAP : AccountsConverter
	{
		public AccountsConverterARAP(BusinessObjectFactory factory, NotificationBuffer notifications)
			: base(factory, notifications)
		{
		}

		/// <summary>
		/// Accumulate ledger specific processed transactions.
		/// </summary>
		/// <param name="converter">Opposite ledger type to the current instance.</param>
		/// <remarks>This method adds the counters of one ledger specific child instance (eg:AP) to the counters of this
		/// instance (eg:AR) for the pursposes of reporting the results of the all AR and AP transactions. </remarks>
		public virtual void AccumulateCounters(AccountsConverter converter)
		{
			/* Add the counters of one child instance (eg:AP) to the counters of the other
			 * child instance (eg:AR), then report the stats using one instance (eg:AR). */

			fNumberOfAccrualPostingProcessed += converter.NumberOfAccrualPostingProcessed;
			fNumberOfAccrualReversingProcessed += converter.NumberOfAccrualReversingProcessed;
			fNumberOfAdjustmentNotesProcessed += converter.NumberOfAdjustmentNotesProcessed;
			fNumberOfCreditNotesProcessed += converter.NumberOfCreditNotesProcessed;
			fNumberOfInvoicesProcessed += converter.NumberOfInvoicesProcessed;
			fNumberOfWipPostingProcessed += converter.NumberOfWipPostingProcessed;
			fNumberOfWipReversingProcessed += converter.NumberOfWipReversingProcessed;
		}
	}
}

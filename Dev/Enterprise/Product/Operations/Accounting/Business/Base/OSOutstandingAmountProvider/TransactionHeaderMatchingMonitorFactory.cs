using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business
{
	public class TransactionHeaderMatchingMonitorFactory
	{
		public TransactionHeaderMatchingMonitor CreateMatchingMonitor(TransactionHeader header)
		{
			TransactionHeaderMatchingMonitor matchingMonitor;
			if (header.AH_TransactionType == TransactionTypes.Journal && (header.AH_Ledger == LedgerTypes.AccountsPayable || header.AH_Ledger == LedgerTypes.AccountsReceivable))
			{
				matchingMonitor = new JournalMatchingMonitor(header);
			}
			else
			{
				matchingMonitor = new TransactionHeaderMatchingMonitor(header);
			}
			return matchingMonitor;
		}
	}
}

using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.GeneralLedger.GLJournal
{
	internal class GLJournalLineLookups : TransactionLineLookups
	{
		public GLJournalLineLookups(DependentTransactionLine parent) : base(parent)
		{
		}

		public override AccTransactionHeaderCollection TransactionHeaders => new GLJournalCollection(Factory);
	}
}

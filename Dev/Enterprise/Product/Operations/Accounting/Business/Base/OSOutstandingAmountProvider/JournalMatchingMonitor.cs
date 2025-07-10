using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business
{
	public class JournalMatchingMonitor : TransactionHeaderMatchingMonitor
	{
		public JournalMatchingMonitor(TransactionHeader sourceHeader) : base(sourceHeader) { }

		protected override bool ShouldMakeOSOutstandingAmountApplicable => base.ShouldMakeOSOutstandingAmountApplicable && Header.LatestMatchLink == null;
	}
}

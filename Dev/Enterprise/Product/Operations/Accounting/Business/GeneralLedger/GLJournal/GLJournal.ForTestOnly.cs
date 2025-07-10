#if DEBUG

using CargoWise.Types;

namespace Enterprise.Accounting.Business.GeneralLedger.GLJournals
{
	public partial class GLJournal
	{
		public ZDecimal RoundAmountToLocalDecimals_ForTestOnly(ZDecimal amount)
		{
			return RoundAmountToLocalDecimals(amount);
		}

		public GLJournalApprovalRequest OriginalRequest_ForTestOnly => OriginalRequest;

		public GLJournalApprovalRequest LastRequest_ForTestOnly => LastRequest;

		public ZGuid LatestLinkedApprovalRequestPK_ForTestOnly
		{
			get { return latestLinkedApprovalRequestPK; }
			set { latestLinkedApprovalRequestPK = value; }
		}

		public bool HasAssignedExportBatchNumberLines_ForTestOnly => HasAssignedExportBatchNumberLines;
	}
}

#endif

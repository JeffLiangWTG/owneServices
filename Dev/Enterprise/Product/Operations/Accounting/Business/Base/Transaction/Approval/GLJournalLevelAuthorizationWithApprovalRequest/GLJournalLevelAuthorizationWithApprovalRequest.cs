using System.Linq;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;

namespace Enterprise.Accounting.Business.TransactionApproval
{
	public class GLJournalLevelAuthorizationWithApprovalRequest : LevelAuthorizationWithApprovalRequest<GLJournal, GLJournalApprovalRequest, GLJournalApprovalRequestDetails>
	{
		public GLJournalLevelAuthorizationWithApprovalRequest(IPostingTransactionApprovalGUIProvider postingGUIProvider, GLJournal journal, bool alwaysCreateApprovalRequest)
			: base(postingGUIProvider)
		{
			this.journal = journal;
			this.alwaysCreateApprovalRequest = alwaysCreateApprovalRequest;
		}
		protected readonly GLJournal journal;
		readonly bool alwaysCreateApprovalRequest;

		protected override bool ShouldAlwaysCreateRequest(GLJournal maxTransaction)
		{
			return GLJournalApprovalAuthorizationHelper.ShouldCreateApprovalWithoutUsersConfirm(maxTransaction)
				|| base.ShouldAlwaysCreateRequest(maxTransaction);
		}

#if DEBUG
		public
#else
		protected
#endif
		override bool ConfirmAndPreEditApprovalRequestByUser(TransactionApprovalRequest<GLJournalApprovalRequestDetails>[] approvalRequests, GLJournal maxTransaction)
		{
			if (GLJournalApprovalAuthorizationHelper.ShouldCreateApprovalWithoutUsersConfirm(journal))
			{
				approvalRequests.First().XP_ReasonDescription = Res.GetString("68E0038B-E3D2-4D98-90EC-D94086F2EA3C", "APPROVAL REQUEST FOR GENERAL LEDGER JOURNAL");
				return true;
			}

			return base.ConfirmAndPreEditApprovalRequestByUser(approvalRequests, maxTransaction);
		}

		public bool PerformLevelAuthorization(bool isApprovedRequestPosting = false)
		{
			return PerformTransactionLevelAuthorization(new[] { journal }, out bool canContinueReversing, isApprovedRequestPosting);
		}

		public static bool CheckLevelSecurityRights(GLJournal transaction)
		{
			return GLJournalApprovalAuthorizationHelper.CheckLevelSecurityRights(transaction);
		}

		protected override bool ShouldCreateRequestOnEveryPosting { get { return true; } }

		protected override bool IsLevelAuthorizationRequiredCore(GLJournal transaction)
		{
			var requiredCheckpoint = GLJournalApprovalAuthorizationHelper.RequiredSecurityCheckPoint(transaction);
			return alwaysCreateApprovalRequest || !requiredCheckpoint.IsAllowed || GLJournalApprovalAuthorizationHelper.IsUserHasRightButNotAllowedToApproveOwnJournal(requiredCheckpoint);
		}

		protected override bool CheckLevelSecurityRightsCore(GLJournal transaction)
		{
			return !alwaysCreateApprovalRequest && CheckLevelSecurityRights(transaction);
		}

		protected override void InitializeApprovalRequestCore(GLJournalApprovalRequest request, GLJournal[] transactions)
		{
			request.Initialize(journal);
		}

		protected override ITransactionApprovalHelper GetNewHelperCore(GLJournalApprovalRequest request)
		{
			return new GLJournalApprovalHelper(request, journal.AH_Desc);
		}
	}
}

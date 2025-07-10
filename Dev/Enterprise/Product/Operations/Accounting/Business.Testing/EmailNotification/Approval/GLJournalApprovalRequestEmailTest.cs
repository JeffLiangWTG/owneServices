using CargoWise.Types;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.EmailNotification.Testing
{
	public class GLJournalApprovalRequestEmailTest : TransactionApprovalRequestEmailTest<GLJournalApprovalRequest, GLJournalApprovalRequestDetails>
	{
		protected override bool ShouldSendEmailForRequestedApproval
		{
			get { return true; }
		}

		protected override TransactionApprovalRequestEmail GetNewEmail(GLJournalApprovalRequest approvalRequest)
		{
			return new GLJournalApprovalRequestEmail(approvalRequest, "General Ledger Journal");
		}

		protected override string GetApprovalBizoName()
		{
			return "General Ledger Journal";
		}

		protected override GLJournalApprovalRequest CreateRequest()
		{
			var journal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today.AddMonths(-1));
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.CR, TestObjectCreator.ExchangeGainLossControlAccount.PK);
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.DR, TestObjectCreator.ExchangeGainLossAdjustmentAccount.PK);
			Factory.Save();
			var request = base.CreateRequest();
			request.Initialize(journal);

			return request;
		}

		protected override string GetExpectedEmailSubjectForTest(bool approved = false, string jobNumber = null, bool firstCheck = false)
		{
			if (firstCheck)
			{
				return "General Ledger Journal approval request number '00000001' for Journal number '00001000' was Requested";
			}
			else
			{
				return "General Ledger Journal approval request number '00000001' for Journal number '00001000' was " + (approved ? "Approved" : "Rejected");
			}
		}

		protected override GlbGroup GetNotificationGroupForRequestedApproval()
		{
			var currentUserInCurrentFactory = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			var group = Factory.New<GlbGroup>();
			group.Staff.Add(currentUserInCurrentFactory);

			return group;
		}
	}
}

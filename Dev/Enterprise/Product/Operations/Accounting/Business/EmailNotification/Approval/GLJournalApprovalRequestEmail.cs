using CargoWise.Types;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static System.FormattableString;

namespace Enterprise.Accounting.Business.EmailNotification
{
	public class GLJournalApprovalRequestEmail : TransactionApprovalRequestEmail
	{
		public GLJournalApprovalRequestEmail(GLJournalApprovalRequest approvalRequest, ZString approvingJournalHumanReadableName)
			: base(approvalRequest)
		{
			journalHumanReadableName = approvingJournalHumanReadableName;
		}

		readonly ZString journalHumanReadableName;

		new GLJournalApprovalRequest ApprovalRequest => base.ApprovalRequest as GLJournalApprovalRequest;

		protected override string GetApprovalBizoName()
		{
			return journalHumanReadableName;
		}

		protected override string GetRequestID()
		{
			var journalNumber = ApprovalRequest.JournalNumber;
			var journalNumberMessage = journalNumber.IsEmpty ? "" : string.Format((NoResString)" for Journal number '{0}'", journalNumber);
			return Invariant($"number '{ApprovalRequest.XP_RequestID}'{journalNumberMessage}");
		}

		protected override GuidRegistryItem NotificationGroupForRequestedApproval()
		{
			return AccountingConfigurationRegistry.Instance.GLJournalsApprovalNotifyGroup;
		}
	}
}

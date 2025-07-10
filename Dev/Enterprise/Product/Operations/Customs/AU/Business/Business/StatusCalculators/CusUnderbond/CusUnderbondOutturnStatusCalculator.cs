
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusUnderbondOutturnStatusCalculator : CMRStatusCalculator<CusUnderbond>
	{
		public CusUnderbondOutturnStatusCalculator(CusUnderbond underbond) : base(underbond)
		{
			this.underbond = underbond;
			WarnIfInInconsistentStateOrOriginalFailed();
		}
		readonly CusUnderbond underbond;

		protected internal override ZPropertyInfo StatusInfo => Parent.OutturnStatus.CodeInfo;

		protected internal override ZString[] InterestedMessageTypes => new ZString[] { CMRMessage.CMRMessageTypes.AIROUT, CMRMessage.CMRMessageTypes.SEAOUT };

		protected internal override bool InterestedInPendingMessages => true;

		protected override void DeriveStatusCore()
		{
			base.DeriveStatusCore();

			if (Parent.OutturnStatus.Code == CMRBaseStatuses.Codes.OriginalAccepted ||
				Parent.OutturnStatus.Code == CMRBaseStatuses.Codes.AmendmentAccepted ||
				Parent.OutturnStatus.Code == CMRBaseStatuses.Codes.WithdrawalAccepted)
			{
				UpdateLastMessageDateForOutturns();
			}

			if (Parent.OutturnStatus.Code == CMRBaseStatuses.Codes.AwaitingResponseToOriginal ||
				Parent.OutturnStatus.Code == CMRBaseStatuses.Codes.AwaitingResponseToAmendment ||
				Parent.OutturnStatus.Code == CMRBaseStatuses.Codes.AwaitingResponseToWithdrawal ||
				Parent.OutturnStatus.Code == CMRBaseStatuses.Codes.NotSent)
			{
				underbond.CusUnderbondOutturnLogManager.CancelAllOutturnLogs();
			}

			if (!Parent.IsDeleted)
			{
				Parent.RefreshBindingIncludingChildren();
			}
		}

		void UpdateLastMessageDateForOutturns()
		{
			var messageDate = ZDateTime.Empty;
			if (Parent.OutturnStatus.Code == CMRBaseStatuses.Codes.OriginalAccepted ||
				Parent.OutturnStatus.Code == CMRBaseStatuses.Codes.AmendmentAccepted)
			{
				messageDate = ZDateTime.Today;
			}

			foreach (CusOutturn outturn in underbond.Outturns)
			{
				if (Parent.OutturnStatus.Code == CMRBaseStatuses.Codes.WithdrawalAccepted || outturn.C5_MessageStatus != CMRUnderbondStatuses.Codes.OutturnAcceptedThisOutturnLineRejected)
				{
					if (outturn.C5_LastMessageDate.IsEmpty || messageDate.IsEmpty)
					{
						outturn.C5_LastMessageDate = messageDate;
					}
				}
			}
		}

		protected void WarnIfInInconsistentStateOrOriginalFailed()
		{
			if (underbond != null && underbond.OutturnStatus != null)
			{
				if (underbond.HasSplitMessageOriginalRejectedLog)
				{
					StatusInfo.AddMessageError("Outturn in error - correct error & resend.\r\nThe previous Outturn message had to be split into multiple messages as it exceeded the maximum number of lines allowed by Customs.\r\nThe first of the split messages has been rejected by Customs.\r\n\r\nYou therefore need to correct the error and resend the outturn as an original again.");
				}
				else if (underbond.HasSplitMessageFailedLog)
				{
					StatusInfo.AddMessageError("Inconsistent Outturn State - withdraw & resend original.\r\nThe previous Outturn message had to be split into multiple messages as it exceeded the maximum number of lines allowed by Customs.\r\nOne of the split messages has been rejected leaving the Outturn in an inconsistent state with Customs.\r\n\r\nYou need to withdraw the current outturn, fix the error noted by Customs and then resend the Outturn again.");
				}
				else if (underbond.HasNonExistantLineAtCustomsLog)
				{
					StatusInfo.AddMessageError("Inconsistent Outturn State - withdraw & resend original.\r\nThe previous Outturn message was rejected trying to amend or delete lines that do not exist at Customs.\r\nThe Outturn is therefore in an inconsistent state to that recorded at Customs.\r\n\r\nYou need to withdraw the current outturn and then resend as an original.");
				}
			}
		}
	}
}

using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.BR.Business
{
	public class LPCOMessageManager : BaseMessageManager
	{
		public LPCOMessageManager(IMessageSendingObject messageSender) : base(messageSender)
		{
		}

		LPCOMessageSendingObject MessageSender => messageSender as LPCOMessageSendingObject;

		CusLPCOHeader LPCOHeader => MessageSender.Parent.LPCOHeader;

		public override string MessageFriendlyName => MessageTypeList.Descriptions.CAT;

		public override bool CanSendOriginal => true;

		public override bool CanSendWithdrawal => false;

		public override bool IsWaitingForResponse => false;

		protected override string OriginalMessageType => LPCOMessageTypesList.Codes.ORI;

		protected override string AmendmentMessageType => LPCOMessageTypesList.Codes.RET;

		protected override string WithdrawalMessageType => null;

		ZString originalStatus;

		protected override void AfterGenerateMessage(IEnumerable<EDIMessage> messages)
		{
			base.AfterGenerateMessage(messages);
			LPCOHeader.Messages.AddRange(messages);
			originalStatus = LPCOHeader.CPH_MessageStatus;
			LPCOHeader.CPH_MessageStatus = BRMessageStatusList.Codes.AwaitingResponse;
		}

		public override void RollbackOnSavingFailed()
		{
			base.RollbackOnSavingFailed();
			LPCOHeader.CPH_MessageStatus = originalStatus;
		}
	}
}

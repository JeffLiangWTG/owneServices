using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.BR.Business
{
	public class SubscriptionMessageManager : BaseMessageManager
	{
		public SubscriptionMessageManager(SubscriptionMessageSendingObject messageSender) : base(messageSender)
		{
		}

		SubscriptionMessageSendingObject MessageSender => messageSender as SubscriptionMessageSendingObject;

		public override string MessageFriendlyName => MessageTypeList.Descriptions.SUB;

		public override bool CanSendOriginal => true;

		public override bool CanSendWithdrawal => false;

		public override bool IsWaitingForResponse => false;

		public override bool HasActiveMessages => false;

		protected override string OriginalMessageType => SubscriptionMessageTypesList.Codes.ORI;

		protected override string AmendmentMessageType => SubscriptionMessageTypesList.Codes.AMD;

		protected override string WithdrawalMessageType => SubscriptionMessageTypesList.Codes.CAN;

		GlbExternalPassword_BRS Subscription => MessageSender.Subscription;

		ZString originalStatusReason;

		ZString originalPasswordStatus;

		protected override void AfterGenerateMessage(IEnumerable<EDIMessage> messages)
		{
			base.AfterGenerateMessage(messages);

			originalStatusReason = Subscription.GP_StatusReason;
			originalPasswordStatus = Subscription.GP_PasswordStatus;
			Subscription.GP_StatusReason = GlbExternalPassword_BRS.StatusReasons.AwaitingResponse;
			Subscription.GP_PasswordStatus = BRPasswordStatusList.Codes.AwaitingResponse;
		}

		public override void RollbackOnSavingFailed()
		{
			base.RollbackOnSavingFailed();

			Subscription.GP_StatusReason = originalStatusReason;
			Subscription.GP_PasswordStatus = originalPasswordStatus;
		}
	}
}

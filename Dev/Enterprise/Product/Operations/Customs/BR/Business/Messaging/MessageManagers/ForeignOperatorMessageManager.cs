using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.BR.Business
{
	public class ForeignOperatorMessageManager : BaseMessageManager
	{
		public ForeignOperatorMessageManager(ForeignOperatorMessageSendingObject messageSender) : base(messageSender)
		{
		}

		ForeignOperatorMessageSendingObject MessageSender => messageSender as ForeignOperatorMessageSendingObject;

		public CusBRForeignOperator ForeignOperator => MessageSender.ForeignOperator;

		public override string MessageFriendlyName => MessageTypeList.Descriptions.OPE;

		public override bool CanSendOriginal => true;

		public override bool CanSendWithdrawal => false;

		public override bool IsWaitingForResponse => ForeignOperator.IsMessageAwaitingResponse;

		public override bool HasActiveMessages => ForeignOperator.IsInAStatusAmendmentSendable;

		protected override string OriginalMessageType => ForeignOperatorMessageTypesList.Codes.ORI;

		protected override string AmendmentMessageType => ForeignOperatorMessageTypesList.Codes.AMD;

		protected override string WithdrawalMessageType => ForeignOperatorMessageTypesList.Codes.CAN;

		ZString originalStatus;

		protected override void AfterGenerateMessage(IEnumerable<EDIMessage> messages)
		{
			base.AfterGenerateMessage(messages);
			ForeignOperator.Messages.AddRange(messages);
			originalStatus = ForeignOperator.BFR_MessageStatus;

			if (messages.Any())
			{
				ForeignOperator.BFR_MessageStatus = BRMessageStatusList.Codes.AwaitingResponse;
				if (!MessageSender.Action.IsEmpty)
				{
					ForeignOperator.Logs.AddNew(Events.MessageSent, MessageSender.Action);
				}
			}
		}

		public override void RollbackOnSavingFailed()
		{
			base.RollbackOnSavingFailed();
			ForeignOperator.BFR_MessageStatus = originalStatus;
		}

		protected override BusinessObject GetBusinessObjectInNewFactory(BusinessObject businessObject)
		{
			var messageSendingObject = businessObject as ForeignOperatorMessageSendingObject;
			var foreignOperator = base.GetBusinessObjectInNewFactory(messageSendingObject.ForeignOperator) as CusBRForeignOperator;
			return new ForeignOperatorMessageSendingObject(foreignOperator);
		}
	}
}

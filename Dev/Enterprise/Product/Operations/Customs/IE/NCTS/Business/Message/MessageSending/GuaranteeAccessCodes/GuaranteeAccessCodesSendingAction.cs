using System;
using CargoWise.Types;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class GuaranteeAccessCodesSendingAction : EU.NCTS.Business.GuaranteeAccessCodesSendingObject, IMessageSendingAction
	{
		public GuaranteeAccessCodesSendingAction(CusGuaranteeHeader guarantee) : base(guarantee)
		{
			MessageType = NCTSOutgoingMessageTypeList.Codes.GuaranteeAccessCodes;
		}

		public new CusGuaranteeHeader CusGuaranteeHeader => (CusGuaranteeHeader)base.CusGuaranteeHeader;

		public MessageSender CreateSender() => (MessageSender)Activator.CreateInstance(typeof(NctsMessageSender), this);

		public ZString MessageType { get; }

		public IMessageAttachee MessageAttachee => CusGuaranteeHeader;

		public void AddMessage(OutboundEDIMessage message)
		{
			CusGuaranteeHeader.Messages.Add(message);
		}
	}
}

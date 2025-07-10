using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.DE.Business
{
	public class MessageSendingAction : BaseMessageSendingObject
	{
		public MessageSendingAction(BusinessObject messagingObject, Func<BusinessObject, ZString> getDetails, MessageSendingActionParent actionParent = null) : base(messagingObject.Factory)
		{
			MessagingObject = messagingObject;
			this.getDetails = getDetails;
			ActionParent = actionParent;
		}
		readonly Func<BusinessObject, ZString> getDetails;

		public MessageSendingActionParent ActionParent { get; }

		public ZString Details => getDetails(MessagingObject);

		public BusinessObject MessagingObject { get; }

		protected void SetMrnCusEntryNumber(ZString value)
		{
			if (MessagingObject is CusEntryHeader entryHeader)
			{
				entryHeader.MovementReferenceNumberSetter(value);
			}
		}
	}
}

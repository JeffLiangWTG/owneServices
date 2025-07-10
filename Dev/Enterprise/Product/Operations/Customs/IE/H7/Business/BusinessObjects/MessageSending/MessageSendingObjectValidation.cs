using CargoWise.EntityFramework;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.H7.Business
{
	public class MessageSendingObjectValidation : EU.H7.Business.MessageSendingObjectValidation
	{
		public MessageSendingObjectValidation(AutoMessageSendingObject parent)
			: base(parent)
		{
		}

		protected override void CheckAction()
		{
			ListValidation.ErrorIfInvalidCode(Parent.ActionInfo);
		}

		protected override void CheckSubStyle()
		{
			ListValidation.ErrorIfInvalidCode(Parent.SubStyleInfo);
		}

		protected override void CheckAmendmentInvalidationReason()
		{
			if (Parent.Action == AISOutgoingMessageTypeList.Codes.InvalidationRequest || Parent.Action == AISOutgoingMessageTypeList.Codes.AmendmentRequest)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.AmendmentInvalidationReasonInfo);
			}
		}
	}
}

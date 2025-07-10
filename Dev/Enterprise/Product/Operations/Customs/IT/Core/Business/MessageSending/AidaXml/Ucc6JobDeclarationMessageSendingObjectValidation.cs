using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.Business;

public class Ucc6JobDeclarationMessageSendingObjectValidation : JobDeclarationMessageSendingObjectValidation
{
	public Ucc6JobDeclarationMessageSendingObjectValidation(Ucc6JobDeclarationMessageSendingObject parent) : base(parent)
	{
	}

	protected override void CheckMessageType()
	{
		base.CheckMessageType();

		var messageTypeInfo = Parent.MessageTypeInfo;
		MandatoryValidation.CheckEntered(messageTypeInfo);
		ListValidation.ErrorIfInvalidCode(messageTypeInfo);
	}

	protected override void CheckVOCReason()
	{
		base.CheckVOCReason();

		if (ShouldSendMessageAndIsMessageTypeCancelOrAmend)
		{
			var vocReasonInfo = Parent.VOCReasonInfo;
			MandatoryValidation.CheckEntered(vocReasonInfo);
			ListValidation.ErrorIfInvalidCode(vocReasonInfo);
		}
	}

		protected override void CheckCancellationAndAmendmentLegislativeReference()
		{
			base.CheckCancellationAndAmendmentLegislativeReference();

			var parent = Parent;
			var referenceInfo = parent.CancellationAndAmendmentLegislativeReferenceInfo;

			if (parent.ShouldSend && parent.IsCancel)
			{
				MandatoryValidation.CheckEntered(referenceInfo);
			}

			if (ShouldSendMessageAndIsMessageTypeCancelOrAmend)
			{
				ListValidation.ErrorIfInvalidCode(referenceInfo);
			}
		}

	protected override void CheckStatusAllowsSending(JobDeclarationMessageSendingObject sendingObject, ZPropertyInfo shouldSendInfo)
	{
		if (!Parent.IsCancel)
		{
			base.CheckStatusAllowsSending(sendingObject, shouldSendInfo);
		}
	}

	bool ShouldSendMessageAndIsMessageTypeCancelOrAmend => Parent.ShouldSend && (Parent.IsCancel || Parent.IsAmend);
}

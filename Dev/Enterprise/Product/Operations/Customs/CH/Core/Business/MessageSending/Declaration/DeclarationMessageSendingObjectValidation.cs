using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public class DeclarationMessageSendingObjectValidation : JobDeclarationMessageSendingObjectValidation
{
	public DeclarationMessageSendingObjectValidation(AutoJobDeclarationMessageSendingObject parent) : base(parent)
	{
	}

	protected new DeclarationMessageSendingObject Parent => (DeclarationMessageSendingObject)base.Parent;

	public override void ValidateAll()
	{
		base.ValidateAll();
		ValidateReasonText();
	}

	protected override void CheckMessageType()
	{
		base.CheckMessageType();

		if (Parent.ShouldSend)
		{
			MandatoryValidation.CheckEntered(Parent.MessageTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.MessageTypeInfo);
		}
	}

	protected override void CheckVOCReason()
	{
		base.CheckVOCReason();

		if (Parent.ShouldSend && !Parent.IsVOCReason_ReadOnly)
		{
			MandatoryValidation.CheckEntered(Parent.VOCReasonInfo);
			ListValidation.ErrorIfInvalidCode(Parent.VOCReasonInfo);
		}

		ValidateReasonText();
	}

	public void ValidateReasonText()
	{
		ValidateCalculatedProperty(Parent.ReasonTextInfo);
	}

	protected virtual void CheckReasonText()
	{
	}
}

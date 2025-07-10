using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.H7.Business;

public class MessageSendingObjectValidation : EU.H7.Business.MessageSendingObjectValidation
{
	public MessageSendingObjectValidation(MessageSendingObject parent)
		: base(parent)
	{
	}

	public new MessageSendingObject Parent => (MessageSendingObject)base.Parent;

	protected override void ValidateAllCore()
	{
		base.ValidateAllCore();
		ValidateLegislativeReference();
	}

	protected override void CheckAmendmentReasonCode()
	{
		if (Parent.ActionIsH7MOrH7C)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.AmendmentReasonCodeInfo);
			ListValidation.MessageErrorIfInvalidCode(Parent.AmendmentReasonCodeInfo);
		}
	}

	public void ValidateLegislativeReference()
	{
		ValidateCalculatedProperty(Parent.LegislativeReferenceInfo);
	}

	protected void CheckLegislativeReference()
	{
		if (Parent.ActionIsH7MOrH7C)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.LegislativeReferenceInfo);
			ListValidation.MessageErrorIfInvalidCode(Parent.LegislativeReferenceInfo);
		}
	}

	public void ValidateDutyAmount()
	{
		ValidateCalculatedProperty(Parent.DutyAmountInfo);
	}

	protected void CheckDutyAmount()
	{
		TypeValidation.CheckValidDecimal(Parent.DutyAmountInfo, MessageSendingObject.Schema.DutyAmountDecimalPrecision, MessageSendingObject.Schema.DutyAmountDecimalPlaces);
	}
}

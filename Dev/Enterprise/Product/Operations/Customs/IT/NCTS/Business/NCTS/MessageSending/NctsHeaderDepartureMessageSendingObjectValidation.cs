using CargoWise.EntityFramework;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsHeaderDepartureMessageSendingObjectValidation : AutoNctsHeaderDepartureMessageSendingObjectValidation
{
	public NctsHeaderDepartureMessageSendingObjectValidation(AutoNctsHeaderDepartureMessageSendingObject parent) : base(parent)
	{
	}

	public new NctsHeaderDepartureMessageSendingObject Parent => (NctsHeaderDepartureMessageSendingObject)base.Parent;

	public void ValidateShouldSend()
	{
		((IValidationInternals)this).Validate(Parent.ShouldSendInfo, () => { CheckShouldSend(); });
	}

	protected virtual void CheckShouldSend()
	{
		var entryMessageSendingObjectInfo = (IEntryMessageSendingObjectInfo)Parent;
		if (Parent.ShouldSend && !entryMessageSendingObjectInfo.EntryStatusAllowsSending)
		{
			Parent.ShouldSendInfo.AddWarning(ValidationCaptions.MessageSendingObject.NonSendableStatusWarning(entryMessageSendingObjectInfo.EntryReference));
		}
	}
}

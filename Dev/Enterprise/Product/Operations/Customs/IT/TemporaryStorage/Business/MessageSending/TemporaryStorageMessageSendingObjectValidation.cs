using CargoWise.EntityFramework;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

public class TemporaryStorageMessageSendingObjectValidation : EU.Business.CusTempStorage.TemporaryStorageMessageSendingObjectValidation
{
	public TemporaryStorageMessageSendingObjectValidation(TemporaryStorageMessageSendingObject parent) : base(parent)
	{
	}

	public new TemporaryStorageMessageSendingObject Parent => (TemporaryStorageMessageSendingObject)base.Parent;

	public void ValidateShouldSend()
	{
		((IValidationInternals)this).Validate(Parent.ShouldSendInfo, CheckShouldSend);
	}

	protected virtual void CheckShouldSend()
	{
		var sendingObject = (IEntryMessageSendingObjectInfo)Parent;
		if(Parent.ShouldSend && !sendingObject.EntryStatusAllowsSending)
		{
			Parent.ShouldSendInfo.AddWarning(ValidationCaptions.MessageSendingObject.NonSendableStatusWarning(sendingObject.EntryReference));
		}
	}
}

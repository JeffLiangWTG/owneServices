using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NctsHeaderCommonMessageSendingObjectValidation : NctsHeaderMessageSendingObjectValidation
{
	public NctsHeaderCommonMessageSendingObjectValidation(AutoNctsHeaderMessageSendingObject parent) : base(parent)
	{
	}

	protected override void CheckMessageType()
	{
		MandatoryValidation.CheckEntered(Parent.MessageTypeInfo);
		ListValidation.ErrorIfInvalidCode(Parent.MessageTypeInfo);
	}
}

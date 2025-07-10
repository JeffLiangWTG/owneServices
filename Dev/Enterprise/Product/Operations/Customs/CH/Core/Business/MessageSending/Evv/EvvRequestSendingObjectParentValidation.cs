using CargoWise.EntityFramework;

namespace Enterprise.Customs.CH.Business;

public class EvvRequestSendingObjectParentValidation : AutoEvvRequestSendingObjectParentValidation
{
	internal EvvRequestSendingObjectParentValidation(AutoEvvRequestSendingObjectParent parent) : base(parent)
	{
	}

	protected override void CheckMrnVersion()
	{
		base.CheckMrnVersion();
		MandatoryValidation.MessageErrorIfIsNegative(Parent.MrnVersionInfo);
	}
}

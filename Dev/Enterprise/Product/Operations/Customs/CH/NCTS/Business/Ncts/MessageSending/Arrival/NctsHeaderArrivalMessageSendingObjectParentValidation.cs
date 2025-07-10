using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NctsHeaderArrivalMessageSendingObjectParentValidation : ZValidation
{
	public NctsHeaderArrivalMessageSendingObjectParentValidation(NctsHeaderArrivalMessageSendingObjectParent parent)
		: base(parent)
	{
	}

	protected NctsHeaderArrivalMessageSendingObjectParent Parent => (NctsHeaderArrivalMessageSendingObjectParent)ParentFilter;

	public override Type AutoValidationType => typeof(NctsHeaderArrivalMessageSendingObjectParentValidation);

	public override void ValidateAll()
	{
		ValidateDateOfUnloading();
	}

	public void ValidateDateOfUnloading()
	{
		ValidateCalculatedProperty(Parent.DateOfUnloadingInfo);
	}

	protected void CheckDateOfUnloading()
	{
		if (Parent.DateOfUnloading.IsInTheFuture())
		{
			Parent.DateOfUnloadingInfo.AddMessageError(PassarValidationMessages.MessageNP70025);
		}
	}
}

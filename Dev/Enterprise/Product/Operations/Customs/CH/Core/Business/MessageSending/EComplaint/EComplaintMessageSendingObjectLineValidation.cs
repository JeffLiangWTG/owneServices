using CargoWise.EntityFramework;

namespace Enterprise.Customs.CH.Business;

public class EComplaintMessageSendingObjectLineValidation : AutoEComplaintMessageSendingObjectLineValidation
{
	internal EComplaintMessageSendingObjectLineValidation(AutoEComplaintMessageSendingObjectLine parent) : base(parent)
	{
	}

	protected override void CheckLocation()
	{
		base.CheckLocation();
		MandatoryValidation.CheckEntered(Parent.LocationInfo);
		ListValidation.ErrorIfInvalidCode(Parent.LocationInfo);
	}

	protected override void CheckFieldName()
	{
		base.CheckFieldName();
		MandatoryValidation.CheckEntered(Parent.FieldNameInfo);
		ListValidation.ErrorIfInvalidCode(Parent.FieldNameInfo);
	}

	protected override void CheckEntryLinePK()
	{
		base.CheckEntryLinePK();
		if (Parent.Location == EComplaintLocationList.Codes.Line)
		{
			MandatoryValidation.CheckEntered(Parent.EntryLinePKInfo);
			ListValidation.ErrorIfInvalidPK(Parent.EntryLinePKInfo);
		}
	}
}

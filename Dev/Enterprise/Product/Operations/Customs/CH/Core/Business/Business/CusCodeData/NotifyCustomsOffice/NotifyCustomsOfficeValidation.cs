using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public class NotifyCustomsOfficeValidation : CusCodeDataValidation
{
	public NotifyCustomsOfficeValidation(AutoCusCodeData parent) : base(parent)
	{
	}

	new NotifyCustomsOffice Parent => (NotifyCustomsOffice)base.Parent;

	protected override void CheckCY_Data()
	{
		base.CheckCY_Data();
		ListValidation.MessageErrorIfInvalidCode(Parent.CY_DataInfo);
	}

	protected override void CheckCY_Code()
	{
	}
}

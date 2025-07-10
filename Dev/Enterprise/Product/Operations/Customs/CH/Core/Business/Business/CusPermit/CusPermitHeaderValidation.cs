using CargoWise.EntityFramework;

namespace Enterprise.Customs.CH.Business;

public class CusPermitHeaderValidation : Customs.Business.BaseCusPermitHeaderValidation
{
	public CusPermitHeaderValidation(CusPermitHeader parent)
		: base(parent)
	{
	}

	protected override void CheckCPH_UnitOfMeasure()
	{
		base.CheckCPH_UnitOfMeasure();

		ListValidation.MessageErrorIfInvalidCode(Parent.CPH_UnitOfMeasureInfo);
	}
}

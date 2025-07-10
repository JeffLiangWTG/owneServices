using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsHeaderPhase5Validation : EU.NCTS.Business.NctsHeaderPhase5Validation
{
	public NctsHeaderPhase5Validation(NctsHeader parent)
		: base(parent)
	{
	}

	#region BH_CustomsProfile

	protected override void CheckBH_CustomsProfile()
	{
		base.CheckBH_CustomsProfile();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.BH_CustomsProfileInfo);
	}

	#endregion
}

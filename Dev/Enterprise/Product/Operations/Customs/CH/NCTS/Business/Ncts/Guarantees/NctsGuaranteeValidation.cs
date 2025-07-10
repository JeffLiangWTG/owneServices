using CargoWise.EntityFramework;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NctsGuaranteeValidation : EU.NCTS.Business.NctsGuaranteePhase5Validation
{
	public NctsGuaranteeValidation(NctsGuarantee guarantee) : base(guarantee)
	{
	}

	protected override void CheckPW_BondType()
	{
		if (Parent.NctsHeader.IsDepartureMovement)
		{
			base.CheckPW_BondType();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.PW_BondTypeInfo);
		}
	}
}

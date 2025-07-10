using CargoWise.EntityFramework;

namespace Enterprise.Customs.CH.Business;

public class RestrictionValidation : Customs.Business.CusSupportingInfoValidation
{
	public RestrictionValidation(Restriction parent) : base(parent)
	{
	}

	new Restriction Parent => (Restriction)base.Parent;

	protected override void CheckCSI_Code()
	{
		base.CheckCSI_Code();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CSI_CodeInfo);
	}

	protected override void CheckCSI_Description()
	{
		base.CheckCSI_Description();
		ListValidation.MessageErrorIfInvalidCode(Parent.CSI_DescriptionInfo);
	}
}

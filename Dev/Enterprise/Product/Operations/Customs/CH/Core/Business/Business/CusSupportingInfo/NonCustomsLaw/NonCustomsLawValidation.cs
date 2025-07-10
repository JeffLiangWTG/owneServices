using CargoWise.EntityFramework;

namespace Enterprise.Customs.CH.Business;

internal class NonCustomsLawValidation : Customs.Business.CusSupportingInfoValidation
{
	public NonCustomsLawValidation(NonCustomsLaw parent) : base(parent) { }

	protected override void CheckCSI_Code()
	{
		base.CheckCSI_Code();

		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CSI_CodeInfo);
	}
}

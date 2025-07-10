using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.BE.Business;

public class CusGuaranteeHeaderValidation : EU.Business.CusGuaranteeHeaderValidation
{
	public CusGuaranteeHeaderValidation(CusGuaranteeHeader parent)
		: base(parent)
	{
	}

	public new CusGuaranteeHeader Parent => (CusGuaranteeHeader)base.Parent;

	protected override void CheckMainAccessPersonName()
	{
		base.CheckMainAccessPersonName();
		var parent = Parent;
		if (ValidationExtendMethods.GuaranteeTypesApplicable.Contains(parent.CPH_SubType))
		{
			MandatoryValidation.MessageErrorIfNotEntered(parent.MainAccessPersonNameInfo);
		}
	}
}

using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.BE.Business;

public class AccessCodePinRuleValidation : EU.Business.AccessCodePinRuleValidation
{
	public AccessCodePinRuleValidation(CusGuaranteeRule parent)
		: base(parent)
	{
	}

	public new CusGuaranteeRule Parent => (CusGuaranteeRule)base.Parent;

	protected override void CheckCPR_Description()
	{
		base.CheckCPR_Description();
		var parent = Parent;
		if (ValidationExtendMethods.GuaranteeTypesApplicable.Contains(parent.PermitHeader?.CPH_SubType))
		{
			MandatoryValidation.MessageErrorIfNotEntered(parent.CPR_DescriptionInfo);
		}
	}
}

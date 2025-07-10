using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.Business
{
	class CusAuthorisationRuleValidation : Customs.Business.CusAuthorisationRuleValidation
	{
		public CusAuthorisationRuleValidation(CusAuthorisationRule parent) : base(parent)
		{
		}

		protected override void CheckCPR_Description()
		{
			base.CheckCPR_Description();
			var parent = Parent;
			if (CusAuthorisationRuleRequirementHelper.IsDescriptionMandatory(parent.AuthorisationHeader.CPH_Type, parent.CPR_RuleCode)) 
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.CPR_DescriptionInfo);
			}
		}

		protected override void CheckCPR_ValueFromIsValidCode()
		{
			if (Parent.CPR_RuleCode != CusAuthorisationRuleTypeList.Codes.CON)
			{
				base.CheckCPR_ValueFromIsValidCode();
			}
		}
	}
}

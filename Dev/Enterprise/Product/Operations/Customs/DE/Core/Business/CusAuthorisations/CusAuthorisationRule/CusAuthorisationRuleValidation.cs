using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.DE.Business
{
	public class CusAuthorisationRuleValidation : Customs.Business.CusAuthorisationRuleValidation
	{
		public CusAuthorisationRuleValidation(CusAuthorisationRule parent) : base(parent)
		{
		}

		protected override void CheckCPR_RuleCode()
		{
			base.CheckCPR_RuleCode();

			var parent = Parent;
			var ruleCode = parent.CPR_RuleCode;
			var value = parent.CPR_ValueFrom;
			if (ruleCode == Customs.Business.CusAuthorisationRuleTypeList.Codes.Location && !value.IsEmpty)
			{
				var linkedRuleCodeCUSValue = parent.LinkedCusAuthorisationRules.FirstOrDefault(x => x.CPR_RuleCode == LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice)?.CPR_ValueFrom ?? ZString.Empty;
				if (!linkedRuleCodeCUSValue.IsEmpty)
				{
					var existsAnotherRuleWithSameLinkedRuleCodeCUSValue = parent.AuthorisationHeader.CusAuthorisationRules.Where(x => x.CPR_RuleCode == ruleCode && x.CPR_ValueFrom == value && x.PK != parent.PK)
						.SelectMany(y => y.LinkedCusAuthorisationRules).Any(x => x.CPR_RuleCode == LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice && x.CPR_ValueFrom == linkedRuleCodeCUSValue);
					if (existsAnotherRuleWithSameLinkedRuleCodeCUSValue)
					{
						parent.CPR_RuleCodeInfo.AddError(Res.GetString("701A013C-5269-4E59-B98B-3D32BF7C0B66", "This Location Code with same CUS value already exists for this authorization."));
					}
				}
			}
		}
	}
}

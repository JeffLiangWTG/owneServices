using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CA.Business
{
	public class CustomsRuleRuleLookups : Customs.Business.CustomsRuleRuleLookups
	{
		public CustomsRuleRuleLookups(Customs.Business.CustomsRuleRule parent) : base(parent)
		{
		}

		public new CustomsRuleRule Parent => (CustomsRuleRule)base.Parent;

		public override CodeDescriptionPairList RuleCodes
		{
			get
			{
				if (ruleCodes == null)
				{
					ruleCodes = new CustomsRuleRuleCodeList();
					ruleCodes.RemoveCode(CustomsRuleRuleCodeList.Codes.PaymentType);
				}
				return ruleCodes;
			}
		}
		CustomsRuleRuleCodeList ruleCodes;
	}
}

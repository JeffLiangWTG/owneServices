namespace Enterprise.Customs.EU.Business
{
	public class AccessCodePinRuleValidation : Customs.Business.AccessCodePinRuleValidation
	{
		public AccessCodePinRuleValidation(CusGuaranteeRule parent)
			: base(parent)
		{
		}

		public new CusGuaranteeRule Parent => (CusGuaranteeRule)base.Parent;
	}
}

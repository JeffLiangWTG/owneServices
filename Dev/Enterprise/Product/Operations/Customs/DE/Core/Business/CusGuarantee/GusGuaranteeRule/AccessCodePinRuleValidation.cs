
namespace Enterprise.Customs.DE.Business
{
	public class AccessCodePinRuleValidation : EU.Business.AccessCodePinRuleValidation
	{
		public AccessCodePinRuleValidation(CusGuaranteeRule parent)
			: base(parent)
		{
		}

		public new CusGuaranteeRule Parent => (CusGuaranteeRule)base.Parent;

		protected override void CheckCPR_ValueFrom()
		{
			base.CheckCPR_ValueFrom();
			var isTRAGuaranteeType = Parent.GuaranteeHeader.IsTRAGuaranteeType;
			if (isTRAGuaranteeType && Parent.CPR_ValueFrom.Length != 4)
			{
				Parent.CPR_ValueFromInfo.AddMessageError(Res.GetString("9E5ED2C9-7E5B-429C-9548-9A090AE98601", "Access Codes must have 4 digits."));
			}
		}
	}
}

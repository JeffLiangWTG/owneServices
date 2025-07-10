using System.Linq;

namespace Enterprise.Customs.FR.Business
{
	public class CusGuaranteeHeaderValidation : EU.Business.CusGuaranteeHeaderValidation
	{
		public CusGuaranteeHeaderValidation(CusGuaranteeHeader parent)
			: base(parent)
		{
		}

		public new CusGuaranteeHeader Parent => (CusGuaranteeHeader)base.Parent;

		protected override bool ApplyRuleTR0301
		{
			get
			{
				var parent = Parent;
				return parent.CPH_Type == GuaranteeTypeList.Codes.COD && !parent.CPH_SubType.IsEmpty && !parent.AdditionalGuaranteeReferences.OfType<CusGuaranteeReferenceNumber>().Any(x => x.CY_Code == OrgCusAccountDeltaTTypeList.Codes.TR);
			}
		}

		protected override void CheckCPH_Type()
		{
			base.CheckCPH_Type();
			var parent = Parent;
			if ((parent.CPH_Type == GuaranteeTypeList.Codes.COD || parent.CPH_Type == GuaranteeTypeList.Codes.DEF) && !parent.CusGuaranteeRules.Any(x => x.CPR_RuleCode == PermitRuleCodeList.Codes.ENT))
			{
				parent.CPH_TypeInfo.AddError(Res.GetString("E6F098AC-8CEC-445B-B447-F390B06CAC4D", "There should be an ENT Rule for guarantees of type COD or DEF."));
			}
		}
	}
}

using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business
{
	public class CusGuaranteeRuleValidation : EU.Business.CusGuaranteeRuleValidation
	{
		public CusGuaranteeRuleValidation(CusGuaranteeRule parent) : base(parent)
		{
		}

		protected new CusGuaranteeRule Parent => (CusGuaranteeRule)base.Parent;

		protected override void CheckCPR_ValueFrom()
		{
			base.CheckCPR_ValueFrom();
			var parent = Parent;

			switch (parent.CPR_RuleCode)
			{
				case PermitRuleCodeList.Codes.ADD:
					var org = parent.PermitHeader.PermitHolder;
					if (org != null && !org.Addresses.Cast<OrgAddress>().Any(oa => oa.OA_Code == parent.CPR_ValueFrom))
					{
						parent.CPR_ValueFromInfo.AddMessageError(Res.GetString("28fad176-5d9b-41fa-93f8-8d6b9c2a1278", "The selected address short code does not exist on the organization"));
					}
					break;
				case PermitRuleCodeList.Codes.MOD:
				case PermitRuleCodeList.Codes.CAN:
					ListValidation.ErrorIfInvalidCode(Parent.CPR_ValueFromInfo);
					break;
				case PermitRuleCodeList.Codes.ENT:
					ListValidation.ErrorIfInvalidCodeOrEmpty(Parent.CPR_ValueFromInfo);
					break;
			}
		}

		protected override void CheckCPR_RuleCode()
		{
			base.CheckCPR_RuleCode();
			var parent = Parent;
			if (parent.CPR_RuleCode == PermitRuleCodeList.Codes.CAN && parent.GuaranteeHeader.CusGuaranteeRules.Count(x => x.CPR_RuleCode == PermitRuleCodeList.Codes.CAN) > 1)
			{
				parent.CPR_RuleCodeInfo.AddError(Res.GetString("5F1F3F93-D1B0-41B2-A660-2F4D5A6D7472", "Error : CANA for re-export Exemption (AI2) can be entered only one time"));
			}
		}
	}
}

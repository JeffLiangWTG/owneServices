using System.Collections;

namespace Enterprise.Customs.FR.Business
{
	class CusGuaranteeRuleLookups : EU.Business.CusGuaranteeRuleLookups
	{
		public CusGuaranteeRuleLookups(CusGuaranteeRule parent)
			: base(parent)
		{
		}

		public override ICollection CPR_ValueFromList
		{
			get
			{
				switch (Parent.CPR_RuleCode)
				{
					case PermitRuleCodeList.Codes.MOD:
						return new GuaranteeModeCodeList();
					case PermitRuleCodeList.Codes.CAN:
						ICollection result = null;
						if (Parent.PermitHeader != null)
						{
							if (Parent.PermitHeader.CPH_Type == GuaranteeTypeList.Codes.AI2)
							{
								result = new VatCanaForAI2List(this.Factory);
							}
							else if (Parent.PermitHeader.CPH_Type == GuaranteeTypeList.Codes.ALT)
							{
								result = new VatCanaForALTList(this.Factory);
							}
						}
						return result;
					case PermitRuleCodeList.Codes.ENT:
						return new GuaranteeEntryTypeList();
					default:
						return base.CPR_ValueFromList;
				}
			}
		}
	}
}

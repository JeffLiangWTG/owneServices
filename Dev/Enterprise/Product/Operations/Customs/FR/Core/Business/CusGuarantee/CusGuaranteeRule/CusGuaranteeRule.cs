using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business
{
	public class CusGuaranteeRule : EU.Business.CusGuaranteeRule, Integration.Customs.FR.ICusGuaranteeRule
	{
		public CusGuaranteeRule(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
		public new CusGuaranteeHeader GuaranteeHeader => (CusGuaranteeHeader)base.GuaranteeHeader;

		protected override Customs.Business.CusGuaranteeRuleValidation GuaranteeRuleValidation => new CusGuaranteeRuleValidation(this);

		protected override CusPermitRuleLookups GetNewLookups() => new CusGuaranteeRuleLookups(this);

		public override ZString CPR_RuleCode
		{
			get => base.CPR_RuleCode;
			set
			{
				base.CPR_RuleCode = value;
				switch (value)
				{
					case PermitRuleCodeList.Codes.MOD:
						base.CPR_ValueFrom = GuaranteeModeCodeList.Codes.Guarantee;
						break;
					case PermitRuleCodeList.Codes.CAN:
						var list = (CodeDescriptionPairList)Lookups.CPR_ValueFromList;

						if (list.Count == 1)
						{
							base.CPR_ValueFrom = list[0].Code;
						}
						break;
					case PermitRuleCodeList.Codes.ENT:
						GuaranteeHeader.Validation.ValidateCPH_Type();
						break;
				}
			}
		}
	}
}

using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business
{
	public class CusGuaranteeRule : Customs.Business.CusGuaranteeRule, Integration.Customs.EU.ICusGuaranteeRule
	{
		public CusGuaranteeRule(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static new readonly CusGuaranteeRuleTypeDecider TypeDecider = new CusGuaranteeRuleTypeDecider();

		protected override Customs.Business.AccessCodePinRuleValidation AccessCodePinRuleValidation => new AccessCodePinRuleValidation(this);

		protected override CusPermitRuleLookups GetNewLookups() => new CusGuaranteeRuleLookups(this);

		protected override Customs.Business.CusGuaranteeRuleValidation GuaranteeRuleValidation => new CusGuaranteeRuleValidation(this);

		protected override bool CPR_ValueTo_ReadOnly => base.CPR_ValueTo_ReadOnly
			|| CPR_RuleCode.ToString() is PermitRuleCodeList.Codes.LAP or PermitRuleCodeList.Codes.TSP or PermitRuleCodeList.Codes.CUS or PermitRuleCodeList.Codes.PCP or PermitRuleCodeList.Codes.PCV or PermitRuleCodeList.Codes.PCD;
	}
}

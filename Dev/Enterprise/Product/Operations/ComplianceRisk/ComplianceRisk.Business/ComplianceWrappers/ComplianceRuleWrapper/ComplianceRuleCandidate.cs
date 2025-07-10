using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ComplianceRisk.Business
{
	public class ComplianceRuleCandidate : NonPersistentBusinessObject
	{
		public ComplianceRuleCandidate(ComplianceRule complianceRule)
			: base(complianceRule.Factory)
		{
			this.complianceRule = complianceRule;
		}

		readonly ComplianceRule complianceRule;

		public ZString Origin => complianceRule.CRU_Origin;
		public ZString Destination => complianceRule.CRU_Destination;
		public ZString HarmonizedCode => complianceRule.CRU_HarmonizedCode;
		public ZString RiskStatus => complianceRule.CRU_RiskStatus;
	}
}

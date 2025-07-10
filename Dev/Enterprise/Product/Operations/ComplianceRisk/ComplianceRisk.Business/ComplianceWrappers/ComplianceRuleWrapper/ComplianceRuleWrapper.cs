using CargoWise.EntityFramework;

namespace Enterprise.ComplianceRisk.Business
{
	public class ComplianceRuleWrapper : NonPersistentBusinessObject
	{
		public ComplianceRuleWrapper(ComplianceRuleCandidateCollection rules, ComplianceRuleTargetCountryCollection countries)
		{
			this.rules = rules;
			this.countries = countries;
		}

		readonly ComplianceRuleCandidateCollection rules;
		readonly ComplianceRuleTargetCountryCollection countries;

		public ComplianceRuleCandidateCollection Rules => rules;
		public ComplianceRuleTargetCountryCollection Countries => countries;
	}
}

using CargoWise.EntityFramework;
using Enterprise.ComplianceRisk.Business;

namespace Enterprise.ComplianceRisk.GUI
{
	public class ComplianceRuleBindingObject : NonPersistentBusinessObject
	{
		public ComplianceRuleBindingObject(ComplianceRuleCollection complianceRules)
		{
			ComplianceRules = complianceRules;
		}

		public ComplianceRuleCollection ComplianceRules { get; }
	}
}

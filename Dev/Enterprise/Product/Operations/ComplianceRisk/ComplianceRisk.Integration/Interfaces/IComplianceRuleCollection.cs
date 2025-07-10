using CargoWise.Types;

namespace Enterprise.ComplianceRisk.Integration
{
	public interface IComplianceRuleCollection
	{
		void LoadComplianceRules(ZString countryCode);

		void SetReadOnlyIncludingChildren(bool readOnly);
	}
}

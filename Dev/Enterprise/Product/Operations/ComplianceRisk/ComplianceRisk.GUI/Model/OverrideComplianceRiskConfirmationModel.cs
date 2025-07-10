using Enterprise.Registry.Business;

namespace Enterprise.ComplianceRisk.GUI
{
	public class OverrideComplianceRiskConfirmationModel : DpsMarkJobClearConfirmationModel
	{
		protected override RequireReasonForCLRWrapper RegistryValue => OrganisationsDataRegistry.Instance.ComplianceRiskOverrideDecisionReason.Value;
	}
}

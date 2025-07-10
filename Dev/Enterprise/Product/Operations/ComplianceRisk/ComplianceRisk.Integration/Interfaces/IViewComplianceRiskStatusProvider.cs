namespace Enterprise.ComplianceRisk.Integration
{
	public interface IViewComplianceRiskStatusProvider
	{
		IComplianceItemRiskStatusProvider GetProviderBusinessObject();
	}
}

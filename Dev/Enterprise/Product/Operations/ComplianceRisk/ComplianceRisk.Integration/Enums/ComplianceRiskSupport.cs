namespace Enterprise.ComplianceRisk.Integration
{
	public enum ComplianceRiskSupport
	{
		None = 0,
		SupportInitialization = 1,
		SupportSubCompliances = 1 << 1,
		FullySupported = SupportInitialization | SupportSubCompliances
	}
}

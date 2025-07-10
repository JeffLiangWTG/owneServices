using static Enterprise.ComplianceRisk.Integration.ComplianceRiskSupport;

namespace Enterprise.ComplianceRisk.Integration
{
	public static class ComplianceRiskSupportExtensions
	{
		public static bool IsSupportInitialization(this ComplianceRiskSupport support)
		{
			return (support & SupportInitialization) == SupportInitialization;
		}

		public static bool IsSupportSubCompliances(this ComplianceRiskSupport support)
		{
			return (support & SupportSubCompliances) == SupportSubCompliances;
		}
	}
}

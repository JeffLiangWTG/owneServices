using System;

namespace Enterprise.ComplianceRisk.Integration
{
	[AttributeUsage(AttributeTargets.Class)]
	public class ViewComplianceRiskStatusProviderAttribute : Attribute
	{
		public Type ProviderBusinessObjectType { get; set; }
	}
}

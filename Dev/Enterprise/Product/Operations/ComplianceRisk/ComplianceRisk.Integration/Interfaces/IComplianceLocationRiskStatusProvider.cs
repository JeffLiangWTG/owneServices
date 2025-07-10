using System.Collections.Generic;

namespace Enterprise.ComplianceRisk.Integration
{
	///<Summary>
	///This interface is to be implemented if the implementing object bears a location compliance risk
	///<value>Property <c>Locations</c> represents a collection of locations with the compliance risk</value>
	///</Summary>
	public interface IComplianceLocationRiskStatusProvider : IComplianceItemRiskStatusProvider
	{
		IEnumerable<IComplianceLocation> Locations { get; }
	}
}

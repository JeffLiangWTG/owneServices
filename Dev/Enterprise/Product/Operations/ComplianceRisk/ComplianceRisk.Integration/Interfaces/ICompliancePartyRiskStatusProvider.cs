using System.Collections.Generic;

namespace Enterprise.ComplianceRisk.Integration
{
	///<Summary>
	///This interface is to be implemented if the implementing object bears a party compliance risk
	///<value>Property <c>Parties</c> represents a collection of parties with the compliance risk</value>
	///</Summary>
	public interface ICompliancePartyRiskStatusProvider : IComplianceItemRiskStatusProvider
	{
		IEnumerable<IScreeningParty> Parties { get; }
	}
}

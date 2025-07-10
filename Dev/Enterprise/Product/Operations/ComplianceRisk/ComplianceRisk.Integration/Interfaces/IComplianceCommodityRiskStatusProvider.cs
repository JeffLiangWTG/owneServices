using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Security;

namespace Enterprise.ComplianceRisk.Integration
{
	///<Summary>
	///This interface is to be implemented if the implementing object bears a commodity compliance risk
	///<value>Property <c>Commodities</c> represents a collection of commodities with the compliance risk</value>
	///<value>Property <c>AssessmentPointPairInfo</c> represents the origin/destination locations' pair info</value>
	///<value>Property <c>EffectiveDate</c> represents the effective date of commodity compliance</value>
	///<value>Property <c>EditHarmonizedCodeSecurity</c> represents security checkpoint for editing harmonized code</value>
	///<value>Property <c>EditComplianceAssessmentSecurity</c> represents security checkpoint for editing compliance assessment</value>
	///<value>Property <c>AllowComplianceAssessmentSecurity</c> represents security checkpoint for allowing compliance assessment check</value>
	///<value>Property <c>DeclineComplianceAssessmentSecurity</c> represents security checkpoint for declining compliance assessment</value>
	///</Summary>
	public interface IComplianceCommodityRiskStatusProvider : IComplianceItemRiskStatusProvider
	{
		IEnumerable<IComplianceCommodity> Commodities { get; }

		ComplianceAssessmentPointPairInfo AssessmentPointPairInfo { get; }

		ZDateTime EffectiveDate { get; }

		ZBool IsEditingCommoditySupported { get; }

		CommodityRiskCalculateFactor RiskCalculateFactor { get; }

		SecurityCheckpoint EditHarmonizedCodeSecurity { get; }

		SecurityCheckpoint EditComplianceAssessmentSecurity { get; }

		SecurityCheckpoint AllowComplianceAssessmentSecurity { get; }

		SecurityCheckpoint DeclineComplianceAssessmentSecurity { get; }
	}
}

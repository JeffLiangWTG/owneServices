using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Security;

namespace Enterprise.ComplianceRisk.Integration
{
	///<Summary>
	///This interface should NOT be implemented directly. Use interfaces ICompliancePartyRiskStatusProvider, IComplianceLocationRiskStatusProvider and IComplianceCommodityRiskStatusProvider to implement respective risk factors
	///<value>Property <c>Factory</c> represents the business object factory for ORM operations</value>
	///<value>Property <c>ParentID</c> represents the PK of current implementing object</value>
	///<value>Property <c>ParentTableCode</c> represents the table code prefix of current implementing object</value>
	///<value>Property <c>ComplianceRiskSupport</c> represents the compliance risk support enum flag</value>
	///<value>Func <c>InitializeComplianceWorkflowPopupIfNeeded</c> represents the function that is  executed when job document is delivered</value>
	///<value>Property <c>SubComplianceRiskStatusProviders</c> represents the internal nested compliance risk status providers</value>
	///<value>Property <c>ParentComplianceRiskStatusProviders</c> represents the parent nested compliance risk status providers</value>
	///<value>Property <c>IsEnabledComplianceWise</c> represents the compliance risk check module registry setting</value>
	///<value>Property <c>AllowOverrideOverallRiskStatusSecurity</c> represents security checkpoint for allowing overriding overall risk status</value>
	///<value>Property <c>AllowResynchronizeRiskStatusSecurity</c> represents security checkpoint for allowing resynchronizing risk status</value>
	///<value>Property <c>AllowOverrideFreightMovementRestrictions</c> represents security checkpoint for allowing overriding freight movement restrictions</value>
	///</Summary>
	public interface IComplianceItemRiskStatusProvider
	{
		BusinessObjectFactory Factory { get; }

		ZGuid ParentID { get; }

		ZString ParentTableCode { get; }

		ComplianceRiskSupport ComplianceRiskSupport { get; }

		(ZBool IsCurrent, ZDateTime JobEndDate) JobTime { get; }

		Func<DocumentDeliveryResultForComplianceWorkflow> InitializeComplianceWorkflowPopupIfNeeded { get; set; }

		IEnumerable<IComplianceItemRiskStatusProvider> SubComplianceRiskStatusProviders { get; }

		IEnumerable<IComplianceItemRiskStatusProvider> ParentComplianceRiskStatusProviders { get; }

		ZBool IsEnabledComplianceWise { get; }

		SecurityCheckpoint AllowOverrideOverallRiskStatusSecurity { get; }

		SecurityCheckpoint AllowResynchronizeRiskStatusSecurity { get; }

		SecurityCheckpoint AllowOverrideFreightMovementRestrictionsSecurity { get; }
	}
}

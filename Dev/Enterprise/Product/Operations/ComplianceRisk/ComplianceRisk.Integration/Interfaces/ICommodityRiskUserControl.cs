using System;

namespace Enterprise.ComplianceRisk.Integration
{
	public interface ICommodityRiskUserControl
	{
		Func<bool> ValidateSecurityEditHarmonizedCode { get; set; }

		Func<bool> ValidateSecurityEditComplianceAssessment { get; set; }

		void AddOrRemoveComplianceAlertsForExportDeclaration(bool needImportAlerts);
	}
}

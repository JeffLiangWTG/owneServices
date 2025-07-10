using System;

namespace Enterprise.ComplianceRisk.Integration
{
	public interface IComplianceBorderWiseProvider
	{
		void LaunchBorderWiseWebsite(Guid requestId, string focusedCommodity);
	}
}

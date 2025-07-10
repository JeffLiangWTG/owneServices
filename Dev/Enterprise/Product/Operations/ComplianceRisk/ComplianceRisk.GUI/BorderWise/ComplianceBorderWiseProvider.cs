using System;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.GUI;

namespace Enterprise.ComplianceRisk.GUI
{
	public class ComplianceBorderWiseProvider : IComplianceBorderWiseProvider
	{
		public ComplianceBorderWiseProvider()
		{
		}

		public void LaunchBorderWiseWebsite(Guid requestId, string focusedCommodity)
		{
			new BorderWiseLauncher().LaunchExternalApplication(new BorderWiseFilters { RequestId = requestId.ToString(), AddCountryParameter = false, FocusedCommodity = focusedCommodity });
		}
	}
}

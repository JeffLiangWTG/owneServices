using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ComplianceRisk.GUI.Test
{
	public class ComplianceBorderWiseProviderTest : TestCaseWithFactory
	{
		public void TestLaunchBorderWiseWeb()
		{
			var guid = Guid.NewGuid();
			var borderWise = new ComplianceBorderWiseProvider();
			borderWise.LaunchBorderWiseWebsite(guid, "1234.56 78");

			AssertEquals("Border Wise URL should launched:", FormattableString.Invariant($"https://app.borderwise.com?requestId={guid}&focusedCommodity=1234.56+78"), WebUrlLauncher.LastUrlLaunched);
		}
	}
}

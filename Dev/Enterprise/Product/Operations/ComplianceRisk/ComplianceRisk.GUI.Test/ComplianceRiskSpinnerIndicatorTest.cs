using System.Linq;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.ComplianceRisk.GUI.Test
{
	[TestedType(typeof(ComplianceRiskSpinnerIndicator))]
	class ComplianceRiskSpinnerIndicatorTest : ComplianceRiskHelperTest
	{
		public void TestUserControls()
		{
			using var userControl = new ComplianceRiskSpinnerIndicator();
			AssertType<ZLabel>(userControl.Controls.Find("SpinnerLabel", searchAllChildren: true).Single());
			AssertType<ZPictureBox>(userControl.Controls.Find("SpinnerIcon", searchAllChildren: true).Single());
		}
	}
}

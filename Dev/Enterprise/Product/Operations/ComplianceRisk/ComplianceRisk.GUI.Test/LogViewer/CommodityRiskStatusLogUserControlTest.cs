using System.Linq;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ComplianceRisk.GUI.Test
{
	public class CommodityRiskStatusLogUserControlTest : ComplianceRiskHelperTest
	{
		public void TestControls()
		{
			using var control = new CommodityRiskStatusLogUserControl();
			AssertType<ZTextBox>(control.Controls.Find("CommodityRiskStatus", searchAllChildren: true).Single());
			AssertType<ZTextBox>(control.Controls.Find("AssessmentNotesTextBox", searchAllChildren: true).Single());
			AssertType<ZGroupBox>(control.Controls.Find("AssessmentNotesGroupBox", searchAllChildren: true).Single());
		}
	}
}

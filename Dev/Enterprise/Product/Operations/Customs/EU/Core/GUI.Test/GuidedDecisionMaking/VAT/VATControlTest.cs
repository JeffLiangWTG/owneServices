using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class VATControlTest : TestCaseWithFactory
	{
		public void TestPopulateVATDetailControls()
		{
			var gdmBasic = VATContainerPanelTest.SetUpGDMBasicWithVATForTest(Factory);
			var vats = gdmBasic.VATApplicabilities.Cast<GuidedDecisionMakingVAT>();
			using (var control = new VATControl(vats, new ZString("XXX")))
			{
				var vatDetailControls = control.FindAll<VATDetailControl>().OrderBy(c => c.Top).ToArray();
				AssertEquals("VATDetailControls should be generated for all provided VATs.", 3, vatDetailControls.Length);

				AssertEquals("VATGroupBox.Text should be set as provided value.", "XXX", (control.Controls.Find("VATControlGroupBox", true).Single() as ZGroupBox).Text);
			}
		}

		public void TestVATShouldBeTickedExclusively()
		{
			var gdmBasic = VATContainerPanelTest.SetUpGDMBasicWithVATForTest(Factory);
			var vats = gdmBasic.VATApplicabilities.Cast<GuidedDecisionMakingVAT>();
			using (var control = new VATControl(vats, ZString.Empty))
			{
				var vatDetailControls = control.FindAll<VATDetailControl>().OrderBy(c => c.Top).ToArray();
				var radioButtons = vatDetailControls.Select(x => x.FindSingle<ZRadioButton>("VATDetailRadioButton")).ToArray();
				AssertArrayEqualsByElements("By default, no radio button is checked.", new[] { false, false, false }, radioButtons.Select(r => r.Checked).ToArray());

				radioButtons[0].PerformClick();
				AssertArrayEqualsByElements("First radio button can be checked.", new[] { true, false, false }, radioButtons.Select(r => r.Checked).ToArray());

				radioButtons[1].PerformClick();
				AssertArrayEqualsByElements("When second button is checked, checked first radio button should be unchecked.", new[] { false, true, false }, radioButtons.Select(r => r.Checked).ToArray());

				radioButtons[2].PerformClick();
				AssertArrayEqualsByElements("When third button is checked, checked second radio button should be unchecked.", new[] { false, false, true }, radioButtons.Select(r => r.Checked).ToArray());
			}
		}
	}
}

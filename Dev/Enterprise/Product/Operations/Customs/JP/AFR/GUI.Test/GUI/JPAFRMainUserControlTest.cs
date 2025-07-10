using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.AFR.GUI.Testing
{
	class JPAFRMainUserControlTest : TestCaseWithFactory
	{
		public void TestControlVisibilityForDifferentJobtype()
		{
			using (var testControl = new JPAFRMainUserControl())
			{
				testControl.UpdateCaption();
				var jPH_BillRegistrationStatusTextBox = testControl.Controls.Find("JPH_BillRegistrationStatusTextBox", true).FirstOrDefault() as ZTextBox;
				var jPH_OperationalCarrierVoyageNoTextBox = testControl.Controls.Find("JPH_OperationalCarrierVoyageNoTextBox", true).FirstOrDefault() as ZTextBox;
				var jPH_VesselDetailsChangedCheckBox = testControl.Controls.Find("JPH_VesselDetailsChangedCheckBox", true).FirstOrDefault() as ZCheckBox;
				AssertNotNull(jPH_BillRegistrationStatusTextBox);

				AssertEquals("Check Default Caption String", true, jPH_BillRegistrationStatusTextBox.CaptionResourceString.IsEmpty());
				testControl.UpdateControlLayout(false);
				AssertEquals(false, jPH_OperationalCarrierVoyageNoTextBox?.Visible);
				AssertEquals(true, jPH_VesselDetailsChangedCheckBox?.Visible);
				AssertEquals("Check Caption String for NVOCC", true, jPH_BillRegistrationStatusTextBox.CaptionResourceString.IsEmpty());
				testControl.UpdateControlLayout(true);
				AssertEquals(true, jPH_OperationalCarrierVoyageNoTextBox?.Visible);
				AssertEquals(false, jPH_VesselDetailsChangedCheckBox?.Visible);
				AssertEquals("Check Caption String for VOCC", "ATD Registration Status", jPH_BillRegistrationStatusTextBox.CaptionResourceString.Caption);
				AssertEquals("Check Caption String for VOCC", "ATD Registration", jPH_BillRegistrationStatusTextBox.CaptionResourceString.ShortCaption);
			}
		}
	}
}

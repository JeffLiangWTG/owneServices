using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.AFR.GUI.Testing
{
	class JPAFRVesselInformationUserControlTest : TestCaseWithFactory
	{
		public void TestControlVisibilityForDifferentJobtype()
		{
			using (var testControl = new JPAFRVesselInformationUserControl())
			{
				testControl.UpdateCaption();
				var jPH_VesselDetailsChangedCheckBox = testControl.Controls.Find("JPH_VesselDetailsChangedCheckBox", true).FirstOrDefault() as ZCheckBox;
				testControl.UpdateControlLayout(false);
				AssertEquals(true, jPH_VesselDetailsChangedCheckBox?.Visible);
				testControl.UpdateControlLayout(true);
				AssertEquals(false, jPH_VesselDetailsChangedCheckBox?.Visible);
			}
		}
	}
}

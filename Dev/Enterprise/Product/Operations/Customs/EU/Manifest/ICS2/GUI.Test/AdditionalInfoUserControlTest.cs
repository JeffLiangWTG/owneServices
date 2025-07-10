using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Manifest.ICS2.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI.Test
{
	class AdditionalInfoUserControlTest : TestCaseWithFactory
	{
		public void TestLayout()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();

			using (var form = new ZForm(manifestHeader))
			using (var control = new AdditionalInfoUserControl(ParentTabType.Header))
			{
				control.SetDataBinding(manifestHeader, string.Empty);

				form.Controls.Add(control);
				form.Show();

				EUICS2GUITestHelper.AssertGridLayout(control, "AdditionalInfoGrid", "CSI_Code", "CSI_Description", "AdditionalInfoDescription");
			}
		}
	}
}

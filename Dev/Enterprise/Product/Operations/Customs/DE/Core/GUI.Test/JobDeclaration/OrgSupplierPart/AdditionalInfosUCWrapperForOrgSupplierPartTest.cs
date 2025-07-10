using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.Testing
{
	sealed class AdditionalInfosUCWrapperForOrgSupplierPartTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using (var form = new ZForm())
			using (var control = new AdditionalInfosUCWrapperForOrgSupplierPart())
			{
				form.Controls.Add(control);
				form.Show();
				CombineAssertions(() =>
				{
					AssertEquals("ExportAdditionalInfosUserControl", expected: true, control.ExportAdditionalInfosUserControl.Visible);
					AssertEquals("ImportAdditionalInfosUserControl", expected: true, control.ImportAdditionalInfosUserControl.Visible);
					AssertNull("AdditionalInfosGrid", control.FindSingleOrDefault<ZGrid>("AdditionalInfosGrid", 0));
					AssertNull("AdditionalInfosPanel", control.FindSingleOrDefault<ZPanel>("AdditionalInfosPanel", 0));
				});
			}
		}
	}
}

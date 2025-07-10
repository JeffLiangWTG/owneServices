using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.Testing
{
	sealed class PreviousDocumentsUCWrapperForOrgSupplierPartTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using (var form = new ZForm())
			using (var control = new PreviousDocumentsUCWrapperForOrgSupplierPart())
			{
				form.Controls.Add(control);
				form.Show();
				CombineAssertions(() =>
				{
					AssertEquals("ExportAdditionalInfosUserControl", expected: true, control.ExportPreviousDocumentsUserControl.Visible);
					AssertEquals("ImportAdditionalInfosUserControl", expected: true, control.ImportPreviousDocumentsUserControl.Visible);
					AssertNull("TopPanel", control.FindSingleOrDefault<ZPanel>("TopPanel", 0));
					AssertNull("BottomPanel", control.FindSingleOrDefault<ZPanel>("BottomPanel", 0));
				});
			}
		}
	}
}

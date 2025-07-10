using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.Testing
{
	sealed class SupportingDocumentsUCWrapperForOrgSupplierPartTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using (var form = new ZForm())
			using (var control = new SupportingDocumentsUCWrapperForOrgSupplierPart())
			{
				form.Controls.Add(control);
				form.Show();
				CombineAssertions(() =>
				{
					AssertEquals("ExportSupportingDocumentsUserControl", expected: true, control.ExportSupportingDocumentsUserControl.Visible);
					AssertEquals("ImportSupportingDocumentsUserControl", expected: true, control.ImportSupportingDocumentsUserControl.Visible);
					AssertNull("SupportingDocumentsGrid", control.FindSingleOrDefault<ZGrid>("SupportingDocumentsGrid", 0));
					AssertNull("gridSplitter", control.FindSingleOrDefault<KSplitter>("gridSplitter", 0));
					AssertNull("BottomPanel", control.FindSingleOrDefault<ZPanel>("BottomPanel", 0));
				});
			}
		}
	}
}

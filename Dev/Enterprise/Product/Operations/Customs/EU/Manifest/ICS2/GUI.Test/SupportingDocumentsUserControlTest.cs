using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI.Test
{
	class SupportingDocumentsUserControlTest : TestCaseWithFactory
	{
		public void TestLayout()
		{
			var manifest = Factory.NewWithValidTestData<Business.AsycudaManifestHeader>();

			using (var form = new ZForm(manifest))
			using (var control = new SupportingDocumentsUserControl())
			{
				control.SetDataBinding(manifest, string.Empty);

				form.Controls.Add(control);
				form.Show();

				EUICS2GUITestHelper.AssertGridLayout(control, "SupportingDocumentsGrid", "CSI_Code", "CSI_ReferenceNumber", "DocumentDescription");
			}
		}
	}
}

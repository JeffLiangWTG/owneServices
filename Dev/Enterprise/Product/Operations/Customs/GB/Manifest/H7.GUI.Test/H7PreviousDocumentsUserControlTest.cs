using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.H7.GUI.Testing;
using Enterprise.Customs.GB.H7.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.H7.GUI.Testing
{
	sealed class H7PreviousDocumentsUserControlTest : TestCaseWithFactory
	{
		public void TestLayout()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();

			using (var form = new ZForm(manifest))
			using (var control = new H7PreviousDocumentsUserControl())
			{
				control.SetDataBinding(manifest, string.Empty);

				form.Controls.Add(control);
				form.Show();

				AssertEquals("Caption", "Previous Documents", control.AdditionalTabPageCaption.Caption);
				AssertEquals("Tab page sequence", 50, control.TabPageSequence);

				var grid = control.FindSingle<ZGrid>("AdditionalInfosGrid");

				EUH7GUITestHelper.AssertGridLayout(grid,
					[
						(CusSupportingInfo.Schema.CSI_Code, typeof(ZDropEditColumnStyleInfo)),
						(CusSupportingInfo.Schema.CSI_ReferenceNumber, typeof(ZTextBoxColumnStyleInfo)),
						("DocumentDescription", typeof(ZTextBoxColumnStyleInfo))
					]);

				var additionalInfosGroupBox = control.FindSingle<ZGroupBox>("AdditionalInfosGroupBox");
				AssertEquals(additionalInfosGroupBox.CaptionResourceString.Caption, "Previous Documents");

				var codeDropEdit = control.FindSingle<ZDropEdit>("CodeDropEdit");
				Assert("CodeDropEdit should be visible", codeDropEdit.Visible);

				var referenceTextBox = control.FindSingle<ZTextBox>("ReferenceTextBox");
				Assert("ReferenceTextBox should be visible", referenceTextBox.Visible);
			}
		}
	}
}

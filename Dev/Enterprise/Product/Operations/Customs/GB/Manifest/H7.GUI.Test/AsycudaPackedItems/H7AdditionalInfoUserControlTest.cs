using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.H7.GUI.Testing;
using Enterprise.Customs.GB.H7.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.H7.GUI.Testing
{
	sealed class H7AdditionalInfoUserControlTest : TestCaseWithFactory
	{
		public void TestLayout()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();

			using (var form = new ZForm(packedItem))
			using (var control = new H7AdditionalInfoUserControl())
			{
				control.SetDataBinding(packedItem, string.Empty);

				form.Controls.Add(control);
				form.Show();

				var grid = control.FindSingle<ZGrid>("AdditionalInfosGrid");

				EUH7GUITestHelper.AssertGridLayout(grid,
						[
							(CusSupportingInfo.Schema.CSI_Code, typeof(ZDropEditColumnStyleInfo)),
							(CusSupportingInfo.Schema.CSI_Description, typeof(ZTextBoxColumnStyleInfo))
						]);

				var additionalInfosGroupBox = control.FindSingle<ZGroupBox>("AdditionalInfosGroupBox");
				AssertEquals(additionalInfosGroupBox.CaptionResourceString.Caption, "Additional Info");

				var codeDropEdit = control.FindSingle<ZDropEdit>("AddInfoTypeCodeDropEdit");
				Assert("CodeDropEdit should be visible", codeDropEdit.Visible);

				var referenceTextBox = control.FindSingle<ZTextBox>("AddiInfoDescriptionTextBox");
				Assert("ReferenceTextBox should be visible", referenceTextBox.Visible);
			}
		}
	}
}

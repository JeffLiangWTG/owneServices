using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.H7.GUI.Testing
{
	sealed class AdditionalInfoUserControlTest : TestCaseWithFactory
	{
		public void TestLayout()
		{
			var bill = Factory.NewWithValidTestData<AsycudaBill>();
			var supportingDocument = bill.SupportingDocuments.AddNew();

			using (var form = new ZForm(bill))
			using (var control = new AdditionalInfoUserControl())
			{
				control.SetDataBinding(bill, string.Empty);

				form.Controls.Add(control);
				form.Show();

				var grid = control.FindSingle<ZGrid>("AdditionalInfosGrid");
				var detailsLayout = control.FindSingle<DynamicLayoutPanel>("DetailsPanel");

				CombineAssertions(() =>
				{
					Assert("Grid should be visible", grid.Visible);
					Assert("Details panel should be visible", detailsLayout.Visible);
					EUH7GUITestHelper.AssertGridLayout(grid, [CusSupportingInfo.Schema.CSI_Code, CusSupportingInfo.Schema.CSI_Description]);
				});
			}
		}
	}
}

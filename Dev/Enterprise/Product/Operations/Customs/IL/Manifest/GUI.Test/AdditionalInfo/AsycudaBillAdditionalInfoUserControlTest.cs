using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.Manifest.GUI.Testing
{
	sealed class AsycudaAdditionalInfoUserControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using (var control = new AsycudaAdditionalInfoUserControl())
			{
				var additionalInfosPanel = control.FindSingle<ZPanel>("additionalInfosPanel");
				AssertNotNull("additionalInfosPanel not null", additionalInfosPanel);
				AssertEquals("additionalInfosPanel Controls Count", 1, additionalInfosPanel.Controls.Count);

				var additionalInfosGrid = additionalInfosPanel.Controls[0];
				AssertNotNull("additionalInfosGrid not null", additionalInfosGrid);
			}
		}

		public void TestGridColumns()
		{
			using (var control = new AsycudaAdditionalInfoUserControl())
			{
				var additionalInfosGrid = (ZGrid)control.Controls.Find("additionalInfosGrid", true).First();
				var columns = additionalInfosGrid.ColumnStyles.Cast<ZGridColumnInfo>();
				CombineAssertions(() =>
				{
					AssertContainsExactElementsInExactOrder("additionalInfosGrid Column Names",
						new[]
						{
						"CSI_Code",
						"CSI_ReferenceNumber",
						"CSI_Description",
						},
						columns.Select(x => x.ColumnName));

					AssertContainsExactElementsInExactOrder("additionalInfosGrid Column widths",
						new[]
						{
						120,
						150,
						180,
						},
						columns.Select(x => x.Width));

					AssertType<ZDropEditColumnStyleInfo>(additionalInfosGrid.GetColumnStyle("CSI_Code"));
					AssertType<ZMultiControlColumnStyleInfo>(additionalInfosGrid.GetColumnStyle("CSI_ReferenceNumber"));
					AssertType<ZMultiControlColumnStyleInfo>(additionalInfosGrid.GetColumnStyle("CSI_Description"));
				});
			}
		}

		public void TestIAdditionalTabPage()
		{
			using (var control = new AsycudaAdditionalInfoUserControl())
			{
				var additionalTabPage = control as IAdditionalTabPage;

				CombineAssertions(() =>
				{
					AssertEquals("Additional Info", additionalTabPage.AdditionalTabPageCaption.Caption);
					AssertEquals(1, additionalTabPage.TabPageSequence);
					AssertEquals(true, additionalTabPage.AdditionalControlVisibility.isVisible(null));
					AssertEquals(control, additionalTabPage.AdditionalTabPageUserControl);
				});
			}
		}

		public void TestISupportingInfoUserControls()
		{
			using (var control = new AsycudaAdditionalInfoUserControl())
			{
				var additionalTabPage = control as ISupportingInfoUserControls;

				CombineAssertions(() =>
				{
					AssertEquals("Binding member should be as expected", "AdditionalInfos", additionalTabPage.GridBindingMember);
					AssertNotNull("Grid should be returned", additionalTabPage.Grid);
				});
			}
		}
	}
}

using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class HouseConsignmentPreviousDocumentsTabUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(CommonPreviousDocumentCollection<CommonPreviousDocument>), userControl.BindingSource.DataSourceType);
		}

		public void TestControls()
		{
			CombineAssertions(() =>
			{
				var splitContainer = userControl.PreviousDocumentsSplitContainer;
				AssertEquals("PreviousDocumentsSplitContainer.Dock", DockStyle.Fill, splitContainer.Dock);
				AssertEquals("PreviousDocumentsSplitContainer.Orientation", Orientation.Horizontal, splitContainer.Orientation);
				AssertEquals("PreviousDocumentDynamicLayoutPanel within PreviousDocumentsSplitContainer.Panel2", true, splitContainer.Panel2.Contains(userControl.PreviousDocumentDynamicLayoutPanel));
			});
		}

		public void TestSplitterPanelMinSize() => CombineAssertions(() =>
		{
			AssertEquals("Grid min size", 75, userControl.PreviousDocumentsSplitContainer.Panel1MinSize);
			AssertEquals("Details min size", 75, userControl.PreviousDocumentsSplitContainer.Panel2MinSize);
			AssertEquals("Default Details min size", 75, userControl.PreviousDocumentsSplitContainer.Panel2.Height);
		});

		public void TestHouseConsignmentPreviousDocumentsGridUserControl()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			userControl.SetDataBinding(nctsHeader, "");

			var grid = userControl.PreviousDocumentsSplitContainer.Panel1.FindSingle<HouseConsignmentPreviousDocumentsGridUserControl>();
			CombineAssertions(() =>
			{
				AssertEquals("Dock", DockStyle.Fill, grid.Dock);
				AssertEquals("BindingMember", ".", grid.GetBindingMember());
			});
		}

		public void TestPreviousDocumentDynamicLayoutPanel()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			userControl.SetDataBinding(nctsHeader, "");

			var dynamicLayoutPanel = userControl.PreviousDocumentDynamicLayoutPanel;
			CombineAssertions(() =>
			{
				AssertEquals("Dock", DockStyle.Fill, dynamicLayoutPanel.Dock);
				AssertEquals("AutoScroll", true, dynamicLayoutPanel.AutoScroll);

				DynamicLayoutPanelTest.AssertControlsOrder(dynamicLayoutPanel,
					nameof(PreviousDocumentControlBag.TypeCodeFindBox),
					nameof(PreviousDocumentControlBag.ReferenceNumberTextBox),
					nameof(PreviousDocumentControlBag.ComplementTextBox));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new HouseConsignmentPreviousDocumentsTabUserControl();
		}
		HouseConsignmentPreviousDocumentsTabUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}

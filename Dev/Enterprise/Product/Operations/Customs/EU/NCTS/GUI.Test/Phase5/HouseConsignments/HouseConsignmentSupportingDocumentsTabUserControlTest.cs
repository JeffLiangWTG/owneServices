using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class HouseConsignmentSupportingDocumentsTabUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(NctsSupportingDocumentCollection<NctsSupportingDocument>), userControl.BindingSource.DataSourceType);
		}

		public void TestControls()
		{
			CombineAssertions(() =>
			{
				var splitContainer = userControl.HouseConsignmentSupportingDocumentsSplitContainer;
				AssertEquals("HouseConsignmentSupportingDocumentsSplitContainer.Orientation", Orientation.Horizontal, splitContainer.Orientation);
				AssertEquals("HouseConsignmentSupportingDocumentsDynamicLayoutPanel in Panel2", true, splitContainer.Panel2.Contains(userControl.HouseConsignmentSupportingDocumentsDynamicLayoutPanel));
			});
		}

		public void TestSplitterPanelMinSize() => CombineAssertions(() =>
		{
			AssertEquals("Grid min size", 75, userControl.HouseConsignmentSupportingDocumentsSplitContainer.Panel1MinSize);
			AssertEquals("Details min size", 100, userControl.HouseConsignmentSupportingDocumentsSplitContainer.Panel2MinSize);
			AssertEquals("Default Details min size", 100, userControl.HouseConsignmentSupportingDocumentsSplitContainer.Panel2.Height);
		});

		public void TestHouseConsignmentSupportingDocumentsGridUserControl()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			userControl.SetDataBinding(nctsHeader, "");

			var grid = userControl.HouseConsignmentSupportingDocumentsSplitContainer.Panel1.FindSingle<HouseConsignmentSupportingDocumentsGridUserControl>();
			CombineAssertions(() =>
			{
				AssertEquals("Dock", DockStyle.Fill, grid.Dock);
				AssertEquals("BindingMember", ".", grid.GetBindingMember());
			});
		}

		public void TestHouseConsignmentSupportingDocumentsDynamicLayoutPanel()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			userControl.SetDataBinding(nctsHeader, "");

			var dynamicLayoutPanel = userControl.HouseConsignmentSupportingDocumentsDynamicLayoutPanel;
			CombineAssertions(() =>
			{
				AssertEquals("Dock", DockStyle.Fill, dynamicLayoutPanel.Dock);
				AssertEquals("AutoScroll", true, dynamicLayoutPanel.AutoScroll);

				DynamicLayoutPanelTest.AssertControlsOrder(dynamicLayoutPanel,
					nameof(SupportingDocumentControlBag.TypeCodeFindBox),
					nameof(SupportingDocumentControlBag.ReferenceNumberTextBox),
					nameof(SupportingDocumentControlBag.ItemNumberCalcEdit),
					nameof(SupportingDocumentControlBag.ComplementTextBox));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new HouseConsignmentSupportingDocumentsTabUserControl();
		}
		HouseConsignmentSupportingDocumentsTabUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}

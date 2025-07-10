using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class HouseConsignmentAdditionalDocumentsTabUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSource()
		{
			AssertEquals(typeof(NctsBillAdditionalDocumentCollection<NctsBillAdditionalDocument>), userControl.BindingSource.DataSourceType);
		}

		public void TestControls()
		{
			CombineAssertions(() =>
			{
				var splitContainer = userControl.AdditionalDocumentsSplitContainer;
				AssertEquals("AdditionalDocumentsSplitContainer.Dock", DockStyle.Fill, splitContainer.Dock);
				AssertEquals("AdditionalDocumentsSplitContainer.Orientation", Orientation.Horizontal, splitContainer.Orientation);
				AssertEquals("AdditionalDocumentsSplitContainer within AdditionalDocumentsSplitContainer.Panel2", true, splitContainer.Panel2.Contains(userControl.AdditionalDocumentDynamicLayoutPanel));
			});
		}

		public void TestSplitterPanelMinSize() => CombineAssertions(() =>
		{
			AssertEquals("Grid min size", 75, userControl.AdditionalDocumentsSplitContainer.Panel1MinSize);
			AssertEquals("Details min size", 100, userControl.AdditionalDocumentsSplitContainer.Panel2MinSize);
			AssertEquals("Default Details min size", 100, userControl.AdditionalDocumentsSplitContainer.Panel2.Height);
		});

		public void TestHouseConsignmentAdditionalDocumentsGridUserControl()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			userControl.SetDataBinding(nctsHeader, "");

			var grid = userControl.AdditionalDocumentsSplitContainer.Panel1.FindSingle<HouseConsignmentAdditionalDocumentGridUserControl>();
			CombineAssertions(() =>
			{
				AssertEquals("Dock", DockStyle.Fill, grid.Dock);
				AssertEquals("BindingMember", ".", grid.GetBindingMember());
			});
		}

		public void TestAdditionalDocumentDynamicLayoutPanel()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			userControl.SetDataBinding(nctsHeader, "");

			var dynamicLayoutPanel = userControl.AdditionalDocumentDynamicLayoutPanel;
			CombineAssertions(() =>
			{
				AssertEquals("Dock", DockStyle.Fill, dynamicLayoutPanel.Dock);
				AssertEquals("AutoScroll", true, dynamicLayoutPanel.AutoScroll);

				DynamicLayoutPanelTest.AssertControlsOrder(dynamicLayoutPanel,
					nameof(AdditionalDocumentControlBag.KindDropEdit),
					nameof(AdditionalDocumentControlBag.TypeCodeFindBox),
					nameof(AdditionalDocumentControlBag.ReferenceNumberTextBox),
					nameof(AdditionalDocumentControlBag.DescriptionTextBox));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new HouseConsignmentAdditionalDocumentsTabUserControl();
		}
		HouseConsignmentAdditionalDocumentsTabUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}

using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class NctsPackagePanelUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSource()
		{
			AssertEquals("NctsPackagePanelUserControl BindingSource DataSourceType", typeof(NctsPackage), userControl.BindingSource.DataSourceType);
		}

		public void TestPackagesSplitContainer()
		{
			var packagesSplitContainer = userControl.PackagesSplitContainer;
			CombineAssertions(() =>
			{
				AssertEquals("PackagesSplitContainer is within HouseConsignmentPackagesPanelUserControl", true, userControl.Controls.Contains(packagesSplitContainer));
				AssertEquals("Dock", System.Windows.Forms.DockStyle.Fill, packagesSplitContainer.Dock);
				AssertEquals("Panel2 Min Size", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(200), packagesSplitContainer.Panel2MinSize);
			});
		}

		public void TestPackagesDynamicLayoutPanel()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			userControl.SetDataBinding(nctsHeader, "");
			var packageDynamicLayoutPanel = userControl.PackagesDynamicLayoutPanel;
			CombineAssertions(() =>
			{
				AssertEquals("PackagesDynamicLayoutPanel is within PackagesSplitContainer.Panel2", true, userControl.PackagesSplitContainer.Panel2.Controls.Contains(packageDynamicLayoutPanel));
				AssertEquals("Dock", System.Windows.Forms.DockStyle.Fill, packageDynamicLayoutPanel.Dock);

				DynamicLayoutPanelTest.AssertControlsOrder(packageDynamicLayoutPanel,
						nameof(NctsPackageControlBag.SequenceNumberCalcEdit),
						nameof(NctsPackageControlBag.PlaceHolder1Label),
						nameof(NctsPackageControlBag.DeclaredValueLabel),
						nameof(NctsPackageControlBag.UnitTypeDropEdit),
						nameof(NctsPackageControlBag.UnitCountCalcEdit),
						nameof(NctsPackageControlBag.MarksAndNumbersTextBox));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new NctsPackagePanelUserControl();
		}
		NctsPackagePanelUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}

using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class GoodsItemPackagesUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(NctsCommonCargoDescCollection<NctsCommonCargoDesc>), control.BindingSource.DataSourceType);
		}

		public void TestPackagesGroupBox()
		{
			var packagesGroupBox = control.PackagesGroupBox;
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Packages", packagesGroupBox.CaptionResourceString.Caption);
				AssertEquals("Dock", System.Windows.Forms.DockStyle.Fill, packagesGroupBox.Dock);
			});
		}

		public void TestPackagesSplitContainer()
		{
			var packagesSplitContainer = control.PackagesSplitContainer;
			CombineAssertions(() =>
			{
				AssertEquals("PackagesSplitContainer is within PackagesGroupBox", true, control.PackagesGroupBox.Controls.Contains(packagesSplitContainer));
				AssertEquals("Dock", System.Windows.Forms.DockStyle.Fill, packagesSplitContainer.Dock);
			});
		}

		public void TestPackagesGrid()
		{
			var packagesGrid = control.PackagesGrid;
			CombineAssertions(() =>
			{
				AssertEquals("BindTo", "Packages", packagesGrid.BindTo);
				AssertEquals("Dock", System.Windows.Forms.DockStyle.Fill, packagesGrid.Dock);

				var unitTypeColumnInfo = packagesGrid.GetColumnStyle(nameof(NctsPackage.B5_UnitType));
				AssertType<ZDropEditColumnStyleInfo>("B5_UnitType column type", unitTypeColumnInfo);
				AssertEquals("B5_UnitType column width", 60, unitTypeColumnInfo.Width);
				AssertEquals("B5_UnitType character casing", System.Windows.Forms.CharacterCasing.Upper, unitTypeColumnInfo.CharacterCasing);

				var unitCountColumnInfo = packagesGrid.GetColumnStyle(nameof(NctsPackage.B5_UnitCount));
				AssertType<ZCalcEditColumnStyleInfo>("B5_UnitCount column type", unitCountColumnInfo);
				AssertEquals("B5_UnitCount column width", 80, unitCountColumnInfo.Width);

				var marksAndNumbersColumnInfo = packagesGrid.GetColumnStyle(nameof(NctsPackage.B5_MarksAndNumbers));
				AssertType<ZTextBoxColumnStyleInfo>("B5_MarksAndNumbers column type", marksAndNumbersColumnInfo);
				AssertEquals("B5_MarksAndNumbers column width", 200, marksAndNumbersColumnInfo.Width);
				AssertEquals("B5_MarksAndNumbers character casing", System.Windows.Forms.CharacterCasing.Normal, marksAndNumbersColumnInfo.CharacterCasing);

				AssertEquals("PackagesGrid is within PackagesSplitContainer.Panel1", true, control.PackagesSplitContainer.Panel1.Controls.Contains(packagesGrid));
			});
		}

		public void TestPackageDynamicLayoutPanel()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			control.SetDataBinding(nctsHeader, "");
			var packageDynamicLayoutPanel = control.PackageDynamicLayoutPanel;
			CombineAssertions(() =>
			{
				AssertEquals("PackageDynamicLayoutPanel is within PackagesSplitContainer.Panel2", true, control.PackagesSplitContainer.Panel2.Controls.Contains(packageDynamicLayoutPanel));
				AssertEquals("Dock", System.Windows.Forms.DockStyle.Fill, packageDynamicLayoutPanel.Dock);

				DynamicLayoutPanelTest.AssertControlsOrder(packageDynamicLayoutPanel,
						nameof(GoodsItemPackageControlBag.PackageTypeDropEdit),
						nameof(GoodsItemPackageControlBag.NumberOfPackagesCalcEdit),
						nameof(GoodsItemPackageControlBag.MarksAndNumbersTextBox));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new GoodsItemPackagesUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}

		GoodsItemPackagesUserControl control;
	}
}

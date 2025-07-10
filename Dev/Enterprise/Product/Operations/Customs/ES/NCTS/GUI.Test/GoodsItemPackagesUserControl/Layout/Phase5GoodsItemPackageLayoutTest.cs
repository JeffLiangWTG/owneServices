using System.Collections.Generic;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing.GoodsItemPackagesUserControl.Layout;
[TestedType(typeof(Phase5GoodsItemPackageLayout))]
sealed class Phase5GoodsItemPackageLayoutTest : LayoutsAbstractTest
{
	protected override int ControlBagCount => 1;

	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
		}
	}
	
	protected override ICommonLayoutBuilder CommonLayoutBuilder => new GoodsItemPackageLayoutBuilder<NctsPackage>();
	public PanelLayout Layout => layout ?? (layout = new Phase5GoodsItemPackageLayout().Layout);
	PanelLayout layout;

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (GoodsItemPackageControlBag.Instance.PackageTypeDropEdit, ControlWidthClass.Long);
			yield return (GoodsItemPackageControlBag.Instance.NumberOfPackagesCalcEdit, ControlWidthClass.Medium);
			yield return (GoodsItemPackageControlBag.Instance.MarksAndNumbersTextBox, ControlWidthClass.Long);
			yield return (GoodsItemPackageControlBag.Instance.PackageIDTextBox, ControlWidthClass.Long);
			yield return (GoodsItemPackageControlBag.Instance.BrandTextBox, ControlWidthClass.Long);
			yield return (GoodsItemPackageControlBag.Instance.ModelTextBox, ControlWidthClass.Long);
		}
	}

	public void TestVisibility_IsVehiclesFalse()
	{
		goodsItem.IsVehicles = false;
		var package = goodsItem.Packages.AddNew();
		CombineAssertions(() =>
		{
			AssertEquals("if IsVehicles True PackageTypeDropEdit visible", true, Layout.IsVisible(GoodsItemPackageControlBag.Instance.PackageTypeDropEdit, package));
			AssertEquals("if IsVehicles True NumberOfPackagesCalcEdit visible", true, Layout.IsVisible(GoodsItemPackageControlBag.Instance.NumberOfPackagesCalcEdit, package));
			AssertEquals("if IsVehicles True MarksAndNumbersTextBox visible", true, Layout.IsVisible(GoodsItemPackageControlBag.Instance.MarksAndNumbersTextBox, package));
			AssertEquals("if IsVehicles True PackageIDTextBox not visible", false, Layout.IsVisible(GoodsItemPackageControlBag.Instance.PackageIDTextBox, package));
			AssertEquals("if IsVehicles True BrandTextBox not visible", false, Layout.IsVisible(GoodsItemPackageControlBag.Instance.BrandTextBox, package));
			AssertEquals("if IsVehicles True ModelTextBox not visible", false, Layout.IsVisible(GoodsItemPackageControlBag.Instance.ModelTextBox, package));
		});
	}

	public void TestVisibility_IsVehiclesTrue()
	{
		goodsItem.IsVehicles = true;
		var package = goodsItem.Packages.AddNew();
		CombineAssertions(() =>
		{
			AssertEquals("if IsVehicles False PackageTypeDropEdit not visible", false, Layout.IsVisible(GoodsItemPackageControlBag.Instance.PackageTypeDropEdit, package));
			AssertEquals("if IsVehicles False NumberOfPackagesCalcEdit not visible", false, Layout.IsVisible(GoodsItemPackageControlBag.Instance.NumberOfPackagesCalcEdit, package));
			AssertEquals("if IsVehicles False MarksAndNumbersTextBox not visible", false, Layout.IsVisible(GoodsItemPackageControlBag.Instance.MarksAndNumbersTextBox, package));
			AssertEquals("if IsVehicles False PackageIDTextBox visible", true, Layout.IsVisible(GoodsItemPackageControlBag.Instance.PackageIDTextBox, package));
			AssertEquals("if IsVehicles False BrandTextBox visible", true, Layout.IsVisible(GoodsItemPackageControlBag.Instance.BrandTextBox, package));
			AssertEquals("if IsVehicles False ModelTextBox visible", true, Layout.IsVisible(GoodsItemPackageControlBag.Instance.ModelTextBox, package));
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		var bill = nctsHeader.Bills.AddNew();
		goodsItem = bill.GoodsItems.AddNew();
	}
	NctsDepartureCargoDesc goodsItem;
}

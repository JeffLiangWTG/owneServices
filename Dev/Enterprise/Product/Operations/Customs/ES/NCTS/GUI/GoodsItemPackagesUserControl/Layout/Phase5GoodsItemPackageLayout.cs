using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI;

public sealed class Phase5GoodsItemPackageLayout : IPanelLayoutProvider
{
	public PanelLayout Layout { get; }

	PanelLayout IPanelLayoutProvider.Layout => Layout;

	public Phase5GoodsItemPackageLayout()
	{
		Layout = CreateGoodsItemPackageLayout();
	}

	PanelLayout CreateGoodsItemPackageLayout()
	{
		GoodsItemPackageLayoutBuilder<NctsPackage> goodsItemPackageLayoutBuilder = new GoodsItemPackageLayoutBuilder<NctsPackage>();
		GoodsItemPackageControlBag commonBag = goodsItemPackageLayoutBuilder.CommonBag;
		goodsItemPackageLayoutBuilder.AddColumn();
		goodsItemPackageLayoutBuilder.Add(commonBag.PackageTypeDropEdit, ControlWidthClass.Long);
		goodsItemPackageLayoutBuilder.Add(commonBag.NumberOfPackagesCalcEdit, ControlWidthClass.Medium);
		goodsItemPackageLayoutBuilder.Add(commonBag.MarksAndNumbersTextBox, ControlWidthClass.Long);
		goodsItemPackageLayoutBuilder.Add(commonBag.PackageIDTextBox, ControlWidthClass.Long);
		goodsItemPackageLayoutBuilder.Add(commonBag.BrandTextBox, ControlWidthClass.Long);
		goodsItemPackageLayoutBuilder.Add(commonBag.ModelTextBox, ControlWidthClass.Long);

		goodsItemPackageLayoutBuilder.SetVisibility(commonBag.PackageTypeDropEdit, p => !((NctsDepartureCargoDesc)p.Parent).IsVehicles, p => ((NctsDepartureCargoDesc)p.Parent).IsVehiclesInfo);
		goodsItemPackageLayoutBuilder.SetVisibility(commonBag.NumberOfPackagesCalcEdit, p => !((NctsDepartureCargoDesc)p.Parent).IsVehicles, p => ((NctsDepartureCargoDesc)p.Parent).IsVehiclesInfo);
		goodsItemPackageLayoutBuilder.SetVisibility(commonBag.MarksAndNumbersTextBox, p => !((NctsDepartureCargoDesc)p.Parent).IsVehicles, p => ((NctsDepartureCargoDesc)p.Parent).IsVehiclesInfo);
		goodsItemPackageLayoutBuilder.SetVisibility(commonBag.PackageIDTextBox, p => ((NctsDepartureCargoDesc)p.Parent).IsVehicles, p => ((NctsDepartureCargoDesc)p.Parent).IsVehiclesInfo);
		goodsItemPackageLayoutBuilder.SetVisibility(commonBag.BrandTextBox, p => ((NctsDepartureCargoDesc)p.Parent).IsVehicles, p => ((NctsDepartureCargoDesc)p.Parent).IsVehiclesInfo);
		goodsItemPackageLayoutBuilder.SetVisibility(commonBag.ModelTextBox, p => ((NctsDepartureCargoDesc)p.Parent).IsVehicles, p => ((NctsDepartureCargoDesc)p.Parent).IsVehiclesInfo);

		return goodsItemPackageLayoutBuilder.Build();
	}
}

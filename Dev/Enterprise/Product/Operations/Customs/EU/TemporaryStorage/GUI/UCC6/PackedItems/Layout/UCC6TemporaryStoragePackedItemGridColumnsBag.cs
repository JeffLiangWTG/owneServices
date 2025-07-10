using System;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI;

public sealed class UCC6TemporaryStoragePackedItemGridColumnsBag
{
	public static UCC6TemporaryStoragePackedItemGridColumnsBag Instance => instance ??= new();

	[ThreadStatic]
	static UCC6TemporaryStoragePackedItemGridColumnsBag instance;

	public UCC6TemporaryStoragePackedItemGridColumnsBag()
	{
		var width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX;
		var grossWeightGroupName = Res.GetData("9A587CD6-CCFD-4320-B76E-C831CFBE3771", "Gross Weight");
		var netWeightGroupName = Res.GetData("FB4CA7DD-00A8-4A19-9139-9F0D54AB8F86", "Net Weight");
		var customsQty2GroupName = Res.GetData("0030D175-E194-4771-8DFF-F33976DF617B", "Sup. Qty", "Supplementary Qty", "Supplementary Quantity");

		LineNoCalcEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(nameof(TemporaryStoragePackedItem.API_LineNo), width(80), c =>
		{
			c.BindToDecimalPlaces = null;
			c.DefaultCollectionIndex = 0;
		});
		FormattedTariffColumn = new GridColumnReference<Universal.GUI.TariffColumnStyleInfo>(nameof(TemporaryStoragePackedItem.API_FormattedTariff), width(80), c =>
		{
			c.DefaultCollectionIndex = 0;
			c.SelectNomenclatureModes = null;
			c.ShowDescriptionFilterOnNonNomenclatureTariffModule = false;
			c.TariffType = null;
		});
		GoodsDescriptionTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(nameof(TemporaryStoragePackedItem.API_GoodsDescription), width(120), c =>
		{
			c.DefaultCollectionIndex = 0;
		});
		GrossWeightCalcEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(nameof(TemporaryStoragePackedItem.API_GrossWeight), width(80), c =>
		{
			c.BindToDecimalPlaces = null;
			c.DefaultCollectionIndex = 0;
			c.GroupName = grossWeightGroupName;
		});
		GrossWeightUnitDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(nameof(TemporaryStoragePackedItem.API_GrossWeightUQ), width(80), c =>
		{
			c.DefaultCollectionIndex = 0;
			c.GroupName = grossWeightGroupName;
		});
		NetWeightCalcEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(nameof(TemporaryStoragePackedItem.API_NetWeight), width(80), c =>
		{
			c.BindToDecimalPlaces = null;
			c.DefaultCollectionIndex = 0;
			c.GroupName = netWeightGroupName;
		});
		NetWeightUnitDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(nameof(TemporaryStoragePackedItem.API_NetWeightUQ), width(80), c =>
		{
			c.DefaultCollectionIndex = 0;
			c.GroupName = netWeightGroupName;
		});
		CustomsQty2CalcEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(nameof(TemporaryStoragePackedItem.API_CustomsQty2), width(80), c =>
		{
			c.BindToDecimalPlaces = null;
			c.DefaultCollectionIndex = 0;
			c.GroupName = customsQty2GroupName;
		});
		CustomsUnit2DropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(nameof(TemporaryStoragePackedItem.API_CustomsUQ2), width(80), c =>
		{
			c.DefaultCollectionIndex = 0;
			c.GroupName = customsQty2GroupName;
		});
		ChemicalSubstanceCodeFindBoxColumn = new GridColumnReference<ZCodeFindBoxColumnStyleInfo>(nameof(TemporaryStoragePackedItem.API_ChemicalSubstanceCode), width(40), c =>
		{
			c.DefaultCollectionIndex = 0;
		});
	}

	public IGridColumnReference LineNoCalcEditColumn { get; }

	public IGridColumnReference FormattedTariffColumn { get; }

	public IGridColumnReference GoodsDescriptionTextBoxColumn { get; }

	public IGridColumnReference GrossWeightCalcEditColumn { get; }

	public IGridColumnReference GrossWeightUnitDropEditColumn { get; }

	public IGridColumnReference NetWeightCalcEditColumn { get; }

	public IGridColumnReference NetWeightUnitDropEditColumn { get; }

	public IGridColumnReference CustomsQty2CalcEditColumn { get; }

	public IGridColumnReference CustomsUnit2DropEditColumn { get; }

	public IGridColumnReference ChemicalSubstanceCodeFindBoxColumn { get; }
}

using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class Phase5GoodsItemDifferencesDetailsGridColumnsBag
	{
		public Phase5GoodsItemDifferencesDetailsGridColumnsBag()
		{
			LineNoTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(NctsDepartureCargoDesc.Schema.BY_LineNo, 80);
			DeclarationGoodsItemNumberTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(NctsDepartureCargoDesc.Schema.BY_DeclarationGoodsItemNumber, 80);
			UnloadedStateDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(NctsDepartureCargoDesc.Schema.BY_UnloadedState, 80);
			CusC4NumberCodeFindBoxColumn = new GridColumnReference<ZCodeFindBoxColumnStyleInfo>(NctsDepartureCargoDesc.Schema.BY_CusC4Number, 80);
			DescriptionTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(NctsDepartureCargoDesc.Schema.BY_Description, 240);
			GrossWeightCalcEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(NctsDepartureCargoDesc.Schema.BY_GrossWeight, 80, c => c.BindToDecimalPlaces = null);
			GrossWeightUnitDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(NctsDepartureCargoDesc.Schema.BY_GrossWeightUnit, 80);
			NetWeightCalcEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(NctsDepartureCargoDesc.Schema.BY_NetWeight, 80, c => c.BindToDecimalPlaces = null);
			NetWeightUnitDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(NctsDepartureCargoDesc.Schema.BY_NetWeightUnit, 80);
			FormattedHarmonisedTariffColumn = new GridColumnReference<Universal.GUI.TariffColumnStyleInfo>(NctsDepartureCargoDesc.Schema.BY_FormattedHarmonisedTariff, 150, c =>
			{
				c.SelectNomenclatureModes = null;
				c.ShowDescriptionFilterOnNonNomenclatureTariffModule = false;
				c.TariffType = null;
			});
		}

		public static Phase5GoodsItemDifferencesDetailsGridColumnsBag Instance => instance ??= new Phase5GoodsItemDifferencesDetailsGridColumnsBag();

		public IGridColumnReference LineNoTextBoxColumn { get; }

		public IGridColumnReference DeclarationGoodsItemNumberTextBoxColumn { get; }

		public IGridColumnReference UnloadedStateDropEditColumn { get; }

		public IGridColumnReference CusC4NumberCodeFindBoxColumn { get; }

		public IGridColumnReference DescriptionTextBoxColumn { get; }

		public IGridColumnReference GrossWeightCalcEditColumn { get; }

		public IGridColumnReference GrossWeightUnitDropEditColumn { get; }

		public IGridColumnReference NetWeightCalcEditColumn { get; }

		public IGridColumnReference NetWeightUnitDropEditColumn { get; }

		public IGridColumnReference FormattedHarmonisedTariffColumn { get; }

		[ThreadStatic]
		static Phase5GoodsItemDifferencesDetailsGridColumnsBag instance;
	}
}

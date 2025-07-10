using System;
using Enterprise.Customs.EU.Intrastat.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Intrastat.GUI
{
	sealed class TransactionLinesGridColumnsBag
	{
		TransactionLinesGridColumnsBag()
		{
			DescriptionTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(AutoCusIntrastatLine.Schema.CIL_DescriptionOfGoods, 385);
			TariffColumn = new GridColumnReference<Universal.GUI.TariffColumnStyleInfo>(CusIntrastatLine.Schema.CIL_FormattedTariff, 90, c =>
			{
				c.TariffType = Universal.Constants.TariffTypes.Export;
			});
			InvoiceValueCalcEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(AutoCusIntrastatLine.Schema.CIL_InvoiceValue, 135, c => c.BindToDecimalPlaces = null);
			StatisticalValueCalcEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(AutoCusIntrastatLine.Schema.CIL_StatisticalValue, 135, c => c.BindToDecimalPlaces = null);
			CurrencyDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(AutoCusIntrastatLine.Schema.CIL_RX_NKCurrency, 70);
			MassInKilogramsCalcEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(AutoCusIntrastatLine.Schema.CIL_MassInKilograms, 95, c => c.BindToDecimalPlaces = null);
			MassInKilogramsUnitDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(CusIntrastatLine.Schema.CIL_MassInKilogramsUnit, 70);
			SupplementaryQuantityCalcEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(AutoCusIntrastatLine.Schema.CIL_SupplementaryQuantity, 95, c => c.BindToDecimalPlaces = null);
			SupplementaryQuantityUnit = new GridColumnReference<ZDropEditColumnStyleInfo>(AutoCusIntrastatLine.Schema.CIL_SupplementaryQuantityUnit, 70);
			CountryOfOriginDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(AutoCusIntrastatLine.Schema.CIL_RN_NKCountryOfOrigin, 70);
			RegionDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(AutoCusIntrastatLine.Schema.CIL_Region, 70);
		}

		public static TransactionLinesGridColumnsBag Instance => instance ??= new TransactionLinesGridColumnsBag();
		[ThreadStatic]
		static TransactionLinesGridColumnsBag instance;

		public IGridColumnReference DescriptionTextBoxColumn { get; }
		public IGridColumnReference TariffColumn { get; }
		public IGridColumnReference InvoiceValueCalcEditColumn { get; }
		public IGridColumnReference StatisticalValueCalcEditColumn { get; }
		public IGridColumnReference CurrencyDropEditColumn { get; }
		public IGridColumnReference MassInKilogramsCalcEditColumn { get; }
		public IGridColumnReference MassInKilogramsUnitDropEditColumn { get; }
		public IGridColumnReference SupplementaryQuantityCalcEditColumn { get; }
		public IGridColumnReference SupplementaryQuantityUnit { get; }
		public IGridColumnReference CountryOfOriginDropEditColumn { get; }
		public IGridColumnReference RegionDropEditColumn { get; }
	}
}

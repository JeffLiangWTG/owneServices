using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed partial class Phase5DepartureGoodsItemsGridUserControl : ZUserControl
	{
		public Phase5DepartureGoodsItemsGridUserControl()
		{
			InitializeComponent();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			UpdateGridColumnLayout();
			InitializeTariffColumnTariffType();
		}

		new NctsHeader DataSource => base.DataSource as NctsHeader;

		void UpdateGridColumnLayout()
		{
			var header = DataSource;
			var layoutProvider = NctsPhase5LayoutProvider.GetLayoutProvider(header?.DefaultDataGroupingCode);
			var gridColumnLayout = header != null && header.IsInPhase5TransitionPeriod && layoutProvider is INctsPhase5TransitionPeriodLayoutProvider transitionPeriodLayoutProvider
				? transitionPeriodLayoutProvider.GetDepartureGoodsItemsGridColumnLayout()
				: layoutProvider.GetDepartureGoodsItemsGridColumnLayout();
			GoodsItemsGrid.ApplyGridColumnLayout(gridColumnLayout);
		}

		void InitializeTariffColumnTariffType()
		{
			var columnStyle = (Universal.GUI.TariffColumnStyleInfo)GoodsItemsGrid.GetColumnStyle(nameof(NctsCommonCargoDesc.BY_FormattedHarmonisedTariff));
			if (columnStyle != null)
			{
				columnStyle.GetEffectiveDate = () => CurrentGoodsItem()?.ValuationDate ?? ZDateTime.Today;
				columnStyle.GetTariffType = () => CurrentGoodsItem()?.TariffType ?? Universal.Constants.TariffTypes.Import;
				columnStyle.GetDataGrouping = () => CurrentGoodsItem()?.DataGroupingCode ?? ZString.Empty;
				columnStyle.GetSelectNomenclatureModes = () => CurrentGoodsItem()?.GetTariffNomenclatureSelectionModes().ToList();

				NctsDepartureCargoDesc CurrentGoodsItem() => CurrentDataItem is NctsDepartureCargoDesc { IsDeleted: false } goodsItem ? goodsItem : null;
			}
		}
	}
}

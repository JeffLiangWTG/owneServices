using System;
using System.Linq;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.NCTS.GUI;

sealed partial class Phase5DepartureGoodsItemsGridUserControl : ZUserControl
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

	void UpdateGridColumnLayout()
	{
		var layoutProvider = new NctsPhase5LayoutProvider();
		INctsPhase5TransitionPeriodLayoutProvider transitionPeriodProvider = layoutProvider;
		INctsPhase5LayoutProvider phase5Provider = layoutProvider;
		var isInPhase5TransitionPeriod = CurrentDataItem is NctsDepartureCargoDesc goodsItem && goodsItem.IsInPhase5TransitionPeriod;
		GoodsItemsGrid.ApplyGridColumnLayout(isInPhase5TransitionPeriod ? transitionPeriodProvider.GetDepartureGoodsItemsGridColumnLayout() : phase5Provider.GetDepartureGoodsItemsGridColumnLayout());
	}

	void InitializeTariffColumnTariffType()
	{
		if (CurrentDataItem is NctsDepartureCargoDesc goodsItem)
		{
			var columnStyle = (Universal.GUI.TariffColumnStyleInfo)GoodsItemsGrid.GetColumnStyle(nameof(NctsCommonCargoDesc.BY_FormattedHarmonisedTariff));
			columnStyle.GetEffectiveDate = () => goodsItem.ValuationDate;
			columnStyle.GetTariffType = () => goodsItem.TariffType;
			columnStyle.GetDataGrouping = () => goodsItem.DataGroupingCode;
			columnStyle.GetSelectNomenclatureModes = () => goodsItem.GetTariffNomenclatureSelectionModes().ToList();
		}
	}
}

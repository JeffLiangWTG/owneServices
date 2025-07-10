using System;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class Phase5GoodsItemDifferencesGridUserControl : ZUserControl
	{
		public Phase5GoodsItemDifferencesGridUserControl()
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
			var layoutProvider = NctsPhase5LayoutProvider.GetLayoutProvider(DataSource?.DefaultDataGroupingCode);
			GoodsItemDifferencesGrid.ApplyGridColumnLayout(layoutProvider.GetGoodsItemDifferencesDetailsGridColumnLayout());
		}

		void InitializeTariffColumnTariffType()
		{
			var columnStyle = (Universal.GUI.TariffColumnStyleInfo)GoodsItemDifferencesGrid.GetColumnStyle(nameof(NctsCommonCargoDesc.BY_FormattedHarmonisedTariff));
			columnStyle.GetEffectiveDate = () => (CurrentDataItem as NctsArrivalCargoDesc)?.ValuationDate ?? ZDateTime.Today;
			columnStyle.GetTariffType = () => (CurrentDataItem as NctsArrivalCargoDesc)?.TariffType ?? ZString.Empty;
			columnStyle.GetDataGrouping = () => (CurrentDataItem as NctsArrivalCargoDesc)?.DataGroupingCode ?? ZString.Empty;
		}
	}
}

using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class Phase5GoodsItemDifferencesTabUserControl : ZUserControl
	{
		public Phase5GoodsItemDifferencesTabUserControl()
		{
			InitializeComponent();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			var provider = NctsPhase5LayoutProvider.GetLayoutProvider(DataSource?.DefaultDataGroupingCode);
			var itemsDetailLayout = provider.GoodsItemDifferencesDetailsLayout;
			var itemsDetailColumnLayout = provider.GoodsItemDifferencesDetailsColumnLayout;
			var liabilityDetailLayout = provider.LiabilityDetailsLayout;
			BindingSource.SetBindingMember(GoodsItemDifferencesDetailsDynamicLayoutPanel, ".");
			GoodsItemDifferencesDetailsDynamicLayoutPanel.UpdateLayout(itemsDetailLayout);
			BindingSource.SetBindingMember(GoodsItemDifferencesDetailsColumnDynamicLayoutPanel, ".");
			GoodsItemDifferencesDetailsColumnDynamicLayoutPanel.UpdateLayout(itemsDetailColumnLayout);
			BindingSource.SetBindingMember(LiabilityCalculationDynamicLayoutPanel, ".");
			LiabilityCalculationDynamicLayoutPanel.UpdateLayout(liabilityDetailLayout);
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (CurrentDataItem is NctsArrivalCargoDesc arrivalGoodsItem)
			{
				arrivalGoodsItem.BY_UnloadedStateInfo.ValueChanged -= ShowLiabilityCalculationTabPage;
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			if (CurrentDataItem is NctsArrivalCargoDesc arrivalGoodsItem)
			{
				arrivalGoodsItem.BY_UnloadedStateInfo.ValueChanged += ShowLiabilityCalculationTabPage;
				ShowLiabilityCalculationTabPage(arrivalGoodsItem, null);
			}
			base.OnCurrentDataItemChanged(e);
		}

		void ShowLiabilityCalculationTabPage(object sender, EventArgs e)
		{
			if (sender is NctsArrivalCargoDesc arrivalGoodsItem)
			{
				LiabilityCalculationTabPage.TabVisible = arrivalGoodsItem.IsLiabilityCalculationForArrivalSupported;
			}
		}

		protected new NctsHeader DataSource => base.DataSource as NctsHeader;

		public new NctsArrivalCargoDesc CurrentDataItem => (NctsArrivalCargoDesc)base.CurrentDataItem;
	}
}

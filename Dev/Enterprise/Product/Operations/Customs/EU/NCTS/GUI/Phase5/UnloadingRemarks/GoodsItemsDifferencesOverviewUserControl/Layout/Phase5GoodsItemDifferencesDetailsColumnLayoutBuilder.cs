using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public class Phase5GoodsItemDifferencesDetailsColumnLayoutBuilder<T> : ColumnLayoutBuilder<T, Phase5GoodsItemDifferencesDetailsColumnControlBag> where T : Business.NctsArrivalCargoDesc
	{
		public override Phase5GoodsItemDifferencesDetailsColumnControlBag CommonBag => Phase5GoodsItemDifferencesDetailsColumnControlBag.Instance;

		protected override int MaxColumns => 2;

		protected override void SetDefaultVisibilities()
		{
			base.SetDefaultVisibilities();

			SetVisibility(CommonBag.UnloadedValueLabel, x => x.UnloadedColumsVisible, x => x.BY_UnloadedStateInfo);
			SetVisibility(CommonBag.UnloadedCommodityCodeCodeFindBox, x => x.UnloadedColumsVisible, x => x.BY_UnloadedStateInfo);
			SetVisibility(CommonBag.UnloadedCusCodeCodeFindBox, x => x.UnloadedColumsVisible, x => x.BY_UnloadedStateInfo);
			SetVisibility(CommonBag.UnloadedDescriptionTextBox, x => x.UnloadedColumsVisible, x => x.BY_UnloadedStateInfo);
			SetVisibility(CommonBag.UnloadedGrossWeightDropEdit, x => x.UnloadedColumsVisible, x => x.BY_UnloadedStateInfo);
			SetVisibility(CommonBag.UnloadedNetWeightDropEdit, x => x.UnloadedColumsVisible, x => x.BY_UnloadedStateInfo);
		}
	}
}

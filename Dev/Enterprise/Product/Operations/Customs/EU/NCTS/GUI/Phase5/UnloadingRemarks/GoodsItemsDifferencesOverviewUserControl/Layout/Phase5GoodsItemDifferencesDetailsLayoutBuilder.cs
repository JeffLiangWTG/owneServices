using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public class Phase5GoodsItemDifferencesDetailsLayoutBuilder<T> : ColumnLayoutBuilder<T, Phase5GoodsItemDifferencesDetailsControlBag> where T : Business.NctsArrivalCargoDesc
	{
		public override Phase5GoodsItemDifferencesDetailsControlBag CommonBag => Phase5GoodsItemDifferencesDetailsControlBag.Instance;

		protected override int MaxColumns => 1;
	}
}

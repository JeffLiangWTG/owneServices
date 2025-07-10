using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public class GoodsItemDetailsLayoutBuilder<T> : ColumnLayoutBuilder<T, GoodsItemDetailsControlBag> where T : Business.NctsDepartureCargoDesc
	{
		public override GoodsItemDetailsControlBag CommonBag => GoodsItemDetailsControlBag.Instance;

		protected override int MaxColumns => 3;
	}
}

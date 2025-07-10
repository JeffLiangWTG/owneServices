using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI;

public class GoodsItemDetailsLayoutBuilder<T> : ColumnLayoutBuilder<T, GoodsItemDetailsControlBag> where T : Business.NctsDepartureCargoDesc
{
	public override GoodsItemDetailsControlBag CommonBag => GoodsItemDetailsControlBag.Instance;

	protected override int MaxColumns => 2;
}

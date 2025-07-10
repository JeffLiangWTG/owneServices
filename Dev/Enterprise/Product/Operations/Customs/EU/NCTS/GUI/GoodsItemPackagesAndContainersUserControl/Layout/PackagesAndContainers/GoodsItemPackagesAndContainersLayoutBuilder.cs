using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public class GoodsItemPackagesAndContainersLayoutBuilder<T> : ColumnLayoutBuilder<T, GoodsItemPackagesAndContainersControlBag> where T : Business.NctsDepartureCargoDesc
	{
		public override GoodsItemPackagesAndContainersControlBag CommonBag => GoodsItemPackagesAndContainersControlBag.Instance;

		protected override int MaxColumns => 2;
	}
}

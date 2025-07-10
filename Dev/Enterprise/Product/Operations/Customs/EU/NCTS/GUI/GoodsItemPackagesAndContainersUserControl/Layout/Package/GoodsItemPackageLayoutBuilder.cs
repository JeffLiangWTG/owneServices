using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public class GoodsItemPackageLayoutBuilder<T> : ColumnLayoutBuilder<T, GoodsItemPackageControlBag> where T : Business.NctsPackage
	{
		public override GoodsItemPackageControlBag CommonBag => GoodsItemPackageControlBag.Instance;

		protected override int MaxColumns => 1;
	}
}

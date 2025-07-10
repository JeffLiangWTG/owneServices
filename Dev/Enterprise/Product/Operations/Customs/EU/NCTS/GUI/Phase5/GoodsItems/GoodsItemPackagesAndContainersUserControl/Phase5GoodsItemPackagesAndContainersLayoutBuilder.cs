using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public class Phase5GoodsItemPackagesAndContainersLayoutBuilder<T> : ColumnLayoutBuilder<T, Phase5GoodsItemPackagesAndContainersControlBag> where T : Business.NctsPackage
	{
		public override Phase5GoodsItemPackagesAndContainersControlBag CommonBag => Phase5GoodsItemPackagesAndContainersControlBag.Instance;

		protected override int MaxColumns => 2;

		public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;
	}
}

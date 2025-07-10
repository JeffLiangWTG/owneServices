using Enterprise.Customs.IL.Manifest.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.Manifest.GUI
{
	public class AsycudaPackedItemDetailsBuilder : ColumnLayoutBuilder<AsycudaPackedItem, AsycudaPackedItemDetailsControlBag>
	{
		public override AsycudaPackedItemDetailsControlBag CommonBag => AsycudaPackedItemDetailsControlBag.Instance;

		public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;

		protected override int MaxColumns => 1;
	}
}

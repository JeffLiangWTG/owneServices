using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class CusMiscRequestHeaderViewLayoutBuilder : ColumnLayoutBuilder<CusMiscRequestHeader, CusMiscRequestHeaderViewControlBag>
	{
		public override CusMiscRequestHeaderViewControlBag CommonBag => CusMiscRequestHeaderViewControlBag.Instance;
		public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;
		protected override int MaxColumns => 2;
	}
}

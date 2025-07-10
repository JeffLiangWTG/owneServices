using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class FTADetailsLayoutBuilder : ColumnLayoutBuilder<JobDeclaration, FTADetailsControlBag>
	{
		public override FTADetailsControlBag CommonBag => FTADetailsControlBag.Instance;
		public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;
		protected override int MaxColumns => 1;
	}
}

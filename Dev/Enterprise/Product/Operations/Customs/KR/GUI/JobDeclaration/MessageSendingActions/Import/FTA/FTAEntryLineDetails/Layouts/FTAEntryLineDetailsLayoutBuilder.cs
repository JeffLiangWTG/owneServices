using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class FTAEntryLineDetailsLayoutBuilder : ColumnLayoutBuilder<JobDeclaration, FTAEntryLineDetailsControlBag>
	{
		public override FTAEntryLineDetailsControlBag CommonBag => FTAEntryLineDetailsControlBag.Instance;
		public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;
		protected override int MaxColumns => 2;
	}
}

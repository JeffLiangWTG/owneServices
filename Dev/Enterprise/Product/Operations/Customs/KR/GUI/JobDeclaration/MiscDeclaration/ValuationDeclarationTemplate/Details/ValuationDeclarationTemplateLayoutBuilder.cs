using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class ValuationDeclarationTemplateLayoutBuilder : ColumnLayoutBuilder<JobDeclaration, ValuationDeclarationTemplateDetailsControlBag>
	{
		public override ValuationDeclarationTemplateDetailsControlBag CommonBag => ValuationDeclarationTemplateDetailsControlBag.Instance;
		public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Medium;
		protected override int MaxColumns => 1;
	}
}

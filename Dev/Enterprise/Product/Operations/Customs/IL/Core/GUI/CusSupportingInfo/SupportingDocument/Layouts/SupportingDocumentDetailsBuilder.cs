using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.GUI
{
	public class SupportingDocumentDetailsBuilder : ColumnLayoutBuilder<CusSupportingInfo, SupportingDocumentDetailsControlBag>
	{
		public override SupportingDocumentDetailsControlBag CommonBag => SupportingDocumentDetailsControlBag.Instance;

		public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;

		protected override int MaxColumns => 2;
	}
}

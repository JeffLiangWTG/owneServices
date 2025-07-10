using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

public class SupportingDocumentDetailsLayoutBuilder<T> : ColumnLayoutBuilder<T, SupportingDocumentDetailsControlBag> where T : SupportingDocument
{
	public override SupportingDocumentDetailsControlBag CommonBag => SupportingDocumentDetailsControlBag.Instance;

	protected override int MaxColumns => 1;

	public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;
}

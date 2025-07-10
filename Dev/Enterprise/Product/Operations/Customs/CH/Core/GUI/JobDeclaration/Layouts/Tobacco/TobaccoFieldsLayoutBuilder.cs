using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

class TobaccoFieldsLayoutBuilder<T> : ColumnLayoutBuilder<T, TobaccoFieldsControlBag> where T : Tobacco
{
	public override TobaccoFieldsControlBag CommonBag { get; } = TobaccoFieldsControlBag.Instance;

	public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;

	protected override int MaxColumns => 1;
}

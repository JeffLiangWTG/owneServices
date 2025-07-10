using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

public sealed class AdditionalInformationFieldsLayoutBuilder<T> : ColumnLayoutBuilder<T, AdditionalInformationFieldsControlBag> where T : AdditionalInformation
{
	public override AdditionalInformationFieldsControlBag CommonBag { get; } = AdditionalInformationFieldsControlBag.Instance;

	public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;

	protected override int MaxColumns => 1;
}

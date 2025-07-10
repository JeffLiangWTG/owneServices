using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

public sealed class InAndOutwardProcessingFieldsLayoutBuilder : ColumnLayoutBuilder<JobComInvoiceLine, InAndOutwardProcessingFieldsControlBag>
{
	public override InAndOutwardProcessingFieldsControlBag CommonBag { get; } = InAndOutwardProcessingFieldsControlBag.Instance;

	public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;

	protected override int MaxColumns => 1;
}

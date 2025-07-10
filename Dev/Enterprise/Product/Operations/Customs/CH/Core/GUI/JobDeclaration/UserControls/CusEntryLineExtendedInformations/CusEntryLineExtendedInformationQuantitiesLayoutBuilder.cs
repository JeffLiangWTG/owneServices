using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

public sealed class CusEntryLineExtendedInformationQuantitiesLayoutBuilder<T> : ColumnLayoutBuilder<T, CusEntryLineExtendedInformationQuantitiesControlBag> where T : CusEntryLine
{
	public override CusEntryLineExtendedInformationQuantitiesControlBag CommonBag { get; } = CusEntryLineExtendedInformationQuantitiesControlBag.Instance;

	protected override int MaxColumns => 1;

	public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;
}

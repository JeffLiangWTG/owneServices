using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

public class DeclarationOtherDetailsLayoutBuilder : ColumnLayoutBuilder<JobDeclaration, DeclarationOtherDetailsControlBag>
{
	public override DeclarationOtherDetailsControlBag CommonBag => DeclarationOtherDetailsControlBag.Instance;

	protected override int MaxColumns => 1;

	public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;
}

using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class BondedFactoryLayoutBuilder : ColumnLayoutBuilder<JobDeclaration, EntryInstructionLayoutControlBag>
	{
		public override EntryInstructionLayoutControlBag CommonBag => EntryInstructionLayoutControlBag.Instance;
		public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Medium;
		protected override int MaxColumns => 1;
	}
}

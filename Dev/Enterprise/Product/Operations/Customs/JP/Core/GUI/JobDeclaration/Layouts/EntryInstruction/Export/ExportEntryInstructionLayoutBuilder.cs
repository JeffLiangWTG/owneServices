using Enterprise.Customs.JP.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	public sealed class ExportEntryInstructionLayoutBuilder : ColumnLayoutBuilder<CusEntryInstruction, ExportEntryInstructionControlBag>
	{
		public override ExportEntryInstructionControlBag CommonBag => ExportEntryInstructionControlBag.Instance;

		public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;
	}
}

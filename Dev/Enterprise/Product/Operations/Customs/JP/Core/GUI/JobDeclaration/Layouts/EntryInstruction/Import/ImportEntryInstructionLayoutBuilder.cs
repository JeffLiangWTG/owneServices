using Enterprise.Customs.JP.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	public sealed class ImportEntryInstructionLayoutBuilder : ColumnLayoutBuilder<CusEntryInstruction, ImportEntryInstructionControlBag>
	{
		public override ImportEntryInstructionControlBag CommonBag => ImportEntryInstructionControlBag.Instance;

		public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;
	}
}

using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

public sealed class MessageAndCustomsStatusWithOverrideLayoutBuilder : ColumnLayoutBuilder<CusEntryInstruction, EntryInstructionDetailsControlBag>
{
	public override EntryInstructionDetailsControlBag CommonBag => EntryInstructionDetailsControlBag.Instance;

	protected override int MaxColumns => 1;
}

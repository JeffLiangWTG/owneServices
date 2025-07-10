using Enterprise.Customs.GUI;
using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

public class EntryInstructionsDetailsLayoutBuilder : ColumnLayoutBuilder<CusEntryInstruction, EntryInstructionBasicDetailsControlBag>
{
	public override EntryInstructionBasicDetailsControlBag CommonBag => EntryInstructionBasicDetailsControlBag.Instance;

	public EntryInstructionDetailsControlBag INBag { get; } = EntryInstructionDetailsControlBag.Instance;

	protected override int MaxColumns => 3;
}

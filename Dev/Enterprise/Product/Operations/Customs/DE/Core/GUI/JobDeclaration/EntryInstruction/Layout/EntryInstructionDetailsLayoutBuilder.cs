using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public class EntryInstructionDetailsLayoutBuilder : ColumnLayoutBuilder<CusEntryInstruction, EntryInstructionDetailsControlBag>
	{
		public override EntryInstructionDetailsControlBag CommonBag { get; } = EntryInstructionDetailsControlBag.Instance;

		protected override int MaxColumns => 3;
	}
}

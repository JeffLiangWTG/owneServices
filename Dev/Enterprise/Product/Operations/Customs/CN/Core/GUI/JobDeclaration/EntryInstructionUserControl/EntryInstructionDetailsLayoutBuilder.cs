using Enterprise.Customs.CN.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CN.GUI
{
	public class EntryInstructionDetailsLayoutBuilder : ColumnLayoutBuilder<CusEntryInstruction, EntryInstructionBasicDetailsControlBag>
	{
		public override EntryInstructionBasicDetailsControlBag CommonBag => EntryInstructionBasicDetailsControlBag.Instance;

		public EntryInstructionDetailsControlBag CNBag { get; } = EntryInstructionDetailsControlBag.Instance;

		protected override int MaxColumns => 3;
	}
}

using Enterprise.Customs.JP.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	public sealed class CDB01EntryInstructionLayoutBuilder : ColumnLayoutBuilder<CusEntryInstruction, CDB01EntryInstructionControlBag>
	{
		public override CDB01EntryInstructionControlBag CommonBag => CDB01EntryInstructionControlBag.Instance;

		protected override int MaxColumns => 3;
	}
}

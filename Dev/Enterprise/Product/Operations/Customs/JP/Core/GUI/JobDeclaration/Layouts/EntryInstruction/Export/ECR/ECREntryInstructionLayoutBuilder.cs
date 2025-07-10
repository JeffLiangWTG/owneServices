using Enterprise.Customs.JP.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	public sealed class ECREntryInstructionLayoutBuilder : ColumnLayoutBuilder<CusEntryInstruction, ECREntryInstructionControlBag>
	{
		public override ECREntryInstructionControlBag CommonBag => ECREntryInstructionControlBag.Instance;

		protected override int MaxColumns => 3;
	}
}

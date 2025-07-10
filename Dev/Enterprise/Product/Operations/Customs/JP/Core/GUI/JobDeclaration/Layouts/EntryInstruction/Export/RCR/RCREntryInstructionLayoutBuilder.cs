using Enterprise.Customs.JP.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	public sealed class RCREntryInstructionLayoutBuilder : ColumnLayoutBuilder<CusEntryInstruction, RCREntryInstructionControlBag>
	{
		public override RCREntryInstructionControlBag CommonBag => RCREntryInstructionControlBag.Instance;
	}
}

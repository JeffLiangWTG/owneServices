using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.CA.Business.MessageBuilders;

namespace Enterprise.Customs.CA.GUI;

internal class CloseReportHouseTreeNode : TreeNode
{
	internal CloseReportHouseTreeNode(HouseCCNInstruction instruction)
		: base(instruction.CCN)
	{
		this.instruction = instruction;
	}

	readonly HouseCCNInstruction instruction;

	public void UpdateInstructionIsShouldSend(ZBool isShouldSend)
	{
		this.instruction.IsShouldSend = isShouldSend;
	}

	internal ZString HouseCCN
	{
		get { return instruction.CCN; }
	}
}

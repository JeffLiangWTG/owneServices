namespace Enterprise.Customs.IN.Business;

public class CusContainerOnEntryInstruction : Customs.Business.CusContainerOnEntryInstruction
{
	public CusContainerOnEntryInstruction(CusEntryInstruction instruction) : base(instruction)
	{
	}

	protected override Customs.Business.CusContainerOnEntryInstructionValidation GetNewValidation()
	{
		return new CusContainerOnEntryInstructionValidation(this);
	}

	public new CusEntryInstruction Instruction => (CusEntryInstruction)base.Instruction;

	public new CusContainer Container
	{
		get => (CusContainer)base.Container;
		set => base.Container = value;
	}
}

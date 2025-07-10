namespace Enterprise.Customs.IT.Business.Declaration;

public class GuaranteeForEntryInstructionCollection : EU.Business.Declaration.GuaranteeForEntryInstructionCollection
{
	public GuaranteeForEntryInstructionCollection(CusEntryInstruction master) : base(master)
	{
	}

	public new GuaranteeForEntryInstruction this[int index] => (GuaranteeForEntryInstruction)base[index];

	public new GuaranteeForEntryInstruction AddNew() => (GuaranteeForEntryInstruction)base.AddNew();
}

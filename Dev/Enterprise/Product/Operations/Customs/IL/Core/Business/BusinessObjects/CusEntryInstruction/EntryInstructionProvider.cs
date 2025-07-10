using Enterprise.Customs.Business;

namespace Enterprise.Customs.IL.Business;
public class EntryInstructionProvider : Customs.Business.EntryInstructionProvider
{
	public EntryInstructionProvider(JobDeclaration declaration)
		: base(declaration)
	{
	}

	protected new JobDeclaration ParentDeclaration => (JobDeclaration)base.ParentDeclaration;

	public new ICusEntryInstructionCollection<CusEntryInstruction> CustomsEntryInstructions => (CusEntryInstructionCollection<CusEntryInstruction>)base.CustomsEntryInstructions;
	protected override ICusEntryInstructionCollection<Customs.Business.CusEntryInstruction> GetNewCusEntryInstructionCollectionCore() => new CusEntryInstructionCollection<CusEntryInstruction>(ParentDeclaration);
}

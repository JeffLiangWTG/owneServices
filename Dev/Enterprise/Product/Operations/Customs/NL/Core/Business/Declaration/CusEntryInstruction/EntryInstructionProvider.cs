using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.NL.Business.Declaration;

public class EntryInstructionProvider : EU.Business.Declaration.EntryInstructionProvider
{
	public EntryInstructionProvider(JobDeclaration declaration) : base(declaration)
	{
	}

	protected new JobDeclaration ParentDeclaration => base.ParentDeclaration as JobDeclaration;

	protected override Customs.Business.ICusEntryInstructionCollection<Customs.Business.CusEntryInstruction> GetNewCusEntryInstructionCollectionCore() => new CusEntryInstructionCollection(ParentDeclaration);

	public new Customs.Business.ICusEntryInstructionCollection<Customs.Business.CusEntryInstruction> CustomsEntryInstructions => (CusEntryInstructionCollection<CusEntryInstruction>)GetNewCusEntryInstructionCollectionCore();
}

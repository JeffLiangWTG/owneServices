using Enterprise.Customs.Business;

namespace Enterprise.Customs.BE.Business.Declaration;

public class EntryInstructionProvider : EU.Business.Declaration.EntryInstructionProvider
{
	public EntryInstructionProvider(EU.Business.Declaration.JobDeclaration declaration) : base(declaration)
	{
	}

	public EntryInstructionProvider(JobDeclaration declaration, CusEntryInstructionComparer comparer) : base(declaration, comparer)
	{
	}

	protected new JobDeclaration ParentDeclaration => base.ParentDeclaration as JobDeclaration;

	protected override ICusEntryInstructionCollection<Customs.Business.CusEntryInstruction> GetNewCusEntryInstructionCollectionCore() => new CusEntryInstructionCollection(ParentDeclaration);

	public new CusEntryInstructionCollection CustomsEntryInstructions => (CusEntryInstructionCollection)base.CustomsEntryInstructions;
}

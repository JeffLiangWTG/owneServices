using Enterprise.Customs.Business;

namespace Enterprise.Customs.IN.Business;

public class EntryInstructionProvider : Customs.Business.EntryInstructionProvider
{
	public EntryInstructionProvider(JobDeclaration declaration) : base(declaration)
	{
	}

	protected new JobDeclaration ParentDeclaration => (JobDeclaration)base.ParentDeclaration;

	protected override ICusEntryInstructionCollection<Customs.Business.CusEntryInstruction> GetNewCusEntryInstructionCollectionCore() => new CusEntryInstructionCollection(ParentDeclaration);
}

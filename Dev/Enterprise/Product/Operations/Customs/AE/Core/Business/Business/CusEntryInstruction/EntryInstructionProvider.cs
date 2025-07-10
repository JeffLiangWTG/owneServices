using Enterprise.Customs.Business;

namespace Enterprise.Customs.AE.Business;

public class EntryInstructionProvider : Customs.Business.EntryInstructionProvider
{
	public EntryInstructionProvider(JobDeclaration declaration) : base(declaration)
	{
	}

	protected new JobDeclaration ParentDeclaration => (JobDeclaration)base.ParentDeclaration;

	protected override ICusEntryInstructionCollection<Customs.Business.CusEntryInstruction> GetNewCusEntryInstructionCollectionCore() => new CusEntryInstructionCollection(ParentDeclaration);
}

using Enterprise.Customs.Business;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class EntryInstructionProvider : EU.Business.Declaration.EntryInstructionProvider
	{
		public EntryInstructionProvider(JobDeclaration declaration) : base(declaration)
		{
		}

		protected new JobDeclaration ParentDeclaration => base.ParentDeclaration as JobDeclaration;

		protected override ICusEntryInstructionCollection<Customs.Business.CusEntryInstruction> GetNewCusEntryInstructionCollectionCore() => new CusEntryInstructionCollection(ParentDeclaration);
	}
}

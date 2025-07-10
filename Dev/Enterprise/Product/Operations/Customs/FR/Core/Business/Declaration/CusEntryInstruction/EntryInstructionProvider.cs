namespace Enterprise.Customs.FR.Business.Declaration
{
	public class EntryInstructionProvider : EU.Business.Declaration.EntryInstructionProvider
	{
		public EntryInstructionProvider(EU.Business.Declaration.JobDeclaration declaration) : base(declaration)
		{
		}

		protected new JobDeclaration ParentDeclaration => (JobDeclaration)base.ParentDeclaration;

		protected override Customs.Business.ICusEntryInstructionCollection<Customs.Business.CusEntryInstruction> GetNewCusEntryInstructionCollectionCore() => new CusEntryInstructionCollection(ParentDeclaration);
	}
}

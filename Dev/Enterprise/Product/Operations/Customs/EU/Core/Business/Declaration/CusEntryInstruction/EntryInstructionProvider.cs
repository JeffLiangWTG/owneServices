using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class EntryInstructionProvider : Customs.Business.EntryInstructionProvider
	{
		public EntryInstructionProvider(JobDeclaration declaration, CusEntryInstructionComparer comparer) : base(declaration, comparer)
		{
		}

		public EntryInstructionProvider(JobDeclaration declaration) : base(declaration)
		{
		}

		protected new JobDeclaration ParentDeclaration => base.ParentDeclaration as JobDeclaration;

		protected override ICusEntryInstructionCollection<Customs.Business.CusEntryInstruction> GetNewCusEntryInstructionCollectionCore() => new CusEntryInstructionCollection<CusEntryInstruction>(ParentDeclaration);

		public new ICusEntryInstructionCollection<CusEntryInstruction> CustomsEntryInstructions => (ICusEntryInstructionCollection<CusEntryInstruction>)base.CustomsEntryInstructions;
	}
}

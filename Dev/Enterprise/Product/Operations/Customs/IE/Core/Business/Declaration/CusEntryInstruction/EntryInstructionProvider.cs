using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class EntryInstructionProvider : EU.Business.Declaration.EntryInstructionProvider
	{
		public EntryInstructionProvider(JobDeclaration declaration, CusEntryInstructionComparer comparer) : base(declaration, comparer)
		{
		}

		protected new JobDeclaration ParentDeclaration => (JobDeclaration)base.ParentDeclaration;

		public new ICusEntryInstructionCollection<CusEntryInstruction> CustomsEntryInstructions => (ICusEntryInstructionCollection<CusEntryInstruction>)base.CustomsEntryInstructions;

		protected override ICusEntryInstructionCollection<Customs.Business.CusEntryInstruction> GetNewCusEntryInstructionCollectionCore() => new EU.Business.Declaration.CusEntryInstructionCollection<CusEntryInstruction>(ParentDeclaration);
	}
}

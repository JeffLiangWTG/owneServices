using Enterprise.Customs.Business;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class EntryInstructionProvider : EU.Business.Declaration.EntryInstructionProvider
	{
		public EntryInstructionProvider(JobDeclaration declaration) : base(declaration)
		{
		}

		protected new JobDeclaration ParentDeclaration => base.ParentDeclaration as JobDeclaration;

		protected override ICusEntryInstructionCollection<Customs.Business.CusEntryInstruction> GetNewCusEntryInstructionCollectionCore() => new EU.Business.Declaration.CusEntryInstructionCollection<CusEntryInstruction>(ParentDeclaration);

		public new ICusEntryInstructionCollection<CusEntryInstruction> CustomsEntryInstructions => (ICusEntryInstructionCollection<CusEntryInstruction>)base.CustomsEntryInstructions;
	}
}

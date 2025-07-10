using Enterprise.Customs.Business;

namespace Enterprise.Customs.BR.Business
{
	public class EntryInstructionProvider : Customs.Business.EntryInstructionProvider
	{
		public EntryInstructionProvider(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected new JobDeclaration ParentDeclaration => base.ParentDeclaration as JobDeclaration;

		protected override ICusEntryInstructionCollection<Customs.Business.CusEntryInstruction> GetNewCusEntryInstructionCollectionCore() => new CusEntryInstructionCollection<CusEntryInstruction>(ParentDeclaration);

		public new CusEntryInstructionCollection<CusEntryInstruction> CustomsEntryInstructions => (CusEntryInstructionCollection<CusEntryInstruction>)base.CustomsEntryInstructions;
	}
}

using ECB = Enterprise.Customs.Business;

namespace Enterprise.Customs.CN.Business
{
	public class EntryInstructionProvider : ECB.EntryInstructionProvider
	{
		public EntryInstructionProvider(JobDeclaration declaration) : base(declaration)
		{
		}

		protected new JobDeclaration ParentDeclaration => base.ParentDeclaration as JobDeclaration;

		protected override ECB.ICusEntryInstructionCollection<ECB.CusEntryInstruction> GetNewCusEntryInstructionCollectionCore() => new CusEntryInstructionCollection(ParentDeclaration);

		public new CusEntryInstructionCollection CustomsEntryInstructions => (CusEntryInstructionCollection)base.CustomsEntryInstructions;
	}
}

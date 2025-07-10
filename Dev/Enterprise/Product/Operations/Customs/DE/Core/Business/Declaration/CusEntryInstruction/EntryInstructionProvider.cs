using Enterprise.Customs.DE.Business.Declaration;
using CusEntryInstructionComparer = Enterprise.Customs.DE.Business.Declaration.CusEntryInstructionComparer;

namespace Enterprise.Customs.DE.Business
{
	public class EntryInstructionProvider : EU.Business.Declaration.EntryInstructionProvider
	{
		public EntryInstructionProvider(JobDeclaration declaration, CusEntryInstructionComparer comparer) : base(declaration, comparer)
		{
		}

		protected new JobDeclaration ParentDeclaration => base.ParentDeclaration as JobDeclaration;

		protected override Customs.Business.ICusEntryInstructionCollection<Customs.Business.CusEntryInstruction> GetNewCusEntryInstructionCollectionCore() => new EU.Business.Declaration.CusEntryInstructionCollection<CusEntryInstruction>(ParentDeclaration);

		public new Customs.Business.ICusEntryInstructionCollection<CusEntryInstruction> CustomsEntryInstructions => (Customs.Business.ICusEntryInstructionCollection<CusEntryInstruction>)base.CustomsEntryInstructions;
	}
}

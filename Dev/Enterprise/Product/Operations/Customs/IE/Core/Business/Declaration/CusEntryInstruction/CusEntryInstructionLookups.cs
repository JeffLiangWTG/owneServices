using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class CusEntryInstructionLookups : EU.Business.Declaration.CusEntryInstructionLookups
	{
		public CusEntryInstructionLookups(CusEntryInstruction cusEntryInstruction) : base(cusEntryInstruction)
		{
		}

		public new CusEntryInstruction Parent => (CusEntryInstruction)base.Parent;

		protected override CodeDescriptionPairList DeclarationTypeListCore => Factory.GetCachedValue<ImportDeclarationTypeList>();
	}
}

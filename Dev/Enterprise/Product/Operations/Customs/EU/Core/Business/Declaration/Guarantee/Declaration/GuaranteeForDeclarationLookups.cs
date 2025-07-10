using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class GuaranteeForDeclarationLookups : CommonGuaranteeLookups
	{
		public GuaranteeForDeclarationLookups(GuaranteeForDeclaration guarantee) : base(guarantee)
		{
		}
		protected new GuaranteeForDeclaration Parent => (GuaranteeForDeclaration)base.Parent;

		public override CodeDescriptionPairList HolderIdentificationList => CommonLookups.EORILookup(Parent?.Declaration, null);

		public ICusEntryInstructionCollection<Customs.Business.CusEntryInstruction> EntryInstructions => Parent.Declaration?.CustomsEntryInstructions;

		protected override ZGuid? PrimaryGuaranteeHolderAddress => Parent.Declaration?.Importer?.PK;
		protected override ZGuid? SecondaryGuaranteeHolderAddress => Parent.Declaration?.Declarant?.OA_OH;
	}
}

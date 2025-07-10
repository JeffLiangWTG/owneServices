using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class GuaranteeForDeclarationValidation : CommonGuaranteeValidation
	{
		public GuaranteeForDeclarationValidation(GuaranteeForDeclaration parent) : base(parent)
		{
		}
		protected new GuaranteeForDeclaration Parent => (GuaranteeForDeclaration)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateEntryInstructionID();
		}

		public void ValidateEntryInstructionID() => ValidateCalculatedProperty(Parent.EntryInstructionIDInfo);
		protected virtual void CheckEntryInstructionID() => TypeValidation.CheckValidGuid(Parent.EntryInstructionIDInfo);
	}
}

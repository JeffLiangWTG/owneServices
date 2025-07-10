using CargoWise.EntityFramework;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class DeltaGCusEntryInstructionValidation : CusEntryInstructionValidation
	{
		public DeltaGCusEntryInstructionValidation(CusEntryInstruction parent) : base(parent)
		{
		}

		protected override void CheckCEI_SubStyle()
		{
			base.CheckCEI_SubStyle();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CEI_SubStyleInfo);
		}
	}
}

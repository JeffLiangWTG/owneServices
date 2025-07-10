using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class GuaranteeForEntryInstructionValidation : EU.Business.Declaration.GuaranteeForEntryInstructionValidation
	{
		public GuaranteeForEntryInstructionValidation(GuaranteeForEntryInstruction parent) : base(parent)
		{
		}

		protected override void CheckPW_BondAmount()
		{
			base.CheckPW_BondAmount();

			MandatoryValidation.MessageErrorIfNotEntered(Parent.PW_BondAmountInfo);
			MandatoryValidation.CheckNotNegative(Parent.PW_BondAmountInfo);
		}
	}
}

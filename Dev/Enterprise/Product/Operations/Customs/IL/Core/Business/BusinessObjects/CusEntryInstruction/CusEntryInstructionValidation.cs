using CargoWise.EntityFramework;

namespace Enterprise.Customs.IL.Business
{
	public class CusEntryInstructionValidation : AutoILCusEntryInstructionValidation
	{
		public CusEntryInstructionValidation(CusEntryInstruction parent) : base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();

			using (((ISingleElementListInternal)Parent).SuspendListChanged())
			{
				ValidateCEI_FormattedProcedure();
			}
		}

		public void ValidateCEI_FormattedProcedure()
		{
			ValidateCalculatedProperty(Parent.CEI_FormattedProcedureInfo);
		}

		protected override void ValidateStyleList()
		{
			// Intentionally kept blank: Validation not required 
		}

		protected new CusEntryInstruction Parent => (CusEntryInstruction)base.Parent;

		protected void CheckCEI_FormattedProcedure()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CEI_FormattedProcedureInfo);
		}

		protected override void CheckCEI_DateForDuty()
		{
			base.CheckCEI_DateForDuty();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CEI_DateForDutyInfo);
		}

		protected override void CheckCEI_Description()
		{
			base.CheckCEI_Description();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CEI_DescriptionInfo);
		}

		protected override void CheckCEI_AutonomyRegionType()
		{
			base.CheckCEI_AutonomyRegionType();
			ListValidation.MessageErrorIfInvalidCode(Parent.CEI_AutonomyRegionTypeInfo);
		}

		protected override void CheckCEI_CustomsPackType()
		{
			base.CheckCEI_CustomsPackType();
			ListValidation.MessageErrorIfInvalidCode(Parent.CEI_CustomsPackTypeInfo);
		}
	}
}

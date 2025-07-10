
using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business
{
	// Whether this class is necessary will be investigated in the next check-in
	public class B2JobDeclarationValidation : JobDeclarationValidation
	{
		public B2JobDeclarationValidation(JobDeclaration parent)
			: base(parent)
		{
		}

		protected override void CheckJE_EntryAuthorisationDateIsValidZDateTimeRange()
		{
			var limits = new TypeValidationLimits() { PastYearsBeforeWarning = 4 };
			TypeValidation.CheckValidZDateTimeRange(Parent.JE_EntryAuthorisationDateInfo, limits);
		}

		protected override void CheckJE_CarrierCode()
		{
			base.CheckJE_CarrierCode();

			if (Parent is JobDeclaration dec && dec.IsB3X)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_CarrierCodeInfo);
			}
		}
	}
}

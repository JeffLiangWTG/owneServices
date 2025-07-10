//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRTreatmentRatePeriodAdditionalDutyCalculationValidation
//
//    This class should be used for overriding validation in AutoCMRTreatmentRatePeriodAdditionalDutyCalculationValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRTreatmentRatePeriodAdditionalDutyCalculationValidation : AutoCMRTreatmentRatePeriodAdditionalDutyCalculationValidation
	{
		public CMRTreatmentRatePeriodAdditionalDutyCalculationValidation(AutoCMRTreatmentRatePeriodAdditionalDutyCalculation parent)
			: base(parent)
		{
		}
	}
}

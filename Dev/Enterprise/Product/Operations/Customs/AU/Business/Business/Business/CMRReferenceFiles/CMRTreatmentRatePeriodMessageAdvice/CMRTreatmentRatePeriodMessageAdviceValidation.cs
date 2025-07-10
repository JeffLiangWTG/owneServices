//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRTreatmentRatePeriodMessageAdviceValidation
//
//    This class should be used for overriding validation in AutoCMRTreatmentRatePeriodMessageAdviceValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRTreatmentRatePeriodMessageAdviceValidation : AutoCMRTreatmentRatePeriodMessageAdviceValidation
	{
		public CMRTreatmentRatePeriodMessageAdviceValidation(AutoCMRTreatmentRatePeriodMessageAdvice parent)
			: base(parent)
		{
		}
	}
}

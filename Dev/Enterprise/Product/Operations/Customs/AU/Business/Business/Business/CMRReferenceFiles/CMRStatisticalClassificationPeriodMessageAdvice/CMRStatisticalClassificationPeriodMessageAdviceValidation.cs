//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRStatisticalClassificationPeriodMessageAdviceValidation
//
//    This class should be used for overriding validation in AutoCMRStatisticalClassificationPeriodMessageAdviceValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRStatisticalClassificationPeriodMessageAdviceValidation : AutoCMRStatisticalClassificationPeriodMessageAdviceValidation
	{
		public CMRStatisticalClassificationPeriodMessageAdviceValidation(AutoCMRStatisticalClassificationPeriodMessageAdvice parent)
			: base(parent)
		{
		}
	}
}

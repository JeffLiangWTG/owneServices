//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRTariffClassificationMessageAdviceValidation
//
//    This class should be used for overriding validation in AutoCMRTariffClassificationMessageAdviceValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRTariffClassificationMessageAdviceValidation : AutoCMRTariffClassificationMessageAdviceValidation
	{
		public CMRTariffClassificationMessageAdviceValidation(AutoCMRTariffClassificationMessageAdvice parent)
			: base(parent)
		{
		}
	}
}

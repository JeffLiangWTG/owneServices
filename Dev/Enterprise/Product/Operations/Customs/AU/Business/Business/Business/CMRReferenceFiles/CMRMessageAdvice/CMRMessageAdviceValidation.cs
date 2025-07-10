//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRMessageAdviceValidation
//
//    This class should be used for overriding validation in AutoCMRMessageAdviceValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRMessageAdviceValidation : AutoCMRMessageAdviceValidation
	{
		public CMRMessageAdviceValidation(AutoCMRMessageAdvice parent)
			: base(parent)
		{
		}
	}
}

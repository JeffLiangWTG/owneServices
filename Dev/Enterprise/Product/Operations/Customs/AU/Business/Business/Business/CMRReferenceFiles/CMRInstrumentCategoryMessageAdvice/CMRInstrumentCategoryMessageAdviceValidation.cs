//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRInstrumentCategoryMessageAdviceValidation
//
//    This class should be used for overriding validation in AutoCMRInstrumentCategoryMessageAdviceValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRInstrumentCategoryMessageAdviceValidation : AutoCMRInstrumentCategoryMessageAdviceValidation
	{
		public CMRInstrumentCategoryMessageAdviceValidation(AutoCMRInstrumentCategoryMessageAdvice parent)
			: base(parent)
		{
		}
	}
}

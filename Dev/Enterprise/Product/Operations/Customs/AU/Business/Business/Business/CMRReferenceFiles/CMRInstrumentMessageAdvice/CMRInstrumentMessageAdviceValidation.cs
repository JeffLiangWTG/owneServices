//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRInstrumentMessageAdviceValidation
//
//    This class should be used for overriding validation in AutoCMRInstrumentMessageAdviceValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRInstrumentMessageAdviceValidation : AutoCMRInstrumentMessageAdviceValidation
	{
		public CMRInstrumentMessageAdviceValidation(AutoCMRInstrumentMessageAdvice parent)
			: base(parent)
		{
		}
	}
}

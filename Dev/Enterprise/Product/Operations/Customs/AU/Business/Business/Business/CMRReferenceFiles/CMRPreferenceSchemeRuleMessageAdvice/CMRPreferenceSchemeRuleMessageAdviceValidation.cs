//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRPreferenceSchemeRuleMessageAdviceValidation
//
//    This class should be used for overriding validation in AutoCMRPreferenceSchemeRuleMessageAdviceValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRPreferenceSchemeRuleMessageAdviceValidation : AutoCMRPreferenceSchemeRuleMessageAdviceValidation
	{
		public CMRPreferenceSchemeRuleMessageAdviceValidation(AutoCMRPreferenceSchemeRuleMessageAdvice parent)
			: base(parent)
		{
		}
	}
}

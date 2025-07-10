//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRPreferenceSchemeRuleValidation
//
//    This class should be used for overriding validation in AutoCMRPreferenceSchemeRuleValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRPreferenceSchemeRuleValidation : AutoCMRPreferenceSchemeRuleValidation
	{
		public CMRPreferenceSchemeRuleValidation(AutoCMRPreferenceSchemeRule parent)
			: base(parent)
		{
		}
	}
}

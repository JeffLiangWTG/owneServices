//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRPreferenceRulePeriodCountryValidation
//
//    This class should be used for overriding validation in AutoCMRPreferenceRulePeriodCountryValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRPreferenceRulePeriodCountryValidation : AutoCMRPreferenceRulePeriodCountryValidation
	{
		public CMRPreferenceRulePeriodCountryValidation(AutoCMRPreferenceRulePeriodCountry parent)
			: base(parent)
		{
		}
	}
}

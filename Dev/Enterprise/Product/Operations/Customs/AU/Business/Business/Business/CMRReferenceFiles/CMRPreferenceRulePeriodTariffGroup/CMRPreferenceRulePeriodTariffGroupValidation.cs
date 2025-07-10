//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRPreferenceRulePeriodTariffGroupValidation
//
//    This class should be used for overriding validation in AutoCMRPreferenceRulePeriodTariffGroupValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRPreferenceRulePeriodTariffGroupValidation : AutoCMRPreferenceRulePeriodTariffGroupValidation
	{
		public CMRPreferenceRulePeriodTariffGroupValidation(AutoCMRPreferenceRulePeriodTariffGroup parent)
			: base(parent)
		{
		}
	}
}

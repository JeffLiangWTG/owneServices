//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRPreferenceRulePeriodCharacteristicValidation
//
//    This class should be used for overriding validation in AutoCMRPreferenceRulePeriodCharacteristicValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRPreferenceRulePeriodCharacteristicValidation : AutoCMRPreferenceRulePeriodCharacteristicValidation
	{
		public CMRPreferenceRulePeriodCharacteristicValidation(AutoCMRPreferenceRulePeriodCharacteristic parent)
			: base(parent)
		{
		}
	}
}

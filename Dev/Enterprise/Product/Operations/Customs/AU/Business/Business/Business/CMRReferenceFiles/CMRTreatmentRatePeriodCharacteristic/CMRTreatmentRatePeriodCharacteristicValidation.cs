//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRTreatmentRatePeriodCharacteristicValidation
//
//    This class should be used for overriding validation in AutoCMRTreatmentRatePeriodCharacteristicValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRTreatmentRatePeriodCharacteristicValidation : AutoCMRTreatmentRatePeriodCharacteristicValidation
	{
		public CMRTreatmentRatePeriodCharacteristicValidation(AutoCMRTreatmentRatePeriodCharacteristic parent)
			: base(parent)
		{
		}
	}
}

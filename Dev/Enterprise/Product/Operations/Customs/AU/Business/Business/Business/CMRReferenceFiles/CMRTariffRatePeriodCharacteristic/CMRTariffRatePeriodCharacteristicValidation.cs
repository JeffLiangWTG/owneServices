//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRTariffRatePeriodCharacteristicValidation
//
//    This class should be used for overriding validation in AutoCMRTariffRatePeriodCharacteristicValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRTariffRatePeriodCharacteristicValidation : AutoCMRTariffRatePeriodCharacteristicValidation
	{
		public CMRTariffRatePeriodCharacteristicValidation(AutoCMRTariffRatePeriodCharacteristic parent)
			: base(parent)
		{
		}
	}
}

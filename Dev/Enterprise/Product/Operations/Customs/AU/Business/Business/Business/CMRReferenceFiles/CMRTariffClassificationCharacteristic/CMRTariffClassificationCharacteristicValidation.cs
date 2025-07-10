//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRTariffClassificationCharacteristicValidation
//
//    This class should be used for overriding validation in AutoCMRTariffClassificationCharacteristicValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRTariffClassificationCharacteristicValidation : AutoCMRTariffClassificationCharacteristicValidation
	{
		public CMRTariffClassificationCharacteristicValidation(AutoCMRTariffClassificationCharacteristic parent)
			: base(parent)
		{
		}
	}
}

//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRStatisticalClassificationPeriodCharacteristicValidation
//
//    This class should be used for overriding validation in AutoCMRStatisticalClassificationPeriodCharacteristicValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRStatisticalClassificationPeriodCharacteristicValidation : AutoCMRStatisticalClassificationPeriodCharacteristicValidation
	{
		public CMRStatisticalClassificationPeriodCharacteristicValidation(AutoCMRStatisticalClassificationPeriodCharacteristic parent)
			: base(parent)
		{
		}
	}
}

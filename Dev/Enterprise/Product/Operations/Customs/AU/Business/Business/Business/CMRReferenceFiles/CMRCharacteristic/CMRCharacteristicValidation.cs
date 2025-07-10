//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRCharacteristicValidation
//
//    This class should be used for overriding validation in AutoCMRCharacteristicValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRCharacteristicValidation : AutoCMRCharacteristicValidation
	{
		public CMRCharacteristicValidation(AutoCMRCharacteristic parent)
			: base(parent)
		{
		}
	}
}

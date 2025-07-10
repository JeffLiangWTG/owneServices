//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRInstrumentCharacteristicValidation
//
//    This class should be used for overriding validation in AutoCMRInstrumentCharacteristicValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRInstrumentCharacteristicValidation : AutoCMRInstrumentCharacteristicValidation
	{
		public CMRInstrumentCharacteristicValidation(AutoCMRInstrumentCharacteristic parent)
			: base(parent)
		{
		}
	}
}

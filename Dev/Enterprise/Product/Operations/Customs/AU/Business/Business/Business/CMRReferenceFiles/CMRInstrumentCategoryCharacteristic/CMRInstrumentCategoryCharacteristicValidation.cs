//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRInstrumentCategoryCharacteristicValidation
//
//    This class should be used for overriding validation in AutoCMRInstrumentCategoryCharacteristicValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRInstrumentCategoryCharacteristicValidation : AutoCMRInstrumentCategoryCharacteristicValidation
	{
		public CMRInstrumentCategoryCharacteristicValidation(AutoCMRInstrumentCategoryCharacteristic parent)
			: base(parent)
		{
		}
	}
}

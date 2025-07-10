//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCACargoControlNumberAddInfoValidation
//
//    This class should be used for overriding validation in AutoCACargoControlNumberAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.CA.Business
{
	public class CACargoControlNumberAddInfoValidation : AutoCACargoControlNumberAddInfoValidation
	{
		public CACargoControlNumberAddInfoValidation(AutoCACargoControlNumberAddInfo parent) : base(parent)
		{
		}
	}
}

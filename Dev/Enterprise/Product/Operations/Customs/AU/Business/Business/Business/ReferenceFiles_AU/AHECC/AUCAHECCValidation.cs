//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAUCAHECCValidation
//
//    This class should be used for overriding validation in AutoAUCAHECCValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AUCAHECCValidation : AutoAUCAHECCValidation
	{
		public AUCAHECCValidation(AutoAUCAHECC parent)
			: base(parent)
		{
		}
	}
}

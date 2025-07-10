//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRAHECCCodeValidation
//
//    This class should be used for overriding validation in AutoCMRAHECCCodeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRAHECCCodeValidation : AutoCMRAHECCCodeValidation
	{
		public CMRAHECCCodeValidation(AutoCMRAHECCCode parent)
			: base(parent)
		{
		}
	}
}

//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRAqisPremisesValidation
//
//    This class should be used for overriding validation in AutoCMRAqisPremisesValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRAqisPremisesValidation : AutoCMRAqisPremisesValidation
	{
		public CMRAqisPremisesValidation(AutoCMRAqisPremises parent)
			: base(parent)
		{
		}
	}
}

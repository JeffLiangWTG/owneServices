//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAUCClassValidation
//
//    This class should be used for overriding validation in AutoAUCClassValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AUCClassValidation : AutoAUCClassValidation
	{
		public AUCClassValidation(AutoAUCClass parent)
			: base(parent)
		{
		}
	}
}

//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAUCSectionValidation
//
//    This class should be used for overriding validation in AutoAUCSectionValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AUCSectionValidation : AutoAUCSectionValidation
	{
		public AUCSectionValidation(AutoAUCSection parent)
			: base(parent)
		{
		}
	}
}

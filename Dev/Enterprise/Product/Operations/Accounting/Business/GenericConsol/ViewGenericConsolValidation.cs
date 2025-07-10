//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoViewGenericConsolValidation
//
//    This class should be used for overriding validation in AutoViewGenericConsolValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Accounting.Business.GenericConsol
{
	public class ViewGenericConsolValidation : AutoViewGenericConsolValidation
	{
		public ViewGenericConsolValidation(AutoViewGenericConsol parent) : base(parent)
		{
		}
	}
}


//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusCAeMHContainerValidation
//
//    This class should be used for overriding validation in AutoCusCAeMHContainerValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.CA.Business
{
	public class CusCAeMHContainerValidation : AutoCusCAeMHContainerValidation
	{
		public CusCAeMHContainerValidation(AutoCusCAeMHContainer parent) : base(parent)
		{
		}
	}
}

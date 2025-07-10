//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefStlScriptValidation
//
//    This class should be used for overriding validation in AutoRefStlScriptValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Billing.Business
{
	public class RefStlScriptValidation : AutoRefStlScriptValidation
	{
		public RefStlScriptValidation(AutoRefStlScript parent) : base(parent)
		{
		}
	}
}

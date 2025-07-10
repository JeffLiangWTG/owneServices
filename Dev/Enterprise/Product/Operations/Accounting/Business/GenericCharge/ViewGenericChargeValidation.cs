//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoViewGenericChargeValidation
//
//    This class should be used for overriding validation in AutoViewGenericChargeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Accounting.Business.GenericCharge
{
	public class ViewGenericChargeValidation : AutoViewGenericChargeValidation
	{
		public ViewGenericChargeValidation(AutoViewGenericCharge parent)
			: base(parent)
		{
		}
	}
}


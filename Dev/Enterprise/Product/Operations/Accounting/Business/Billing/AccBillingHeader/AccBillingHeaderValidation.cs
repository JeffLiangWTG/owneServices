//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccBillingHeaderValidation
//
//    This class should be used for overriding validation in AutoAccBillingHeaderValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Accounting.Business.Billing
{
	public class AccBillingHeaderValidation : AutoAccBillingHeaderValidation
	{
		public AccBillingHeaderValidation(AutoAccBillingHeader parent) : base(parent)
		{
		}
	}
}


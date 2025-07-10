//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccBillingItemValidation
//
//    This class should be used for overriding validation in AutoAccBillingItemValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Accounting.Business.Billing
{
	public class AccBillingItemValidation : AutoAccBillingItemValidation
	{
		public AccBillingItemValidation(AutoAccBillingItem parent) : base(parent)
		{
		}
	}
}


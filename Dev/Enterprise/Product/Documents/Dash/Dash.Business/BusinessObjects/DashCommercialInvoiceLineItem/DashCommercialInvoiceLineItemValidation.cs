//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDashCommercialInvoiceLineItemValidation
//
//    This class should be used for overriding validation in AutoDashCommercialInvoiceLineItemValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Dash.Business
{
	public class DashCommercialInvoiceLineItemValidation : AutoDashCommercialInvoiceLineItemValidation
	{
		public DashCommercialInvoiceLineItemValidation(AutoDashCommercialInvoiceLineItem parent) : base(parent)
		{
		}
	}
}

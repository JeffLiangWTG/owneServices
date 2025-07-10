//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDashCommercialInvoiceLineItemLookups
//
//    This class should be used for overriding validation in AutoDashCommercialInvoiceLineItemLookups.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Dash.Business
{
	public class DashCommercialInvoiceLineItemLookups : AutoDashCommercialInvoiceLineItemLookups
	{
		public DashCommercialInvoiceLineItemLookups(AutoDashCommercialInvoiceLineItem parent) : base(parent)
		{
		}
	}
}

//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDashCommercialInvoiceLookups
//
//    This class should be used for overriding validation in AutoDashCommercialInvoiceLookups.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Dash.Business
{
	public class DashCommercialInvoiceLookups : AutoDashCommercialInvoiceLookups
	{
		public DashCommercialInvoiceLookups(AutoDashCommercialInvoice parent) : base(parent)
		{
		}
	}
}

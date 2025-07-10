//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDashAPInvoiceChargeRefLineLookups
//
//    This class should be used for overriding validation in AutoDashAPInvoiceChargeRefLineLookups.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Dash.Business
{
	public class DashAPInvoiceChargeLineRefLookups : AutoDashAPInvoiceChargeLineRefLookups
	{
		public DashAPInvoiceChargeLineRefLookups(AutoDashAPInvoiceChargeLineRef parent) : base(parent)
		{
		}
	}
}

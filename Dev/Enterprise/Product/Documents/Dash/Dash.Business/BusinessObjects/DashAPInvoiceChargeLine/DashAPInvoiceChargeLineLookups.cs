//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDashAPInvoiceChargeLineLookups
//
//    This class should be used for overriding validation in AutoDashAPInvoiceChargeLineLookups.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Dash.Business
{
	public class DashAPInvoiceChargeLineLookups : AutoDashAPInvoiceChargeLineLookups
	{
		public DashAPInvoiceChargeLineLookups(AutoDashAPInvoiceChargeLine parent) : base(parent)
		{
		}
	}
}

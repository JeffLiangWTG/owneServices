//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDashAPInvoiceLookups
//
//    This class should be used for overriding validation in AutoDashAPInvoiceLookups.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Dash.Business
{
	public class DashAPInvoiceLookups : AutoDashAPInvoiceLookups
	{
		public DashAPInvoiceLookups(AutoDashAPInvoice parent) : base(parent)
		{
		}
	}
}

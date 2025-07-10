//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDashAPInvoiceRefLookups
//
//    This class should be used for overriding validation in AutoDashAPInvoiceRefLookups.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Dash.Business
{
	public class DashAPInvoiceRefLookups : AutoDashAPInvoiceRefLookups
	{
		public DashAPInvoiceRefLookups(AutoDashAPInvoiceRef parent) : base(parent)
		{
		}
	}
}

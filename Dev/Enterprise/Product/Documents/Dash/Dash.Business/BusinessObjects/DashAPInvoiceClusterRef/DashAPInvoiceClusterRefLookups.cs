//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDashAPInvoiceClusterRefLookups
//
//    This class should be used for overriding validation in AutoDashAPInvoiceClusterRefLookups.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Dash.Business
{
	public class DashAPInvoiceClusterRefLookups : AutoDashAPInvoiceClusterRefLookups
	{
		public DashAPInvoiceClusterRefLookups(AutoDashAPInvoiceClusterRef parent) : base(parent)
		{
		}
	}
}

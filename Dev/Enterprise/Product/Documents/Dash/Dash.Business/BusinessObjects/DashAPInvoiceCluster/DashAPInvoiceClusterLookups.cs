//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDashAPInvoiceClusterLookups
//
//    This class should be used for overriding validation in AutoDashAPInvoiceClusterLookups.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Dash.Business
{
	public class DashAPInvoiceClusterLookups : AutoDashAPInvoiceClusterLookups
	{
		public DashAPInvoiceClusterLookups(AutoDashAPInvoiceCluster parent) : base(parent)
		{
		}
	}
}

//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDashAPInvoiceClusterValidation
//
//    This class should be used for overriding validation in AutoDashAPInvoiceClusterValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Dash.Business
{
	public class DashAPInvoiceClusterValidation : AutoDashAPInvoiceClusterValidation
	{
		public DashAPInvoiceClusterValidation(AutoDashAPInvoiceCluster parent) : base(parent)
		{
		}
	}
}

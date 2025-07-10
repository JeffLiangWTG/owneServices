//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDashDocumentLookups
//
//    This class should be used for overriding validation in AutoDashDocumentLookups.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Dash.Business
{
	public class DashDocumentLookups : AutoDashDocumentLookups
	{
		public DashDocumentLookups(AutoDashDocument parent) : base(parent)
		{
		}
	}
}

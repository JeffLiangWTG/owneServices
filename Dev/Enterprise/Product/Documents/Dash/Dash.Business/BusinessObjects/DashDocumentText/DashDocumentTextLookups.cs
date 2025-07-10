//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDashDocumentTextLookups
//
//    This class should be used for overriding validation in AutoDashDocumentTextLookups.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Dash.Business
{
	public class DashDocumentTextLookups : AutoDashDocumentTextLookups
	{
		public DashDocumentTextLookups(AutoDashDocumentText parent) : base(parent)
		{
		}
	}
}

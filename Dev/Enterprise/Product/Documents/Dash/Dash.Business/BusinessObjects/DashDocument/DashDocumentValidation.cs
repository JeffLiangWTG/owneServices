//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDashDocumentValidation
//
//    This class should be used for overriding validation in AutoDashDocumentValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Dash.Business
{
	public class DashDocumentValidation : AutoDashDocumentValidation
	{
		public DashDocumentValidation(AutoDashDocument parent) : base(parent)
		{
		}
	}
}

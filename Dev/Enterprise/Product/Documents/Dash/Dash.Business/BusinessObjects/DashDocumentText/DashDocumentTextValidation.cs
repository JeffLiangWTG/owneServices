//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDashDocumentTextValidation
//
//    This class should be used for overriding validation in AutoDashDocumentTextValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Dash.Business
{
	public class DashDocumentTextValidation : AutoDashDocumentTextValidation
	{
		public DashDocumentTextValidation(AutoDashDocumentText parent) : base(parent)
		{
		}
	}
}

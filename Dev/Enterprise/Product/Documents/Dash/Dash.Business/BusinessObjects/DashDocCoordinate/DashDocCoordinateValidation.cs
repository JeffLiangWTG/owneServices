//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDashDocCoordinateValidation
//
//    This class should be used for overriding validation in AutoDashDocCoordinateValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Dash.Business
{
	public class DashDocCoordinateValidation : AutoDashDocCoordinateValidation
	{
		public DashDocCoordinateValidation(AutoDashDocCoordinate parent) : base(parent)
		{
		}
	}
}

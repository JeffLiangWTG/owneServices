//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDashDocCoordinateLookups
//
//    This class should be used for overriding validation in AutoDashDocCoordinateLookups.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Dash.Business
{
	public class DashDocCoordinateLookups : AutoDashDocCoordinateLookups
	{
		public DashDocCoordinateLookups(AutoDashDocCoordinate parent) : base(parent)
		{
		}
	}
}

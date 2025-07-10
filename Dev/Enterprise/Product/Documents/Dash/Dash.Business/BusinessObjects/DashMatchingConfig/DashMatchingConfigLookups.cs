//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDashMatchingConfigLookups
//
//    This class should be used for overriding validation in AutoDashMatchingConfigLookups.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Dash.Business
{
	public class DashMatchingConfigLookups : AutoDashMatchingConfigLookups
	{
		public DashMatchingConfigLookups(AutoDashMatchingConfig parent) : base(parent)
		{
		}
	}
}

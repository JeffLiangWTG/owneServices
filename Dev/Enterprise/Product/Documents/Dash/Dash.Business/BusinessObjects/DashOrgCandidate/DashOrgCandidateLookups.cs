//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDashOrgCandidateLookups
//
//    This class should be used for overriding validation in AutoDashOrgCandidateLookups.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Dash.Business
{
	public class DashOrgCandidateLookups : AutoDashOrgCandidateLookups
	{
		public DashOrgCandidateLookups(AutoDashOrgCandidate parent) : base(parent)
		{
		}
	}
}

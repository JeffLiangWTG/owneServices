//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDashOrgCandidateNameLookups
//
//    This class should be used for overriding validation in AutoDashOrgCandidateNameLookups.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Dash.Business
{
	public class DashOrgCandidateNameLookups : AutoDashOrgCandidateNameLookups
	{
		public DashOrgCandidateNameLookups(AutoDashOrgCandidateName parent) : base(parent)
		{
		}
	}
}

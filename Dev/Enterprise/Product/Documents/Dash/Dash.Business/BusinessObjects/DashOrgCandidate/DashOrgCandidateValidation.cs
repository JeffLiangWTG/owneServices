//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDashOrgCandidateValidation
//
//    This class should be used for overriding validation in AutoDashOrgCandidateValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Dash.Business
{
	public class DashOrgCandidateValidation : AutoDashOrgCandidateValidation
	{
		public DashOrgCandidateValidation(AutoDashOrgCandidate parent) : base(parent)
		{
		}
	}
}

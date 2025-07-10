//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDashOrgCandidateNameValidation
//
//    This class should be used for overriding validation in AutoDashOrgCandidateNameValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Dash.Business
{
	public class DashOrgCandidateNameValidation : AutoDashOrgCandidateNameValidation
	{
		public DashOrgCandidateNameValidation(AutoDashOrgCandidateName parent) : base(parent)
		{
		}
	}
}

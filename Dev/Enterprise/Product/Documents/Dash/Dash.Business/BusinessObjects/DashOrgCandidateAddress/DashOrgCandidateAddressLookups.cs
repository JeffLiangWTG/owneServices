//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDashOrgCandidateAddressLookups
//
//    This class should be used for overriding validation in AutoDashOrgCandidateAddressLookups.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Dash.Business
{
	public class DashOrgCandidateAddressLookups : AutoDashOrgCandidateAddressLookups
	{
		public DashOrgCandidateAddressLookups(AutoDashOrgCandidateAddress parent) : base(parent)
		{
		}
	}
}

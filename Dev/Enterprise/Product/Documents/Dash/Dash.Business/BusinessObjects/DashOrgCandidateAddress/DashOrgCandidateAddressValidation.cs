//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDashOrgCandidateAddressValidation
//
//    This class should be used for overriding validation in AutoDashOrgCandidateAddressValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Dash.Business
{
	public class DashOrgCandidateAddressValidation : AutoDashOrgCandidateAddressValidation
	{
		public DashOrgCandidateAddressValidation(AutoDashOrgCandidateAddress parent) : base(parent)
		{
		}
	}
}

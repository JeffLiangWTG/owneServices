//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDashMatchingConfigValidation
//
//    This class should be used for overriding validation in AutoDashMatchingConfigValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Dash.Business
{
	public class DashMatchingConfigValidation : AutoDashMatchingConfigValidation
	{
		public DashMatchingConfigValidation(AutoDashMatchingConfig parent) : base(parent)
		{
		}
	}
}

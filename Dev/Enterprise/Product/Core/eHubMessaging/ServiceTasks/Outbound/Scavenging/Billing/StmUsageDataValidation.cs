//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmUsageDataValidation
//
//    This class should be used for overriding validation in AutoStmUsageDataValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.eHubMessaging.ServiceTasks.Outbound.Scavenging.Billing
{
	public class StmUsageDataValidation : AutoStmUsageDataValidation
	{
		public StmUsageDataValidation(AutoStmUsageData parent) : base(parent)
		{
		}
	}
}

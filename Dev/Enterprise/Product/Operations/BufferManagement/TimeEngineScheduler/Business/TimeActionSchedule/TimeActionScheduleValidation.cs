//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoProcessEstimateLogValidation
//
//    This class should be used for overriding validation in AutoProcessEstimateLogValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.TimeEngineScheduler.Business
{
	public class TimeActionScheduleValidation : AutoTimeActionScheduleValidation
	{
		public TimeActionScheduleValidation(AutoTimeActionSchedule parent) : base(parent)
		{
		}
	}
}

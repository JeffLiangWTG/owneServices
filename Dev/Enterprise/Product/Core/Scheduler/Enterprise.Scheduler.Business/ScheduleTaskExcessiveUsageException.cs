using System;

namespace Enterprise.Scheduler.Business
{
	[Serializable]
	public class ScheduleTaskExcessiveUsageException : Exception
	{
		public ScheduleTaskExcessiveUsageException(string message) : base(message)
		{
		}

#if NETFRAMEWORK
		protected ScheduleTaskExcessiveUsageException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif
	}
}

using CargoWise.EntityFramework;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class CA
		{
			public interface IScheduleB3MessageSupporter : IBaseAutoSendingMessageSupporter
			{
				bool SupportScheduleB3Message { get; }
				string NotSupportScheduleB3MessageReason { get; }
				IProcessor CreateScheduleB3MessageProcessor();
			}
		}
	}
}

using System;

namespace Enterprise.ZArchitecture
{
	[Serializable]
	public class BatchNotification : NotificationSubscriberNotification
	{
		public BatchNotification(ErrorType errorType, string message) : base(errorType, message)
		{
		}

		public BatchNotification(string message)
			: base(NotificationSubscriberType.Info, message)
		{
		}

		public ErrorType ErrorType
		{
			get { return (ErrorType)base.Type; }
		}
	}
}

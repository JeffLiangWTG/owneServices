using System;

namespace Enterprise.ZArchitecture
{
	[Serializable]
	public class ErrorNotification : NotificationSubscriberNotification
	{
		public ErrorNotification(ErrorType errorType) : base(errorType, "")
		{
		}

		public ErrorNotification(ErrorType errorType, string message) : base(errorType, message)
		{
		}

		public ErrorType ErrorType
		{
			get { return (ErrorType)base.Type; }
		}
	}
}

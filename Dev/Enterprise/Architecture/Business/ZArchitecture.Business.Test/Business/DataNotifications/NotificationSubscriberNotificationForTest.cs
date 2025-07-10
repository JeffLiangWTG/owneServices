using System;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[Serializable]
	sealed class NotificationSubscriberNotificationForTest : NotificationSubscriberNotification
	{
		public NotificationSubscriberNotificationForTest(NotificationSubscriberType type, string additionalInfo)
			: base(type, additionalInfo)
		{
		}
	}
}

using System;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[Serializable]
	sealed class NotificationSubscriberTypeForTest : NotificationSubscriberType
	{
		public NotificationSubscriberTypeForTest(string name, string message) : base(name, message)
		{
		}

		public NotificationSubscriberTypeForTest(string message) : this(message, message)
		{
		}
	}
}

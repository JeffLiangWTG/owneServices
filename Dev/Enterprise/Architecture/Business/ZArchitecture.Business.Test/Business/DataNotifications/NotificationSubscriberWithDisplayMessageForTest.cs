using System;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[Serializable]
	sealed class NotificationSubscriberWithDisplayMessageForTest : NotificationSubscriberNotification
	{
		public NotificationSubscriberWithDisplayMessageForTest(NotificationSubscriberType type, string additionalInfo)
			: base(type, additionalInfo)
		{
		}

		protected override string DisplayMessageCore
		{
			get { return "OverriddenDisplayMessage"; }
		}
	}
}

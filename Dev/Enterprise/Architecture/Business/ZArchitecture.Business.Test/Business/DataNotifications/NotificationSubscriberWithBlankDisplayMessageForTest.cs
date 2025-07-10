using System;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[Serializable]
	sealed class NotificationSubscriberWithBlankDisplayMessageForTest : NotificationSubscriberNotification
	{
		public NotificationSubscriberWithBlankDisplayMessageForTest() : base(NotificationSubscriberType.Info, "")
		{
		}

		protected override string DisplayMessageCore
		{
			get { return ""; }
		}

		public bool PublicAllowBlankDisplayMessage;
		protected override bool AllowBlankDisplayMessage
		{
			get { return PublicAllowBlankDisplayMessage; }
		}
	}
}

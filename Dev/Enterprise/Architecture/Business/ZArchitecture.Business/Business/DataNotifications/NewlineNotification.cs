using System;

namespace Enterprise.ZArchitecture
{
	[Serializable]
	public class NewlineNotification : NotificationSubscriberNotification
	{
		public NewlineNotification() : base(NotificationSubscriberType.Info, "")
		{
		}

		protected override string DisplayMessageCore
		{
			get { return ""; }
		}

		protected override string MultiLineDisplayMessageCore
		{
			get { return "\r\n"; }
		}

		protected override bool AllowBlankDisplayMessage
		{
			get { return true; }
		}
	}
}

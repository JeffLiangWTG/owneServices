using System;

namespace Enterprise.ZArchitecture
{
	[Serializable]
	public class WarningNotification : NotificationSubscriberNotification
	{
		public WarningNotification(WarningType type, string additionalInfo) : base(type, additionalInfo)
		{
		}

		public WarningNotification(string additionalInfo) : base(WarningType.Warning, additionalInfo)
		{
		}
	}
}

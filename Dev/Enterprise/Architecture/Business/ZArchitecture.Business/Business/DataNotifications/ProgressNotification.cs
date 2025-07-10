using System;

namespace Enterprise.ZArchitecture
{
	[Serializable]
	public class ProgressNotification : NotificationSubscriberNotification
	{
		public ProgressNotification(int percentageComplete) : base(NotificationSubscriberType.Progress, "")
		{
			this.PercentageComplete = percentageComplete;
		}

		public int PercentageComplete;
	}
}
